// See https://aka.ms/new-console-template for more information

using CarbonAware;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Configuration;
using CarbonAware.Model;
using CarbonAware.Tools.WattTimeClient.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, CarbonAware!");

var key1 = $"{CarbonAwareVariablesConfiguration.Key}:CarbonIntensityDataSource";
var key2 = $"{WattTimeClientConfiguration.Key}:Username";
var key3 = $"{WattTimeClientConfiguration.Key}:Password";
var settings = new Dictionary<string, string> {
            {key1, "WattTime"},
            {key2, "user"},
            {key3, "passwd"}
        };
var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(settings)
        .Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddCarbonAwareEmissionServices(configuration);
var serviceProvider = serviceCollection.BuildServiceProvider();
var agg = serviceProvider.GetRequiredService<ICarbonAwareAggregator>();
var parameters = new CarbonAwareParameters()
        {
            SingleLocation = new Location { Name = "westus"},
            Start = DateTimeOffset.Parse("2022-03-01T15:30:00Z"),
            End = DateTimeOffset.Parse("2022-03-01T18:30:00Z")
        };
var result = await agg.CalculateAverageCarbonIntensityAsync(parameters);

Console.WriteLine($"Result: {result}");
