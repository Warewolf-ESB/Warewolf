/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Dev2.Common;
using Microsoft.Extensions.Logging;
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
        readonly TokenCredential?                _credential;
        readonly SecretClient?                   _client;
        readonly ILogger<KeyVaultSecretManager>  _logger;
        readonly string?                         _debugSecret;

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
                if (_debugSecret is not null)
                {
                    Dev2Logger.Warn("KeyVaultSecretManager using DEBUG_AZURE_KEYVAULT_SECRET (Key Vault skipped)", executionId);

                    _logger.LogInformation(
                        "KeyVault | Development mode — Key Vault call skipped.");
                    ParseAndSetMaterial(_debugSecret);

                    Dev2Logger.Info("KeyVaultSecretManager InitializeAsync completed (DEBUG mode).", executionId);
                    return;
                }

                Dev2Logger.Debug($"KeyVaultSecretManager fetching secret '{_secretName}' from '{_vaultUri}'. CredentialType: {_credential!.GetType().Name}", executionId);

                // Secret name, vault URI and credential type are Debug-only (see the
                // Dev2Logger.Debug line above) — they must not reach production sinks.
                _logger.LogInformation("KeyVault | Fetching configuration secret from Key Vault.");

                KeyVaultSecret secret =
                    await _client!.GetSecretAsync(_secretName, version: null, cancellationToken)
                                 .ConfigureAwait(false);

                Dev2Logger.Debug($"KeyVaultSecretManager successfully fetched secret '{_secretName}' from Key Vault", executionId);

                ParseAndSetMaterial(secret.Value);

                Dev2Logger.Info("KeyVaultSecretManager InitializeAsync completed successfully.", executionId);
            }
            catch (Exception ex)
            {
                // Secret name, vault URI and the exception object are withheld from Error:
                // an Azure SDK exception message can name the vault, the secret and the
                // identity that was refused. Full detail stays at Debug.
                Dev2Logger.Error($"KeyVaultSecretManager InitializeAsync failed. ExceptionType={ex.GetType().Name}", executionId);
                Dev2Logger.Debug("KeyVaultSecretManager InitializeAsync failure details.", ex, executionId);
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
                Dev2Logger.Error($"KeyVaultSecretManager ParseAndSetMaterial failed. ExceptionType={ex.GetType().Name}", executionId);
                Dev2Logger.Debug("KeyVaultSecretManager ParseAndSetMaterial failure details.", ex, executionId);
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
                    // The actual length is withheld from Error (it characterises the key
                    // material); it remains available at Debug.
                    Dev2Logger.Error("KeyVaultSecretManager invalid key length (expected 32 bytes)", executionId);
                    Dev2Logger.Debug($"KeyVaultSecretManager invalid key length: {keyBytes.Length} bytes (expected 32)", executionId);
                    throw new InvalidOperationException(
                        $"AES-256 key must be 32 bytes; got {keyBytes.Length}. " +
                        "Re-run Encrypt-Config.ps1 to regenerate the key material.");
                }

                Dev2Logger.Debug($"KeyVaultSecretManager GetKeyBytes successful. KeyId: {_material.KeyId}, KeyLength: {keyBytes.Length} bytes", executionId);
                return keyBytes;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                Dev2Logger.Error($"KeyVaultSecretManager GetKeyBytes failed. ExceptionType={ex.GetType().Name}", executionId);
                Dev2Logger.Debug("KeyVaultSecretManager GetKeyBytes failure details.", ex, executionId);
                throw;
            }
        }

        // ── Key-material DTO (matches the JSON written by Encrypt-Config.ps1) ──

        internal sealed record KeyRingMaterial(
            [property: JsonPropertyName("version")] int    Version,
            [property: JsonPropertyName("keyId")]   string KeyId,
            [property: JsonPropertyName("key")]     string Key,
            [property: JsonPropertyName("created")] string Created
        );
    }
}
