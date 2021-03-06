﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MonolithToMicroservices.Repository;
using Swashbuckle.Swagger.Model;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace MonolithToMicroservices
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //Add SqLite support
            services.AddDbContext<CustomersDbContext>(options => {
                options.UseSqlite(Configuration.GetConnectionString("CustomersSqliteConnectionString"));
            });

            services.AddMvc();

            services.AddScoped<ICustomersRepository, CustomersRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddScoped<IOrdersRepository, OrdersRepository>();
            services.AddTransient<CustomersDbSeeder>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory, 
            CustomersDbSeeder customersDbSeeder)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Serve wwwroot as root
            app.UseFileServer();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                //https://github.com/aspnet/JavaScriptServices/blob/dev/samples/angular/MusicStore/Startup.cs
                routes.MapSpaFallbackRoute("spa-fallback", new { controller = "Home", action = "Index" });

            });

            customersDbSeeder.SeedAsync(app.ApplicationServices).Wait();
        }
    }
}
