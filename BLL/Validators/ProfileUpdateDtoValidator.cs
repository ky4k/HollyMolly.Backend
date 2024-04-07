using FluentValidation;
using HM.BLL.Models;
using System.Text.RegularExpressions;

namespace HM.BLL.Validators;

public partial class ProfileUpdateDtoValidator : AbstractValidator<ProfileUpdateDto>
{
    private const string NameRegex = @"^(?:[АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЮЯ]){1}[абвгґдеєжзиіїйклмнопрстуфхцчшщьюя'`’]*(?:(?:-[АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЮЯ]){1}[абвгґдеєжзиіїйклмнопрстуфхцчшщьюя'`’]*)*$";

    private const string InvalidNameMessage = "Name may contain only letters" +
        "АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЮЯабвгґдеєжзиіїйклмнопрстуфхцчшщьюя, -, or apostrophe. " +
        "Every part of the name must start with a capital letter but no additional capital letters are allowed.";
    public ProfileUpdateDtoValidator()
    {
        RuleFor(pud => pud.FirstName)
            .MaximumLength(50)
                .WithMessage("First name may not be longer than 50 characters.")
            .Matches(NameRegex)
                .WithMessage(InvalidNameMessage)
            .Must(n => !InvalidCombinationSymbolsInName().IsMatch(n))
                .WithMessage("Firs name may not contain two non-letter in a row.");
        RuleFor(pud => pud.LastName)
            .MaximumLength(50)
                .WithMessage("Last name may not be longer than 50 characters.")
            .Matches(NameRegex)
                .WithMessage(InvalidNameMessage)
            .Must(n => !InvalidCombinationSymbolsInName().IsMatch(n))
                .WithMessage("Last name may not contain two non-letter in a row.");
        RuleFor(pud => pud.PhoneNumber)
            .Matches(@"^\+[0-9]{12}$|^[0-9]{10}$")
                .WithMessage("Phone number may be either exactly 10 digits long or start with the plus (+) sign followed by 12 digits");
        RuleFor(pud => pud.DateOfBirth)
            .Must(db => db == null || db < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past");
        RuleFor(pud => pud.City)
            .MinimumLength(1);
        RuleFor(pud => pud.DeliveryAddress)
            .MinimumLength(1);
    }

    [GeneratedRegex("[-'`’]{2}")]
    private static partial Regex InvalidCombinationSymbolsInName();
}
