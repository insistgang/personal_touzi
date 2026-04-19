using Microsoft.AspNetCore.Mvc;
using PersonalTouzi.Infrastructure.Services;

namespace PersonalTouzi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public PortfolioController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardSummary>> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _portfolioService.GetDashboardAsync(30, cancellationToken);
        return Ok(dashboard);
    }

    [HttpGet("snapshots")]
    public async Task<ActionResult<IReadOnlyList<PortfolioSnapshotSummary>>> GetSnapshots(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var snapshots = await _portfolioService.GetSnapshotsAsync(startDate, endDate, cancellationToken);
        return Ok(snapshots);
    }

    [HttpGet("net-value-trend")]
    public async Task<ActionResult<IReadOnlyList<NetValuePoint>>> GetNetValueTrend(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var trend = await _portfolioService.GetNetValueTrendAsync(days, cancellationToken);
        return Ok(trend);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<object>> GetSummary(CancellationToken cancellationToken)
    {
        var accounts = await _portfolioService.GetAccountsAsync(cancellationToken);
        var dashboard = await _portfolioService.GetDashboardAsync(30, cancellationToken);

        var byType = dashboard.AssetDistribution.Select(distribution => new
        {
            type = distribution.Name,
            count = dashboard.TopPositions.Count(position => position.Type == distribution.Name),
            marketValue = distribution.Value
        });

        return Ok(new
        {
            totalAccounts = accounts.Count,
            totalPositions = accounts.Sum(account => account.PositionCount),
            byType,
            topPositions = dashboard.TopPositions
        });
    }
}
