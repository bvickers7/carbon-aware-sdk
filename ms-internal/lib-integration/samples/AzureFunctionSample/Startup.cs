using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using CarbonAware.Aggregators.Configuration;

[assembly: FunctionsStartup(typeof(myfunc.Startup))]

namespace myfunc
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