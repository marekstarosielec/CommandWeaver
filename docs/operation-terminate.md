# Terminate Operation

The Terminate operation stops command execution immediately. An optional message can be displayed before termination.

---

## Parameters

| Parameter | Description | Required | Allowed Values |
|-----------|-------------|----------|---------------|
| **message** | The message displayed before terminating execution. | No | `text` |

---

## Usage

The Terminate operation can be used to stop execution if certain conditions are met.

### Example:
```json
{
  "commands": [
    {
      "name": "terminate-example",
      "operations": [
        {
          "operation": "Terminate",
          "message": "Critical error encountered. Stopping execution.",
          "conditions": {
            "AreEqual": {
               "value1": "{{errorLevel}}",
               "value2": "critical"
            }
          }
        },
        {
          "operation": "Output",
          "value": "This will not be printed if errorLevel is 'critical'."
        }
      ]
    }
  ]
}
```

### Explanation:
- Terminate operation checks if `errorLevel` is `critical`.
- If `errorLevel` is `critical`, the message `"Critical error encountered. Stopping execution."` is printed, and execution stops.
- If `errorLevel` is not `critical`, Output operation runs as usual.