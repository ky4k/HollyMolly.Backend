using FluentValidation.TestHelper;
using HM.BLL.Models.Products;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class ProductUpdateDtoValidatorTests
{
    private readonly ProductUpdateDtoValidator _validator;
    public ProductUpdateDtoValidatorTests()
    {
        _validator = new ProductUpdateDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidProductsUpdate), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenModelIsValid(ProductUpdateDto model)
    {
        TestValidationResult<ProductUpdateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidProductsUpdate), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenModelIsInvalid(ProductUpdateDto model)
    {
        TestValidationResult<ProductUpdateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
