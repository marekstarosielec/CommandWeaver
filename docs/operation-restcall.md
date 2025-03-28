# RestCall Operation

The RestCall operation allows performing HTTP requests to interact with APIs. It supports specifying request details such as headers, body, timeout, and authentication using certificates.

---

## Parameters

| Parameter | Description | Required | Allowed Values                          |
|-----------|-------------|----------|-----------------------------------------|
| **url** | The endpoint of the API to call. | ✅ Yes | `text`                                  |
| **method** | The HTTP method for the request. | ✅ Yes | `GET`, `POST`, `PUT`, `DELETE`, `PATCH` |
| **headers** | Metadata for the request. | No | List of key-value pairs                 |
| **body** | Data sent with the request. | No | `text`                             |
| **timeout** | Seconds to wait for a response before failing. | No | `number`                                 |
| **certificate** | Path to the certificate file or a resource. | No | Certificate can be loaded from a [resource](resource.md).|
| **certificatePassword** | Password for the certificate if required. | No | `text`                                  |

---

## Usage

A RestCall operation performs an API request and stores the response in the `lastRestCall` variable.

### Example:
```json
{
  "commands": [
    {
      "name": "restTest",
      "category": "tests",
      "operations": [
        {
          "operation": "RestCall",
          "method": "POST",
          "certificate": {
            "fromResource": "BuiltIn\\TestCertificate.pfx"
          },
          "certificatePassword": "test",
          "url": "https://jsonplaceholder.typicode.com/posts",
          "timeout": 2,
          "headers": [
            { 
              "name": "Accept",
              "value": "application/json",
              "conditions": {
                "IsNull": "{{ abc }}"
              }
            }
          ],
          "body": {
            "title": "foo",
            "body": "bar",
            "userId": 1
          }
        },
        {
          "operation": "Output",
          "value": "[[#FF0000]]{{rest_request.body_json.userId}}"
        }
      ]
    }
  ]
}
```

### Explanation:
- RestCall operation sends a `POST` request to `https://jsonplaceholder.typicode.com/posts`.
- A certificate (`TestCertificate.pfx`) is used for authentication.
- Request times out if no response is received within 2 seconds.
- Request body contains `"title"`, `"body"`, and `"userId"` fields.
- Header `"Accept": "application/json"` is included **only if `abc` is null**.
- Request and response is stored in the `lastRestCall` variable.
- Output operation prints the `userId` from the last RestCall request body in **red**.

## Events

The `events` section allows executing additional operations before sending the request and after receiving the response. This can be useful for logging, modifying requests, or processing responses dynamically.

---

### Supported Events
| Event | Description | Available variables             |
|--------|-------------|---------------------------------|
| **requestPrepared** | Executes operations before sending the request. | `rest_request`                  |
| **responseReceived** | Executes operations after receiving the response. | `rest_request`, `rest_response` |

---

### Example:
```json
"events": {
  "requestPrepared": [
    {
      "operation": "Output",
      "value": "Request is being prepared..."
    }
  ],
  "responseReceived": [
    {
      "operation": "Output",
      "value": "Response received successfully."
    }
  ]
}
```

### Explanation:
- `requestPrepared`: Runs before sending the request and prints `"Request is being prepared..."`.
- `responseReceived`: Runs after receiving the response and prints `"Response received successfully."`.

## Available Variables

During the execution of a `RestCall` operation, the variables `rest_request` and `rest_response` are created. These variables store details about the request and response, making them accessible in subsequent operations.

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

---

### Properties of `rest_response`
| Property | Description |
|----------|-------------|
| **status** | The HTTP status code of the response (e.g., `200`, `404`). |
| **created** | The timestamp when the response was received (ISO 8601 format). |
| **success** | A boolean indicating whether the response status code represents success (`true` for 2xx codes). |
| **body** | The raw response body as a string. |
| **body_json** | The parsed JSON response body (if valid JSON). |
| **headers** | A list of headers, where each entry contains: |
| → `key` | Header name. |
| → `value` | Header value(s), joined into a string if multiple exist. |

## Logging Full Request and Response Details

To output the complete details of the request and response, built-in operations can be used in the `events` section.

### Example:
```json
"events": {
  "requestPrepared": [
    {
      "fromVariable": "output-request"
    }
  ],
  "responseReceived": [
    {
      "fromVariable": "output-response"
    }
  ]
}
```