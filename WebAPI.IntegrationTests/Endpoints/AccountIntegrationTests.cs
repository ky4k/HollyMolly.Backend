using HM.BLL.Models.Users;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using WebAPI.IntegrationTests.TestHelpers;
using WebAPI.IntegrationTests.WebApplicationFactory;

namespace WebAPI.IntegrationTests.Endpoints;

public class AccountIntegrationTests : IClassFixture<SharedWebAppFactory>
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly SharedWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly AuthorizationHelper _authorizationHelper;
    public AccountIntegrationTests(SharedWebAppFactory factory)
    {
        _factory = factory;
        _factory.Initialize();
        _factory.SeedContextAsync(SeedDefaultEntities.SeedAsync).WaitAsync(CancellationToken.None);
        _httpClient = _factory.CreateClient();
        _authorizationHelper = new AuthorizationHelper(_httpClient);
    }
    [Fact]
    public async Task Registration_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Post, "api/Account/registration");
        RegistrationRequest registrationRequest = new()
        {
            Email = "user101@example.com",
            Password = "password"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(registrationRequest),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using Stream stream = await httpResponse.Content.ReadAsStreamAsync();
        RegistrationResponse? result = await JsonSerializer.DeserializeAsync<RegistrationResponse>(
            stream, jsonSerializerOptions);

        Assert.NotNull(result);
        Assert.NotNull(result.Email);
    }
    [Fact]
    public async Task Login_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Post, "api/Account/login");
        LoginRequest loginRequest = new()
        {
            Email = "user1@example.com",
            Password = "password"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using Stream stream = await httpResponse.Content.ReadAsStreamAsync();
        LoginResponse? result = await JsonSerializer.DeserializeAsync<LoginResponse>(
            stream, jsonSerializerOptions);

        Assert.NotNull(result);
        Assert.NotNull(result.UserEmail);
        Assert.NotNull(result.AccessToken);
        Assert.NotEmpty(result.Roles);
    }
    [Fact]
    public async Task GetProfile_ShouldWork()
    {
        string email = "user2@example.com";
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Account/userInfo");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync(email, "password");
        ProfileUpdateDto profileUpdate = new()
        {
            FirstName = "Правильне",
            LastName = "Ім'я"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(profileUpdate),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using Stream stream = await httpResponse.Content.ReadAsStreamAsync();
        UserDto? result = await JsonSerializer.DeserializeAsync<UserDto>(stream, jsonSerializerOptions);

        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }
    [Fact]
    public async Task CreateProfile_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Post, "api/Account/profiles");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user1@example.com", "password");
        ProfileUpdateDto profileUpdate = new()
        {
            FirstName = "Правильне",
            LastName = "Ім'я"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(profileUpdate),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();

        using Stream stream = await httpResponse.Content.ReadAsStreamAsync();
        ProfileDto? result = await JsonSerializer.DeserializeAsync<ProfileDto>(
            stream, jsonSerializerOptions);

        Assert.NotNull(result);
        Assert.Equal(profileUpdate.FirstName, result.FirstName);
        Assert.Equal(profileUpdate.LastName, result.LastName);
    }
    [Fact]
    public async Task UpdateProfile_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Put, "api/Account/profiles/2");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user2@example.com", "password");
        ProfileUpdateDto profileUpdate = new()
        {
            FirstName = "Правильне",
            LastName = "Ім'я"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(profileUpdate),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();

        using Stream stream = await httpResponse.Content.ReadAsStreamAsync();
        ProfileDto? result = await JsonSerializer.DeserializeAsync<ProfileDto>(
            stream, jsonSerializerOptions);

        Assert.NotNull(result);
        Assert.Equal(profileUpdate.FirstName, result.FirstName);
        Assert.Equal(profileUpdate.LastName, result.LastName);
    }
    [Fact]
    public async Task DeleteProfile_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Delete, "api/Account/profiles/3");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user3@example.com", "password");
        ProfileUpdateDto profileUpdate = new()
        {
            FirstName = "Правильне",
            LastName = "Ім'я"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(profileUpdate),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        Profile? profile = await context!.Profiles.FirstOrDefaultAsync(p => p.Id == 3);

        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
        Assert.Null(profile);
    }
    [Fact]
    public async Task UpdateEmail_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Put, "api/Account/profile/email");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user3@example.com", "password");
        EmailDto emailUpdateDto = new()
        {
            Email = "user102@example.com"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(emailUpdateDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);

        httpResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
    }
    [Fact]
    public async Task ChangeUserPassword_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Put, "api/Account/profile/password");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user4@example.com", "password");
        ChangePasswordDto changePassword = new()
        {
            OldPassword = "password",
            NewPassword = "newPassword"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(changePassword),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);

        httpResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
    }

    [Fact]
    public async Task SendForgetPasswordEmail_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Put, "api/Account/forgetPassword");
        EmailDto emailDto = new()
        {
            Email = "user5@example.com"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(emailDto),
            Encoding.UTF8, "application/json");
        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);

        httpResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
    }
}
