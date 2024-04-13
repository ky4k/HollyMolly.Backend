namespace HM.DAL.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string ImageLink { get; set; } = string.Empty;
    public string ImageFilePath { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = [];
}
