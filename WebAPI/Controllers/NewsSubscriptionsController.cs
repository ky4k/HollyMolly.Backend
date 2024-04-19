using HM.BLL.Interfaces;
using HM.BLL.Models;
using HM.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NewsSubscriptionsController(
    INewsSubscriptionService subscriptionService,
    IEmailService emailService
    ) : ControllerBase
{
    /// <summary>
    /// Allows administrators to get list of the all news subscriptions.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns list of the all subscriptions.</response>
    /// <response code="401">Indicates that the user is not authorized to update the order.</response>
    /// <response code="403">Indicates that the user does not have permission to update the order.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<NewsSubscriptionDto>>> GetAllSubscriptions(CancellationToken cancellationToken)
    {
        return Ok(await subscriptionService.GetAllSubscriptionsAsync(cancellationToken));
    }

    /// <summary>
    /// Allows to add the subscription.
    /// </summary>
    /// <param name="subscription">Email to receive news.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Indicates that operation succeeded and returns the result message.</response>
    /// <response code="400">Returns the error message.</response>
    [Route("")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddSubscription(NewsSubscriptionCreateDto subscription, CancellationToken cancellationToken)
    {
        OperationResult response = await subscriptionService
            .AddSubscriptionAsync(subscription, cancellationToken);
        return response.Succeeded ? Ok(response.Message) : BadRequest(response.Message);
    }

    /// <summary>
    /// Allows to remove subscription.
    /// </summary>
    /// <param name="removeToken">Remove token that is send to email with news.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Indicates that operation succeeded and returns the result message.</response>
    /// <response code="400">Returns the error message.</response>
    [Route("{removeToken}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CancelSubscription(string removeToken, CancellationToken cancellationToken)
    {
        OperationResult response = await subscriptionService
            .RemoveSubscriptionAsync(removeToken, cancellationToken);
        return response.Succeeded ? Ok(response.Message) : BadRequest(response.Message);
    }

    /// <summary>
    /// Allow administrators to send news to all subscribers.
    /// </summary>
    /// <param name="subject">News subjects.</param>
    /// <param name="bodyTextHtml">News text. Text can be formatted by using HTML tags.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the result of the news sending.</response>
    /// <response code="400">Returns the error message.</response>
    /// <response code="401">Indicates that the user is not authorized.</response>
    /// <response code="403">Indicates that the user does not have permission.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("send")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> SendNewsToAllSubscribers(string subject, string bodyTextHtml, CancellationToken cancellationToken)
    {
        IEnumerable<NewsSubscriptionDto> subscriptions = await subscriptionService.GetAllSubscriptionsAsync(cancellationToken);
        if (subscriptions.Count() > 3)
        {
            return BadRequest("During the test period ensure that you send not more than 3 email at the time. " +
                $"You try to send email to {subscriptions.Count()} subscribers. Remove some subscribers before sending news.");
        }

        OperationResult response = await emailService
            .SendNewsEmailAsync(subscriptions, subject, bodyTextHtml, cancellationToken);
        return response.Succeeded ? Ok(response.Message) : BadRequest(response.Message);
    }
}
