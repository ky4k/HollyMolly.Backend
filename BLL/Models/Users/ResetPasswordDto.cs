namespace HM.BLL.Models.Users;

public class ResetPasswordDto
{
    public string ResetToken { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
