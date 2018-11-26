using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dex_webapp.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace dex_webapp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.MigrateAsync().Wait();
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                            .ConfigureAppConfiguration((webHostBuilderContext, configurationbuilder) =>
                            {
                                var environment = webHostBuilderContext.HostingEnvironment;
                                string pathOfCommonSettingsFile = Path.Combine(environment.ContentRootPath);
                                configurationbuilder
                                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true);
                                configurationbuilder.AddEnvironmentVariables();
                            })
                            .UseStartup<Startup>()
                            .UseUrls("http://localhost:4434/");
    }
}
