using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using CarbonAware.Interfaces;

namespace CarbonAware.LocationSources.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddLocationSourcesService(this IServiceCollection services, IConfiguration configuration)
    {
        // configuring dependency injection to have config.
        services.Configure<LocationDataSourcesConfiguration>(c =>
        {
            configuration.GetSection(LocationDataSourcesConfiguration.Key).Bind(c);
        });
        services.TryAddSingleton<ILocationSource, LocationSource>();
    }
}
