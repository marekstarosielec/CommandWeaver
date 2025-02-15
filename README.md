# CommandWeaver

🚀 **CommandWeaver** is a **highly configurable console application** designed to execute [**Commands**](docs/command.md) (defined in JSON format). These commands allow seamless interaction with **REST services, variable management, and automation workflows**, enabling flexible and script-driven execution.

CommandWeaver is designed with **modularity** in mind, allowing users to extend its functionality with **custom commands and operations**.

---

## ✨ Features

✅ **REST API Integration** – Make HTTP requests, process responses, and manage authentication.  
✅ **Variable System** – Use dynamic variables and expressions in scripts.  
✅ **Session Support** – Manage persistent variables across multiple executions.  
✅ **Customizable Logging** – Control log levels and output format.

---

## 🚀 Quick Start

To begin using **CommandWeaver**, follow these steps:

### 📌 Step 1: Determine File Paths
Run the following command to check where JSON scripts should be stored:
```sh
commandweaver paths

# Example Output (Windows):
# Local path: C:\Users\YourUser\AppData\Local\CommandWeaver\Workspaces\Default\Application
# Current session path: C:\Users\YourUser\AppData\Local\CommandWeaver\Workspaces\Default\Sessions\session1

# Example Output (macOS/Linux):
# Local path: /home/youruser/.local/share/CommandWeaver/Workspaces/Default/Application
# Current session path: /home/youruser/.local/share/CommandWeaver/Workspaces/Default/Sessions/session1
```

### 📌 Step 2: Create a Command Script
Save the following JSON file as `commands.json` in the `Local path` or `Current session path`:
```json
{
    "commands": [
        {
            "name": "say-hello",
            "operations": [
                {
                    "operation": "output",
                    "value": "Hello"
                }
            ]
        }
    ]
}
```
> **Note:** Name of the file is not important as long as it has `json` extension.

### 📌 Step 3: Execute the Command
```sh
commandweaver say-hello

# Expected Output:
# Hello
```

## 📖 Documentation

For more details on specific topics, check out the following:

- [Command](docs/command.md) – Overview of available commands and how to use them.
- [Command parameter](docs/command-parameter.md) – How to pass parameters to commands.
- [Operation](docs/operation.md) – Understanding operations and execution flow.
- [Operation parameter](docs/operation-parameter.md) – Defining and using operation parameters.
- [Variable](docs/variable.md) – Managing dynamic variables.
- [Variable scope](docs/variable-scope.md) – Understanding local vs global variables. 
- [Styling](docs/styling.md) – Customizing the output and logs.
- [Sessions](docs/session.md) – Handling session-based execution.
