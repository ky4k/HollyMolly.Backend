using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class LoginOidcRequestValidator : AbstractValidator<LoginOidcRequest>
{
    public LoginOidcRequestValidator()
    {
        RuleFor(lor => lor.Token)
            .NotEmpty()
                .WithMessage("Token is required");
    }
}
