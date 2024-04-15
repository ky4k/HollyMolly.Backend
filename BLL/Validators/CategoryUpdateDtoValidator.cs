using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class CategoryUpdateDtoValidator : AbstractValidator<CategoryUpdateDto>
{
    public CategoryUpdateDtoValidator()
    {
        RuleFor(cud => cud.CategoryName)
            .ApplyCategoryValidationRules();
    }
}
