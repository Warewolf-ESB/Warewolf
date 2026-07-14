# Spec: `download-bite/exchange` endpoint on the warewolf-devops server

## Why

`pipeline-CLOUD.yml`'s "Execute Simple Exchange Test Workflow" job has been
failing every run with:

```json
{ "hasErrors": true, "errors": ["Invalid Email Source"], "output": { "Result": "" } }
```

Root cause (confirmed by direct calls to the deployed smoke endpoint and to
`exchange.warewolf.online` — see analysis below): the failure happens
**before** any EWS call is attempted. `Azure Exchange Source.bite`
(`Dev\Warewolf.Execution.Lightweight\Resources\Azure Exchange Source.bite`)
was committed once, statically, with its `ConnectionString` AES-GCM-encrypted
(`WFAES::...`) under whatever Key Vault `dp-keyring-v1` key version was
current on 2026-04-27 (commit `4edc8a8487`). It has never been re-encrypted
since. Any Key Vault key rotation after that date (the documented 90-day
policy — see `Dev\Warewolf.Execution.Lightweight\docs\KeyRotationRunbook.md`)
silently invalidates the ciphertext: `LightweightSourceLoader.LoadSourceFile`
catches the resulting `CryptographicException` and returns `null`, so
`ResourceCatalog.GetResource<ExchangeSource>` never resolves the source, and
`DsfExchangeEmailNewActivity` emits `ErrorResource.InvalidEmailSource`
(`Dev\Dev2.Activities\Activities\Exchange\DsfExchangeEmailNewActivity.cs:198-202`).
The wiremock/EWS stub behind `exchange.warewolf.online` was confirmed
reachable and unaffected.

`NewSqlServerSource`/`RabbitMQ` avoid this class of bug entirely by never
committing an encrypted blob — the pipeline fetches a fresh `.bite` from the
devops server on every run via
`https://devops.warewolf.online/warewolf-devops/api/download-bite/{mssql|rabbitmq}`,
so the ConnectionString is always encrypted with whatever Key Vault key is
current *at deploy time*. `Dev\.azure\pipeline-CLOUD.yml` now expects the same
pattern for Exchange: a new `download-bite/exchange` route.

## What the pipeline now does

New step "Download Azure Exchange Source.bite from DevOps Endpoint" (added to
`Dev\.azure\pipeline-CLOUD.yml`, mirrors the existing mssql/rabbitmq steps
byte-for-byte in structure):

```
GET https://devops.warewolf.online/warewolf-devops/api/download-bite/exchange
Headers:
  CF-Access-Client-Id:     $(CFAccessClientId)
  CF-Access-Client-Secret: $(CFAccessClientSecret)
```

Expected on success: HTTP 200, a non-HTML content type, and a body containing
`<Source ... ID="e8a8f864-8ce8-47a1-8fca-5aef24bd63b3" ... ResourceType="ExchangeSource" ...>`.
The pipeline validates the body against that exact `ID` string and rejects
anything that looks like an HTML page (the Cloudflare Access SSO redirect
symptom already handled for mssql/rabbitmq). The response is saved as
`Resources\Azure Exchange Source.bite` in the Functions publish output,
overwriting the (now-fallback-only) static copy that ships in
`Dev\Warewolf.Execution.Lightweight\Resources\Azure Exchange Source.bite`.

## What needs to change on the devops server (`C:\Builds\warewolf-devops-mcp`, port 8788, behind Cloudflare Access)

Add a new route alongside the existing `mssql`/`rabbitmq` handlers under
`/warewolf-devops/api/download-bite/`:

`GET /warewolf-devops/api/download-bite/exchange`

1. **Auth**: same Cloudflare Access Service Token gate already enforced for
   `mssql`/`rabbitmq` (`CF-Access-Client-Id` / `CF-Access-Client-Secret`
   headers). No new auth mechanism needed.
2. **Source of truth for the plaintext connection properties**
   (`AutoDiscoverUrl`, `UserName`, `Password`, `Timeout`) for the
   `Azure Exchange Source` (`ResourceID e8a8f864-8ce8-47a1-8fca-5aef24bd63b3`,
   `AutoDiscoverUrl=http://exchange.warewolf.online/EWS/Exchange.asmx`,
   `UserName=testuser`): reuse whatever the devops server already holds for
   this shared Exchange test account (config/secret store on that machine —
   out of scope for this repo). This must **not** be the stale committed
   `.bite`; it must be the plaintext (or a copy decryptable with the
   currently-active key) so a fresh encryption can be produced.
3. **Encrypt fresh, every request**, using the *current* Key Vault
   `dp-keyring-v1` secret version (the same key
   `Warewolf.Execution.Lightweight` resolves at cold start — see
   `Dev\Warewolf.Execution.Lightweight\docs\README-Encryption.md`):
   ```
   ConnectionString = "WFAES::" + Base64([12-byte nonce][AES-256-GCM ciphertext of
       "AutoDiscoverUrl=...;UserName=...;Password=...;Timeout=..."][16-byte GCM tag])
   ```
   This is exactly what `Encrypt-Config.ps1` does for a `.bite` file; the
   endpoint should perform the equivalent encryption in-process (or shell out
   to the same routine) rather than caching a previously-encrypted value, so
   it is immune to future key rotations without any pipeline change.
4. **Response body** — valid `ExchangeSource` `.bite` XML, matching the shape
   already produced for `mssql`/`rabbitmq` and the existing committed sample:
   ```xml
   <Source ID="e8a8f864-8ce8-47a1-8fca-5aef24bd63b3"
           Name="Azure Exchange Source"
           ResourceType="ExchangeSource"
           IsValid="false"
           ConnectionString="WFAES::<fresh-ciphertext>"
           Type="ExchangeSource"
           ServerVersion="0.0.0.0"
           ServerID="51a58300-7e9d-4927-a57b-e5d700b11b55">
   </Source>
   ```
5. **Content-Type**: anything that is not `text/html` (the pipeline only
   checks for the absence of an HTML/SSO-login response, matching the
   mssql/rabbitmq checks) — `application/xml` or `text/plain` is fine.
6. **Failure modes**: return a real non-200 status (or an empty/malformed
   body) on error rather than an HTML page, so pipeline retries/failures are
   attributable to the correct cause.

## Verification once deployed

- `Invoke-WebRequest -Uri https://devops.warewolf.online/warewolf-devops/api/download-bite/exchange -Headers @{...}`
  should return the XML above with a **new** ciphertext each time the Key
  Vault key rotates (same ciphertext between rotations is fine/expected if
  the encryption is deterministic-per-key, but must change immediately after
  a rotation).
- Re-run the "Execute Simple Exchange Test Workflow" pipeline job; it should
  no longer return `Invalid Email Source`.
