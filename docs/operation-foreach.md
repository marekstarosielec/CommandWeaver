# ForEach Operation

The ForEach operation allows looping through a list and executing a set of operations for each element.

---

## Parameters

| Parameter  | Description | Required | Allowed Type |
|------------|-------------|----------|--------------|
| **list**   | The list to iterate through. | Yes | `list`        |
| **element** | Name of the variable where each element of the list will be stored during iteration. | Yes | `text`       |

Both parameters are required.

---

## Usage

The operation iterates over a list and executes its nested operations for each element.

### Example:
```json
{
    "commands": [
        {
            "name": "loop-example",
            "operations": [
                {
                    "operation": "ForEach",
                    "list": "{{items}}",
                    "element": "item",
                    "operations": [
                        {
                            "operation": "Output",
                            "value": "Processing: {{item}}"
                        }
                    ]
                }
            ]
        }
    ]
}
```

### Explanation:
- ForEach operation loops through the list stored in `items`.
- Each element from the list is stored in the variable `item`.
- Output operation prints `Processing: {{item}}` for each element.
