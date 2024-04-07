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
    public async Task<OperationResult<UserDto>> RegisterUserAsync(RegistrationRequest request)
    {
        if (await userManager.FindByEmailAsync(request.Email) != null)
        {
            return new OperationResult<UserDto>(false, "A user with such an email already exist.");
        }
        var user = new User()
        {
            UserName = request.Email,
            Email = request.Email
        };
        try
        {
            await userManager.CreateAsync(user);
            await userManager.AddPasswordAsync(user, request.Password);
            await userManager.AddToRoleAsync(user, DefaultRoles.User);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "User has not been created.");
            return new OperationResult<UserDto>(false, "An error has happened while creating user");
        }
        var userDto = new UserDto()
        {
            Id = user.Id,
            Email = user.Email,
            Roles = await userManager.GetRolesAsync(user),
        };

        return new OperationResult<UserDto>(true, userDto);
    }

    public async Task<OperationResult<LoginResponse>> LoginAsync(LoginRequest loginRequest)
    {
        User? user = await userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null)
        {
            return new OperationResult<LoginResponse>(false, "No user with such a name or email exists.");
        }
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
            AccessToken = token,
            Roles = roles
        };

        return new OperationResult<LoginResponse>(true, response);
    }

    private JwtSecurityToken GetToken(User user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
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
}
