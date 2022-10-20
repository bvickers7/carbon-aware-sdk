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
        ParametersValidator validator = SetupValidator(parameterType);
        validator.Validate(baseParameters);
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

    public static ParametersValidator SetupValidator(ParameterType type)
    {
        var validator = new ParametersValidator();
        validator.SetValidations(ValidationName.StartBeforeEnd);
        switch (type)
        {
            case ParameterType.EmissionsParameters:
                {
                    validator.SetRequiredProperties(PropertyName.MultipleLocations);
                    validator.SetValidations(ValidationName.StartRequiredIfEnd, ValidationName.StartBeforeEnd);
                    break;
                }
            case ParameterType.CurrentForecastParameters:
                {
                    validator.SetRequiredProperties(PropertyName.MultipleLocations);
                    validator.SetValidations(ValidationName.StartBeforeEnd);
                    break;
                }
            case ParameterType.ForecastParameters:
                {
                    validator.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Requested);
                    validator.SetValidations(ValidationName.StartBeforeEnd);
                    break;
                }
            case ParameterType.CarbonIntensityParameters:
                {
                    validator.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End);
                    validator.SetValidations(ValidationName.StartBeforeEnd);
                    break;
                }
        }
        return validator;
    }
}