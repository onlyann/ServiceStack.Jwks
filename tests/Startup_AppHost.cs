using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ServiceStack.Jwks.Tests {
    public class Startup<TAppHost> where TAppHost : AppHostBase, new() {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)=> Configuration = configuration;

        public TAppHost AppHost { get; private set; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddSingleton<TAppHost>(x => new TAppHost {
                AppSettings = new NetCoreAppSettings(Configuration)
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            AppHost = app.ApplicationServices.GetRequiredService<TAppHost>();
            app.UseServiceStack(AppHost);
        }
    }

    public static class WebHostUtils {
        public static TestServer CreateTestServer<TAppHost>(IConfiguration config = null)where TAppHost : AppHostBase, new() {
            if (config == null) {
                config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .Build();
            }

            var hostBuilder = new WebHostBuilder()
                .UseStartup<Startup<TAppHost>>()
                .UseConfiguration(config);

            return new TestServer(hostBuilder);
        }
    }
}