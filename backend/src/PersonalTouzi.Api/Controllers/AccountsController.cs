using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Api.Models;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;
using PersonalTouzi.Infrastructure.Services;

namespace PersonalTouzi.Api.Controllers;

[ApiController]
[Route("api/portfolio/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPortfolioService _portfolioService;

    public AccountsController(ApplicationDbContext context, IPortfolioService portfolioService)
    {
        _context = context;
        _portfolioService = portfolioService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountSummary>>> GetAccounts(CancellationToken cancellationToken)
    {
        var accounts = await _portfolioService.GetAccountsAsync(cancellationToken);
        return Ok(accounts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountSummary>> GetAccount(int id, CancellationToken cancellationToken)
    {
        var account = await _portfolioService.GetAccountAsync(id, cancellationToken);
        if (account is null) return NotFound();
        return Ok(account);
    }

    [HttpPost]
    public async Task<ActionResult<AccountSummary>> CreateAccount(
        [FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var account = new Account
        {
            Name = request.Name,
            Description = request.Description ?? "",
            InitialCash = request.InitialCash
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        await _portfolioService.RefreshTodaySnapshotsAsync(cancellationToken);

        var summary = await _portfolioService.GetAccountAsync(account.Id, cancellationToken);
        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, summary);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(
        int id,
        [FromBody] UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var account = await _context.Accounts.FindAsync([id], cancellationToken);
        if (account == null) return NotFound();

        account.Name = request.Name;
        account.Description = request.Description ?? account.Description;
        account.InitialCash = request.InitialCash;

        await _context.SaveChangesAsync(cancellationToken);
        await _portfolioService.RefreshTodaySnapshotsAsync(cancellationToken);

        var summary = await _portfolioService.GetAccountAsync(id, cancellationToken);
        return Ok(summary);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts.FindAsync([id], cancellationToken);
        if (account == null) return NotFound();

        var hasPositions = await _context.Positions.AnyAsync(position => position.AccountId == id, cancellationToken);
        if (hasPositions)
        {
            return BadRequest(new { error = "该账户下仍有持仓，请先清空持仓后再删除账户。" });
        }

        var hasTransactions = await _context.Transactions.AnyAsync(transaction => transaction.AccountId == id, cancellationToken);
        if (hasTransactions)
        {
            return BadRequest(new { error = "该账户下仍有交易记录，请先清理交易记录后再删除账户。" });
        }

        var snapshots = await _context.AssetSnapshots
            .Where(snapshot => snapshot.AccountId == id)
            .ToListAsync(cancellationToken);
        if (snapshots.Count > 0)
        {
            _context.AssetSnapshots.RemoveRange(snapshots);
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync(cancellationToken);
        await _portfolioService.RefreshTodaySnapshotsAsync(cancellationToken);

        return NoContent();
    }
}

