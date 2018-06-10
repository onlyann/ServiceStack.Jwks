using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NUnit.Framework;

namespace ServiceStack.Jwks.Tests {

    public class OpenIdConnectAuthStartup {
        public IConfiguration Configuration { get; }
        public OpenIdConnectAuthStartup(IConfiguration configuration)=> Configuration = configuration;
        public void ConfigureServices(IServiceCollection services) {
            services.AddLogging(builder => {
                builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddFilter("Microsoft", LogLevel.Debug)
                    .AddTestLogger();
            });

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.Audience = "test-audience";
                options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    "content/sample_openid_discovery_RS256.json",
                    new OpenIdConnectConfigurationRetriever(),
                    new FileDocumentRetriever());
                options.TokenValidationParameters.NameClaimType = "name";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            app.UseAuthentication();
            app.Run(context => {
                context.Response.StatusCode = context.User.Identity.IsAuthenticated ? 204 : 401;
                return Task.CompletedTask;
            });
        }
    }

    [TestFixture]
    public class OpenIdConnectAuthTests : BaseTests {
        protected override TestServer CreateTestServer() {
            var hostBuilder = new WebHostBuilder()
                .UseStartup<OpenIdConnectAuthStartup>()
                .UseConfiguration(configuration);

            return new TestServer(hostBuilder);
        }

        [Test]
        public async Task No_token_results_in_401() {
            var response = await httpClient.GetAsync("/");
            Assert.That(response.StatusCode.Is(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Valid_token_results_in_204() {
            var token = CreateJwt(configuration["jwt.RS256.PrivateKeyXml"].ToPrivateRSAParameters(), "RS256", "test-audience");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync("/");
            Assert.That(response.StatusCode.Is(HttpStatusCode.NoContent));
        }
    }
}