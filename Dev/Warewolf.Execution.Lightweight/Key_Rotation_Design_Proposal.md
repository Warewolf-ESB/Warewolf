# Key Rotation Design Proposal

**Warewolf.Execution.Lightweight · AES-256-GCM Encryption Layer**  
Prepared for Architecture Review · August 2026

---

## 1. Purpose

This document describes the current encryption architecture in Warewolf.Execution.Lightweight, identifies the gap in key rotation support, and documents the implemented and planned changes that allow Azure Key Vault keys to be rotated without breaking existing encrypted resources.

The intended audience is the solution architect and senior engineers responsible for the security posture of the Warewolf platform.

---

## 2. Current Architecture (Before Changes)

### 2.1 Algorithm and Key Storage

All sensitive values are protected with AES-256-GCM. A single 32-byte key is stored in Azure Key Vault under the secret name `dp-keyring-v1` and fetched once at cold start by `KeyVaultSecretManager.InitializeAsync()`. After initialisation, no further Key Vault calls are made — all cryptographic operations are purely in-memory.

The ciphertext format for every encrypted value is:

```
WFAES::{Base64([12-byte nonce][ciphertext][16-byte GCM tag])}
```

A fresh random 96-bit nonce is generated per encryption call, so the same plaintext always produces different ciphertext. This format is unchanged by any of the work described in this document.

### 2.2 Encrypted Resource Types

| Resource type | When encrypted |
|---|---|
| `.bite` source files | Deploy time — external `Encrypt-Config.ps1` script |
| Suspended workflow environments | Runtime — `SuspendExecutionActivity` via `DpapiWrapper` |
| Suspended workflow principals | Runtime — `SuspendExecutionActivity` via `DpapiWrapper` |
| Dropbox OAuth tokens | Runtime — `DropboxOAuthFunction` on token refresh |

### 2.3 The Gap — What Breaks on Key Rotation

> ⚠️ **Critical gap:** Rotating the Key Vault secret immediately breaks all previously encrypted artifacts. `FileDecryptionHelper.DecryptConnectionString` used exactly one key — a GCM tag mismatch threw `CryptographicException` with no fallback, no retry, and no re-encryption path.

| Affected artifact | Impact |
|---|---|
| `.bite` source files | Fail to decrypt at startup — service cannot start |
| Suspended Hangfire jobs | Fail to resume — workflows permanently stuck until old key is restored |
| Dropbox OAuth tokens | Fail to decrypt — OAuth flow broken until file is re-encrypted |

---

## 3. Phase 1 — Implemented: Multi-Key Decryption Fallback

### 3.1 Summary

The fallback chain allows the system to decrypt data encrypted with any previous key, transparently and automatically. No environment variables or feature flags are required — the fallback activates whenever `previousKeys` are present in the Key Vault secret. When no previous keys exist, the system behaves exactly as before (single-key fast path, zero overhead).

### 3.2 Key Vault Secret — Version 2 Schema

The Key Vault JSON secret is extended with an optional `PreviousKeys` array. Version 1 secrets (no `previousKeys` field) remain fully supported — no migration required for environments that have not yet rotated.

```json
{
  "Version": 2,
  "KeyId": "key-2026-08",
  "Key": "<base64 new key>",
  "Created": "2026-08-05",
  "PreviousKeys": [
    { "KeyId": "key-2024-01", "Key": "<base64 old key>", "Retired": "2026-08-05" }
  ]
}
```

**Constraints:**
- `PreviousKeys` is optional — omitting it is equivalent to an empty array.
- Retain no more than 3 previous keys. Older entries can be pruned once all resources encrypted with them have been re-encrypted.
- The `Retired` date is informational — it records when the key was rotated out and helps operators decide when it is safe to prune.

### 3.3 Changes Made

**`KeyVaultSecretManager.cs`**
- Added `PreviousKeyEntry` record (`keyId`, `key`, `retired`) to represent a retired key.
- Extended `KeyRingMaterial` DTO with an optional `PreviousKeys` list, defaulting to null when absent (backward compatible).
- Added `GetAllKeyBytes()` — returns the primary key first, followed by any previous keys in reverse-chronological order (most recently retired first). If no previous keys exist, only the primary key is returned.

**`FileDecryptionHelper.cs`**
- Constructor now accepts `ILogger<FileDecryptionHelper>` and stores the full key ring from `GetAllKeyBytes()` instead of a single `byte[]`.
- `DecryptConnectionString` walks the key ring on GCM failure. Steps:

| Step | Behaviour |
|---|---|
| 1 | Attempt decryption with the primary key — succeeds for all data encrypted after the last rotation, zero overhead in steady state. |
| 2 | On `CryptographicException`: iterate previous keys in order, attempt decryption with each. |
| 3 | If a previous key succeeds, return the plaintext and log a structured warning identifying the fallback `KeyId` used. |
| 4 | If all keys fail, re-throw with a `CryptographicException` listing every `KeyId` attempted — no silent failures. |

