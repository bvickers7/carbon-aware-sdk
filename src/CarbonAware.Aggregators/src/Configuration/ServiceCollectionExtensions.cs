using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.Interfaces;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.DataSources.Configuration;
using CarbonAware.DataSources.Json;
using CarbonAware.DataSources.WattTime;
using System.Reflection;

namespace CarbonAware.Aggregators.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add services needed in order to pull data from a Carbon Intensity data source.
    /// </summary>
    public static IServiceCollection AddCarbonAwareEmissionServices(this IServiceCollection services, IConfiguration configuration)
    {
        var envVars = configuration.GetSection(CarbonAwareVariablesConfiguration.Key).Get<CarbonAwareVariablesConfiguration>();
        var configuredDataSourceType = envVars.CarbonIntensityDataSource;
        Console.WriteLine($"Configured data source: {configuredDataSourceType}");
        var assembly = Assembly.Load($"CarbonAware.DataSources.{configuredDataSourceType}");
        var carbonIntensityDataSources = assembly.GetTypes()
                .Where(type => typeof(ICarbonIntensityDataSource).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        Console.WriteLine($"Found {carbonIntensityDataSources.Count()} data sources in assembly {assembly.FullName}");
        foreach (Type dataSourceType in carbonIntensityDataSources){
            Console.WriteLine($"dataSourceType: {dataSourceType}");
            Console.WriteLine($"dataSourceType.Name: {dataSourceType.Name}");
            Console.WriteLine($"Adding {dataSourceType.Name} to services");
        
            // services.TryAddSingleton<ICarbonIntensityDataSource, Type.GetType(dataSourceType.Name)>();
        }

        var allTypes = assembly.GetReferencedAssemblies().Select(name => Assembly.Load(name).GetTypes()
            .Where(type => !type.IsInterface && !type.IsAbstract)).SelectMany(x => x).ToList();
        foreach (var type in allTypes){
            Console.WriteLine($"Found {type.Name} in {assembly}");
        }

        // foreach (var reference in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
        // {
        //     var assembly = Assembly.Load(reference);
        //     var carbonIntensityDataSources = assembly.GetTypes()
        //         .Where(type => typeof(ICarbonIntensityDataSource).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        //     Console.WriteLine($"Found {carbonIntensityDataSources.Count()} data sources in assembly {assembly.Name}");
        //     foreach (var dataSourceType in carbonIntensityDataSources){
        //         Console.WriteLine($"dataSourceType: {dataSourceType}");
        //         Console.WriteLine($"configuredDataSourceType: {configuredDataSourceType}");
        //         Console.WriteLine($"dataSourceType.Name: {dataSourceType.Name}");
        //         if (dataSourceType.Name == configuredDataSourceType){
        //             Console.WriteLine($"Adding {dataSourceType.Name} to services");
        //             // services.TryAddSingleton(typeof(ICarbonIntensityDataSource), dataSourceType);
        //             //  _ = configuration ?? throw new ConfigurationException("WattTime configuration required.");
        //             // services.ConfigureWattTimeClient(configuration);
        //             // services.TryAddSingleton<ICarbonIntensityDataSource, WattTimeDataSource>();
        //             // services.Configure<LocationDataSourcesConfiguration>(c =>
        //             // {
        //             //     configuration.GetSection(LocationDataSourcesConfiguration.Key).Bind(c);
        //             // });
        //             // services.TryAddSingleton<ILocationSource, LocationSource>();
        //         }
        //     }
        // }
        // foreach (var type in assembly.GetTypes())
        // {
        //     // Register all classes that implement the IIntegration interface
        //     if (typeof(IIntegration).IsAssignableFrom(type))
        //     {
        //         // Add as a singleton as the Worker is a singleton and we'll only have one
        //         // instance. If this would be a Controller or something else with clearly defined
        //         // scope that is not the lifetime of the application, use AddScoped.
        //         services.AddSingleton(typeof(IIntegration), type);
        //     }

        //     // Register all classes that implement the ISettings interface
        //     if (typeof(ISettings).IsAssignableFrom(type))
        //     {
        //         var settings = Activator.CreateInstance(type);
        //         // appsettings.json or some other configuration provider should contain
        //         // a key with the same name as the type
        //         // e.g. "HttpIntegrationSettings": { ... }
        //         if (!configuration.GetSection(type.Name).Exists())
        //         {
        //             // If it does not contain the key, throw an error
        //             throw new ArgumentException($"Configuration does not contain key [{type.Name}]");
        //         }
        //         configuration.Bind(type.Name, settings);

        //         // Settings can be singleton as we'll only ever read it
        //         services.AddSingleton(type, settings);
        //     }
        // }
        // services.AddDataSourceService(configuration);
        // services.TryAddSingleton<ICarbonAwareAggregator, CarbonAwareAggregator>();
        return services;
    }
}
