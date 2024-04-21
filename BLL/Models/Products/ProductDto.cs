using HM.DAL.Entities;

namespace HM.BLL.Models.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Rating { get; set; }
    public int TimesRated { get; set; }
    public int CategoryId { get; set; }
    public List<ProductInstanceDto> ProductsInstances { get; set; } = [];
    public List<ProductFeedback> Feedbacks { get; set; } = [];
}
