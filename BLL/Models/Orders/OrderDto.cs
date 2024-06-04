namespace HM.BLL.Models.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public CustomerDto Customer { get; set; } = null!;
    public List<OrderRecordDto> OrderRecords { get; set; } = [];
    public decimal TotalCost
    {
        get
        {
            decimal total = 0;
            foreach (OrderRecordDto record in OrderRecords)
            {
                total += record.TotalCost;
            }
            return total;
        }
    }
    public DateTimeOffset OrderDate { get; set; }
    public string Status { get; set; } = null!;
    public List<OrderStatusHistoryDto> StatusHistory { get; set; } = [];
    public bool PaymentReceived { get; set; }
    public string Notes { get; set; } = null!;
}
