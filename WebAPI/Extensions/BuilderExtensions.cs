using HM.BLL.Helpers;
using HM.BLL.Interfaces;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Services;
using HM.BLL.Services.NewPost;
using HM.DAL.Data;
using HM.WebAPI.Middleware;
using System.IdentityModel.Tokens.Jwt;

namespace HM.WebAPI.Extensions;

public static class BuilderExtensions
{
    public static void RegisterDIServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<IConfigurationHelper, ConfigurationHelper>();
        builder.Services.AddScoped<ReplaceAuthorizationHeaderMiddleware>();
        builder.Services.AddScoped<TokenRevocationMiddleware>();
        builder.Services.AddScoped<HmDbContextInitializer>();
        builder.Services.AddScoped<JwtSecurityTokenHandler>();
        builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IImageService, ImageService>();
        builder.Services.AddScoped<ISmtpClientFactory, SmtpClientFactory>();
        builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<INewsSubscriptionService, NewsSubscriptionService>();
        builder.Services.AddScoped<IStatisticsService, StatisticsService>();
        builder.Services.AddScoped<IExcelHelper, ExcelHelper>();
        builder.Services.AddScoped<IWishListService, WishListService>();
        builder.Services.AddScoped<ICheckoutService, CheckoutService>();
        builder.Services.AddScoped<Stripe.Checkout.SessionService>();
        builder.Services.AddScoped<ISupportService, SupportService>();
        builder.Services.AddScoped<INewPostCitiesService, NewPostCitiesService>();
        builder.Services.AddScoped<INewPostCounerAgentService,NewPostCounterAgentService>();
        builder.Services.AddScoped<INewPostInternetDocumentService, NewPostInternetDocumentService>();
    }
}
