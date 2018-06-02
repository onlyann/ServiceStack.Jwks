using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace ServiceStack.Jwks.Tests {

    public abstract class BaseTests {
        protected JsonHttpClient client;
        protected TestServer server;
        protected IConfiguration configuration;

        [OneTimeSetUp]
        public void Setup() {
            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            server = CreateTestServer();
        }

        protected abstract TestServer CreateTestServer();

        [SetUp]
        public void BeforeTests() {
            client = new JsonHttpClient("https://localhost") {
                HttpClient = server.CreateClient()
            };
        }

        [OneTimeTearDown]
        public void TearDown() {
            server?.Dispose();
        }
    }
}