using Microsoft.AspNetCore.Http;
using ResulTus;
using ResulTus.AspNetCore;

namespace BuildingBlocks.AspNetCore;

/// <summary>
/// HTTP status mapping for ResulTus errors.
/// </summary>
/// <remarks>
/// Legacy codes keep their historical status and a generic <see cref="ErrorType.Failure"/> stays 400
/// (the old default for database failures). Every other category is delegated to
/// <see cref="ResultHttpStatus.GetStatusCode(Error)"/> from <c>ResulTus.AspNetCore</c>.
/// </remarks>
public static class ResultStatus
{
    /// <summary>Resolves the HTTP status code for an error.</summary>
    /// <param name="error">The error to evaluate.</param>
    /// <returns>The HTTP status code.</returns>
    public static int Map(Error error) => error.Code switch
    {
        "NOT_FOUND" => StatusCodes.Status404NotFound,
        "VALIDATION_ERROR" => StatusCodes.Status400BadRequest,
        "RETRY_EXHAUSTED" => StatusCodes.Status503ServiceUnavailable,
        "TIMEOUT" => StatusCodes.Status504GatewayTimeout,
        _ => error.Type == ErrorType.Failure
            ? StatusCodes.Status400BadRequest
            : ResultHttpStatus.GetStatusCode(error)
    };

    /// <summary>Maps an error to an RFC 9457 problem result.</summary>
    /// <param name="error">The error to map.</param>
    /// <returns>The problem result.</returns>
    public static IResult ToProblemHttpResult(this Error error) =>
        Result.Failure(error).ToHttpResult(Map);
}
