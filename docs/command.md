# Commands

In **CommandWeaver**, a **Command** is an executable unit that defines a sequence of actions. Commands are stored in [repositories](repository.md) as JSON files and are responsible for executing a set of [operations](operation.md).

Each Command consists of:
- [Command Parameters](command-parameter.md) – These can be provided as arguments when executing CommandWeaver.
- A list of [Operations](operation.md) – Defines the actual actions that the command performs.

---

## Command Structure

A command is defined in a JSON file and follows this structure:

```json
{
    "commands": [
        {
            "name": "say-hello",
            "parameters": [
                {
                    "name": "name",
                    "type": "string",
                    "default": "World"
                }
            ],
            "operations": [
                {
                    "operation": "output",
                    "value": "Hello, {{name}}!"
                }
            ]
        }
    ]
}
