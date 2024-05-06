using Microsoft.AspNetCore.Mvc.Testing;

namespace WebAPI.IntegrationTests;

public class ProgramTests
{
    [Fact]
    public void Program_ShouldStartCorrectly()
    {
        WebApplicationFactory<Program> factory = new();

        Assert.NotNull(factory.Services);
    }
}
