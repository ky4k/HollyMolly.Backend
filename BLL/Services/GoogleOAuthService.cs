using HM.BLL.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HM.BLL.Services;

public class GoogleOAuthService : IGoogleOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleOAuthService> _logger;

    private const string GoogleApiEndpoint = "https://openidconnect.googleapis.com/v1/userinfo";

    private const string _scope = "https://www.googleapis.com/auth/userinfo.email";
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _authUri;
    private readonly string _tokenUri;
    public GoogleOAuthService(
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<GoogleOAuthService> logger
        )
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;

        _clientId = GetCredential("GoogleOAuth:client_id");
        _clientSecret = GetCredential("GoogleOAuth:client_secret");
        _authUri = GetCredential("GoogleOAuth:auth_uri");
        _tokenUri = GetCredential("GoogleOAuth:token_uri");
    }

    public string GenerateOAuthRequestUrl(string redirectUri)
    {
        var queryParam = new Dictionary<string, string?>()
        {
            { "client_id", _clientId },
            { "redirect_uri", redirectUri },
            { "response_type", "code" },
            { "scope", _scope }
        };
        return QueryHelpers.AddQueryString(_authUri, queryParam);
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
            var response = await _httpClient.PostAsync(_tokenUri, content, cancellationToken);
            using var stream = response.Content.ReadAsStream(cancellationToken);
            var token = JsonSerializer.Deserialize<TokenResult>(stream);
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
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, GoogleApiEndpoint);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            using var stream = response.Content.ReadAsStream(cancellationToken);
            var userInfo = JsonSerializer.Deserialize<UserInfo>(stream);
            return userInfo?.Email;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting the user email address from the Google API");
            return null;
        }
    }

    private string GetCredential(string key)
    {
        string? value = Environment.GetEnvironmentVariable(key);
        value ??= _configuration.GetValue<string>(key);
        if (value == null)
        {
            _logger.LogError("Cannot get the value of the {key} from the environment or " +
                "the configuration. Google API may not work correctly.", key);
        }
        return value ?? "";
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
