using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PersonalTouzi.Infrastructure.Services;

public class GlmAIOptions
{
    public const string SectionName = "AI";
    public string Provider { get; set; } = "GLM";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "glm-4-flash";
    public int MaxTokens { get; set; } = 4096;
    public double Temperature { get; set; } = 0.7;
}

public class GlmAIService : IAIService
{
    private const string BaseUrl = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

    private readonly HttpClient _httpClient;
    private readonly GlmAIOptions _options;
    private readonly IPortfolioService _portfolioService;
    private readonly ILogger<GlmAIService> _logger;

    public GlmAIService(
        HttpClient httpClient,
        IOptions<GlmAIOptions> options,
        IPortfolioService portfolioService,
        ILogger<GlmAIService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _portfolioService = portfolioService;
        _logger = logger;
    }

    public async Task<string> ChatAsync(string message, string? context = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "请输入您想分析的问题，我可以帮您查看持仓、收益、风险和调仓思路。";
        }

        if (HasApiKey())
        {
            var remoteReply = await CallGlmAsync(
                "你是一个专业的量化投资助手，帮助用户用简洁、清晰的中文回答投资组合问题。",
                string.IsNullOrWhiteSpace(context) ? message : $"{message}\n\n上下文：{context}");

            if (!string.IsNullOrWhiteSpace(remoteReply))
            {
                return remoteReply;
            }
        }

