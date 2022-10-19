using PropertyName = CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters.PropertyName;

namespace CarbonAware.Aggregators.CarbonAware;

public class ParametersValidator
{
    public enum ValidationName { StartBeforeEnd, StartRequiredIfEnd };

    private readonly CarbonAwareParameters _parameters;

    private readonly List<Validator> _validations;

    public ParametersValidator(CarbonAwareParameters _parameters)
    {
        this._parameters = _parameters;
        _validations = new List<Validator>();
    }

    /// <summary>
    /// Accepts any PropertyNames as arguments and sets the associated property as required for validation.
    /// </summary>
    /// <param name="requiredProperties"></param>
    public void SetRequiredProperties(params PropertyName[] requiredProperties)
    {
        foreach (var propertyName in requiredProperties)
        {
            _parameters._props[propertyName].IsRequired = true;
        }
    }

    /// <summary>
    /// Accepts any ValidationName as arguments and sets the associated validation to check.
    /// </summary>
    /// <param name="validationName"></param>
    public void SetValidations(params ValidationName[] validationNames)
    {
        foreach (var validationName in validationNames)
        {
            switch (validationName)
            {
                case ValidationName.StartBeforeEnd:
                    _validations.Add(StartBeforeEnd());
                    break;
                case ValidationName.StartRequiredIfEnd:
                    _validations.Add(StartRequiredIfEnd());
                    break;
            }
        }
    }

    /// <summary>
    /// Validates the properties and relationships between properties. Any validation errors found are packaged into an
    /// ArgumentException and thrown. If there are no errors, simply returns void. 
    /// </summary>
    /// <remarks> Validation includes two checks.
    ///  - Check that required properties are set
    ///  - Check that specified validations (like start < end) are true
    ///  If any validation errors are found during property validation, with throw without validating property relationships
    /// </remarks>
    public void Validate()
    {
        // Validate Properties
        var errors = new Dictionary<string, List<string>>();
        foreach (var propertyName in CarbonAwareParameters.GetPropertyNames())
        {
            var property = _parameters._props[propertyName];
            if (!property.IsValid) { errors.AppendValue(property.DisplayName, $"{property.DisplayName} is not set"); }
        }

        // Assert no property validation errors before validating relationships. Throws if any errors.
        AssertNoErrors(errors);

        // Check validations
        foreach (var validation in _validations)
        {
            if (!validation.IsValid()) errors.AppendValue(validation.ErrorKey!, validation.ErrorMessage!);
        }

        // Assert no relationship validation errors. Throws if any errors.
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
            ArgumentException error = new ArgumentException("Invalid _parameters");
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

    private Validator StartBeforeEnd()
    {
        return new Validator(
            ValidationName.StartBeforeEnd,
            () => _parameters.Start < _parameters.End,
            _parameters._props[PropertyName.Start].DisplayName,
            $"{_parameters._props[PropertyName.Start].DisplayName} must be before {_parameters._props[PropertyName.End].DisplayName}"
        );
    }

    private Validator StartRequiredIfEnd()
    {
        return new Validator(
            ValidationName.StartRequiredIfEnd,
            () => !(_parameters._props[PropertyName.End].IsSet && !_parameters._props[PropertyName.Start].IsSet),
            _parameters._props[PropertyName.Start].DisplayName,
            $"{_parameters._props[PropertyName.Start].DisplayName} must be defined if {_parameters._props[PropertyName.End].DisplayName} is defined"
        );
    }
}


