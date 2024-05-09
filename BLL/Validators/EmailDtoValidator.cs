using FluentValidation;
using HM.BLL.Models.Users;

namespace HM.BLL.Validators;

public class EmailDtoValidator : AbstractValidator<EmailDto>
{
    public EmailDtoValidator()
    {
        RuleFor(e => e.Email)
            .ApplyEmailValidationRules();
    }
}
