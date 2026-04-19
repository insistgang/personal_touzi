using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;

namespace PersonalTouzi.Infrastructure.Services;

public class TransactionSettlementService : ITransactionSettlementService
{
    private readonly ApplicationDbContext _context;

    public TransactionSettlementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction> RecordTransactionAsync(
        RecordTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizeTradeType(command.Type);
        var normalizedSymbol = NormalizeSymbol(command.Symbol);

        var account = await _context.Accounts
            .FirstOrDefaultAsync(item => item.Id == command.AccountId, cancellationToken);

        if (account is null)
        {
            throw new KeyNotFoundException($"找不到账户 {command.AccountId}。");
        }

        var position = await _context.Positions
            .FirstOrDefaultAsync(
                item => item.AccountId == command.AccountId && item.Code == normalizedSymbol,
                cancellationToken);

        var amount = command.Quantity * command.Price;
        var assetName = ResolveAssetName(command.Name, position);

        if (normalizedType == "buy")
        {
            ApplyBuy(account, position, command, normalizedSymbol, assetName, amount);
        }
        else
        {
            ApplySell(account, position, command, assetName, amount);
        }

        var transaction = new Transaction
        {
            AccountId = account.Id,
            Code = normalizedSymbol,
            Name = assetName,
            Type = normalizedType,
            Quantity = command.Quantity,
            Price = command.Price,
            TransactionDate = command.TradeDate,
            Remark = command.Remark
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return transaction;
    }

    private void ApplyBuy(
        Account account,
        Position? existingPosition,
        RecordTransactionCommand command,
        string symbol,
        string assetName,
        decimal amount)
    {
        if (account.InitialCash < amount)
        {
            throw new PortfolioRuleException("账户可用现金不足，无法完成本次买入。");
        }

        account.InitialCash -= amount;

        if (existingPosition is null)
        {
            _context.Positions.Add(new Position
            {
                AccountId = account.Id,
                Code = symbol,
                Name = assetName,
                Type = NormalizeAssetType(command.AssetType),
                Quantity = command.Quantity,
                CostPrice = command.Price,
                CurrentPrice = command.Price,
                UpdatedAt = DateTime.Now
            });

            return;
        }

        var previousCost = existingPosition.Quantity * existingPosition.CostPrice;
        var newQuantity = existingPosition.Quantity + command.Quantity;
        var newCost = previousCost + amount;

        existingPosition.Quantity = newQuantity;
        existingPosition.CostPrice = newQuantity > 0 ? newCost / newQuantity : command.Price;
        existingPosition.CurrentPrice = command.Price;
        existingPosition.Name = assetName;
        existingPosition.UpdatedAt = DateTime.Now;
    }

    private void ApplySell(
        Account account,
        Position? existingPosition,
        RecordTransactionCommand command,
        string assetName,
        decimal amount)
    {
        if (existingPosition is null)
        {
            throw new PortfolioRuleException("当前账户没有该持仓，无法卖出。");
        }

        if (existingPosition.Quantity < command.Quantity)
        {
            throw new PortfolioRuleException("卖出数量超过当前持仓数量。");
        }

        account.InitialCash += amount;

        var remainingQuantity = existingPosition.Quantity - command.Quantity;
        if (remainingQuantity == 0)
        {
            _context.Positions.Remove(existingPosition);
            return;
        }

        existingPosition.Quantity = remainingQuantity;
        existingPosition.CurrentPrice = command.Price;
        existingPosition.Name = assetName;
        existingPosition.UpdatedAt = DateTime.Now;
    }

    private static string NormalizeTradeType(string type)
    {
        var normalized = type.Trim().ToLowerInvariant();
        if (normalized is not ("buy" or "sell"))
        {
            throw new PortfolioRuleException("交易类型必须是 buy 或 sell。");
        }

        return normalized;
    }

    private static string NormalizeSymbol(string symbol)
    {
        var normalized = symbol.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new PortfolioRuleException("证券代码不能为空。");
        }

        return normalized;
    }

    private static string ResolveAssetName(string? requestedName, Position? existingPosition)
    {
        var normalized = requestedName?.Trim();
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            return normalized;
        }

        if (!string.IsNullOrWhiteSpace(existingPosition?.Name))
        {
            return existingPosition.Name;
        }

        throw new PortfolioRuleException("首次录入该标的时必须填写证券名称。");
    }

    private static string NormalizeAssetType(string? assetType)
    {
        var normalized = assetType?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "fund" => "fund",
            "bond" => "bond",
            _ => "stock"
        };
    }
}
