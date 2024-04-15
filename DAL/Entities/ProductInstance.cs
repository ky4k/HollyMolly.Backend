namespace HM.DAL.Entities;

public class ProductInstance
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string? SKU { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int? DiscountId { get; set; }
    public Discount? Discount { get; set; }
    public List<ProductImage> Images { get; set; } = [];
    public List<OrderRecord> OrderRecords { get; set; } = [];
}
