using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BLL.Services;

public class AccountService(
    IConfiguration configuration
    ) : IAccountService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        User user = new() { Id = 1, UserName = "admin", Email = "admin@example.com" };

        string? token;
        try
        {
            token = new JwtSecurityTokenHandler().WriteToken(await GetTokenAsync(user));
        }
        catch (Exception ex)
        {
            //logger.LogError(ex, "An error occurred while setting the JWT token.");
            return new LoginResponse() { Succeeded = false, Message = ex.Message };
        }

        return new LoginResponse()
        {
            Succeeded = true,
            Message = "Login is complete",
            Token = token,
            UserName = user.UserName,
            Roles = []
        };
    }

    public async Task<JwtSecurityToken> GetTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user?.UserName ?? string.Empty),
            new(ClaimTypes.Email, user?.Email ?? string.Empty)
        };
        //foreach (string role in await userManager.GetRolesAsync(user, new CancellationToken()))
        //{
        //    claims.Add(new Claim(ClaimTypes.Role, role));
        //}

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
