using System;
using System.IO;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Funq;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using ServiceStack.Auth;

namespace ServiceStack.Jwks.Tests {

    public class AppHostJwksReader : AppHostBase {
        public AppHostJwksReader(): base("Test App Host", typeof(AppHostJwksReader).Assembly) { }

        public override void Configure(Container container) {
            SetConfig(new HostConfig { DebugMode = true });

            var authFeature = new AuthFeature(
                ()=> new AuthUserSession(),
                new IAuthProvider[] { new JwtAuthProviderReader(AppSettings)});

            var jwksUrl = "https://server.example.com/jwks";

            var stubClient = new JsonHttpClient();
            stubClient.ResultsFilter = (responseType, httpMethod, requestUri, request)=> {
                if (requestUri == jwksUrl) {
                    return File.ReadAllText("content/expected_jwks_RS512.json").FromJson<JsonWebKeySetResponse>();
                }
                return null;
            };

            authFeature.RegisterPlugins.Add(new JwksFeature() {
                JwksUrl = jwksUrl,
                    JwksClient = stubClient
            });

            Plugins.Add(authFeature);
        }
    }

    [TestFixture]
    public class JwksReaderTests : JwksReaderBaseTests {
        protected override TestServer CreateTestServer()=> WebHostUtils.CreateTestServer<AppHostJwksReader>(configuration);

        [Test]
        public override void No_token_returns_401()=> base.No_token_returns_401();

        [Test]
        public override void Valid_jwt_is_accepted()=> base.Valid_jwt_is_accepted();

        [Test]
        public override void Invalid_jwt_is_rejected()=> base.Invalid_jwt_is_rejected();
    }
}