**`ServiceCollectionExtensions.cs`**
- Updated `FileDecryptionHelper` DI registration to inject `ILogger<FileDecryptionHelper>`.

### 3.4 What Does Not Change

- `FileEncryptionHelper.Encrypt` — always encrypts with the primary key. No changes needed.
- `WFAES::` wire format — nonce + ciphertext + tag layout is unchanged.
- Existing `.bite` files, Hangfire jobs, and OAuth tokens on disk — untouched. They are decrypted via the fallback chain and re-encrypted only when written back (naturally or via `Encrypt-Config.ps1`).

### 3.5 Key Rotation Procedure (Phase 1)

| Step | Action |
|---|---|
| 1 — Generate | Generate a new 32-byte AES key using Azure Key Vault or a FIPS-approved tool. |
| 2 — Update secret | Update `dp-keyring-v1`: move the current `Key` into `PreviousKeys` with a `Retired` timestamp, set the new key as `Key`, update `KeyId` and `Created`. |
| 3 — Deploy | Deploy the updated service. At cold start, the new key is loaded and the fallback chain is ready. |
| 4 — Verify | Confirm in logs that `Key ring loaded — 2 key(s) available for decryption`. Check that `.bite` sources and suspended workflows load without error. Watch for `KeyRotation | Decrypted using fallback key` warnings — these identify resources still on the old key. |
| 5 — Re-encrypt `.bite` files | Run `Encrypt-Config.ps1` to re-encrypt all `.bite` files with the new primary key. After this, `.bite` file fallback warnings should stop. |
| 6 — Prune (after 30 days) | Once no fallback warnings appear in logs, remove the `PreviousKeys` entry from the Key Vault secret. Redeploy. Startup logs should show `Key ring loaded — primary key only`. |

### 3.6 How to Test

**Baseline — confirm nothing broke**
Deploy with an existing Version 1 secret. Startup logs must show: `Key ring loaded — primary key only (no previous keys in secret).` All resources must decrypt as before.

**Simulate a rotation**
Update the Key Vault secret to Version 2 — move the current key to `previousKeys`, set a new key as primary. Leave `.bite` files on disk unchanged (still encrypted with the old key).

**Verify fallback decryption**
Deploy. Logs must show: `Key ring loaded — 2 key(s) available for decryption.` All `.bite` sources and suspended Hangfire jobs must load without error. Each resource using the old key must produce a log warning: `KeyRotation | Decrypted using fallback key 'key-2024-01'.`

**Verify new encryptions use the new key**
Trigger a workflow suspension or Dropbox OAuth refresh. Confirm no fallback warning for those new values — they must decrypt on the first attempt with the primary key.

**Confirm failure behaviour**
Set an incorrect key in the secret with no matching previous key. Confirm a `CryptographicException` is thrown listing all `KeyId`s tried — no partial loads or silent failures.

**Rotation complete — prune**
Remove `previousKeys` from the secret and redeploy. Startup logs must show primary key only. Existing resources (re-encrypted by `Encrypt-Config.ps1`) must all decrypt on the first attempt.

---

## 4. Phase 2 — Planned: Operational Controls and Auto Re-encryption Sweep

Phase 2 builds on the fallback chain to eliminate the remaining manual step (running `Encrypt-Config.ps1`) and adds operator controls for managing the rotation lifecycle.

### 4.1 AUTOREENCRYPTIONENABLED Environment Variable

Introduce an `AUTOREENCRYPTIONENABLED` environment variable (`true` / `false`, default `false`) that activates the startup re-encryption sweep (§4.2). This gives operators explicit control over when re-encryption runs:

- `false` (default): fallback chain is still active, but no re-encryption sweep runs. Resources are decrypted via the fallback on demand only. Matches current Phase 1 behaviour.
- `true`: activates the sweep — resources still on old keys are proactively re-encrypted at cold start. Set this during the rotation window and revert once sweep completion is confirmed in logs.

The env var also gates loading of `PreviousKeys` into memory — when `false`, previous keys are parsed from the secret but not held long-term, reducing the in-memory footprint outside of rotation windows.

### 4.2 .bite File Re-encryption Sweep

When `AUTOREENCRYPTIONENABLED=true`, a sweep runs at cold start — after `InitializeKeyVaultAsync()` and before service traffic is accepted:

- Enumerate all `.bite` files in the resource catalog.
- For each file: decrypt the `ConnectionString` field (fallback chain handles old keys), re-encrypt with the current primary key, write back atomically (write to `.tmp`, then rename).
- Files already on the current key succeed on the first attempt and are skipped with no I/O — idempotent on repeat restarts.
- Failures are logged with the file path and exception; the sweep continues with remaining files rather than aborting startup.
- On completion, emit a structured log summary: files swept, files skipped, errors, duration.

This eliminates the need to manually run `Encrypt-Config.ps1` after a key rotation.

### 4.3 Hangfire Job Re-encryption Sweep

