using CarbonAware.Extensions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace CarbonAware.Aggregators.CarbonAware;

public class CarbonAwareAggregator : ICarbonAwareAggregator
{
    private static readonly ActivitySource Activity = new ActivitySource(nameof(CarbonAwareAggregator));
    private readonly ILogger<CarbonAwareAggregator> _logger;
    private readonly ICarbonIntensityDataSource _dataSource;

    /// <summary>
    /// Creates a new instance of the <see cref="CarbonAwareAggregator"/> class.
    /// </summary>
    /// <param name="logger">The logger for the aggregator</param>
    /// <param name="dataSource">An <see cref="ICarbonIntensityDataSource"> data source.</param>
    public CarbonAwareAggregator(ILogger<CarbonAwareAggregator> logger, ICarbonIntensityDataSource dataSource)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._dataSource = dataSource;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
    {
        using (var activity = Activity.StartActivity())
        {
            DateTimeOffset end = GetOffsetOrDefault(props, CarbonAwareConstants.End, DateTimeOffset.Now.ToUniversalTime());
            DateTimeOffset start = GetOffsetOrDefault(props, CarbonAwareConstants.Start, end.AddDays(-7));
            _logger.LogInformation("Aggregator getting carbon intensity from data source");
            return await this._dataSource.GetCarbonIntensityAsync(GetLocationOrThrow(props), start, end);
        }
    }

    public async Task<EmissionsData?> GetBestEmissionsDataAsync(IDictionary props)
    {
        var results = await GetEmissionsDataAsync(props);
        return GetOptimalEmissions(results);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsForecast>> GetCurrentForecastDataAsync(IDictionary props)
    {
        using (var activity = Activity.StartActivity())
        {
            TimeSpan windowSize = GetDurationOrDefault(props);
            _logger.LogInformation("Aggregator getting carbon intensity forecast from data source");

            var forecasts = new List<EmissionsForecast>();
            foreach (var location in GetLocationOrThrow(props))
            {
                var forecast = await this._dataSource.GetCurrentCarbonIntensityForecastAsync(location);
                var firstDataPoint = forecast.ForecastData.First();
                var lastDataPoint = forecast.ForecastData.Last();
                forecast.StartTime = GetOffsetOrDefault(props, CarbonAwareConstants.Start, firstDataPoint.Time);
                forecast.EndTime = GetOffsetOrDefault(props, CarbonAwareConstants.End, lastDataPoint.Time + lastDataPoint.Duration);
                forecast.Validate();
                forecast.ForecastData = IntervalHelper.FilterByDuration(forecast.ForecastData, forecast.StartTime, forecast.EndTime);
                forecast.ForecastData = forecast.ForecastData.RollingAverage(windowSize);
                forecast.OptimalDataPoint = GetOptimalEmissions(forecast.ForecastData);
                if (forecast.ForecastData.Any())
                {
                    forecast.WindowSize = forecast.ForecastData.First().Duration;
                }
                forecasts.Add(forecast);
            }

            return forecasts;
        }
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast?> GetForecastDataAsync(IDictionary props)
    {
        using (var activity = Activity.StartActivity())
        {
            var start = GetOffsetOrDefault(props, CarbonAwareConstants.Start, DateTimeOffset.MinValue);
            var end = GetOffsetOrDefault(props, CarbonAwareConstants.End, DateTimeOffset.MaxValue);
            var windowSize = GetDurationOrDefault(props);
            var location = GetLocationOrThrow(props).First(); // Should only be one location
            var requestedAt = GetOffsetOrDefault(props, CarbonAwareConstants.RequestedAt, default);

            _logger.LogInformation("Aggregator getting carbon intensity forecast from data source");
            // Consuming data source start/end parameters with the value of requestedAt to get only the forecast at one specific moment.
            // NOTE: Signature of these methods should be re-evaluate since only one EmissionsForecast is returned, or create new ones
            // that are more explicit with the intent.
            var list = new List<EmissionsForecast>();
            await foreach (var forecast in this._dataSource.GetCarbonIntensityForecastAsync(location, requestedAt, requestedAt))
            {
                var firstDataPoint = forecast.ForecastData.First();
                var lastDataPoint = forecast.ForecastData.Last();
                forecast.StartTime = GetOffsetOrDefault(props, CarbonAwareConstants.Start, firstDataPoint.Time);
                forecast.EndTime = GetOffsetOrDefault(props, CarbonAwareConstants.End, lastDataPoint.Time + lastDataPoint.Duration);
                forecast.ForecastData = IntervalHelper.FilterByDuration(forecast.ForecastData, forecast.StartTime, forecast.EndTime);
                if (!forecast.ForecastData.Any())
                {
                    continue;
                }
                forecast.ForecastData = forecast.ForecastData.RollingAverage(windowSize);
                forecast.OptimalDataPoint = GetOptimalEmissions(forecast.ForecastData);
                if (forecast.ForecastData.Any())
                {
                    forecast.WindowSize = forecast.ForecastData.First().Duration;
                }
                list.Add(forecast);
                break;
            }
            return list.FirstOrDefault();
        }
    }

    private EmissionsData? GetOptimalEmissions(IEnumerable<EmissionsData> emissionsData)
    {
        if (!emissionsData.Any())
        {
            return null;
        }
        return emissionsData.MinBy(x => x.Rating);
    }

    /// <summary>
    /// Extracts the given offset prop and converts to DateTimeOffset. If prop is not defined, defaults to provided default
    /// </summary>
    /// <param name="props"></param>
    /// <returns>DateTimeOffset representing end period of carbon aware data search. </returns>
    /// <exception cref="ArgumentException">Throws exception if prop isn't a valid DateTimeOffset. </exception>
    private DateTimeOffset GetOffsetOrDefault(IDictionary props, string field, DateTimeOffset defaultValue)
    {
        // Default if null
        var dateTimeOffset = props[field] ?? defaultValue;

        // If fail to parse property, throw error
        if (!DateTimeOffset.TryParse(dateTimeOffset.ToString(), null, DateTimeStyles.AssumeUniversal, out defaultValue))
        {
            Exception ex = new ArgumentException("Failed to parse" + field + "field. Must be a valid DateTimeOffset");
            _logger.LogError("argument exception", ex);
            throw ex;
        }

        return defaultValue;
    }

    private IEnumerable<Location> GetLocationOrThrow(IDictionary props)
    {
        if (props[CarbonAwareConstants.Locations] is IEnumerable<Location> locations)
        {
            return locations;
        }
        Exception ex = new ArgumentException("locations parameter must be provided and be non empty");
        _logger.LogError("argument exception", ex);
        throw ex;
    }

    private TimeSpan GetDurationOrDefault(IDictionary props, TimeSpan defaultValue = default)
    {
        if (props[CarbonAwareConstants.Duration] is int duration)
        {
            return TimeSpan.FromMinutes(duration);
        }
        return defaultValue;
    }
}
