using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funq;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using ServiceStack.Auth;
using ServiceStack.Text;

namespace ServiceStack.Jwks.Tests {

    public class AppHostR512WithFallbackKeys : AppHostBase {
        public AppHostR512WithFallbackKeys(): base("Test App Host", typeof(AppHostR512WithFallbackKeys).Assembly) { }

        public override void Configure(Container container) {
            container.Register<IUserAuthRepository>(new InMemoryAuthRepository());

            var authFeature = new AuthFeature(()=> new AuthUserSession(),
                new IAuthProvider[] {
                    new JwtAuthProvider {
                        PrivateKeyXml = AppSettings.Get<string>("jwt.RS512.PrivateKeyXml"),
                            FallbackPublicKeys = AppSettings.Get<List<string>>("jwt.RS512.FallbackKeys").Select(x => x.ToPublicRSAParameters()).ToList(),
                            HashAlgorithm = "RS512"
                    }
                });

            authFeature.RegisterPlugins.Add(new JwksFeature());
            Plugins.Add(authFeature);
        }
    }

    [TestFixture]
    public class JwksRS512WithFallbackKeysTests : BaseTests {
        protected override TestServer CreateTestServer()=> WebHostUtils.CreateTestServer<AppHostR512WithFallbackKeys>();

        [Test]
        public void Jwks_returns_JsonWebKeySets() {
            var response = client.Send(new GetJsonWebKeySet());
            response.PrintDump();

            Assert.That(response.Keys, Has.Exactly(3).Items);

            var expectedJwks = File.ReadAllText("content/expected_jwks_RS512.json").FromJson<JsonWebKeySetResponse>();
            foreach (var expectedKey in expectedJwks.Keys) {
                var key = response.Keys.FirstOrDefault(x => x.KeyId == expectedKey.KeyId);
                Assert.That(key.ToJson(), Is.EqualTo(expectedKey.ToJson()));
            }
        }
    }
}