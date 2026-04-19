using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;

namespace PersonalTouzi.Infrastructure.Services;

public class PortfolioImportService : IPortfolioImportService
{
    private readonly ApplicationDbContext _context;
    private readonly ITransactionSettlementService _transactionSettlementService;

    public PortfolioImportService(
        ApplicationDbContext context,
        ITransactionSettlementService transactionSettlementService)
    {
        _context = context;
        _transactionSettlementService = transactionSettlementService;
    }

    public async Task<InitialPositionImportResult> ImportInitialPositionsAsync(
        ImportInitialPositionsCommand command,
        CancellationToken cancellationToken = default)
    {
        EnsureCsvContent(command.CsvContent);

        var accountExists = await _context.Accounts
            .AnyAsync(item => item.Id == command.AccountId, cancellationToken);

        if (!accountExists)
        {
            throw new KeyNotFoundException($"找不到账户 {command.AccountId}。");
        }

        var hasExistingPositions = await _context.Positions
            .AnyAsync(item => item.AccountId == command.AccountId, cancellationToken);
        var hasExistingTransactions = await _context.Transactions
            .AnyAsync(item => item.AccountId == command.AccountId, cancellationToken);

        if (hasExistingPositions || hasExistingTransactions)
        {
            throw new PortfolioRuleException("初始持仓导入仅支持空账户。请新建账户后导入，或使用交易导入来维护已有账户。");
        }

        var rows = ParseInitialPositionRows(command.CsvContent, command.HasHeader);
        var duplicateSymbols = rows
            .GroupBy(item => item.Symbol)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(item => item)
            .ToArray();

        if (duplicateSymbols.Length > 0)
        {
            throw new PortfolioRuleException($"导入文件中存在重复证券代码：{string.Join("、", duplicateSymbols)}。");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var row in rows)
            {
                _context.Positions.Add(new Position
                {
                    AccountId = command.AccountId,
                    Code = row.Symbol,
                    Name = row.Name,
                    Type = row.Type,
                    Quantity = row.Quantity,
                    CostPrice = row.CostPrice,
                    CurrentPrice = row.CurrentPrice,
                    UpdatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new InitialPositionImportResult(
                AccountId: command.AccountId,
                ImportedCount: rows.Count,
                TotalCostBasis: rows.Sum(item => item.Quantity * item.CostPrice),
                TotalMarketValue: rows.Sum(item => item.Quantity * item.CurrentPrice));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<TransactionImportResult> ImportTransactionsAsync(
        ImportTransactionsCommand command,
        CancellationToken cancellationToken = default)
    {
        EnsureCsvContent(command.CsvContent);

        var accountExists = await _context.Accounts
            .AnyAsync(item => item.Id == command.AccountId, cancellationToken);

        if (!accountExists)
        {
            throw new KeyNotFoundException($"找不到账户 {command.AccountId}。");
        }

        var rows = ParseTransactionRows(command.CsvContent, command.HasHeader);
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var row in rows)
            {
                await _transactionSettlementService.RecordTransactionAsync(
                    new RecordTransactionCommand(
                        AccountId: command.AccountId,
                        Symbol: row.Symbol,
                        Name: row.Name,
                        Type: row.Type,
                        Quantity: row.Quantity,
                        Price: row.Price,
                        TradeDate: row.TradeDate,
                        Remark: row.Remark,
                        AssetType: row.AssetType),
                    cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            return new TransactionImportResult(
                AccountId: command.AccountId,
                ImportedCount: rows.Count,
                BuyCount: rows.Count(item => item.Type == "buy"),
                SellCount: rows.Count(item => item.Type == "sell"),
                TotalAmount: rows.Sum(item => item.Quantity * item.Price));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private static void EnsureCsvContent(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
        {
            throw new PortfolioRuleException("CSV 内容不能为空。");
        }
    }

    private static IReadOnlyList<InitialPositionImportRow> ParseInitialPositionRows(string csvContent, bool hasHeader)
    {
        var rows = ParseCsvRows(csvContent, hasHeader);
        var result = new List<InitialPositionImportRow>(rows.Count);

        foreach (var row in rows)
        {
            if (row.Fields.Count != 6)
            {
                throw new PortfolioRuleException(
                    $"第 {row.LineNumber} 行列数不正确，应为 6 列：symbol,name,type,quantity,costPrice,currentPrice。");
            }

            result.Add(new InitialPositionImportRow(
                Symbol: NormalizeSymbol(row.Fields[0], row.LineNumber),
                Name: ParseRequiredText(row.Fields[1], row.LineNumber, "证券名称"),
                Type: NormalizeAssetType(row.Fields[2], row.LineNumber),
                Quantity: ParsePositiveDecimal(row.Fields[3], row.LineNumber, "持仓数量"),
                CostPrice: ParseNonNegativeDecimal(row.Fields[4], row.LineNumber, "成本价"),
                CurrentPrice: ParseNonNegativeDecimal(row.Fields[5], row.LineNumber, "现价")));
        }

        return result;
    }

    private static IReadOnlyList<TransactionImportRow> ParseTransactionRows(string csvContent, bool hasHeader)
    {
        var rows = ParseCsvRows(csvContent, hasHeader);
        var result = new List<TransactionImportRow>(rows.Count);

        foreach (var row in rows)
        {
            if (row.Fields.Count != 8)
            {
                throw new PortfolioRuleException(
                    $"第 {row.LineNumber} 行列数不正确，应为 8 列：tradeDate,symbol,name,assetType,type,quantity,price,remark。");
            }

            result.Add(new TransactionImportRow(
                TradeDate: ParseDate(row.Fields[0], row.LineNumber),
                Symbol: NormalizeSymbol(row.Fields[1], row.LineNumber),
                Name: ParseRequiredText(row.Fields[2], row.LineNumber, "证券名称"),
                AssetType: NormalizeAssetType(row.Fields[3], row.LineNumber),
                Type: NormalizeTradeType(row.Fields[4], row.LineNumber),
                Quantity: ParsePositiveDecimal(row.Fields[5], row.LineNumber, "交易数量"),
                Price: ParsePositiveDecimal(row.Fields[6], row.LineNumber, "交易价格"),
                Remark: ParseOptionalText(row.Fields[7])));
        }

        return result;
    }

    private static List<CsvRow> ParseCsvRows(string csvContent, bool hasHeader)
    {
        var normalized = csvContent
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var lines = normalized.Split('\n');
        var rows = new List<CsvRow>();
        var headerSkipped = !hasHeader;

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!headerSkipped)
            {
                headerSkipped = true;
                continue;
            }

            rows.Add(new CsvRow(index + 1, SplitCsvLine(line, index + 1)));
        }

        if (rows.Count == 0)
        {
            throw new PortfolioRuleException("CSV 内容中没有可导入的数据行。");
        }

        return rows;
    }

    private static List<string> SplitCsvLine(string line, int lineNumber)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var ch = line[index];
            if (ch == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    current.Append('"');
                    index++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                fields.Add(NormalizeField(current.ToString()));
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        if (inQuotes)
        {
            throw new PortfolioRuleException($"第 {lineNumber} 行 CSV 引号不完整。");
        }

        fields.Add(NormalizeField(current.ToString()));
        return fields;
    }

    private static string NormalizeField(string value)
    {
        return value.Trim().Trim('\uFEFF');
    }

    private static string NormalizeSymbol(string value, int lineNumber)
    {
        var normalized = value.Trim().Trim('\uFEFF').ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new PortfolioRuleException($"第 {lineNumber} 行证券代码不能为空。");
        }

        return normalized;
    }

    private static string NormalizeAssetType(string value, int lineNumber)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "stock" => "stock",
            "fund" => "fund",
            "bond" => "bond",
            _ => throw new PortfolioRuleException($"第 {lineNumber} 行资产类型必须是 stock、fund 或 bond。")
        };
    }

    private static string NormalizeTradeType(string value, int lineNumber)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "buy" => "buy",
            "sell" => "sell",
            _ => throw new PortfolioRuleException($"第 {lineNumber} 行交易类型必须是 buy 或 sell。")
        };
    }

    private static string ParseRequiredText(string value, int lineNumber, string fieldName)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new PortfolioRuleException($"第 {lineNumber} 行{fieldName}不能为空。");
        }

        return normalized;
    }

    private static string? ParseOptionalText(string value)
    {
        var normalized = value.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static decimal ParsePositiveDecimal(string value, int lineNumber, string fieldName)
    {
        var parsed = ParseDecimal(value, lineNumber, fieldName);
        if (parsed <= 0)
        {
            throw new PortfolioRuleException($"第 {lineNumber} 行{fieldName}必须大于 0。");
        }

        return parsed;
    }

    private static decimal ParseNonNegativeDecimal(string value, int lineNumber, string fieldName)
    {
        var parsed = ParseDecimal(value, lineNumber, fieldName);
        if (parsed < 0)
        {
            throw new PortfolioRuleException($"第 {lineNumber} 行{fieldName}不能为负数。");
        }

        return parsed;
    }

    private static decimal ParseDecimal(string value, int lineNumber, string fieldName)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out parsed))
        {
            return parsed;
        }

        throw new PortfolioRuleException($"第 {lineNumber} 行{fieldName}格式不正确。");
    }

    private static DateTime ParseDate(string value, int lineNumber)
    {
        if (DateTime.TryParseExact(
                value,
                ["yyyy-MM-dd", "yyyy/MM/dd", "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss"],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            return parsed;
        }

        if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
        {
            return parsed;
        }

        throw new PortfolioRuleException($"第 {lineNumber} 行交易日期格式不正确。");
    }

    private sealed record CsvRow(int LineNumber, List<string> Fields);

    private sealed record InitialPositionImportRow(
        string Symbol,
        string Name,
        string Type,
        decimal Quantity,
        decimal CostPrice,
        decimal CurrentPrice);

    private sealed record TransactionImportRow(
        DateTime TradeDate,
        string Symbol,
        string Name,
        string AssetType,
        string Type,
        decimal Quantity,
        decimal Price,
        string? Remark);
}
