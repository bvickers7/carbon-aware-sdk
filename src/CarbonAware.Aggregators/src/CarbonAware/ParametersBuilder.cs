using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;
using static CarbonAware.Aggregators.CarbonAware.ParametersValidator;
using PropertyName = CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters.PropertyName;

namespace CarbonAware.Aggregators.CarbonAware;
/// <summary>
/// Single class builder that does field validation real-time as users try to set it based on instantiated ParameterType
/// </summary>
public class ParametersBuilder
{
    private readonly CarbonAwareParametersBaseDTO baseParameters;
    private readonly ParameterType parameterType;

    public ParametersBuilder(ParameterType type)
    {
        baseParameters = new CarbonAwareParametersBaseDTO();
        parameterType = type;
    }

    public ParametersBuilder(CarbonAwareParametersBaseDTO parameters, ParameterType type)
    {
        baseParameters = parameters;
        parameterType = type;
    }

    public CarbonAwareParameters Build()
    {
        CarbonAwareParameters parameters = baseParameters;
        parameters.SetupValidator(parameterType);
        parameters.Validate();
        return parameters;
    }

    public void AddStartTime(DateTimeOffset start)
    {
        baseParameters.Start = start;
    }
    public void AddEndTime(DateTimeOffset end)
    {
        baseParameters.End = end;
    }
    public void AddLocation(string location)
    {
        AddLocations(new string[] { location });
    }

    public void AddLocations(string[] locations)
    {
        switch (parameterType)
        {
            case ParameterType.EmissionsParameters:
            case ParameterType.CurrentForecastParameters:
                {
                    if (locations.Any())
                    {
                        baseParameters.MultipleLocations = locations;
                    }
                    break;
                }
            case ParameterType.ForecastParameters:
            case ParameterType.CarbonIntensityParameters:
                {
                    if (locations.Any() && locations.Length == 1)
                        {
                            baseParameters.SingleLocation = locations[0];
                        }
                    break;
                }
        }
    }

    public void AddDuration(int duration)
    {
        baseParameters.Duration = duration;
    }
}