# Block Operation

The **Block** operation allows grouping multiple operations together under a single condition. Instead of setting conditions individually for each operation, you can define a **Block** that contains multiple operations and applies a condition to the entire group.

---

## Usage

A Block operation is defined using the `operations` array inside a command.

### Example:
```json
{
    "commands": [
        {
            "name": "example-block",
            "operations": [
                {
                    "operation": "Block",
                    "conditions": {
                        "IsNotNull": "{{userId}}"
                    },
                    "operations": [
                        {
                            "operation": "Output",
                            "value": "User ID is valid."
                        },
                        {
                            "operation": "SetVariable",
                            "key": "status",
                            "value": "verified"
                        }
                    ]
                }
            ]
        }
    ]
}
```
### Explanation:
- **Block** operation groups multiple operations together.
- Condition `IsNotNull: "{{userId}}"` ensures that all contained operations execute only if `userId` is not null.
- If the condition is met:
    - The first operation prints `"User ID is valid."`.
    - The second operation sets `status` to `"verified"`.
