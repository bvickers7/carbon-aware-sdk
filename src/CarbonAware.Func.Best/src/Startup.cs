using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using CarbonAware.Aggregators.Configuration;

[assembly: FunctionsStartup(typeof(CarbonAwareFunction.Startup))]

namespace CarbonAwareFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddCarbonAwareEmissionServices(configuration);
        }
    }
}
