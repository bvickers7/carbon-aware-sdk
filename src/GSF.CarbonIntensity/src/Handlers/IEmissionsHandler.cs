using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Model;

namespace GSF.CarbonIntensity.Handlers;

public interface IEmissionsHandler
{
    /// <summary>
    /// Return Carbon Intensity Rating
    /// </summary>
    // TODO. Need to 'hide' from the manager interface, all access to the Aggregator
    Task<double> GetRatingAsync(CarbonAwareParameters parameter);

    /// <summary>
    /// Return the lowest EmissionsData
    /// </summary>
    Task<EmissionsData> GetLowestForecastAsync(CarbonAwareParameters parameter);
}
