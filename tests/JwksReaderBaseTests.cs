using System.Net.Http.Headers;
using NUnit.Framework;

namespace ServiceStack.Jwks.Tests {
    public abstract class JwksReaderBaseTests : BaseTests {
        public virtual void No_token_returns_401() {
            Assert.That(
                ()=> client.Send(new Hello()),
                Throws.TypeOf<WebServiceException>()
                .With.Matches<WebServiceException>(ex => ex.StatusCode == 401));
        }

        public virtual void Valid_jwt_is_accepted() {
            var token = CreateJwt(configuration["jwt.RS512.PrivateKeyXml"].ToPrivateRSAParameters(), "RS512");
            client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = client.Send(new Hello());
            Assert.That(response.Result, Is.EqualTo("Hello, Test!"));
        }

        public virtual void Invalid_jwt_is_rejected() {
            var token = CreateJwt(RsaUtils.CreatePrivateKeyParams(), "RS512");
            client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Assert.That(
                ()=> client.Send(new Hello()),
                Throws.TypeOf<WebServiceException>()
                .With.Matches<WebServiceException>(ex => ex.StatusCode == 401));
        }
    }
}