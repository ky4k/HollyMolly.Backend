using HM.BLL.Models.Common;
using HM.BLL.Models.Users;

namespace HM.BLL.Interfaces;

public interface IAccountService
{
    Task<OperationResult<RegistrationResponse>> RegisterUserAsync(RegistrationRequest request);
    Task<OperationResult> RegisterOidcUserAsync(string email);
    Task<OperationResult<string>> GetOidcTokenAsync(string email);
    Task<OperationResult<LoginResponse>> LoginOidcUserAsync(string oidcToken);
    Task<OperationResult<ConfirmationEmailDto>> GetConfirmationEmailKeyAsync(string userId);
    Task<OperationResult> ConfirmEmailAsync(string userId, string confirmationEmailKey);
    Task<OperationResult<LoginResponse>> LoginAsync(LoginRequest loginRequest);
    Task<OperationResult> InvalidateAllPreviousTokensAsync(string userId);
    Task<OperationResult<UserDto>> UpdateUserProfileAsync(string userId, ProfileUpdateDto profile);
    Task<OperationResult> UpdateEmailAsync(string userId, string newEmail);
    Task<OperationResult<ResetPasswordTokenDto>> ChangePasswordAsync(string userId, ChangePasswordDto passwords);
    Task<OperationResult<ResetPasswordTokenDto>> CreatePasswordResetKeyAsync(string email);
    Task<OperationResult<UserDto>> ResetPasswordAsync(string userId, ResetPasswordDto resetPassword);
}
