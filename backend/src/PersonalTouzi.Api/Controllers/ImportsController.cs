using Microsoft.AspNetCore.Mvc;
using PersonalTouzi.Api.Models;
using PersonalTouzi.Infrastructure.Services;

namespace PersonalTouzi.Api.Controllers;

[ApiController]
[Route("api/portfolio/[controller]")]
public class ImportsController : ControllerBase
{
    private readonly IPortfolioImportService _portfolioImportService;
    private readonly IPortfolioService _portfolioService;

    public ImportsController(
        IPortfolioImportService portfolioImportService,
        IPortfolioService portfolioService)
    {
        _portfolioImportService = portfolioImportService;
        _portfolioService = portfolioService;
    }

    [HttpPost("initial-positions")]
    public async Task<ActionResult<InitialPositionImportResult>> ImportInitialPositions(
        [FromBody] ImportInitialPositionsRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _portfolioImportService.ImportInitialPositionsAsync(
                new ImportInitialPositionsCommand(
                    AccountId: request.AccountId,
                    CsvContent: request.CsvContent,
                    HasHeader: request.HasHeader),
                cancellationToken);

            await _portfolioService.RefreshTodaySnapshotsAsync(cancellationToken);
            return Ok(result);
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

    [HttpPost("transactions")]
    public async Task<ActionResult<TransactionImportResult>> ImportTransactions(
        [FromBody] ImportTransactionsRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _portfolioImportService.ImportTransactionsAsync(
                new ImportTransactionsCommand(
                    AccountId: request.AccountId,
                    CsvContent: request.CsvContent,
                    HasHeader: request.HasHeader),
                cancellationToken);

            await _portfolioService.RefreshTodaySnapshotsAsync(cancellationToken);
            return Ok(result);
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
}
