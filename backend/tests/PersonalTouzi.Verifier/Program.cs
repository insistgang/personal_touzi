using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Core.Entities;
using PersonalTouzi.Infrastructure.Data;
using PersonalTouzi.Infrastructure.Services;

await RunAsync();

static async Task RunAsync()
{
    await VerifyBuySettlementAsync();
    await VerifySellSettlementAsync();
    await VerifyOversellValidationAsync();
    await VerifyInitialPositionImportAsync();
    await VerifyTransactionImportRollbackAsync();

    Console.WriteLine("Verifier passed: settlement and import rules are working.");
}

static async Task VerifyBuySettlementAsync()
{
    await using var database = await CreateDatabaseAsync();
    await using var context = database.CreateContext();

    var account = new Account
    {
        Name = "验证账户",
        Description = "买入场景",
        InitialCash = 10000m
    };

    context.Accounts.Add(account);
    await context.SaveChangesAsync();

    context.Positions.Add(new Position
    {
        AccountId = account.Id,
        Code = "000001",
        Name = "平安银行",
        Type = "stock",
        Quantity = 100m,
        CostPrice = 10m,
        CurrentPrice = 10m
    });
    await context.SaveChangesAsync();

    var service = new TransactionSettlementService(context);

    await service.RecordTransactionAsync(new RecordTransactionCommand(
        AccountId: account.Id,
        Symbol: "000001",
        Name: "平安银行",
        Type: "buy",
        Quantity: 50m,
        Price: 12m,
        TradeDate: new DateTime(2026, 4, 17),
        Remark: "加仓验证",
        AssetType: "stock"));

    var refreshedAccount = await context.Accounts.SingleAsync();
    var refreshedPosition = await context.Positions.SingleAsync();

    AssertEqual(9400m, refreshedAccount.InitialCash, "buy cash");
    AssertEqual(150m, refreshedPosition.Quantity, "buy quantity");
    AssertEqual(12m, refreshedPosition.CurrentPrice, "buy current price");
    AssertEqual(Math.Round(1600m / 150m, 6), Math.Round(refreshedPosition.CostPrice, 6), "buy weighted cost");
}

static async Task VerifySellSettlementAsync()
{
    await using var database = await CreateDatabaseAsync();
    await using var context = database.CreateContext();

    var account = new Account
    {
        Name = "验证账户",
        Description = "卖出场景",
        InitialCash = 5000m
    };

    context.Accounts.Add(account);
    await context.SaveChangesAsync();

    context.Positions.Add(new Position
    {
        AccountId = account.Id,
        Code = "159915",
        Name = "创业板ETF",
        Type = "fund",
        Quantity = 300m,
        CostPrice = 2m,
        CurrentPrice = 2.1m
    });
    await context.SaveChangesAsync();

    var service = new TransactionSettlementService(context);

    await service.RecordTransactionAsync(new RecordTransactionCommand(
        AccountId: account.Id,
        Symbol: "159915",
        Name: "创业板ETF",
        Type: "sell",
        Quantity: 300m,
        Price: 2.25m,
        TradeDate: new DateTime(2026, 4, 17),
        Remark: "清仓验证",
        AssetType: "fund"));

    var refreshedAccount = await context.Accounts.SingleAsync();

    AssertEqual(5675m, refreshedAccount.InitialCash, "sell cash");
    AssertEqual(0, await context.Positions.CountAsync(), "sell removes position");
}

