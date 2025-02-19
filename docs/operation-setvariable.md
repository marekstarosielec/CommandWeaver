# SetVariable Operation

The SetVariable operation assigns a value to a variable, optionally specifying its scope and persistence.

---

## Parameters

| Parameter | Description | Required | Allowed Values |
|-----------|-------------|----------|---------------|
| **name** | The name of the variable to set. | ✅ Yes | `text` |
| **value** | The value to assign to the variable. | No | Any type |
| **scope** | The scope of the variable. If not specified, it defaults to the command scope. | No | `Command`, `Session`, `Application` |
| **id** | The optional file name where the variable should be stored. Ignored for `Command` scope variables. | No | `text` |

For more details on variable scopes, see [Variable Scope](variable-scope.md).

---

## Usage

The SetVariable operation creates or updates a variable.

### Example:
```json
{
  "commands": [
    {
      "name": "set-variable-example",
      "operations": [
        {
          "operation": "SetVariable",
          "name": "username",
          "value": "Alice",
          "id": "user-config.json"
        },
        {
          "operation": "Output",
          "value": "User: {{username}}"
        }
      ]
    }
  ]
}
```

### Explanation:
- SetVariable operation sets `Alice` as the value of the `username` variable.
- `id` parameter specifies `user-config.json`, meaning this variable may be stored for persistence.
- Output operation prints `User: Alice` using the stored variable.
- Since no scope is defined, the variable is stored in the **Command** scope, meaning `id` is ignored.


## Using Scope

A variable can be assigned a specific scope.

### Example:
```json
{
  "operation": "SetVariable",
  "name": "sessionUser",
  "value": "Alice",
  "scope": "Session",
  "id": "session-data.json"
}
```
### Explanation:
- `sessionUser` variable is stored at the **Session** scope, making it available throughout the session.
- `id` parameter specifies `session-data.json`, meaning the variable may be persisted in this file.
