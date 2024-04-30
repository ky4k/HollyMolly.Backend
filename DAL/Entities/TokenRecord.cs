namespace HM.DAL.Entities;

public class TokenRecord
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
