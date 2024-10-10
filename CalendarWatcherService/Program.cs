using CalendarWatcherService.Options;
using System.Globalization;

namespace CalendarWatcherService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<CalendarWatcherService>();
                    services.Configure<AppSettings>(hostContext.Configuration.GetSection(nameof(AppSettings)));
                    services.AddLogging(builder =>
                        builder
                            .AddDebug()
                            .AddConsole()
                            .AddConfiguration(hostContext.Configuration.GetSection("Logging"))
                            .SetMinimumLevel(LogLevel.Information));
                })
                .Build();

            host.Run();
        }
    }
}