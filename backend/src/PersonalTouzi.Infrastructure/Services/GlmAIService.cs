using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace PersonalTouzi.Infrastructure.Services;

/// <summary>
/// GLM AI 配置选项
/// </summary>
public class GlmAIOptions
{
    public const string SectionName = "AI";
    public string Provider { get; set; } = "GLM";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "glm-4-flash";
    public int MaxTokens { get; set; } = 4096;
    public double Temperature { get; set; } = 0.7;
}

/// <summary>
/// GLM (智谱AI) 服务实现
/// </summary>
public class GlmAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly GlmAIOptions _options;
    private const string BaseUrl = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

    public GlmAIService(HttpClient httpClient, IOptions<GlmAIOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    /// <summary>
    /// 智能对话
    /// </summary>
    public async Task<string> ChatAsync(string message, string? context = null)
    {
        var systemPrompt = "你是一个专业的量化投资助手，帮助用户分析投资组合、提供市场洞察和投资建议。";

        if (!string.IsNullOrEmpty(context))
        {
            systemPrompt += $"\n\n上下文信息：{context}";
        }

        var response = await CallGlmAsync(systemPrompt, message);
        return response;
    }

    /// <summary>
    /// 分析投资组合
    /// </summary>
    public async Task<PortfolioAnalysis> AnalyzePortfolioAsync(int accountId)
    {
        var prompt = $@"
请分析账户 ID {accountId} 的投资组合，并提供以下信息（基于示例数据）：

1. 风险等级（0-1之间的数值）
2. 风险评估描述
3. 投资建议列表
4. 分析摘要

请以 JSON 格式返回，格式如下：
{{
  ""riskLevel"": 0.5,
  ""riskAssessment"": ""中等风险"",
  ""suggestions"": [""建议1"", ""建议2"", ""建议3""],
  ""summary"": ""整体分析摘要""
}}
";

        var response = await CallGlmAsync(
            "你是一个专业的投资组合分析师，擅长评估投资风险和提供优化建议。",
            prompt);

        try
        {
            var jsonDoc = JsonDocument.Parse(response);
            var root = jsonDoc.RootElement;

            return new PortfolioAnalysis(
                RiskLevel: root.GetProperty("riskLevel").GetDouble(),
                RiskAssessment: root.GetProperty("riskAssessment").GetString() ?? "",
                Suggestions: root.GetProperty("suggestions").EnumerateArray()
                    .Select(x => x.GetString() ?? "")
                    .ToList(),
                Summary: root.GetProperty("summary").GetString() ?? ""
            );
        }
        catch
        {
            // 返回默认值
            return new PortfolioAnalysis(
                RiskLevel: 0.5,
                RiskAssessment: "中等风险",
                Suggestions: new List<string> { "建议分散投资以降低风险", "定期 review 投资组合", "关注市场动态" },
                Summary: response
            );
        }
    }

    /// <summary>
    /// 生成投资报告
    /// </summary>
    public async Task<string> GenerateReportAsync(int accountId, string reportType = "weekly")
    {
        var reportTypeName = reportType switch
        {
            "monthly" => "月度",
            "yearly" => "年度",
            _ => "周度"
        };

        var prompt = $@"
请为账户 ID {accountId} 生成一份{reportTypeName}投资报告，包含以下内容：

1. 报告期间
2. 整体收益表现
3. 主要持仓分析
4. 交易记录摘要
5. 风险指标
6. 下期展望

报告格式要专业、清晰，便于阅读。
";

        return await CallGlmAsync(
            "你是一个专业的投资报告撰写专家，擅长生成结构清晰、内容详实的投资报告。",
            prompt);
    }

    /// <summary>
    /// 市场情绪分析
    /// </summary>
    public async Task<MarketSentiment> AnalyzeMarketSentimentAsync(string[] stockCodes)
    {
        var stocks = string.Join(", ", stockCodes);
        var prompt = $@"
请分析以下股票的市场情绪：{stocks}

请提供：
1. 整体情绪（看涨/看跌/中性）
2. 置信度分数（0-1之间）
3. 关键影响因素列表
4. 投资建议

请以 JSON 格式返回：
{{
  ""overallSentiment"": ""看涨"",
  ""confidenceScore"": 0.75,
  ""keyFactors"": [""因素1"", ""因素2"", ""因素3""],
  ""recommendation"": ""建议持有""
}}
";

        var response = await CallGlmAsync(
            "你是一个专业的市场分析师，擅长分析市场情绪和预测市场走向。",
            prompt);

        try
        {
            var jsonDoc = JsonDocument.Parse(response);
            var root = jsonDoc.RootElement;

            return new MarketSentiment(
                OverallSentiment: root.GetProperty("overallSentiment").GetString() ?? "",
                ConfidenceScore: root.GetProperty("confidenceScore").GetDouble(),
                KeyFactors: root.GetProperty("keyFactors").EnumerateArray()
                    .Select(x => x.GetString() ?? "")
                    .ToList(),
                Recommendation: root.GetProperty("recommendation").GetString() ?? ""
            );
        }
        catch
        {
            return new MarketSentiment(
                OverallSentiment: "中性",
                ConfidenceScore: 0.5,
                KeyFactors: new List<string> { "市场波动较大", "需要更多数据支持" },
                Recommendation: "建议观望"
            );
        }
    }

    /// <summary>
    /// 收益预测
    /// </summary>
    public async Task<RevenuePrediction> PredictRevenueAsync(int accountId, int days = 30)
    {
        var prompt = $@"
请为账户 ID {accountId} 预测未来 {days} 天的收益情况。

请提供：
1. 预测数据点（日期、预测值、置信度）
2. 预期收益率
3. 整体置信度水平
4. 分析说明

请以 JSON 格式返回：
{{
  ""predictions"": [
    {{ ""date"": ""2024-01-15"", ""predictedValue"": 10000.50, ""confidence"": 0.8 }}
  ],
  ""expectedReturn"": 0.05,
  ""confidenceLevel"": 0.75,
  ""analysis"": ""基于历史数据和市场趋势...""
}}
";

        var response = await CallGlmAsync(
            "你是一个专业的量化分析师，擅长使用统计模型预测投资收益。",
            prompt);

        try
        {
            var jsonDoc = JsonDocument.Parse(response);
            var root = jsonDoc.RootElement;

            var predictions = root.GetProperty("predictions").EnumerateArray()
                .Select(x => new PredictionPoint(
                    Date: DateTime.Parse(x.GetProperty("date").GetString() ?? DateTime.Now.ToString("yyyy-MM-dd")),
                    PredictedValue: x.GetProperty("predictedValue").GetDecimal(),
                    Confidence: x.GetProperty("confidence").GetDouble()
                ))
                .ToList();

            return new RevenuePrediction(
                Predictions: predictions,
                ExpectedReturn: root.GetProperty("expectedReturn").GetDouble(),
                ConfidenceLevel: root.GetProperty("confidenceLevel").GetDouble(),
                Analysis: root.GetProperty("analysis").GetString() ?? ""
            );
        }
        catch
        {
            // 返回示例预测数据
            var today = DateTime.Today;
            var predictions = new List<PredictionPoint>();
            for (int i = 1; i <= days; i++)
            {
                predictions.Add(new PredictionPoint(
                    Date: today.AddDays(i),
                    PredictedValue: 10000m + (i * 100m),
                    Confidence: Math.Max(0.5, 1.0 - (i * 0.02))
                ));
            }

            return new RevenuePrediction(
                Predictions: predictions,
                ExpectedReturn: 0.05,
                ConfidenceLevel: 0.7,
                Analysis: "基于历史数据和市场趋势的预测（示例数据）"
            );
        }
    }

    /// <summary>
    /// 调用 GLM API
    /// </summary>
    private async Task<string> CallGlmAsync(string systemPrompt, string userMessage)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            return "AI 服务未配置，请在 appsettings.json 中设置 API Key。";
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

            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
            request.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"API 调用失败: {response.StatusCode} - {responseContent}";
            }

            var jsonDoc = JsonDocument.Parse(responseContent);
            return jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }
        catch (Exception ex)
        {
            return $"调用 AI 服务时发生错误: {ex.Message}";
        }
    }
}
