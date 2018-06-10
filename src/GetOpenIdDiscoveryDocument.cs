using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;

namespace ServiceStack.Jwks {

    [Route("/openid-config")]
    public class GetOpenIdDiscoveryDocument : IGet, IPost, IReturn<OpenIdDiscoveryDocument> { }
}