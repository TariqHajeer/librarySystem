using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RedisLocking;
/// <summary>
/// Provides extension methods to register Redis-based distributed locking services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Redis-based distributed locking services to the DI container.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configureOptions">An action to configure <see cref="RedisLockingOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// The <see cref="ILockService"/> is registered as a Singleton because:
    /// <list type="bullet">
    /// <item>
    /// <description>It holds a <see cref="RedLockFactory"/>, which internally manages Redis connections that are expensive to create.</description>
    /// </item>
    /// <item>
    /// <description>The service is stateless aside from configuration, so it can be safely reused across the application.</description>
    /// </item>
    /// <item>
    /// <description>Singleton ensures a single instance of the service and Redis connections are reused efficiently, avoiding unnecessary overhead.</description>
    /// </item>
    /// <item>
    /// <description>Since the service implements <see cref="IDisposable"/>, the DI container will dispose it automatically on application shutdown, cleaning up Redis connections.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddRedisLocking(this IServiceCollection services, Action<RedisLockingOptions> configureOptions)
    {
        // Configure options from the provided action
        services.Configure(configureOptions);

        // Register ILockService conditionally
        services.AddSingleton<ILockService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RedisLockingOptions>>().Value;

            if (options.RedisEndpoints != null && options.RedisEndpoints.Any())
            {
                // Redis endpoints provided, use RedisLockService
                return new RedisLockService(options); // make sure RedisLockService accepts RedisLockingOptions in constructor
            }
            else
            {
                // No endpoints, fallback to SemaphoreLockService
                return new SemaphoreLockService();
            }
        });

        return services;
    }

}