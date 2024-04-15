using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class ProfileUpdateDtoValidator : AbstractValidator<ProfileUpdateDto>
{
    public ProfileUpdateDtoValidator()
    {
        RuleFor(pud => pud.FirstName)
            .ApplyNameValidationRules();
        RuleFor(pud => pud.LastName)
            .ApplyNameValidationRules();
        RuleFor(pud => pud.PhoneNumber)
            .ApplyPhoneNumberValidationRules();
        RuleFor(pud => pud.DateOfBirth)
            .Must(db => db == null || db < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past");
        RuleFor(pud => pud.City)
            .MinimumLength(1);
        RuleFor(pud => pud.DeliveryAddress)
            .MinimumLength(1);
    }
}
