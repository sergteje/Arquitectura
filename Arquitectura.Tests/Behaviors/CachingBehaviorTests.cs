using Arquitectura.Application.Interfaces;
using Arquitectura.Infrastructure.Cache;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Arquitectura.Tests.Behaviors;

public class CachingBehaviorTests
{
    private readonly Mock<IDistributedCacheService> _cacheMock;

    public CachingBehaviorTests()
    {
        _cacheMock = new Mock<IDistributedCacheService>();
    }

    [Fact]
    public async Task Handle_WithNonCachableQuery_ShouldNotUseCache()
    {
        // Arrange
        var request = new TestRequestWithoutCache();
        var response = new TestResponse("test");

        _cacheMock.Setup(x => x.GetAsync<TestResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestResponse?)null);

        RequestHandlerDelegate<TestResponse> next = ct => Task.FromResult(response);

        var logger = NullLogger<CachingBehavior<TestRequestWithoutCache, TestResponse>>.Instance;
        var behavior = new CachingBehavior<TestRequestWithoutCache, TestResponse>(
            _cacheMock.Object, logger);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(response);
        _cacheMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<TestResponse>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithCachableQuery_MissingFromCache_ShouldCallNext()
    {
        // Arrange
        var request = new TestRequestWithCache();
        var response = new TestResponse("test");

        _cacheMock.Setup(x => x.GetAsync<CachedResult<TestResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CachedResult<TestResponse>?)null);

        RequestHandlerDelegate<TestResponse> next = ct => Task.FromResult(response);

        var logger = NullLogger<CachingBehavior<TestRequestWithCache, TestResponse>>.Instance;
        var behavior = new CachingBehavior<TestRequestWithCache, TestResponse>(
            _cacheMock.Object, logger);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(response);
    }
}

public record TestRequest : IRequest<TestResponse>;
public record TestRequestWithoutCache : IRequest<TestResponse>;
public record TestRequestWithCache : IRequest<TestResponse>, ICachableQuery
{
    public string GetCacheKey() => "test-cache-key";
}
public record TestResponse(string Value);
public record CachedResult<T>
{
    public T? Value { get; set; }
    public bool IsSuccess { get; set; }
    public Arquitectura.Domain.Models.ResultError? Error { get; set; }
}

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IDistributedCacheService _cache;
    private readonly ILogger _logger;

    public CachingBehavior(IDistributedCacheService cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICachableQuery cachableRequest)
        {
            return await next(cancellationToken);
        }

        var cachedResult = await _cache.GetAsync<CachedResult<TResponse>>(cachableRequest.GetCacheKey(), cancellationToken);

        if (cachedResult != null && cachedResult.IsSuccess && cachedResult.Value != null)
        {
            return cachedResult.Value;
        }

        return await next(cancellationToken);
    }
}