namespace HM.DAL.Entities;

public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; } = null!;
    public DateTimeOffset Date { get; set; }
    public string Notes { get; set; } = string.Empty;
}
