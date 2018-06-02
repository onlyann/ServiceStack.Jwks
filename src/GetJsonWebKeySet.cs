using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;

namespace ServiceStack.Jwks {

    [Route("/jwks")]
    public class GetJsonWebKeySet : IGet, IPost, IReturn<JsonWebKeySetResponse> { }

    [DataContract]
    public class JsonWebKeySetResponse {

        [DataMember(Name = "keys")]
        public List<JsonWebKey> Keys { get; set; }
    }
}