namespace HM.DAL.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<Product> Products { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public string Status { get; set; } = null!;
}
