using FluentValidation.TestHelper;
using HM.BLL.Models.Categories;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class CategoryGroupCreateDtoValidatorTests
{
    private readonly CategoryGroupCreateDtoValidator _validator;
    public CategoryGroupCreateDtoValidatorTests()
    {
        _validator = new CategoryGroupCreateDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidCategoryGroups), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenCategoryIsValid(CategoryGroupCreateDto categoryGroup)
    {
        TestValidationResult<CategoryGroupCreateDto> result = _validator.TestValidate(categoryGroup);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidCategoryGroups), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenCategoryIsInvalid(CategoryGroupCreateDto categoryGroup)
    {
        TestValidationResult<CategoryGroupCreateDto> result = _validator.TestValidate(categoryGroup);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
