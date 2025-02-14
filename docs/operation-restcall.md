# RestCall Operation

The RestCall operation allows performing HTTP requests to interact with APIs. It supports specifying request details such as headers, body, timeout, and authentication using certificates.

---

## Parameters

| Parameter | Description | Required | Allowed Values                          |
|-----------|-------------|----------|-----------------------------------------|
| **url** | The endpoint of the API to call. | ✅ Yes | `text`                                  |
| **method** | The HTTP method for the request. | ✅ Yes | `GET`, `POST`, `PUT`, `DELETE`, `PATCH` |
| **headers** | Metadata for the request. | No | List of key-value pairs                 |
| **body** | Data sent with the request. | No | JSON object                             |
| **timeout** | Seconds to wait for a response before failing. | No | `number`                                 |
| **certificate** | Path to the certificate file or a resource. | No | `text` / Resource                       |
| **certificatePassword** | Password for the certificate if required. | No | `text`                                  |

A certificate can be loaded from a [resource](resource.md).

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
          "value": "[[#FF0000]]{{lastRestCall.request.body.userId}}"
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
