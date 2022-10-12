using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;

namespace CarbonAware.Library.CarbonIntensity;

public interface ICarbonIntensity
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