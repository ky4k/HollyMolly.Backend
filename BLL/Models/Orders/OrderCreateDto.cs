namespace HM.BLL.Models.Orders;

public class OrderCreateDto
{
    public CustomerCreateDto Customer { get; set; } = null!;
    public List<OrderRecordCreateDto> OrderRecords { get; set; } = [];
}
