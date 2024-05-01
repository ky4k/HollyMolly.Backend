using HM.BLL.Models.Users;
using System.Text;
using System.Text.Json;
using WebAPI.IntegrationTests.TestHelpers;
using WebAPI.IntegrationTests.WebApplicationFactory;

namespace WebAPI.IntegrationTests.Endpoints;

public class AccountIntegrationTests : IClassFixture<SharedWebAppFactory>
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _httpClient;
    public AccountIntegrationTests(SharedWebAppFactory factory)
    {
        factory.FactoryHelper = new WebAppFactoryHelper()
        {
            FakePolicyEvaluatorOptions = FakePolicyEvaluatorOptions.AllowAll
        };
        factory.Initialize();
        factory.SeedContextAsync(SeedDefaultEntities.SeedAsync).WaitAsync(CancellationToken.None);
        _httpClient = factory.CreateClient();
    }
    [Fact]
    public async Task Registration_ShouldWork()
    {
        const string RequestURI = "api/account/registration";
        var registrationRequest = new RegistrationRequest()
        {
            Email = "user1@example.com",
            Password = "password"
        };
        var content = new StringContent(JsonSerializer.Serialize(registrationRequest),
            Encoding.UTF8, "application/json");

        var httpResponse = await _httpClient.PostAsync(RequestURI, content);
        httpResponse.EnsureSuccessStatusCode();
        var result = JsonSerializer.Deserialize<RegistrationResponse>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(result);
        Assert.NotNull(result.Email);
    }
    [Fact]
    public async Task Login_ShouldWork()
    {
        const string RequestURI = "api/account/login";
        var loginRequest = new LoginRequest()
        {
            Email = "user1@example.com",
            Password = "password"
        };
        var content = new StringContent(JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8, "application/json");

        var httpResponse = await _httpClient.PostAsync(RequestURI, content);
        httpResponse.EnsureSuccessStatusCode();
        var result = JsonSerializer.Deserialize<LoginResponse>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(result);
        Assert.NotNull(result.UserEmail);
        Assert.NotNull(result.AccessToken);
        Assert.NotEmpty(result.Roles);
    }
    [Fact]
    public async Task GetProfile_ShouldWork()
    {
        const string RequestURI = "api/Account/profile";

        var httpResponse = await _httpClient.GetAsync(RequestURI);
        httpResponse.EnsureSuccessStatusCode();
        var result = JsonSerializer.Deserialize<UserDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(result);
        Assert.NotNull(result.Email);
    }
}
