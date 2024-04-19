namespace HM.DAL.Entities;

public class ProductInstanceStatistics
{
    public int Id { get; set; }
    public int ProductStatisticsId { get; set; }
    public ProductStatistics ProductStatistics { get; set; } = null!;
    public int ProductInstanceId { get; set; }
    public ProductInstance ProductInstance { get; set; } = null!;
    public int NumberOfPurchases { get; set; }
    public decimal TotalRevenue { get; set; }
}
