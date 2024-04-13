using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models;
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
        var existedUser = await userManager.FindByEmailAsync(request.Email.ToLower());
        if (existedUser != null && !existedUser.IsOidcUser)
        {
            return new OperationResult<RegistrationResponse>(false, "A user with such an email already exist.");
        }
        try
        {
            var user = existedUser ?? new User()
            {
                UserName = request.Email.ToLower(),
                Email = request.Email.ToLower()
            };

            if (existedUser != null && existedUser.IsOidcUser)
            {
                await userManager.RemovePasswordAsync(existedUser);
                await userManager.AddPasswordAsync(existedUser, request.Password);
                existedUser.IsOidcUser = false;
                await userManager.UpdateAsync(existedUser);
            }
            else
            {
                await userManager.CreateAsync(user);
                await userManager.AddPasswordAsync(user, request.Password);
                await userManager.AddToRoleAsync(user, DefaultRoles.User);
            }
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

    public async Task<OperationResult<LoginResponse>> LoginAsync(LoginRequest loginRequest)
    {
        User? user = await userManager.FindByEmailAsync(loginRequest.Email.ToLower());
        if (user == null)
        {
            return new OperationResult<LoginResponse>(false, "No user with such a email exists.");
        }
        if(!await userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            return new OperationResult<LoginResponse>(false, "Invalid password.");
        }
        return await GetLoginResultAsync(user);
    }

    public async Task<OperationResult<LoginResponse>> LoginOidcUserAsync(string email)
    {
        if (email == null)
        {
            return new OperationResult<LoginResponse>(false, "Login has failed.");
        }

        User? user = await userManager.FindByEmailAsync(email.ToLower());
        if (user == null)
        {
            return new OperationResult<LoginResponse>(false, "No user with such a email exists.");
        }
        return await GetLoginResultAsync(user);
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
            new(ClaimTypes.NameIdentifier, user?.Id ?? string.Empty),
            new(ClaimTypes.Name, user?.UserName ?? string.Empty),
            new(ClaimTypes.Email, user?.Email ?? string.Empty)
        };
        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        double expiresIn = double.TryParse(
            configuration?["JwtSettings:ExpirationTimeInMinutes"], out double exp) ? exp : 60.0;

        var key = Encoding.UTF8.GetBytes(configuration?["JwtSettings:SecurityKey"] ?? "defaultKey_that_is_32_characters");
        var secret = new SymmetricSecurityKey(key);

        return new JwtSecurityToken(
            issuer: configuration?["JwtSettings:Issuer"] ?? "HollyMolly",
            audience: configuration?["JwtSettings:Audience"] ?? "*",
            claims: claims,
            expires: DateTime.Now.AddMinutes(expiresIn),
            signingCredentials: new SigningCredentials(secret, SecurityAlgorithms.HmacSha256));
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

        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            IEnumerable<string> roles = await userManager.GetRolesAsync(user);
            UserDto userDto = user.ToUserDto(roles);
            return new OperationResult<UserDto>(true, userDto);
        }
        else
        {
            logger.LogError("User has not been updated: {errors}",
                string.Join(' ', result.Errors.Select(e => e.Description)));
            return new OperationResult<UserDto>(false, "An error occurred while updating user");
        }
    }

    public async Task<OperationResult> ChangePasswordAsync(string userId, ChangePasswordDto passwords)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult(false, "User with such an id does not exist.");
        }
        var result = await userManager.ChangePasswordAsync(
            user, passwords.OldPassword, passwords.NewPassword);
        if (result.Succeeded)
        {
            return new OperationResult(true);
        }
        else
        {
            var errors = string.Join(' ', result.Errors.Select(e => e.Description));
            return new OperationResult(false, $"Password has not been changed: {errors}");
        }
    }
}
