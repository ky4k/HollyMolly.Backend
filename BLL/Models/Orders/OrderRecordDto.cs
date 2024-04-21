namespace HM.BLL.Models.Orders;

public class OrderRecordDto
{
    public int ProductInstanceId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal TotalCostBeforeDiscount => Quantity * Price;
    public decimal Discount { get; set; }
    public int Quantity { get; set; }
    public decimal TotalCost => Quantity * Price - Discount;
}
