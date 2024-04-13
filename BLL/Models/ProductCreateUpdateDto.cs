namespace HM.BLL.Models;

public class ProductCreateUpdateDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = null!;
    public int StockQuantity { get; set; }
}
