using HM.BLL.UnitTests.TestHelpers;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HM.BLL.UnitTests.Data;

public class HmDbContextInitializerTests
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<HmDbContextInitializer> _logger;
    private readonly HmDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly HmDbContextInitializer _initializer;
    public HmDbContextInitializerTests()
    {
        _configuration = Substitute.For<IConfiguration>();
        _context = ServiceHelper.GetTestDbContext();
        _userManager = ServiceHelper.GetUserManager(_context);
        _roleManager = ServiceHelper.GetRoleManager(_context);
        _logger = Substitute.For<ILogger<HmDbContextInitializer>>();
        _initializer = new HmDbContextInitializer(_configuration, _context,
            _userManager, _roleManager, _logger);
    }

    [Fact]
    public async Task ApplyMigrationsAsync_ShouldApplyMigrationsCorrectly()
    {
        // In order to reach the actual test you need to provide a connection string for your instance
        // of the MS SQL Server. The test connection string MUST NOT be the same as the default connection
        // because the test database will be deleted every test.
        // To provide test connection string placed it in the following file:
        // BLL.UnitTests\bin\Debug\net8.0\connection.json
        // under section "ConnectionStrings":"TestConnection"
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("connection.json", optional: true, reloadOnChange: true)
            .Build();
        var connection = configuration.GetConnectionString("TestConnection");
        if (string.IsNullOrWhiteSpace(connection))
        {
            // Skip test if no test connection string was provided.
            Assert.True(false);
            return;
        }

        var options = new DbContextOptionsBuilder<HmDbContext>()
            .UseSqlServer(connection)
            .Options;
        var context = new HmDbContext(options);
        context.Database.EnsureDeleted();

        var initializer = new HmDbContextInitializer(_configuration, context,
            _userManager, _roleManager, _logger);

        Exception? exception = await Record.ExceptionAsync(initializer.ApplyMigrationsAsync);

        Assert.Null(exception);
        context.Database.EnsureDeleted();
    }
    [Fact]
    public async Task ApplyMigrationsAsync_ShouldWork_WhenNoNewMigrations()
    {
        var connection = new SqliteConnection("Data Source=InMemorySample;Mode=Memory;Cache=Shared");
        connection.Open();
        var options = new DbContextOptionsBuilder<HmDbContext>().UseSqlite(connection).Options;
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(options);
        var dbFacade = Substitute.ForPartsOf<DatabaseFacade>(dbContextMock);
        dbContextMock.Database.Returns(dbFacade);
        var initializer = new HmDbContextInitializer(_configuration, dbContextMock,
            _userManager, _roleManager, _logger);

        Exception? exception = await Record.ExceptionAsync(initializer.ApplyMigrationsAsync);

        Assert.Null(exception);
        connection.Close();
    }
    [Fact]
    public async Task ApplyMigrationsAsync_ShouldHandleErrors()
    {
        Exception? exception = await Record.ExceptionAsync(_initializer.ApplyMigrationsAsync);

        Assert.Null(exception);
    }

    [Fact]
    public async Task SeedAsync_ShouldSeedDefaultRoles()
    {
        int numberBefore = await _context.Roles.CountAsync();

        await _initializer.SeedAsync();
        int numberAfter = await _context.Roles.CountAsync();

        Assert.Equal(0, numberBefore);
        Assert.Equal(3, numberAfter);
    }
    [Fact]
    public async Task SeedAsync_ShouldNotSeedDefaultRoles_WhenTheyAlreadyExist()
    {
        int numberBefore = await _context.Roles.CountAsync();

        await _initializer.SeedAsync();
        int numberAfter1 = await _context.Roles.CountAsync();
        await _initializer.SeedAsync();
        int numberAfter2 = await _context.Roles.CountAsync();

        Assert.Equal(0, numberBefore);
        Assert.Equal(3, numberAfter1);
        Assert.Equal(3, numberAfter2);
    }
    [Fact]
    public async Task SeedAsync_ShouldSeedDefaultAdmin_WhenTheyNotExist()
    {
        int numberBefore = await _context.Users.CountAsync();
        _configuration["DefaultAdmin:Email"] = "default@example.com";
        _configuration["DefaultAdmin:Password"] = "defaultPassword";
        _userManager.AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(IdentityResult.Success);

        await _initializer.SeedAsync();
        int numberAfter = await _context.Users.CountAsync();

        Assert.Equal(0, numberBefore);
        Assert.Equal(1, numberAfter);
    }
    [Fact]
    public async Task SeedAsync_ShouldNotSeedDefaultAdmin_WhenTheyAlreadyExist()
    {
        await _userManager.CreateAsync(new User()
        {
            Email = "default@example.com",
            UserName = "default@example.com"
        });
        _configuration["DefaultAdmin:Email"] = "default@example.com";
        _configuration["DefaultAdmin:Password"] = "defaultPassword";
        int numberBefore = await _context.Users.CountAsync();

        await _initializer.SeedAsync();
        int numberAfter = await _context.Users.CountAsync();

        Assert.Equal(1, numberBefore);
        Assert.Equal(1, numberAfter);
    }
    [Fact]
    public async Task SeedAsync_ShouldNotSeedDefaultAdmin_WhenEmailNotProvided()
    {
        _configuration["DefaultAdmin:Password"] = "defaultPassword";

        await _initializer.SeedAsync();
        int numberAfter = await _context.Users.CountAsync();

        Assert.Equal(0, numberAfter);
    }
    [Fact]
    public async Task SeedAsync_ShouldNotSeedDefaultAdmin_WhenPasswordNotProvided()
    {
        _configuration["DefaultAdmin:Email"] = "default@example.com";

        await _initializer.SeedAsync();
        int numberAfter = await _context.Users.CountAsync();

        Assert.Equal(0, numberAfter);
    }
    [Fact]
    public async Task SeedAsync_ShouldHandleErrors()
    {
        _configuration["DefaultAdmin:Email"] = "default@example.com";
        _configuration["DefaultAdmin:Password"] = "defaultPassword";

        Exception? exception = await Record.ExceptionAsync(_initializer.SeedAsync);

        Assert.Null(exception);
    }
}
