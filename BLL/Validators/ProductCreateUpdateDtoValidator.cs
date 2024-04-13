using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class ProductCreateUpdateDtoValidator : AbstractValidator<ProductCreateUpdateDto>
{
    public ProductCreateUpdateDtoValidator()
    {
        RuleFor(product => product.Name)
            .NotEmpty()
            .Length(5, 50)
            .Matches(@"^[\p{L}0-9\s]+$")
            .WithMessage("Product Name must be 5-50 characters long and contain only Latin " +
            "or Ukrainian letters, numbers, and spaces.");

        RuleFor(product => product.Description)
            .MaximumLength(500)
            .Matches(@"^[\p{L}0-9\s!#$%&""/?,.\-_]+$")
            .When(product => !string.IsNullOrEmpty(product.Description))
            .WithMessage("Product Description may only contain Latin or Ukrainian letters, " +
            "numbers, and special characters (!#$%&\"/?.,-_).");

        RuleFor(product => product.Price)
            .GreaterThan(0)
            .WithMessage("Product price must be greater than 0.");

        RuleFor(product => product.Category)
            .NotEmpty()
            .Length(3, 20)
            .Matches(@"^[\p{L}0-9\s]+$")
            .WithMessage("Category must be 3-20 characters long and contain only Latin " +
            "or Ukrainian letters, numbers, and spaces.");

        RuleFor(product => product.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock Quantity must be greater than or equal to 0.");
    }
}
