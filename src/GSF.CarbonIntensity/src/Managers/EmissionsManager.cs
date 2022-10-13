using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Model;
using Microsoft.Extensions.Logging;

namespace GSF.CarbonIntensity.Managers;

internal class EmissionsManager : IEmissionsManager
{
    private readonly ICarbonAwareAggregator _aggregator;
    private readonly ILogger<EmissionsManager> _logger;

    public EmissionsManager(ICarbonAwareAggregator aggregator, ILogger<EmissionsManager> logger)
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
