# Validation

Validation is used to enforce constraints on command parameters and other input values. It ensures that only valid values are accepted based on predefined rules.

---

## Validation Properties

| Property | Description |
|----------|-------------|
| **Required** | Determines if the parameter must be provided. |
| **AllowedTextValues** | Specifies a list of accepted text values. |
| **AllowedEnumValues** | Restricts values to a predefined enum type. |
| **AllowedType** | Defines the expected data type (currently supports `text`). |
| **List** | Determines whether the parameter should be treated as a list. |

---

## Using Validation in Command Parameters

Validation rules can be applied to command parameters to enforce input constraints.

### Example:
```json
{
  "commands": [
    {
      "name": "validate-example",
      "parameters": [
        {
          "key": "mode",
          "required": true,
          "allowedTextValues": ["auto", "manual"]
        }
      ],
      "operations": [
        {
          "operation": "Output",
          "value": "Mode selected: {{mode}}"
        }
      ]
    }
  ]
}
```
### Explanation:
- `mode` parameter is required, meaning the command must include it.
- Only `"auto"` or `"manual"` are valid values for `mode`, any other input will cause an error.
- Output operation prints the selected mode if validation passes.
