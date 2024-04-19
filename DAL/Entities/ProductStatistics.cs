namespace HM.DAL.Entities;

public class ProductStatistics
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int NumberViews { get; set; }
    public int NumberFeedbacks { get; set; }
    public int NumberWishlistAdditions { get; set; }
    public List<ProductInstanceStatistics> ProductInstanceStatistics { get; set; } = [];
}
