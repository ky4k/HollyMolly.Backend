using System.Net;
using System.Net.Http.Headers;
using WebAPI.IntegrationTests.TestHelpers;
using WebAPI.IntegrationTests.WebApplicationFactory;

namespace WebAPI.IntegrationTests.Middleware;

public class TokenRevocationMiddlewareIntegrationTests : IClassFixture<SharedWebAppFactory>
{
    private readonly SharedWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly AuthorizationHelper _authorizationHelper;
    public TokenRevocationMiddlewareIntegrationTests(SharedWebAppFactory factory)
    {
        _factory = factory;
        _factory.Initialize();
        _factory.SeedContextAsync(SeedDefaultEntities.SeedAsync).WaitAsync(CancellationToken.None);
        _httpClient = _factory.CreateClient();
        _authorizationHelper = new AuthorizationHelper(_httpClient);
    }
    [Fact]
    public async Task AuthenticatedUser_ShouldBeAbleToAccessAuthorizedEndpoints()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Test/authorize");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user3@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }
    [Fact]
    public async Task AuthenticatedUser_ShouldNotBeAbleToAccessAuthorizedEndpoints_AfterTokenWasInvalidated()
    {
        AuthenticationHeaderValue authHeader = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user3@example.com", "password");
        HttpRequestMessage logoutRequestMessage = new(HttpMethod.Post, "api/Account/logoutAllDevices");
        logoutRequestMessage.Headers.Authorization = authHeader;

        HttpResponseMessage logoutHttpResponse = await _httpClient.SendAsync(logoutRequestMessage);
        logoutHttpResponse.EnsureSuccessStatusCode();

        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Test/authorize");
        requestMessage.Headers.Authorization = authHeader;

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }
    [Fact]
    public async Task AuthenticatedUser_ShouldBeAbleToAccessAuthorizedEndpointsAfter_WhenUserLoginAgainAfterTokenWasInvalidated()
    {
        HttpRequestMessage logoutRequestMessage = new(HttpMethod.Post, "api/Account/logoutAllDevices");
        logoutRequestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user3@example.com", "password");

        HttpResponseMessage logoutHttpResponse = await _httpClient.SendAsync(logoutRequestMessage);
        logoutHttpResponse.EnsureSuccessStatusCode();

        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Test/authorize");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user3@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task UnauthenticatedUser_ShouldBeAbleToAccessUnauthorizedEndpoint()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Test");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }
    [Fact]
    public async Task UnauthenticatedUser_ShouldNotBeAbleToAccessAuthorizedEndpoint()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Test/authorize");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }
    [Fact]
    public async Task Middleware_ShouldProperlyHandleRequest_WhenTheEndpointDoesNotExist()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Test/non-exist");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }
}
