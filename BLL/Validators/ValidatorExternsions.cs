using FluentValidation;
using System.Text.RegularExpressions;

namespace HM.BLL.Validators;

public static partial class ValidatorExtensions
{
    private const string NameRegex = @"^(?:[АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЮЯ]){1}[абвгґдеєжзиіїйклмнопрстуфхцчшщьюя'`’]*(?:(?:-[АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЮЯ]){1}[абвгґдеєжзиіїйклмнопрстуфхцчшщьюя'`’]*)*$";

    [GeneratedRegex("[-'`’]{2}")]
    private static partial Regex InvalidCombinationSymbolsInName();

    private const int MinEmailRecipientNameLength = 1;
    private const int MaxEmailRecipientNameLength = 50;
    private const int MinEmailDomainNameLength = 4;
    private const int MaxEmailDomainNameLength = 50;

    private static readonly string incorrectEmailLengthMessage = "Incorrect length of the email part(s): ensure that" +
        $" the recipient name is between {MinEmailRecipientNameLength} and {MaxEmailRecipientNameLength} characters long" +
        $" and the domain name is between {MinEmailDomainNameLength} and {MaxEmailDomainNameLength} characters long.";


    private const string InvalidNameMessage = "Name may contain only letters " +
        "АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЮЯабвгґдеєжзиіїйклмнопрстуфхцчшщьюя, -, or apostrophe. " +
        "Every part of the name must start with a capital letter but no additional capital letters are allowed.";

    public static IRuleBuilderOptions<T, string?> ApplyEmailValidationRules<T>(this IRuleBuilder<T, string?> email)
    {
        return email.
            NotEmpty()
                .WithMessage("Email is required.")
            .Matches(@"^[A-Za-z0-9-_.+]*@[A-Za-z0-9]{1}[A-Za-z0-9-.]*\.[A-Za-z0-9]{2,}$")
                .WithMessage("Email is in an invalid format.")
            .Must(ValidateEmailPartsLength)
                .WithMessage(incorrectEmailLengthMessage);
    }

    private static bool ValidateEmailPartsLength(string email)
    {
        var parts = email.Split('@');
        if (parts.Length == 2)
        {
            bool isRecipientNameOfValidLength = parts[0].Length >= MinEmailRecipientNameLength
                && parts[0].Length <= MaxEmailRecipientNameLength;
            bool isDomainNameOfValidLength = parts[1].Length >= MinEmailDomainNameLength
                && parts[1].Length <= MaxEmailDomainNameLength;
            return isDomainNameOfValidLength && isRecipientNameOfValidLength;
        }
        else
        {
            return false;
        }
    }

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

    public static IRuleBuilderOptions<T, string?> ApplyNameValidationRules<T>(this IRuleBuilder<T, string?> name)
    {
        return name
            .MaximumLength(50)
                .WithMessage("First or last name may not be longer than 50 characters.")
            .Matches(NameRegex)
                .WithMessage(InvalidNameMessage)
            .Must(n => !InvalidCombinationSymbolsInName().IsMatch(n))
                .WithMessage("Firs or last name may not contain two non-letter in a row.");
    }

    public static IRuleBuilderOptions<T, string?> ApplyPhoneNumberValidationRules<T>(this IRuleBuilder<T, string?> phoneNumber)
    {
        return phoneNumber
            .Matches(@"^\+[0-9]{12}$|^[0-9]{10}$")
                .WithMessage("Phone number may be either exactly 10 digits long or start with the plus (+) sign followed by 12 digits");
    }

    public static IRuleBuilderOptions<T, string?> ApplyCategoryValidationRules<T>(this IRuleBuilder<T, string?> category)
    {
        return category
            .NotEmpty()
                .WithMessage("Category name is required.")
            .MinimumLength(3)
                .WithMessage("Category name must be at least 3 characters long.")
            .MaximumLength(20)
                .WithMessage("Category name cannot be longer than 20 characters.")
            .Matches(@"^[\p{L}0-9\s]+$")
                .WithMessage("Category must be 3-20 characters long and contain only Latin " +
                    "or Ukrainian letters, numbers, and spaces.");
    }
}
