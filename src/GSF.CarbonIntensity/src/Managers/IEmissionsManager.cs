using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Model;

namespace GSF.CarbonIntensity.Managers;

public interface IEmissionsManager
{
    /// <summary>
    /// Return Carbon Intensity Rate
    /// </summary>
    Task<double> GetRateAsync(CarbonAwareParameters parameter);

    /// <summary>
    /// Return the lowest EmissionsData
    /// </summary>
    Task<EmissionsData> GetLowestForecastAsync(CarbonAwareParameters parameter);
}
