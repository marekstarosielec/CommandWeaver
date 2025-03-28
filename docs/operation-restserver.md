# RestServer Operation

The `RestServer` operation sets up HTTP server that listens for incoming requests and executes predefined operations based on configured endpoints. It allows defining response behavior, custom event handling, and request processing.

---

👉 **Only one server can be started at the same time.**  
👉 **To stop the server, execute the** [`RestServerKill`](operation-restserverkill.md) **operation.**  
👉 **If the server is not stopped, the application will keep running until terminated manually (e.g., with a kill signal from the console).**

---

## Parameters

| Parameter   | Description                                                      | Required | Allowed Values                    |
|------------|------------------------------------------------------------------|----------|----------------------------------|
| **port**   | The port where the server listens for incoming requests.         | ✅ Yes   | `number`                         |
| **endpoints** | A list of endpoint definitions specifying request handling rules. | ✅ Yes   | List of `EndpointDefinition` |

---

## Usage

A `RestServer` operation starts an HTTP listener that processes requests and executes operations dynamically.

### **Example**
```json
{
  "commands": [
    {
      "name": "startServer",
      "operations": [
        {
          "operation": "RestServer",
          "port": 8080,
          "endpoints": [
            {
              "url": ["^/api/test$"],
              "responseCode": 200,
              "responseBody": "{ \"message\": \"Hello, world!\" }",
              "events": {
                "requestReceived": [
                  {
                    "operation": "Output",
                    "value": "Received a request at /api/test"
                  }
                ]
              }
            }
          ]
        }
      ]
    }
  ]
}
```

### Explanation
- `RestServer` starts listening on port **8080**.
- The endpoint **`/api/test`**:
    - Responds with a **200 OK** status code.
    - Returns a JSON response: `{ "message": "Hello, world!" }`.
    - Triggers a **requestReceived event**, which logs `"Received a request at /api/test"`.

## Events

The `events` section allows defining operations that execute when a request is received.

### **Supported Events**
| Event             | Description                                    | Available Variables |
|------------------|--------------------------------|------------------|
| **requestReceived** | Executes operations when a request is received at a matching endpoint. | `rest_request` |

---

### Example with Event Handling
```json
"events": {
  "requestReceived": [
    {
      "operation": "Output",
      "value": "Processing request..."
    },
    {
      "operation": "SetVariable",
      "name": "lastRequest",
      "value": "{{ rest_request.body_json }}"
    }
  ]
}
```
### Explanation
- Outputs `"Processing request..."` when a request is received.
- Saves the request body as a variable `lastRequest` for later use.

## Available Variables

When listener receives a request, the variable `rest_request` is created. That variable store details about the request, making it accessible in subsequent operations.

### Properties of `rest_request`
| Property | Description |
|----------|-------------|
| **method** | The HTTP method used in the request (e.g., `"GET"`, `"POST"`). |
| **url** | The full URL of the request. |
| **created** | The timestamp when the request was created (ISO 8601 format). |
| **body** | The raw request body as a string. |
| **body_json** | The parsed JSON request body (if valid JSON). |
| **headers** | A list of headers, where each entry contains: |
| → `key` | Header name. |
| → `value` | Header value(s), joined into a string if multiple exist. |
