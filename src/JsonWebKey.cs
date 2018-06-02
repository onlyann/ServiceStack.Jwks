using System.Runtime.Serialization;

namespace ServiceStack.Jwks {

    [DataContract]
    public class JsonWebKey {

        [DataMember(Name = "kty")]
        public string KeyType { get; set; }

        [DataMember(Name = "alg")]
        public string Algorithm { get; set; }

        [DataMember(Name = "e")]
        public string Exponent { get; set; }

        [DataMember(Name = "n")]
        public string Modulus { get; set; }

        [DataMember(Name = "kid")]
        public string KeyId { get; set; }

        [DataMember(Name = "use")]
        public KeyUse PublicKeyUse { get; set; }
    }

    [DataContract]
    public enum KeyUse {
        [EnumMember(Value = "sig")]
        Signature,

        [EnumMember(Value = "enc")]
        Encryption
    }
}