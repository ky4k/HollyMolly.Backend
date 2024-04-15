namespace HM.BLL.Models;

public class ProductInstanceCreateDto
{
    public string? SKU { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
