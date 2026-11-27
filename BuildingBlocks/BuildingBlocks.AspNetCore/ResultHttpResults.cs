using Microsoft.AspNetCore.Http;
using ResulTus;
using ResulTus.AspNetCore;

namespace BuildingBlocks.AspNetCore;

/// <summary>
/// Success shapes that <c>ResulTus.AspNetCore</c> does not cover (204, 201 and 202), composed with
/// its RFC 9457 problem results.
/// </summary>
public static class ResultHttpResults
{
    /// <summary>Returns 204 on success, or the RFC 9457 problem result.</summary>
    /// <param name="result">The result to project.</param>
    /// <returns>The HTTP result.</returns>
    public static IResult ToNoContentHttpResult(this Result result) =>
        result.Match(
            () => Results.NoContent(),
            error => error.ToProblemHttpResult());

    /// <summary>Returns 201 Created on success, or the RFC 9457 problem result.</summary>
    /// <typeparam name="T">The success payload type.</typeparam>
    /// <param name="result">The result to project.</param>
    /// <param name="uri">The location of the created resource.</param>
    /// <returns>The HTTP result.</returns>
    public static IResult ToCreatedHttpResult<T>(this Result<T> result, string uri) =>
        result.Match(
            value => Results.Created(uri, value),
            error => error.ToProblemHttpResult());

    /// <summary>Returns 202 Accepted on success, or the RFC 9457 problem result.</summary>
    /// <typeparam name="T">The success payload type.</typeparam>
    /// <param name="result">The result to project.</param>
    /// <returns>The HTTP result.</returns>
    public static IResult ToAcceptedHttpResult<T>(this Result<T> result) =>
        result.Match(
            value => Results.Accepted(value: value),
            error => error.ToProblemHttpResult());
}
