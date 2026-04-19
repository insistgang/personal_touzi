using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Api.Models;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;
using PersonalTouzi.Infrastructure.Services;

namespace PersonalTouzi.Api.Controllers;

[ApiController]
[Route("api/portfolio/[controller]")]
public class PositionsController : ControllerBase
{
    private const string ManualPositionCreateError =
        "持仓数量和成本已由交易记录驱动，当前不支持直接新增持仓。请通过交易记录入账；初始持仓导入将作为独立流程提供。";

    private const string ManualPositionDeleteError =
        "当前不支持直接删除持仓。请通过新增卖出交易或反向交易，将仓位调整到目标数量。";

    private const string PositionReadOnlyFieldsError =
        "当前只允许更新持仓名称、类型和现价。数量、成本、代码和所属账户需通过交易记录或后续导入流程维护。";

    private readonly ApplicationDbContext _context;
    private readonly IPortfolioService _portfolioService;

    public PositionsController(ApplicationDbContext context, IPortfolioService portfolioService)
    {
        _context = context;
        _portfolioService = portfolioService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetPositions()
    {
        var positions = await _context.Positions.ToListAsync();
        return positions.Select(p => MapToDto(p)).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetPosition(int id)
    {
        var position = await _context.Positions.FindAsync(id);
        if (position == null) return NotFound();
        return MapToDto(position);
    }

    [HttpGet("by-account/{accountId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetPositionsByAccount(int accountId)
    {
        var positions = await _context.Positions
            .Where(p => p.AccountId == accountId)
            .ToListAsync();
        return positions.Select(p => MapToDto(p)).ToList();
    }

    [HttpPost]
    public ActionResult<object> CreatePosition([FromBody] CreatePositionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return BadRequest(new { error = ManualPositionCreateError });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePosition(
        int id,
        [FromBody] UpdatePositionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var position = await _context.Positions.FindAsync([id], cancellationToken);
        if (position == null) return NotFound();

        if (request.Symbol is not null
            || request.Quantity.HasValue
            || request.CostPrice.HasValue
            || request.AccountId.HasValue)
        {
            return BadRequest(new { error = PositionReadOnlyFieldsError });
        }

        if (request.Name != null) position.Name = request.Name;
        if (request.Type != null) position.Type = request.Type;
        if (request.CurrentPrice.HasValue) position.CurrentPrice = request.CurrentPrice.Value;
        position.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);
        await _portfolioService.RefreshTodaySnapshotsAsync(cancellationToken);
        return Ok(MapToDto(position));
    }

    [HttpDelete("{id}")]
    public IActionResult DeletePosition(int id)
    {
        return BadRequest(new { error = ManualPositionDeleteError });
    }

    private static object MapToDto(Position p) => new
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
    };
}
