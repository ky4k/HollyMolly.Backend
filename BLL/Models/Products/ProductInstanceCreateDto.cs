namespace HM.BLL.Models.Products;

public class ProductInstanceCreateDto
{
    public bool IsNewCollection { get; set; }
    public string? Status { get; set; }
    public string? SKU { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public decimal Price { get; set; }
    public decimal AbsoluteDiscount { get; set; }
    public decimal PercentageDiscount { get; set; }
    public int StockQuantity { get; set; }
}
