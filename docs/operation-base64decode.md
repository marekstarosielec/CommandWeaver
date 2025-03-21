# Base64DecodeOperation

The `Base64Decode` operation decodes a Base64-encoded string into plain text and saves the result into a variable. It is useful for handling encoded values passed between systems or stored in configuration.

---

> 👉 This operation only supports decoding UTF-8 encoded Base64 strings.  
> 👉 If the input is not a valid Base64 string, the operation will terminate with an error.  
> 👉 The decoded result is saved into a variable using the name provided in `saveTo`.

---

## **Parameters**

| Parameter | Description                                      | Required | Allowed Values |
|-----------|--------------------------------------------------|----------|----------------|
| **value** | The Base64-encoded string to decode.             | ✅ Yes   | `text`         |
| **saveTo**| Name of the variable to store the decoded result.| ✅ Yes   | `text`         |

---

## **Usage**

### **Example**
```json
{
  "commands": [
    {
      "name": "decodeExample",
      "operations": [
        {
          "operation": "Base64Decode",
          "value": "SGVsbG8gd29ybGQh",
          "saveTo": "decodedText"
        },
        {
          "operation": "Output",
          "value": "{{ decodedText }}"
        }
      ]
    }
  ]
}
```

### Explanation
- The encoded value `SGVsbG8gd29ybGQh` is Base64 for `Hello world!`.
- The decoded result is saved to the variable `decodedText`.
- The `Output` operation prints `"Hello world!"`.