using HM.BLL.Interfaces;
using HM.BLL.Services;
using HM.BLL.UnitTests.TestHelpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace HM.BLL.UnitTests.Services;

public class GoogleOAuthServiceTests
{
    private readonly IConfigurationHelper _configurationHelper;
    private readonly MockHttpMessageHandler _httpMessageHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleOAuthService> _logger;
    private readonly GoogleOAuthService _googleOAuthService;

    public GoogleOAuthServiceTests()
    {
        _configurationHelper = Substitute.For<IConfigurationHelper>();
        _configurationHelper.GetConfigurationValue(Arg.Any<string>()).Returns("https://test.com/token");
        _httpMessageHandler = new(HttpStatusCode.OK);
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _httpClientFactory.CreateClient().Returns(new HttpClient(_httpMessageHandler));
        _logger = Substitute.For<ILogger<GoogleOAuthService>>();
        _googleOAuthService = new GoogleOAuthService(_configurationHelper, _httpClientFactory, _logger);
    }

    [Fact]
    public void GenerateOAuthRequestUrl_ShouldReturnUrlWithAllRequiredParameters()
    {
        string requestUrl = _googleOAuthService.GenerateOAuthRequestUrl("redirectTo");

        Assert.Contains("client_id", requestUrl);
        Assert.Contains("redirect_uri", requestUrl);
        Assert.Contains("response_type", requestUrl);
        Assert.Contains("code", requestUrl);
        Assert.Contains("scope", requestUrl);
    }
    [Fact]
    public async Task ExchangeCodeOnTokenAsync_ShouldReturnAccessToken()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;
        _httpMessageHandler.ResponseContent = new
        {
            access_token = "TestToken",
            expires_in = 120,
            scope = "name",
            token_type = "jwt",
            refresh_token = "RefreshToken"
        };

        string? token = await _googleOAuthService
            .ExchangeCodeOnTokenAsync("code", "redirectTo", CancellationToken.None);

        Assert.NotNull(token);
    }
    [Fact]
    public async Task ExchangeCodeOnTokenAsync_ShouldReturnNull_WhenGoogleReturnNoToken()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.BadRequest;

        string? token = await _googleOAuthService
            .ExchangeCodeOnTokenAsync("code", "redirectTo", CancellationToken.None);

        Assert.Null(token);
    }
    [Fact]
    public async Task ExchangeCodeOnTokenAsync_ShouldReturnNull_WhenResponseCannotBeDeserialized()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;
        _httpMessageHandler.ResponseContent = new byte[32];

        string? token = await _googleOAuthService
            .ExchangeCodeOnTokenAsync("code", "redirectTo", CancellationToken.None);

        Assert.Null(token);
    }
    [Fact]
    public async Task GetUserEmailAsync_ShouldReturnAccessUserEmail()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;
        _httpMessageHandler.ResponseContent = new
        {
            sub = "sub",
            picture = "picture",
            email = "test@example.com",
            email_verified = true
        };

        string? email = await _googleOAuthService.GetUserEmailAsync("TestToken", CancellationToken.None);

        Assert.NotNull(email);
        Assert.Equal("test@example.com", email);
    }
    [Fact]
    public async Task GetUserEmailAsync_ShouldReturnNull_WhenGoogleReturnNoEmail()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;

        string? email = await _googleOAuthService.GetUserEmailAsync("TestToken", CancellationToken.None);

        Assert.Null(email);
    }
    [Fact]
    public async Task GetUserEmailAsync_ShouldReturnNull_WhenResponseCannotBeDeserialized()
    {
        _httpMessageHandler.StatusCode = HttpStatusCode.OK;
        _httpMessageHandler.ResponseContent = new byte[32];

        string? email = await _googleOAuthService.GetUserEmailAsync("TestToken", CancellationToken.None);

        Assert.Null(email);
    }
}
