using HappyTravel.StdOutLogger.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HappyTravel.TokenReceiver.Api.Environments;

namespace HappyTravel.TokenReceiver.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }


        public static IWebHostBuilder CreateHostBuilder(string[] args)
            => WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var environment = hostingContext.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", false, true)
                        .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders()
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

                    var env = hostingContext.HostingEnvironment;
                    if (env.IsLocal())
                        logging.AddConsole();
                    else
                        logging
                            .AddStdOutLogger(options =>
                            {
                                options.IncludeScopes = false;
                                options.RequestIdHeader = "x-request-id";
                                options.UseUtcTimestamp = true;
                            })
                            .AddSentry(options => { options.Dsn = EnvironmentVariableHelper.Get("Logging:Sentry:Endpoint", hostingContext.Configuration); });
                })
                .UseSetting(WebHostDefaults.SuppressStatusMessagesKey, "true");
    }
}