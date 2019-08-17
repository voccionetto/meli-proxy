using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PROXY_MELI
{
    using System.Net.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using MongoDB.Driver;
    using PROXY_MELI_DATABASE.Mongo;
    using ReverseProxy;

    public class Startup
    {
        private IConfiguration Configuration { get; }

        private IHostingEnvironment Environment { get; }

        public Startup(IHostingEnvironment environment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true)
                .AddJsonFile($"resources.{environment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging().AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddOptions();
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.Configure<ProxyMeliMongoDatabaseSettings>(
                Configuration.GetSection(nameof(ProxyMeliMongoDatabaseSettings)));

            services.AddSingleton<IProxyMeliMongoDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<ProxyMeliMongoDatabaseSettings>>().Value);

            services.AddSingleton<HttpClient>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //loggerFactory.AddLog4Net($"log4net.{env.EnvironmentName}.config");

            app.UseMiddleware<ReverseProxyMiddleware>();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("<p>PROXY - MELI <br> by Netto Voccio ;)</p>");
            });
        }
    }
}
