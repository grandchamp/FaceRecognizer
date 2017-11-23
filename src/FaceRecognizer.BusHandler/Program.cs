using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;

namespace FaceRecognizer.BusHandler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                                        .AddEnvironmentVariables()
                                        .Build();

            Log.Logger = new LoggerConfiguration()
                                .ReadFrom.Configuration(configuration)
                                .Enrich.FromLogContext()
                                .WriteTo.Console()
                                .CreateLogger();

            WebHost.CreateDefaultBuilder(args)
                   .UseUrls("http://[::]:5000")
                   .UseConfiguration(configuration)
                   .UseDefaultServiceProvider(options => options.ValidateScopes = false)
                   .UseSerilog()
                   .UseStartup<Startup>()
                   .Build()
                   .Run();
        }
    }
}
