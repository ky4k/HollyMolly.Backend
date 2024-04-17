using HM.BLL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    /// <summary>
    /// Allows to test API accessibility for any user.
    /// </summary>
    /// <response code="200">Indicates that API is working.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Test()
    {
        return Ok(new { Succeded = true });
    }

    /// <summary>
    /// Allows to test API accessibility for the authenticated users.
    /// </summary>
    /// <response code="200">Indicates that user is authenticated.</response>
    /// <response code="401">Indicates that user is not authenticated.</response>
    [Authorize]
    [Route("authorize")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult TestAuthorize()
    {
        return Ok(new { Succeded = true });
    }

    /// <summary>
    /// Allows to test login on the site via Google account. Returns object that contains
    /// redirectUrl. User should be navigated to that url (in real endpoint for entering their
    /// credentials).
    /// </summary>
    /// <param name = "frontendUrlToRedirectUserTo"> For testing purposes you may set this parameter.
    /// It imitates redirecting user after entering the credentials. User will be redirected to
    /// the specified url. This parameter will not work in real endpoint: The url to the frontend
    /// component should be specified on the server. After testing provide the backend team with
    /// the correct url of the frontend component that will handle login via token.</param>
    /// <response code="200">Returns redirect url user should be navigated to.</response>
    /// <remarks>
    ///    Request example:<br />
    /// https://teamchallenge-001-site1.ktempurl.com/api/test/login/google?frontendUrlToRedirectUserTo=https://holly-molly.vercel.app/handleLoginToken
    ///    <br />After getting response user should be redirected by the frontend to the url from response redirectUrl parameter.
    ///    If everything works correct you will be navigated to the component specified in frontendUrlToRedirectUserTo
    ///    and token will appear as a query parameter: <br />
    ///    https://https://holly-molly.vercel.app/handleLoginToken?token=someToken
    /// </remarks>
    [Route("login/google")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult ImitateGetRedirectUrl(string frontendUrlToRedirectUserTo)
    {
        string redirectUrl = $"https://{Request.Host}{Request.PathBase}/api/Test/login/google/getToken?redirectTo={frontendUrlToRedirectUserTo}";
        return Ok(new { RedirectUrl = redirectUrl });
    }

    /// <summary>
    /// The endpoint imitates communication with the Google server.
    /// As its real version it should not be called directly from the frontend.
    /// </summary>
    /// <param name="redirectTo">For testing purposes is used to redirect back to the frontend.</param>
    /// <response code="302">Redirects user to the frontend page and set token as query parameter.</response>
    [Route("login/google/getToken")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public ActionResult ImitateGoogleRedirect(string redirectTo)
    {
        string? oidcToken = "1234-5678-9012-3456";

        string redirectToFrontendUrl = $"{redirectTo}?token={oidcToken}";
        return Redirect(redirectToFrontendUrl);
    }

    /// <summary>
    /// Endpoint that imitates the functionality to getting user profile info
    /// in exchange of the token that was received during the previous GET request.
    /// </summary>
    /// <param name="loginRequest">The user token that was provided in the previous GET request.</param>
    /// <response code="200">Indicates that the login was successful and returns an access token and user information.</response>
    [Route("login/google")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult ImitateGoogleLogin(LoginOidcRequest loginRequest)
    {
        if (loginRequest.Token == "1234-5678-9012-3456")
        {
            return Ok(new LoginResponse()
            {
                UserId = "testId",
                UserEmail = "test@email.com",
                AccessToken = "accessToken",
                Roles = []
            });
        }
        else
        {
            return BadRequest("Token is invalid");
        }
    }
}
