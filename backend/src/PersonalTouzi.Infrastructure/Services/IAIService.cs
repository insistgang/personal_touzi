namespace PersonalTouzi.Infrastructure.Services;

public interface IAIService
{
    Task<string> ChatAsync(string message, string? context = null);
    Task<PortfolioAnalysis> AnalyzePortfolioAsync(int accountId);
    Task<InvestmentReport> GenerateReportAsync(int accountId, string reportType = "weekly");
    Task<MarketSentiment> AnalyzeMarketSentimentAsync(string[] stockCodes);
    Task<RevenuePrediction> PredictRevenueAsync(int accountId, int days = 30);
}

public record PortfolioAnalysis(
    double RiskLevel,
    string RiskAssessment,
    List<string> Suggestions,
    string Summary
);

public record SentimentFactor(
    string Name,
    int Impact,
    string Description
);

public record MarketSentiment(
    string Overall,
    double Score,
    double Bullish,
    double Neutral,
    double Bearish,
    List<SentimentFactor> Factors,
    List<string> Advices,
    string Recommendation
);

public record RevenuePrediction(
    List<string> Dates,
    List<double> PredictedValues,
    List<double> LowerBounds,
    List<double> UpperBounds,
    double ExpectedReturn,
    double LowerBound,
    double UpperBound,
    double ConfidenceLevel,
    List<string> Warnings,
    string Analysis
);

public record InvestmentReport(
    string GenerateTime,
    string Period,
    double TotalReturn,
    double TotalAmount,
    double BenchmarkReturn,
    double ExcessReturn,
    double PositionValue,
    int PositionCount,
    double Concentration,
    int TradeCount,
    double TradeAmount,
    double WinRate,
    double ProfitLossRatio,
    List<string> Risks,
    List<string> Suggestions,
    string Content
);
