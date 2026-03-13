using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;

namespace PersonalTouzi.Api.Controllers;

[ApiController]
[Route("api/portfolio/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AccountsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAccounts()
    {
        var accounts = await _context.Accounts.ToListAsync();
        return accounts.Select(a => MapToDto(a)).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetAccount(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null) return NotFound();
        return MapToDto(account);
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateAccount([FromBody] AccountDto dto)
    {
        var account = new Account
        {
            Name = dto.Name,
            Description = dto.Broker ?? dto.Type ?? "",
            InitialCash = dto.Balance
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, MapToDto(account));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] AccountDto dto)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null) return NotFound();

        account.Name = dto.Name;
        account.Description = dto.Broker ?? dto.Type ?? account.Description;
        account.InitialCash = dto.Balance;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(account));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null) return NotFound();

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static object MapToDto(Account a) => new
    {
        id = a.Id,
        name = a.Name,
        broker = a.Description,
        type = " securities", // default type
        balance = (double)a.InitialCash
    };
}

public record AccountDto(
    string Name,
    string? Broker,
    string? Type,
    decimal Balance
);
