using FluentValidation.TestHelper;
using HM.BLL.Models.Users;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;
    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }
    [Fact]
    public void Validation_ShouldSucceed_WhenAllFieldsAreProvided()
    {
        LoginRequest request = new()
        {
            Email = "test@example.com",
            Password = "newPassword"
        };

        var result = _validator.TestValidate(request);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Fact]
    public void Validation_ShouldFail_WhenEmailIsEmpty()
    {
        LoginRequest request = new()
        {
            Email = "",
            Password = "newPassword"
        };

        var result = _validator.TestValidate(request);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
    [Fact]
    public void Validation_ShouldFail_WhenPasswordIsEmpty()
    {
        LoginRequest request = new()
        {
            Email = "test@example.com",
            Password = ""
        };

        var result = _validator.TestValidate(request);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
