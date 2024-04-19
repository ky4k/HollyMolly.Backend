namespace HM.DAL.Entities;

public class NewsSubscription
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string RemoveToken { get; set; } = null!;
}
