using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Hooome.Identity;

public class Config
{
    public static IEnumerable<ApiScope> ApiScopes =>
        [new ApiScope("HooomeWebApi", "Web API")];

    public static IEnumerable<IdentityResource> IdentityResources => [new IdentityResources.OpenId(), new IdentityResources.Profile()];

    public static IEnumerable<ApiResource> ApiResources =>
        [
            new ApiResource("HooomeWebApi", "Web API", new[] { JwtClaimTypes.Name })
            {
                Scopes = { "HooomeWebApi" }
            }
        ];

    public static IEnumerable<Client> Clients =>
        [
            // основной клиент для SPA (Code + PKCE)
            new Client
            {
                ClientId = "react-client",
                ClientName = "React Frontend",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = { "http://localhost:3000/callback" },
                PostLogoutRedirectUris = { "http://localhost:3000" },
                AllowedCorsOrigins = { "http://localhost:3000" },
                AllowedScopes = { "openid", "profile", "HooomeWebApi" },
                AllowAccessTokensViaBrowser = true
            },

            new Client
            {
                ClientId = "react-password-client",
                ClientName = "React Password Flow",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                RequireClientSecret = false,
                AllowedScopes = { "openid", "profile", "HooomeWebApi", "offline_access" },
                AllowAccessTokensViaBrowser = true,
                AllowOfflineAccess = true
            }
        ];
}
