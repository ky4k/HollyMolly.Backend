using FluentValidation;
using HM.BLL.Interfaces;
using HM.BLL.Models.Users;

namespace HM.BLL.Validators;

public class ProfileUpdateDtoValidator : AbstractValidator<ProfileUpdateDto>
{
    public ProfileUpdateDtoValidator(INewPostService newPostService)
    {
        RuleFor(pud => pud.FirstName)
            .ApplyNameValidationRules();
        RuleFor(pud => pud.LastName)
            .ApplyNameValidationRules();
        RuleFor(pud => pud.PhoneNumber)
            .ApplyPhoneNumberValidationRules();
        RuleFor(pud => pud.DateOfBirth)
            .Must(db => db == null || db.Value < DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Date of birth must be in the past");
        RuleFor(pud => pud.DeliveryAddress)
            .MustAsync(async (customer, address, cancellation) => address == null
                || customer.City != null && await newPostService.CheckIfAddressIsValidAsync(customer.City, address, cancellation))
                .WithMessage("New post office does not exist on the specified address in the city.");
    }
}
