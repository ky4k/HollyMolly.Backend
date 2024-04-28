using FluentValidation.TestHelper;
using HM.BLL.Models.Users;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class RegistrationRequestValidatorTests
{
    private readonly RegistrationRequestValidator _validator;
    public RegistrationRequestValidatorTests()
    {
        _validator = new RegistrationRequestValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidRegistrationRequests), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenModelIsValid(RegistrationRequest request)
    {
        TestValidationResult<RegistrationRequest> result = _validator.TestValidate(request);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidRegistrationRequests), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenModelIsInvalid(RegistrationRequest request)
    {
        TestValidationResult<RegistrationRequest> result = _validator.TestValidate(request);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
