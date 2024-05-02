using HM.BLL.Models.Users;
using System.Net;
using System.Net.Http.Headers;
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
            Email = "user101@example.com",
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
            Email = "user2@example.com",
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
    [Fact]
    public async Task UpdateProfile_ShouldWork()
    {
        const string RequestURI = "api/Account/profile";
        ProfileUpdateDto profileUpdate = new()
        {
            FirstName = "Правильне",
            LastName = "Ім'я"
        };
        var content = new StringContent(JsonSerializer.Serialize(profileUpdate),
            Encoding.UTF8, "application/json");

        var httpResponse = await _httpClient.PutAsync(RequestURI, content);
        httpResponse.EnsureSuccessStatusCode();
        var result = JsonSerializer.Deserialize<UserDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(result);
        Assert.NotNull(result.Email);
        Assert.Equal(profileUpdate.FirstName, result.FirstName);
        Assert.Equal(profileUpdate.LastName, result.LastName);
    }
    [Fact]
    public async Task UpdateEmail_ShouldWork()
    {
        EmailUpdateDto emailUpdateDto = new()
        {
            NewEmail = "user102@example.com"
        };
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, "api/Account/profile/email");
        requestMessage.Headers.Authorization = await GetAuthorizationHeaderAsync("user3@example.com", "password");
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(emailUpdateDto),
            Encoding.UTF8, "application/json");

        var httpResponse = await _httpClient.SendAsync(requestMessage);

        httpResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
    }
    [Fact]
    public async Task ChangeUserPassword_ShouldWork()
    {
        const string RequestURI = "api/Account/profile/password";
        ChangePasswordDto changePassword = new()
        {
            OldPassword = "password",
            NewPassword = "password"
        };
        var content = new StringContent(JsonSerializer.Serialize(changePassword),
            Encoding.UTF8, "application/json");

        var httpResponse = await _httpClient.PutAsync(RequestURI, content);

        httpResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
    }

    [Fact]
    public async Task SendForgetPasswordEmail_ShouldWork()
    {
        const string RequestURI = "api/Account/forgetPassword";

        var httpResponse = await _httpClient.PutAsync(RequestURI, null);

        httpResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
    }

    private async Task<AuthenticationHeaderValue> GetAuthorizationHeaderAsync(string email, string password)
    {
        const string RequestURI = "api/account/login";
        var loginRequest = new LoginRequest()
        {
            Email = email,
            Password = password
        };
        var content = new StringContent(JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8, "application/json");

        var httpResponse = await _httpClient.PostAsync(RequestURI, content);
        var result = JsonSerializer.Deserialize<LoginResponse>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);
        return new AuthenticationHeaderValue("Bearer", result?.AccessToken);
    }
}
