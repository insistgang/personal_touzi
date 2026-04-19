using System.ComponentModel.DataAnnotations;

namespace PersonalTouzi.Api.Models;

/// <summary>
/// 创建账户请求
/// </summary>
public record CreateAccountRequest
{
    [Required(ErrorMessage = "账户名称不能为空")]
    [StringLength(100, ErrorMessage = "账户名称不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "描述不能超过200个字符")]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "初始资金不能为负数")]
    public decimal InitialCash { get; set; }
}

/// <summary>
/// 更新账户请求
/// </summary>
public record UpdateAccountRequest
{
    [Required(ErrorMessage = "账户名称不能为空")]
    [StringLength(100, ErrorMessage = "账户名称不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "描述不能超过200个字符")]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "资金不能为负数")]
    public decimal InitialCash { get; set; }
}

/// <summary>
/// 创建持仓请求
/// </summary>
public record CreatePositionRequest
{
    [Required(ErrorMessage = "证券代码不能为空")]
    [StringLength(20, ErrorMessage = "证券代码不能超过20个字符")]
    public string Symbol { get; set; } = string.Empty;

    [Required(ErrorMessage = "证券名称不能为空")]
    [StringLength(100, ErrorMessage = "证券名称不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "类型不能为空")]
    [RegularExpression("^(stock|fund|bond)$", ErrorMessage = "类型必须是 stock、fund 或 bond")]
    public string Type { get; set; } = "stock";

    [Range(0.0001, double.MaxValue, ErrorMessage = "数量必须大于0")]
    public decimal Quantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "成本价不能为负数")]
    public decimal CostPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "现价不能为负数")]
    public decimal CurrentPrice { get; set; }

    [Required(ErrorMessage = "账户ID不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "账户ID必须大于0")]
    public int AccountId { get; set; }
}

/// <summary>
/// 更新持仓请求
/// </summary>
public record UpdatePositionRequest
{
    [StringLength(20, ErrorMessage = "证券代码不能超过20个字符")]
    public string? Symbol { get; set; }

    [StringLength(100, ErrorMessage = "证券名称不能超过100个字符")]
    public string? Name { get; set; }

    [RegularExpression("^(stock|fund|bond)$", ErrorMessage = "类型必须是 stock、fund 或 bond")]
    public string? Type { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "数量必须大于0")]
    public decimal? Quantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "成本价不能为负数")]
    public decimal? CostPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "现价不能为负数")]
    public decimal? CurrentPrice { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "账户ID必须大于0")]
    public int? AccountId { get; set; }
}

/// <summary>
/// 初始持仓导入请求
/// </summary>
public record ImportInitialPositionsRequest
{
    [Required(ErrorMessage = "账户ID不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "账户ID必须大于0")]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "CSV 内容不能为空")]
    public string CsvContent { get; set; } = string.Empty;

    public bool HasHeader { get; set; } = true;
}

/// <summary>
/// 批量交易导入请求
/// </summary>
public record ImportTransactionsRequest
{
    [Required(ErrorMessage = "账户ID不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "账户ID必须大于0")]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "CSV 内容不能为空")]
    public string CsvContent { get; set; } = string.Empty;

    public bool HasHeader { get; set; } = true;
}

/// <summary>
/// 创建交易请求
/// </summary>
public record CreateTransactionRequest
{
    [Required(ErrorMessage = "证券代码不能为空")]
    [StringLength(20, ErrorMessage = "证券代码不能超过20个字符")]
    public string Symbol { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "证券名称不能超过100个字符")]
    public string? Name { get; set; }

    [RegularExpression("^(stock|fund|bond)?$", ErrorMessage = "资产类型必须是 stock、fund 或 bond")]
    public string? AssetType { get; set; }

    [Required(ErrorMessage = "交易类型不能为空")]
    [RegularExpression("^(buy|sell)$", ErrorMessage = "交易类型必须是 buy 或 sell")]
    public string Type { get; set; } = "buy";

    [Range(0.0001, double.MaxValue, ErrorMessage = "数量必须大于0")]
    public decimal Quantity { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "价格必须大于0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "交易日期不能为空")]
    public DateTime TradeDate { get; set; }

    [Required(ErrorMessage = "账户ID不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "账户ID必须大于0")]
    public int AccountId { get; set; }

    [StringLength(500, ErrorMessage = "备注不能超过500个字符")]
    public string? Remark { get; set; }
}

/// <summary>
/// 更新交易请求
/// </summary>
public record UpdateTransactionRequest
{
    [StringLength(20, ErrorMessage = "证券代码不能超过20个字符")]
    public string? Symbol { get; set; }

    [StringLength(100, ErrorMessage = "证券名称不能超过100个字符")]
    public string? Name { get; set; }

    [RegularExpression("^(stock|fund|bond)?$", ErrorMessage = "资产类型必须是 stock、fund 或 bond")]
    public string? AssetType { get; set; }

    [RegularExpression("^(buy|sell)$", ErrorMessage = "交易类型必须是 buy 或 sell")]
    public string? Type { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "数量必须大于0")]
    public decimal? Quantity { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "价格必须大于0")]
    public decimal? Price { get; set; }

    public DateTime? TradeDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "账户ID必须大于0")]
    public int? AccountId { get; set; }

    [StringLength(500, ErrorMessage = "备注不能超过500个字符")]
    public string? Remark { get; set; }
}
