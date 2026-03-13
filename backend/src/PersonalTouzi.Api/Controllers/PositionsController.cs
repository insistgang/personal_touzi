using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;

namespace PersonalTouzi.Api.Controllers;

[ApiController]
[Route("api/portfolio/[controller]")]
public class PositionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PositionsController(ApplicationDbContext context)
    {
        _context = context;
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
    public async Task<ActionResult<object>> CreatePosition([FromBody] PositionDto dto)
    {
        var position = new Position
        {
            Code = dto.Symbol,
            Name = dto.Name,
            Type = dto.Type ?? "stock",
            Quantity = dto.Quantity,
            CostPrice = dto.CostPrice,
            CurrentPrice = dto.CurrentPrice,
            AccountId = dto.AccountId
        };

        _context.Positions.Add(position);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPosition), new { id = position.Id }, MapToDto(position));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePosition(int id, [FromBody] PositionDto dto)
    {
        var position = await _context.Positions.FindAsync(id);
        if (position == null) return NotFound();

        position.Code = dto.Symbol;
        position.Name = dto.Name;
        position.Type = dto.Type ?? position.Type;
        position.Quantity = dto.Quantity;
        position.CostPrice = dto.CostPrice;
        position.CurrentPrice = dto.CurrentPrice;
        position.AccountId = dto.AccountId;
        position.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(position));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePosition(int id)
    {
        var position = await _context.Positions.FindAsync(id);
        if (position == null) return NotFound();

        _context.Positions.Remove(position);
        await _context.SaveChangesAsync();

        return NoContent();
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

public record PositionDto(
    string Symbol,
    string Name,
    string? Type,
    decimal Quantity,
    decimal CostPrice,
    decimal CurrentPrice,
    int AccountId
);
