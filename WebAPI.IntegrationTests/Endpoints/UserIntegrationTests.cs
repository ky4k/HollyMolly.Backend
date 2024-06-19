using HM.BLL.Models.Users;
using HM.DAL.Constants;
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

public class UserIntegrationTests : IClassFixture<SharedWebAppFactory>
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly SharedWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly AuthorizationHelper _authorizationHelper;
    public UserIntegrationTests(SharedWebAppFactory factory)
    {
        _factory = factory;
        _factory.Initialize();
        _factory.SeedContextAsync(SeedDefaultEntities.SeedAsync).WaitAsync(CancellationToken.None);
        _httpClient = factory.CreateClient();
        _authorizationHelper = new AuthorizationHelper(_httpClient);
    }
    [Fact]
    public async Task GetAllUsers_ShouldReturnAllUsers()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Users");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using Stream stream = await httpResponse.Content.ReadAsStreamAsync();
        IEnumerable<UserDto>? users = await JsonSerializer.DeserializeAsync<IEnumerable<UserDto>>(
            stream, jsonSerializerOptions);

        Assert.NotNull(users);
        Assert.NotEmpty(users);
        Assert.Equal(6, users.Count());
    }
    [Fact]
    public async Task GetUserById_ShouldReturnUser()
    {
        string userId = "1";
        HttpRequestMessage requestMessage = new(HttpMethod.Get, $"api/Users/{userId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using Stream stream = await httpResponse.Content.ReadAsStreamAsync();
        UserDto? user = await JsonSerializer.DeserializeAsync<UserDto>(
            stream, jsonSerializerOptions);

        Assert.NotNull(user);
        Assert.Equal("user1@example.com", user.Email);
    }
    [Fact]
    public async Task ChangeUserRoles_ShouldWork()
    {
        string userId = "4";
        HttpRequestMessage requestMessage = new(HttpMethod.Put, $"api/Users/{userId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");
        string[] roles = [DefaultRoles.Manager, DefaultRoles.User];
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(roles),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using Stream stream = await httpResponse.Content.ReadAsStreamAsync();
        UserDto? user = await JsonSerializer.DeserializeAsync<UserDto>(
            stream, jsonSerializerOptions);

        Assert.NotNull(user);
        Assert.Equal(2, user.Roles.Count);
        Assert.Contains(DefaultRoles.Manager, user.Roles);
    }
    [Fact]
    public async Task DeleteUser_ShouldWork()
    {
        string userId = "5";
        HttpRequestMessage requestMessage = new(HttpMethod.Delete, $"api/Users/{userId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        User? user = await context!.Users.FirstOrDefaultAsync(u => u.Id == userId);

        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
        Assert.Null(user);
    }
    [Fact]
    public async Task GetAllRoles_ShouldReturnAllRoles()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, $"api/Users/roles");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using Stream stream = await httpResponse.Content.ReadAsStreamAsync();
        IEnumerable<string>? roles = await JsonSerializer.DeserializeAsync<IEnumerable<string>>(
            stream, jsonSerializerOptions);

        Assert.NotNull(roles);
        Assert.Contains(DefaultRoles.Administrator, roles);
        Assert.Contains(DefaultRoles.Manager, roles);
        Assert.Contains(DefaultRoles.User, roles);
    }
}
