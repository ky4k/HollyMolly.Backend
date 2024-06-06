using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.DAL.Constants;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HM.BLL.Services;

public class AccountService(
    HmDbContext context,
    UserManager<User> userManager,
    JwtSecurityTokenHandler jwtSecurityTokenHandler,
    IConfigurationHelper configurationHelper,
    ILogger<AccountService> logger
    ) : IAccountService
{
    private const string UserNotExist = "User with such an id does not exist.";
    public async Task<OperationResult<RegistrationResponse>> RegisterUserAsync(RegistrationRequest request)
    {
        if (await userManager.FindByEmailAsync(request.Email) != null)
        {
            return new OperationResult<RegistrationResponse>(false, "User with such an email already exist.");
        }
        User user = new()
        {
            UserName = request.Email,
            Email = request.Email
        };
        try
        {
            await userManager.CreateAsync(user);
            await userManager.AddPasswordAsync(user, request.Password);
            await userManager.AddToRoleAsync(user, DefaultRoles.User);
            var response = new RegistrationResponse()
            {
                Id = user.Id,
                Email = user.Email!,
                Roles = await userManager.GetRolesAsync(user),
            };
            return new OperationResult<RegistrationResponse>(true, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "User has not been created.");
            return new OperationResult<RegistrationResponse>(false, "An error has happened while creating user");
        }
    }

    public async Task<OperationResult> RegisterOidcUserAsync(string email)
    {
        if (email == null)
        {
            return new OperationResult(false);
        }
        if (await userManager.FindByEmailAsync(email.ToLower()) != null)
        {
            return new OperationResult(true);
        }
        var user = new User()
        {
            IsOidcUser = true,
            UserName = email.ToLower(),
            Email = email.ToLower(),
            EmailConfirmed = true
        };
        try
        {
            await userManager.CreateAsync(user);
            await userManager.AddPasswordAsync(user, Guid.NewGuid().ToString());
            await userManager.AddToRoleAsync(user, DefaultRoles.User);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "User has not been created.");
            return new OperationResult(false, "An error has happened while creating user");
        }
    }

    public async Task<OperationResult<ConfirmationEmailDto>> GetConfirmationEmailKeyAsync(string userId)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult<ConfirmationEmailDto>(false, UserNotExist);
        }
        try
        {
            string confirmationEmailKey = await userManager.GenerateEmailConfirmationTokenAsync(user);
            ConfirmationEmailDto confirmationEmailDto = new()
            {
                UserId = user.Id,
                Token = confirmationEmailKey
            };
            return new OperationResult<ConfirmationEmailDto>(true, confirmationEmailDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating email confirmation token.");
            return new OperationResult<ConfirmationEmailDto>(false, "Confirmation email token was not created.");
        }
    }

    public async Task<OperationResult> ConfirmEmailAsync(string userId, string confirmationEmailKey)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult<ConfirmationEmailDto>(false, UserNotExist);
        }
        IdentityResult result = await userManager.ConfirmEmailAsync(user, confirmationEmailKey);
        return new OperationResult(result.Succeeded, string.Join(" ", result.Errors));
    }

    public async Task<OperationResult<LoginResponse>> LoginAsync(LoginRequest loginRequest)
    {
        User? user = await userManager.FindByEmailAsync(loginRequest.Email.ToLower());
        if (user == null)
        {
            return new OperationResult<LoginResponse>(false, "No user with such a email exists.");
        }
        if (!await userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            return new OperationResult<LoginResponse>(false, "Invalid password.");
        }
        return await GetLoginResultAsync(user);
    }

    public async Task<OperationResult<string>> GetOidcTokenAsync(string email)
    {
        User? user = await userManager.FindByEmailAsync(email.ToLower());
        if (user == null)
        {
            return new OperationResult<string>(false, "No user with such a email exists.", "");
        }
        try
        {
            string oidcToken = Guid.NewGuid().ToString();
            user.OidcToken = oidcToken;
            await userManager.UpdateAsync(user);
            return new OperationResult<string>(true, "", oidcToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating OIDC token");
            return new OperationResult<string>(false, "OIDC token was not created", null!);
        }
    }

    public async Task<OperationResult<LoginResponse>> LoginOidcUserAsync(string oidcToken)
    {
        User? user = await userManager.Users
            .FirstOrDefaultAsync(u => u.OidcToken != null && u.OidcToken == oidcToken);
        if (user == null)
        {
            return new OperationResult<LoginResponse>(false, "Login failed.");
        }
        else
        {
            user.OidcToken = null;
            await userManager.UpdateAsync(user);
            return await GetLoginResultAsync(user);
        }
    }

    private async Task<OperationResult<LoginResponse>> GetLoginResultAsync(User user)
    {
        IEnumerable<string> roles = await userManager.GetRolesAsync(user);

        string? accessToken;
        string? refreshToken;
        try
        {
            accessToken = jwtSecurityTokenHandler.WriteToken(GetAccessToken(user, roles));
            refreshToken = jwtSecurityTokenHandler.WriteToken(GetRefreshToken(user.UserName!));
            await context.Tokens.AddAsync(new TokenRecord()
            {
                UserName = user.UserName,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while setting the JWT token.");
            return new OperationResult<LoginResponse>(false, "Cannot create JWT token.");
        }
        LoginResponse response = new()
        {
            UserId = user.Id,
            UserEmail = user.Email,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Roles = roles.ToList()
        };
        return new OperationResult<LoginResponse>(true, response);
    }

    private JwtSecurityToken GetAccessToken(User user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new("IssuedAt", DateTimeOffset.UtcNow.Ticks.ToString())
        };
        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        double expiresIn = double.TryParse(configurationHelper.GetConfigurationValue(
            "JwtSettings:ExpirationTimeInMinutes"), out double exp) ? exp : 60.0;

        byte[] key = Encoding.UTF8.GetBytes(configurationHelper.GetConfigurationValue(
            "JwtSettings:SecurityKey") ?? "defaultKey_that_is_32_characters");
        var secret = new SymmetricSecurityKey(key);

        return new JwtSecurityToken(
            issuer: configurationHelper.GetConfigurationValue("JwtSettings:Issuer") ?? "HollyMolly",
            audience: configurationHelper.GetConfigurationValue("JwtSettings:Audience") ?? "*",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: new SigningCredentials(secret, SecurityAlgorithms.HmacSha256));
    }
    private JwtSecurityToken GetRefreshToken(string userName)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, userName)
        };

        double expiresIn = double.TryParse(configurationHelper.GetConfigurationValue(
            "JwtSettings:RefreshTokenValidForMinutes"), out double exp) ? exp : 120.0;

        byte[] key = Encoding.UTF8.GetBytes(configurationHelper.GetConfigurationValue(
            "JwtSettings:SecurityKey") ?? "defaultKey_that_is_32_characters");
        var secret = new SymmetricSecurityKey(key);

        return new JwtSecurityToken(
            issuer: configurationHelper.GetConfigurationValue("JwtSettings:Issuer") ?? "HollyMolly",
            audience: configurationHelper.GetConfigurationValue("JwtSettings:Audience") ?? "*",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: new SigningCredentials(secret, SecurityAlgorithms.HmacSha256));
    }

    public async Task<OperationResult<LoginResponse>> RefreshTokenAsync(TokensDto tokens)
    {
        TokenRecord? oldToken = await context.Tokens
            .FirstOrDefaultAsync(t => t.AccessToken == tokens.AccessToken
                && t.RefreshToken == tokens.RefreshToken);
        if (oldToken == null)
        {
            return new OperationResult<LoginResponse>(false, "Invalid refresh token");
        }
        try
        {
            JwtSecurityToken oldRefreshToken = jwtSecurityTokenHandler.ReadJwtToken(tokens.RefreshToken);
            if (oldRefreshToken.ValidTo < DateTimeOffset.UtcNow)
            {
                context.Tokens.Remove(oldToken);
                await context.SaveChangesAsync();
                return new OperationResult<LoginResponse>(false, "Refresh token has expired");
            }
            string? userName = oldRefreshToken.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value;
            User? user = await userManager.FindByNameAsync(userName!);
            if (user == null)
            {
                return new OperationResult<LoginResponse>(false, UserNotExist);
            }
            context.Tokens.Remove(oldToken);
            await context.SaveChangesAsync();
            return await GetLoginResultAsync(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while refreshing token.");
            return new OperationResult<LoginResponse>(false, "Token was not refreshed.");
        }
    }
    public async Task<OperationResult> InvalidateAllPreviousTokensAsync(string userId)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult(false, UserNotExist);
        }
        user.InvalidateTokenBefore = DateTimeOffset.UtcNow.Ticks;
        await userManager.UpdateAsync(user);
        return new OperationResult(true);
    }
    public async Task<UserDto?> GetUserInfoAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Profiles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return user?.ToUserDto();
    }
    public async Task<OperationResult<ProfileDto>> CreateProfileAsync(
        string userId, ProfileUpdateDto updatedProfile, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .Include(u => u.Profiles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return new OperationResult<ProfileDto>(false, UserNotExist);
        }
        Profile profile = new();
        UpdateProfileProperties(profile, updatedProfile);
        try
        {
            user.Profiles.Add(profile);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult<ProfileDto>(true, profile.ToProfileDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Profile has not been created: {@Profile}", profile);
            return new OperationResult<ProfileDto>(false, "Profile has not been created.");
        }
    }

    public async Task<OperationResult<ProfileDto>> UpdateProfileAsync(
        string userId, int profileId, ProfileUpdateDto updatedProfile, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .Include(u => u.Profiles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return new OperationResult<ProfileDto>(false, UserNotExist);
        }
        Profile? profile = user.Profiles.Find(p => p.Id == profileId);
        if (profile == null)
        {
            return new OperationResult<ProfileDto>(false, $"User with id {userId} does not contain profile with id {profileId}");
        }
        try
        {
            UpdateProfileProperties(profile, updatedProfile);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult<ProfileDto>(true, profile.ToProfileDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Profile has not been updated: {@Profile}", profile);
            return new OperationResult<ProfileDto>(false, "An error occurred while updating user profile.");
        }
    }
    private static void UpdateProfileProperties(Profile profile, ProfileUpdateDto updatedProfile)
    {
        profile.FirstName = updatedProfile.FirstName;
        profile.LastName = updatedProfile.LastName;
        profile.PhoneNumber = updatedProfile.PhoneNumber;
        profile.DateOfBirth = updatedProfile.DateOfBirth;
        profile.City = updatedProfile.City;
        profile.DeliveryAddress = updatedProfile.DeliveryAddress;
    }
    public async Task<OperationResult> DeleteProfileAsync(string userId, int profileId, CancellationToken cancellationToken)
    {
        Profile? profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == profileId, cancellationToken);
        if (profile == null)
        {
            return new OperationResult(false, "Profile does not exist.");
        }
        if (profile.UserId != userId)
        {
            return new OperationResult(false, "Profile does not belong to the user.");
        }
        try
        {
            context.Profiles.Remove(profile);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Profile was not deleted {@Profile}", profile);
            return new OperationResult(false, "Profile has not been deleted.");
        }
    }
    public async Task<OperationResult> UpdateEmailAsync(string userId, string newEmail)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult(false, UserNotExist);
        }
        if (user.IsOidcUser)
        {
            return new OperationResult(false, "You are authorized via Google. Register account on our website.");
        }
        if (await userManager.FindByEmailAsync(newEmail.ToLower()) != null)
        {
            return new OperationResult(false, $"User with the email {newEmail} already exist.");
        }
        user.Email = newEmail.ToLower();
        user.UserName = newEmail.ToLower();
        user.EmailConfirmed = false;
        user.InvalidateTokenBefore = DateTimeOffset.UtcNow.Ticks;
        await userManager.UpdateAsync(user);
        return new OperationResult(true);
    }

    public async Task<OperationResult<ResetPasswordTokenDto>> ChangePasswordAsync(string userId, ChangePasswordDto passwords)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult<ResetPasswordTokenDto>(false, UserNotExist);
        }
        IdentityResult result = await userManager.ChangePasswordAsync(
            user, passwords.OldPassword, passwords.NewPassword);
        if (result.Succeeded)
        {
            user.InvalidateTokenBefore = DateTimeOffset.UtcNow.Ticks;
            await userManager.UpdateAsync(user);
            OperationResult<ResetPasswordTokenDto> createPasswordResult = await CreatePasswordResetKeyAsync(user);
            return new OperationResult<ResetPasswordTokenDto>(true, createPasswordResult.Payload!);
        }
        else
        {
            string errors = string.Join(' ', result.Errors.Select(e => e.Description));
            return new OperationResult<ResetPasswordTokenDto>(false, $"Password has not been changed: {errors}");
        }
    }

    public async Task<OperationResult<ResetPasswordTokenDto>> CreatePasswordResetKeyAsync(string email)
    {
        User? user = await userManager.FindByEmailAsync(email.ToLower());
        if (user == null)
        {
            return new OperationResult<ResetPasswordTokenDto>(false, $"User with the email {email} does not exist");
        }
        return await CreatePasswordResetKeyAsync(user);
    }

    private async Task<OperationResult<ResetPasswordTokenDto>> CreatePasswordResetKeyAsync(User user)
    {
        try
        {
            string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            ResetPasswordTokenDto resetPasswordKeyDto = new()
            {
                UserId = user.Id,
                Token = resetToken
            };
            return new OperationResult<ResetPasswordTokenDto>(true, resetPasswordKeyDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred while creating password reset token for user " +
                "with Id {UserId}", user.Id);
            return new OperationResult<ResetPasswordTokenDto>(false, "Reset token was not created.");
        }
    }

    public async Task<OperationResult<UserDto>> ResetPasswordAsync(string userId, ResetPasswordDto resetPassword)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult<UserDto>(false, UserNotExist);
        }
        if (!await userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.PasswordResetTokenProvider,
            UserManager<User>.ResetPasswordTokenPurpose, resetPassword.ResetToken))
        {
            return new OperationResult<UserDto>(false, "Reset token is incorrect.");
        }
        try
        {
            await userManager.RemovePasswordAsync(user);
            await userManager.AddPasswordAsync(user, resetPassword.NewPassword);
            user.IsOidcUser = false;
            user.InvalidateTokenBefore = DateTimeOffset.UtcNow.Ticks;
            await userManager.UpdateAsync(user);
            return new OperationResult<UserDto>(true, user.ToUserDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while resetting the password.");
            return new OperationResult<UserDto>(false, "Password has not been reset.");
        }
    }
}
