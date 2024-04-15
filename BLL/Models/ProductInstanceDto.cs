namespace HM.BLL.Models;

public class ProductInstanceDto
{
    public int Id { get; set; }
    public string? SKU { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DiscountDto? Discount { get; set; }
    public List<ProductImageDto> Images { get; set; } = [];
}
