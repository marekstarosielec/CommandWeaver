# Resources

A **resource** is any file inside a repository. CommandWeaver distinguishes between **JSON resources** that define commands and variables and **other resources** that can be used in different operations.

---

## Resource Types

### JSON Resources
- Automatically scanned for commands and variables.
- Loaded into CommandWeaver*just like any other repository file.

### Other Resources
- Not scanned for commands or variables.
- Accessible for specific tasks, such as providing a certificate for an HTTPS REST connection.

---

## Using Non-JSON Resources

Some operations can reference **non-JSON resources** stored inside a repository.

### Example (Using a Certificate in a REST Call):
```json
{
  "operation": "RestCall",
  "method": "POST",
  "certificate": {
    "fromResource": "BuiltIn\\TestCertificate.pfx"
  },
  "certificatePassword": "test",
  "url": "https://api.example.com/secure"
}
