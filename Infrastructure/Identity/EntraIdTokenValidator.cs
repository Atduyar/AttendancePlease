using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity;

public class EntraIdTokenValidator
{
    private readonly IConfiguration _configuration;
    private readonly HttpDocumentRetriever _documentRetriever;

    public EntraIdTokenValidator(IConfiguration configuration)
    {
        _configuration = configuration;
        _documentRetriever = new HttpDocumentRetriever { RequireHttps = true };
    }

    /// <summary>
    /// Validates an Entra ID access token and returns its claims.
    /// Does NOT create or update local users — purely validates and extracts claims.
    /// </summary>
    public async Task<ClaimsPrincipal> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var clientId = _configuration["AzureAd:ClientId"]!;
        var tenantId = _configuration["AzureAd:TenantId"]!;
        var instance = _configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/";
        var v2Issuer = $"{instance}{tenantId}/v2.0";
        var v1Issuer = $"https://sts.windows.net/{tenantId}/";
        var wellKnownUrl = $"{v2Issuer}/.well-known/openid-configuration";

        var discoveryDocument = await OpenIdConnectConfigurationRetriever.GetAsync(
            wellKnownUrl, _documentRetriever, cancellationToken);
        var signingKeys = discoveryDocument.SigningKeys;

        var validationParameters = new TokenValidationParameters
        {
            // Depending on the app registration's accessTokenAcceptedVersion, Entra may issue
            // either v2 tokens from login.microsoftonline.com or v1 tokens from sts.windows.net.
            ValidIssuers = new[] { v2Issuer, v1Issuer, discoveryDocument.Issuer },
            ValidAudiences = new[] { clientId, $"api://{clientId}" },
            IssuerSigningKeys = signingKeys,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5),
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(accessToken, validationParameters, out _);

        return principal;
    }

    /// <summary>
    /// Extracts all claims from an Entra ID token as a dictionary for embedding into our JWT.
    /// </summary>
    public async Task<(ClaimsPrincipal Principal, Dictionary<string, object?> EntraClaims)> ValidateAndExtractAsync(
        string accessToken, CancellationToken cancellationToken = default)
    {
        var principal = await ValidateTokenAsync(accessToken, cancellationToken);

        var entraClaims = new Dictionary<string, object?>();
        foreach (var claim in principal.Claims)
        {
            var key = SimplifyClaimType(claim.Type);
            entraClaims[key] = claim.Value;
        }

        return (principal, entraClaims);
    }

    private static string SimplifyClaimType(string claimType)
    {
        return claimType switch
        {
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" => "oid",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => "name",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" => "email",
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => "role",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname" => "given_name",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname" => "family_name",
            _ => claimType
        };
    }
}
