using FluentValidation.TestHelper;
using HM.BLL.Models.Categories;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class CategoryCreateDtoValidatorTests
{
    private readonly CategoryCreateDtoValidator _validator;
    public CategoryCreateDtoValidatorTests()
    {
        _validator = new CategoryCreateDtoValidator();
    }

    [Theory]
    [MemberData(nameof(ValidationData.ValidCategories), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenCategoryIsValid(CategoryCreateDto category)
    {
        TestValidationResult<CategoryCreateDto> result = _validator.TestValidate(category);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidCategories), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenCategoryIsInvalid(CategoryCreateDto category)
    {
        TestValidationResult<CategoryCreateDto> result = _validator.TestValidate(category);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
