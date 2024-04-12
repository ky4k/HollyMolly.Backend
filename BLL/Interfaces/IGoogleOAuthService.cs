namespace HM.BLL.Interfaces;

public interface IGoogleOAuthService
{
    string GenerateOAuthRequestUrl(string redirectUri);
    Task<string?> ExchangeCodeOnTokenAsync(string code, string redirectUri,
        CancellationToken cancellationToken);
    Task<string?> GetUserEmailAsync(string token, CancellationToken cancellationToken);
}
