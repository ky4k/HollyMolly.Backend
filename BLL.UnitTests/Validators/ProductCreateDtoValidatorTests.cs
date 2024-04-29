using FluentValidation.TestHelper;
using HM.BLL.Models.Products;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class ProductCreateDtoValidatorTests
{
    private readonly ProductCreateDtoValidator _validator;
    public ProductCreateDtoValidatorTests()
    {
        _validator = new ProductCreateDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidProductsCreate), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenModelIsValid(ProductCreateDto model)
    {
        TestValidationResult<ProductCreateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidProductsCreate), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenModelIsInvalid(ProductCreateDto model)
    {
        TestValidationResult<ProductCreateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
