# Security Checklist — Warewolf Lightweight Encryption

Review this checklist before every production deployment.

---

## Pre-Deployment

- [ ] `AZURE_KEYVAULT_NAME` app setting is set on the Function App.
- [ ] `KEYVAULT_SECRET_NAME` app setting is present (or default `dp-keyring-v1` is acceptable).
- [ ] Managed Identity is enabled on the Function App
      (`az functionapp identity show --name ... --resource-group ...`).
- [ ] "Key Vault Secrets User" role is assigned to the Function's Managed Identity
      and **not** any broader role (Contributor / Owner).
- [ ] No developer personal accounts have "Key Vault Secrets User" or broader on the vault
      in production (use "Key Vault Secrets Officer" scope-limited to non-prod vaults).
- [ ] Key Vault has `--enable-soft-delete true` and `--enable-purge-protection true`.
- [ ] All `.bite` source files in the deployment package have `WFAES::` on every
      `ConnectionString` attribute (no plain-text or DPAPI values remain).
- [ ] `.bite.bak` backup files are **not** included in the deployment package.
- [ ] Offline backup of `dp-keyring-v1.bak` is stored securely.

---

## Key Vault Access Policy (RBAC)

| Identity | Role | Scope |
|----------|------|-------|
| Function Managed Identity | Key Vault Secrets **User** | Vault resource |
| CI/CD service principal (if applicable) | Key Vault Secrets **User** | Vault resource |
| Developers (non-prod only) | Key Vault Secrets **Officer** | Non-prod vault |
| No one | Key Vault **Administrator** | (not needed at runtime) |

> Use the principle of least privilege.  "Secrets User" can only `GET` secrets,
> not list, set, or delete them.

---

## Known Security Concerns and Mitigations

### 1. AES key lives in Function instance memory for its lifetime

**Risk**: If a Function instance is compromised at OS/container level, the key
bytes in memory are accessible.

**Mitigation**: Consumption plan instances are ephemeral, isolated containers.
Azure does not share container memory across customers.  Key is wiped when the
instance is recycled (~20 min idle).  For higher assurance, consider Azure
Functions Premium plan with VNet injection + Private Endpoint on Key Vault.

---

### 2. Decrypted connection strings live in `DbSource` objects in memory

**Risk**: Once `DbSource` is constructed the plain connection string (including
password) lives in the managed heap.

**Mitigation**: This is the same risk as the full Warewolf server.  It is
inherent to the design and requires runtime memory protection (SecureString,
pinned buffers) if needed.  That is out of scope for this task.

---

### 3. `.bite.bak` backup files may contain DPAPI-encrypted values

**Risk**: Backups left in the repository or file system expose connection
string metadata.

**Mitigation**: Delete `.bite.bak` files immediately after verifying the
deployment.  Add `*.bite.bak` to `.gitignore`.

---

### 4. PowerShell `Add-Type` C# compilation happens at runtime

**Risk**: On air-gapped or policy-restricted machines, `Add-Type` may be
blocked.

**Mitigation**: `Encrypt-Config.ps1` is run by developers on trusted corporate
machines before deployment.  If `Add-Type` is blocked, compile
`WfAesHelper` separately and import the DLL instead.

---

### 5. Standard SKU Key Vault uses software-protected keys (no HSM)

**Risk**: Azure-side software vulnerability could expose key material.

**Mitigation**: Standard SKU is acceptable for most enterprise workloads.
Upgrade to Premium SKU with HSM-backed keys if your security policy requires
FIPS 140-2 Level 3 protection.  No code changes are required — only the
vault SKU changes.

---

### 6. AES-GCM nonce is randomly generated per encryption

**Risk**: Nonce collision probability for a 96-bit random nonce over
2^32 encryptions is ~10^-19 — negligible for this use case (source files
are re-encrypted at most hundreds of times in their lifetime).

**Mitigation**: No action needed.  If billions of encryptions are required,
switch to a counter-based nonce or HKDF-derived nonces.

---

### 7. No forward secrecy

**Risk**: Compromise of the single AES-256 key reveals all connection strings
encrypted with it.

**Mitigation**: Rotate the key every 90 days per the [Key Rotation Runbook](KeyRotationRunbook.md).
On rotation, re-encrypt all files.  This limits the blast radius to the
current rotation window.

---

### 8. Key Vault Standard SKU billable operations

**Risk**: Very high cold-start frequency (e.g., load spike) generates many
Key Vault GET ops → unexpected cost.

**Mitigation**: Consumption plan recycles idle instances after ~20 min.
At 1 GET per cold start, even 10,000 cold starts/day = 10,000 ops/day ≈
$0.03/day on Standard SKU.  This is negligible.  Monitor via Key Vault
diagnostic logs if concerned.

---

### 9. Connection strings in Application Insights logs

**Risk**: If a developer accidentally logs `dbSource.ConnectionString` at
`Information` level, the plain connection string reaches Application Insights.

**Mitigation**: `AuditLogger` never logs key material or decrypted values.
Review all logging calls in `WorkflowExecutor` and activity code.  Prefer
structured logging with explicit property names (never log whole objects).

---

## Monitoring Recommendations

- Enable **Key Vault diagnostic logs** → Log Analytics workspace.
  Alert on: unexpected `SecretGet` from unknown identities.
- Enable **Application Insights** on the Function App.
  Alert on: `SECURITY_AUDIT | Event=KeyVaultError`.
- Review Key Vault access logs monthly.
- Set an **Azure Policy** to deny Key Vault creation without soft-delete and
  purge-protection.
