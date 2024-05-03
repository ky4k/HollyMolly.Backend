using HM.DAL.Enums;

namespace HM.DAL.Entities;

public class Support
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public SupportTopic Topic { get; set; }
    public string Description { get; set; } = null!;
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
}
