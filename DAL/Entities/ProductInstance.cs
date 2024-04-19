namespace HM.DAL.Entities;

public class ProductInstance
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public bool IsNewCollection { get; set; }
    public string? Status { get; set; }
    public string? SKU { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public decimal AbsoluteDiscount { get; set; }
    public decimal PercentageDiscount { get; set; }
    public List<ProductImage> Images { get; set; } = [];
    public List<OrderRecord> OrderRecords { get; set; } = [];
    public List<ProductInstanceStatistics> ProductInstanceStatistics { get; set; } = [];
    public decimal GetCombinedDiscount()
    {
        return AbsoluteDiscount + (Price - AbsoluteDiscount) * PercentageDiscount / 100;
    }
}
