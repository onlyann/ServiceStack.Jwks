# ServiceStack.Jwks

[![Build status](https://ci.appveyor.com/api/projects/status/4hfmc0b4fcq87b3y/branch/master?svg=true)](https://ci.appveyor.com/project/onlyann/servicestack-jwks/branch/master)
[![Nuget](https://img.shields.io/nuget/v/ServiceStack.Jwks.svg)](https://www.nuget.org/packages/ServiceStack.Jwks/)

A ServiceStack v5 plugin to expose and consume [Json Web Key](https://tools.ietf.org/html/rfc7517) sets using a subset of the [OpenID Connect discovery document](https://openid.net/specs/openid-connect-discovery-1_0.html).

Potential use cases:
-  simplify [JSON Web token](http://docs.servicestack.net/jwt-authprovider) key rotation between ServiceStack services
-  protect a [stateless ServiceStack service](http://docs.servicestack.net/jwt-authprovider#services-only-validating-tokens) with a third-party authentication service that supports Open ID connect - Azure AD, Auth0, Okta, ...
-  protect an ASP.NET Core app with the [Microsoft.AspNetCore.Authentication.JwtBearer](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer/) middleware using the OpenID discovery document from a ServiceStack Authentication service.

## Getting Started

Add the `ServiceStack.Jwks` Nuget package:
```
dotnet add package ServiceStack.Jwks --version 1.0.0
```

### Authentication service

Register `JwksFeature` in the `AuthFeature`:

```cs
// existing Auth feature using the JwtAuthProvider
var authFeature = new AuthFeature(...);

authFeature.RegisterPlugins.Add(new JwksFeature());
```

The Discovery document is now accessible at `/openid-config` and the JSON Web key set at `/jwks`.

### Protected ServiceStack service

Register `JwksFeature` in the `AuthFeature`:

```cs
// existing Auth feature using the JwtAuthProviderReader
var authFeature = new AuthFeature(...);

authFeature.RegisterPlugins.Add(new JwksFeature() {
    OpenIdDiscoveryUrl = "https://myauthapi.example.com/openid-config"
    // or JwksUrl = "https://myauthapi.example.com/jwks"
});
```

### Protected ASP.NET Core service

```cs
public class StartUp {
    public void ConfigureServices(IServiceCollection services) {
        ...
        services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options => {
            // must match the configured audience on the ServiceStack Auth service
            options.Audience = "my-audience"; 
            // ServiceStack Auth service discovery url
            options.MetadataAddress = "https://myauthapi.example.com/openid-config" 
            // optional to map the Identity Name property to the `name` claim used by ServiceStack.
            options.TokenValidationParameters.NameClaimType = "name"; 
        });
    }

    public void Configure(IApplicationBuilder app) {
        ...
        // authenticate the user in the presence of a JWT Bearer token
        app.UseAuthentication(); 
        ...
    }
}
```

## Notes

Supported algorithms are the Asymetric RSA algorithms (RS256, RS384, RS512).

The metadata isn't technically valid according to [OpenID connect metadata specifications](https://openid.net/specs/openid-connect-discovery-1_0.html#ProviderMetadata).  
ServiceStack isn't an OpenID provider and the metadata is only used to expose information about the `JWTAuthProvider`.

   

