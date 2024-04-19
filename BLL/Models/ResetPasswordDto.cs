namespace HM.BLL.Models;

public class ResetPasswordDto
{
    public string ResetToken { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
