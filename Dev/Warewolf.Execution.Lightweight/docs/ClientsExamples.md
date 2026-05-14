# Client Examples — Curl Requests for wwexecution

Complete curl examples for acquiring tokens and calling secure workflows on the wwexecution Azure Function App. Covers every supported client type.

---

## Variables (replace with your values)

```bash
TENANT_ID="22222222-2222-2222-2222-222222222222"
RESOURCE_APP_ID="11111111-1111-1111-1111-111111111111"
SCOPE="api://${RESOURCE_APP_ID}/.default"
TOKEN_ENDPOINT="https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/token"
DEVICE_CODE_ENDPOINT="https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/devicecode"
FUNCTION_URL="https://wwexecution.azurewebsites.net"
```

---

## 1. Daemon / Service — Client Credentials Flow

Best for: background services, CI/CD pipelines, automated integrations.

### Acquire Token

```bash
CLIENT_ID="33333333-3333-3333-3333-333333333333"
CLIENT_SECRET="your-client-secret"

TOKEN=$(curl -s -X POST "${TOKEN_ENDPOINT}" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=${CLIENT_ID}&client_secret=${CLIENT_SECRET}&scope=${SCOPE}" \
  | jq -r '.access_token')

echo "Token: ${TOKEN}"
```

### Call Secure Workflow (GET)

```bash
curl -X GET "${FUNCTION_URL}/secure/Hello%20World.json?Name=Test" \
  -H "Authorization: Bearer ${TOKEN}"
```

### Call Secure Workflow (POST with JSON body)

```bash
curl -X POST "${FUNCTION_URL}/secure/ProcessOrder.json" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"OrderId": "ORD-001", "Customer": "Contoso", "Amount": 250.00}'
```

### Call Secure Workflow (POST with form data)

```bash
curl -X POST "${FUNCTION_URL}/secure/ProcessOrder.json" \
  -H "Authorization: Bearer ${TOKEN}" \
  -d "OrderId=ORD-001&Customer=Contoso&Amount=250"
```

### One-liner: Acquire + Call

```bash
curl -H "Authorization: Bearer $(curl -s -X POST "${TOKEN_ENDPOINT}" \
  -d "grant_type=client_credentials&client_id=${CLIENT_ID}&client_secret=${CLIENT_SECRET}&scope=${SCOPE}" \
  | jq -r '.access_token')" \
  "${FUNCTION_URL}/secure/Hello%20World.json?Name=ping"
```

---

## 2. SPA / Interactive User — Device Code Flow

Best for: CLI tools, headless terminals, testing delegated tokens.

### Step 1: Request Device Code

```bash
SPA_CLIENT_ID="44444444-4444-4444-4444-444444444444"

curl -s -X POST "${DEVICE_CODE_ENDPOINT}" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=${SPA_CLIENT_ID}&scope=${SCOPE}"
```

Response:
```json
{
  "device_code": "BAQABAAE...",
  "user_code": "ABCD1234",
  "verification_uri": "https://microsoft.com/devicelogin",
  "expires_in": 900,
  "interval": 5
}
```

**Action**: Open `https://microsoft.com/devicelogin` and enter the `user_code`.

### Step 2: Poll for Token

```bash
DEVICE_CODE="BAQABAAE..."  # from step 1

TOKEN=$(while true; do
  RESP=$(curl -s -X POST "${TOKEN_ENDPOINT}" \
    -d "grant_type=urn:ietf:params:oauth:grant-type:device_code&client_id=${SPA_CLIENT_ID}&device_code=${DEVICE_CODE}")

  ERROR=$(echo "$RESP" | jq -r '.error // empty')
  if [ -z "$ERROR" ]; then
    echo "$RESP" | jq -r '.access_token'
    break
  elif [ "$ERROR" = "authorization_pending" ]; then
    sleep 5
  else
    echo "Error: $ERROR" >&2
    break
  fi
done)
```

### Step 3: Call Workflow with Delegated Token

```bash
curl -X GET "${FUNCTION_URL}/secure/Hello%20World.json?Name=User" \
  -H "Authorization: Bearer ${TOKEN}"
```

---

## 3. Confidential Web App — Authorization Code Flow

Best for: server-side web applications (ASP.NET, Node.js).

> **Note**: The authorization code flow requires browser-based user interaction for the initial code grant. The curl examples below show only the token exchange step after receiving the auth code.

### Exchange Authorization Code for Token

```bash
WEB_CLIENT_ID="55555555-5555-5555-5555-555555555555"
WEB_CLIENT_SECRET="web-client-secret"
AUTH_CODE="0.AAAA..."  # received from redirect after user login
REDIRECT_URI="https://myapp.contoso.com/signin-oidc"

TOKEN=$(curl -s -X POST "${TOKEN_ENDPOINT}" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=authorization_code&client_id=${WEB_CLIENT_ID}&client_secret=${WEB_CLIENT_SECRET}&code=${AUTH_CODE}&redirect_uri=${REDIRECT_URI}&scope=${SCOPE}" \
  | jq -r '.access_token')
```

### Call Workflow

