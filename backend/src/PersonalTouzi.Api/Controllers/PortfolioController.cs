using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;

namespace PersonalTouzi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PortfolioController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboard()
    {
        var accounts = await _context.Accounts.ToListAsync();
        var positions = await _context.Positions.ToListAsync();
        var transactions = await _context.Transactions.ToListAsync();
        var snapshots = await _context.AssetSnapshots
            .OrderByDescending(s => s.SnapshotDate)
            .Take(30)
            .ToListAsync();

        var cash = accounts.Sum(a => a.InitialCash);
        var positionsValue = positions.Sum(p => p.MarketValue);
        var totalAssets = cash + positionsValue;
        var totalCost = positions.Sum(p => p.Quantity * p.CostPrice);
        var totalGainLoss = positionsValue - totalCost;
        var totalGainLossPercent = totalCost > 0 ? (double)(totalGainLoss / totalCost) * 100 : 0;
        var todayGainLoss = snapshots.Count >= 2
            ? snapshots[0].TotalAssets - snapshots[1].TotalAssets
            : 0;

        // 净值走势 (最近30天)
        var netValueTrend = snapshots
            .OrderBy(s => s.SnapshotDate)
            .Select(s => new { date = s.SnapshotDate.ToString("yyyy-MM-dd"), value = (double)s.NetValue })
            .ToList();

        // 资产分布 (按类型)
        var assetDistribution = positions
            .GroupBy(p => p.Type)
            .Select(g => new { name = g.Key, value = (double)g.Sum(p => p.MarketValue) })
            .ToList();

        // Top 5 持仓
        var topPositions = positions
            .OrderByDescending(p => p.MarketValue)
            .Take(5)
            .Select(p => new
            {
                id = p.Id,
                symbol = p.Code,
                name = p.Name,
                type = p.Type,
                quantity = p.Quantity,
                avgCost = (double)p.CostPrice,
                costPrice = (double)p.CostPrice,
                currentPrice = (double)p.CurrentPrice,
                marketValue = (double)p.MarketValue,
                gainLoss = (double)p.ProfitLoss,
                gainLossPercent = (double)p.ProfitLossPercent,
                accountId = p.AccountId
            })
            .ToList();

        return new
        {
            totalAssets = (double)totalAssets,
            totalGainLoss = (double)totalGainLoss,
            totalGainLossPercent,
            cash = (double)cash,
            positionsValue = (double)positionsValue,
            todayGainLoss = (double)todayGainLoss,
            netValueTrend,
            assetDistribution,
            topPositions
        };
    }

    [HttpGet("snapshots")]
    public async Task<ActionResult<object>> GetSnapshots([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = _context.AssetSnapshots.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(s => s.SnapshotDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.SnapshotDate <= endDate.Value);

        var snapshots = await query
            .OrderByDescending(s => s.SnapshotDate)
            .Select(s => new
            {
                id = s.Id,
                date = s.SnapshotDate.ToString("yyyy-MM-dd"),
                totalAssets = (double)s.TotalAssets,
                netValue = (double)s.NetValue,
                cash = (double)s.Cash,
                positionsValue = (double)s.PositionsValue,
                gainLoss = (double)s.GainLoss,
                gainLossPercent = (double)s.GainLossPercent
            })
            .ToListAsync();

        return Ok(snapshots);
    }

    [HttpGet("net-value-trend")]
    public async Task<ActionResult<object>> GetNetValueTrend([FromQuery] int days = 30)
    {
        var startDate = DateTime.Today.AddDays(-days);

        var snapshots = await _context.AssetSnapshots
            .Where(s => s.SnapshotDate >= startDate)
            .OrderBy(s => s.SnapshotDate)
            .Select(s => new
            {
                date = s.SnapshotDate.ToString("yyyy-MM-dd"),
                value = (double)s.NetValue
            })
            .ToListAsync();

        return Ok(snapshots);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<object>> GetSummary()
    {
        var accounts = await _context.Accounts.ToListAsync();
        var positions = await _context.Positions.ToListAsync();

        var byType = positions
            .GroupBy(p => p.Type)
            .Select(g => new
            {
                type = g.Key,
                count = g.Count(),
                marketValue = g.Sum(p => p.MarketValue),
                profitLoss = g.Sum(p => p.ProfitLoss)
            })
            .ToList();

        return new
        {
            totalAccounts = accounts.Count,
            totalPositions = positions.Count,
            byType,
            topPositions = positions.OrderByDescending(p => p.MarketValue).Take(5)
        };
    }
}
