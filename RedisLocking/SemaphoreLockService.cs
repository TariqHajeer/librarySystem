using System;
using System.Collections.Concurrent;

namespace RedisLocking;

public class SemaphoreLockService:ILockService
{
       private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores
          = new ConcurrentDictionary<string, SemaphoreSlim>();


    public async Task<bool> RunWithLockAsync(string resourceKey, TimeSpan expiry, Func<Task> action, TimeSpan wait, TimeSpan retry)
    {
        // Get or create a semaphore for this resourceKey
        var semaphore = _semaphores.GetOrAdd(resourceKey, _ => new SemaphoreSlim(1, 1));

        var waitUntil = DateTime.UtcNow + wait;

        while (DateTime.UtcNow < waitUntil)
        {
            if (await semaphore.WaitAsync(0)) // Try to acquire immediately
            {
                try
                {
                    using var cts = new CancellationTokenSource(expiry);
                    await action(); // Execute action
                    return true;
                }
                finally
                {
                    semaphore.Release();
                }
            }

            await Task.Delay(retry); // Wait before retrying
        }

        return false; // Could not acquire lock
    }

    public Task<bool> RunWithLockAsync(string resourceKey, TimeSpan expiry, Func<Task> action)
    {
        var wait = TimeSpan.FromSeconds(5);
        var retry = TimeSpan.FromMilliseconds(500);
        return RunWithLockAsync(resourceKey, expiry, action, wait, retry);
    }
        // Generic version that returns T
    public async Task<T> RunWithLockAsync<T>(string resourceKey, TimeSpan expiry, Func<Task<T>> action, TimeSpan wait, TimeSpan retry)
    {
        var semaphore = _semaphores.GetOrAdd(resourceKey, _ => new SemaphoreSlim(1, 1));
        var waitUntil = DateTime.UtcNow + wait;

        while (DateTime.UtcNow < waitUntil)
        {
            if (await semaphore.WaitAsync(0))
            {
                try
                {
                    using var cts = new CancellationTokenSource(expiry);
                    return await action();
                }
                finally
                {
                    semaphore.Release();
                }
            }
            await Task.Delay(retry);
        }

        return default; // Could not acquire lock
    }

    public Task<T> RunWithLockAsync<T>(string resourceKey, TimeSpan expiry, Func<Task<T>> action)
    {
        var wait = TimeSpan.FromSeconds(5);
        var retry = TimeSpan.FromMilliseconds(500);
        return RunWithLockAsync(resourceKey, expiry, action, wait, retry);
    }
}
