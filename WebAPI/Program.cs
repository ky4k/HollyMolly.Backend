using FluentValidation;
using HM.BLL.Interfaces;
using HM.BLL.Services;
using HM.BLL.Validators;
using HM.DAL.Data;
using HM.DAL.Entities;
using HM.WebAPI.Configurations;
using HM.WebAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HmDbContext>((sp, options) =>
{
    options.UseSqlServer(Environment.GetEnvironmentVariable("DefaultConnection")
        ?? builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<HmDbContext>()
    .AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider);
builder.Services.ConfigureOptions<IdentityConfigureOptions>();

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddCors();
builder.Services.ConfigureOptions<CorsConfigureOptions>();

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
        new JwtBearerConfigurationOptions(builder.Configuration).Configure);
builder.Services.ConfigureOptions<AuthenticationConfigureOptions>();

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<SwaggerGenConfigureOptions>();

builder.Services.AddSerilog(new SerilogConfigureOptions(builder.Configuration).Configure);

Stripe.StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("Stripe:SecretKey")
    ?? builder.Configuration["Stripe:SecretKey"] ?? "";

builder.Services.AddHttpClient();
builder.Services.AddScoped<ReplaceAuthorizationHeaderMiddleware>();
builder.Services.AddScoped<TokenRevocationMiddleware>();
builder.Services.AddScoped<HmDbContextInitializer>();
builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INewsSubscriptionService, NewsSubscriptionService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IWishListService, WishListService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<Stripe.Checkout.SessionService>();

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
app.UseMiddleware<TokenRevocationMiddleware>();

app.MapControllers();

app.Run();
