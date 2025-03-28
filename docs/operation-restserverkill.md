# RestServerKill Operation

The `RestServerKill` operation stops the currently running `RestServer`. It is used to terminate the server gracefully, ensuring that it no longer listens for incoming requests.

---

## **Usage**

Executing `RestServerKill` will immediately stop the active `RestServer`.

### **Example**
```json
{
  "commands": [
    {
      "name": "stopServer",
      "operations": [
        {
          "operation": "RestServerKill"
        }
      ]
    }
  ]
}
```

### Explanation
- Calls `RestServerKill` to stop the currently running `RestServer`.
- If no server is running, this operation does nothing.