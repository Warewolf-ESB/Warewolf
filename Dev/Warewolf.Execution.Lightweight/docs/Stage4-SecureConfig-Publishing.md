# Stage 4 — Deep dive: Publishing `secure.config` to wwexecution

This document expands Stage 4 of `EasyAuth-Runbook.md`. It covers the file
format, encryption, runtime resolution order, every supported deployment
option on a Free Consumption plan, verification, and the failure modes you
will hit if the file isn't where the runtime expects it.

---

## 4.0 What `secure.config` actually is

A single binary file deployed alongside the function package (or mounted
at runtime). It is the **only authoritative source** of "which group can do
what to which workflow" inside the lightweight engine — no portal, no
database, no Key Vault. Loss or corruption of this file forces the engine
into open-access mode.

**On disk:** a Base64 wrapper around an AES-CBC ciphertext whose plaintext
is UTF-8 JSON. The decrypted JSON deserialises to `Dev2.Common.SecuritySettingsTO`
and contains three fields the engine cares about:

| Field | Purpose |
|---|---|
| `SecretKey` | Base64-encoded HMAC-SHA256 key used to sign / validate Warewolf JWTs (`JwtGenerator` / `JwtValidator`). If empty, `SecureConfigLoader` generates a fresh one in-memory so the process can still issue tokens. |
| `WindowsGroupPermissions[]` | Flat list of permission rows. Each row binds a group / UPN to either a server-wide flag (`IsServer == true`) or a specific resource (`ResourceName != ""`). |
| `AuthenticationOverrideWorkflow` | Optional workflow name run by `LoginFunction` when callers POST to `/login`. |

**Why encrypted?** The file embeds the HMAC secret used to sign Warewolf-issued
JWTs, plus the precise authorization map (a useful target for an attacker).
Encryption isolates the disclosure risk from filesystem ACLs.

**Encryption:** uses `Dev2.Services.Security.SecurityEncryption.Encrypt / TryDecrypt`
from `Warewolf.Security.dll` — the same routine the Warewolf studio writes
with. The AES key is a platform secret baked into Warewolf assemblies; **do
not roll your own encryption** — files written outside this routine will
fail to decrypt.

---

## 4.1 How the runtime finds the file

Every line below maps to actual code in `Security/SecureConfigLoader.cs`.

```text
SecureConfigLoader.Config        // Lazy<T>, ExecutionAndPublication
   └─ Load()
        1. envPath = Environment.GetEnvironmentVariable("WAREWOLF_SECURE_CONFIG")
           if !string.IsNullOrWhiteSpace(envPath) → ReadConfig(envPath)
        2. binPath = Path.Combine(AppContext.BaseDirectory, "secure.config")
           ReadConfig(binPath)

   ReadConfig(path)
        if !File.Exists(path) → SecureConfigData.AllowAll  (IsLoaded = false)
        try
            encrypted = File.ReadAllText(path)
            decrypted = SecurityEncryption.TryDecrypt(encrypted)
            settings  = JsonConvert.DeserializeObject<SecuritySettingsTO>(decrypted)
            if settings.SecretKey is empty
                generate base64(HMACSHA256.Key)              ← in-memory fallback
            permissions = settings.WindowsGroupPermissions
                          .Select(p => new PermissionEntry(...))
            return SecureConfigData(IsLoaded=true,
                                    SecretKey=…,
                                    Permissions=…,
                                    // WOLF-8516: EntraTenantId/EntraAudience are read from the
                                    // merged WAREWOLF_ENTRA_CONFIG JSON app setting (parsed once
                                    // by Auth.Models.EntraIdentityOptions.FromEnvironment(), the
                                    // same parse EntraAuthOptions/ServiceBusEntraAuthOptions use)
                                    // instead of two independent env-var reads:
                                    EntraTenantId=EntraIdentityOptions.FromEnvironment().TenantId,
                                    EntraAudience=EntraIdentityOptions.FromEnvironment().Audience,
                                    LoginWorkflowName=…)
        catch
            return SecureConfigData.AllowAll                 ← any exception silenced
```

Two consequences worth internalising:

1. **Silent fallback.** A missing OR malformed file lands you in *open-access*
   mode. `SecureConfigData.IsLoaded == false` is the only signal — there is no
   exception. The mandatory verification in §4.6 catches this.
