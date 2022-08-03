namespace CarbonAware.CLI;

using System.CommandLine;
using CarbonAware;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        var serviceProvider = BootstrapServices();

        var fileOption = new Option<FileInfo?>(
            name: "--file",
            description: "The file to read and display on the console.");

        var rootCommand = new RootCommand("Sample app for System.CommandLine");
        rootCommand.AddOption(fileOption);

        rootCommand.SetHandler((file) =>
        {
            ReadFile(file!);
        },
            fileOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static void ReadFile(FileInfo file)
    {
        File.ReadLines(file.FullName).ToList()
            .ForEach(line => Console.WriteLine(line));
    }

    private static ServiceProvider BootstrapServices()
    {

        var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

        var config = configurationBuilder.Build();
        var services = new ServiceCollection();

        services.Configure<CarbonAwareVariablesConfiguration>(config.GetSection(CarbonAwareVariablesConfiguration.Key));
        services.AddSingleton<IConfiguration>(config);
        services.AddCarbonAwareEmissionServices(config);

        services.AddLogging(configure => configure.AddConsole());

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider;
    }


}
