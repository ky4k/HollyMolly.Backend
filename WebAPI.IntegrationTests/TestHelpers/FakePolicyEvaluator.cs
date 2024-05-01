using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace WebAPI.IntegrationTests.TestHelpers;

internal class FakePolicyEvaluator(
    FakePolicyEvaluatorOptions options
    ) : IPolicyEvaluator
{
    public async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        if (!options.IsAuthenticated)
        {
            return await Task.FromResult(AuthenticateResult.Fail("Failed"));
        }
        ClaimsIdentity claimsIdentity = new("FakeScheme");
        if (options.UserId != null)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, options.UserId));
        }
        if (options.UserEmail != null)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, options.UserEmail));
        }
        if (options.IssuedAt != null)
        {
            claimsIdentity.AddClaim(new Claim("IssuedAt", options.IssuedAt.Value.Ticks.ToString()));
        }
        foreach (string role in options.Roles)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        var principal = new ClaimsPrincipal();
        principal.AddIdentity(claimsIdentity);

        return await Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(principal, new AuthenticationProperties(), "FakeScheme")));
    }

    public async Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        return options.IsAuthorized
            ? await Task.FromResult(PolicyAuthorizationResult.Success())
            : await Task.FromResult(PolicyAuthorizationResult.Forbid());
    }
}
