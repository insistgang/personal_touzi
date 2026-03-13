namespace PersonalTouzi.Infrastructure.Services;

/// <summary>
/// AI 服务接口
/// </summary>
public interface IAIService
{
    /// <summary>
    /// 智能对话
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="context">上下文信息（可选）</param>
    /// <returns>AI 回复</returns>
    Task<string> ChatAsync(string message, string? context = null);

    /// <summary>
    /// 分析投资组合
    /// </summary>
    /// <param name="accountId">账户 ID</param>
    /// <returns>投资组合分析结果</returns>
    Task<PortfolioAnalysis> AnalyzePortfolioAsync(int accountId);

    /// <summary>
    /// 生成投资报告
    /// </summary>
    /// <param name="accountId">账户 ID</param>
    /// <param name="reportType">报告类型（weekly/monthly/yearly）</param>
    /// <returns>报告内容</returns>
    Task<string> GenerateReportAsync(int accountId, string reportType = "weekly");

    /// <summary>
    /// 市场情绪分析
    /// </summary>
    /// <param name="stockCodes">股票代码列表</param>
    /// <returns>市场情绪分析结果</returns>
    Task<MarketSentiment> AnalyzeMarketSentimentAsync(string[] stockCodes);

    /// <summary>
    /// 收益预测
    /// </summary>
    /// <param name="accountId">账户 ID</param>
    /// <param name="days">预测天数</param>
    /// <returns>收益预测结果</returns>
    Task<RevenuePrediction> PredictRevenueAsync(int accountId, int days = 30);
}

/// <summary>
/// 投资组合分析结果
/// </summary>
public record PortfolioAnalysis(
    double RiskLevel,
    string RiskAssessment,
    List<string> Suggestions,
    string Summary
);

/// <summary>
/// 市场情绪分析结果
/// </summary>
public record MarketSentiment(
    string OverallSentiment,
    double ConfidenceScore,
    List<string> KeyFactors,
    string Recommendation
);

/// <summary>
/// 收益预测结果
/// </summary>
public record RevenuePrediction(
    List<PredictionPoint> Predictions,
    double ExpectedReturn,
    double ConfidenceLevel,
    string Analysis
);

/// <summary>
/// 预测数据点
/// </summary>
public record PredictionPoint(
    DateTime Date,
    decimal PredictedValue,
    double Confidence
);
