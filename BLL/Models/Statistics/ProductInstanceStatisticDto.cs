namespace HM.BLL.Models.Statistics;

public class ProductInstanceStatisticDto
{
    public int Id { get; set; }
    public string? SKU { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public int NumberOfPurchases { get; set; }
    public decimal TotalRevenue { get; set; }
}
