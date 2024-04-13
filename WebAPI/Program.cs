using FluentValidation;
using HM.BLL.Interfaces;
using HM.BLL.Services;
using HM.BLL.Validators;
using HM.DAL.Data;
using HM.DAL.Entities;
using HM.WebAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}
builder.Services.AddDbContext<HmDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddIdentity<User, Role>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
})
    .AddEntityFrameworkStores<HmDbContext>();

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddCors(options => options.AddPolicy(name: "FreeCORSPolicy", cfg =>
{
    cfg.AllowAnyHeader();
    cfg.AllowAnyMethod();
    cfg.WithOrigins("*");
}));
builder.Services.AddScoped<ReplaceAuthorizationHeaderMiddleware>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        string? key = Environment.GetEnvironmentVariable("JwtSettings:SecurityKey");
        if (string.IsNullOrEmpty(key))
        {
            key = builder.Configuration?["JwtSettings:SecurityKey"];
        }
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            RequireExpirationTime = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration?["JwtSettings:Issuer"] ?? "HollyMolly",
            ValidAudience = builder.Configuration?["JwtSettings:Audience"] ?? "*",
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(key ?? "defaultKey_that_is_32_characters"))
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "HollyMolly", Version = "v1" });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddSerilog(options =>
    options.MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.File("./logs/log.txt", LogEventLevel.Information, rollingInterval: RollingInterval.Day)
        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug));

builder.Services.AddHttpClient();

builder.Services.AddScoped<HmDbContextInitializer>();
builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<HmDbContextInitializer>();
    await initializer.ApplyMigrationsAsync();
    await initializer.SeedAsync();
}

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("FreeCORSPolicy");
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseMiddleware<ReplaceAuthorizationHeaderMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
