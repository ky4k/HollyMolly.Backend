using FluentValidation.TestHelper;
using HM.BLL.Models.Categories;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class CategoryUpdateDtoValidatorTests
{
    private readonly CategoryUpdateDtoValidator _validator;
    public CategoryUpdateDtoValidatorTests()
    {
        _validator = new CategoryUpdateDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidCategoryUpdates), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenCategoryIsValid(CategoryUpdateDto category)
    {
        TestValidationResult<CategoryUpdateDto> result = _validator.TestValidate(category);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidCategoryUpdates), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenCategoryIsInvalid(CategoryUpdateDto category)
    {
        TestValidationResult<CategoryUpdateDto> result = _validator.TestValidate(category);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
