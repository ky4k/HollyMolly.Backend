namespace HM.DAL.Entities;

public class Discount
{
    public int Id { get; set; }
    public decimal AbsoluteDiscount { get; set; }
    public decimal PercentageDiscount { get; set; }
    public List<ProductInstance> ProductInstances { get; set; } = [];
}
