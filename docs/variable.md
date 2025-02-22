# Variables

Variables in **CommandWeaver** store values that can be referenced and modified during execution. They allow dynamic behavior by enabling commands and operations to work with runtime data.

## Purpose of Variables

Variables provide a way to:
- Store and reuse values across multiple operations.
- Pass data between different parts of a command.
- Dynamically modify execution flow based on runtime values.
- Use command parameters as named variables.

## Defining and Using Variables

Variables are referenced using double curly braces - `{{variableName}}`.

### Example:
```json
{
    "commands": [
        {
            "name": "example-variables",
            "operations": [
                {
                    "operation": "SetVariable",
                    "key": "greeting",
                    "value": "Hello"
                },
                {
                    "operation": "Output",
                    "value": "{{greeting}}, CommandWeaver!"
                }
            ]
        }
    ]
}
```
### Explanation:
- First operation sets a variable named `greeting` to `"Hello"`.
- Second operation prints `"Hello, CommandWeaver!"`, dynamically inserting the value of `greeting`.


## Variable Types

A variable in **CommandWeaver** can store different types of values:

| Type              | Description                                         | Example |
|-------------------|-----------------------------------------------------|---------|
| **Text (string)** | Stores textual data.                               | `"Hello"` |
| **Boolean**       | Stores `true` or `false`.                          | `true` |
| **Number**        | Stores integer values.                             | `42` |
| **Precision Number** | Stores decimal values.                         | `3.1415` |
| **Date**          | Stores a date or datetime value.                   | `"2024-02-10T12:30:00Z"` |
| **Object**        | A collection of key-value pairs (similar to JSON objects). | `{"name": "Alice", "age": 30}` |
| **List**          | An array of values, which can contain any of the above types. | `["apple", "banana", "cherry"]` |


## Variable Sources

Variables in **CommandWeaver** can come from different sources:
- **Command Parameters** – Every [Command Parameter](command-parameter.md) is automatically stored as a variable.
- **SetVariable Operations** – The [SetVariable](operation-setvariable.md) operation assigns new values.
- **REST Call Responses** – Data from API responses can be stored in variables.
- **System Variables** – Predefined values such as execution metadata.

---

## Modifying Variables

Variables can be updated by setting a new value using the `SetVariable` operation.

### Example:
```json
{
    "operation": "SetVariable",
    "key": "message",
    "value": "{{greeting}}, have a great day!"
}
```
This appends text to the existing value of greeting, resulting in `"Hello, have a great day!"`.

## Variable Scope

Each variable belongs to a specific **scope**, which defines where it is accessible. See [Variable Scope](variable-scope.md) for more details.

---

## Nested Variables

CommandWeaver **supports variable nesting**, allowing one variable to reference another dynamically.

### Example:
```json
{
    "operation": "Output",
    "value": "{{variable{{name}}Suffix}}"
}
```

If `name=="User"`, and `variableUserSuffix=="Welcome!"`, this resolves to:
```
Welcome!
```

This allows dynamic variable construction, making it easier to work with complex and structured data.

## Accessing Object Properties

When a variable contains an object, specific properties can be accessed using **dot notation**:  
`{{variable.property}}`

### Example:
```json
{
    "commands": [
        {
            "name": "example-object-variable",
            "operations": [
                {
                    "operation": "SetVariable",
                    "key": "user",
                    "value": {
                        "name": "Alice",
                        "age": 30,
                        "location": "New York"
                    }
                },
                {
                    "operation": "Output",
                    "value": "User: {{user.name}}, Age: {{user.age}}, Location: {{user.location}}"
                }
            ]
        }
    ]
}
```
### Explanation

- `SetVariable` operation stores an object in the `user` variable with three properties: `name`, `age`, and `location`.
- `Output` operation prints specific properties of `user` using dot notation.
- Expected output will be:  
  `User: Alice, Age: 30, Location: New York`

In an object, a property can contain another object, and users can access its nested properties using dot notation `{{object.property.subproperty}}`.

## Working with Lists

A variable can store a list containing any type of value, including text, numbers, objects, and nested lists.
If a list contains **objects**, and each object has a `key` property, the object can be accessed using `{{list[key].property}}` notation.

### Example:
```json
{
    "commands": [
        {
            "name": "list-object-key-example",
            "operations": [
                {
                    "operation": "SetVariable",
                    "key": "users",
                    "value": [
                        { "key": "alice", "name": "Alice", "age": 30 },
                        { "key": "bob", "name": "Bob", "age": 25 }
                    ]
                },
                {
                    "operation": "Output",
                    "value": "User: {{users[alice].name}}, Age: {{users[alice].age}}"
                }
            ]
        }
    ]
}
```

### Accessing List Elements by Index

Lists also support **index-based access**, allowing retrieval of specific elements using zero-based indexing.

#### Example:
```json
{
    "commands": [
        {
            "name": "list-index-example",
            "operations": [
                {
                    "operation": "SetVariable",
                    "key": "items",
                    "value": ["apple", "banana", "cherry"]
                },
                {
                    "operation": "Output",
                    "value": "First item: {{items[0]}}, Last item: {{items[2]}}"
                }
            ]
        }
    ]
}
```

#### Explanation:
- `SetVariable` creates a list `items` with three elements.
- `Output` retrieves elements using their index:
  - `{{items[0]}}` resolves to `"apple"`.
  - `{{items[2]}}` resolves to `"cherry"`.

This allows direct access to list elements without iteration.