using static CarbonAware.Aggregators.CarbonAware.ParametersBuilder;
using PropertyName = CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters.PropertyName;

namespace CarbonAware.Aggregators.CarbonAware;

public class ParametersValidator
{
    public enum ValidationName { 
        // Start < End
        StartBeforeEnd, 
        // if End, End && Start
        StartRequiredIfEnd };

    private readonly List<PropertyName> _requiredProperties;

    private readonly List<Func<CarbonAwareParameters, Validator>> _validations;

    public ParametersValidator()
    {
        _requiredProperties = new List<PropertyName>();
        _validations = new List<Func<CarbonAwareParameters, Validator>>();
    }

    /// <summary>
    /// Accepts any PropertyNames as arguments and sets the associated property as required for validation.
    /// </summary>
    /// <param name="requiredProperties"></param>
    public ParametersValidator SetRequiredProperties(params PropertyName[] requiredProperties)
    {
        _requiredProperties.AddRange(requiredProperties);
        return this;
    }

    /// <summary>
    /// Accepts any ValidationName as arguments and sets the associated validation to check.
    /// </summary>
    /// <param name="validationName"></param>
    public ParametersValidator SetValidations(params ValidationName[] validationNames)
    {
        foreach (var validationName in validationNames)
        {
            switch (validationName)
            {
                case ValidationName.StartBeforeEnd:
                    _validations.Add(StartBeforeEnd);
                    break;
                case ValidationName.StartRequiredIfEnd:
                    _validations.Add(StartRequiredIfEnd);
                    break;
            }
        }
        return this;
    }

    /// <summary>
    /// Takes in a CarbonAwarePArameters object and validates the properties and relationships between properties. Any validation errors found are packaged into an
    /// ArgumentException and thrown. If there are no errors, simply returns void. 
    /// </summary>
    /// <remarks> Validation includes two checks.
    ///  - Check that required properties are set
    ///  - Check that specified validations (like start < end) are true
    ///  If any validation errors are found during property validation, with throw without validating property relationships
    /// </remarks>
    public void Validate(CarbonAwareParameters parameters)
    {
        // Validate Properties
        var errors = new Dictionary<string, List<string>>();
        foreach (var propertyName in CarbonAwareParameters.GetPropertyNames())
        {
            var property = parameters._props[propertyName];
            if (_requiredProperties.Contains(propertyName)) property.IsRequired = true;
            if (!property.IsValid) { errors.AppendValue(property.DisplayName, $"{property.DisplayName} is not set"); }
        }

        // Assert no property validation errors before validating relationships. Throws if any errors.
        AssertNoErrors(errors);

        // Check validations
        foreach (var validation in _validations)
        {
            var validator = validation(parameters);
            if (!validator.IsValid()) errors.AppendValue(validator.ErrorKey!, validator.ErrorMessage!);
        }

        // Assert no validation errors. Throws if any errors.
        AssertNoErrors(errors);
    }

    /// <summary>
    /// Asserts there are no errors or throws ArgumentException.
    /// </summary>
    /// <param name="errors"> Dictionary of errors mapping the name of the parameter that caused the error to any associated error messages.</param>
    /// <remarks>All errors packed into a single ArgumentException with corresponding Data entries.</remarks>
    private static void AssertNoErrors(Dictionary<string, List<string>> errors)
    {
        if (errors.Keys.Count > 0)
        {
            var error = new ArgumentException("Invalid _parameters");
            foreach (KeyValuePair<string, List<string>> message in errors)
            {
                error.Data[message.Key] = message.Value.ToArray();
            }
            throw error;
        }
    }

    internal class Validator
    {
        private Func<bool> _predicate { get; init; }
        private string _errorKey { get; init; }
        private string _errorMessage { get; init; }
        public ValidationName Name { get; init; }

        // Contains a value if isValid() evaluates to false
        public string? ErrorKey { get; private set; }

        // Contains a value if isValid() evaluates to false
        public string? ErrorMessage { get; private set; }

        public Validator(ValidationName name, Func<bool> predicate, string errorKey, string errorMessage)
        {
            Name = name;
            _predicate = predicate;
            _errorKey = errorKey;
            _errorMessage = errorMessage;
        }

        /// <summary>
        /// Checks if the validator is valid
        /// </summary>
        /// <remarks> If result is false, will set ErrorKey and ErrorMessage property of the object. </remarks>
        public bool IsValid()
        {
            var result = _predicate();
            if (!result)
            {
                ErrorKey = _errorKey;
                ErrorMessage = _errorMessage;
            }
            return result;
        }
    }

    private Validator StartBeforeEnd(CarbonAwareParameters parameters)
    {
        return new Validator(
            ValidationName.StartBeforeEnd,
            () => parameters.Start < parameters.End,
            parameters._props[PropertyName.Start].DisplayName,
            $"{parameters._props[PropertyName.Start].DisplayName} must be before {parameters._props[PropertyName.End].DisplayName}"
        );
    }

    private Validator StartRequiredIfEnd(CarbonAwareParameters parameters)
    {
        return new Validator(
            ValidationName.StartRequiredIfEnd,
            () => !(parameters._props[PropertyName.End].IsSet && !parameters._props[PropertyName.Start].IsSet),
            parameters._props[PropertyName.Start].DisplayName,
            $"{parameters._props[PropertyName.Start].DisplayName} must be defined if {parameters._props[PropertyName.End].DisplayName} is defined"
        );
    }

    public static ParametersValidator EmissionsValidator() => new ParametersValidator()
        .SetRequiredProperties(PropertyName.MultipleLocations)
        .SetValidations(ValidationName.StartRequiredIfEnd, ValidationName.StartBeforeEnd);

    public static ParametersValidator CarbonIntensityValidator() => new ParametersValidator()
        .SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End)
        .SetValidations(ValidationName.StartBeforeEnd);

    public static ParametersValidator CurrentForecastValidator() => new ParametersValidator()
        .SetRequiredProperties(PropertyName.MultipleLocations)
        .SetValidations(ValidationName.StartBeforeEnd);

    public static ParametersValidator ForecastValidator() => new ParametersValidator()
        .SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Requested)
        .SetValidations(ValidationName.StartBeforeEnd);
}


