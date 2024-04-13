using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class ProductCreateUpdateDtoValidator : AbstractValidator<ProductCreateUpdateDto>
{
    public ProductCreateUpdateDtoValidator()
    {
        // TO DO: Write validators for all the properties except Images that will be validated in the service.

    }
}
