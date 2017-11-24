using FaceRecognizer.Bus.RabbitMQ;
using FaceRecognizer.Core.Repositories.Contracts;
using FaceRecognizer.Data.Repositories;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FaceRecognizer.API
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

            services.AddSingleton<RabbitConnection>();
            services.AddScoped<IRabbitService, RabbitService>();
            services.AddScoped<IPersonRepository, PersonRepository>();

            services.AddMediatR(typeof(Core.Commands.ExtractFacesCommand).GetTypeInfo().Assembly,
                                typeof(Bus.RabbitMQ.Handlers.ExtractFacesCommandHandler).GetTypeInfo().Assembly);

            services.AddMvc();

            services.AddScoped<IDocumentStore>(provider => DocumentStore.For(Configuration.GetSection("ConnectionStrings:Marten").GetValue<string>("FaceDB")));
            services.AddScoped(provider => provider.GetRequiredService<IDocumentStore>().OpenSession());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Home/Error");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
