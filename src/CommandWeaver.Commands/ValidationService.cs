public interface IValidationService
{
    void Validate(IValidatable validatable, DynamicValue valueToValidate, string parameterKey);
}

public class ValidationService(IFlowService flowService) : IValidationService
{
    public void Validate(IValidatable validatable, DynamicValue valueToValidate, string parameterKey)
    {
        if (validatable.Required && valueToValidate.IsNull())
            flowService.Terminate($"Parameter '{parameterKey}' is required.");

        if (validatable.List != true)
            SingleValueValidation(validatable, valueToValidate, parameterKey);
        else
        {
            //List values validation
            if (valueToValidate.IsNull())
                return;
            
            if (valueToValidate.ListValue == null)
                flowService.Terminate($"Parameter '{parameterKey}' requires list of values.");

            foreach (var listElement in valueToValidate.ListValue!)
                SingleValueValidation(validatable, listElement, parameterKey);
        }
    }

    private void SingleValueValidation(IValidatable validatable, DynamicValue valueToValidate, string parameterKey)
    {
        //Single value validation
        AllowedTextValuesValidation(validatable, valueToValidate, parameterKey);

        AllowedEnumValuesValidation(validatable, valueToValidate, parameterKey);

        AllowedTypeValidation(validatable, valueToValidate, parameterKey);
    }

    private void AllowedTypeValidation(IValidatable validatable, DynamicValue valueToValidate, string parameterKey)
    {
        if (valueToValidate.IsNull())
            return;
        switch (validatable.AllowedType?.ToLower())
        {
            case "text" when valueToValidate.TextValue == null:
                flowService.Terminate($"'{parameterKey}' requires text value.");
                break;
            //case "number":
        }
    }

    private void AllowedEnumValuesValidation(IValidatable validatable, DynamicValue valueToValidate, string parameterKey)
    {
        if (validatable.AllowedEnumValues != null && valueToValidate.TextValue != null &&
            !Enum.GetNames(validatable.AllowedEnumValues).Any(name =>
                name.Equals(valueToValidate.TextValue, StringComparison.OrdinalIgnoreCase)))
            flowService.Terminate($"Invalid value for argument '{parameterKey}'.");
    }

    private void AllowedTextValuesValidation(IValidatable validatable, DynamicValue valueToValidate, string parameterKey)
    {
        if (validatable.AllowedTextValues != null && valueToValidate.TextValue != null &&
            !validatable.AllowedTextValues.Any(value =>
                string.Equals(value, valueToValidate.TextValue, StringComparison.OrdinalIgnoreCase)))
            flowService.Terminate(
                $"Invalid value for argument '{parameterKey}'. Allowed values: {string.Join(", ", validatable.AllowedTextValues)}.");
    }
}