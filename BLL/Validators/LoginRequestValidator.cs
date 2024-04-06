using BLL.Models;
using FluentValidation;

namespace BLL.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(lr => lr.UserName)
            .NotEmpty();
        RuleFor(lr => lr.Password)
            .NotEmpty();
    }
}
