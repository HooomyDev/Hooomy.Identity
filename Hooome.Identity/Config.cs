using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Hooome.Identity;

public class Config
{
    public static IEnumerable<ApiScope> ApiScopes
        => [new ApiScope("HooomeWebApi", "Web API")];

    public static IEnumerable<IdentityResource> IdentityResources
        => [new IdentityResources.OpenId(), new IdentityResources.Profile()];

    public static IEnumerable<ApiResource> ApiResources =>
            [
                new ApiResource("HooomeWebApi", "Web API", [JwtClaimTypes.Name])
                {
                    Scopes = { "HooomeWebApi" }
                }
            ];

    public static IEnumerable<Client> Clients =>
    [
        new Client
        {
            ClientId = "hooome-web-api",
            ClientName = "Hooome React SPA",
            AllowedGrantTypes = GrantTypes.Code,
            RequireClientSecret = false,
            RequirePkce = true,

            RedirectUris =
            {
                "http://localhost:3000/signin-oidc"
            },
            PostLogoutRedirectUris =
            {
                "http://localhost:3000/signout-oidc"
            },
            AllowedCorsOrigins =
            {
                "http://localhost:3000"
            },

            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                "HooomeWebApi"
            },

            AllowAccessTokensViaBrowser = true
        }
    ];

}