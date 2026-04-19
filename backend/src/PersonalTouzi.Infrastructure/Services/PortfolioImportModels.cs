namespace PersonalTouzi.Infrastructure.Services;

public record ImportInitialPositionsCommand(
    int AccountId,
    string CsvContent,
    bool HasHeader = true);

public record ImportTransactionsCommand(
    int AccountId,
    string CsvContent,
    bool HasHeader = true);

public record InitialPositionImportResult(
    int AccountId,
    int ImportedCount,
    decimal TotalCostBasis,
    decimal TotalMarketValue);

public record TransactionImportResult(
    int AccountId,
    int ImportedCount,
    int BuyCount,
    int SellCount,
    decimal TotalAmount);
