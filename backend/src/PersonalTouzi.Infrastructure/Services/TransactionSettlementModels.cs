namespace PersonalTouzi.Infrastructure.Services;

public sealed record RecordTransactionCommand(
    int AccountId,
    string Symbol,
    string? Name,
    string Type,
    decimal Quantity,
    decimal Price,
    DateTime TradeDate,
    string? Remark,
    string? AssetType
);

public sealed class PortfolioRuleException : Exception
{
    public PortfolioRuleException(string message) : base(message)
    {
    }
}
