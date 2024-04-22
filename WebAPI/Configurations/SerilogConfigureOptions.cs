using Serilog;
using Serilog.Events;

namespace HM.WebAPI.Configurations;

public class SerilogConfigureOptions(IConfiguration configuration)
{
    public void Configure(LoggerConfiguration options)
    {
        options.MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .ReadFrom.Configuration(configuration)
            .WriteTo.File("./logs/log.txt", LogEventLevel.Information, rollingInterval: RollingInterval.Day)
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug);
    }
}
