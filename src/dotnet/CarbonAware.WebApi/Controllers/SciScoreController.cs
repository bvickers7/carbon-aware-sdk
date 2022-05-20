using CarbonAware.Aggregators.SciScore;
using CarbonAware.WebApi.Models;
using SerializableSciScore = CarbonAware.WebApi.Models.SciScore;
using CarbonAware.Model;
using CarbonAware.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Diagnostics;

namespace CarbonAware.WebApi.Controllers;

/// <summary>
/// Controller for the API routes that lead to retrieving the sci scores and marginal carbon intensities 
/// </summary>
[Route("sci-scores")]
[ApiController]
public class SciScoreController : ControllerBase
{
    private readonly ILogger<SciScoreController> _logger;
    private readonly ISciScoreAggregator _aggregator;

    private readonly ActivitySource _activitySource;

    public SciScoreController(ILogger<SciScoreController> logger, ISciScoreAggregator aggregator, ActivitySource activitySource)
    {
        _logger = logger;
        _aggregator = aggregator;
        _activitySource = activitySource;
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    /// <summary> Gets sci-score value, currently dummy function to keep consistency </summary>
    /// <param name="input"> input from JSON request converted to input object with location and time interval </param>
    /// <returns>Result of the call to the aggregator that calculates the sci score</returns>

    public async Task<IActionResult> CreateAsync(SciScoreInput input)
    {
        _logger.LogDebug("calculate sciscore with input: {input}", input);

        var score = await _aggregator.CalculateSciScoreAsync(input.Resources, input.TimeInterval);
        _logger.LogDebug("calculated sciscore values: {score}", score);
        return Ok(score);
    }

    [HttpPost("marginal-carbon-intensity")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    /// <summary> Gets the marginal carbon intensity value </summary>
    /// <param name="input"> input from JSON request converted to input object with location and time interval </param>
    /// <returns>Result of the call to the aggregator to retrieve carbon intenstiy</returns>
    public async Task<IActionResult> GetCarbonIntensityAsync(SciScoreInput input)
    {
        using (var activity = _activitySource.StartActivity(nameof(SciScoreController)))
        {
            _logger.LogDebug("calling to aggregator to calculate the average carbon intensity with input: {input}", input);

            // check that there is some location passed in
            if (input.Location == null)
            {
                var error = new CarbonAwareWebApiError() { Message = "Location is required" };
                _logger.LogDebug("get carbon intensity failed with error: {error}  with input {input}", error, input);
                return BadRequest(error);
            }

            // check that there is a time interval passed in
            if (String.IsNullOrEmpty(input.TimeInterval))
            {
                var error = new CarbonAwareWebApiError() { Message = "TimeInterval is required" };
                _logger.LogDebug("get carbon intensity failed with error: {error} with input {input}", error, input);
                return BadRequest(error);
            }
            try
            {
                var carbonIntensity = await _aggregator.CalculateAverageCarbonIntensityAsync(GetLocation(input.Location), input.TimeInterval);

                SerializableSciScore score = new SerializableSciScore
                {
                    MarginalCarbonIntensityValue = carbonIntensity,
                };
                _logger.LogDebug("calculated marginal carbon intensity: {score}", score);
                return Ok(score);
            }
            //for invalid inputs and broadly 3rd Party dependency exceptions
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured during marginal calculation execution of input :{input}", input);
                var error = new CarbonAwareWebApiError() { Message = ex.ToString() };
                return BadRequest(error);
            }
        }
    }


    [HttpPost("energy")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    /// <summary> Gets the energy consumption value </summary>
    /// 
    // {
    //   "timeInterval": "",
    //   "computeResources": [
    //     {
    //         "type": "CloudProvider",
    //         "provider": "Azure",
    //         "computeType": "Microsoft.Compute/virtualMachines",
    //         "computeId": "vm-MyVirtualMachine"
    //     },
    //     {
    //         "type": "RawData",
    //         "computeId": "myId",
    //         "powerConsumption": 0.012345
    //     }
    //   ]
    // }
    /// <param name="input"> input from JSON request converted to input object with location and time interval </param>
    /// <returns>Result of the call to the aggregator to estimate energy consumption</returns>
    public async Task<IActionResult> GetEnergyAsync(SciScoreInput input)
    {
        using (var activity = _activitySource.StartActivity(nameof(SciScoreController)))
        {
            // _logger.LogDebug("calling to aggregator to calculate the energy usage with input: {input}", input);
            // var resources = new List<CloudComputeResource>()
            // {
            //     new CloudComputeResource()
            //     {
            //         Name = "vm-CSEGreenHack-01",
            //         VmType = "Standard_E64d_v4"
            //     }
            // };

            // var data = new List<ComputeResourceUtilizationData>();
            // for (int i = 0; i < 6; i++)
            // {
            //     int mins = i * 10;
            //     var utilizationData = new ComputeResourceUtilizationData()
            //     {
            //         Timestamp = new DateTimeOffset(2022, 5, 18, (mins/60), (mins%60), 0, TimeSpan.Zero),
            //         CpuUtilizationPercentage = 0.99,
            //         Duration = TimeSpan.FromMinutes(10)
            //     };
            //     data.Add(utilizationData);
            // }

            // resources[0].UtilizationData = data;

            // var timeInterval = "2022-05-19T13:00:00Z/2022-05-19T14:00:00Z";

            var energy = await _aggregator.CalculateEnergyAsync(input.Resources, input.TimeInterval);

            SerializableSciScore score = new SerializableSciScore
            {
                EnergyValue = energy,
            };
            _logger.LogDebug("calculated energy: {score}", score);
            return Ok(score);
        }
    }

    /// Validate the user input location and convert it to the internal Location object.
    //  Throws ArgumentException if input is invalid.
    private Location GetLocation(LocationInput locationInput)
    {
        LocationType locationType;
        CloudProvider cloudProvider;

        if (!Enum.TryParse<LocationType>(locationInput.LocationType, true, out locationType))
        {
            _logger.LogError("Can't parse location type with location input: ", locationInput);
            throw new ArgumentException($"locationType '{locationInput.LocationType}' is invalid");
        }

        Enum.TryParse<CloudProvider>(locationInput.CloudProvider, true, out cloudProvider);
        var location = new Location
        {
            LocationType = locationType,
            Latitude = locationInput.Latitude,
            Longitude = locationInput.Longitude,
            CloudProvider = cloudProvider,
            RegionName = locationInput.RegionName
        };

        return location;
    }


}