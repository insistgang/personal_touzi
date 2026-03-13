using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;

namespace PersonalTouzi.Api.Controllers;

[ApiController]
[Route("api/portfolio/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TransactionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetTransactions()
    {
        var transactions = await _context.Transactions
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
        return transactions.Select(t => MapToDto(t)).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetTransaction(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null) return NotFound();
        return MapToDto(transaction);
    }

    [HttpGet("by-account/{accountId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetTransactionsByAccount(int accountId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
        return transactions.Select(t => MapToDto(t)).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateTransaction([FromBody] TransactionDto dto)
    {
        var transaction = new Transaction
        {
            Code = dto.Symbol,
            Name = dto.Name ?? "",
            Type = dto.Type,
            Quantity = dto.Quantity,
            Price = dto.Price,
            TransactionDate = DateTime.Parse(dto.TradeDate),
            AccountId = dto.AccountId
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, MapToDto(transaction));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, [FromBody] TransactionDto dto)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null) return NotFound();

        transaction.Code = dto.Symbol;
        transaction.Name = dto.Name ?? "";
        transaction.Type = dto.Type;
        transaction.Quantity = dto.Quantity;
        transaction.Price = dto.Price;
        transaction.TransactionDate = DateTime.Parse(dto.TradeDate);
        transaction.AccountId = dto.AccountId;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(transaction));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null) return NotFound();

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static object MapToDto(Transaction t) => new
    {
        id = t.Id,
        symbol = t.Code,
        name = t.Name,
        type = t.Type,
        quantity = t.Quantity,
        price = (double)t.Price,
        amount = (double)t.Amount,
        tradeDate = t.TransactionDate.ToString("yyyy-MM-dd"),
        accountId = t.AccountId
    };
}

public record TransactionDto(
    string Symbol,
    string? Name,
    string Type,
    decimal Quantity,
    decimal Price,
    string TradeDate,
    int AccountId
);
