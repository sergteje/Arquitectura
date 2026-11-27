using BuildingBlocks.Abstractions.Mediator;
using Microsoft.AspNetCore.Http;
using ResulTus;
using ResulTus.AspNetCore;
using ResulTus.Validation;

namespace BuildingBlocks.AspNetCore;

/// <summary>
/// HTTP projections for the mediator's <see cref="Result{TSuccess,TError}"/> tier.
/// </summary>
/// <remarks>
/// <see cref="Result{TSuccess,TError}"/> is not covered by <c>ResulTus.AspNetCore</c> because its error
/// type is application owned. A <see cref="RequestError"/> that carries <see cref="ValidationErrors"/>
/// renders through <c>ResulTus.AspNetCore</c>'s validation conversion, so the RFC 9457 <c>errors</c>
/// member keeps its previous shape; any other failure renders as the standard problem details.
/// </remarks>
public static class RequestResultHttpResults
{
    /// <summary>Returns 200 on success, or the RFC 9457 problem result.</summary>
    /// <typeparam name="T">The success payload type.</typeparam>
    /// <param name="result">The result to project.</param>
    /// <returns>The HTTP result.</returns>
    public static IResult ToHttpResult<T>(this Result<T, RequestError> result) =>
        result.Match(
            value => Results.Json(value),
            error => error.ToProblemHttpResult());

    /// <summary>Returns 204 on success, or the RFC 9457 problem result.</summary>
    /// <typeparam name="T">The success payload type.</typeparam>
    /// <param name="result">The result to project.</param>
    /// <returns>The HTTP result.</returns>
    public static IResult ToNoContentHttpResult<T>(this Result<T, RequestError> result) =>
        result.Match(
            _ => Results.NoContent(),
            error => error.ToProblemHttpResult());

    /// <summary>Returns 201 Created on success, or the RFC 9457 problem result.</summary>
    /// <typeparam name="T">The success payload type.</typeparam>
    /// <param name="result">The result to project.</param>
    /// <param name="uri">The location of the created resource.</param>
    /// <returns>The HTTP result.</returns>
    public static IResult ToCreatedHttpResult<T>(this Result<T, RequestError> result, string uri) =>
        result.Match(
            value => Results.Created(uri, value),
            error => error.ToProblemHttpResult());

    /// <summary>Returns 202 Accepted on success, or the RFC 9457 problem result.</summary>
    /// <typeparam name="T">The success payload type.</typeparam>
    /// <param name="result">The result to project.</param>
    /// <returns>The HTTP result.</returns>
    public static IResult ToAcceptedHttpResult<T>(this Result<T, RequestError> result) =>
        result.Match(
            value => Results.Accepted(value: value),
            error => error.ToProblemHttpResult());

    /// <summary>Maps a request failure to an RFC 9457 problem result.</summary>
    /// <param name="requestError">The request failure.</param>
    /// <returns>The problem result.</returns>
    public static IResult ToProblemHttpResult(this RequestError requestError)
    {
        ArgumentNullException.ThrowIfNull(requestError);

        return requestError.ValidationErrors is { } validationErrors
            ? Result<object, ValidationErrors>.Failure(validationErrors).ToHttpResult()
            : requestError.Error.ToProblemHttpResult();
    }
}
