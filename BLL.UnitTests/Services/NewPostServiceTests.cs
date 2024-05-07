using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using HM.BLL.Services;
using HM.BLL.UnitTests.TestHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace HM.BLL.UnitTests.Services;

public class NewPostServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly MockHttpMessageHandler _httpMessageHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NewPostService> _logger;
    private readonly NewPostService _newPostService;
    public NewPostServiceTests()
    {
        _configuration = Substitute.For<IConfiguration>();
        _configuration["NewPost:BaseUrl"].Returns("https://test.com/newpost");
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _logger = Substitute.For<ILogger<NewPostService>>();
        _httpMessageHandler = new(HttpStatusCode.OK);
        _httpClientFactory.CreateClient().Returns(new HttpClient(_httpMessageHandler));
        _newPostService = new NewPostService(_configuration, _httpClientFactory, _logger);
    }
    [Fact]
    public async Task GetCitiesAsync_ShouldSendRequest_WhenNoCitySpecified()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;
        _httpMessageHandler.ResponseContent = Array.Empty<NewPostCity>();

        OperationResult<IEnumerable<NewPostCity>> result = await _newPostService
            .GetCitiesAsync(null, 1, CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task GetCitiesAsync_ShouldReturnObtainedCities()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;
        _httpMessageHandler.ResponseContent = new NewPostResponse<NewPostCity>()
        {
            Results =
            [
                new() { Id = "1", Koatuu = "1", Text = "1" },
                new() { Id = "2", Koatuu = "2", Text = "2" },
                new() { Id = "3", Koatuu = "3", Text = "3" },
                new() { Id = "4", Koatuu = "4", Text = "4" }
            ]
        };

        OperationResult<IEnumerable<NewPostCity>> result = await _newPostService
            .GetCitiesAsync(null, 1, CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task GetCitiesAsync_ShouldReturnEmptyArray_WhenNoCitiesWereObtained()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;
        _httpMessageHandler.ResponseContent = new NewPostResponse<NewPostCity>()
        {
            Results = null!
        };

        OperationResult<IEnumerable<NewPostCity>> result = await _newPostService
            .GetCitiesAsync(null, 1, CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task GetCitiesAsync_ShouldReturnFalseResult_WhenResponseWasNotProcessedCorrectly()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;
        _httpMessageHandler.ResponseContent = new byte[32];

        OperationResult<IEnumerable<NewPostCity>> result = await _newPostService
            .GetCitiesAsync(null, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task GetWarehousesAsync_ShouldReturnObtainedWarehouses()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;
        _httpMessageHandler.ResponseContent = new NewPostResponse<NewPostWarehouse>()
        {
            Results =
            [
                new() { Id = "1", Text = "1" },
                new() { Id = "2", Text = "2" },
                new() { Id = "3", Text = "3" },
                new() { Id = "4", Text = "4" }
            ]
        };

        OperationResult<IEnumerable<NewPostWarehouse>> result = await _newPostService
            .GetWarehousesAsync(null, 1, CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task GetWarehousesAsync_ShouldReturnEmptyArray_WhenResponseDoesNotContainElements()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;
        _httpMessageHandler.ResponseContent = new NewPostResponse<NewPostWarehouse>()
        {
            Results = null
        };

        OperationResult<IEnumerable<NewPostWarehouse>> result = await _newPostService
            .GetWarehousesAsync(null, 1, CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task GetWarehousesAsync_ShouldReturnEmptyArray_WhenNoWarehousesWereObtained()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;
        _httpMessageHandler.ResponseContent = Array.Empty<NewPostWarehouse>();

        OperationResult<IEnumerable<NewPostWarehouse>> result = await _newPostService
            .GetWarehousesAsync(null, 1, CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task GetWarehousesAsync_ShouldReturnFalseResult_WhenResponseWasNotProcessedCorrectly()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;
        _httpMessageHandler.ResponseContent = new byte[32];

        OperationResult<IEnumerable<NewPostWarehouse>> result = await _newPostService
            .GetWarehousesAsync(null, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task CheckIfAddressIsValidAsync_ShouldReturnTrue_WhenCityAndWarehouseAreValid()
    {
        MockMultipleHttpMessageHandler multipleHttpMessageHandler = new(2);
        multipleHttpMessageHandler.StatusCodes[0] = HttpStatusCode.OK;
        multipleHttpMessageHandler.ResponseContent[0] = new NewPostResponse<NewPostCity>()
        {
            Results =
            [
                new() { Id = "1", Koatuu = "1", Text = "1" }
            ]
        };
        multipleHttpMessageHandler.StatusCodes[1] = HttpStatusCode.OK;
        multipleHttpMessageHandler.ResponseContent[1] = new NewPostResponse<NewPostWarehouse>()
        {
            Results =
            [
                new() { Id = "1", Text = "1" }
            ]
        };
        _httpClientFactory.CreateClient().Returns(new HttpClient(multipleHttpMessageHandler));
        var newPostService = new NewPostService(_configuration, _httpClientFactory, _logger);

        bool isValid = await newPostService.CheckIfAddressIsValidAsync("1", "1", CancellationToken.None);

        Assert.True(isValid);
    }
    [Fact]
    public async Task CheckIfAddressIsValidAsync_ShouldReturnFalse_WhenCityWasNotObtained()
    {
        MockMultipleHttpMessageHandler multipleHttpMessageHandler = new(2);
        multipleHttpMessageHandler.StatusCodes[0] = HttpStatusCode.OK;
        multipleHttpMessageHandler.ResponseContent[0] = new byte[32];
        multipleHttpMessageHandler.StatusCodes[1] = HttpStatusCode.OK;
        multipleHttpMessageHandler.ResponseContent[1] = new NewPostResponse<NewPostWarehouse>()
        {
            Results =
            [
                new() { Id = "1", Text = "1" }
            ]
        };
        _httpClientFactory.CreateClient().Returns(new HttpClient(multipleHttpMessageHandler));
        var newPostService = new NewPostService(_configuration, _httpClientFactory, _logger);

        bool isValid = await newPostService.CheckIfAddressIsValidAsync("1", "1", CancellationToken.None);

        Assert.False(isValid);
    }
    [Fact]
    public async Task CheckIfAddressIsValidAsync_ShouldReturnFalse_WhenCityNameIsIncorrect()
    {
        MockMultipleHttpMessageHandler multipleHttpMessageHandler = new(2);
        multipleHttpMessageHandler.StatusCodes[0] = HttpStatusCode.OK;
        multipleHttpMessageHandler.ResponseContent[0] = new NewPostResponse<NewPostCity>()
        {
            Results =
            [
                new() { Id = "11", Koatuu = "11", Text = "11" }
            ]
        };
        multipleHttpMessageHandler.StatusCodes[1] = HttpStatusCode.OK;
        multipleHttpMessageHandler.ResponseContent[1] = new NewPostResponse<NewPostWarehouse>()
        {
            Results =
            [
                new() { Id = "1", Text = "1" }
            ]
        };
        _httpClientFactory.CreateClient().Returns(new HttpClient(multipleHttpMessageHandler));
        var newPostService = new NewPostService(_configuration, _httpClientFactory, _logger);

        bool isValid = await newPostService.CheckIfAddressIsValidAsync("1", "1", CancellationToken.None);

        Assert.False(isValid);
    }
    [Fact]
    public async Task CheckIfAddressIsValidAsync_ShouldReturnFalse_WhenWarehouseNameIsIncorrect()
    {
        MockMultipleHttpMessageHandler multipleHttpMessageHandler = new(2);
        multipleHttpMessageHandler.StatusCodes[0] = HttpStatusCode.OK;
        multipleHttpMessageHandler.ResponseContent[0] = new NewPostResponse<NewPostCity>()
        {
            Results =
            [
                new() { Id = "1", Koatuu = "1", Text = "1" }
            ]
        };
        multipleHttpMessageHandler.StatusCodes[1] = HttpStatusCode.OK;
        multipleHttpMessageHandler.ResponseContent[1] = new NewPostResponse<NewPostWarehouse>()
        {
            Results =
            [
                new() { Id = "11", Text = "11" }
            ]
        };
        _httpClientFactory.CreateClient().Returns(new HttpClient(multipleHttpMessageHandler));
        var newPostService = new NewPostService(_configuration, _httpClientFactory, _logger);

        bool isValid = await newPostService.CheckIfAddressIsValidAsync("1", "1", CancellationToken.None);

        Assert.False(isValid);
    }
    [Fact]
    public async Task CheckIfAddressIsValidAsync_ShouldReturnFalse_WhenWarehouseWasNotObtained()
    {
        MockMultipleHttpMessageHandler multipleHttpMessageHandler = new(2);
        multipleHttpMessageHandler.StatusCodes[0] = HttpStatusCode.OK;
        multipleHttpMessageHandler.ResponseContent[0] = new NewPostResponse<NewPostCity>()
        {
            Results =
            [
                new() { Id = "1", Koatuu = "1", Text = "1" }
            ]
        };
        multipleHttpMessageHandler.StatusCodes[1] = HttpStatusCode.OK;
        multipleHttpMessageHandler.ResponseContent[1] = new byte[32];
        _httpClientFactory.CreateClient().Returns(new HttpClient(multipleHttpMessageHandler));
        var newPostService = new NewPostService(_configuration, _httpClientFactory, _logger);

        bool isValid = await newPostService.CheckIfAddressIsValidAsync("1", "1", CancellationToken.None);

        Assert.False(isValid);
    }
}
