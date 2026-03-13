using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;

namespace PersonalTouzi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PositionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Position>>> GetPositions()
    {
        return await _context.Positions.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Position>> GetPosition(int id)
    {
        var position = await _context.Positions.FindAsync(id);
        if (position == null) return NotFound();
        return position;
    }

    [HttpGet("by-account/{accountId}")]
    public async Task<ActionResult<IEnumerable<Position>>> GetPositionsByAccount(int accountId)
    {
        return await _context.Positions
            .Where(p => p.AccountId == accountId)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Position>> CreatePosition(Position position)
    {
        _context.Positions.Add(position);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPosition), new { id = position.Id }, position);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePosition(int id, Position position)
    {
        if (id != position.Id) return BadRequest();

        _context.Entry(position).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Positions.Any(p => p.Id == id))
                return NotFound();
            throw;
        }

        return NoContent();
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
}