```bash
curl -X POST "${FUNCTION_URL}/services/SyncData.json" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"Region": "EMEA", "SinceDate": "2024-01-01"}'
```

### Refresh Token (when access token expires)

```bash
REFRESH_TOKEN="0.AAAA..."  # from initial token response

NEW_TOKEN=$(curl -s -X POST "${TOKEN_ENDPOINT}" \
  -d "grant_type=refresh_token&client_id=${WEB_CLIENT_ID}&client_secret=${WEB_CLIENT_SECRET}&refresh_token=${REFRESH_TOKEN}&scope=${SCOPE}" \
  | jq -r '.access_token')
```

---

## 4. ROPC — Resource Owner Password Credentials (Test/Dev Only)

> ⚠️ **NOT for production**. Does not support MFA. Use only for automated testing.

```bash
USERNAME="testuser@contoso.com"
PASSWORD="UserPassword123"

TOKEN=$(curl -s -X POST "${TOKEN_ENDPOINT}" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&client_id=${WEB_CLIENT_ID}&client_secret=${WEB_CLIENT_SECRET}&scope=${SCOPE}&username=${USERNAME}&password=${PASSWORD}" \
  | jq -r '.access_token')

curl -X GET "${FUNCTION_URL}/secure/Hello%20World.json" \
  -H "Authorization: Bearer ${TOKEN}"
```

---

## 5. Calling Different Route Types

### Public Routes (no token required)

```bash
# No Authorization header needed
curl "${FUNCTION_URL}/public/Hello%20World.json?Name=Anonymous"
```

### Secure Routes (token required)

```bash
curl -X GET "${FUNCTION_URL}/secure/MyWorkflow.json?param=value" \
  -H "Authorization: Bearer ${TOKEN}"
```

### Services Routes (token required)

```bash
curl -X POST "${FUNCTION_URL}/services/MyWorkflow.json" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"key": "value"}'
```

---

## 6. Token Inspection

### Decode JWT (bash)

```bash
echo "${TOKEN}" | cut -d. -f2 | base64 -d 2>/dev/null | jq .
```

### Expected Claims — Daemon Token (Client Credentials)

```json
{
  "aud": "api://11111111-1111-1111-1111-111111111111",
  "iss": "https://login.microsoftonline.com/22222222-.../v2.0",
  "oid": "daemon-sp-object-id",
  "roles": ["Permission.Execute", "Permission.View"],
  "exp": 1705000000
}
```

### Expected Claims — User Token (Delegated)

```json
{
  "aud": "api://11111111-1111-1111-1111-111111111111",
  "iss": "https://login.microsoftonline.com/22222222-.../v2.0",
  "preferred_username": "alice@contoso.com",
  "roles": ["Developers", "Permission.View", "Permission.Execute"],
  "scp": "user_impersonation",
  "exp": 1705000000
}
```

---

## 7. Error Handling

| HTTP Status | Meaning | Action |
|---|---|---|
| **200** | Success | Parse response body |
| **401** | No token / invalid / expired | Re-acquire token, retry once |
| **302** | Browser redirect to login | You're hitting a secure route without `Authorization` header from an API client; add the Bearer token |
| **403** | Token valid but access denied | Check `secure.config` — user/daemon needs the correct group + permissions for this workflow |
| **404** | Workflow not found | Verify the workflow name exists in the function app's Resources |

### Example: Handle 401 and retry

```bash
RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "Authorization: Bearer ${TOKEN}" \
  "${FUNCTION_URL}/secure/MyWorkflow.json")

if [ "$RESPONSE" = "401" ]; then
  # Re-acquire token
  TOKEN=$(curl -s -X POST "${TOKEN_ENDPOINT}" \
    -d "grant_type=client_credentials&client_id=${CLIENT_ID}&client_secret=${CLIENT_SECRET}&scope=${SCOPE}" \
    | jq -r '.access_token')

  # Retry
  curl -H "Authorization: Bearer ${TOKEN}" \
    "${FUNCTION_URL}/secure/MyWorkflow.json"
fi
```

---

## 8. PowerShell Equivalents

### Client Credentials

```powershell
$body = @{
    grant_type    = 'client_credentials'
    client_id     = $ClientId
    client_secret = $ClientSecret
    scope         = "api://$ResourceAppId/.default"
}
$token = (Invoke-RestMethod -Method POST -Uri $TokenEndpoint -Body $body).access_token

Invoke-RestMethod -Uri "$FunctionUrl/secure/Hello%20World.json?Name=Test" `
    -Headers @{ Authorization = "Bearer $token" }
```

### Full PowerShell script

See [`Scripts/Get-WwExecutionToken.ps1`](../Scripts/Get-WwExecutionToken.ps1) for complete examples of all grant types with error handling.

---

## See Also

- [Azure Provisioning — Function App](AzureProvisioning-FunctionApp.md)
- [Azure Provisioning — Client Apps](AzureProvisioning-ClientApps.md)
- [README-Authentication](README-Authentication.md) — full end-to-end guide
- [Example-ClientApps-OrdersSales.ps1](../Scripts/Example-ClientApps-OrdersSales.ps1) — working multi-client demo