        return await BuildLocalChatResponseAsync(message, context);
    }

    public async Task<PortfolioAnalysis> AnalyzePortfolioAsync(int accountId)
    {
        var accountContext = await GetRequiredAccountContextAsync(accountId);
        return BuildPortfolioAnalysis(accountContext);
    }

    public async Task<InvestmentReport> GenerateReportAsync(int accountId, string reportType = "weekly")
    {
        var accountContext = await GetRequiredAccountContextAsync(accountId);
        return BuildInvestmentReport(accountContext, NormalizeReportType(reportType));
    }

    public async Task<MarketSentiment> AnalyzeMarketSentimentAsync(string[] stockCodes)
    {
        var dashboard = await _portfolioService.GetDashboardAsync();
        return BuildMarketSentiment(dashboard, stockCodes);
    }

    public async Task<RevenuePrediction> PredictRevenueAsync(int accountId, int days = 30)
    {
        var accountContext = await GetRequiredAccountContextAsync(accountId);
        return BuildRevenuePrediction(accountContext, Math.Clamp(days, 7, 180));
    }

    private async Task<AccountAiContext> GetRequiredAccountContextAsync(int accountId)
    {
        var accountContext = await _portfolioService.GetAccountAiContextAsync(accountId);
        if (accountContext is null)
        {
            throw new KeyNotFoundException($"找不到账户 {accountId}。");
        }

        return accountContext;
    }

    private static PortfolioAnalysis BuildPortfolioAnalysis(AccountAiContext context)
    {
        var positionsValue = context.Account.PositionsValue;
        var concentration = positionsValue > 0
            ? decimal.ToDouble((context.Positions.MaxBy(position => position.MarketValue)?.MarketValue ?? 0) / positionsValue)
            : 0;
        var cashRatio = context.Account.TotalAssets > 0
            ? decimal.ToDouble(context.Account.Balance / context.Account.TotalAssets)
            : 1;
        var losingRatio = context.Positions.Count == 0
            ? 0
            : (double)context.Positions.Count(position => position.GainLoss < 0) / context.Positions.Count;
        var typeCount = context.Positions.Select(position => position.Type).Distinct(StringComparer.OrdinalIgnoreCase).Count();

        var riskLevel = Clamp01(
            0.18 +
            (concentration * 0.4) +
            ((1 - Math.Min(typeCount, 3) / 3d) * 0.18) +
            (losingRatio * 0.16) +
            ((cashRatio < 0.15 ? 1 - (cashRatio / 0.15) : 0) * 0.12));

        var riskAssessment = riskLevel switch
        {
            >= 0.72 => "高风险，仓位集中且缓冲资金偏少",
            >= 0.45 => "中等风险，建议继续优化仓位结构",
            _ => "风险可控，整体组合较为稳健"
        };

        var suggestions = new List<string>();

        if (concentration >= 0.45)
        {
            suggestions.Add("单一标的占比偏高，建议逐步分散到 3-5 个核心仓位。");
        }

        if (cashRatio < 0.15)
        {
            suggestions.Add("现金缓冲不足，建议预留至少 10%-20% 的机动仓位。");
        }

        if (losingRatio > 0.5)
        {
            suggestions.Add("当前亏损持仓较多，建议复盘每个仓位的建仓逻辑与止损条件。");
        }

        if (typeCount < 2 && context.Positions.Count > 0)
        {
            suggestions.Add("资产类型较单一，可增加 ETF 或低波动资产来平滑净值。");
        }

        if (suggestions.Count == 0)
        {
            suggestions.Add("继续保持当前仓位结构，重点关注估值与仓位纪律。");
            suggestions.Add("定期更新现价与账户现金，保证仪表盘统计结果持续可靠。");
        }

        var summary =
            $"账户 {context.Account.Name} 当前总资产约为 {FormatCurrency(context.Account.TotalAssets)}，" +
            $"持仓 {context.Account.PositionCount} 只，账面盈亏 {FormatSignedCurrency(context.Account.GainLoss)}。" +
            $"组合风险评估为 {riskAssessment}。";

        return new PortfolioAnalysis(
            RiskLevel: Math.Round(riskLevel, 2),
            RiskAssessment: riskAssessment,
            Suggestions: suggestions,
            Summary: summary
        );
    }

    private static MarketSentiment BuildMarketSentiment(DashboardSummary dashboard, IReadOnlyCollection<string> stockCodes)
    {
        var scoreBase = 50d + (dashboard.TotalGainLossPercent * 1.2d);
        var score = Math.Clamp(scoreBase, 20d, 85d);

        string overall;
        double bullish;
        double neutral;
        double bearish;

        if (score >= 63)
        {
            overall = "bullish";
            bullish = 0.62;
            neutral = 0.24;
            bearish = 0.14;
        }
        else if (score <= 42)
        {
            overall = "bearish";
            bullish = 0.18;
            neutral = 0.26;
            bearish = 0.56;
        }
        else
        {
            overall = "neutral";
            bullish = 0.34;
            neutral = 0.41;
            bearish = 0.25;
        }

        var focusText = stockCodes.Count > 0
            ? $"跟踪标的包含 {string.Join("、", stockCodes.Take(3))}{(stockCodes.Count > 3 ? " 等" : string.Empty)}。"
            : "当前分析基于组合整体表现与核心仓位结构。";

        var factors = new List<SentimentFactor>
        {
            new(
                Name: "组合收益状态",
                Impact: dashboard.TotalGainLoss >= 0 ? 2 : -2,
                Description: dashboard.TotalGainLoss >= 0
                    ? "当前账面收益为正，风险偏好相对改善。"
                    : "当前组合仍处于回撤区间，需控制情绪化加仓。"),
            new(
                Name: "仓位集中度",
                Impact: dashboard.TopPositions.Count > 0 && dashboard.PositionsValue > 0 &&
                        decimal.ToDouble(dashboard.TopPositions[0].MarketValue / dashboard.PositionsValue) > 0.45
                    ? -1
                    : 1,
                Description: dashboard.TopPositions.Count > 0 && dashboard.PositionsValue > 0 &&
                             decimal.ToDouble(dashboard.TopPositions[0].MarketValue / dashboard.PositionsValue) > 0.45
                    ? "龙头仓位偏重，市场波动时净值弹性会被放大。"
                    : "仓位分散度尚可，有利于抵御局部波动。"),
            new(
                Name: "现金缓冲",
                Impact: dashboard.TotalAssets > 0 && decimal.ToDouble(dashboard.Cash / dashboard.TotalAssets) >= 0.2 ? 1 : -1,
                Description: dashboard.TotalAssets > 0 && decimal.ToDouble(dashboard.Cash / dashboard.TotalAssets) >= 0.2
                    ? "现金充足，面对回撤时具备再平衡空间。"
                    : "现金仓位偏低，短期应更重视风险控制。"),
            new(
                Name: "关注范围",
                Impact: 0,
                Description: focusText)
        };

        var advices = new List<string>();

        if (overall == "bullish")
        {
            advices.Add("优先增配确定性更高、波动更可控的核心标的。");
            advices.Add("分批执行加仓，不建议一次性打满仓位。");
            advices.Add("对涨幅过快的标的保留止盈计划，避免利润回吐。");
        }
        else if (overall == "bearish")
        {
            advices.Add("先控制回撤，避免在弱势阶段追高补仓。");
            advices.Add("优先检查高波动和高集中度仓位，必要时降低暴露。");
            advices.Add("把更多精力放在现金管理和风险边界上。");
        }
        else
        {
            advices.Add("维持现有节奏，等待更清晰的方向信号。");
            advices.Add("通过定投、分批调仓而不是重仓博弈来提升确定性。");
            advices.Add("持续关注组合中的弱势仓位，必要时做结构优化。");
        }

        return new MarketSentiment(
            Overall: overall,
            Score: Math.Round(score, 1),
            Bullish: bullish,
            Neutral: neutral,
            Bearish: bearish,
            Factors: factors,
            Advices: advices,
            Recommendation: advices[0]
        );
    }

    private static RevenuePrediction BuildRevenuePrediction(AccountAiContext context, int days)
    {
        var baseAssets = decimal.ToDouble(context.Account.TotalAssets);
        if (baseAssets <= 0)
        {
            baseAssets = Math.Max(1d, decimal.ToDouble(context.Account.Balance));
        }

        var concentration = context.Account.PositionsValue > 0
            ? decimal.ToDouble((context.Positions.MaxBy(position => position.MarketValue)?.MarketValue ?? 0) / context.Account.PositionsValue)
            : 0;
        var gainLossRatio = Math.Clamp(context.Account.GainLossPercent / 100d, -0.25d, 0.25d);
        var expectedDailyReturn = Math.Clamp((gainLossRatio / 90d) + 0.00035d, -0.0025d, 0.0025d);
        var bandFactor = 0.035d + (concentration * 0.09d);

        var dates = new List<string>(days);
        var predictedValues = new List<double>(days);
        var lowerBounds = new List<double>(days);
        var upperBounds = new List<double>(days);

        for (var index = 1; index <= days; index++)
        {
            var progress = index / (double)days;
            var seasonal = Math.Sin(index / 6d) * 0.003d;
            var predicted = baseAssets * (1 + (expectedDailyReturn * index) + seasonal);
            var band = baseAssets * bandFactor * Math.Sqrt(progress) * 0.32d;

            dates.Add(DateTime.Today.AddDays(index).ToString("yyyy-MM-dd"));
            predictedValues.Add(Math.Round(predicted, 2));
            lowerBounds.Add(Math.Round(Math.Max(0, predicted - band), 2));
            upperBounds.Add(Math.Round(predicted + band, 2));
        }

        var finalPredicted = predictedValues[^1];
        var finalLower = lowerBounds[^1];
        var finalUpper = upperBounds[^1];

        var warnings = new List<string>
        {
            concentration >= 0.45
                ? "组合集中度偏高，预测区间会明显放大。"
                : "当前组合分散度尚可，预测波动相对可控。",
            context.Account.Balance <= 0
                ? "账户现金较少，后续回撤承受能力偏弱。"
                : "保留一定现金有助于应对波动和再平衡。",
            "预测模型主要依据当前仓位结构和账面收益状态，不代表未来真实收益。"
        };

        return new RevenuePrediction(
            Dates: dates,
            PredictedValues: predictedValues,
            LowerBounds: lowerBounds,
            UpperBounds: upperBounds,
            ExpectedReturn: Math.Round((finalPredicted / baseAssets) - 1, 4),
            LowerBound: Math.Round((finalLower / baseAssets) - 1, 4),
            UpperBound: Math.Round((finalUpper / baseAssets) - 1, 4),
            ConfidenceLevel: Math.Round(Math.Clamp(0.86d - (bandFactor * 2.1d), 0.45d, 0.84d), 2),
            Warnings: warnings,
            Analysis: $"基于当前资产规模 {FormatCurrency((decimal)baseAssets)} 和仓位结构，未来 {days} 天预计呈现温和波动走势。"
        );
    }

    private static InvestmentReport BuildInvestmentReport(AccountAiContext context, string reportType)
    {
        var periodDays = reportType switch
        {
            "monthly" => 30,
            "quarterly" => 90,
            "yearly" => 365,
            _ => 7
        };

        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-(periodDays - 1));
        var transactions = context.Transactions
            .Where(transaction => DateOnly.ParseExact(transaction.TradeDate, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                >= DateOnly.FromDateTime(startDate))
            .ToList();

        var concentration = context.Account.PositionsValue > 0
            ? decimal.ToDouble((context.Positions.MaxBy(position => position.MarketValue)?.MarketValue ?? 0) / context.Account.PositionsValue)
            : 0;

        var benchmarkReturn = reportType switch
        {
            "monthly" => 0.031,
            "quarterly" => 0.078,
            "yearly" => 0.145,
            _ => 0.012
        };

        var totalReturn = context.Account.PositionsValue > 0
            ? context.Account.GainLossPercent / 100d
            : 0d;
        var totalAmount = decimal.ToDouble(context.Account.GainLoss);
        var excessReturn = totalReturn - benchmarkReturn;
        var tradeAmount = decimal.ToDouble(transactions.Sum(transaction => transaction.Amount));
        var profitablePositions = context.Positions.Where(position => position.GainLoss > 0).ToList();
        var losingPositions = context.Positions.Where(position => position.GainLoss < 0).ToList();
        var winRate = context.Account.PositionCount > 0
            ? (double)profitablePositions.Count / context.Account.PositionCount
            : 0.5;

        var averageProfit = profitablePositions.Count > 0
            ? profitablePositions.Average(position => decimal.ToDouble(position.GainLoss))
            : 0;
        var averageLoss = losingPositions.Count > 0
            ? Math.Abs(losingPositions.Average(position => decimal.ToDouble(position.GainLoss)))
            : 0;
        var profitLossRatio = averageLoss > 0
            ? Math.Round(averageProfit / averageLoss, 2)
            : Math.Round(Math.Max(1.2, averageProfit > 0 ? averageProfit / 1000d : 1), 2);

        var risks = new List<string>();
        if (concentration > 0.45)
        {
            risks.Add("前两大仓位占比较高，组合净值对单一标的波动较敏感。");
        }

        if (context.Account.Balance / Math.Max(context.Account.TotalAssets, 1) < 0.12m)
        {
            risks.Add("现金缓冲偏低，遇到回撤时再平衡空间有限。");
        }

        if (losingPositions.Count > profitablePositions.Count)
        {
            risks.Add("当前亏损持仓数量偏多，建议检查弱势仓位是否仍满足持有逻辑。");
        }

        if (transactions.Count > Math.Max(6, periodDays / 3))
        {
            risks.Add("报告期内交易较频繁，需警惕过度交易侵蚀收益。");
        }

        if (risks.Count == 0)
        {
            risks.Add("目前未发现显著结构性风险，但仍需关注仓位纪律与市场波动。");
        }

        var suggestions = new List<string>();
        if (concentration > 0.45)
        {
            suggestions.Add("将单一高权重仓位分批降至更合理水平，降低组合波动。");
        }

        suggestions.Add(context.Account.GainLoss >= 0
            ? "继续围绕盈利仓位做强弱分化，保留趋势更强的核心资产。"
            : "优先复盘亏损仓位，明确继续持有的基本面依据。");
        suggestions.Add("为每笔新增交易设置触发条件和退出规则，提升执行一致性。");

        var content =
            $"本期 {GetReportLabel(reportType)}显示，账户 {context.Account.Name} 当前总资产为 {FormatCurrency(context.Account.TotalAssets)}，" +
            $"持仓市值 {FormatCurrency(context.Account.PositionsValue)}，账面盈亏 {FormatSignedCurrency(context.Account.GainLoss)}。" +
            $"报告期内共发生 {transactions.Count} 笔交易，建议继续关注仓位集中度与现金管理。";

        return new InvestmentReport(
            GenerateTime: DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            Period: $"{startDate:yyyy-MM-dd} 至 {endDate:yyyy-MM-dd}",
            TotalReturn: Math.Round(totalReturn, 4),
            TotalAmount: Math.Round(totalAmount, 2),
            BenchmarkReturn: Math.Round(benchmarkReturn, 4),
            ExcessReturn: Math.Round(excessReturn, 4),
            PositionValue: Math.Round(decimal.ToDouble(context.Account.PositionsValue), 2),
            PositionCount: context.Account.PositionCount,
            Concentration: Math.Round(concentration, 4),
            TradeCount: transactions.Count,
            TradeAmount: Math.Round(tradeAmount, 2),
            WinRate: Math.Round(winRate, 4),
            ProfitLossRatio: profitLossRatio,
            Risks: risks,
            Suggestions: suggestions,
            Content: content
        );
    }

    private async Task<string> BuildLocalChatResponseAsync(string message, string? context)
    {
        var dashboard = await _portfolioService.GetDashboardAsync();
        var normalized = message.Trim().ToLowerInvariant();

        string reply;

        if (normalized.Contains("持仓") || normalized.Contains("仓位"))
        {
            var largest = dashboard.TopPositions.FirstOrDefault();
            reply =
                $"当前组合共有 {dashboard.TopPositions.Count} 个重点持仓，持仓总市值约 {FormatCurrency(dashboard.PositionsValue)}。" +
                (largest is null
                    ? string.Empty
                    : $" 当前第一大仓位是 {largest.Name}（{largest.Symbol}），市值约 {FormatCurrency(largest.MarketValue)}。");
        }
        else if (normalized.Contains("收益") || normalized.Contains("盈亏"))
        {
            reply =
                $"当前总资产约 {FormatCurrency(dashboard.TotalAssets)}，组合账面盈亏 {FormatSignedCurrency(dashboard.TotalGainLoss)}，" +
                $"收益率 {dashboard.TotalGainLossPercent:F2}% 。";
        }
        else if (normalized.Contains("风险"))
        {
            var largestWeight = dashboard.PositionsValue > 0 && dashboard.TopPositions.Count > 0
                ? decimal.ToDouble(dashboard.TopPositions[0].MarketValue / dashboard.PositionsValue)
                : 0;
            reply =
                $"目前最大的风险点是仓位集中度和现金缓冲。第一大仓位约占持仓市值的 {largestWeight * 100:F1}% ，" +
                $"现金占总资产约 {(dashboard.TotalAssets > 0 ? decimal.ToDouble(dashboard.Cash / dashboard.TotalAssets) * 100 : 0):F1}% 。";
        }
        else if (normalized.Contains("建议") || normalized.Contains("怎么做"))
        {
            reply =
                $"建议先围绕三件事优化：1. 控制单一仓位占比；2. 定期更新持仓现价；3. 用交易记录复盘加减仓节奏。当前总资产 {FormatCurrency(dashboard.TotalAssets)}。";
        }
        else
        {
            reply =
                $"当前组合总资产约 {FormatCurrency(dashboard.TotalAssets)}，其中现金 {FormatCurrency(dashboard.Cash)}，持仓市值 {FormatCurrency(dashboard.PositionsValue)}。" +
                $"如果你愿意，我可以继续帮你看持仓、收益、风险或生成投资建议。";
        }

        if (!string.IsNullOrWhiteSpace(context))
        {
            reply += $"\n\n补充上下文：{context}";
        }

        return reply;
    }

    private async Task<string?> CallGlmAsync(string systemPrompt, string userMessage)
    {
        if (!HasApiKey())
        {
            return null;
        }

        try
        {
            var requestBody = new
            {
                model = _options.Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                max_tokens = _options.MaxTokens,
                temperature = _options.Temperature
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
            request.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GLM chat request failed with status code {StatusCode}", response.StatusCode);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseContent);

            return jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Calling GLM chat endpoint failed");
            return null;
        }
    }

    private bool HasApiKey() => !string.IsNullOrWhiteSpace(_options.ApiKey);

    private static string NormalizeReportType(string reportType)
    {
        return reportType.Trim().ToLowerInvariant() switch
        {
            "week" or "weekly" => "weekly",
            "month" or "monthly" => "monthly",
            "quarter" or "quarterly" => "quarterly",
            "year" or "yearly" => "yearly",
            _ => "weekly"
        };
    }

    private static string GetReportLabel(string reportType) => reportType switch
    {
        "monthly" => "月度报告",
        "quarterly" => "季度报告",
        "yearly" => "年度报告",
        _ => "周度报告"
    };

    private static string FormatCurrency(decimal value) => $"¥{value:N2}";

    private static string FormatSignedCurrency(decimal value) => $"{(value >= 0 ? "+" : string.Empty)}¥{value:N2}";

    private static double Clamp01(double value) => Math.Clamp(value, 0.05d, 0.95d);
}
