# KB: Getting SQL Server, PostgreSQL & Redis sources working on the Lightweight Azure engine

**Applies to:** `Warewolf.Execution.Lightweight` Azure Function (`wwenginetestv1`, RG `dev2`, https://wwenginetestv1.azurewebsites.net)
**Related:** [README-Encryption.md](README-Encryption.md), [KeyRotationRunbook.md](KeyRotationRunbook.md)
**Status:** SQL Server, PostgreSQL, and Redis are all running on the engine (workflows return `{}` / HTTP 200). PostgreSQL and Redis needed only the encrypt‑and‑deploy procedure (Steps 1‑4); SQL Server additionally required a TLS fix (Section 4).

---

## 1. The core problem

On the full Warewolf **Server**, source `.bite` files protect their `ConnectionString` with **Windows DPAPI** (machine‑scoped). DPAPI **cannot be decrypted on Azure** (no machine key), so a DPAPI‑encrypted source deployed to the lightweight engine yields **empty connection fields at runtime**. Each source type then fails in its own way:

| Source | Symptom when the source is still DPAPI (not decryptable on Azure) |
|--------|------------------------------------------------------------------|
| SQL Server | `SQL Error: Index was out of range … ADP.IsEndpoint … set_ConnectionString` (empty **Data Source**) |
| PostgreSQL | `Host can't be null` (Npgsql, empty **Host**) |
| Redis | Connection fails / empty **HostName** |

The lightweight engine instead uses **AES‑256‑GCM** (`WFAES::…`) backed by an **Azure Key Vault** secret, decrypted in‑memory at runtime via `DpapiWrapper.AesDecryptHook` (wired at cold start before resources load). Every source type that stores a `ConnectionString` (`DbSource` for SQL/Postgres/MySQL/Oracle, `RedisSource`, `EmailSource`, `RabbitMQSource`, …) goes through the same hook, so the procedure below is identical for all of them.

---

## 2. Key facts for this engine

| Item | Value |
|------|-------|
| Key Vault | `WWExecutionEngine` |
| Secret | `WWExecutionEngineTestSecret` |
| App settings on the Function App | `AZURE_KEYVAULT_NAME=WWExecutionEngine`, `KEYVAULT_SECRET_NAME=WWExecutionEngineTestSecret` |
| Encrypt script | `Dev/Warewolf.Execution.Lightweight/Scripts/Encrypt-Config.ps1` |
| Prereqs | PowerShell 7+, Azure CLI logged in (`az login`) |

> The other vault `WarewolfServerExecution` is **ForbiddenByRbac** for the current user — do not use it.

---

## 3. General procedure (all source types)

### Step 1 — Encrypt the source to WFAES

Every time a source is saved in Studio it is rewritten as **DPAPI**, so always re‑encrypt and deploy the **WFAES output** — never the Studio‑saved file.

`C:\ProgramData\Warewolf\Resources\` grants normal users only Read+Execute, and the running Warewolf Server rewrites files it holds. So **encrypt a copy** in a writable folder rather than in place:

```powershell
# copy first, then encrypt the copy
Copy-Item "C:\ProgramData\Warewolf\Resources\<name>.bite" "C:\temp\enc\<name>.bite"

cd D:\Warewolf\Warewolf_Net6\Dev\Warewolf.Execution.Lightweight\Scripts
.\Encrypt-Config.ps1 `
    -FilePath   "C:\temp\enc\<name>.bite" `
    -VaultName  "WWExecutionEngine" `
    -SecretName "WWExecutionEngineTestSecret"
# answer "n" to the backup prompt (you are working on a copy)
```

Confirm the result — the `ConnectionString` should now start with `WFAES::`.

### Step 2 — Deploy to Azure (Kudu VFS)

Basic‑auth publishing is disabled (returns 401); use an **AAD bearer token**:

```bash
TOKEN=$(az account get-access-token --query accessToken -o tsv)
curl -X PUT \
  -H "Authorization: Bearer $TOKEN" -H "If-Match: *" \
  --data-binary @"C:/temp/enc/<name>.bite" \
  "https://wwenginetestv1.scm.azurewebsites.net/api/vfs/site/wwwroot/Resources/<name>.bite"
# expect HTTP 204 (overwrite) or 201 (new file)
```

### Step 3 — Reload with a FULL stop → start (NOT `restart`)

```bash
az functionapp stop  --name wwenginetestv1 --resource-group dev2
az functionapp start --name wwenginetestv1 --resource-group dev2
```

> **Why not `az functionapp restart`?** On the Consumption plan, `restart` can reuse a **warm worker** that still holds the previously‑loaded (DPAPI, empty‑field) source in memory, so the new WFAES source silently doesn't take effect. This was seen as a persistent `Host can't be null` for Postgres even after deploying the correct file. A full `stop` then `start` forces a clean cold start that reloads and decrypts the source.

### Step 4 — Verify

```bash
curl -s "https://wwenginetestv1.azurewebsites.net/Public/<workflow>.json"
# success = HTTP 200 with {} (or your mapped output) and no "hasErrors": true
```

---

## 4. Source‑specific notes

### PostgreSQL — works with Steps 1‑4 only
- Decrypted connection string looks like:
  `Host=<host>:<port>;Username=…;Password=…;Database=…;Timeout=30`
- **Port after a colon** (`host:port`) is a Postgres convention. `DbSource` only splits the host on a **comma** to extract the port, so `Server` keeps the whole `host:port` and `Port=0`. Npgsql 9.x tolerates this, so no action is needed. (SqlClient would **not** tolerate a colon — SQL sources use `host,port`.)
- No TLS/cert step was required.

### Redis — works with Steps 1‑4 only  ✅ confirmed running
- `RedisSource` decrypts through the same hook and parses `HostName;Port;AuthenticationType;Password`. The stored value is escaped/unescaped internally, but `Encrypt-Config.ps1` simply swaps DPAPI→WFAES on the same value, so no special handling is needed.
- No SQL‑style cert‑trust setting exists on the Redis source.
- Deploy the source (`redissource.bite`) **and** its workflow (`redissourceworkflow.bite`), then full stop→start.

### SQL Server — needs Steps 1‑4 **plus a code/config TLS fix**

After the WFAES source is deployed, SQL Server fails at the TLS handshake:

> `A connection was successfully established with the server, but then an error occurred during the login process. (provider: SSL Provider, error: 0 - The certificate chain was issued by an authority that is not trusted.)`

**Cause:** the lightweight engine uses `Microsoft.Data.SqlClient`, which defaults to `Encrypt=true` with full certificate‑chain validation and rejects self‑signed / tunnelled (e.g. ngrok) certs. The old full server used `System.Data.SqlClient` (`Encrypt=false` by default), which is why it worked there.

**This cannot be fixed in the `.bite` file.** `DbSource.ConnectionString` rebuilds the connection string from parsed parts and drops any `TrustServerCertificate` / `Encrypt` key. The fix is in the single connection choke point, `Dev2.Services.Sql/SqlConnectionWrapper.CreateConnectionString`, gated behind an env var so production keeps strict validation by default:

```csharp
// inside CreateConnectionString, after building conStrBuilder
if (bool.TryParse(Environment.GetEnvironmentVariable("WAREWOLF_SQL_TRUST_SERVER_CERT"), out var trust) && trust)
{
    conStrBuilder.TrustServerCertificate = true;
}
```

Then set the flag on the Function App:

```bash
az functionapp config appsettings set --name wwenginetestv1 --resource-group dev2 \
  --settings WAREWOLF_SQL_TRUST_SERVER_CERT=true
```

The code change must ship in the **`Dev2.Services.Sql.dll`** the engine loads — see the deployment warning below.

### SQL Server — Azure SQL with "Active Directory Password" auth (no TLS fix needed)

Used for `Resources/rabbit/RabbitProcess.bite` / `RabbitProcess2.bite`'s `NewSqlServerSource.bite` against
`WarewolfServer-UAT`. Distinct from the self-signed/ngrok scenario above — Azure SQL's own certificate chain
is trusted by default, so **no `WAREWOLF_SQL_TRUST_SERVER_CERT` flag is required here**.

- The source's `Authentication` value in the (pre-encryption) connection string must be
  **`Active Directory Password`**, not `Active Directory Managed Identity` — the latter only works when the
  Function App's own managed identity has been granted a SQL login, which is a separate, unconfigured path
  for this engine. A downloaded/exported source that reads "Managed Identity" but ships a real password is
  mislabeled and must be corrected to `Active Directory Password` before encrypting, or the engine will
  attempt (and fail) an MI token acquisition instead of using the supplied credential.
- Steps 1–4 (WFAES-encrypt, Kudu-deploy, full stop→start, verify) apply as normal — see §3 above. On
  `WarewolfServer-UAT` specifically, `WEBSITE_RUN_FROM_PACKAGE=1` means Kudu VFS single-file PUT is
  **not available**; the encrypted source must instead be staged into a full `Deploy-WwExecutionEngine.ps1`
  redeploy's `-WorkflowsSourcePath` folder (see `Scripts/README.md` and the deploy skill's phased-flow
  description).
- Decryption at runtime requires `AZURE_KEYVAULT_NAME` + `KEYVAULT_SECRET_NAME` app settings and the Function
  App's managed identity holding `Key Vault Secrets User` on that vault — provision both once per engine
  (`Encrypt-Config.ps1` only writes the `WFAES::…` payload; it does not wire up the runtime decrypt path).

---

## 5. ⚠️ Deploying an engine DLL (only needed for the SQL code fix)

**Do not hot‑swap an engine DLL with a plain local build.** The deployed engine is a CI `-AutoVersion` build whose assemblies are stamped e.g. `3.0.2.85`. A normal local `dotnet build` produces `0.0.0.0` (repo has `GenerateAssemblyInfo=false`, `EnableDefaultItems=false`, and version comes from `Dev/AssemblyCommonInfo.cs` which `Compile.ps1` rewrites). Deploying a `0.0.0.0` DLL over the deployed one **breaks assembly binding for all DB execution**:

> `Could not load file or assembly 'Dev2.Services.Sql, Version=3.0.2.85 …'. The system cannot find the file specified.`

If you must hot‑patch the DLL (interim fix):

```bash
# 1. Stamp the version to match the deployed build, in Dev/AssemblyCommonInfo.cs:
#    [assembly: AssemblyVersion("3.0.2.85")]
#    [assembly: AssemblyFileVersion("3.0.2.85")]
# 2. Rebuild (force, not incremental) and verify the stamp:
dotnet build Dev/Dev2.Services.Sql/Dev2.Services.Sql.csproj -c Release -t:Rebuild
#    [Reflection.AssemblyName]::GetAssemblyName(dll).Version  -> 3.0.2.85
# 3. The DLL is locked while the app runs (PUT returns 409) — STOP the app first:
az functionapp stop --name wwenginetestv1 --resource-group dev2
TOKEN=$(az account get-access-token --query accessToken -o tsv)
curl -X PUT -H "Authorization: Bearer $TOKEN" -H "If-Match: *" \
  --data-binary @"Dev/Dev2.Services.Sql/bin/Release/net8.0/Dev2.Services.Sql.dll" \
  "https://wwenginetestv1.scm.azurewebsites.net/api/vfs/site/wwwroot/Dev2.Services.Sql.dll"
az functionapp start --name wwenginetestv1 --resource-group dev2
# 4. Revert Dev/AssemblyCommonInfo.cs back to 0.0.0.0 (it is CI-managed).
```

> **Durable path:** commit the `SqlConnectionWrapper.cs` change and ship it via a proper `Compile.ps1` CI build + full engine redeploy. A future full redeploy from CI overwrites any hot‑swapped DLL, but the fix survives if it is committed; the `WAREWOLF_SQL_TRUST_SERVER_CERT` app setting persists across redeploys.

---

## 6. Troubleshooting quick reference

| Error (workflow response) | Root cause | Fix |
|---|---|---|
| `Index was out of range … ADP.IsEndpoint` (SQL) | DPAPI source → empty Data Source | Deploy WFAES source (Steps 1‑3) |
| `Host can't be null` (Postgres) | DPAPI source **or** warm‑worker cache holding old source | Deploy WFAES source; **full stop→start** (not restart) |
| `certificate chain … not trusted` (SQL) | `Microsoft.Data.SqlClient` `Encrypt=true` vs untrusted cert | `SqlConnectionWrapper` fix + `WAREWOLF_SQL_TRUST_SERVER_CERT=true` |
| `Could not load file or assembly 'Dev2.Services.Sql, Version=3.0.2.85'` | Hot‑swapped a `0.0.0.0` local DLL | Rebuild stamped `3.0.2.85`, redeploy (Section 5) |
| `FROM address is not in the valid format:` (Email) | DPAPI EmailSource → empty UserName (From falls back to it) | Deploy WFAES EmailSource |
| Secret `dp-keyring-v1` not found / Forbidden | Wrong vault/secret | Use `WWExecutionEngine` / `WWExecutionEngineTestSecret` |

---

## 7. What was done in this exercise (summary)

1. **PostgreSQL** — encrypted `postgresource.bite` to WFAES, deployed via Kudu, **full stop→start** → workflow returns `{}` (200). No code change.
2. **SQL Server** — encrypted `sqlsource.bite` to WFAES **and** applied the env‑gated `TrustServerCertificate` fix in `SqlConnectionWrapper.cs`, rebuilt `Dev2.Services.Sql.dll` stamped `3.0.2.85`, deployed the DLL + WFAES source, set `WAREWOLF_SQL_TRUST_SERVER_CERT=true`, **full stop→start** → workflow returns `{}` (200).
3. **Redis** — encrypted `redissource.bite` to WFAES, deployed the source + its workflow, **full stop→start** → workflow runs fine. No code change and no TLS step.
