# Setup `DEBUG_PRINCIPAL_TOKEN` for Local Debugging

When running the Warewolf Execution Lightweight engine **locally** (e.g. under
`func start` or the Visual Studio debugger), the Azure App Service EasyAuth
platform is not present, so no `X-MS-CLIENT-PRINCIPAL` header is injected into
inbound requests.

`DEBUG_PRINCIPAL_TOKEN` lets you pin a **real** Entra-authenticated identity
during local debugging so that `secure.config` permission checks, group
membership, and `apis.json` discovery filtering all behave identically to the
deployed App Service environment.

> **Security note** — this env var is only read when
> `AZURE_FUNCTIONS_ENVIRONMENT=Development`. It has no effect in staging or
> production environments.

---

## Prerequisites

- Access to the deployed App Service, e.g.
  `https://wwexecutiondev.azurewebsites.net`
- A browser or `curl`/PowerShell to call the `/.auth/me` endpoint
- PowerShell (any version ≥ 5.1) to base64-encode the payload

---

## Step 1 — Retrieve your `.auth/me` token

1. Open a browser and sign in to the deployed App Service using the Entra
   identity you want to debug as.
2. In the **same browser session**, navigate to:

   ```
   https://wwexecutiondev.azurewebsites.net/.auth/me
   ```

3. The response is a JSON array. It looks like this (abbreviated):

   ```json
   [
	 {
	   "access_token": "eyJ0eXAi...",
	   "expires_on": "2026-05-25T11:17:03Z",
	   "id_token": "eyJ0eXAi...",
	   "provider_name": "aad",
	   "user_claims": [
		 { "typ": "aud",    "val": "05b557d6-..." },
		 { "typ": "name",   "val": "Sehul Shah" },
		 { "typ": "roles",  "val": "wwusers" },
		 { "typ": "preferred_username", "val": "Sehul.Shah@theunlimited.co.za" },
		 ...
	   ],
	   "user_id": "Sehul.Shah@dev2.co.za"
	 }
   ]
   ```

4. Copy the **entire response** (the outer `[...]` array, including
   `access_token`, `user_claims`, etc.).

---

## Step 2 — Base64-encode the payload

Paste the copied JSON into a PowerShell variable and encode it:

```powershell
# Paste the full /.auth/me response array here
$json = '[{ "provider_name": "aad", "user_claims": [...], ... }]'

$b64 = [Convert]::ToBase64String(
	[System.Text.Encoding]::UTF8.GetBytes($json)
)

Write-Output $b64
```

Copy the single-line base64 string that is printed.

> **Tip** — if you saved the response to a file you can encode it directly:
>
> ```powershell
> $b64 = [Convert]::ToBase64String(
>     [System.IO.File]::ReadAllBytes('C:\temp\authme.json')
> )
> ```

---

## Step 3 — Set `DEBUG_PRINCIPAL_TOKEN` in `local.settings.json`

Open `Warewolf.Execution.Lightweight\local.settings.json` and paste the
base64 string as the value:

```json
{
  "IsEncrypted": false,
  "Values": {
	...
	"DEBUG_PRINCIPAL_TOKEN": "<paste-base64-string-here>"
  }
}
```

Make sure `AZURE_FUNCTIONS_ENVIRONMENT` is set to `Development` (this is the
default when running locally via `func start` or the VS debugger):

```json
"AZURE_FUNCTIONS_ENVIRONMENT": "Development"
```

---

## Step 4 — Start the function app

Run the function app as normal. On the first inbound request you will see a
log line confirming the principal was loaded:

```
[DebugPrincipal] Loaded fixed debug principal: User=Sehul.Shah@theunlimited.co.za Groups=[wwusers]
```

Every subsequent request will be authenticated as that identity without
requiring a live EasyAuth session.

---

## Supported input formats

The parser accepts **all three** shapes that can come out of `/.auth/me` or
the `X-MS-CLIENT-PRINCIPAL` header, so you can paste whichever is most
convenient:

| Format | How to obtain | Claims field |
|---|---|---|
| **EasyAuth v2 array** *(recommended)* | Full `/.auth/me` response | `user_claims` + `provider_name` |
| **EasyAuth v1 array** | `/.auth/me` on older App Service runtimes | `clientPrincipal.claims` + `clientPrincipal.auth_typ` |
| **Raw header value** | Value of `X-MS-CLIENT-PRINCIPAL` header | `claims` + `auth_typ` |

---

## Refreshing the token

`/.auth/me` tokens are short-lived (typically 1 hour). The `DEBUG_PRINCIPAL_TOKEN`
value is read **once at startup** and cached for the lifetime of the process,
so an expired token does not cause runtime errors — group membership is
evaluated from the claims inside the token, not by re-validating it against
Entra at request time.

To use a fresh identity or refresh group membership, repeat steps 1–3 and
**restart** the function app.

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `[DebugPrincipal] DEBUG_PRINCIPAL_TOKEN is set but could not produce an authenticated principal` | Value is not valid base64, or the decoded JSON does not match any known format | Re-encode with PowerShell as shown in Step 2; verify the raw JSON parses with `$json \| ConvertFrom-Json` |
| Principal is ignored; requests fall through to EasyAuth / Bearer parsers | `AZURE_FUNCTIONS_ENVIRONMENT` is not `Development` | Add `"AZURE_FUNCTIONS_ENVIRONMENT": "Development"` to `local.settings.json` |
| No `[DebugPrincipal]` log line at all | `DEBUG_PRINCIPAL_TOKEN` is empty or whitespace | Ensure the key has a non-empty value in `local.settings.json` |
| Permission checks still deny access | User's Entra roles do not match `secure.config` group names | Confirm that the `roles` claim value in `user_claims` matches a `GroupName` in `secure.config` |
