using HM.BLL.Models;

namespace HM.BLL.Interfaces;

public interface IAccountService
{
    Task<OperationResult<RegistrationResponse>> RegisterUserAsync(RegistrationRequest request);
    Task<OperationResult<ConfirmationEmailDto>> GetConfirmationEmailKey(string userId);
    Task<OperationResult> ConfirmEmailAsync(string userId, string confirmationEmailKey);
    Task<OperationResult<LoginResponse>> LoginAsync(LoginRequest loginRequest);
    Task<OperationResult<UserDto>> UpdateUserProfileAsync(string userId, ProfileUpdateDto profile);
    Task<OperationResult<ResetPasswordTokenDto>> ChangePasswordAsync(string userId, ChangePasswordDto passwords);
    Task<OperationResult> RegisterOidcUserAsync(string email);
    Task<OperationResult<string>> GetOidcTokenAsync(string email);
    Task<OperationResult<LoginResponse>> LoginOidcUserAsync(string oidcToken);
    Task<OperationResult<ResetPasswordTokenDto>> CreatePasswordResetKeyAsync(string email);
    Task<OperationResult<UserDto>> ResetPasswordAsync(string userId, ResetPasswordDto resetPassword);
}
