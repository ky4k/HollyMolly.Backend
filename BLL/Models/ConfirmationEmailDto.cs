namespace HM.BLL.Models;

public class ConfirmationEmailDto
{
    public string UserId { get; set; } = null!;
    public string Token { get; set; } = null!;
}
