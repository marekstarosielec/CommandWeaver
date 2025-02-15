# Operations

Operations are coded actions that **CommandWeaver** executes within a command. Each command can contain one or more operations, defining the sequence of tasks that should be performed.

Every operation has its own parameters, which control how the operation behaves.

---

## Supported Operations

CommandWeaver currently supports the following operations:

- [Block](operation-block.md) – Groups multiple operations and executes them as a unit.
- [ExtractFromNameValue](operation-extractfromnamevalue.md) – Extracts values from structured name-value pairs.
- [ForEach](operation-foreach.md) – Iterates over a list and executes nested operations for each item.
- [ListGroup](operation-listgroup.md) – Groups items in a list based on specified criteria.
- [Output](operation-output.md) – Prints a message or variable to the console.
- [RestCall](operation-restcall.md) – Performs HTTP requests to interact with REST APIs.
- [SetVariable](operation-setvariable.md) – Assigns a value to a variable for later use.
- [Terminate](operation-terminate.md) – Stops command execution based on specified conditions.

---

## Using Operations in Commands

Operations are defined within the `operations` section of a command.

### Example:
```json
{
    "commands": [
        {
            "name": "example-command",
            "operations": [
                {
                    "operation": "Output",
                    "value": "Hello, CommandWeaver!"
                },
                {
                    "operation": "SetVariable",
                    "key": "greeting",
                    "value": "Welcome!"
                },
                {
                    "operation": "Output",
                    "value": "{{greeting}}"
                }
            ]
        }
    ]
}
```

### Explanation:
- The first operation prints `"Hello, CommandWeaver!"` to the console.
- The second operation stores `"Welcome!"` in a variable named `greeting`.
- The third operation prints the value of `greeting`, which results in `"Welcome!"`.

## Loading Operations from Variables

CommandWeaver allows operations to be loaded from [variables](variable.md), making it possible to reuse predefined operations across multiple commands. This approach helps avoid duplication and ensures consistency.

### Example:
```json
{
    "commands": [
        {
            "name": "example-command",
            "operations": [
                {
                    "fromVariable": "common-validation"
                },
                {
                    "operation": "Output",
                    "value": "Processing completed."
                }
            ]
        }
    ],
    "variables": [
        {
            "key": "common-validation",
            "value": {
                "operation": "Terminate",
                "conditions": {
                    "IsNull": "{{requiredValue}}"
                }
            }
        }
    ]
}
```

### Explanation:
- First operation is dynamically loaded from the `common-validation` variable.
- Second operation prints `"Processing completed."` to the console.
- The variable `common-validation` contains a [Terminate](operation-terminate.md) operation that stops execution if `requiredValue` is null.

This mechanism enables flexible command composition, allowing commonly used operations to be defined once and reused across multiple commands.
