using System.Runtime.Serialization;

namespace ServiceStack.Jwks {

    [DataContract]
    public class OpenIdDiscoveryDocument {

        [DataMember(Name = "jwks_uri")]
        public string JwksUri { get; set; }
    }
}