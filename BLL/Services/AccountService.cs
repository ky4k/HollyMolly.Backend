using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.DAL.Constants;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HM.BLL.Services;

public class AccountService(
    UserManager<User> userManager,
    IConfiguration configuration,
    ILogger<AccountService> logger
    ) : IAccountService
{
    public async Task<OperationResult<RegistrationResponse>> RegisterUserAsync(RegistrationRequest request)
    {
        if (await userManager.FindByEmailAsync(request.Email.ToLower()) != null)
        {
            return new OperationResult<RegistrationResponse>(false, "A user with such an email already exist.");
        }
        User user = new()
        {
            UserName = request.Email.ToLower(),
            Email = request.Email.ToLower()
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
            return new OperationResult<ConfirmationEmailDto>(false, "User with such an id does not exist.");
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
            return new OperationResult<ConfirmationEmailDto>(false, "User with such an id does not exist.");
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
        User? user = userManager.Users
            .FirstOrDefault(u => u.OidcToken != null && u.OidcToken == oidcToken);
        if (user == null)
        {
            return new OperationResult<LoginResponse>(false, "Login failed.");
        }
        else
        {
            user.OidcToken = null;
            return await GetLoginResultAsync(user);
        }
    }

    private async Task<OperationResult<LoginResponse>> GetLoginResultAsync(User user)
    {
        IEnumerable<string> roles = await userManager.GetRolesAsync(user);

        string? token;
        try
        {
            token = new JwtSecurityTokenHandler().WriteToken(GetToken(user, roles));
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
            AccessToken = token,
            Roles = roles
        };

        return new OperationResult<LoginResponse>(true, response);
    }

    private JwtSecurityToken GetToken(User user, IEnumerable<string> roles)
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

        double expiresIn = double.TryParse(
            configuration?["JwtSettings:ExpirationTimeInMinutes"], out double exp) ? exp : 60.0;

        byte[] key = Encoding.UTF8.GetBytes(
            Environment.GetEnvironmentVariable("JwtSettings:SecurityKey")
            ?? configuration?["JwtSettings:SecurityKey"]
            ?? "defaultKey_that_is_32_characters");
        var secret = new SymmetricSecurityKey(key);

        return new JwtSecurityToken(
            issuer: configuration?["JwtSettings:Issuer"] ?? "HollyMolly",
            audience: configuration?["JwtSettings:Audience"] ?? "*",
            claims: claims,
            expires: DateTime.Now.AddMinutes(expiresIn),
            signingCredentials: new SigningCredentials(secret, SecurityAlgorithms.HmacSha256));
    }

    public async Task<OperationResult> InvalidateAllPreviousTokensAsync(string userId)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult(false, "User with such an id does not exist.");
        }
        user.InvalidateTokenBefore = DateTimeOffset.UtcNow.Ticks;
        await userManager.UpdateAsync(user);
        return new OperationResult(true);
    }

    public async Task<OperationResult<UserDto>> UpdateUserProfileAsync(string userId, ProfileUpdateDto profile)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult<UserDto>(false, "User with such an id does not exist.");
        }

        user.FirstName = profile.FirstName;
        user.LastName = profile.LastName;
        user.PhoneNumber = profile.PhoneNumber;
        user.DateOfBirth = profile.DateOfBirth;
        user.City = profile.City;
        user.DeliveryAddress = profile.DeliveryAddress;

        IdentityResult result = await userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            IEnumerable<string> roles = await userManager.GetRolesAsync(user);
            return new OperationResult<UserDto>(true, user.ToUserDto(roles));
        }
        else
        {
            logger.LogError("User has not been updated: {errors}",
                string.Join(' ', result.Errors.Select(e => e.Description)));
            return new OperationResult<UserDto>(false, "An error occurred while updating user");
        }
    }

    public async Task<OperationResult> UpdateEmailAsync(string userId, string newEmail)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult(false, "No user with such an id exist.");
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
            return new OperationResult<ResetPasswordTokenDto>(false, "User with such an id does not exist.");
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
                "with Id {userId}", user.Id);
            return new OperationResult<ResetPasswordTokenDto>(false, "Reset token was not created.");
        }
    }

    public async Task<OperationResult<UserDto>> ResetPasswordAsync(string userId, ResetPasswordDto resetPassword)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult<UserDto>(false, "No user with such an id exist.");
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
