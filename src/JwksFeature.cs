using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ServiceStack.Auth;
using ServiceStack.Text;

namespace ServiceStack.Jwks {
    public class JwksFeature : IPlugin {
        public string JwksUrl { get; set; }

        public string OpenIdDiscoveryUrl { get; set; }

        public IRestClient JwksClient { get; set; } = new JsonServiceClient();

        protected JwtAuthProviderReader JwtAuthProvider { get; private set; }

        public void Register(IAppHost appHost) {
            JwtAuthProvider = AuthenticateService.GetJwtAuthProvider();
            if (JwtAuthProvider is JwtAuthProvider) {
                appHost.RegisterService(typeof(JwksService));
            } else if (JwtAuthProvider != null) {
                var keySet = RetrieveKeySet();
                LoadKeySet(keySet);
            }
        }

        protected virtual JsonWebKeySetResponse RetrieveKeySet() {
            if (!string.IsNullOrEmpty(OpenIdDiscoveryUrl)) {
                var discoveryDoc = JwksClient.Get<OpenIdDiscoveryDocument>(OpenIdDiscoveryUrl);
                JwtAuthProvider.Issuer = discoveryDoc.Issuer;
                JwksUrl = discoveryDoc.JwksUri;
            }

            if (string.IsNullOrEmpty(JwksUrl)) {
                throw new NotSupportedException($"Missing {nameof(JwksUrl)} {JwksUrl}");
            }

            return JwksClient.Get<JsonWebKeySetResponse>(JwksUrl);
        }

        protected virtual void LoadKeySet(JsonWebKeySetResponse keySet) {
            if (keySet?.Keys.IsEmpty()?? true) {
                throw new NotSupportedException($"Expecting at least one key from keyset {keySet.Dump()}");
            }

            if (JwtAuthProvider.RequireHashAlgorithm) {
                // infer the algorithm if it is described by a key from the set
                var algo = keySet.Keys.FirstOrDefault(x => !string.IsNullOrEmpty(x.Algorithm))? .Algorithm;
                if (algo != null) {
                    JwtAuthProvider.HashAlgorithm = algo;
                }
            }

            var key = keySet.Keys.First();

            JwtAuthProvider.PublicKey = ToRsaParameters(key);
            JwtAuthProvider.KeyId = key.KeyId;

            JwtAuthProvider.FallbackPublicKeys = keySet.Keys.Skip(1)
                .Select(x => ToRsaParameters(x).Value)
                .ToList();
        }

        static RSAParameters? ToRsaParameters(JsonWebKey key) {
            if (key == null)return null;

            return new RSAParameters {
                Exponent = key.Exponent.FromBase64UrlSafe(),
                    Modulus = key.Modulus.FromBase64UrlSafe()
            };
        }
    }
}