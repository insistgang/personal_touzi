namespace PersonalTouzi.Core.Entities;

public class Transaction
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "buy";
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Amount => Quantity * Price;
    public DateTime TransactionDate { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
