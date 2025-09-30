using System;

namespace RedisLocking;

/// <summary>
/// Options for configuring Redis-based distributed locking.
/// </summary>
public class RedisLockingOptions
{
    /// <summary>
    /// Gets or sets the list of Redis endpoints used for distributed locking.
    /// <para>
    /// - In development, this is typically a single endpoint (e.g., "localhost:6379").
    /// - In production, you can provide multiple endpoints for RedLock quorum support (e.g., ["redis1:6379", "redis2:6379", "redis3:6379"]).
    /// </para>
    /// </summary>
    public IEnumerable<string>? RedisEndpoints { get; set; }
}