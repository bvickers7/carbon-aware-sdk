using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;
using static CarbonAware.Aggregators.CarbonAware.ParametersValidator;
using PropertyName = CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters.PropertyName;

namespace CarbonAware.Aggregators.CarbonAware;
/// <summary>
/// Single class builder that does field validation real-time as users try to set it based on instantiated ParameterType
/// </summary>
public class ParametersBuilder
{
    public enum ParameterType { EmissionsParameters, CurrentForecastParameters, ForecastParameters, CarbonIntensityParameters }
    private readonly CarbonAwareParametersBaseDTO baseParameters;
    private readonly ParameterType parameterType;

    public ParametersBuilder(ParameterType type, CarbonAwareParametersBaseDTO? parameters = null)
    {
        baseParameters = parameters ?? new CarbonAwareParametersBaseDTO();
        parameterType = type;
    }

    public CarbonAwareParameters Build()
    {
        GetValidator(parameterType)
            .Validate(baseParameters);
        return baseParameters;
    }

    public ParametersBuilder AddStartTime(DateTimeOffset start)
    {
        baseParameters.Start = start;
        return this;
    }
    public ParametersBuilder AddEndTime(DateTimeOffset end)
    {
        baseParameters.End = end;
        return this;
    }
    public ParametersBuilder AddLocation(string location)
    {
        AddLocations(new string[] { location });
        return this;
    }

    public ParametersBuilder AddLocations(string[] locations)
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
        return this;
    }

    public ParametersBuilder AddDuration(int duration)
    {
        baseParameters.Duration = duration;
        return this;
    }

    public static ParametersValidator GetValidator(ParameterType type)
    {
        return type switch
        {
            ParameterType.EmissionsParameters => EmissionsValidator(),
            ParameterType.CurrentForecastParameters => CurrentForecastValidator(),
            ParameterType.ForecastParameters => ForecastValidator(),
            ParameterType.CarbonIntensityParameters => CarbonIntensityValidator(),
            _ => new ParametersValidator(),
        };
    }
}