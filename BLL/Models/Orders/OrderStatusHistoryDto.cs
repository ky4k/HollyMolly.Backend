namespace HM.BLL.Models.Orders;

public class OrderStatusHistoryDto
{
    public string Status { get; set; } = null!;
    public DateTimeOffset Date { get; set; }
    public string Notes { get; set; } = string.Empty;
}
