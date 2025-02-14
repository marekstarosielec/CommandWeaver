# Output Operation

The Output operation prints a message or variable value to the console. It supports optional styling and log levels for formatted output.

---

## Parameters

| Parameter  | Description | Required | Allowed Values | Default Value |
|------------|-------------|----------|---------------|---------------|
| **value**  | The text or variable to print. | Yes | Any | — |
| **styling** | The type of styling to apply. | No | Possible values are explained below. | `Default` |
| **logLevel** | The logging level of the output, which determines if the message is printed based on the current log level setting. | No | Possible values are explained below. | — |

---

## Styling Options

| Value        | Description |
|-------------|-------------|
| **Markup** | Formats output using markup styling, which may include rich text or enhanced visual elements. |
| **MarkupLine** | Uses markup styling but ensures the output is displayed as a single line. |
| **Json** | Formats the output as structured JSON. |
| **Raw** | Outputs content exactly as it is, without formatting. |
| **Default** | Uses the default styling based on the output service implementation. |

---

## Log Level Options

The `logLevel` parameter controls whether a message is printed based on the current log level setting. If the log level of the message is **lower than the configured log level**, it will not be printed.

| Value        | Description |
|-------------|-------------|
| **Trace** | Detailed diagnostic information for tracing execution. |
| **Debug** | Diagnostic messages used for debugging. |
| **Information** | General runtime information about the application’s state. |
| **Warning** | Indicates a potential issue or noteworthy condition. |
| **Error** | Represents an error or critical issue that needs immediate attention. |

---

## Usage

The operation prints a message with optional styling and log levels.

### Example:
```json
{
    "commands": [
        {
            "name": "output-example",
            "operations": [
                {
                    "operation": "Output",
                    "value": "Hello, CommandWeaver!"
                },
                {
                    "operation": "Output",
                    "value": "Warning: Check configuration.",
                    "styling": "Markup",
                    "logLevel": "Warning"
                }
            ]
        }
    ]
}
```
### Explanation:
- First Output operation prints `"Hello, CommandWeaver!"` with default styling.
- Second Output operation prints `"Warning: Check configuration."` with `Markup` styling and a `Warning` log level.
- If the current log level is set to `Error`, the second message will **not** be printed since `Warning` is a lower level.
