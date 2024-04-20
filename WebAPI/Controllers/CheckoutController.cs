using HM.BLL.Interfaces;
using HM.BLL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CheckoutController(
    ICheckoutService checkoutService
    ) : ControllerBase
{
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
        OperationResult<string> response = await checkoutService
            .PayForOrderAsync(orderId, userId, baseUrl, cancellationToken);
        if(!response.Succeeded || response.Payload == null)
        {
            return BadRequest(response.Message);
        }
        return Ok(new LinkDto { RedirectToUrl = response.Payload });
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
        OperationResult response = await checkoutService.CheckoutSuccessAsync(sessionId);
        if(!response.Succeeded)
        {
            return BadRequest(response.Message);
        }
        string redirectToFrontendUrl = $"https://holly-molly.vercel.app/?paymentSucceeded=true";
        return Redirect(redirectToFrontendUrl);
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
        string redirectToFrontendUrl = $"https://holly-molly.vercel.app/?paymentSucceeded=false";
        return Redirect(redirectToFrontendUrl);
    }
}
