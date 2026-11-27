using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Abstractions.Mediator;
using FluentAssertions;
using ResulTus;
using Xunit;

namespace Arquitectura.Tests.Caching;

public class QueryCachingExtensionsTests
{
    private sealed record Payload(string Value);

    private sealed record CacheableQuery(string Key) : ICachableQuery
    {
        public string GetCacheKey() => Key;

        public TimeSpan CacheDuration => TimeSpan.FromMinutes(1);
    }

    private sealed class InMemoryCache : IDistributedCacheService
    {
        private readonly Dictionary<string, object> _entries = [];

        public int GetCalls { get; private set; }

        public int SetCalls { get; private set; }

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            GetCalls++;
            return Task.FromResult(_entries.TryGetValue(key, out var value) ? (T?)value : default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            SetCalls++;
            _entries[key] = value!;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _entries.Remove(key);
            return Task.CompletedTask;
        }

        public Task RemoveByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default)
        {
            foreach (var key in _entries.Keys.Where(key => key.StartsWith(keyPrefix, StringComparison.Ordinal)).ToList())
            {
                _entries.Remove(key);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(_entries.ContainsKey(key));
    }

    [Fact]
    public async Task GetOrSetAsync_OnMiss_RunsFactoryAndCachesThePayload()
    {
        var cache = new InMemoryCache();
        var query = new CacheableQuery("k1");
        var factoryCalls = 0;

        var result = await cache.GetOrSetAsync<Payload>(
            query,
            _ =>
            {
                factoryCalls++;
                return Task.FromResult(Result<Payload>.Success(new Payload("value")));
            },
            CancellationToken.None);

        factoryCalls.Should().Be(1);
        cache.SetCalls.Should().Be(1);
        result.TryGetValue(out var payload).Should().BeTrue();
        payload!.Value.Should().Be("value");
    }

    [Fact]
    public async Task GetOrSetAsync_OnHit_DoesNotRunFactory()
    {
        var cache = new InMemoryCache();
        var query = new CacheableQuery("k2");

        await cache.SetAsync("k2", new Payload("cached"), cancellationToken: TestContext.Current.CancellationToken);
        var factoryCalls = 0;

        var result = await cache.GetOrSetAsync<Payload>(
            query,
            _ =>
            {
                factoryCalls++;
                return Task.FromResult(Result<Payload>.Success(new Payload("fresh")));
            },
            CancellationToken.None);

        factoryCalls.Should().Be(0);
        result.TryGetValue(out var payload).Should().BeTrue();
        payload!.Value.Should().Be("cached");
    }

    [Fact]
    public async Task GetOrSetAsync_OnFailure_DoesNotCache()
    {
        var cache = new InMemoryCache();
        var query = new CacheableQuery("k3");

        var result = await cache.GetOrSetAsync<Payload>(
            query,
            _ => Task.FromResult(Result<Payload>.Failure(Error.NotFound("MISSING", "nope"))),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        cache.SetCalls.Should().Be(0);
    }
}
