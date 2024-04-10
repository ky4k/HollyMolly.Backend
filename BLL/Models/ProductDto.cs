using HM.DAL.Entities;

namespace HM.BLL.Models;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal Rating { get; set; }
    public string Category { get; set; } = null!;
    public int StockQuantity { get; set; }
    public List<string> Images { get; set; } = new();
    public List<ProductFeedback> Feedbacks { get; set; } = new();
}
