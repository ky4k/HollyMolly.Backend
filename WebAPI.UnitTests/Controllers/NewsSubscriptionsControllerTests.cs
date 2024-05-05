using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewsSubscriptions;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace WebAPI.UnitTests.Controllers;

public class NewsSubscriptionsControllerTests
{
    private readonly INewsSubscriptionService _subscriptionService;
    private readonly IEmailService _emailService;
    private readonly NewsSubscriptionsController _newsSubscriptionsController;
    public NewsSubscriptionsControllerTests()
    {
        _subscriptionService = Substitute.For<INewsSubscriptionService>();
        _emailService = Substitute.For<IEmailService>();
        _newsSubscriptionsController = new NewsSubscriptionsController(_subscriptionService, _emailService);
    }
    [Fact]
    public async Task GetAllSubscriptions_ShouldReturnOkResult()
    {
        _subscriptionService.GetAllSubscriptionsAsync(Arg.Any<CancellationToken>()).Returns([]);

        ActionResult<IEnumerable<NewsSubscriptionDto>> response = await _newsSubscriptionsController
            .GetAllSubscriptions(CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task AddSubscription_ShouldReturnOkResult_WhenSucceeded()
    {
        _subscriptionService.AddSubscriptionAsync(Arg.Any<NewsSubscriptionCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true, "Succeded!"));

        ActionResult<IEnumerable<NewsSubscriptionDto>> response = await _newsSubscriptionsController
            .AddSubscription(new NewsSubscriptionCreateDto(), CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task AddSubscription_ShouldReturnBadRequest_WhenFailed()
    {
        _subscriptionService.AddSubscriptionAsync(Arg.Any<NewsSubscriptionCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed!"));

        ActionResult<IEnumerable<NewsSubscriptionDto>> response = await _newsSubscriptionsController
            .AddSubscription(new NewsSubscriptionCreateDto(), CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task CancelSubscription_ShouldReturnOkResult_WhenSucceeded()
    {
        _subscriptionService.RemoveSubscriptionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true, "Succeded!"));

        ActionResult<IEnumerable<NewsSubscriptionDto>> response = await _newsSubscriptionsController
            .CancelSubscription("remove token", CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task CancelSubscription_ShouldReturnBadRequest_WhenFailed()
    {
        _subscriptionService.RemoveSubscriptionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed!"));

        ActionResult<IEnumerable<NewsSubscriptionDto>> response = await _newsSubscriptionsController
            .CancelSubscription("remove token", CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task SendNewsToAllSubscribers_ShouldReturnOkResult_WhenSucceeded()
    {
        _subscriptionService.GetAllSubscriptionsAsync(Arg.Any<CancellationToken>()).Returns([]);
        _emailService.SendNewsEmailAsync(Arg.Any<IEnumerable<NewsSubscriptionDto>>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true, "Succeded!"));

        ActionResult<string> response = await _newsSubscriptionsController
            .SendNewsToAllSubscribers("subject", "text", CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task SendNewsToAllSubscribers_ShouldReturnBadRequest_WhenEmailDailyLimitIsExceeded()
    {
        _subscriptionService.GetAllSubscriptionsAsync(Arg.Any<CancellationToken>())
            .Returns(new NewsSubscriptionDto[4]);
        _emailService.SendNewsEmailAsync(Arg.Any<IEnumerable<NewsSubscriptionDto>>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true, "Succeded!"));

        ActionResult<string> response = await _newsSubscriptionsController
            .SendNewsToAllSubscribers("subject", "text", CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task SendNewsToAllSubscribers_ShouldReturnBadRequest_WhenFailed()
    {
        _subscriptionService.GetAllSubscriptionsAsync(Arg.Any<CancellationToken>()).Returns([]);
        _emailService.SendNewsEmailAsync(Arg.Any<IEnumerable<NewsSubscriptionDto>>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed"));

        ActionResult<string> response = await _newsSubscriptionsController
            .SendNewsToAllSubscribers("subject", "text", CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.NotNull(result.Value);
    }
}
