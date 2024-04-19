namespace HM.BLL.Models.Statistics;

public class OrderStatisticDto
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }
    public int NumberOfOrders { get; set; }
    public decimal TotalCostBeforeDiscount => TotalCost + TotalDiscount;
    public decimal TotalDiscount { get; set; }
    public decimal AverageOrderCost => NumberOfOrders == 0 ? 0 : TotalCost / NumberOfOrders;
    public decimal TotalCost { get; set; }
}
