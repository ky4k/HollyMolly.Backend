namespace HM.BLL.Models.Products;

public class ProductCreateDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public List<ProductInstanceCreateDto> ProductInstances { get; set; } = [];
}
