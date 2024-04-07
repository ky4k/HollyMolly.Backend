using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
{
    public RegistrationRequestValidator()
    {
        RuleFor(rr => rr.Email)
            .NotEmpty()
                .WithMessage("Email is required.")
            .Matches("^[A-Za-z0-9-_.]{1,50}@[A-Za-z0-9-.]{1,40}\\.[A-Za-z0-9-]{2,10}$")
                .WithMessage("Email is in an invalid format.");
        RuleFor(rr => rr.Password)
            .NotEmpty()
                .WithMessage("Password is required.")
            .MinimumLength(8)
                .WithMessage("Password must be between 8 and 25 characters long.")
            .MaximumLength(50)
                .WithMessage("Password must be between 8 and 25 characters long.")
            .Matches("^[A-Za-z0-9~`!@#$%^&*()_\\-+={[}\\]|\\\\:;\"'<,>.?\\/]{8,25}$")
                .WithMessage("Password may only contain Latin letters, digits, or special characters.")
            .Must(p => !int.TryParse(p, out _))
                .WithMessage("Password cannot consist of digits only.");
    }
}
