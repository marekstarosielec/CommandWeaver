# Repositories

Repositories are sources from which CommandWeaver loads [commands](command.md) and [variables](variable.md). They allow organizing and managing command definitions efficiently by storing them in JSON files on disk.

---

## Supported Repository Types


| Repository Type | Description |
|----------------|-------------|
| **Embedded** | Stores built-in commands and variables that come with CommandWeaver. These are predefined and cannot be modified. |
| **File** | Loads commands and variables from JSON files stored on disk, allowing users to define and modify them dynamically. |

## Checking Repository Paths

To determine where CommandWeaver loads [commands](command.md) and [variables](variable.md) from, run:

```sh
commandweaver paths
```
### Example Output (Windows):
```sh
Local path: C:\Users\YourUser\AppData\Local\CommandWeaver\Workspaces\Default\Application
Current session path: C:\Users\YourUser\AppData\Local\CommandWeaver\Workspaces\Default\Sessions\session1
```
### Example Output (macOS/Linux):
```sh
Local path: /home/youruser/.local/share/CommandWeaver/Workspaces/Default/Application
Current session path: /home/youruser/.local/share/CommandWeaver/Workspaces/Default/Sessions/session1
```

## File Repository Behavior

- All JSON files in the repository path are loaded automatically.
- The name of the file (except the `.json` extension) does not matter.
- Subdirectories inside the repository path are also scanned.
- All commands and variables from all files are merged into one big list.

This means you can organize your JSON files freely without worrying about filenames or directory structures.
