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

### 📌 Step 1: Compile application
To build CommandWeaver from source, navigate to the project directory and run:

```sh
dotnet build
```

After a successful build, the compiled executable can be found in the bin/Debug/netX.X/ directory.

### 📌 Step 2: Get the List of Available Commands
Run the following command to display all available built-in commands:
```sh
commandweaver help
```

Follow next steps to create your own command.

### 📌 Step 3: Determine File Paths
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

This will output a list of commands along with descriptions and available parameters.

### 📌 Step 4: Create a Command Script
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

### 📌 Step 5: Execute the Command
```sh
commandweaver say-hello

# Expected Output:
# Hello
```

## 📖 Documentation

For more details on specific topics, check out the following:

- [Command](docs/command.md) – Understanding what is command and how to use it.
- [Command parameter](docs/command-parameter.md) – How to define and validate parameters for commands.
- [Conditions](docs/conditions.md) – Rules that determine whether an operation should execute.
- [Operation](docs/operation.md) – Understanding operations and how they control execution flow.
- [Repository](docs/repository.md) – Sources from which commands and variables are loaded.
- [Resource](docs/resource.md) – Files stored in repositories, including JSON-based commands and external assets.
- [Session](docs/session.md) – Managing session-scoped variables and execution context.
- [Styling](docs/styling.md) – Customizing console output using text formatting and colors.
- [Validation](docs/validation.md) – Enforcing constraints on command parameters and operation inputs.
- [Variable](docs/variable.md) – Handling dynamic values and passing data between operations.
- [Variable scope](docs/variable-scope.md) – Defining the lifespan and accessibility of variables.
