# ListGroup Operation

The ListGroup operation groups elements in a list based on a specified property and stores the grouped result in a new variable.

---

## Parameters

| Parameter  | Description | Required | Allowed Type |
|------------|-------------|----------|--------------|
| **list**   | The list to group. | Yes | `list`        |
| **property** | The property used for grouping elements. | Yes | `text`       |
| **saveTo**  | The variable name where grouped values will be stored. | Yes | `text`       |

All parameters are required.

---

## Usage

The operation processes a list, groups elements by a specified property, and saves the result in a variable.

### Example:
```json
{
    "commands": [
        {
            "name": "group-example",
            "operations": [
                {
                    "operation": "SetVariable",
                    "key": "items",
                    "value": [
                        {"name": "Item1", "category": "A"},
                        {"name": "Item2", "category": "B"},
                        {"name": "Item3", "category": "A"}
                    ]
                },
                {
                    "operation": "ListGroup",
                    "list": "{{items}}",
                    "property": "category",
                    "saveTo": "groupedItems"
                },
                {
                    "operation": "ForEach",
                    "list": "{{groupedItems}}",
                    "element": "group",
                    "operations": [
                        {
                            "operation": "Output",
                            "value": "Group: {{group}}"
                        }
                    ]
                }
            ]
        }
    ]
}
```

### Explanation:
- SetVariable operation initializes `items` as a list containing elements with a `category` property.
- ListGroup operation groups elements in `items` based on the `category` property.
- Grouped values are stored in the variable `groupedItems`.
- ForEach operation iterates over `groupedItems` and prints each group.
