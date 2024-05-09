using HM.BLL.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HM.BLL.UnitTests.Helpers;

public class ConfigurationHelperTests
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationHelper> _logger;
    private readonly ConfigurationHelper _configurationHelper;
    public ConfigurationHelperTests()
    {
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<ConfigurationHelper>>();
        _configurationHelper = new ConfigurationHelper(_configuration, _logger);
    }
    [Fact]
    public void GetConfigurationValue_ShouldUseEnvironmentVariable_WhenProvided()
    {
        string key = "TestKey";
        string value = "TestValue";
        string? tempVariable = Environment.GetEnvironmentVariable(key);
        Environment.SetEnvironmentVariable(key, value);

        string? actual = _configurationHelper.GetConfigurationValue(key);

        Assert.Equal(value, actual);
        Environment.SetEnvironmentVariable(key, tempVariable);
    }
    [Fact]
    public void GetConfigurationValue_ShouldUseConfiguration_WhenProvided()
    {
        string key = "TestKey";
        string value = "TestValue";
        _configuration[key].Returns(value);

        string? actual = _configurationHelper.GetConfigurationValue(key);

        Assert.Equal(value, actual);
    }
    [Fact]
    public void GetConfigurationValue_ShouldReturnNull_WhenValueIsNotProvided()
    {
        string key = "TestKey";
        _configuration[key].Returns((string?)null);

        string? actual = _configurationHelper.GetConfigurationValue(key);

        Assert.Null(actual);
    }
}
