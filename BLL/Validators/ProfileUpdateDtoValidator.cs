using FluentValidation;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Users;

namespace HM.BLL.Validators;

public class ProfileUpdateDtoValidator : AbstractValidator<ProfileUpdateDto>
{
    public ProfileUpdateDtoValidator(INewPostCityesService newPostService)
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
        RuleFor(pud => pud.City)
            .MustAsync(async (city, cancellation) => string.IsNullOrEmpty(city)
                || await newPostService.CheckIfCityIsValidAsync(city, cancellation))
                .WithMessage("The city name must be in the exact format provided by the New Post.");
        RuleFor(pud => pud.DeliveryAddress)
            .MustAsync(async (customer, address, cancellation) => address == null
                || customer.City != null && await newPostService.CheckIfAddressIsValidAsync(customer.City, address, cancellation))
                .WithMessage("There is no New Post office at the specified address in the city.");
    }
}
