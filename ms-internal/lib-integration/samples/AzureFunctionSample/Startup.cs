using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using GSF.CarbonIntensity.Configuration;

[assembly: FunctionsStartup(typeof(myfunc.Startup))]

namespace myfunc
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddCarbonIntensityServices(configuration);
        }
    }
}