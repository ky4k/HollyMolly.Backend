using FluentValidation;
using HM.BLL.Models.Users;

namespace HM.BLL.Validators;

public class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
{

    public RegistrationRequestValidator()
    {
        RuleFor(rr => rr.Email)
            .ApplyEmailValidationRules();
        RuleFor(rr => rr.Password)
            .ApplyPasswordValidationRules();
    }
}