2. **Process-lifetime cache.** `Lazy<SecureConfigData>` reads exactly once per
   worker. Updating the file requires a process restart (`az functionapp restart`).
   On Consumption, that means waiting out the current cold-start window or
   forcing one.

`AppContext.BaseDirectory` on Windows Consumption resolves to
`D:\home\site\wwwroot\` for Functions running in-process and to
`D:\home\site\wwwroot\<appname>\` in some isolated configurations. Always
use `WAREWOLF_SECURE_CONFIG` in production to remove the ambiguity.

---

## 4.2 Generating the file

Two supported producers — both write the exact same binary format.

### 4.2.1 Warewolf Studio (interactive)

1. Open Warewolf Studio against any Warewolf Server (a workstation install is
   fine; the studio doesn't need to be connected to the production server).
2. *Settings → Security → Resource Permissions*.
3. For each row in `Scripts/secure.config.example.json`, add a matching entry:
   *Group / Resource / View / Execute / Contribute / DeployTo / DeployFrom /
   Administrator*.
4. Save. The studio writes the encrypted file to its server's bin directory.
5. Copy that `secure.config` into your deployment package (§4.3) or upload it
   to Azure (§4.4).

### 4.2.2 Programmatic CLI utility (recommended for CI)

A 30-line console app that references `Warewolf.Security.dll` and emits the
file deterministically from a JSON source you commit (the *plain-text* JSON,
**not** the encrypted bytes):

```csharp
// tools/SecureConfigPacker/Program.cs
using System.IO;
using Dev2.Services.Security;
using Newtonsoft.Json;

var sourceJson  = File.ReadAllText(args[0]);  // secure.config.example.json
var settings    = JsonConvert.DeserializeObject(sourceJson, typeof(object));
var encrypted   = SecurityEncryption.Encrypt(JsonConvert.SerializeObject(settings));
File.WriteAllText(args[1], encrypted);        // secure.config
Console.WriteLine($"Wrote {args[1]} ({new FileInfo(args[1]).Length} bytes)");
```

Build once, then in your pipeline:

```powershell
dotnet run --project tools/SecureConfigPacker -- `
    .\Scripts\secure.config.example.json `
    .\bin\publish\secure.config
```

The plain-text JSON stays in source control (or, for stricter shops, in
Key Vault and fetched at build time). Only the encrypted output is shipped.

---

## 4.3 Option A — Bundle with the publish package (recommended on Consumption)

This is the only option that works without paying for an upgrade and without
external storage. The file ships *inside* the zip the runtime extracts to
`D:\home\site\wwwroot\`.

### 4.3.1 Wire it into the csproj

Add to `Warewolf.Execution.Lightweight.csproj`:

```xml
<ItemGroup>
  <None Update="secure.config">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  </None>
</ItemGroup>
```

`CopyToPublishDirectory` is the important attribute — without it the file
ends up in `bin\Debug\net8.0\` but is silently dropped from the publish zip.

### 4.3.2 Layout after `dotnet publish`

```
bin\publish\
├─ host.json
├─ worker.config.json
├─ Warewolf.Execution.Lightweight.dll
├─ secure.config                ← bundled
├─ Resources\workflows\…
└─ …
```

### 4.3.3 Resolve to the bundled file

Either:

* Leave `WAREWOLF_SECURE_CONFIG` *unset* — `SecureConfigLoader` will fall
  back to `AppContext.BaseDirectory\secure.config` (Step 2 in §4.1).
* Or set it explicitly:

```powershell
az functionapp config appsettings set -n $AppName -g $Rg `
    --settings 'WAREWOLF_SECURE_CONFIG=D:\home\site\wwwroot\secure.config'
```

Explicit is better — removes ambiguity if Microsoft ever changes the base
directory of isolated workers.

### 4.3.4 Trade-offs

| Pros | Cons |
|---|---|
| Atomic deploy: code + policy ship together | Permission update needs a redeploy |
| No extra Azure resources, free tier–compatible | Encrypted blob in source control / build artefacts (mitigate with KV-fetched plaintext) |
| Same file on every cold-start, no race condition | Cannot be edited live by a non-developer |

---

## 4.4 Option B — Azure File share mount

