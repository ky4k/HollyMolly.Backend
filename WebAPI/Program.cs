using FluentValidation;
using HM.BLL.Validators;
using HM.DAL.Data;
using HM.DAL.Entities;
using HM.WebAPI.Configurations;
using HM.WebAPI.Extensions;
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
    ?? builder.Configuration["Stripe:SecretKey"];

builder.RegisterDIServices();

var app = builder.Build();

await app.SeedDatabaseAsync();

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("FreeCORSPolicy");
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseReplaceAuthorizationHeader();
app.UseAuthentication();
app.UseAuthorization();
app.UseTokenRevocation();

app.MapControllers();

app.Run();
