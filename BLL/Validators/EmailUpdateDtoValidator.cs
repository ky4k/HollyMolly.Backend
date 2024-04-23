using FluentValidation;
using HM.BLL.Models.Users;

namespace HM.BLL.Validators;

public class EmailUpdateDtoValidator : AbstractValidator<EmailUpdateDto>
{
    public EmailUpdateDtoValidator()
    {
        RuleFor(e => e.NewEmail)
            .ApplyEmailValidationRules();
    }
}
