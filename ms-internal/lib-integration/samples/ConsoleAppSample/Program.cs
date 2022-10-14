using GSF.CarbonIntensity.Handlers;
using GSF.CarbonIntensity.Parameters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, ConsoleAppSample!");

var key1 = $"{CarbonAwareVariablesConfiguration.Key}:CarbonIntensityDataSource";
var settings = new Dictionary<string, string> {
            {key1, "JSON"}
        };
var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(settings)
        .Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddCarbonIntensityServices(configuration);
var serviceProvider = serviceCollection.BuildServiceProvider();
var handler = serviceProvider.GetRequiredService<IEmissionsHandler>();

const string startDate = "2022-03-01T15:30:00Z";
const string endDate = "2022-03-01T18:30:00Z";
const string location = "eastus";
var builder = new EmissionsParametersBuilder();
var param = builder.AddLocations(new string[] { location } )
                .AddStartTime(DateTimeOffset.Parse(startDate))
                .AddEndTime(DateTimeOffset.Parse(endDate))
                .Build();

var result = await handler.GetRatingAsync(param);
Console.WriteLine($"For location {location} Starting at: {startDate} Ending at: {endDate} the Carbon Emissions Rating is: {result}.");
