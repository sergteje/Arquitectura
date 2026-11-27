using ResulTus;

namespace BuildingBlocks.Abstractions.Errors;

/// <summary>
/// Cross-cutting errors shared by the modules.
/// </summary>
[ResultErrors]
public static partial class CommonErrors
{
    /// <summary>A database operation failed.</summary>
    /// <param name="detail">The underlying failure detail.</param>
    /// <returns>A failure error.</returns>
    [ErrorFactory("DATABASE_ERROR", ErrorType.Failure, "Database error: {detail}")]
    public static partial Error Database(string detail);
}
