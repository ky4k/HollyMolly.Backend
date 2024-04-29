using FluentValidation.TestHelper;
using HM.BLL.Models.Products;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class ProductInstanceCreateDtoValidatorTests
{
    private readonly ProductInstanceCreateDtoValidator _validator;
    public ProductInstanceCreateDtoValidatorTests()
    {
        _validator = new ProductInstanceCreateDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidProductInstances), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenModelIsValid(ProductInstanceCreateDto model)
    {
        TestValidationResult<ProductInstanceCreateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidProductInstances), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenModelIsInvalid(ProductInstanceCreateDto model)
    {
        TestValidationResult<ProductInstanceCreateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
