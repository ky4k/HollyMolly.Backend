using HM.BLL.Models.Users;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebAPI.IntegrationTests.TestHelpers;

internal class AuthorizationHelper(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    public async Task<AuthenticationHeaderValue> GetAuthorizationHeaderAsync(string email, string password)
    {
        const string RequestURI = "api/account/login";
        var loginRequest = new LoginRequest()
        {
            Email = email,
            Password = password
        };
        var content = new StringContent(JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8, "application/json");

        var httpResponse = await httpClient.PostAsync(RequestURI, content);
        var result = JsonSerializer.Deserialize<LoginResponse>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);
        return new AuthenticationHeaderValue("Bearer", result?.AccessToken);
    }
}
