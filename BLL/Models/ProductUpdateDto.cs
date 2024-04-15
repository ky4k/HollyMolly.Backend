namespace HM.BLL.Models;

public class ProductUpdateDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
}
