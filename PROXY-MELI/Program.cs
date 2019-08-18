using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PROXY_MELI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try { 

            CreateWebHostBuilder(args).Build().Run();
            }
            catch(Exception ex)
            {

            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:5001")
                .UseIISIntegration()
                .UseStartup<Startup>()
            ;
    }
}
