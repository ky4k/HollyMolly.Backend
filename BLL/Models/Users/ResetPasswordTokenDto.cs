namespace HM.BLL.Models.Users;

public class ResetPasswordTokenDto
{
    public string UserId { get; set; } = null!;
    public string Token { get; set; } = null!;
}
