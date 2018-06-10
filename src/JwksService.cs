using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ServiceStack.Auth;

namespace ServiceStack.Jwks {
    public class JwksService : Service {
        public object Any(GetJsonWebKeySet request) {
            var jwtProvider = AuthenticateService.GetJwtAuthProvider();
            if (!JwtAuthProviderReader.RsaSignAlgorithms.ContainsKey(jwtProvider.HashAlgorithm)) {
                return HttpError.MethodNotAllowed("Non RSA algorithms are not supported");
            }

            var keys = new List<JsonWebKey>();
            var publicKey = jwtProvider.GetPublicKey(Request);
            var alg = jwtProvider.HashAlgorithm;
            var fallbackKeys = jwtProvider.GetFallbackPublicKeys(Request);

            if (publicKey != null) {
                keys.Add(CreateJWKey(publicKey.Value, alg, jwtProvider.GetKeyId(Request)));
            }

            keys.AddRange(fallbackKeys.Select(key => CreateJWKey(key, alg)));

            return new JsonWebKeySetResponse {
                Keys = keys
            };
        }

        public object Any(GetOpenIdDiscoveryDocument request) {
            var jwtProvider = AuthenticateService.GetJwtAuthProvider();
            return new OpenIdDiscoveryDocument {
                JwksUri = new GetJsonWebKeySet().ToAbsoluteUri(Request),
                    Issuer = jwtProvider.Issuer
            };
        }

        static JsonWebKey CreateJWKey(
            RSAParameters publicKey,
            string algorithm,
            string keyId = null,
            string keyType = "RSA")=> new JsonWebKey {
            KeyType = keyType,
            Algorithm = algorithm,
            PublicKeyUse = KeyUse.Signature,
            Exponent = publicKey.Exponent.ToBase64UrlSafe(),
            Modulus = publicKey.Modulus.ToBase64UrlSafe(),
            KeyId = keyId ?? publicKey.KeyId()
        };
    }

    public static class JwtUtils {
        public static string KeyId(this RSAParameters publicKey)=> publicKey.Modulus.ToBase64UrlSafe().Substring(0, 3);
    }
}