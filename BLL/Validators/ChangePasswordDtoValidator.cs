using FluentValidation;
using HM.BLL.Models.Users;

namespace HM.BLL.Validators;

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(cpd => cpd.OldPassword)
            .NotEmpty()
                .WithMessage("Old password is required");
        RuleFor(cpd => cpd.NewPassword)
            .ApplyPasswordValidationRules();
    }
}
