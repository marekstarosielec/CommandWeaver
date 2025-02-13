# Variable Scope

**Variable Scope** defines the lifespan and availability of a variable within CommandWeaver. It determines where and how long a variable can be accessed during execution.

There are three levels of variable scope:

- **Command Scope** – The variable exists only during a single command execution.
- **Session Scope** – The variable persists for all executions within current [session](session.md).
- **Application Scope** – The variable remains available across all sessions.

---

## Using Variable Scopes

When defining a variable, its scope determines how long it is retained.

### Example:
```json
{
    "operation": "SetVariable",
    "key": "username",
    "value": "Alice",
    "scope": "Session"
}
```
### Explanation:
- A variable `Alice` is created and remains available for all commands within current session.