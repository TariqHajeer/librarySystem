using System;

namespace RedisLocking;
/// <summary>
/// Provides methods for acquiring a Redis-based distributed lock and executing an action exclusively.
/// </summary>
public interface ILockService
{
    /// <summary>
    /// Attempts to acquire a distributed lock for the specified resource and execute the given <paramref name="action"/>.
    /// Waits up to <paramref name="wait"/> while retrying every <paramref name="retry"/> if the lock is not immediately available.
    /// </summary>
    /// <param name="resourceKey">
    /// A unique identifier for the lock. Typically represents the resource you want to protect
    /// (e.g., a wallet ID or transaction ID).
    /// </param>
    /// <param name="expiry">
    /// The maximum duration the lock should be held. Acts as a safety timeout to automatically release
    /// the lock if the process crashes or fails to release it.
    /// </param>
    /// <param name="action">
    /// The asynchronous operation to execute while holding the lock.
    /// </param>
    /// <param name="wait">
    /// The maximum time to wait for the lock to become available before giving up.
    /// </param>
    /// <param name="retry">
    /// The interval between attempts to acquire the lock while waiting.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the lock was successfully acquired and the action executed;
    /// otherwise, <c>false</c> if the lock could not be acquired within the specified wait time.
    /// </returns>
    Task<bool> RunWithLockAsync(string resourceKey, TimeSpan expiry, Func<Task> action, TimeSpan wait, TimeSpan retry);

    /// <summary>
    /// Attempts to acquire a distributed lock for the specified resource and execute the given <paramref name="action"/>
    /// using default wait (5 seconds) and retry (500 milliseconds) values.
    /// </summary>
    /// <param name="resourceKey">
    /// A unique identifier for the lock. Typically represents the resource you want to protect
    /// (e.g., a wallet ID or transaction ID).
    /// </param>
    /// <param name="expiry">
    /// The maximum duration the lock should be held. Acts as a safety timeout to automatically release
    /// the lock if the process crashes or fails to release it.
    /// </param>
    /// <param name="action">
    /// The asynchronous operation to execute while holding the lock.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the lock was successfully acquired and the action executed;
    /// otherwise, <c>false</c> if the lock could not be acquired within the default wait time.
    /// </returns>
    Task<bool> RunWithLockAsync(string resourceKey, TimeSpan expiry, Func<Task> action);
    Task<T> RunWithLockAsync<T>(string resourceKey, TimeSpan expiry, Func<Task<T>> action, TimeSpan wait, TimeSpan retry);
    Task<T> RunWithLockAsync<T>(string resourceKey, TimeSpan expiry, Func<Task<T>> action);
}