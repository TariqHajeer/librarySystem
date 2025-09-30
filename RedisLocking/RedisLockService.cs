using Microsoft.Extensions.Options;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace RedisLocking;

internal class RedisLockService : ILockService, IDisposable
{
    private readonly RedLockFactory _redLockFactory;
    public RedisLockService(RedisLockingOptions options)
    {
        var redisEndpoints = options.RedisEndpoints;
        if (redisEndpoints == null || (!redisEndpoints.Any()))
            throw new ArgumentException("Redis endpoints must be configured");
        var endpoints = redisEndpoints
            .Select(e => new RedLockEndPoint
            {
                EndPoint = ConfigurationOptions.Parse(e).EndPoints.First()
            })
            .ToList();
        _redLockFactory = RedLockFactory.Create(endpoints);
    }
    public async Task<bool> RunWithLockAsync(string resourceKey, TimeSpan expiry, Func<Task> action, TimeSpan wait, TimeSpan retry)
    {
        using var redLock = await _redLockFactory.CreateLockAsync(
            resource: $"lock:{resourceKey}", // Unique key
            expiryTime: expiry,// Auto-release after expiry
            waitTime: wait,// How long to wait for the lock
            retryTime: retry // Retry interval
        );

        if (!redLock.IsAcquired)
            return false; // Could not get lock

        await action();
        return true;
    }

    public Task<bool> RunWithLockAsync(string resourceKey, TimeSpan expiry, Func<Task> action)
    {
        var wait = TimeSpan.FromSeconds(5);
        var retry = TimeSpan.FromMilliseconds(500);
        return RunWithLockAsync(resourceKey, expiry, action, wait, retry);
    }


    public void Dispose()
    {
        _redLockFactory?.Dispose();
    }

    public async Task<T> RunWithLockAsync<T>(string resourceKey, TimeSpan expiry, Func<Task<T>> action, TimeSpan wait, TimeSpan retry)
    {
        using var redLock = await _redLockFactory.CreateLockAsync(
            resource: $"lock:{resourceKey}",
            expiryTime: expiry,
            waitTime: wait,
            retryTime: retry
        );

        if (!redLock.IsAcquired)
        {
            throw new Exception("Could not acquire lock, please try again later.");
        }

        return await action();
    }

    public Task<T> RunWithLockAsync<T>(string resourceKey, TimeSpan expiry, Func<Task<T>> action)
    {
        var wait = TimeSpan.FromSeconds(5);
        var retry = TimeSpan.FromMilliseconds(500);
        return RunWithLockAsync(resourceKey, expiry, action, wait, retry);
    }
}
