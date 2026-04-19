using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Api.Middleware;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;
using PersonalTouzi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Personal Touzi API",
        Version = "v1",
        Description = "个人量化投资组合管理系统 API"
    });
});

// 配置 EF Core (使用 SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=personaltouzi.db"));

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 配置 AI 服务
builder.Services.Configure<GlmAIOptions>(
    builder.Configuration.GetSection(GlmAIOptions.SectionName));
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IPortfolioImportService, PortfolioImportService>();
builder.Services.AddScoped<ITransactionSettlementService, TransactionSettlementService>();
builder.Services.AddHttpClient<IAIService, GlmAIService>();

var app = builder.Build();

// 全局异常处理（放在最前面）
app.UseExceptionHandling();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Personal Touzi API v1");
    });
}

app.UseCors("AllowVueDev");
app.UseAuthorization();
app.MapControllers();

// 自动创建数据库并添加种子数据
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    // 添加测试账户
    if (!db.Accounts.Any())
    {
        db.Accounts.Add(new Account { Name = "主账户", Description = "主要投资账户", InitialCash = 100000 });
        db.Accounts.Add(new Account { Name = "副账户", Description = "备用账户", InitialCash = 50000 });
        db.SaveChanges();
    }

    // 添加测试持仓
    if (!db.Positions.Any())
    {
        var account = db.Accounts.First();
        db.Positions.AddRange(
            new Position { AccountId = account.Id, Code = "600519", Name = "贵州茅台", Type = "stock", Quantity = 100, CostPrice = 1800, CurrentPrice = 1850 },
            new Position { AccountId = account.Id, Code = "000001", Name = "平安银行", Type = "stock", Quantity = 1000, CostPrice = 12.5m, CurrentPrice = 13.2m },
            new Position { AccountId = account.Id, Code = "159915", Name = "创业板ETF", Type = "fund", Quantity = 5000, CostPrice = 2.1m, CurrentPrice = 2.25m }
        );
        db.SaveChanges();
    }

    // 添加测试交易记录
    if (!db.Transactions.Any())
    {
        var account = db.Accounts.First();
        db.Transactions.AddRange(
            new Transaction { AccountId = account.Id, Code = "600519", Name = "贵州茅台", Type = "buy", Quantity = 100, Price = 1800, TransactionDate = DateTime.Now.AddDays(-30) },
            new Transaction { AccountId = account.Id, Code = "000001", Name = "平安银行", Type = "buy", Quantity = 1000, Price = 12.5m, TransactionDate = DateTime.Now.AddDays(-20) }
        );
        db.SaveChanges();
    }

    var portfolioService = scope.ServiceProvider.GetRequiredService<IPortfolioService>();
    await portfolioService.SeedHistoricalSnapshotsIfEmptyAsync();
    await portfolioService.RefreshTodaySnapshotsAsync();
}

app.Run();
