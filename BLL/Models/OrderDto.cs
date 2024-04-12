namespace HM.BLL.Models;

public class OrderDto
{
    public int Id { get; set; }
    public CustomerDto Customer { get; set; } = null!;
    public List<OrderRecordDto> OrderRecords { get; set; } = [];
    public decimal TotalPrice
    {
        get
        {
            decimal total = 0;
            foreach (var record in OrderRecords)
            {
                total += record.Total;
            }
            return total;
        }
    }
    public DateTimeOffset OrderDate { get; set; }
    public string Status { get; set; } = null!;
    public string Notes { get; set; } = null!;
}
