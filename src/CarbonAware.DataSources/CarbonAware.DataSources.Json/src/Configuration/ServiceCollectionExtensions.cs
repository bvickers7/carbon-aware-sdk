using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.Interfaces;

namespace CarbonAware.DataSources.Json.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddJsonEmissionsDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        // configuring dependency injection to have config.
        services.Configure<JsonDataSourceConfiguration>(c =>
        {
            c = dataSourcesConfig.EmissionsConfiguration<JsonDataSourceConfiguration>();
        });
        services.TryAddSingleton<IEmissionsDataSource, JsonDataSource>();
    }
}