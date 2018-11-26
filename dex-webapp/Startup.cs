using dex_webapp.Data;
using dex_webapp.Services;
using dex_webapp.Services.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace dex_webapp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                  options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.Configure<EthereumSettings>(Configuration.GetSection(nameof(EthereumSettings)));
            services.Configure<AbiLocation>(Configuration.GetSection(nameof(AbiLocation)));
            services.Configure<EtherscanSettings>(Configuration.GetSection(nameof(EtherscanSettings)));
            services.Configure<BackgroundScannerSettings>(Configuration.GetSection(nameof(BackgroundScannerSettings)));

            services.AddTransient<AbiProvider>();
            services.AddTransient<ParametersService>();
            services.AddSingleton<EtherscanService>();

            services.AddTransient<IOHCLService, OHCLService>();
            services.AddTransient<ITradesService, TradesService>();
            services.AddTransient<IOrdersService, OrdersService>();
            services.AddTransient<IEthereumService, EthereumService>();
            services.AddSingleton<IHostedService, BackgroundScanningService>();

            services.AddSignalR();
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            UpdateDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true
            });
            // Add support for node_modules but only during development **temporary**
            //if (env.IsDevelopment())
            //{
            //    app.UseStaticFiles(new StaticFileOptions()
            //    {
            //        FileProvider = new PhysicalFileProvider(
            //            Path.Combine(Directory.GetCurrentDirectory(), @"node_modules")),
            //        RequestPath = new PathString("/vendor")
            //    });
            //}
            app.UseCookiePolicy();

            var fordwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            fordwardedHeaderOptions.KnownNetworks.Clear();
            fordwardedHeaderOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(fordwardedHeaderOptions);

            app.UseSignalR(routes =>
            {
                routes.MapHub<OrderSignalR>("/sr/chart");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}
