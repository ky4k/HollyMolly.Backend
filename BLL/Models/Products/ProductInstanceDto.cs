namespace HM.BLL.Models.Products;

public class ProductInstanceDto
{
    public int Id { get; set; }
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
    public decimal PriceAfterDiscount => (Price - AbsoluteDiscount) * (100 - PercentageDiscount) / 100;
    public List<ProductImageDto> Images { get; set; } = [];
}
