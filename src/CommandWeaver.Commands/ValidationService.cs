public interface IValidationService
{
    void Validate(Validation? validatable, DynamicValue valueToValidate, string parameterKey);
}

public class ValidationService(IFlowService flowService) : IValidationService
{
    public void Validate(Validation? validation, DynamicValue valueToValidate, string parameterKey)
    {
        if (validation == null)
            return;
        
        if (validation.Required && valueToValidate.IsNull())
            flowService.Terminate($"Parameter '{parameterKey}' is required.");

        if (validation.List != true)
            SingleValueValidation(validation, valueToValidate, parameterKey);
        else
        {
            //List values validation
            if (valueToValidate.IsNull())
                return;
            
            if (valueToValidate.ListValue == null)
                flowService.Terminate($"Parameter '{parameterKey}' requires list of values.");

            foreach (var listElement in valueToValidate.ListValue!)
                SingleValueValidation(validation, listElement, parameterKey); 
        }
    }

    private void SingleValueValidation(Validation validation, DynamicValue valueToValidate, string parameterKey)
    {
        //Single value validation
        AllowedTextValuesValidation(validation, valueToValidate, parameterKey);

        AllowedEnumValuesValidation(validation, valueToValidate, parameterKey);

        AllowedTypeValidation(validation, valueToValidate, parameterKey);
        
        AllowedStrongTypeValidation(validation, valueToValidate, parameterKey);
    }

    private void AllowedTypeValidation(Validation validation, DynamicValue valueToValidate, string parameterKey)
    {
        if (valueToValidate.IsNull())
            return;
        switch (validation.AllowedType?.ToLower())
        {
            case "text" when valueToValidate.TextValue == null:
                flowService.Terminate($"'{parameterKey}' requires text value.");
                break;
            case "number" when valueToValidate.NumericValue == null:
                flowService.Terminate($"'{parameterKey}' requires number.");
                break;
        }
    }
    
    private void AllowedStrongTypeValidation(Validation validation, DynamicValue valueToValidate, string parameterKey)
    {
        if (valueToValidate.IsNull())
            return;
        
        
    }

    private void AllowedEnumValuesValidation(Validation validation, DynamicValue valueToValidate, string parameterKey)
    {
        if (validation.AllowedEnumValues != null && valueToValidate.TextValue != null &&
            !Enum.GetNames(validation.AllowedEnumValues).Any(name =>
                name.Equals(valueToValidate.TextValue, StringComparison.OrdinalIgnoreCase)))
            flowService.Terminate($"Invalid value for argument '{parameterKey}'.");
    }

    private void AllowedTextValuesValidation(Validation validation, DynamicValue valueToValidate, string parameterKey)
    {
        if (validation.AllowedTextValues != null && valueToValidate.TextValue != null &&
            !validation.AllowedTextValues.Any(value =>
                string.Equals(value, valueToValidate.TextValue, StringComparison.OrdinalIgnoreCase)))
            flowService.Terminate(
                $"Invalid value for argument '{parameterKey}'. Allowed values: {string.Join(", ", validation.AllowedTextValues)}.");
    }
}