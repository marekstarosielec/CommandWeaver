# Conditions

**Conditions** establish a mechanism for dynamically determining whether an action should be performed, an element included, or an operation executed, based on predefined criteria.
By using the conditions mechanism, CommandWeaver allows dynamic execution control, ensuring that actions, data, and workflow behavior can be adapted based on runtime values.

The primary use of **conditions** is to control when an [operation](operation.md) should execute. However, **conditions** are also used in other parts of CommandWeaver.

---

## Available Conditions

Each condition checks a specific rule before executing the operation:

- **IsNull** – Executes the operation **only if a value is `null`**.
- **IsNotNull** – Executes the operation **only if a value is NOT `null`**.
- **AreEqual** – Executes the operation **only if two values are equal**.
- **AreNotEqual** – Executes the operation **only if two values are NOT equal**.

---

## Defining Conditions in Operations

Conditions are specified within the `conditions` property inside an operation.

### Example:
```json
{
    "commands": [
        {
            "name": "example-conditions",
            "operations": [
                {
                    "operation": "SetVariable",
                    "key": "username",
                    "value": "Guest",
                    "conditions": {
                        "isNull": "{{username}}"
                    }
                },
                {
                    "operation": "Output",
                    "value": "Welcome, {{username}}!"
                },
                {
                    "operation": "Terminate",
                    "conditions": {
                        "areEqual": {
                          "value1": "{{status}}",
                          "value2": "error"
                        }
                    }
                }
            ]
        }
    ]
}
```

### Explanation:
- The **first operation** sets `"username"` to `"Guest"` **only if `username` is null**.
- The **second operation** prints `"Welcome, {{username}}!"`, ensuring a username is always displayed.
- The **third operation** terminates execution **if `status` is `"error"`**.
