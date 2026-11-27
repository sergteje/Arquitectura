using System.Text;
using System.Text.Json;

namespace Arquitectura.Host.Web.Middleware;

/// <summary>
/// Prepares an HTTP body for logging: masks credential-shaped JSON members and refuses to echo
/// anything that is not JSON.
/// </summary>
/// <remarks>
/// Bodies are a debugging aid, never an audit record, so the default posture is to reveal as
/// little as possible. Only JSON is echoed back: a multipart upload or a form post has no shape
/// this class can reason about, and echoing it verbatim is how a password ends up in a log file.
/// </remarks>
public static class LogBodyRedactor
{
    /// <summary>The value written in place of a sensitive member's value.</summary>
    public const string RedactedValue = "***";

    /// <summary>The placeholder written when the body is not JSON.</summary>
    public const string NonJsonPlaceholder = "[non-JSON body omitted]";

    private const int MaxBodyLength = 1000;

    private static readonly HashSet<string> SensitiveMembers = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "currentPassword",
        "newPassword",
        "confirmPassword",
        "accessToken",
        "refreshToken",
        "token",
        "apiKey",
        "secret",
        "authorization"
    };

    /// <summary>
    /// Returns a log-safe representation of <paramref name="body"/>.
    /// </summary>
    /// <param name="body">The raw request or response body.</param>
    /// <returns>
    /// The body with sensitive members masked and truncated, an empty string when there is no body,
    /// or <see cref="NonJsonPlaceholder"/> when the body is not JSON.
    /// </returns>
    public static string Redact(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            using var buffer = new MemoryStream();

            using (var writer = new Utf8JsonWriter(buffer))
            {
                Write(document.RootElement, writer);
            }

            return Truncate(Encoding.UTF8.GetString(buffer.ToArray()));
        }
        catch (JsonException)
        {
            return NonJsonPlaceholder;
        }
    }

    private static void Write(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();

                foreach (var property in element.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);

                    if (SensitiveMembers.Contains(property.Name))
                    {
                        writer.WriteStringValue(RedactedValue);
                    }
                    else
                    {
                        Write(property.Value, writer);
                    }
                }

                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();

                foreach (var item in element.EnumerateArray())
                {
                    Write(item, writer);
                }

                writer.WriteEndArray();
                break;

            default:
                element.WriteTo(writer);
                break;
        }
    }

    private static string Truncate(string content) =>
        content.Length <= MaxBodyLength ? content : content[..MaxBodyLength] + "... [truncated]";
}
