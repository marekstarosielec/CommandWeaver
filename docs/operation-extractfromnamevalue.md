# ExtractFromNameValue Operation

The ExtractFromNameValue operation extracts a value from a **URL query string** or a **name-value collection** based on a specified key. The extracted value is stored in a new variable named `name_value`.

---

## Parameters

| Parameter | Description | Required | Allowed Type |
|-----------|-------------|----------|-------------|
| **input** | URL or name-value collection to search in. | Yes | `text` |
| **name**  | The name (key) to find within the input. | Yes | `text` |

Both parameters are required, and their values must be of type `text`.

---

## Usage

The operation searches for a key in the given input and extracts its associated value.

### Example:
```json
{
    "commands": [
        {
            "name": "extract-example",
            "operations": [
                {
                    "operation": "ExtractFromNameValue",
                    "input": "https://example.com?id=123&user=Alice",
                    "name": "user"
                },
                {
                    "operation": "Output",
                    "value": "Extracted user: {{name_value}}"
                }
            ]
        }
    ]
}
```

### Explanation:
- ExtractFromNameValue operation searches the provided URL for the key `user`.
- It finds `Alice` as the corresponding value and stores it in the variable `name_value`.
- Output operation then prints `"Extracted user: Alice"`, using the extracted value.
