using FaceRecognizer.Bus.RabbitMQ;
using FaceRecognizer.Core.Configuration;
using FaceRecognizer.Core.Services;
using FaceRecognizer.Core.Services.Contracts;
using Hangfire;
using Hangfire.PostgreSql;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace FaceRecognizer.BusHandler
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RabbitConfiguration>(Configuration.GetSection("RabbitMQ"));
            services.Configure<EmguCVConfiguration>(Configuration.GetSection("EmguCV"));

            services.AddSingleton<RabbitConnection>();

            services.AddSingleton<RabbitListener>();
            services.AddScoped<IRabbitService, RabbitService>();
            services.AddScoped<IFaceService, FaceService>();

            services.AddScoped<IDocumentStore>(provider => DocumentStore.For(Configuration.GetSection("ConnectionStrings:Marten").GetValue<string>("FaceDB")));
            services.AddScoped(provider => provider.GetRequiredService<IDocumentStore>().OpenSession());

            services.AddHangfire(configuration => configuration.UsePostgreSqlStorage(Configuration.GetSection("ConnectionStrings:Hangfire").GetValue<string>("HangfireDB")));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHangfireServer();
            app.UseHangfireDashboard();

            AssemblyLoadContext.Default.LoadFromAssemblyPath(@"C:\Users\nicolas.grandchamp\Source\Repos\FaceRecognizer2\src\FaceRecognizer.BusHandler\bin\Debug\netcoreapp2.0\Emgu.CV.World.NetStandard1_4.dll");

            BackgroundJob.Enqueue(() => app.ApplicationServices.GetService<RabbitListener>().StartExtractFaces());
        }
    }
}
