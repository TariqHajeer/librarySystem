using System;
using System.Collections.Concurrent;

namespace LibrarySystem.BusinessLogic.Helper;

public interface ILockService
{
    Task LockAsync(string key, TimeSpan? timeout = null);
    void Release(string key);
}
public class KeyedLockService : ILockService
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task LockAsync(string key, TimeSpan? timeout = null)
    {
        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        if (timeout.HasValue)
        {
            if (!await semaphore.WaitAsync(timeout.Value))
            {
                throw new TimeoutException($"Failed to acquire lock for key '{key}' within the specified timeout.");
            }
        }
        else
        {
            await semaphore.WaitAsync();
        }
    }

    public void Release(string key)
    {
        if (_locks.TryGetValue(key, out var semaphore))
        {
            semaphore.Release();
        }
        else
        {
            throw new InvalidOperationException($"No lock exists for key '{key}' to release.");
        }
    }
}
