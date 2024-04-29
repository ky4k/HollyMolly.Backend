using FluentValidation.TestHelper;
using HM.BLL.Models.Orders;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class OrderCreateDtoValidatorTests
{
    private readonly OrderCreateDtoValidator _validator;
    public OrderCreateDtoValidatorTests()
    {
        _validator = new OrderCreateDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidOrdersCreate), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenModelIsValid(OrderCreateDto model)
    {
        TestValidationResult<OrderCreateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidOrdersCreate), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenModelIsInvalid(OrderCreateDto model)
    {
        TestValidationResult<OrderCreateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
