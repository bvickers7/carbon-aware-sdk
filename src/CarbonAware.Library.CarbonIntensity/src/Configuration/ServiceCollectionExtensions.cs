using CarbonAware.Aggregators.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.Library.CarbonIntensity;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add services needed in order to use an CarbonIntensity service.
    /// </summary>
    public static IServiceCollection AddCarbonIntensityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCarbonAwareEmissionServices(configuration)
                .TryAddSingleton<ICarbonIntensity, CarbonIntensity>();
        return services;
    }
}