> **Plan caveat.** Bring-your-own-storage mounts are supported on Functions
> *Premium* and *Dedicated (App Service)* plans. **Not on Consumption.** If
> you're on the free Y1 plan, skip this section and use §4.3 or §4.5.

### 4.4.1 Procedure (Premium / Dedicated only)

```powershell
$ShareName = 'warewolf-config'

az storage share-rm create `
    --resource-group $Rg --storage-account $Stg --name $ShareName --quota 1

# Encrypt locally (§4.2), then upload
az storage file upload `
    --account-name $Stg --share-name $ShareName `
    --source .\secure.config

# Mount the share on the function app
az webapp config storage-account add `
    -g $Rg -n $AppName --custom-id ware-cfg `
    --storage-type AzureFiles --account-name $Stg `
    --share-name $ShareName --mount-path /mnt/warewolf-config `
    --access-key (az storage account keys list -g $Rg -n $Stg --query [0].value -o tsv)

# Tell SecureConfigLoader where to look
az functionapp config appsettings set -n $AppName -g $Rg `
    --settings 'WAREWOLF_SECURE_CONFIG=/mnt/warewolf-config/secure.config'
```

Pros: independently update the policy file without redeploying code; share
the file across multiple Function Apps; fine-grained RBAC on the storage
account. Cons: the plan upgrade isn't free, mount latency adds ~1s to cold
start, and you must lock down the storage account properly.

---

## 4.5 Option C — Hot upload via Kudu (Consumption-friendly)

A pragmatic middle ground: bundle a placeholder in the package (so the
deploy pipeline stays simple), then push updates straight to
`D:\home\site\wwwroot\secure.config` over Kudu's VFS API and bounce the app.

```powershell
# Get publishing creds (one-time)
$profile = az functionapp deployment list-publishing-profiles `
    -n $AppName -g $Rg --query "[?publishMethod=='MSDeploy']|[0]" -o json |
    ConvertFrom-Json
$pair = "$($profile.userName):$($profile.userPWD)"
$auth = "Basic $([Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($pair)))"

# PUT the encrypted file to wwwroot
Invoke-RestMethod `
    -Uri  "https://$AppName.scm.azurewebsites.net/api/vfs/site/wwwroot/secure.config" `
    -Method PUT `
    -InFile .\secure.config `
    -Headers @{ Authorization=$auth; 'If-Match'='*' }

# Restart so SecureConfigLoader re-reads on the next request
az functionapp restart -n $AppName -g $Rg
```

Pros: no plan upgrade, edits are live within 30 s, doesn't pollute source
control. Cons: out-of-band from CI; harder to roll back without keeping the
prior file; relies on basic auth to SCM (disable when not in use).

---

## 4.6 Verification — proving the file loaded

Three independent signals — check at least two after every deploy.

### 4.6.1 Startup log

App Insights → `traces` filter:

```kusto
traces
| where timestamp > ago(10m)
| where message has_any (
      "WorkflowAuthPolicyLoader initialised",
      "secure.config not loaded")
