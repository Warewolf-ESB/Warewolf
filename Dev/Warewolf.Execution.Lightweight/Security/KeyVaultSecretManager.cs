/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Dev2.Common;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Fetches and caches AES-256-GCM key material from Azure Key Vault on each
    /// cold start.  After <see cref="InitializeAsync"/> completes, every call to
    /// <see cref="GetKeyBytes"/> is a pure in-memory operation — zero Key Vault
    /// round-trips per invocation.
    ///
    /// Key Vault ops per Function instance lifetime: 1 GET (cold start only).
    ///
    /// In development, when <c>DEBUG_AZURE_KEYVAULT_SECRET</c> is set, the Key Vault
    /// call is bypassed entirely and the secret value is taken directly from that
    /// environment variable — no Azure authentication required.
    ///
    /// Authentication strategy is resolved externally and injected as a
    /// <see cref="Azure.Core.TokenCredential"/>:
    ///   • Azure cloud   → <see cref="Azure.Identity.ManagedIdentityCredential"/> (system-assigned or user-assigned)
    ///   • Local dev     → <see cref="Azure.Identity.ChainedTokenCredential"/> (EnvironmentCredential → AzureCli → VisualStudio)
    /// </summary>
    public sealed class KeyVaultSecretManager
    {
        static readonly JsonSerializerOptions _jsonOptions =
            new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Highest <c>Version</c> of the key-ring document this build understands.
        /// Version 1 = primary key only; Version 2 = primary key + <c>previousKeys</c>.
        /// A higher version is accepted with a warning (see <see cref="ParseAndSetMaterial"/>).
        /// </summary>
        const int MaxSupportedVersion = 2;

        /// <summary>
        /// Upper bound on the legacy-repair regex engine. The secret is external input, so
        /// an unbounded match on pathological content could hang cold start indefinitely.
        /// A timeout surfaces as a clean startup error instead of a hung Function host.
        /// </summary>
        static readonly TimeSpan LegacyRepairTimeout = TimeSpan.FromMilliseconds(250);

        /// <summary>Quotes bare object keys: <c>{version:</c> → <c>{"version":</c>.</summary>
        static readonly Regex LegacyKeyQuoteRegex =
            new(@"([\{,])\s*([a-zA-Z_]\w*)\s*:", RegexOptions.None, LegacyRepairTimeout);

        /// <summary>Quotes bare scalar values: <c>:abc,</c> → <c>:"abc",</c>.</summary>
        static readonly Regex LegacyValueQuoteRegex =
            new(@":\s*(?!"")([^,\}]+)", RegexOptions.None, LegacyRepairTimeout);

        readonly string                         _vaultUri;
        readonly string                         _secretName;
        readonly TokenCredential?               _credential;
        readonly SecretClient?                  _client;
        readonly ILogger<KeyVaultSecretManager> _logger;
        readonly string?                        _debugSecret;

        KeyRingMaterial? _material;

        public KeyVaultSecretManager(
            string                         vaultUri,
            string                         secretName,
            TokenCredential?               credential,
            ILogger<KeyVaultSecretManager> logger,
            string?                        debugSecret = null)
        {
            const string executionId = "KeyVaultSecretManager-Constructor";

            _vaultUri    = vaultUri   ?? throw new ArgumentNullException(nameof(vaultUri));
            _secretName  = secretName ?? throw new ArgumentNullException(nameof(secretName));
            _logger      = logger     ?? throw new ArgumentNullException(nameof(logger));
            _debugSecret = debugSecret;

            // Vault URI, secret name, key id and any length of the key material are never
            // logged, at any level — only presence/absence flags, which are what actually
            // diagnose a misconfiguration.
            Dev2Logger.Debug($"KeyVaultSecretManager constructor. HasDebugSecret: {debugSecret != null}, HasCredential: {credential != null}", executionId);

            if (debugSecret is null)
            {
                _credential = credential ?? throw new ArgumentNullException(nameof(credential));
                _client     = new SecretClient(new Uri(_vaultUri), _credential);
                Dev2Logger.Info("KeyVaultSecretManager initialized with SecretClient.", executionId);
            }
            else
            {
                Dev2Logger.Warn("KeyVaultSecretManager initialized with DEBUG secret (Key Vault will be bypassed)", executionId);
            }
        }

        /// <summary>
        /// Key ID from the fetched secret (used in audit log; never the actual key).
        /// </summary>
        public string KeyId => _material?.KeyId ?? "(not initialised)";

        /// <summary>
        /// Fetches the secret from Key Vault and deserialises the key ring material.
        /// In development, when a debug secret value is configured, the Key Vault call
        /// is skipped and the value is parsed directly.
        /// Must be called once at startup before any call to <see cref="GetKeyBytes"/>.
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            const string executionId = "KeyVaultSecretManager-Initialize";

            Dev2Logger.Info("KeyVaultSecretManager InitializeAsync starting.", executionId);

            try
            {
                // Defence in depth. ServiceCollectionExtensions already gates the debug
                // bypass on HostEnvironmentConfig.IsDevelopment, so _debugSecret is
                // normally null in the cloud. This second check closes the remaining hole:
                // someone setting ASPNETCORE_ENVIRONMENT=Development on a live Function App
                // would otherwise re-open the bypass and inject unmanaged key material.
                // Cloud markers are authoritative here — they cannot be faked by an env-var
                // flip. Fail-fast is correct: this is a misconfiguration, not a rotation
                // edge case.
                if (_debugSecret is not null && IsProductionEnvironment())
                {
                    Dev2Logger.Error("DEBUG_AZURE_KEYVAULT_SECRET is set in a production/cloud-hosted environment — refusing to bypass Key Vault.", executionId);
                    _logger.LogError(
                        "KeyVault | DEBUG_AZURE_KEYVAULT_SECRET is set while running cloud-hosted — refusing to bypass Key Vault.");
                    throw new InvalidOperationException(
                        "DEBUG_AZURE_KEYVAULT_SECRET must not be used in production. " +
                        "Remove the environment variable from the Function App and use " +
                        "Managed Identity + Key Vault instead.");
                }

                if (_debugSecret is not null)
                {
                    Dev2Logger.Warn("KeyVaultSecretManager using DEBUG_AZURE_KEYVAULT_SECRET (Key Vault skipped)", executionId);

                    _logger.LogInformation(
                        "KeyVault | Development mode — Key Vault call skipped.");
                    ParseAndSetMaterial(_debugSecret);

                    Dev2Logger.Info("KeyVaultSecretManager InitializeAsync completed (DEBUG mode).", executionId);
                    return;
                }

                Dev2Logger.Debug($"KeyVaultSecretManager fetching configuration secret. CredentialType: {_credential!.GetType().Name}", executionId);

                // Secret name, vault URI and credential type are Debug-only (see the
                // Dev2Logger.Debug line above) — they must not reach production sinks.
                _logger.LogInformation("KeyVault | Fetching configuration secret from Key Vault.");

                KeyVaultSecret secret =
                    await _client!.GetSecretAsync(_secretName, version: null, cancellationToken)
                                 .ConfigureAwait(false);

                Dev2Logger.Debug("KeyVaultSecretManager successfully fetched configuration secret from Key Vault", executionId);

                ParseAndSetMaterial(secret.Value);

                Dev2Logger.Info("KeyVaultSecretManager InitializeAsync completed successfully.", executionId);
            }
            catch (Exception ex)
            {
                // Neither the exception object nor ex.Message is emitted: this catch covers
                // GetSecretAsync, so ex is the raw Azure SDK exception — a 403 RequestFailedException
                // names the caller client IP, a 404 the vault and secret, an AuthenticationFailedException
                // the tenant and identity. Only the exception type is safe to surface.
                Dev2Logger.Error($"KeyVaultSecretManager InitializeAsync failed. ExceptionType={ex.GetType().Name}", executionId);
                throw;
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// <c>true</c> when the process is running cloud-hosted AND not explicitly marked
        /// as a development environment.
        ///
        /// Detection uses platform-injected markers rather than a configuration value:
        /// <c>WEBSITE_INSTANCE_ID</c> and <c>FUNCTIONS_WORKER_RUNTIME</c> are set by Azure
        /// Functions / App Service and cannot be produced by flipping an app setting, so
        /// they are a trustworthy signal that this is real hosted infrastructure.
        ///
        /// An absent environment name while cloud-hosted is treated as production — the
        /// safe default, since the debug bypass should never be reachable by omission.
        /// Local development is unaffected: with no cloud markers present this always
        /// returns <c>false</c> and the bypass keeps working.
        /// </summary>
        static bool IsProductionEnvironment()
        {
            var isCloudHosted =
                Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")     is not null ||
                Environment.GetEnvironmentVariable("FUNCTIONS_WORKER_RUNTIME") is not null;

            if (!isCloudHosted)
                return false;

            var environmentName =
                Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")      ??
                Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            // Unset while cloud-hosted → treat as production.
            return environmentName is null
                || string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase);
        }

        void ParseAndSetMaterial(string rawJson)
        {
            const string executionId = "KeyVaultSecretManager-Parse";

            // The secret's length characterises the key material — not logged.
            Dev2Logger.Debug("KeyVaultSecretManager ParseAndSetMaterial starting.", executionId);

            // ── Legacy unquoted-JSON repair ────────────────────────────────────────────
            // Old versions of Encrypt-Config.ps1 wrote unquoted JSON, e.g.
            //   {version:1,keyId:abc,...} → {"version":1,"keyId":"abc",...}
            // The trigger below is deliberately narrow so a canonical secret is NEVER
            // touched by the regex engine.
            var wasRepaired = false;

            if (!rawJson.TrimStart().StartsWith("{\""))
            {
                // The repair is FLAT-ONLY. Both passes are line-noise regexes with no
                // concept of '[', ']' or nesting, so on a Version 2 secret (which carries
                // a previousKeys array) they silently mangle the array into invalid JSON
                // and surface later as a confusing "could not be converted to
                // List<PreviousKeyEntry>" deserialisation error. Refuse up front and tell
                // the operator exactly what to do instead of corrupting the document.
                if (rawJson.Contains('['))
                {
                    Dev2Logger.Error($"KeyVaultSecretManager secret '{_secretName}' is unquoted JSON containing a nested array — the legacy repair cannot handle nested structures and will not be attempted.", executionId);
                    throw new InvalidOperationException(
                        $"Key Vault secret '{_secretName}' is in the legacy unquoted-JSON format AND contains a " +
                        "nested array (previousKeys). The legacy auto-repair only supports the flat Version 1 " +
                        "shape and cannot repair nested structures without corrupting them. " +
                        "Rewrite the secret as canonical, fully-quoted JSON — e.g. re-run " +
                        "Encrypt-Config.ps1 -GenerateKeys, or write the value from a UTF-8 (no BOM) file via " +
                        "'az keyvault secret set --file' so the quotes are preserved.");
                }

                Dev2Logger.Warn($"KeyVaultSecretManager secret '{_secretName}' contained unquoted JSON - auto-repairing", executionId);

                try
                {
                    rawJson = LegacyKeyQuoteRegex.Replace(rawJson, "$1\"$2\":");
                    rawJson = LegacyValueQuoteRegex.Replace(rawJson, ":\"$1\"");
                }
                catch (RegexMatchTimeoutException ex)
                {
                    Dev2Logger.Error($"KeyVaultSecretManager legacy repair of secret '{_secretName}' exceeded the {LegacyRepairTimeout.TotalMilliseconds}ms timeout — aborting.", ex, executionId);
                    throw new InvalidOperationException(
                        $"Legacy unquoted-JSON repair of Key Vault secret '{_secretName}' exceeded the " +
                        $"{LegacyRepairTimeout.TotalMilliseconds}ms limit. The secret is malformed or " +
                        "pathologically large. Rewrite it as canonical, fully-quoted JSON.", ex);
                }

                wasRepaired = true;

                _logger.LogWarning(
                    "KeyVault | Secret '{SecretName}' contained unquoted JSON — auto-repaired (flat Version 1 shape). " +
                    "Re-run Encrypt-Config.ps1 -GenerateKeys to store a canonical version.",
                    _secretName);
            }

            try
            {
                // Fail closed: if the repaired document still does not deserialise, the
                // secret is a non-repairable legacy format — say so plainly rather than
                // letting a generic parse error mislead the operator.
                _material = JsonSerializer.Deserialize<KeyRingMaterial>(rawJson, _jsonOptions)
                            ?? throw new InvalidOperationException(
                                wasRepaired
                                    ? $"Key Vault secret '{_secretName}' is a non-repairable legacy format — " +
                                      "auto-repair of the unquoted JSON ran but the result still did not " +
                                      "deserialise. Rewrite the secret as canonical, fully-quoted JSON."
                                    : $"Failed to deserialise key material from secret '{_secretName}'.");

                if (string.IsNullOrWhiteSpace(_material.Key))
                {
                    Dev2Logger.Error("KeyVaultSecretManager configuration secret contains empty key material", executionId);
                    throw new InvalidOperationException(
                        "Key Vault secret contains empty key material.");
                }

                // KeyId / Created are key-material metadata — Debug-only (see GetKeyBytes).
                Dev2Logger.Info("KeyVaultSecretManager key material parsed successfully.", executionId);

                _logger.LogInformation("KeyVault | Key loaded.");
            }
            catch (Exception ex)
            {
                // ex.Message is withheld: the deserialise failure thrown above names the secret,
                // and a JsonException reports the path/position inside the raw key-material JSON.
                Dev2Logger.Error($"KeyVaultSecretManager ParseAndSetMaterial failed. ExceptionType={ex.GetType().Name}", executionId);
                throw;
            }
        }

        /// <summary>
        /// Returns the 32-byte (256-bit) AES key decoded from the secret.
        /// Throws <see cref="InvalidOperationException"/> when called before
        /// <see cref="InitializeAsync"/>.
        /// </summary>
        public byte[] GetKeyBytes()
        {
            const string executionId = "KeyVaultSecretManager-GetKeyBytes";

            if (_material is null)
            {
                Dev2Logger.Error($"KeyVaultSecretManager GetKeyBytes called before InitializeAsync", executionId);
                throw new InvalidOperationException(
                    $"{nameof(KeyVaultSecretManager)} is not initialised. " +
                    $"Call {nameof(InitializeAsync)} before resolving {nameof(FileDecryptionHelper)}.");
            }

            try
            {
                var keyBytes = Convert.FromBase64String(_material.Key);

                if (keyBytes.Length != 32)
                {
                    // The actual length is never logged — it characterises the key material.
                    // The thrown InvalidOperationException below still carries it to the caller.
                    Dev2Logger.Error("KeyVaultSecretManager invalid key length (expected 32 bytes)", executionId);
                    throw new InvalidOperationException(
                        $"AES-256 key must be 32 bytes; got {keyBytes.Length}. " +
                        "Re-run Encrypt-Config.ps1 to regenerate the key material.");
                }

                Dev2Logger.Debug("KeyVaultSecretManager GetKeyBytes successful.", executionId);
                return keyBytes;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                Dev2Logger.Error($"KeyVaultSecretManager GetKeyBytes failed: {ex.Message}", executionId);
                throw;
            }
        }

        /// <summary>
        /// Returns all available decryption keys: the primary key first, followed by any
        /// previous keys sorted by <c>Retired</c> descending (most recently retired first),
        /// so the key most likely to match is attempted earliest. Entries with a blank or
        /// unparseable <c>Retired</c> value are retained but sorted last.
        ///
        /// When no <c>PreviousKeys</c> are present in the secret (Version 1 format),
        /// only the primary key is returned — fully backward compatible.
        ///
        /// A previous key with malformed Base64 or a non-32-byte length is SKIPPED with a
        /// warning rather than throwing: one bad retired entry must never prevent the ring
        /// (and therefore the engine) from loading. The primary key remains fail-fast — an
        /// invalid primary key is a genuine misconfiguration and still throws.
        ///
        /// Each entry is a tuple of (KeyId, KeyBytes) so callers can log which key
        /// succeeded without exposing raw key material.
        /// </summary>
        public IReadOnlyList<(string KeyId, byte[] KeyBytes)> GetAllKeyBytes()
        {
            const string executionId = "KeyVaultSecretManager-GetAllKeyBytes";

            if (_material is null)
                throw new InvalidOperationException(
                    $"{nameof(KeyVaultSecretManager)} is not initialised. " +
                    $"Call {nameof(InitializeAsync)} before resolving {nameof(FileDecryptionHelper)}.");

            var keys = new List<(string, byte[])>
            {
                (_material.KeyId, GetKeyBytes()),
            };

            if (_material.PreviousKeys is { Count: > 0 })
            {
                // Order previous keys most-recently-retired first, so the key most likely
                // to match a given ciphertext is attempted earliest during fallback.
                // Blank or unparseable Retired values sort to DateTime.MinValue (last) —
                // they are never dropped, only deprioritised.
                var ordered = _material.PreviousKeys
                    .OrderByDescending(p =>
                        DateTime.TryParse(p.Retired, CultureInfo.InvariantCulture, DateTimeStyles.None, out var retiredOn)
                            ? retiredOn
                            : DateTime.MinValue);

                var skipped = 0;

                foreach (var prev in ordered)
                {
                    // A malformed PREVIOUS key must never abort the ring — the primary key
                    // (already decoded above) stays usable and the engine still starts.
                    // Only the primary key is fail-fast; see GetKeyBytes().
                    byte[] prevBytes;
                    try
                    {
                        prevBytes = Convert.FromBase64String(prev.Key);
                    }
                    catch (FormatException)
                    {
                        skipped++;
                        Dev2Logger.Warn($"Previous key '{prev.KeyId}' has invalid Base64 material — skipping. The key ring remains usable; resources encrypted with this key will fail to decrypt until the secret is corrected.", executionId);
                        _logger.LogWarning(
                            "KeyVault | Previous key '{PreviousKeyId}' has invalid Base64 material — skipped. " +
                            "Correct the '{SecretName}' secret if resources are still encrypted with this key.",
                            prev.KeyId, _secretName);
                        continue;
                    }

                    if (prevBytes.Length != 32)
                    {
                        skipped++;
                        Dev2Logger.Warn($"Previous key '{prev.KeyId}' has invalid length {prevBytes.Length} bytes (expected 32) — skipping.", executionId);
                        _logger.LogWarning(
                            "KeyVault | Previous key '{PreviousKeyId}' has invalid length {KeyLength} bytes (expected 32) — skipped.",
                            prev.KeyId, prevBytes.Length);
                        continue;
                    }

                    keys.Add((prev.KeyId, prevBytes));
                }

                var loadedIds = string.Join(", ", keys.Skip(1).Select(k => k.Item1));
                Dev2Logger.Info($"Key ring loaded — {keys.Count} key(s) available for decryption. PreviousKeyIds (newest retired first): [{loadedIds}]. Skipped: {skipped}.", executionId);
                _logger.LogInformation(
                    "KeyVault | Key ring loaded — {KeyCount} key(s) available. Primary={KeyId}, previous (newest retired first)=[{PreviousKeyIds}], skipped={SkippedCount}.",
                    keys.Count, _material.KeyId, loadedIds, skipped);
            }
            else
            {
                Dev2Logger.Info("Key ring loaded — primary key only (no previous keys in secret). Single-key decryption mode active.", executionId);
                _logger.LogInformation(
                    "KeyVault | Key ring loaded — primary key only (KeyId={KeyId}). No previous keys found in secret. Single-key decryption mode active.",
                    _material.KeyId);
            }

            return keys;
        }

        // ── Key-material DTOs (match the JSON written by Encrypt-Config.ps1) ────

        /// <summary>
        /// A previously-active key that has been rotated out. Retained in the Key Vault
        /// secret for the duration of the rotation window so that resources encrypted
        /// with this key can still be decrypted.
        ///
        /// Declared as a class with init-only properties rather than a positional record
        /// for clarity: each JSON field maps to one explicitly-attributed property, so the
        /// binding is obvious at a glance and does not depend on constructor-parameter
        /// matching. (Positional records DO deserialize correctly under
        /// <see cref="JsonSerializerDefaults.Web"/>; this is a readability choice, not a
        /// workaround.)
        /// </summary>
        internal sealed class PreviousKeyEntry
        {
            [JsonPropertyName("keyId")]
            public string KeyId { get; init; } = string.Empty;

            [JsonPropertyName("key")]
            public string Key { get; init; } = string.Empty;

            [JsonPropertyName("retired")]
            public string Retired { get; init; } = string.Empty;
        }

        /// <summary>
        /// Root key-ring document stored in Key Vault.
        ///
        /// Version 1: primary key only (PreviousKeys absent / null).
        /// Version 2: primary key + optional PreviousKeys array for rotation support.
        /// Both versions are fully supported — PreviousKeys defaults to null when absent.
        ///
        /// Declared as a class with init-only properties rather than a positional record
        /// for clarity: the optional nested <c>PreviousKeys</c> collection reads more
        /// explicitly as a nullable property than as an optional constructor parameter.
        /// (Positional records DO deserialize correctly under
        /// <see cref="JsonSerializerDefaults.Web"/>; this is a readability choice, not a
        /// workaround.)
        /// </summary>
        internal sealed class KeyRingMaterial
        {
            [JsonPropertyName("version")]
            public int Version { get; init; }

            [JsonPropertyName("keyId")]
            public string KeyId { get; init; } = string.Empty;

            [JsonPropertyName("key")]
            public string Key { get; init; } = string.Empty;

            [JsonPropertyName("created")]
            public string Created { get; init; } = string.Empty;

            [JsonPropertyName("previousKeys")]
            public List<PreviousKeyEntry>? PreviousKeys { get; init; }
        }
    }
}
