using FluentValidation;
using HM.BLL.Models.Categories;

namespace HM.BLL.Validators;

public class CategoryGroupCreateDtoValidator : AbstractValidator<CategoryGroupCreateDto>
{
    public CategoryGroupCreateDtoValidator()
    {
        RuleFor(cg => cg.Name)
            .ApplyCategoryValidationRules();
    }
}
