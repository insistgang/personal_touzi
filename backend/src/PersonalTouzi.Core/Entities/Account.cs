namespace PersonalTouzi.Core.Entities;

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal InitialCash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
