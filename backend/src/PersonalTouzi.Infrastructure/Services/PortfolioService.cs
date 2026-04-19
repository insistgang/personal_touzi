using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;

namespace PersonalTouzi.Infrastructure.Services;

public class PortfolioService : IPortfolioService
{
    private const int DefaultSeedDays = 30;

    private readonly ApplicationDbContext _context;

    public PortfolioService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AccountSummary>> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _context.Accounts
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .ToListAsync(cancellationToken);

        var metrics = await BuildAccountMetricsLookupAsync(cancellationToken);

        return accounts
            .Select(account => MapAccountSummary(account, metrics))
            .ToList();
    }

    public async Task<AccountSummary?> GetAccountAsync(int id, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (account is null)
        {
            return null;
        }

        var metrics = await BuildAccountMetricsLookupAsync(cancellationToken);
        return MapAccountSummary(account, metrics);
    }

    public async Task<DashboardSummary> GetDashboardAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var accounts = await GetAccountsAsync(cancellationToken);
        var positions = await GetPositionSummariesAsync(null, cancellationToken);
        var snapshots = await GetSnapshotsAsync(DateTime.Today.AddDays(-(Math.Max(days, 2) - 1)), DateTime.Today, cancellationToken);

        var cash = accounts.Sum(account => account.Balance);
        var positionsValue = positions.Sum(position => position.MarketValue);
        var totalAssets = cash + positionsValue;
        var totalGainLoss = positions.Sum(position => position.GainLoss);
        var totalCost = positions.Sum(position => position.Quantity * position.CostPrice);
        var totalGainLossPercent = totalCost > 0
            ? decimal.ToDouble(totalGainLoss / totalCost) * 100
            : 0;

        var orderedSnapshots = snapshots.OrderBy(snapshot => snapshot.Date).ToList();
        var todayGainLoss = orderedSnapshots.Count >= 2
            ? orderedSnapshots[^1].TotalAssets - orderedSnapshots[^2].TotalAssets
            : 0;

        var assetDistribution = positions
            .GroupBy(position => position.Type)
            .Select(group => new DistributionPoint(group.Key, group.Sum(position => position.MarketValue)))
            .OrderByDescending(point => point.Value)
            .ToList();

        var topPositions = positions
            .OrderByDescending(position => position.MarketValue)
            .Take(5)
            .ToList();

        return new DashboardSummary(
            TotalAssets: totalAssets,
            TotalGainLoss: totalGainLoss,
            TotalGainLossPercent: totalGainLossPercent,
            Cash: cash,
            PositionsValue: positionsValue,
            TodayGainLoss: todayGainLoss,
            NetValueTrend: orderedSnapshots
                .Select(snapshot => new NetValuePoint(snapshot.Date.ToString("yyyy-MM-dd"), snapshot.NetValue))
                .ToList(),
            AssetDistribution: assetDistribution,
            TopPositions: topPositions
        );
    }

    public async Task<IReadOnlyList<PortfolioSnapshotSummary>> GetSnapshotsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AssetSnapshots.AsNoTracking().AsQueryable();

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(snapshot => snapshot.SnapshotDate >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date;
            query = query.Where(snapshot => snapshot.SnapshotDate <= end);
        }

        var rawSnapshots = await query
            .OrderBy(snapshot => snapshot.SnapshotDate)
            .ToListAsync(cancellationToken);

        if (rawSnapshots.Count == 0)
        {
            return Array.Empty<PortfolioSnapshotSummary>();
        }

        var grouped = rawSnapshots
            .GroupBy(snapshot => snapshot.SnapshotDate.Date)
            .OrderBy(group => group.Key)
            .Select(group => new
            {
                Date = group.Key,
                TotalAssets = group.Sum(snapshot => snapshot.TotalAssets),
                Cash = group.Sum(snapshot => snapshot.Cash),
                PositionsValue = group.Sum(snapshot => snapshot.PositionsValue),
                GainLoss = group.Sum(snapshot => snapshot.GainLoss)
            })
            .ToList();

        var baseline = grouped.First().TotalAssets <= 0 ? 1 : grouped.First().TotalAssets;

        return grouped
            .Select(group => new PortfolioSnapshotSummary(
                Date: group.Date,
                TotalAssets: group.TotalAssets,
                NetValue: baseline > 0 ? group.TotalAssets / baseline : 1,
                Cash: group.Cash,
                PositionsValue: group.PositionsValue,
                GainLoss: group.GainLoss,
                GainLossPercent: group.TotalAssets > 0
                    ? decimal.ToDouble(group.GainLoss / group.TotalAssets) * 100
                    : 0
            ))
            .OrderByDescending(snapshot => snapshot.Date)
            .ToList();
    }

    public async Task<IReadOnlyList<NetValuePoint>> GetNetValueTrendAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var snapshots = await GetSnapshotsAsync(DateTime.Today.AddDays(-(Math.Max(days, 1) - 1)), DateTime.Today, cancellationToken);

        return snapshots
            .OrderBy(snapshot => snapshot.Date)
            .Select(snapshot => new NetValuePoint(snapshot.Date.ToString("yyyy-MM-dd"), snapshot.NetValue))
            .ToList();
    }

    public async Task<AccountAiContext?> GetAccountAiContextAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var account = await GetAccountAsync(accountId, cancellationToken);
        if (account is null)
        {
            return null;
        }

        var positions = await GetPositionSummariesAsync(accountId, cancellationToken);
        var transactions = await GetTransactionSummariesAsync(accountId, cancellationToken);

        return new AccountAiContext(account, positions, transactions);
    }

    public async Task RefreshTodaySnapshotsAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _context.Accounts
            .OrderBy(account => account.Id)
            .ToListAsync(cancellationToken);

        var metrics = await BuildAccountMetricsLookupAsync(cancellationToken);
        var today = DateTime.Today;

        foreach (var account in accounts)
        {
            metrics.TryGetValue(account.Id, out var metric);
            var existing = await _context.AssetSnapshots
                .FirstOrDefaultAsync(
                    snapshot => snapshot.AccountId == account.Id && snapshot.SnapshotDate == today,
                    cancellationToken);

            var totalAssets = account.InitialCash + metric.PositionsValue;
            var gainLossPercent = metric.TotalCost > 0
                ? metric.GainLoss / metric.TotalCost * 100
                : 0;
            var netValueBase = account.InitialCash + metric.TotalCost;
            var netValue = netValueBase > 0 ? totalAssets / netValueBase : 1;

            if (existing is null)
            {
                _context.AssetSnapshots.Add(new AssetSnapshot
                {
                    AccountId = account.Id,
                    SnapshotDate = today,
                    TotalAssets = totalAssets,
                    Cash = account.InitialCash,
                    PositionsValue = metric.PositionsValue,
                    GainLoss = metric.GainLoss,
                    GainLossPercent = gainLossPercent,
                    NetValue = netValue
                });

                continue;
            }

            existing.TotalAssets = totalAssets;
            existing.Cash = account.InitialCash;
            existing.PositionsValue = metric.PositionsValue;
            existing.GainLoss = metric.GainLoss;
            existing.GainLossPercent = gainLossPercent;
            existing.NetValue = netValue;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SeedHistoricalSnapshotsIfEmptyAsync(CancellationToken cancellationToken = default)
    {
        var hasSnapshots = await _context.AssetSnapshots.AnyAsync(cancellationToken);
        if (hasSnapshots)
        {
            return;
        }

        var accounts = await _context.Accounts
            .OrderBy(account => account.Id)
            .ToListAsync(cancellationToken);

        if (accounts.Count == 0)
        {
            return;
        }

        var metrics = await BuildAccountMetricsLookupAsync(cancellationToken);
        var startDate = DateTime.Today.AddDays(-(DefaultSeedDays - 1));

        foreach (var account in accounts)
        {
            metrics.TryGetValue(account.Id, out var metric);

            var currentTotalAssets = account.InitialCash + metric.PositionsValue;
            var baselineAssets = currentTotalAssets <= 0 ? 1 : currentTotalAssets * 0.94m;
            var totalCost = metric.TotalCost <= 0 ? currentTotalAssets : metric.TotalCost;

            for (var offset = 0; offset < DefaultSeedDays - 1; offset++)
            {
                var date = startDate.AddDays(offset);
                var progress = (decimal)(offset + 1) / DefaultSeedDays;
                var seasonality = (decimal)Math.Sin((offset + account.Id) * 0.45d) * 0.012m;
                var totalFactor = 0.94m + (progress * 0.07m) + seasonality;
                var positionsFactor = 0.92m + (progress * 0.09m) + seasonality;

                var totalAssets = Math.Max(0, currentTotalAssets * totalFactor);
                var positionsValue = Math.Max(0, metric.PositionsValue * positionsFactor);
                var cash = Math.Max(0, totalAssets - positionsValue);
                var gainLoss = positionsValue - totalCost;
                var gainLossPercent = totalCost > 0
                    ? gainLoss / totalCost * 100
                    : 0;
                var netValue = baselineAssets > 0 ? totalAssets / baselineAssets : 1;

                _context.AssetSnapshots.Add(new AssetSnapshot
                {
                    AccountId = account.Id,
                    SnapshotDate = date,
                    TotalAssets = totalAssets,
                    Cash = cash,
                    PositionsValue = positionsValue,
                    GainLoss = gainLoss,
                    GainLossPercent = gainLossPercent,
                    NetValue = netValue
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        await RefreshTodaySnapshotsAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<PositionSummary>> GetPositionSummariesAsync(
        int? accountId,
        CancellationToken cancellationToken)
    {
        var query = _context.Positions.AsNoTracking().AsQueryable();
        if (accountId.HasValue)
        {
            query = query.Where(position => position.AccountId == accountId.Value);
        }

        var positions = await query.ToListAsync(cancellationToken);

        return positions
            .OrderByDescending(position => position.MarketValue)
            .Select(MapPositionSummary)
            .ToList();
    }

    private async Task<IReadOnlyList<TransactionSummary>> GetTransactionSummariesAsync(
        int accountId,
        CancellationToken cancellationToken)
    {
        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.AccountId == accountId)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .ToListAsync(cancellationToken);

        return transactions
            .Select(transaction => new TransactionSummary(
                Id: transaction.Id,
                Symbol: transaction.Code,
                Name: transaction.Name,
                Type: transaction.Type,
                Quantity: transaction.Quantity,
                Price: transaction.Price,
                Amount: transaction.Amount,
                TradeDate: transaction.TransactionDate.ToString("yyyy-MM-dd"),
                AccountId: transaction.AccountId,
                Remark: transaction.Remark
            ))
            .ToList();
    }

    private async Task<Dictionary<int, AccountMetric>> BuildAccountMetricsLookupAsync(CancellationToken cancellationToken)
    {
        var positions = await _context.Positions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return positions
            .GroupBy(position => position.AccountId)
            .ToDictionary(
                group => group.Key,
                group => new AccountMetric(
                    PositionsValue: group.Sum(position => position.MarketValue),
                    GainLoss: group.Sum(position => position.ProfitLoss),
                    TotalCost: group.Sum(position => position.Quantity * position.CostPrice),
                    PositionCount: group.Count()));
    }

    private static AccountSummary MapAccountSummary(Account account, IReadOnlyDictionary<int, AccountMetric> metrics)
    {
        metrics.TryGetValue(account.Id, out var metric);

        var gainLossPercent = metric.TotalCost > 0
            ? decimal.ToDouble(metric.GainLoss / metric.TotalCost) * 100
            : 0;

        return new AccountSummary(
            Id: account.Id,
            Name: account.Name,
            Broker: account.Description,
            Type: "securities",
            Balance: account.InitialCash,
            PositionsValue: metric.PositionsValue,
            TotalAssets: account.InitialCash + metric.PositionsValue,
            GainLoss: metric.GainLoss,
            GainLossPercent: gainLossPercent,
            PositionCount: metric.PositionCount,
            CreatedAt: account.CreatedAt
        );
    }

    private static PositionSummary MapPositionSummary(Position position)
    {
        return new PositionSummary(
            Id: position.Id,
            Symbol: position.Code,
            Name: position.Name,
            Type: position.Type,
            Quantity: position.Quantity,
            AvgCost: position.CostPrice,
            CostPrice: position.CostPrice,
            CurrentPrice: position.CurrentPrice,
            MarketValue: position.MarketValue,
            GainLoss: position.ProfitLoss,
            GainLossPercent: decimal.ToDouble(position.ProfitLossPercent),
            AccountId: position.AccountId
        );
    }

    private readonly record struct AccountMetric(
        decimal PositionsValue,
        decimal GainLoss,
        decimal TotalCost,
        int PositionCount
    );
}
