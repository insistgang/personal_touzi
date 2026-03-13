using Microsoft.AspNetCore.Mvc;
using PersonalTouzi.Infrastructure.Services;

namespace PersonalTouzi.Api.Controllers;

/// <summary>
/// AI 服务控制器
/// </summary>
[ApiController]
[Route("api/ai")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<AIController> _logger;

    public AIController(IAIService aiService, ILogger<AIController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// 智能对话
    /// </summary>
    /// <param name="request">对话请求</param>
    /// <returns>AI 回复</returns>
    [HttpPost("chat")]
    public async Task<ActionResult<string>> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var response = await _aiService.ChatAsync(request.Message, request.Context);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "对话处理失败");
            return StatusCode(500, new { error = "对话处理失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 分析投资组合
    /// </summary>
    /// <param name="accountId">账户 ID</param>
    /// <returns>投资组合分析结果</returns>
    [HttpPost("analyze/{accountId}")]
    public async Task<ActionResult<PortfolioAnalysis>> AnalyzePortfolio(int accountId)
    {
        try
        {
            var analysis = await _aiService.AnalyzePortfolioAsync(accountId);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "投资组合分析失败: AccountId={AccountId}", accountId);
            return StatusCode(500, new { error = "投资组合分析失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 生成投资报告
    /// </summary>
    /// <param name="accountId">账户 ID</param>
    /// <param name="request">报告生成请求</param>
    /// <returns>报告内容</returns>
    [HttpPost("report/{accountId}")]
    public async Task<ActionResult<string>> GenerateReport(
        int accountId,
        [FromBody] ReportRequest? request = null)
    {
        try
        {
            var reportType = request?.ReportType ?? "weekly";
            var report = await _aiService.GenerateReportAsync(accountId, reportType);
            return Ok(new { reportType, content = report });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "报告生成失败: AccountId={AccountId}", accountId);
            return StatusCode(500, new { error = "报告生成失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 市场情绪分析
    /// </summary>
    /// <param name="request">情绪分析请求</param>
    /// <returns>市场情绪分析结果</returns>
    [HttpPost("sentiment")]
    public async Task<ActionResult<MarketSentiment>> AnalyzeMarketSentiment(
        [FromBody] SentimentRequest request)
    {
        try
        {
            var sentiment = await _aiService.AnalyzeMarketSentimentAsync(request.StockCodes);
            return Ok(sentiment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "市场情绪分析失败");
            return StatusCode(500, new { error = "市场情绪分析失败", message = ex.Message });
        }
    }

    /// <summary>
    /// 收益预测
    /// </summary>
    /// <param name="accountId">账户 ID</param>
    /// <param name="request">预测请求</param>
    /// <returns>收益预测结果</returns>
    [HttpPost("predict/{accountId}")]
    public async Task<ActionResult<RevenuePrediction>> PredictRevenue(
        int accountId,
        [FromBody] PredictionRequest? request = null)
    {
        try
        {
            var days = request?.Days ?? 30;
            var prediction = await _aiService.PredictRevenueAsync(accountId, days);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "收益预测失败: AccountId={AccountId}", accountId);
            return StatusCode(500, new { error = "收益预测失败", message = ex.Message });
        }
    }
}

#region Request DTOs

/// <summary>
/// 对话请求
/// </summary>
public record ChatRequest(
    string Message,
    string? Context = null
);

/// <summary>
/// 报告生成请求
/// </summary>
public record ReportRequest(
    string ReportType = "weekly"
);

/// <summary>
/// 市场情绪分析请求
/// </summary>
public record SentimentRequest(
    string[] StockCodes
);

/// <summary>
/// 收益预测请求
/// </summary>
public record PredictionRequest(
    int Days = 30
);

#endregion
