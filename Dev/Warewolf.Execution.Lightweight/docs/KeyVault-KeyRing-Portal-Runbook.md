# Setting the engine's Key Vault key-ring secret

**Option A — generate the value in Azure Cloud Shell, set it in the Azure Portal**

> This document contains no key material. Never add real secret values to it.

| | |
|---|---|
| **Applies to** | Warewolf Execution Engine (Lightweight, Azure Functions) — `KeyVaultSecretManager` reads the secret at start-up |
| **Secret** | `devprofilerlwkv`, in the vault named by the engine's `KeyVaultName` setting |
| **For** | Operators who create or rotate the encryption key for Warewolf resources (encrypted .bite sources) |
| **Verified** | 30 Oct 2026. Every script in this document was run in PowerShell 7 and its output was loaded by the engine's own `KeyVaultSecretManager`, `FileEncryptionHelper` and `FileDecryptionHelper` classes. |

---

## 1. Which scenario do you need?

| | Scenario 1 — First key | Scenario 2 — Rotate the key |
|---|---|---|
| **Use when** | The secret does not exist yet (new environment). | The secret exists and you want a new key, while data encrypted with the current key keeps working. |
| **Result** | `version` 1, a new primary key, `previousKeys` empty. | `version` 2, a new primary key; the current key moves into `previousKeys`. |
| **Portal action** | Secrets › **+ Generate/Import** (creates the secret) | Secrets › your secret › **+ New Version** (same secret name) |
| **Script** | `first-key.ps1` | `rotate-key.ps1` |
| **Go to** | [Section 4](#4-scenario-1--first-key-previouskeys-empty) | [Section 5](#5-scenario-2--rotate-the-key-current-key-moves-to-previouskeys) |

### How the engine uses the secret

- At start-up the engine reads the **latest version** of the secret once, using its managed identity. A new secret version takes effect only after the engine restarts.
- If the secret cannot be read or parsed, the engine **refuses to start**. Check the value before you restart (steps below).
- Encryption always uses the primary `key`. Decryption tries the primary key first, then each entry in `previousKeys`, most recently retired first. That is what lets old data keep working after a rotation.

---

## 2. Secret value format

The value is a single line of JSON. The scripts in this document produce it for you; this section is the reference.

| Field | Required | Rule |
|---|---|---|
| Whole value | Yes | Compact JSON on **one line** that starts exactly with `{"`. No line breaks and no space after the opening brace. Valid JSON that is pretty-printed or starts with `{ "` is **rejected** at start-up. |
| `key` | Yes | Base64 of exactly **32 random bytes** (AES-256): 44 characters ending in `=`. Anything else stops the engine. |
| `keyId` | Recommended | Unique label for the key (the scripts use a GUID). Shown in logs; never the key itself. |
| `previousKeys` | No | List of retired keys, each `{"keyId","key","retired"}`. Empty or missing: primary key only. A broken entry is skipped with a warning; the engine still starts. |
| `retired` | In each entry | UTC date and time the key was retired (ISO 8601). Sets the order keys are tried in. |
| `version`, `created` | No | Informational. Not checked by the engine. |

Shapes produced by the scripts (wrapped here for reading — the real value is one line):

```text
Scenario 1 (first key)
{"version":1,"keyId":"<guid-A>","key":"<44-char Base64>","created":"<UTC time>","previousKeys":[]}

Scenario 2 (after the first rotation)
{"version":2,"keyId":"<guid-B>","key":"<new 44-char Base64>","created":"<UTC time>",
 "previousKeys":[{"keyId":"<guid-A>","key":"<old 44-char Base64>","retired":"<UTC time>"}]}
```

### How the key ring grows

| After Scenario 1 | After 1st rotation | After 2nd rotation | After pruning (Section 6) |
|---|---|---|---|
| **Primary** key A | **Primary** key B | **Primary** key C | **Primary** key C |
| | previous key A | previous key B (newest) | |
| | | previous key A | |

*Data encrypted under any key in the ring can be decrypted. Only remove a key once nothing is encrypted with it any more.*

---

## 3. Before you start

| Check | Details |
|---|---|
| Your access | **Key Vault Secrets Officer** on the vault (create versions and read values), or an access policy with Get, List and Set. |
| Engine access | The Function App's managed identity has **Key Vault Secrets User** on the vault. |
| Network | If the vault firewall is on, add your client IP under the vault's **Networking** page, otherwise the portal cannot list or show secrets. |
| Names | Vault name (`KeyVaultName`) and secret name `devprofilerlwkv`. The engine's `KeyVaultSecretName` setting must be set to `devprofilerlwkv`; the engine does not use this name by default. |
| Cloud Shell | Portal top bar › **Cloud Shell** icon (`>_`) › **PowerShell**. If asked about storage, choose **No storage account required** (ephemeral) so nothing from the session is kept. |

> [!CAUTION]
> **Do not share key material.** Never paste a secret value into chat, tickets, email, wiki pages or source control. Copy it only between Cloud Shell and the portal. Every script starts by turning off Cloud Shell command history so a pasted value is not saved.

---

## 4. Scenario 1 — First key (previousKeys empty)

**Cloud Shell: `first-key.ps1`** → **Copy the JSON line** → **Portal: Generate/Import** → **Show Secret Value to check**

### Step 1 — Open Cloud Shell

In the Azure Portal, select the **Cloud Shell** icon (`>_`) in the top bar and choose **PowerShell**.

### Step 2 — Generate the value

Paste the whole script (`first-key.ps1`) and press Enter.

```powershell
# Do not save this session's commands (or anything pasted) to Cloud Shell history
Set-PSReadLineOption -HistorySaveStyle SaveNothing

$now   = (Get-Date).ToUniversalTime().ToString('o')
$bytes = [Security.Cryptography.RandomNumberGenerator]::GetBytes(32)
$ring  = [ordered]@{
    version      = 1
    keyId        = [guid]::NewGuid().Guid
    key          = [Convert]::ToBase64String($bytes)
    created      = $now
    previousKeys = @()
}
[Array]::Clear($bytes)
$json = $ring | ConvertTo-Json -Compress -Depth 5

# Validate exactly as the engine does: valid JSON, key = Base64 of 32 bytes
$c = $json | ConvertFrom-Json
if ([Convert]::FromBase64String($c.key).Length -ne 32) { throw 'Primary key is not 32 bytes' }
"Primary: $($c.keyId)   previousKeys: $(@($c.previousKeys).Count)"
$json
```

### Step 3 — Copy the JSON line

The script prints two lines. The first is a summary for you to read; the second is the secret value:

```text
Primary: 3f0c...-....   previousKeys: 0
{"version":1,"keyId":"3f0c...","key":"...","created":"...","previousKeys":[]}
```

Select the **whole second line** and copy it. Do not include the summary line, and do not reformat the JSON.

### Step 4 — Create the secret in the portal

Go to **Key vaults** › your vault › **Objects** › **Secrets** › **+ Generate/Import** and fill in:

| Field | Value |
|---|---|
| Upload options | Manual |
| Name | `devprofilerlwkv` |
| Secret value | Paste the JSON line from Step 3 |
| Content type (optional) | `application/json` |
| Set activation date / Set expiration date | Leave unticked. An expired secret stops the engine from starting. |
| Enabled | Yes |
| Tags | Optional. Never put key material in a tag. |

Select **Create**.

### Step 5 — Check what was stored

Open the secret › select the **Current version** › **Show Secret Value**. It must be the same single line and start with `{"version":1,"keyId":`. If it differs, add a new version with the correct line.

---

## 5. Scenario 2 — Rotate the key (current key moves to previousKeys)

**Portal: copy the current value** → **Cloud Shell: `rotate-key.ps1`** → **Copy the JSON line** → **Portal: New Version** → **Check, then restart engine** → **Check the logs**

### Step 1 — Copy the current secret value

Go to **Key vaults** › your vault › **Secrets** › `devprofilerlwkv` › **Current version** › **Show Secret Value**, then use the copy icon next to the value.

### Step 2 — Generate the new value in Cloud Shell

Open Cloud Shell (PowerShell). First run these lines, pasting the copied value in place of `<PASTE-CURRENT-SECRET-VALUE>` and keeping the single quotes around it:

```powershell
# Do not save this session's commands (or anything pasted) to Cloud Shell history
Set-PSReadLineOption -HistorySaveStyle SaveNothing

# Paste the CURRENT secret value between the quotes (Portal > Show Secret Value > copy)
$current = '<PASTE-CURRENT-SECRET-VALUE>'
```

Then paste and run the rest of the script (`rotate-key.ps1`):

```powershell
$old = $current | ConvertFrom-Json          # stops here if the value is not valid JSON
$now = (Get-Date).ToUniversalTime().ToString('o')

# The current primary becomes the newest previous key; existing previous keys are kept
$prev = @([ordered]@{ keyId = $old.keyId; key = $old.key; retired = $now })
foreach ($p in @($old.previousKeys)) {
    if (-not $p) { continue }
    $retired = $p.retired
    if ($retired -is [datetime]) { $retired = $retired.ToUniversalTime().ToString('o') }
    $prev += [ordered]@{ keyId = $p.keyId; key = $p.key; retired = "$retired" }
}

$bytes = [Security.Cryptography.RandomNumberGenerator]::GetBytes(32)
$ring  = [ordered]@{
    version      = 2
    keyId        = [guid]::NewGuid().Guid
    key          = [Convert]::ToBase64String($bytes)
    created      = $now
    previousKeys = $prev
}
[Array]::Clear($bytes)
$json = $ring | ConvertTo-Json -Compress -Depth 5

# Validate: every key = Base64 of 32 bytes, no duplicate keyIds
$c   = $json | ConvertFrom-Json
$all = @($c) + @($c.previousKeys)
foreach ($k in $all) {
    if ([Convert]::FromBase64String($k.key).Length -ne 32) { throw "Key $($k.keyId) is not 32 bytes" }
}
if (@($all.keyId | Sort-Object -Unique).Count -ne $all.Count) { throw 'Duplicate keyId in ring' }
"Primary: $($c.keyId)   previousKeys: $(@($c.previousKeys.keyId) -join ', ')"
$json
```

### Step 3 — Check the summary and copy the JSON line

```text
Primary: 7b21...-....   previousKeys: 3f0c...-...., <older keyIds, if any>
{"version":2,"keyId":"7b21...","key":"...","created":"...","previousKeys":[{"keyId":"3f0c...", ...}]}
```

The **first** keyId after `previousKeys:` must be the keyId of the value you copied in Step 1. Then copy the whole second line.

### Step 4 — Add a new version of the same secret

Open `devprofilerlwkv` › **+ New Version** and fill in:

| Field | Value |
|---|---|
| Secret value | Paste the JSON line from Step 3 |
| Content type (optional) | `application/json` |
| Set activation date / Set expiration date | Leave unticked |
| Enabled | Yes |

Select **Create**. Keep the same secret name: the engine always reads the latest version. Do not delete or disable older versions; they are your rollback ([Section 7](#7-rollback)).

### Step 5 — Check what was stored, then restart

**Show Secret Value** on the new current version: it must start with `{"version":2,"keyId":` and contain `"previousKeys":[{"keyId":"<old keyId>"`. Then restart the Function App (**Overview** › **Restart**).

### Step 6 — Confirm in the logs

In the Function App's Application Insights › **Logs**, run:

```kusto
traces
| where timestamp > ago(30m)
| where message has "KeyVault |"
| project timestamp, severityLevel, message
| order by timestamp desc
```

Expected message:

```text
KeyVault | Key ring loaded — 2 key(s) available. Primary=<new keyId>,
           previous (newest retired first)=[<old keyId>], skipped=0.
```

`skipped` must be 0. Close Cloud Shell when you are done.

### What happens next

- New data is encrypted with the new primary key.
- Data encrypted with an older key still decrypts. Each time that happens the engine logs a warning: `Decryption | Succeeded using fallback key '<old keyId>'`. That is expected until you re-encrypt ([Section 6](#6-after-a-rotation-re-encrypt-then-remove-old-keys)).
- You can rotate again later with the same steps; the ring keeps every older key until you prune it.

---

## 6. After a rotation: re-encrypt, then remove old keys

Old keys should not stay in the ring for ever. Once every resource is encrypted with the new primary key, remove them.

### Step 1 — Re-encrypt resources with the new key

From your original (plain-text or DPAPI) source files, run `Encrypt-Config.ps1` in **normal mode** with the new value — not `-GenerateKeys`. With `-KVSecret`, keep the value in a variable rather than typing it on the command line:

```powershell
$env:WW_KEYRING = '<current secret value>'   # from Show Secret Value
.\Encrypt-Config.ps1 -FilePath C:\Warewolf\Resources -KVSecret $env:WW_KEYRING -NonInteractive -NoBackup
Remove-Item Env:WW_KEYRING
```

Without the original files, decrypt first with `-Decrypt` using the **previous** secret version's value (`-KVSecret` uses only the primary key of the value you pass), then encrypt the decrypted copy with the new value. Delete the plain-text copy afterwards. Deploy the re-encrypted resources.

### Step 2 — Wait for the fallback warnings to stop

Run the resources' workflows and check that no `Succeeded using fallback key` warnings appear any more.

### Step 3 — Remove the retired keys

Copy the current value (Show Secret Value), then in Cloud Shell run the first lines of `prune-keys.ps1` with the value pasted in:

```powershell
# Do not save this session's commands (or anything pasted) to Cloud Shell history
Set-PSReadLineOption -HistorySaveStyle SaveNothing

# Paste the CURRENT secret value between the quotes (Portal > Show Secret Value > copy)
$current = '<PASTE-CURRENT-SECRET-VALUE>'
```

Then paste and run the rest:

```powershell
# Keep the primary key exactly as it is; drop every retired key
$c       = $current | ConvertFrom-Json
$created = $c.created
if ($created -is [datetime]) { $created = $created.ToUniversalTime().ToString('o') }
$ring = [ordered]@{
    version      = $c.version
    keyId        = $c.keyId
    key          = $c.key
    created      = "$created"
    previousKeys = @()
}
$json = $ring | ConvertTo-Json -Compress -Depth 5

$check = $json | ConvertFrom-Json
if ([Convert]::FromBase64String($check.key).Length -ne 32) { throw 'Primary key is not 32 bytes' }
"Primary kept: $($c.keyId)   removed: $(@($c.previousKeys.keyId) -join ', ')"
$json
```

Check that the summary line's **Primary kept** is the current keyId. Then add the JSON line as a **+ New Version** ([Section 5](#5-scenario-2--rotate-the-key-current-key-moves-to-previouskeys), Steps 4 and 5) and restart the engine. The log should now show *primary key only*.

---

## 7. Rollback

If the engine does not start, or data stops decrypting, after you add a version:

- Open the secret, select the **previous version** › **Show Secret Value** › copy.
- Select **+ New Version**, paste that value, **Create**, then restart the engine.

> [!IMPORTANT]
> **Disabling the new version does not roll back.** The engine always asks for the latest version. Key Vault refuses to return a disabled version, so the engine still cannot start. Add the old value as a new version instead.

---

## 8. Troubleshooting

Start-up failures are logged by the engine as `KeyVaultStartupExtensions InitializeKeyVaultAsync failed ... ExceptionType=<type>`. The exception type tells you which row applies.

| What you see | Likely cause | Fix |
|---|---|---|
| Start-up fails, `RequestFailedException` | Key Vault refused the read: missing role, vault firewall, wrong vault or secret name, or the latest version is disabled or expired. | Check the engine identity has Key Vault Secrets User, the vault Networking page, the names, and that the latest version is enabled with no expiry. |
| Start-up fails, `AuthenticationFailedException` or `CredentialUnavailableException` | The Function App's managed identity is off or misconfigured. | Turn on the identity (Function App › Identity) and re-check the role. |
| Start-up fails, `InvalidOperationException` mentioning *legacy unquoted-JSON* | The value is not exactly the compact line: extra spaces, line breaks or lost quotes. | Add a new version with the script's output line, unchanged. |
| Start-up fails, `JsonException` | The value is not JSON (truncated paste, or a bare key or GUID). | Add a new version with the script's output line. |
| Start-up fails, `FormatException` or *AES-256 key must be 32 bytes* | The primary `key` is not Base64 of 32 bytes. | Generate the value with the scripts; do not hand-edit keys. |
| Warning: *Previous key '...' has invalid ... — skipped* | A `previousKeys` entry is broken. The engine runs, but data under that key cannot be decrypted. | Fix the entry: copy the key from the older secret version where it was primary. |
| Workflow error: *AES-GCM decryption failed for all N key(s)* | The key that encrypted this resource is not in the ring (pruned too early, or `-GenerateKeys` was used). | Put the key back into `previousKeys` from an older secret version, or re-encrypt the resource. |
| Warning: *Succeeded using fallback key* | Expected after a rotation. | Re-encrypt ([Section 6](#6-after-a-rotation-re-encrypt-then-remove-old-keys)). |
| Script stops at `ConvertFrom-Json` | The pasted current value is incomplete or not valid JSON. | Copy it again with the copy icon next to Show Secret Value, keeping the single quotes around it. |

---

## Appendix — Scripts

The scripts are also kept as files next to this document. Paste their contents into Cloud Shell; replace `<PASTE-CURRENT-SECRET-VALUE>` before running `rotate-key.ps1` or `prune-keys.ps1`.

| Script | Purpose | Output | Where |
|---|---|---|---|
| [first-key.ps1](first-key.ps1) | Scenario 1: new key ring with one key | `version` 1, `previousKeys` empty | [Section 4](#4-scenario-1--first-key-previouskeys-empty), Step 2 |
| [rotate-key.ps1](rotate-key.ps1) | Scenario 2: new primary key; current primary and existing previous keys kept | `version` 2, current key first in `previousKeys` | [Section 5](#5-scenario-2--rotate-the-key-current-key-moves-to-previouskeys), Step 2 |
| [prune-keys.ps1](prune-keys.ps1) | Section 6: keep the primary key, remove all retired keys | same primary, `previousKeys` empty | [Section 6](#6-after-a-rotation-re-encrypt-then-remove-old-keys), Step 3 |

**How this was verified.** Each script was run in PowerShell 7.6 and its output was loaded by the engine's own classes: first key; two rotations in a row (three-key ring, correct order); rotation from a version 1 secret without previousKeys; pruning. In each case data encrypted with the new key and with every retired key decrypted, and data encrypted with an unrelated key was rejected. The same value pretty-printed, or with a space after the opening brace, was confirmed to be rejected at start-up.
