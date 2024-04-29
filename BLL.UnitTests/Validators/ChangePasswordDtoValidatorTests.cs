using FluentValidation.TestHelper;
using HM.BLL.Models.Users;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;


namespace HM.BLL.UnitTests.Validators;

public class ChangePasswordDtoValidatorTests
{
    private readonly ChangePasswordDtoValidator _validator;
    public ChangePasswordDtoValidatorTests()
    {
        _validator = new ChangePasswordDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidChangePasswordModels), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenNewPasswordIsValid(ChangePasswordDto changePasswordDto)
    {
        TestValidationResult<ChangePasswordDto> result = _validator.TestValidate(changePasswordDto);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidChangePasswordModels), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenNewPasswordIsInvalid(ChangePasswordDto changePasswordDto)
    {
        TestValidationResult<ChangePasswordDto> result = _validator.TestValidate(changePasswordDto);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validation_ShouldFail_WhenOldPasswordIsEmpty(string? oldPassword)
    {
        ChangePasswordDto changePasswordDto = new()
        {
            OldPassword = oldPassword!,
            NewPassword = "new123$%^password"
        };
        TestValidationResult<ChangePasswordDto> result = _validator.TestValidate(changePasswordDto);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
