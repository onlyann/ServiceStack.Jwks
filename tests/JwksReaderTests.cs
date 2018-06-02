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

            var jwksUrl = "https://someserver/jwks";

            var stubClient = new JsonHttpClient();
            stubClient.ResultsFilter = (responseType, httpMethod, requestUri, request)=> {
                if (requestUri == jwksUrl) {
                    return File.ReadAllText("expected_jwks_RS512.json").FromJson<JsonWebKeySetResponse>();
                }
                return null;
            };

            authFeature.RegisterPlugins.Add(new JwksFeature() {
                JwksUrl = "https://someserver/jwks",
                    JwksClient = stubClient
            });

            Plugins.Add(authFeature);
        }
    }

    [TestFixture]
    public class JwksReaderTests : BaseTests {
        protected override TestServer CreateTestServer()=> WebHostUtils.CreateTestServer<AppHostJwksReader>(configuration);

        [Test]
        public void No_token_returns_401() {
            Assert.That(
                ()=> client.Send(new Hello()),
                Throws.TypeOf<WebServiceException>()
                .With.Matches<WebServiceException>(ex => ex.StatusCode == 401));
        }

        [Test]
        public void Valid_jwt_is_accepted() {
            var token = CreateJwt(configuration["jwt.RS512.PrivateKeyXml"].ToPrivateRSAParameters());
            client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = client.Send(new Hello());
            Assert.That(response.Result, Is.EqualTo("Hello, Test!"));
        }

        [Test]
        public void Invalid_jwt_is_rejected() {
            var token = CreateJwt(RsaUtils.CreatePrivateKeyParams());
            client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Assert.That(
                ()=> client.Send(new Hello()),
                Throws.TypeOf<WebServiceException>()
                .With.Matches<WebServiceException>(ex => ex.StatusCode == 401));
        }

        private string CreateJwt(RSAParameters privateKey) {
            var header = JwtAuthProvider.CreateJwtHeader("RS512");
            var payload = JwtAuthProvider.CreateJwtPayload(new AuthUserSession {
                UserAuthId = "1",
                    DisplayName = "Test",
                    Email = "test@example.com"
            }, "my-jwt", TimeSpan.FromDays(7));

            return JwtAuthProvider.CreateJwt(header, payload,
                data => RsaUtils.Authenticate(data, privateKey, "SHA512", RsaKeyLengths.Bit2048));
        }
    }
}