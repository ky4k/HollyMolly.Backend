namespace HM.DAL.Entities;

public class ProductStatistics
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int NumberViews { get; set; }
    public int NumberPurchases { get; set; }
    public decimal ConversionRate => NumberViews / (decimal)NumberPurchases;
    public int NumberWishlistAdditions { get; set; }
}
