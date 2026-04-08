/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
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
    /// Authentication uses <see cref="DefaultAzureCredential"/>:
    ///   • Azure cloud   → System-Assigned Managed Identity (zero credential config)
    ///   • Local dev     → Azure CLI / Visual Studio / VS Code login
    /// </summary>
    public sealed class KeyVaultSecretManager
    {
        static readonly JsonSerializerOptions _jsonOptions =
            new(JsonSerializerDefaults.Web);

        readonly string                        _vaultUri;
        readonly string                        _secretName;
        readonly ILogger<KeyVaultSecretManager> _logger;

        KeyRingMaterial? _material;

        public KeyVaultSecretManager(
            string                         vaultUri,
            string                         secretName,
            ILogger<KeyVaultSecretManager> logger)
        {
            _vaultUri   = vaultUri   ?? throw new ArgumentNullException(nameof(vaultUri));
            _secretName = secretName ?? throw new ArgumentNullException(nameof(secretName));
            _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Key ID from the fetched secret (used in audit log; never the actual key).
        /// </summary>
        public string KeyId => _material?.KeyId ?? "(not initialised)";

        /// <summary>
        /// Fetches the secret from Key Vault and deserialises the key ring material.
        /// Must be called once at startup before any call to <see cref="GetKeyBytes"/>.
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "KeyVault | Fetching secret '{SecretName}' from '{VaultUri}'",
                _secretName, _vaultUri);

            var credential = new DefaultAzureCredential();
            var client = new SecretClient(new Uri(_vaultUri), credential);

            KeyVaultSecret secret =
                await client.GetSecretAsync(_secretName, version: null, cancellationToken)
                            .ConfigureAwait(false);
            var rawJson = secret.Value;

            // Repair legacy unquoted-key format written by old versions of Encrypt-Config.ps1
            // e.g. {version:1,keyId:abc,...} → {"version":1,"keyId":"abc",...}
            if (!rawJson.TrimStart().StartsWith("{\""))
            {
                rawJson = Regex.Replace(rawJson, @"([\{,])\s*([a-zA-Z_]\w*)\s*:", "$1\"$2\":");
                rawJson = Regex.Replace(rawJson, @":\s*(?!"")([^,\}]+)", ":\"$1\"");
                _logger.LogWarning(
                    "KeyVault | Secret '{SecretName}' contained unquoted JSON — auto-repaired. " +
                    "Re-run Encrypt-Config.ps1 -GenerateKeys to store a canonical version.",
                    _secretName);
            }

            _material = JsonSerializer.Deserialize<KeyRingMaterial>(rawJson, _jsonOptions)
                        ?? throw new InvalidOperationException(
                            $"Failed to deserialise key material from secret '{_secretName}'.");

            if (string.IsNullOrWhiteSpace(_material.Key))
                throw new InvalidOperationException(
                    "Key Vault secret contains empty key material.");

            _logger.LogInformation(
                "KeyVault | Key loaded. KeyId={KeyId} Created={Created}",
                _material.KeyId, _material.Created);
        }

        /// <summary>
        /// Returns the 32-byte (256-bit) AES key decoded from the secret.
        /// Throws <see cref="InvalidOperationException"/> when called before
        /// <see cref="InitializeAsync"/>.
        /// </summary>
        public byte[] GetKeyBytes()
        {
            if (_material is null)
                throw new InvalidOperationException(
                    $"{nameof(KeyVaultSecretManager)} is not initialised. " +
                    $"Call {nameof(InitializeAsync)} before resolving {nameof(FileDecryptionHelper)}.");

            var keyBytes = Convert.FromBase64String(_material.Key);

            if (keyBytes.Length != 32)
                throw new InvalidOperationException(
                    $"AES-256 key must be 32 bytes; got {keyBytes.Length}. " +
                    "Re-run Encrypt-Config.ps1 to regenerate the key material.");

            return keyBytes;
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