static async Task VerifyOversellValidationAsync()
{
    await using var database = await CreateDatabaseAsync();
    await using var context = database.CreateContext();

    var account = new Account
    {
        Name = "验证账户",
        Description = "超量卖出场景",
        InitialCash = 5000m
    };

    context.Accounts.Add(account);
    await context.SaveChangesAsync();

    context.Positions.Add(new Position
    {
        AccountId = account.Id,
        Code = "600519",
        Name = "贵州茅台",
        Type = "stock",
        Quantity = 10m,
        CostPrice = 1500m,
        CurrentPrice = 1800m
    });
    await context.SaveChangesAsync();

    var service = new TransactionSettlementService(context);

    try
    {
        await service.RecordTransactionAsync(new RecordTransactionCommand(
            AccountId: account.Id,
            Symbol: "600519",
            Name: "贵州茅台",
            Type: "sell",
            Quantity: 20m,
            Price: 1800m,
            TradeDate: new DateTime(2026, 4, 17),
            Remark: "超卖验证",
            AssetType: "stock"));
    }
    catch (PortfolioRuleException ex) when (ex.Message == "卖出数量超过当前持仓数量。")
    {
        return;
    }

    throw new InvalidOperationException("oversell validation did not trigger expected rule.");
}

static async Task VerifyInitialPositionImportAsync()
{
    await using var database = await CreateDatabaseAsync();
    await using var context = database.CreateContext();

    var account = new Account
    {
        Name = "导入账户",
        Description = "初始持仓导入",
        InitialCash = 20000m
    };

    context.Accounts.Add(account);
    await context.SaveChangesAsync();

    var service = new PortfolioImportService(context, new TransactionSettlementService(context));

    var result = await service.ImportInitialPositionsAsync(new ImportInitialPositionsCommand(
        AccountId: account.Id,
        CsvContent: """
                    symbol,name,type,quantity,costPrice,currentPrice
                    510300,沪深300ETF,fund,1000,3.95,4.08
                    600036,招商银行,stock,200,34.50,36.10
                    """));

    AssertEqual(2, result.ImportedCount, "initial position import count");
    AssertEqual(2, await context.Positions.CountAsync(), "initial position persisted count");
    AssertEqual(11300m, Math.Round(result.TotalMarketValue, 2), "initial position market value");
}

static async Task VerifyTransactionImportRollbackAsync()
{
    await using var database = await CreateDatabaseAsync();
    await using var context = database.CreateContext();

    var account = new Account
    {
        Name = "导入账户",
        Description = "批量交易回滚",
        InitialCash = 1000m
    };

    context.Accounts.Add(account);
    await context.SaveChangesAsync();

    var service = new PortfolioImportService(context, new TransactionSettlementService(context));

    try
    {
        await service.ImportTransactionsAsync(new ImportTransactionsCommand(
            AccountId: account.Id,
            CsvContent: """
                        tradeDate,symbol,name,assetType,type,quantity,price,remark
                        2026-04-17,000001,平安银行,stock,buy,10,10,首笔买入
                        2026-04-17,600519,贵州茅台,stock,buy,1,1800,超出资金
                        """));
    }
    catch (PortfolioRuleException ex) when (ex.Message == "账户可用现金不足，无法完成本次买入。")
    {
        var refreshedAccount = await context.Accounts.SingleAsync();
        AssertEqual(1000m, refreshedAccount.InitialCash, "transaction import rollback cash");
        AssertEqual(0, await context.Positions.CountAsync(), "transaction import rollback positions");
        AssertEqual(0, await context.Transactions.CountAsync(), "transaction import rollback transactions");
        return;
    }

    throw new InvalidOperationException("transaction import rollback did not trigger expected rule.");
}

static async Task<SqliteTestDatabase> CreateDatabaseAsync()
{
    var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new ApplicationDbContext(options);
    await context.Database.EnsureCreatedAsync();

    return new SqliteTestDatabase(connection, options);
}

static void AssertEqual<T>(T expected, T actual, string label) where T : notnull
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{label} failed. Expected: {expected}; Actual: {actual}");
    }
}

file sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public SqliteTestDatabase(
        SqliteConnection connection,
        DbContextOptions<ApplicationDbContext> options)
    {
        _connection = connection;
        _options = options;
    }

    public ApplicationDbContext CreateContext() => new(_options);

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
