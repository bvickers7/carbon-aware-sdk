using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CarbonAware.WebApi.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddMonitoringAndTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        
        var envVars = configuration?.GetSection(CarbonAwareVariablesConfiguration.Key).Get<CarbonAwareVariablesConfiguration>();
        var telemetryProvider = GetTelemetryProviderFromValue(envVars?.TelemetryProvider);

        switch (telemetryProvider) {
            case TelemetryProviderType.ApplicationInsights:
            {
                if (!String.IsNullOrEmpty(configuration?["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
                {
                    services.AddApplicationInsightsTelemetry();
                }
                break;   
            }
            case TelemetryProviderType.NotProvided:
            {
                break;
            }
            case TelemetryProviderType.OpenTelemetryConsole:
            {
                services.AddOpenTelemetryTracing(b =>
                    {
                        b
                        .AddConsoleExporter()
                        .AddAspNetCoreInstrumentation()
                        .AddSource(TelemetryActivity.ServiceName)
                        .SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService(serviceName: TelemetryActivity.ServiceName, serviceVersion: "0.0.1"));
                    });
                break;
            }
          // Can be extended in the future to support a different provider like Zipkin, Prometheus etc 
        }

    }

    private static TelemetryProviderType GetTelemetryProviderFromValue(string? srcVal)
    {
        TelemetryProviderType result;
        if (String.IsNullOrEmpty(srcVal) ||
            !Enum.TryParse<TelemetryProviderType>(srcVal, true, out result))
        {
            return TelemetryProviderType.NotProvided;
        }
        return result;
    }
}