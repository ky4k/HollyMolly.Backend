namespace HM.WebAPI.Middleware;

/// <summary>
/// Is used to ignore Basic authorization header required by hosting during trial period.
/// </summary>
public class ReplaceAuthorizationHeaderMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string? authHeader = context.Request.Headers.Authorization;
        if (!string.IsNullOrEmpty(authHeader))
        {
            string[] parts = authHeader.ToString().Split(',');
            string? bearer = Array.Find(parts, part => part.Trim().StartsWith("Bearer "))?.Trim();
            if(!string.IsNullOrEmpty(bearer))
            {
                context.Request.Headers.Authorization = bearer;
            }
        }

        await next(context);
    }
}
