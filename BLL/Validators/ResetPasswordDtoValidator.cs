using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

internal class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(rpd => rpd.ResetToken)
            .NotEmpty()
                .WithMessage("Reset key is required");
        RuleFor(rpd => rpd.NewPassword)
            .ApplyPasswordValidationRules();
    }
}
