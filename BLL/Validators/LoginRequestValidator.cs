using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(lr => lr.UserNameOrEmail)
            .NotEmpty();
        RuleFor(lr => lr.Password)
            .NotEmpty();
    }
}
