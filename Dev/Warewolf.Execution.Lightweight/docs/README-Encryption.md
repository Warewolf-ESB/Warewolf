# Warewolf Lightweight — Connection String Encryption

Warewolf source files (`.bite`) may contain database, web, and other
connection strings.  On the full server these are protected with Windows DPAPI
(machine-scoped).  DPAPI is unavailable in Azure, so the lightweight Azure
Function uses **AES-256-GCM** backed by an **Azure Key Vault** secret.

---

## Architecture

```
Developer Machine (Windows)
  └─ Encrypt-Config.ps1
       ├─ Reads .bite files → checks for <Source ConnectionString="...">
       ├─ Detects DPAPI-encrypted values → decrypts (local, current machine)
       ├─ FIRST RUN : generates AES-256 key → stores as KV secret "dp-keyring-v1"
       │  LATER RUNS: retrieves existing key from KV (1 GET op)
       ├─ Encrypts ConnectionString values → WFAES::{Base64(...)}
       └─ (Optional) zip-deploys updated Resources folder via az CLI

Azure Function (.NET 8 — Consumption / Free Tier)
  └─ Cold start (once per instance)
       ├─ KeyVaultSecretManager → DefaultAzureCredential → 1 KV GET
       ├─ Deserialise key bytes → held in memory for instance lifetime
       ├─ Register DpapiWrapper.AesDecryptHook = FileDecryptionHelper.DecryptConnectionString
       └─ WorkflowIndex.WarmUp()

  └─ Every invocation (zero Key Vault ops)
       └─ DbSource(XElement) → DpapiWrapper.CanBeDecrypted/Decrypt
            → hook detects WFAES:: → FileDecryptionHelper.DecryptConnectionString
            → AES-256-GCM in-memory → plain connection string
```

### Encrypted attribute format

```
WFAES::{Base64( [12-byte nonce][ciphertext][16-byte GCM tag] )}
```

- `WFAES::` is not valid base64 (contains `:`), so `DpapiWrapper.CanBeDecrypted`
  returns `false` without the hook — safe on any machine lacking the key.
- AES-256-GCM provides **authenticated encryption**: any bit-flip in the
  ciphertext or tag causes `CryptographicException` at decrypt time.

---

## Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| PowerShell | 7+ | Encrypt-Config.ps1 |
| az CLI | latest | Key Vault + deployment |
| .NET SDK | 8.0 | Build + publish |
| Azure subscription | — | Key Vault + Function App |

---

## One-Time Infrastructure Setup

```bash
# Edit variables at the top of the file first.
bash Scripts/KeyVaultSetup.azcli
```

This creates the Key Vault, enables Managed Identity on the Function App,
and assigns the minimum required RBAC roles.

---

## Encrypting Source Files (Developer Machine)

```powershell
# First run — generates the key and encrypts.
# -SecretName is mandatory and must always be provided.
.\Scripts\Encrypt-Config.ps1 `
    -FilePath   "C:\Warewolf\Resources" `
    -VaultName  "kv-warewolf-prod" `
    -SecretName "dp-keyring-v1" `
    -GenerateKeys

# Subsequent runs — re-encrypt changed files with the existing key.
# Omit -GenerateKeys; -SecretName is still required.
.\Scripts\Encrypt-Config.ps1 `
    -FilePath   "C:\Warewolf\Resources" `
    -VaultName  "kv-warewolf-prod" `
    -SecretName "dp-keyring-v1"
```

The script:
1. Validates `.bite` files contain `<Source ConnectionString="...">` — others are silently skipped.
2. Decrypts any DPAPI-encrypted values in-place first.
3. Writes `WFAES::...` values back to the file.
4. Creates `*.bite.bak` backups of originals.

> **Delete `.bite.bak` files after verifying the deployment** — they may
> contain DPAPI-encrypted values from the original files.

`Encrypt-Config.ps1` does **not** deploy anything. Once the files are
encrypted, deploy the updated `Resources` folder with your normal CI/CD
pipeline or a manual zip-deploy:

```bash
az functionapp deploy \
  --resource-group rg-warewolf-prod \
  --name           func-warewolf-prod \
  --src-path       warewolf-func.zip \
  --type           zip
```

---

## Local Development

Set `AZURE_KEYVAULT_NAME` to enable encryption in local dev (uses Azure CLI
credentials via `DefaultAzureCredential`).

Leave `AZURE_KEYVAULT_NAME` **unset** to skip Key Vault entirely and use
plain or DPAPI-encrypted `.bite` files as normal.

```json
// local.settings.json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
    // AZURE_KEYVAULT_NAME omitted → encryption disabled locally
  }
}
```

---

## Environment Variables

| Variable | Required in Azure | Default | Description |
|----------|:-----------------:|---------|-------------|
| `AZURE_KEYVAULT_NAME` | ✅ | — | Key Vault name (without `.vault.azure.net`) |
| `KEYVAULT_SECRET_NAME` | ❌ | `dp-keyring-v1` | Secret name inside the vault |
| `WorkflowsDirectory` | ❌ | `{BaseDir}/Resources` | Path to `.bite` resource files |

---

## Key Rotation

See [KeyRotationRunbook.md](KeyRotationRunbook.md).

---

## Security Concerns and Mitigations

See [SecurityChecklist.md](SecurityChecklist.md).

---

## Why Not .NET Data Protection?

.NET Data Protection (`IDataProtectionProvider`) is the preferred API for
in-process protect/unprotect scenarios.  For this use case it is not used
because:

- Key persistence on Consumption plan requires Azure Blob Storage, which
  is not free-tier.
- The PowerShell script on the developer machine must produce the same
  ciphertext format — replicating the internal Data Protection XML key-ring
  format from PowerShell is fragile.

AES-256-GCM is a NIST-approved, widely audited algorithm and provides
equivalent security with simpler cross-tool interoperability.

---

> This document is for version 3.0.2.79 of the scripts, as of 2026-06-22.
