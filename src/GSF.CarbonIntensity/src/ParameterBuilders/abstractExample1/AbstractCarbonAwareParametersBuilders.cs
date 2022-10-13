using CarbonAware.Aggregators.CarbonAware;

namespace GSF.CarbonIntensity.Parameters;

/// <summary>
/// Abstract class with children builder where children do validation at `Build()` call by overriding implementation
/// </summary>
public abstract class AbstractCarbonAwareParametersBuilder
{
    internal CarbonAwareParametersBaseDTO parameters;

    internal string[]? locations;

    public AbstractCarbonAwareParametersBuilder() 
    {
        parameters = new CarbonAwareParametersBaseDTO();
    }

    public abstract CarbonAwareParameters Build();

    public AbstractCarbonAwareParametersBuilder AddStartTime(DateTimeOffset start) {
        parameters.Start = start;
        return this;
    }
    public AbstractCarbonAwareParametersBuilder AddEndTime(DateTimeOffset end) {
        parameters.End = end;
        return this;
    }
    public AbstractCarbonAwareParametersBuilder AddLocations(string[] locs) {
        locations = locs;
        return this;
    }

    public AbstractCarbonAwareParametersBuilder AddDuration(int duration) {
        parameters.Duration = duration;
        return this;
    }
}