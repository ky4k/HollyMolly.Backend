namespace HM.BLL.Models.Categories;

public class CategoryGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Position { get; set; }
    public string Link { get; set; } = null!;
    public List<CategoryDto> Categories { get; set; } = [];
}
