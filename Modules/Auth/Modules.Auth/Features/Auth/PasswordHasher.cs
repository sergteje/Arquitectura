using System.Security.Cryptography;
using System.Text;

namespace Modules.Auth.Features.Auth;

/// <summary>
/// Hashes and verifies account passwords with a salted PBKDF2-HMAC-SHA256 key derivation function.
/// </summary>
/// <remarks>
/// The stored format is <c>PBKDF2$&lt;iterations&gt;$&lt;saltBase64&gt;$&lt;hashBase64&gt;</c>.
/// Legacy unsalted SHA-256 hashes (Base64, no prefix) are still accepted so existing accounts keep
/// working; <see cref="NeedsRehash"/> reports them so the login flow can transparently upgrade them.
/// </remarks>
internal static class PasswordHasher
{
    private const string Prefix = "PBKDF2";
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    /// <summary>Computes a new salted PBKDF2 hash for a plain-text password.</summary>
    /// <param name="password">The plain-text password.</param>
    /// <returns>The encoded hash.</returns>
    internal static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    /// <summary>Verifies a plain-text password against a stored hash.</summary>
    /// <param name="password">The plain-text password.</param>
    /// <param name="hash">The stored hash, in the current or the legacy format.</param>
    /// <returns><c>true</c> when the password matches.</returns>
    internal static bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return false;
        }

        return hash.StartsWith(Prefix + "$", StringComparison.Ordinal)
            ? VerifyPbkdf2(password, hash)
            : VerifyLegacySha256(password, hash);
    }

    /// <summary>
    /// Reports whether a stored hash should be replaced with the current format on the next successful
    /// login (legacy format or a weaker iteration count).
    /// </summary>
    /// <param name="hash">The stored hash.</param>
    /// <returns><c>true</c> when a rehash is recommended.</returns>
    internal static bool NeedsRehash(string hash)
    {
        if (!hash.StartsWith(Prefix + "$", StringComparison.Ordinal))
        {
            return true;
        }

        var parts = hash.Split('$');
        return parts.Length != 4
               || !int.TryParse(parts[1], out var iterations)
               || iterations < Iterations;
    }

    private static bool VerifyPbkdf2(string password, string hash)
    {
        var parts = hash.Split('$');
        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);

            var actual = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool VerifyLegacySha256(string password, string hash)
    {
        try
        {
            var expected = Convert.FromBase64String(hash);
            var actual = SHA256.HashData(Encoding.UTF8.GetBytes(password));

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
