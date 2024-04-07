using FluentValidation;

namespace HM.BLL.Validators;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> ApplyPasswordValidationRules<T>(this IRuleBuilder<T, string> password)
    {
        return password
            .NotEmpty()
                .WithMessage("Password is required.")
            .MinimumLength(8)
                .WithMessage("Password must be between 8 and 25 characters long.")
            .MaximumLength(25)
                .WithMessage("Password must be between 8 and 25 characters long.")
            .Matches(@"^[A-Za-z0-9~`!@#$%^&*()_\-+={[}\]|:;'<,>.?/]*$")
                .WithMessage("Invalid symbol. Password may only contain Latin letters, digits, and the following special characters ~`!@#$%^&*()_-+={[}]|:;'<,>.?/")
            .Must(p => !int.TryParse(p, out _))
                .WithMessage("Password cannot consist of digits only.");
    }
}
