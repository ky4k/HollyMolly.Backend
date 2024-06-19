using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HM.WebAPI.Middleware;

/// <summary>
/// Is used to check if JWT token for the user was revoked and is no longer valid.
/// </summary>
public class TokenRevocationMiddleware(HmDbContext dbContext) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<AuthorizeAttribute>() != null
            && context.User.Identity!.IsAuthenticated)
        {
            string userId = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            long issuedAt = long.Parse(context.User.FindFirst("IssuedAt")!.Value);
            if (user.InvalidateTokenBefore != null && issuedAt < user.InvalidateTokenBefore)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }
        await next(context);
    }
}
