namespace HM.BLL.Models.Categories;

public class CategoryGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Link { get; set; } = null!;
    public List<CategoryDto> Categories { get; set; } = [];
}
