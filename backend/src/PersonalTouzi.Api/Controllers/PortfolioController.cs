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

        var totalCash = accounts.Sum(a => a.InitialCash);
        var totalMarketValue = positions.Sum(p => p.MarketValue);
        var totalAssetValue = totalCash + totalMarketValue;
        var totalCost = positions.Sum(p => p.Quantity * p.CostPrice);
        var totalProfitLoss = totalMarketValue - totalCost;
        var profitLossPercent = totalCost > 0 ? (totalProfitLoss / totalCost) * 100 : 0;

        return new
        {
            totalCash,
            totalMarketValue,
            totalAssetValue,
            totalCost,
            totalProfitLoss,
            profitLossPercent,
            accountCount = accounts.Count,
            positionCount = positions.Count,
            recentTransactions = transactions.Take(5)
        };
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
