# Key Rotation Runbook

Rotate the AES-256-GCM key every **90 days** or immediately on suspected
compromise.  Rotation is a zero-downtime operation when sequenced correctly.

---

## Rotation Schedule

| Trigger | Action |
|---------|--------|
| Every 90 days | Planned rotation (see below) |
| Key suspected compromised | Emergency rotation (see below) + incident log |
| Developer leaves team | Emergency rotation |
| Key Vault secret accidentally exposed in logs/repo | Emergency rotation |

---

## Planned Rotation (Zero-Downtime)

### Phase 1 — Generate new key and re-encrypt source files

Run on the developer machine that holds the original (pre-encryption) source
files.  **You must have the original DPAPI-encrypted or plain-text `.bite`
files**, not the deployed `WFAES::`-encrypted versions.

```powershell
# Supply original (DPAPI/plain) files and add -KeyRotate.
# The script fetches the OLD key to decrypt any existing WFAES:: values,
# then generates and stores a NEW key, and re-encrypts all files.
.\Scripts\Encrypt-Config.ps1 `
    -FilePath      "C:\Warewolf\OriginalResources" `
    -VaultName     "kv-warewolf-prod" `
    -KeyRotate `
    -FunctionApp   "func-warewolf-prod" `
    -ResourceGroup "rg-warewolf-prod" `
    -UploadToAzure
```

What happens internally:
1. `az keyvault secret show` retrieves the **current** (old) key version.
2. Any `WFAES::` values in the supplied files are decrypted with the old key.
3. A **new** 256-bit key is generated and stored via `az keyvault secret set`
   (Key Vault retains the old version automatically).
4. All files are re-encrypted with the new key and uploaded.

### Phase 2 — Deploy and verify

```bash
# Deploy the Function App (your normal CI/CD pipeline or manual zip-deploy).
az functionapp deploy \
  --resource-group rg-warewolf-prod \
  --name           func-warewolf-prod \
  --src-path       warewolf-func.zip \
  --type           zip

# Confirm the new cold start uses the new key.
# Application Insights query (KQL):
# traces
# | where message contains "SECURITY_AUDIT" and message contains "ColdStart"
# | project timestamp, message
# | order by timestamp desc
# | take 5
```

Verify that the logged `KeyId` matches the new key's ID (printed by
`Encrypt-Config.ps1`).

### Phase 3 — Retire the old key version (after 1 rotation cycle)

Key Vault retains all previous secret versions.  Delete the old version
after all instances have recycled (typically < 24 h on Consumption plan).

```bash
# List versions — note the old version ID.
az keyvault secret list-versions \
  --vault-name kv-warewolf-prod \
  --name       dp-keyring-v1 \
  --query "[].{id:id, created:attributes.created, enabled:attributes.enabled}" \
  -o table

# Disable the old version (soft delete; recoverable for 90 days).
az keyvault secret set-attributes \
  --vault-name kv-warewolf-prod \
  --name       dp-keyring-v1 \
  --version    <old-version-id> \
  --enabled    false
```

---

## Emergency Rotation (Suspected Compromise)

Same as planned rotation but add `--enabled false` on the current version
**before** restarting instances, to prevent any new Function instances from
loading the compromised key.

```bash
# 1. Disable current version immediately.
CURRENT_VERSION=$(az keyvault secret show \
  --vault-name kv-warewolf-prod \
  --name       dp-keyring-v1 \
  --query id -o tsv | sed 's|.*/||')

az keyvault secret set-attributes \
  --vault-name kv-warewolf-prod \
  --name       dp-keyring-v1 \
  --version    $CURRENT_VERSION \
  --enabled    false

# 2. Run rotation (same as planned — generates new key from original files).
.\Scripts\Encrypt-Config.ps1 -FilePath "..." -VaultName "kv-warewolf-prod" -KeyRotate -UploadToAzure ...

# 3. Restart all instances to force cold start with the new key.
az functionapp restart \
  --name           func-warewolf-prod \
  --resource-group rg-warewolf-prod
```

---

## Disaster Recovery

### Key Vault secret accidentally deleted (soft-delete active)

```bash
# List deleted secrets.
az keyvault secret list-deleted --vault-name kv-warewolf-prod -o table

# Recover.
az keyvault secret recover \
  --vault-name kv-warewolf-prod \
  --name       dp-keyring-v1
```

### Key Vault accidentally deleted (purge-protection active, within retention)

```bash
# List deleted vaults.
az keyvault list-deleted -o table

# Recover the entire vault.
az keyvault recover --name kv-warewolf-prod
```

### Key Vault permanently purged (purge-protection OFF) or beyond retention

There is no recovery path from Key Vault itself.  Use an offline backup:

```bash
# BACKUP (run after each rotation, store offline in a secure location).
az keyvault secret backup \
  --vault-name kv-warewolf-prod \
  --name       dp-keyring-v1 \
  --file       dp-keyring-v1.bak

# RESTORE (to the same or a replacement vault).
az keyvault secret restore \
  --vault-name kv-warewolf-prod \
  --file       dp-keyring-v1.bak
```

> **Store `dp-keyring-v1.bak` in an encrypted offline location** (e.g.,
> encrypted USB, offline secrets manager).  Never commit it to source control.

If both the backup and the vault are lost, the only recovery path is to
re-run `Encrypt-Config.ps1` from the original DPAPI-encrypted `.bite` files
on the original developer machine (DPAPI is still valid there).

---

## Key Lifecycle Summary

```
Day 0   : First run  — key generated, stored in KV, files encrypted.
Day 90  : Rotation   — new key generated, files re-encrypted, old version disabled.
Day 180 : Rotation   — new key generated, old version (Day 90) purged.
...
```

All previous secret versions are retained by Key Vault for at least the
`--retention-days` window (90 days as configured).  Delete old versions
explicitly after at least one full rotation cycle.
