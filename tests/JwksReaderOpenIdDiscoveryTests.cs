using System.IO;
using Funq;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using ServiceStack.Auth;

namespace ServiceStack.Jwks.Tests {

    public class AppHostOpenIdDiscoveryJwksReader : AppHostBase {
        public AppHostOpenIdDiscoveryJwksReader(): base("Test App Host", typeof(AppHostOpenIdDiscoveryJwksReader).Assembly) { }

        public override void Configure(Container container) {
            SetConfig(new HostConfig { DebugMode = true });

            var authFeature = new AuthFeature(
                ()=> new AuthUserSession(),
                new IAuthProvider[] { new JwtAuthProviderReader(AppSettings)});

            var openIdDiscoveryUrl = "https://someserver/open_id_discovery_doc";

            var stubClient = new JsonHttpClient();
            stubClient.ResultsFilter = (responseType, httpMethod, requestUri, request)=> {
                if (requestUri == openIdDiscoveryUrl) {
                    return File.ReadAllText("expected_openid_discovery_document.json").FromJson<OpenIdDiscoveryDocument>();
                }

                if (requestUri == "https://server.example.com/jwks.json") {
                    return File.ReadAllText("expected_jwks_RS512.json").FromJson<JsonWebKeySetResponse>();
                }
                return null;
            };

            authFeature.RegisterPlugins.Add(new JwksFeature() {
                OpenIdDiscoveryUrl = openIdDiscoveryUrl,
                    JwksClient = stubClient
            });

            Plugins.Add(authFeature);
        }
    }

    [TestFixture]
    public class JwksReaderOpenIdDiscoveryTests : JwksReaderTests {

        protected override TestServer CreateTestServer()=> WebHostUtils.CreateTestServer<AppHostOpenIdDiscoveryJwksReader>(configuration);
    }
}