When `AUTOREENCRYPTIONENABLED=true`, a second sweep runs as a background `IHostedService` after the host is ready (does not block service readiness or the health-check endpoint):

- Query Hangfire storage for all jobs in Suspended / Awaiting state.
- For each job: decrypt the `environment` and `currentuserprincipal` parameters via the fallback chain, re-encrypt with the current primary key, update the job record atomically.
- Failed job updates are retried on next restart — workflow execution is unaffected.
- On completion, emit a structured log summary: jobs swept, jobs skipped, errors, duration.

This is the only automated path for re-encrypting suspended workflow data, as `Encrypt-Config.ps1` has no visibility into Hangfire storage.

### 4.4 New Files Required

| File | Purpose |
|---|---|
| `HostEnvironmentConfig.cs` | Add `KeyRotationMode` bool property driven by `AUTOREENCRYPTIONENABLED` |
| New: `BiteSweepService.cs` | `IHostedService` — sweeps `.bite` files at cold start when enabled |
| New: `HangfireJobSweepService.cs` | `IHostedService` — sweeps suspended Hangfire jobs in the background when enabled |
| `KeyVaultStartupExtensions.cs` | Register and conditionally invoke both sweep services |

### 4.5 Updated Rotation Procedure (Phase 2)

Once Phase 2 is in place, the `Encrypt-Config.ps1` manual step is replaced by the sweep:

| Step | Action |
|---|---|
| 1 — Generate | Generate a new 32-byte AES key. |
| 2 — Update secret | Update `dp-keyring-v1` to Version 2 format with the new key and old key in `PreviousKeys`. |
| 3 — Set env var | Set `AUTOREENCRYPTIONENABLED=true` in the deployment configuration. |
| 4 — Deploy | At cold start: new key loaded, `.bite` sweep runs before traffic, Hangfire sweep runs in background. |
| 5 — Verify | Confirm sweep counts in logs. Verify no fallback warnings remain after sweep. |
| 6 — Disable sweep | Set `AUTOREENCRYPTIONENABLED=false`. Remove `PreviousKeys` from Key Vault secret after the 30-day retention window. |

### 4.6 Open Questions for Architect

- Is 30 days an acceptable minimum `PreviousKey` retention window, or does compliance policy require longer?
- Should sweep errors surface as Application Insights alerts or are structured logs sufficient?
- Should the `.bite` sweep block service readiness (delay health-check) or run concurrently with the Hangfire sweep?
- Is there a dual-region Key Vault failover requirement? If so, cold-start behaviour during vault outages must be specified.

---

## 5. Risks and Mitigations

| Risk | Phase | Mitigation |
|---|---|---|
| Fallback key missing from ring — old resource unreadable | 1 & 2 | `CryptographicException` thrown with clear `KeyId` chain. Operator must restore the missing key to `PreviousKeys`. |
| `PreviousKeys` pruned too early | 1 & 2 | Enforce 30-day minimum retention. Log sweep completion counts so operators know when pruning is safe. |
| Key ring grows unbounded | 1 & 2 | Cap `PreviousKeys` at 3 entries. Document that older entries require a manual re-encryption run before pruning. |
| `.bite` sweep fails mid-way | 2 | Sweep is idempotent — re-runs on next restart. Fallback chain covers remaining old-key files. |
| Hangfire sweep update fails | 2 | Each job update is atomic. Failed jobs retry on next restart without affecting workflow execution. |
| Cold-start latency increases | 2 | `.bite` sweep parallelism configurable (default: 4 concurrent). Hangfire sweep runs post-readiness — no cold-start impact. |

---

## 6. Acceptance Criteria

### Phase 1 (Implemented)
- A resource encrypted with key N decrypts successfully after rotating to key N+1, with no manual intervention at deploy time.
- When no previous keys exist in the secret, behaviour is identical to pre-change (single-key fast path).
- A fallback-key success produces a structured warning log with the `KeyId` that matched.
- If all keys in the ring fail, a `CryptographicException` is thrown listing every `KeyId` attempted — no silent failures.
- The wire format (`WFAES::` prefix, nonce+ciphertext+tag layout) is unchanged.
- Existing unit and integration tests pass without modification.

### Phase 2 (Planned)
- When `AUTOREENCRYPTIONENABLED=true`, all `.bite` files are re-encrypted with the current key at cold start before traffic is accepted.
- When `AUTOREENCRYPTIONENABLED=true`, all suspended Hangfire jobs are re-encrypted in the background without blocking readiness.
- Resources already on the current key are not rewritten (idempotent).
- Sweep completion is observable in structured logs: files swept, jobs swept, errors, and duration.
- After a full sweep, no fallback warnings appear in logs — all resources are on the current key.
- Setting `AUTOREENCRYPTIONENABLED=false` reverts to Phase 1 behaviour with zero sweep overhead.

---

*Document version 2.0 · Warewolf Platform Security · For internal review only*
