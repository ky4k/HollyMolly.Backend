namespace HM.BLL.Models.Categories;

public class CategoryUpdateDto
{
    public string CategoryName { get; set; } = null!;
    public int CategoryGroupId { get; set; }
}
