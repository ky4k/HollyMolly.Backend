using System.Text.Json;
using System.Text;
using HM.BLL.Models.Users;
using WebAPI.IntegrationTests.TestHelpers;

namespace WebAPI.IntegrationTests.Endpoints;

public class AccountIntegrationTests
{
    private static JsonSerializerOptions jsonSerializerOptions =
        new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;
    public AccountIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _httpClient = _factory.CreateClient();
    }
    [Fact]
    public async Task Registration_ShouldWork()
    {
        const string RequestURI = "api/account/registration";
        var loginRequest = new RegistrationRequest()
        {
            Email = "user1@example.com",
            Password = "password" };
        var content = new StringContent(JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8, "application/json");

        var httpResponse = await _httpClient.PostAsync(RequestURI, content);
        httpResponse.EnsureSuccessStatusCode();
        var result = JsonSerializer.Deserialize<RegistrationResponse>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(result);
        Assert.NotNull(result.Email);
    }
    [Fact]
    public async Task GetProfile_ShouldWorkForAuthenticatedUser()
    {
        _httpClient.DefaultRequestHeaders.Add(TestAuthHandler.UserId, "1");
        const string RequestURI = "api/Account/profile";

        var httpResponse = await _httpClient.GetAsync(RequestURI);
        httpResponse.EnsureSuccessStatusCode();
        var result = JsonSerializer.Deserialize<UserDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(result);
        Assert.NotNull(result.Email);
    }
}
