namespace HM.DAL.Entities;

public class CategoryGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string ImageLink { get; set; } = string.Empty;
    public string ImageFilePath { get; set; } = string.Empty;
    public List<Category> Categories { get; set; } = [];
}
