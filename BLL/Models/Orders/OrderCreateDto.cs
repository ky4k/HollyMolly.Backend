namespace HM.BLL.Models.Orders;

public class OrderCreateDto
{
    public CustomerDto Customer { get; set; } = null!;
    public List<OrderRecordCreateDto> OrderRecords { get; set; } = [];
}
