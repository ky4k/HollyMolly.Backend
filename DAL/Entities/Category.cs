namespace HM.DAL.Entities;

public class Category
{
    public int Id { get; set; }
    public int CategoryGroupId { get; set; }
    public CategoryGroup CategoryGroup { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int Position { get; set; }
    public string ImageLink { get; set; } = string.Empty;
    public string ImageFilePath { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = [];
}
