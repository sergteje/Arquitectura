namespace Arquitectura.Domain.Models;

public sealed record ResultError
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public Dictionary<string, object>? Details { get; init; }

    public static ResultError Create(string code, string message, Dictionary<string, object>? details = null) => new()
    {
        Code = code,
        Message = message,
        Details = details
    };
}

public readonly struct Result<T>
{
    public T? Value { get; }
    public ResultError? Error { get; }
    public bool IsSuccess => Error == null;
    public bool IsFailure => Error != null;

    private Result(T? value, ResultError? error)
    {
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(ResultError error) => new(default, error);
    public static Result<T> Failure(string code, string message, Dictionary<string, object>? details = null) =>
        Failure(ResultError.Create(code, message, details));

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<ResultError, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error!);

    public async Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onSuccess,
        Func<ResultError, Task<TResult>> onFailure) =>
        IsSuccess ? await onSuccess(Value!) : await onFailure(Error!);
}

public readonly struct Result
{
    public ResultError? Error { get; }
    public bool IsSuccess => Error == null;
    public bool IsFailure => Error != null;

    private Result(ResultError? error) => Error = error;

    public static Result Success() => new(null);
    public static Result Failure(ResultError error) => new(error);
    public static Result Failure(string code, string message, Dictionary<string, object>? details = null) =>
        Failure(ResultError.Create(code, message, details));
    public static Result FromBool(bool success, string code = "OPERATION_FAILED", string message = "Operation failed") =>
        success ? Success() : Failure(code, message);
}

public static class ServiceErrors
{
    public static ResultError NotFound(string entity, string id) => new()
    {
        Code = "NOT_FOUND",
        Message = $"{entity} with ID '{id}' was not found",
        Details = new Dictionary<string, object>
        {
            ["entityId"] = id,
            ["entityType"] = entity
        }
    };

    public static ResultError ValidationError(string message, Dictionary<string, object>? details = null) => new()
    {
        Code = "VALIDATION_ERROR",
        Message = message,
        Details = details
    };

    public static ResultError RetryExhausted(int attempts, string lastError) => new()
    {
        Code = "RETRY_EXHAUSTED",
        Message = $"Operation failed after {attempts} attempts",
        Details = new Dictionary<string, object>
        {
            ["attempts"] = attempts,
            ["lastError"] = lastError
        }
    };

    public static ResultError Timeout(string operation, int timeoutSeconds) => new()
    {
        Code = "TIMEOUT",
        Message = $"Operation '{operation}' timed out after {timeoutSeconds} seconds",
        Details = new Dictionary<string, object>
        {
            ["operation"] = operation,
            ["timeoutSeconds"] = timeoutSeconds
        }
    };
}