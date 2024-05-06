using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HM.WebAPI.Configurations;

public class JwtBearerConfigurationOptions(IConfiguration configuration)
{
    public void Configure(JwtBearerOptions options)
    {
        string? key = Environment.GetEnvironmentVariable("JwtSettings:SecurityKey");
        if (string.IsNullOrEmpty(key))
        {
            key = configuration["JwtSettings:SecurityKey"];
        }
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            RequireExpirationTime = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JwtSettings:Issuer")
                ?? configuration["JwtSettings:Issuer"],
            ValidAudience = Environment.GetEnvironmentVariable("JwtSettings:Audience")
                ?? configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(key!))
        };
    }
}
