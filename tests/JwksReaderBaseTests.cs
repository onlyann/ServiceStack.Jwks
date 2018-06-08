using System;
using System.IO;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Funq;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using ServiceStack.Auth;

namespace ServiceStack.Jwks.Tests {
    public abstract class JwksReaderBaseTests : BaseTests {
        public virtual void No_token_returns_401() {
            Assert.That(
                ()=> client.Send(new Hello()),
                Throws.TypeOf<WebServiceException>()
                .With.Matches<WebServiceException>(ex => ex.StatusCode == 401));
        }

        public virtual void Valid_jwt_is_accepted() {
            var token = CreateJwt(configuration["jwt.RS512.PrivateKeyXml"].ToPrivateRSAParameters());
            client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = client.Send(new Hello());
            Assert.That(response.Result, Is.EqualTo("Hello, Test!"));
        }

        public virtual void Invalid_jwt_is_rejected() {
            var token = CreateJwt(RsaUtils.CreatePrivateKeyParams());
            client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Assert.That(
                ()=> client.Send(new Hello()),
                Throws.TypeOf<WebServiceException>()
                .With.Matches<WebServiceException>(ex => ex.StatusCode == 401));
        }

        protected string CreateJwt(RSAParameters privateKey) {
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