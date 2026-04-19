namespace PersonalTouzi.Infrastructure.Services;

public interface IPortfolioService
{
    Task<IReadOnlyList<AccountSummary>> GetAccountsAsync(CancellationToken cancellationToken = default);
    Task<AccountSummary?> GetAccountAsync(int id, CancellationToken cancellationToken = default);
    Task<DashboardSummary> GetDashboardAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PortfolioSnapshotSummary>> GetSnapshotsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NetValuePoint>> GetNetValueTrendAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<AccountAiContext?> GetAccountAiContextAsync(int accountId, CancellationToken cancellationToken = default);
    Task RefreshTodaySnapshotsAsync(CancellationToken cancellationToken = default);
    Task SeedHistoricalSnapshotsIfEmptyAsync(CancellationToken cancellationToken = default);
}