```

* `WorkflowAuthPolicyLoader initialised with N workflow policies` where `N >= 1`
  → file loaded, decrypted, parsed.
* `secure.config not loaded — WorkflowAuthPolicyLoader has no policies`
  → file missing, malformed, or decryption failed. Engine is in open access.

### 4.6.2 Live request probe

```bash
curl -i 'https://wwexecution.azurewebsites.net/Secure/order.json?id=1'
```

* File loaded → `401` with the `WWW-Authenticate: Bearer` envelope.
* File NOT loaded → `200` (because the route's `AuthorizationLevel` is
  `Anonymous` and the middleware fell back to "open access"). If you see a
  200 here without auth, **stop deploying** and fix Stage 4 first.

### 4.6.3 apis.json behaviour

```bash
curl 'https://wwexecution.azurewebsites.net/Public/apis.json'
```

With a loaded `secure.config`, only workflows where the `Public` group has
View should appear. Without a loaded config, *every* workflow appears.

---

## 4.7 Failure mode catalogue

| Symptom | Root cause | Fix |
|---|---|---|
| `WorkflowAuthPolicyLoader initialised with 0 workflow policies` | `WAREWOLF_SECURE_CONFIG` points to a non-existent path, or bundled file missing from publish zip | Set the app setting; verify with Kudu `vfs` GET; confirm `<CopyToPublishDirectory>` is in csproj |
| Same as above, but file *is* on disk | Decryption failed silently — file was hand-edited or generated outside `SecurityEncryption.Encrypt` | Re-pack with the studio or the CLI utility (§4.2) |
| All `/Secure/*` calls succeed for unauthenticated callers | Engine fell into open-access mode | Same root causes as above |
| `/Secure/order.json` always returns `403 forbidden { "no policy" }` for everyone, including admins | The workflow has no row in `secure.config` (or all its rows have `IsServer == true`) | Add an `IsServer = false`, `ResourceName = "order"` row per group that should access it |
| Admin gets `403 Insufficient group membership` | Per-workflow row missing for `WarewolfAdministrators` (server-wide rows aren't ingested by `WorkflowAuthPolicyLoader`) | Add an admin row with `IsServer = false`, `ResourceName = "<workflow>"`, all flags true |
| Tokens validate against Easy Auth but `principal.Permissions` is empty | App role assignment never completed for that user | Re-run `Configure-WwExecutionAuth.ps1`; check `az rest GET /users/<oid>/appRoleAssignments` |
| Permissions update doesn't take effect | `Lazy<SecureConfigData>` cached the old version | `az functionapp restart -n $AppName -g $Rg` |
| Engine works locally but not in Azure | `AppContext.BaseDirectory` differs between in-proc and isolated workers | Pin via `WAREWOLF_SECURE_CONFIG` |

---

## 4.8 Operational checklist

Run this before declaring Stage 4 complete:

1. ☐ `secure.config` produced by Warewolf Studio or the CLI packer (§4.2).
2. ☐ Plain-text source JSON committed; encrypted binary excluded via `.gitignore` *or* fetched fresh from Key Vault at build time.
3. ☐ Either bundled (csproj `<CopyToPublishDirectory>` in place) or uploaded (§4.4 / §4.5).
4. ☐ `WAREWOLF_SECURE_CONFIG` app setting points to the chosen location.
5. ☐ Function app restarted after upload.
6. ☐ App Insights confirms `WorkflowAuthPolicyLoader initialised with N>=1 workflow policies`.
7. ☐ `/Secure/order.json` returns `401` without a token; `200` with an admin token; `403` with a `PUBLIC`-only token.
8. ☐ `/Public/apis.json` returns only `Public`-viewable workflows.
9. ☐ Rotation playbook documented (§4.9) and tested in non-prod.

---

## 4.9 Rotation, rollback, and audit

* **Rotate the HMAC secret** — set `SecretKey = ""` in the source JSON, re-pack,
  redeploy. `SecureConfigLoader` will detect the empty string and synthesise a
  fresh key in memory. (For a stable secret across replicas, generate one
  with `[Convert]::ToBase64String((New-Object byte[] 32 ; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($_)))`
  and place it in the file.)
* **Roll back** — keep the previous `secure.config` in the build artefact
  store. Revert by re-deploying that artefact, or by `PUT`ing it back via
  Kudu (§4.5).
* **Audit** — every authorise / deny decision is logged with `CallerIdentity`,
  workflow name, and required permissions in `WorkflowAuthorizationMiddleware`.
  Enable a daily KQL summary on App Insights:

```kusto
traces
| where timestamp > ago(24h)
| where message has_any ("Authorised", "Group check failed", "Permission check failed")
| extend kind = case(message has "Authorised", "ALLOW",
                     message has "Group check", "DENY-GROUP",
                     message has "Permission check", "DENY-PERM",
                     "OTHER")
| summarize count() by bin(timestamp, 1h), kind
```

A spike in `DENY-GROUP` after a rotation means the new file mis-mapped a
group; roll back immediately.

---

## 4.10 TL;DR for the Free Consumption case

You are on Y1. Your only two real choices are:

1. **Bundle in the package (§4.3)** — easiest, atomic, requires a redeploy
   to change. Use this for nine cases in ten.
2. **Hot-upload via Kudu (§4.5)** — when permission edits need to land
   without a code redeploy. Pair with the same packer and a versioned
   storage location for rollback.

Either way, the verification in §4.6 is non-negotiable. Open-access mode
silently masquerading as a working deploy is the single biggest foot-gun
in this stack.
