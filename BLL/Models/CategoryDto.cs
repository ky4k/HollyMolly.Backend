namespace HM.BLL.Models;

public class CategoryDto
{
    public int Id { get; set; }
    public int CategoryGroupId { get; set; }
    public string Name { get; set; } = null!;
    public string Link { get; set; } = null!;
}
