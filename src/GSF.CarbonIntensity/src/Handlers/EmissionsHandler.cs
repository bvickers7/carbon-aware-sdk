using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Model;
using Microsoft.Extensions.Logging;

namespace GSF.CarbonIntensity.Handlers;

internal class EmissionsHandler : IEmissionsHandler
{
    private readonly ICarbonAwareAggregator _aggregator;
    private readonly ILogger<EmissionsHandler> _logger;

    public EmissionsHandler(ICarbonAwareAggregator aggregator, ILogger<EmissionsHandler> logger)
    {
        _aggregator = aggregator;
        _logger = logger;
    }

    public async Task<EmissionsData> GetLowestForecastAsync(CarbonAwareParameters parameter)
    {
        _logger.LogWarning($"Not implemented. Returning empty emissions data");
        return await Task.FromResult(new EmissionsData());
    }

    public async Task<double> GetRatingAsync(CarbonAwareParameters parameter)
    {
        var emissions = await _aggregator.GetEmissionsDataAsync(parameter);
        _logger.LogInformation($"Returning first rate out of {emissions.Count()}");
        return emissions.First().Rating;
    }
}
