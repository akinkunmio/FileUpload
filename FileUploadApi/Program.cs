using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;

namespace FileUploadApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            // var logger = host.Services.GetRequiredService<ILogger<Program>>();
            // logger.LogInformation("Qb Upload Service application has started!.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.Udp(configuration["Logging:LogStash:LogstashAddress"], Convert.ToInt32(configuration["Logging:LogStash:LogstashPort"]), new CompactJsonFormatter())
                .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                .CreateLogger();

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
               return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((config) => { config.ClearProviders(); })
                .UseSerilog()
                .UseStartup<Startup>();
        }
    }
}
