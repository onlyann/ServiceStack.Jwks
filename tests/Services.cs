using System;
using ServiceStack;

namespace ServiceStack.Jwks.Tests {

    [Authenticate]
    public class HelloService : Service {
        public object Any(Hello request) {
            var session = this.GetSession();
            return new HelloResponse { Result = $"Hello, {session.DisplayName}!" };
        }
    }

    public class HelloResponse {
        public string Result { get; set; }
    }

    public class Hello : IReturn<HelloResponse> { }
}