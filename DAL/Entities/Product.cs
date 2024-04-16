namespace HM.DAL.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Rating { get; set; }
    public int TimesRated { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ProductStatistics ProductStatistics { get; set; } = null!;
    public List<ProductInstance> ProductInstances { get; set; } = [];
    public List<ProductFeedback> Feedbacks { get; set; } = [];
    public List<WishList> WishLists { get; set; } = [];
}
