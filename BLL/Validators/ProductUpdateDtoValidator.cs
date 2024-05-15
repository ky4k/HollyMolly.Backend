using FluentValidation;
using HM.BLL.Models.Products;

namespace HM.BLL.Validators;

public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
{
    public ProductUpdateDtoValidator()
    {
        RuleFor(product => product.Name)
            .NotEmpty()
            .Length(5, 50)
            .Matches(@"^[\p{L}0-9\s()\-'`’]+$")
                .WithMessage("Product Name must be 5-50 characters long and contain only Latin " +
                    "or Ukrainian letters, numbers, and spaces.");

        RuleFor(product => product.Description)
            .MaximumLength(500)
            .Matches(@"^[\p{L}0-9\s!#$%'&""/?,.\-_]+$")
            .When(product => !string.IsNullOrEmpty(product.Description))
                .WithMessage("Product Description may only contain Latin or Ukrainian letters, " +
                    "numbers, and special characters (!#$%&\"/?.,-_).");
    }
}
