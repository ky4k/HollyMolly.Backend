namespace HM.BLL.Models;

public class OrderRecordDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Total => Quantity * Price;
}
