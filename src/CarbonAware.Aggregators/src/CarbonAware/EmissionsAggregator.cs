﻿using CarbonAware.Extensions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;

namespace CarbonAware.Aggregators.CarbonAware;

public class EmissionsAggregator : IEmissionsAggregator
{
    private static readonly ActivitySource Activity = new ActivitySource(nameof(EmissionsAggregator));
    private readonly ILogger<EmissionsAggregator> _logger;
    private readonly IEmissionsDataSource _dataSource;

    /// <summary>
    /// Creates a new instance of the <see cref="EmissionsAggregator"/> class.
    /// </summary>
    /// <param name="logger">The logger for the aggregator</param>
    /// <param name="dataSource">An <see cref="IEmissionsDataSource"> data source.</param>
    public EmissionsAggregator(ILogger<EmissionsAggregator> logger, IEmissionsDataSource dataSource)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._dataSource = dataSource;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(CarbonAwareParameters parameters)
    {
        using (var activity = Activity.StartActivity())
        {
            parameters.SetRequiredProperties(PropertyName.MultipleLocations);
            parameters.SetValidations(ValidationName.StartRequiredIfEnd);
            parameters.Validate();

            var locations = parameters.MultipleLocations;
            var start = parameters.GetStartOrDefault(DateTimeOffset.UtcNow);
            var end = parameters.GetEndOrDefault(start);

            return await this._dataSource.GetCarbonIntensityAsync(locations, start, end);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetBestEmissionsDataAsync(CarbonAwareParameters parameters)
    {
        parameters.SetRequiredProperties(PropertyName.MultipleLocations);
        parameters.SetValidations(ValidationName.StartRequiredIfEnd);
        parameters.Validate();

        var locations = parameters.MultipleLocations;
        var start = parameters.GetStartOrDefault(DateTimeOffset.UtcNow);
        var end = parameters.GetEndOrDefault(start);

        var results = await this._dataSource.GetCarbonIntensityAsync(locations, start, end);
        return GetOptimalEmissions(results);
    }

    /// <inheritdoc />
    public async Task<double> CalculateAverageCarbonIntensityAsync(CarbonAwareParameters parameters)
    {
        using (var activity = Activity.StartActivity())
        {
            parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End);
            parameters.Validate();

            var end = parameters.End;
            var start = parameters.Start;

            _logger.LogInformation("Aggregator getting average carbon intensity from data source");
            var emissionData = await this._dataSource.GetCarbonIntensityAsync(parameters.SingleLocation, start, end);
            var value = emissionData.AverageOverPeriod(start, end);
            _logger.LogInformation($"Carbon Intensity Average: {value}");

            return value;
        }
    }

    private static IEnumerable<EmissionsData> GetOptimalEmissions(IEnumerable<EmissionsData> emissionsData)
    {
        if (!emissionsData.Any())
        {
            return Array.Empty<EmissionsData>();
        }

        var bestResult = emissionsData.MinBy(x => x.Rating);

        IEnumerable<EmissionsData> results = Array.Empty<EmissionsData>();

        if (bestResult != null)
        {
            results = emissionsData.Where(x => x.Rating == bestResult.Rating);
        }

        return results;
    }
}
