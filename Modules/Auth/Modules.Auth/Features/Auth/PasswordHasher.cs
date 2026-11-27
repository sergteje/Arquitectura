using System.Security.Cryptography;
using System.Text;

namespace Modules.Auth.Features.Auth;

/// <summary>
/// Hashes and verifies account passwords.
/// </summary>
/// <remarks>
/// The scheme is kept as SHA-256 so existing accounts keep working after the migration.
/// See <c>TECHNICAL_DEBT.md</c>: a salted key-derivation function should replace it.
/// </remarks>
internal static class PasswordHasher
{
    /// <summary>Computes the stored hash for a plain-text password.</summary>
    /// <param name="password">The plain-text password.</param>
    /// <returns>The Base64-encoded SHA-256 hash.</returns>
    internal static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>Verifies a plain-text password against a stored hash.</summary>
    /// <param name="password">The plain-text password.</param>
    /// <param name="hash">The stored hash.</param>
    /// <returns><c>true</c> when the password matches.</returns>
    internal static bool Verify(string password, string hash) => Hash(password) == hash;
}
