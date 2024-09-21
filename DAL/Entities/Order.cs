using HM.DAL.Entities.NewPost;

namespace HM.DAL.Entities;

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public CustomerInfo Customer { get; set; } = null!;
    public List<OrderRecord> OrderRecords { get; set; } = [];
    public DateTimeOffset OrderDate { get; set; }
    public string Status { get; set; } = null!;
    public List<OrderStatusHistory> StatusHistory { get; set; } = [];
    public bool PaymentReceived { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<Support> Supports { get; set; } = [];
    public NewPostInternetDocument? NewPostInternetDocument { get; set; }
}
