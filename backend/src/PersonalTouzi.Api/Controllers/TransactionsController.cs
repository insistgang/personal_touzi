using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Api.Models;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;
using PersonalTouzi.Infrastructure.Services;

namespace PersonalTouzi.Api.Controllers;

[ApiController]
[Route("api/portfolio/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPortfolioService _portfolioService;
    private readonly ITransactionSettlementService _transactionSettlementService;

    public TransactionsController(
        ApplicationDbContext context,
        IPortfolioService portfolioService,
        ITransactionSettlementService transactionSettlementService)
    {
        _context = context;
        _portfolioService = portfolioService;
        _transactionSettlementService = transactionSettlementService;
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
    public async Task<ActionResult<object>> CreateTransaction(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var transaction = await _transactionSettlementService.RecordTransactionAsync(
                new RecordTransactionCommand(
                    AccountId: request.AccountId,
                    Symbol: request.Symbol,
                    Name: request.Name,
                    Type: request.Type,
                    Quantity: request.Quantity,
                    Price: request.Price,
                    TradeDate: request.TradeDate,
                    Remark: request.Remark,
                    AssetType: request.AssetType),
                cancellationToken);

            await _portfolioService.RefreshTodaySnapshotsAsync(cancellationToken);
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, MapToDto(transaction));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = "指定的账户不存在", message = ex.Message });
        }
        catch (PortfolioRuleException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public IActionResult UpdateTransaction(
        int id,
        [FromBody] UpdateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        _ = id;
        _ = request;
        _ = cancellationToken;

        return BadRequest(new
        {
            error = "交易记账后不支持直接修改。请新增一笔反向交易或手动调整持仓作为修正。"
        });
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteTransaction(int id, CancellationToken cancellationToken)
    {
        _ = id;
        _ = cancellationToken;

        return BadRequest(new
        {
            error = "交易记账后不支持直接删除。请新增一笔反向交易来冲销原始记录。"
        });
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
        accountId = t.AccountId,
        remark = t.Remark
    };
}
