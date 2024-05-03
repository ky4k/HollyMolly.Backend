using HM.DAL.Enums;

namespace HM.BLL.Models.Supports;

public class SupportDto
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public SupportTopic Topic { get; set; }
    public string Description { get; set; } = null!;
    public int? OrderId { get; set; }
}
