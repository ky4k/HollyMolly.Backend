namespace HM.BLL.Models.Statistics;

public class CategoryStatisticDto
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int NumberProductViews { get; set; }
    public int NumberPurchases { get; set; }
    public decimal TotalRevenue { get; set; }
    public int WishListAddition { get; set; }
    public int NumberReviews { get; set; }
}
