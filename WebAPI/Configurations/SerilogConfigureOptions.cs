using Serilog;
using Serilog.Events;

namespace HM.WebAPI.Configurations;

public class SerilogConfigureOptions(IConfiguration configuration, bool isProduction)
{
    public void Configure(LoggerConfiguration options)
    {
        options.MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .ReadFrom.Configuration(configuration)
            .WriteTo.File("./logs/log.txt", LogEventLevel.Information, rollingInterval: RollingInterval.Day)
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug);
        if(isProduction)
        {
            options.MinimumLevel.Override("HM.BLL.Services.NewPost", LogEventLevel.Warning);
            options.MinimumLevel.Override("Microsoft.Extensions.Http.DefaultHttpClientFactory", LogEventLevel.Information);
        }
    }
}
