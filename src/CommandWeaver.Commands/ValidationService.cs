using System.Collections;
using System.ComponentModel.DataAnnotations;

public interface IValidationService
{
    void Validate(Validation? validatable, DynamicValue valueToValidate, string parameterKey);
}

public class ValidationService : IValidationService
{
    public void Validate(Validation? validation, DynamicValue valueToValidate, string parameterKey)
    {
        if (validation == null)
            return;
        
        if (validation.Required && valueToValidate.IsNull())
            throw new CommandWeaverException($"Parameter '{parameterKey}' is required.");

        if (validation.List != true)
            SingleValueValidation(validation, valueToValidate, parameterKey);
        else
        {
            //List values validation
            if (valueToValidate.IsNull())
                return;
            
            if (valueToValidate.ListValue == null)
                throw new CommandWeaverException($"Parameter '{parameterKey}' requires list of values.");

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
                throw new CommandWeaverException($"'{parameterKey}' requires text value.");
            case "number" when valueToValidate.NumericValue == null:
                throw new CommandWeaverException($"'{parameterKey}' requires number.");
        }
    }

    private void AllowedStrongTypeValidation(Validation validation, DynamicValue valueToValidate, string parameterKey)
    {
        if (validation.AllowedStrongType == null)
            return;

        var targetType = validation.AllowedStrongType;
        try
        {
            valueToValidate.GetAsObject(targetType);
        }
        catch (Exception ex)
        {
            throw new CommandWeaverException(ex.Message);
        }
    }

    private void ValidateDynamicValue(DynamicValue value, Type targetType)
    {
        if (value.IsNull())
            return;

        // Skip validation if targetType is DynamicValue
        if (targetType == typeof(DynamicValue))
            return;

        // Skip validation if targetType is a collection of DynamicValue
        if ((targetType.IsGenericType &&
             typeof(IEnumerable<>).IsAssignableFrom(targetType.GetGenericTypeDefinition()) ||
             targetType.IsArray) && targetType.GetElementType() == typeof(DynamicValue) ||
            targetType.GenericTypeArguments.FirstOrDefault() == typeof(DynamicValue))
            return;

        // Validate primitive types
        if (targetType == typeof(string) && value.TextValue != null)
            return;
        if (targetType == typeof(bool) && value.BoolValue.HasValue)
            return;
        if (targetType == typeof(int) && value.NumericValue.HasValue)
            return;
        if (targetType == typeof(long) && value.NumericValue.HasValue)
            return;
        if (targetType == typeof(double) && value.PrecisionValue.HasValue)
            return;
        if (targetType == typeof(DateTimeOffset) && value.DateTimeValue.HasValue)
            return;

        // Validate collections and arrays
        if (typeof(IEnumerable).IsAssignableFrom(targetType) && value.ListValue != null)
        {
            var elementType = targetType.IsArray
                ? targetType.GetElementType()
                : targetType.GenericTypeArguments.FirstOrDefault();

            if (elementType != null)
            {
                foreach (var item in value.ListValue)
                    ValidateDynamicValue(item, elementType);
                return;
            }
        }

        // Validate enums
        if (targetType.IsEnum && value.TextValue != null && Enum.TryParse(targetType, value.TextValue, true, out _))
            return;

        // Validate complex objects
        if (value.ObjectValue != null)
        {
            var targetProperties = targetType.GetProperties()
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            var providedKeys = value.ObjectValue.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Detect unknown properties (case-insensitive)
            var unknownProperties =
                providedKeys.Except(targetProperties.Keys, StringComparer.OrdinalIgnoreCase).ToList();
            if (unknownProperties.Any())
                throw new InvalidCastException(
                    $"Parameter contains unknown properties that do not exist in {targetType.FullName}: {string.Join(", ", unknownProperties)}.");

            // Validate required properties
            var requiredProperties = targetProperties.Values
                .Where(p => p.GetCustomAttributes(typeof(RequiredAttribute), true).Any())
                .ToList();

            var missingRequiredProperties = requiredProperties
                .Where(p => !providedKeys.Any(k => k.Equals(p.Name, StringComparison.OrdinalIgnoreCase)) ||
                            value.ObjectValue[
                                providedKeys.First(k => k.Equals(p.Name, StringComparison.OrdinalIgnoreCase))].IsNull())
                .Select(p => p.Name)
                .ToList();

            if (missingRequiredProperties.Any())
                throw new InvalidCastException(
                    $"Missing required properties in {targetType.FullName}: {string.Join(", ", missingRequiredProperties)}.");

            // Validate nested properties
            foreach (var property in targetProperties.Values)
            {
                var matchingKey =
                    providedKeys.FirstOrDefault(k => k.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
                if (matchingKey != null && value.ObjectValue.ContainsKey(matchingKey))
                {
                    var propertyValue = value.ObjectValue[matchingKey];
                    ValidateDynamicValue(propertyValue, property.PropertyType);
                }
            }
        }
    }

    private void AllowedEnumValuesValidation(Validation validation, DynamicValue valueToValidate, string parameterKey)
    {
        if (validation.AllowedEnumValues != null && valueToValidate.TextValue != null &&
            !Enum.GetNames(validation.AllowedEnumValues).Any(name =>
                name.Equals(valueToValidate.TextValue, StringComparison.OrdinalIgnoreCase)))
            throw new CommandWeaverException($"Invalid value for argument '{parameterKey}'.");
    }

    private void AllowedTextValuesValidation(Validation validation, DynamicValue valueToValidate, string parameterKey)
    {
        if (validation.AllowedTextValues != null && valueToValidate.TextValue != null &&
            !validation.AllowedTextValues.Any(value =>
                string.Equals(value, valueToValidate.TextValue, StringComparison.OrdinalIgnoreCase)))
            throw new CommandWeaverException(
                $"Invalid value for argument '{parameterKey}'. Allowed values: {string.Join(", ", validation.AllowedTextValues)}.");
    }
}