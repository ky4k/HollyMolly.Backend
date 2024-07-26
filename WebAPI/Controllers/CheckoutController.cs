using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CheckoutController(
    ICheckoutService checkoutService,
    IConfigurationHelper configurationHelper
    ) : ControllerBase
{
    private readonly string _paymentPage = configurationHelper.GetConfigurationValue("FrontendUrls:PaymentPage") ?? "";
    /// <summary>
    /// Allows registered users to get link to the Stripe payment page.
    /// </summary>
    /// <response code="200">Returns link to the Stripe payment page.</response>
    /// <response code="400">Indicates that payment link was not created and returns error message.</response>
    [Authorize]
    [Route("{orderId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LinkDto>> CheckoutOrder(int orderId, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        string baseUrl = $"https://{Request.Host}{Request.PathBase}";
        OperationResult<string> result = await checkoutService
            .PayForOrderAsync(orderId, userId, baseUrl, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }
        return Ok(new LinkDto { RedirectToUrl = result.Payload });
    }

    /// <summary>
    /// Is used for communication with the Stripe. Should not be called manually from the frontend.
    /// </summary>
    /// <param name="sessionId"></param>
    /// <response code="302">Indicates that payment was successfully processed
    /// and redirects user to the frontend page setting paymentSucceeded query parameter to true.</response>
    /// <response code="400">Indicates that payment processing has failed and returns error message.</response>
    [Route("success")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CheckoutSucceeded(string sessionId)
    {
        OperationResult result = await checkoutService.CheckoutSuccessAsync(sessionId);
        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }
        Dictionary<string, string?> parameters = new()
        {
            { "paymentSucceeded", "true" }
        };
        return Redirect(QueryHelpers.AddQueryString(_paymentPage, parameters));
    }

    /// <summary>
    /// Is used for communication with the Stripe. Should not be called manually from the frontend.
    /// </summary>
    /// <response code="302">Indicates that payment was not completed
    /// and redirects user to the frontend page setting paymentSucceeded query parameter to false.</response>
    [Route("failed")]
    [HttpGet]
    public ActionResult CheckoutFailed()
    {
        Dictionary<string, string?> parameters = new()
        {
            { "paymentSucceeded", "false" }
        };
        return Redirect(QueryHelpers.AddQueryString(_paymentPage, parameters));
    }
}
