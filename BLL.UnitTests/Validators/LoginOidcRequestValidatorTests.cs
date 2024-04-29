using FluentValidation.TestHelper;
using HM.BLL.Models.Users;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class LoginOidcRequestValidatorTests
{
    private readonly LoginOidcRequestValidator _validator;
    public LoginOidcRequestValidatorTests()
    {
        _validator = new LoginOidcRequestValidator();
    }
    [Fact]
    public void Validation_ShouldSucceed_WhenModelIsValid()
    {
        LoginOidcRequest request = new() { Token = "ValidToken" };
        TestValidationResult<LoginOidcRequest> result = _validator.TestValidate(request);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Fact]
    public void Validation_ShouldFail_WhenTokenIsEmpty()
    {
        LoginOidcRequest request = new() { Token = "" };
        TestValidationResult<LoginOidcRequest> result = _validator.TestValidate(request);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
