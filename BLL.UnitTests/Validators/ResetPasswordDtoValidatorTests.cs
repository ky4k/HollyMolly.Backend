using FluentValidation.TestHelper;
using HM.BLL.Models.Users;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class ResetPasswordDtoValidatorTests
{
    private readonly ResetPasswordDtoValidator _validator;
    public ResetPasswordDtoValidatorTests()
    {
        _validator = new ResetPasswordDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidResetPasswordModels), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenModelIsValid(ResetPasswordDto model)
    {
        TestValidationResult<ResetPasswordDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidResetPasswordModels), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenModelIsInvalid(ResetPasswordDto model)
    {
        TestValidationResult<ResetPasswordDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validation_ShouldFail_WhenResetTokenIsNotProvided(string? resetToken)
    {
        ResetPasswordDto model = new()
        {
            ResetToken = resetToken!,
            NewPassword = "ValidPassword"
        };

        TestValidationResult<ResetPasswordDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
