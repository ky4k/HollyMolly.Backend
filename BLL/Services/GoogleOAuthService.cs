using HM.BLL.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HM.BLL.Services;

public class GoogleOAuthService(
    IConfigurationHelper configurationHelper,
    IHttpClientFactory httpClientFactory,
    ILogger<GoogleOAuthService> logger
        ) : IGoogleOAuthService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly ILogger<GoogleOAuthService> _logger = logger;

    private readonly string? _googleApiEndpoint = configurationHelper.GetConfigurationValue("GoogleOAuth:api_endpoint");
    private readonly string? _scope = configurationHelper.GetConfigurationValue("GoogleOAuth:scope");
    private readonly string? _clientId = configurationHelper.GetConfigurationValue("GoogleOAuth:client_id");
    private readonly string? _clientSecret = configurationHelper.GetConfigurationValue("GoogleOAuth:client_secret");
    private readonly string? _authUri = configurationHelper.GetConfigurationValue("GoogleOAuth:auth_uri");
    private readonly string? _tokenUri = configurationHelper.GetConfigurationValue("GoogleOAuth:token_uri");

    public string GenerateOAuthRequestUrl(string redirectUri)
    {
        var queryParam = new Dictionary<string, string?>()
        {
            { "client_id", _clientId },
            { "redirect_uri", redirectUri },
            { "response_type", "code" },
            { "scope", _scope }
        };
        return QueryHelpers.AddQueryString(_authUri!, queryParam);
    }

    public async Task<string?> ExchangeCodeOnTokenAsync(string code, string redirectUri,
        CancellationToken cancellationToken)
    {
        var tokenParam = new Dictionary<string, string?>()
        {
            { "client_id", _clientId },
            { "client_secret", _clientSecret },
            { "redirect_uri", redirectUri },
            { "code", code },
            { "grant_type", "authorization_code" }
        };
        var content = new FormUrlEncodedContent(tokenParam);

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(_tokenUri, content, cancellationToken);
            using var stream = response.Content.ReadAsStream(cancellationToken);
            TokenResult? token = JsonSerializer.Deserialize<TokenResult>(stream);
            return token?.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting token from Google api.");
            return null;
        }
    }

    public async Task<string?> GetUserEmailAsync(string token, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, _googleApiEndpoint);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            using var stream = response.Content.ReadAsStream(cancellationToken);
            UserInfo? userInfo = JsonSerializer.Deserialize<UserInfo>(stream);
            return userInfo?.Email;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting the user email address from the Google API");
            return null;
        }
    }

    private sealed class TokenResult
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }

    private sealed class UserInfo()
    {
        [JsonPropertyName("sub")]
        public string? Sub { get; set; }
        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }
    }
}
