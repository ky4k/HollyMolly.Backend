using HM.BLL.Models.Orders;
using HM.DAL.Enums;

namespace HM.BLL.Models.Supports;

public class SupportDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public SupportTopic Topic { get; set; }
    public string Description { get; set; } = null!;
    public OrderDto? Order { get; set; }
}
