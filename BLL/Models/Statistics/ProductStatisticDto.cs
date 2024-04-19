namespace HM.BLL.Models.Statistics;

public class ProductStatisticDto
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int NumberViews { get; set; }
    public int NumberPurchases
    {
        get
        {
            int sum = 0;
            foreach(var instance in InstancesStatistics)
            {
                sum += instance.NumberOfPurchases;
            }
            return sum;
        }
    }
    public decimal TotalRevenue
    {
        get
        {
            decimal sum = 0;
            foreach (var instance in InstancesStatistics)
            {
                sum += instance.TotalRevenue;
            }
            return sum;
        }
    }
    public decimal ConversionRate => NumberPurchases == 0 ? 0 : NumberViews / (decimal)NumberPurchases;
    public int NumberWishListAddition { get; set; }
    public int NumberReviews { get; set; }
    public decimal Rating { get; set; }
    public List<ProductInstanceStatisticDto> InstancesStatistics { get; set; } = [];
}
