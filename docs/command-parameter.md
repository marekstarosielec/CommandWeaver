# Command Parameters

**Command Parameters** allow customization of commands by enabling users to pass arguments when executing **CommandWeaver**. Parameters define what input a command expects.
Parameter can be stored inside variable, so it can be reused in multiple commands.

Each **Command Parameter** consists of:
- **Key** – The unique identifier for the parameter.
- **Enabled** – Determines if the parameter is active *(Defaults to `true`)*.
- **Description** – Explains the purpose of the parameter.
- **Validation** – Defines constraints like allowed values. *(See [Validation](validation.md) for details.)*
- **IfNull** – Specifies fallback values if the parameter is not provided.

👉 Every filled command parameter is saved as a [variable](variable.md) with the same name. The [variable's scope](variable-scope.md) is "Command".

---

## Defining Command Parameters

Command parameters are defined inside the `parameters` section of a command.

### Example:
```json
{
    "commands": [
        {
            "name": "configure",
            "parameters": [
                {
                    "name": "mode",
                    "description": "Defines the operating mode",
                    "validation": {
                        "allowedTextValues": ["auto", "manual"]
                    },
                    "ifNull": {
                        "value": "auto"
                    }
                },
                {
                    "fromVariable": "default-parameter"
                }
            ],
            "operations": [
                {
                    "operation": "output",
                    "value": "Running in {{mode}} mode."
                }
            ]
        }
    ],
    "variables": [
        {
            "key": "default-parameter",
            "value": {
                "name": "type",
                "description": "Defines input type"
            }
        }
    ]
}
```

### Explanation:
- The `mode` parameter has a description to help users.
- The [validation](validation.md) ensures only `auto` or `manual` are accepted.
- If no value is provided, `auto` is used as the fallback (`ifNull`).
- When a value is passed, it is stored as a [variable](variable.md) named `mode` with [scope](variable-scope.md) `Command`. 
- Second parameter is loaded from variable named `default-parameter`.
