using FluentValidation.TestHelper;
using HM.BLL.Models.Users;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class EmailDtoValidatorTests
{
    private readonly EmailDtoValidator _validator;
    public EmailDtoValidatorTests()
    {
        _validator = new EmailDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidEmailUpdates), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenAllFieldsAreValid(EmailDto emailUpdateDto)
    {
        TestValidationResult<EmailDto> result = _validator.TestValidate(emailUpdateDto);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidEmailUpdates), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenAnyFieldIsInvalid(EmailDto emailUpdateDto)
    {
        TestValidationResult<EmailDto> result = _validator.TestValidate(emailUpdateDto);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
