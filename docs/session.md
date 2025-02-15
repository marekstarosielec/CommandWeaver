# Sessions

A session is a directory where session-scoped variables are stored. Every command execution occurs within the context of a session, allowing access to session variables and application-scoped variables.

---

## Session Behavior

- Session-scoped variables are persisted in a dedicated folder.
- Each command runs within a session, ensuring access to session variables.
- Application-scoped variables remain accessible across all sessions.
- Switching sessions updates the execution context, affecting variable access.

---

## Switching Sessions

Users can switch to a different session by running the following command:

```sh
commandweaver switch-session -name my-session
```
### Example Output:
```sh
Current session is my-session
```

### Explanation:
- This command updates the current session to "my-session".
- A new subfolder is created inside the sessions directory to store session-scoped variables.
- The change persists until another session switch occurs.

## Running a Command in a Specific Session

Instead of switching sessions permanently, a command can be executed within a specific session by using the `-session` argument.

```sh
commandweaver run-task -session my-temp-session
```

### Explanation:
- The `-session` argument temporarily sets the session for that command execution.
- The session does not persist, meaning subsequent commands will revert to the default session.
- A subfolder for `my-temp-session` is created inside the sessions directory.
