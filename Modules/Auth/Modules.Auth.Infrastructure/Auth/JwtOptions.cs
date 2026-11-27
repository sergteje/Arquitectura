namespace Modules.Auth.Infrastructure.Auth;

/// <summary>
/// JWT settings, bound from the <c>Jwt</c> configuration section.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Gets or sets the symmetric signing key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the token issuer.</summary>
    public string Issuer { get; set; } = "Arquitectura";

    /// <summary>Gets or sets the token audience.</summary>
    public string Audience { get; set; } = "ArquitecturaClients";

    /// <summary>Gets or sets the access-token lifetime in minutes.</summary>
    public int AccessTokenMinutes { get; set; } = 30;
}
