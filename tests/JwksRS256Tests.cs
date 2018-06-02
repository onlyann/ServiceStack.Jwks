using System.IO;
using System.Linq;
using Funq;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using ServiceStack.Auth;
using ServiceStack.Text;

namespace ServiceStack.Jwks.Tests {

    public class AppHostRS256 : AppHostBase {
        public AppHostRS256(): base("Test App Host", typeof(AppHostRS256).Assembly) { }

        public override void Configure(Container container) {
            container.Register<IUserAuthRepository>(new InMemoryAuthRepository());

            var authFeature = new AuthFeature(()=> new AuthUserSession(),
                new IAuthProvider[] {
                    new JwtAuthProvider {
                        PrivateKeyXml = AppSettings.Get<string>("jwt.RS256.PrivateKeyXml"),
                            HashAlgorithm = "RS256"
                    }
                });

            authFeature.RegisterPlugins.Add(new JwksFeature());
            Plugins.Add(authFeature);
        }
    }

    [TestFixture]
    public class JwksProviderRS256 : BaseTests {
        protected override TestServer CreateTestServer()=> WebHostUtils.CreateTestServer<AppHostRS256>();

        [Test]
        public void Jwks_returns_JsonWebKeySets() {

            var response = client.Send(new GetJsonWebKeySet());
            response.PrintDump();

            Assert.That(response.Keys, Has.Exactly(1).Items);
            var key = response.Keys.First();

            var expectedJwks = File.ReadAllText("expected_jwks_RS256.json").FromJson<JsonWebKeySetResponse>();
            var expectedKey = expectedJwks.Keys.First();
            Assert.That(key.ToJson(), Is.EqualTo(expectedKey.ToJson()));
        }
    }
}