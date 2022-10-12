using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;

namespace CarbonAware.Library.CarbonIntensity;

internal class CarbonIntensity : ICarbonIntensity
{
    private readonly ICarbonAwareAggregator _aggregator;
    private readonly ILogger _logger;

    public CarbonIntensity(ICarbonAwareAggregator aggregator, ILogger<CarbonIntensity> logger)
    {
        _aggregator = aggregator;
        _logger = logger;
    }

    public async Task<EmissionsData> GetLowestForecastAsync(CarbonAwareParameters parameter)
    {
        _logger.LogWarning($"Not implemented. Returning empty emissions data");
        return await Task.FromResult(new EmissionsData());
    }

    public async Task<double> GetRateAsync(CarbonAwareParameters parameter)
    {
        var emissions = await _aggregator.GetEmissionsDataAsync(parameter);
        _logger.LogInformation($"Returning first rate out of {emissions.Count()}");
        return emissions.First().Rating;
    }
}
