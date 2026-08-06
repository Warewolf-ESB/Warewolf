/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Dev2.Common;
using Microsoft.Extensions.Logging;
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

            Dev2Logger.Debug($"KeyVaultSecretManager constructor. VaultUri: {vaultUri}, SecretName: {secretName}, HasDebugSecret: {debugSecret != null}, HasCredential: {credential != null}", executionId);

            if (debugSecret is null)
            {
                _credential = credential ?? throw new ArgumentNullException(nameof(credential));
                _client     = new SecretClient(new Uri(_vaultUri), _credential);
                Dev2Logger.Info($"KeyVaultSecretManager initialized with SecretClient. VaultUri: {_vaultUri}, CredentialType: {_credential.GetType().Name}", executionId);
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

            Dev2Logger.Info($"KeyVaultSecretManager InitializeAsync starting. SecretName: {_secretName}, IsDebugMode: {_debugSecret != null}", executionId);

            try
            {
                if (_debugSecret is not null)
                {
                    Dev2Logger.Warn("KeyVaultSecretManager using DEBUG_AZURE_KEYVAULT_SECRET (Key Vault skipped)", executionId);

                    _logger.LogInformation(
                        "KeyVault | Development mode — using DEBUG_AZURE_KEYVAULT_SECRET (Key Vault skipped).");
                    ParseAndSetMaterial(_debugSecret);

                    Dev2Logger.Info($"KeyVaultSecretManager InitializeAsync completed (DEBUG mode). KeyId: {KeyId}", executionId);
                    return;
                }

                Dev2Logger.Debug($"KeyVaultSecretManager fetching secret '{_secretName}' from '{_vaultUri}'. CredentialType: {_credential!.GetType().Name}", executionId);

                _logger.LogInformation(
                    "KeyVault | Credential={CredentialType} | Fetching secret '{SecretName}' from '{VaultUri}'",
                    _credential!.GetType().Name, _secretName, _vaultUri);

                KeyVaultSecret secret =
                    await _client!.GetSecretAsync(_secretName, version: null, cancellationToken)
                                 .ConfigureAwait(false);

                Dev2Logger.Debug($"KeyVaultSecretManager successfully fetched secret '{_secretName}' from Key Vault", executionId);

                ParseAndSetMaterial(secret.Value);

                Dev2Logger.Info($"KeyVaultSecretManager InitializeAsync completed successfully. KeyId: {KeyId}, SecretName: {_secretName}", executionId);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"KeyVaultSecretManager InitializeAsync failed. SecretName: {_secretName}, VaultUri: {_vaultUri}", ex, executionId);
                throw;
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        void ParseAndSetMaterial(string rawJson)
        {
            const string executionId = "KeyVaultSecretManager-Parse";

            Dev2Logger.Debug($"KeyVaultSecretManager ParseAndSetMaterial starting. JsonLength: {rawJson?.Length ?? 0}", executionId);

            // Repair legacy unquoted-key format written by old versions of Encrypt-Config.ps1
            // e.g. {version:1,keyId:abc,...} → {"version":1,"keyId":"abc",...}
            if (!rawJson.TrimStart().StartsWith("{\""))
            {
                Dev2Logger.Warn($"KeyVaultSecretManager secret '{_secretName}' contained unquoted JSON - auto-repairing", executionId);

                rawJson = Regex.Replace(rawJson, @"([\{,])\s*([a-zA-Z_]\w*)\s*:", "$1\"$2\":");
                rawJson = Regex.Replace(rawJson, @":\s*(?!"")([^,\}]+)", ":\"$1\"");
                _logger.LogWarning(
                    "KeyVault | Secret '{SecretName}' contained unquoted JSON — auto-repaired. " +
                    "Re-run Encrypt-Config.ps1 -GenerateKeys to store a canonical version.",
                    _secretName);
            }

            try
            {
                _material = JsonSerializer.Deserialize<KeyRingMaterial>(rawJson, _jsonOptions)
                            ?? throw new InvalidOperationException(
                                $"Failed to deserialise key material from secret '{_secretName}'.");

                if (string.IsNullOrWhiteSpace(_material.Key))
                {
                    Dev2Logger.Error($"KeyVaultSecretManager secret '{_secretName}' contains empty key material", executionId);
                    throw new InvalidOperationException(
                        "Key Vault secret contains empty key material.");
                }

                var previousKeyCount = _material.PreviousKeys?.Count ?? 0;
                Dev2Logger.Info($"KeyVaultSecretManager key material parsed successfully. KeyId: {_material.KeyId}, Created: {_material.Created}, PreviousKeyCount: {previousKeyCount}", executionId);

                if (previousKeyCount > 0)
                {
                    var retiredIds = string.Join(", ", _material.PreviousKeys!.Select(p => $"'{p.KeyId}' (retired {p.Retired})"));
                    Dev2Logger.Info($"KeyVaultSecretManager secret contains {previousKeyCount} previous key(s) — key rotation fallback will be active: [{retiredIds}].", executionId);
                    _logger.LogInformation(
                        "KeyVault | Key loaded. KeyId={KeyId} Created={Created} | Key rotation fallback active — {PreviousKeyCount} previous key(s) found: [{RetiredIds}]. " +
                        "Resources encrypted with retired keys will be decrypted transparently.",
                        _material.KeyId, _material.Created, previousKeyCount, retiredIds);
                }
                else
                {
                    _logger.LogInformation(
                        "KeyVault | Key loaded. KeyId={KeyId} Created={Created} | No previous keys in secret — single-key mode.",
                        _material.KeyId, _material.Created);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"KeyVaultSecretManager ParseAndSetMaterial failed for secret '{_secretName}'", ex, executionId);
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
                    Dev2Logger.Error($"KeyVaultSecretManager invalid key length: {keyBytes.Length} bytes (expected 32)", executionId);
                    throw new InvalidOperationException(
                        $"AES-256 key must be 32 bytes; got {keyBytes.Length}. " +
                        "Re-run Encrypt-Config.ps1 to regenerate the key material.");
                }

                Dev2Logger.Debug($"KeyVaultSecretManager GetKeyBytes successful. KeyId: {_material.KeyId}, KeyLength: {keyBytes.Length} bytes", executionId);
                return keyBytes;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                Dev2Logger.Error($"KeyVaultSecretManager GetKeyBytes failed. KeyId: {_material.KeyId}", ex, executionId);
                throw;
            }
        }

        /// <summary>
        /// Returns all available decryption keys: the primary key first, followed by
        /// any previous keys in reverse-chronological order (most recently retired first).
        ///
        /// When no <c>PreviousKeys</c> are present in the secret (Version 1 format),
        /// only the primary key is returned — fully backward compatible.
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
                foreach (var prev in _material.PreviousKeys)
                {
                    var prevBytes = Convert.FromBase64String(prev.Key);
                    if (prevBytes.Length != 32)
                    {
                        Dev2Logger.Warn($"Previous key '{prev.KeyId}' has invalid length {prevBytes.Length} bytes — skipping.", executionId);
                        continue;
                    }
                    keys.Add((prev.KeyId, prevBytes));
                }
                Dev2Logger.Info($"Key ring loaded — {keys.Count} key(s) available for decryption. PreviousKeyIds: [{string.Join(", ", _material.PreviousKeys.Select(p => p.KeyId))}]", executionId);
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
        /// Intentionally a class with init properties (not a positional record) so that
        /// System.Text.Json can deserialize it property-by-property without requiring
        /// constructor-parameter matching, which fails for nested types inside a List.
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
        /// Intentionally a class with init properties (not a positional record) so that
        /// System.Text.Json can deserialize all fields — including the nested PreviousKeys
        /// list — property-by-property without constructor-parameter matching, which is
        /// unreliable for optional parameters and nested collection types.
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
