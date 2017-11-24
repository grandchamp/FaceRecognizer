using FaceRecognizer.Bus.RabbitMQ;
using FaceRecognizer.Core.Configuration;
using FaceRecognizer.Core.Repositories.Contracts;
using FaceRecognizer.Core.Services;
using FaceRecognizer.Core.Services.Contracts;
using FaceRecognizer.Data.Repositories;
using Hangfire;
using Hangfire.PostgreSql;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
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
            services.AddScoped<IPersonRepository, PersonRepository>();

            services.AddScoped<IDocumentStore>(provider => DocumentStore.For(Configuration.GetSection("ConnectionStrings:Marten").GetValue<string>("FaceDB")));
            services.AddScoped(provider => provider.GetRequiredService<IDocumentStore>().OpenSession());

            services.AddHangfire(configuration => configuration.UsePostgreSqlStorage(Configuration.GetSection("ConnectionStrings:Hangfire").GetValue<string>("HangfireDB")));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHangfireServer();
            app.UseHangfireDashboard();

            AssemblyLoadContext.Default.LoadFromAssemblyPath(FindFileInPath(env.ContentRootPath ?? env.WebRootPath));

            BackgroundJob.Enqueue(() => app.ApplicationServices.GetService<RabbitListener>().StartExtractFaces());
            BackgroundJob.Enqueue(() => app.ApplicationServices.GetService<RabbitListener>().StartTrainFaceRecognition());
        }

        private string FindFileInPath(string path) => Directory.GetFiles(path, "Emgu.CV.World.NetStandard1_4.dll", SearchOption.AllDirectories).FirstOrDefault();
    }
}
