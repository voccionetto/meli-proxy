using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PROXY_MELI_DATABASE.Mongo;
using StackExchange.Redis;

namespace PROXY_MELI_API
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
            services.Configure<ProxyMeliMongoDatabaseSettings>(
        Configuration.GetSection(nameof(ProxyMeliMongoDatabaseSettings)));

            services.AddSingleton<IProxyMeliMongoDatabaseSettings>(sp =>
        sp.GetRequiredService<IOptions<ProxyMeliMongoDatabaseSettings>>().Value);

            services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(Configuration["RedisSection:RedisConnection"]));

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration["RedisSection:RedisConnection"];
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
