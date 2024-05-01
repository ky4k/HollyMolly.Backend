namespace WebAPI.IntegrationTests.TestHelpers;

internal class FakePolicyEvaluatorOptions
{
    public bool IsAuthenticated { get; set; }
    public bool IsAuthorized { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public DateTime? IssuedAt { get; set; }
    public List<string> Roles { get; set; } = [];

    public static FakePolicyEvaluatorOptions AllowAll => new()
    {
        IsAuthenticated = true,
        IsAuthorized = true,
        UserId = "1",
        UserEmail = "user1@example.com",
        IssuedAt = new DateTime(2020, 12, 20, 0, 0, 0, DateTimeKind.Utc)
    };
}
