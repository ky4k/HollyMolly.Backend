namespace HM.DAL.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string ImageLink { get; set; } = null!;
    public string ImageFilePath { get; set; } = null!;
    public List<Product> Products { get; set; } = [];
}
