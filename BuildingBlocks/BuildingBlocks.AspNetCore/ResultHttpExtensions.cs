using Microsoft.AspNetCore.Http;
using ResulTus;

namespace BuildingBlocks.AspNetCore;

/// <summary>
/// Projects ResulTus results onto <see cref="IResult"/> while preserving the existing HTTP contract:
/// <c>{ code, message, details }</c> bodies and the legacy status mapping.
/// </summary>
public static class ResultHttpExtensions
{
    /// <summary>
    /// Returns 200 with the success value, or the mapped error result.
    /// </summary>
    /// <typeparam name="T">The success payload type.</typeparam>
    /// <param name="result">The result to project.</param>
    /// <returns>The HTTP result.</returns>
    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.Match<IResult>(
            value => Results.Json(value),
            ToErrorResult);

    /// <summary>
    /// Returns 204 on success, or the mapped error result.
    /// </summary>
    /// <param name="result">The result to project.</param>
    /// <returns>The HTTP result.</returns>
    public static IResult ToNoContentResult(this Result result) =>
        result.Match<IResult>(
            () => Results.NoContent(),
            ToErrorResult);

    /// <summary>
    /// Maps a ResulTus <see cref="Error"/> onto the legacy <c>{ code, message, details }</c> body and
    /// its HTTP status code.
    /// </summary>
    /// <param name="error">The error to map.</param>
    /// <returns>The HTTP result.</returns>
    public static IResult ToErrorResult(Error error)
    {
        return Results.Json(
            new ErrorResponse(error.Code ?? string.Empty, error.Message ?? string.Empty),
            statusCode: StatusCodeFor(error));
    }

    /// <summary>
    /// Resolves the HTTP status code for an error.
    /// </summary>
    /// <remarks>
    /// Legacy codes are honored first so the pre-existing endpoints keep their exact status. For the
    /// richer per-module codes the status is derived from <see cref="Error.Type"/>. A generic failure
    /// keeps the legacy 400 (the old mapper's default) rather than 500, so database failures observed
    /// by the frontend keep the same contract; <see cref="ErrorType.Unexpected"/> maps to 500.
    /// </remarks>
    /// <param name="error">The error to evaluate.</param>
    /// <returns>The HTTP status code.</returns>
    public static int StatusCodeFor(Error error) => error.Code switch
    {
        "NOT_FOUND" => StatusCodes.Status404NotFound,
        "VALIDATION_ERROR" => StatusCodes.Status400BadRequest,
        "RETRY_EXHAUSTED" => StatusCodes.Status503ServiceUnavailable,
        "TIMEOUT" => StatusCodes.Status504GatewayTimeout,
        _ => error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Timeout => StatusCodes.Status504GatewayTimeout,
            ErrorType.Cancelled => 499,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        }
    };
}
