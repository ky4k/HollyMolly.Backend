namespace HM.BLL.Models;

public class OrderCreateDto
{
    public CustomerDto Customer { get; set; } = null!;
    public List<OrderRecordCreateDto> OrderRecords { get; set; } = [];
}
