namespace PersonalTouzi.Infrastructure.Services;

public sealed record AccountSummary(
    int Id,
    string Name,
    string Broker,
    string Type,
    decimal Balance,
    decimal PositionsValue,
    decimal TotalAssets,
    decimal GainLoss,
    double GainLossPercent,
    int PositionCount,
    DateTime CreatedAt
);

public sealed record DashboardSummary(
    decimal TotalAssets,
    decimal TotalGainLoss,
    double TotalGainLossPercent,
    decimal Cash,
    decimal PositionsValue,
    decimal TodayGainLoss,
    IReadOnlyList<NetValuePoint> NetValueTrend,
    IReadOnlyList<DistributionPoint> AssetDistribution,
    IReadOnlyList<PositionSummary> TopPositions
);

public sealed record PortfolioSnapshotSummary(
    DateTime Date,
    decimal TotalAssets,
    decimal NetValue,
    decimal Cash,
    decimal PositionsValue,
    decimal GainLoss,
    double GainLossPercent
);

public sealed record NetValuePoint(
    string Date,
    decimal Value
);

public sealed record DistributionPoint(
    string Name,
    decimal Value
);

public sealed record PositionSummary(
    int Id,
    string Symbol,
    string Name,
    string Type,
    decimal Quantity,
    decimal AvgCost,
    decimal CostPrice,
    decimal CurrentPrice,
    decimal MarketValue,
    decimal GainLoss,
    double GainLossPercent,
    int AccountId
);

public sealed record TransactionSummary(
    int Id,
    string Symbol,
    string Name,
    string Type,
    decimal Quantity,
    decimal Price,
    decimal Amount,
    string TradeDate,
    int AccountId,
    string? Remark
);

public sealed record AccountAiContext(
    AccountSummary Account,
    IReadOnlyList<PositionSummary> Positions,
    IReadOnlyList<TransactionSummary> Transactions
);
