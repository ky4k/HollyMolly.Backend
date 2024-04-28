using FluentValidation.TestHelper;
using HM.BLL.Models.Orders;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class CustomerDtoValidatorTests
{
    private readonly CustomerDtoValidator _validator;
    public CustomerDtoValidatorTests()
    {
        _validator = new CustomerDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidCustomers), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenAllFieldsAreValid(CustomerDto customer)
    {
        TestValidationResult<CustomerDto> result = _validator.TestValidate(customer);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidCustomers), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenAnyFieldIsInvalid(CustomerDto customer)
    {
        TestValidationResult<CustomerDto> result = _validator.TestValidate(customer);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
