using Microsoft.AspNetCore.Mvc;
using System.Net;
using Arquitectura.Domain.Models;

namespace Arquitectura.Domain.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.Match(
            onSuccess: value => new OkObjectResult(value),
            onFailure: MapError);
    }

    public static IActionResult MapError(ResultError error)
    {
        return error.Code switch
        {
            "NOT_FOUND" => new NotFoundObjectResult(error),
            "VALIDATION_ERROR" => new BadRequestObjectResult(error),
            "RETRY_EXHAUSTED" => new ObjectResult(error) { StatusCode = (int)HttpStatusCode.ServiceUnavailable },
            "TIMEOUT" => new ObjectResult(error) { StatusCode = (int)HttpStatusCode.GatewayTimeout },
            _ => new BadRequestObjectResult(error)
        };
    }
}