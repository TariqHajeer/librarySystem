using System.Dynamic;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem.DataAccess;

public static class IOC
{
    public static IServiceCollection InjectDataAccessLayer(this IServiceCollection services, Action<AdoNetOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddScoped<AdoNetDataAccessLayer, AdoNetDataAccessLayer>();
        return services;
    }
}
