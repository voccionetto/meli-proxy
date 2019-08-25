using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PROXY_MELI_WEB.Models;

namespace PROXY_MELI_WEB
{
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging().AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddOptions();

            services.Configure<ApiCaller>(
        Configuration.GetSection(nameof(ApiCaller)));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            loggerFactory.AddLog4Net($"log4net.{env.EnvironmentName}.config");

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
