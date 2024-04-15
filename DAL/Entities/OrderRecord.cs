namespace HM.DAL.Entities;

public class OrderRecord
{
    public int Id { get; set; }
    public int ProductInstanceId { get; set; }
    public ProductInstance ProductInstance { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public int OrderId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
