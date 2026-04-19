namespace PersonalTouzi.Infrastructure.Services;

public interface IPortfolioImportService
{
    Task<InitialPositionImportResult> ImportInitialPositionsAsync(
        ImportInitialPositionsCommand command,
        CancellationToken cancellationToken = default);

    Task<TransactionImportResult> ImportTransactionsAsync(
        ImportTransactionsCommand command,
        CancellationToken cancellationToken = default);
}
