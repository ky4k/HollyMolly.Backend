using FluentValidation.TestHelper;
using HM.BLL.Models.Products;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class ProductFeedbackCreateDtoValidatorTests
{
    private readonly ProductFeedbackCreateDtoValidator _validator;
    public ProductFeedbackCreateDtoValidatorTests()
    {
        _validator = new ProductFeedbackCreateDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidProductFeedbacks), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenModelIsValid(ProductFeedbackCreateDto model)
    {
        TestValidationResult<ProductFeedbackCreateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidProductFeedbacks), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenModelIsInvalid(ProductFeedbackCreateDto model)
    {
        TestValidationResult<ProductFeedbackCreateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
