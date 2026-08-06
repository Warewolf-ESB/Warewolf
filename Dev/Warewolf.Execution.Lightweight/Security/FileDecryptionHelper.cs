/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Decrypts AES-256-GCM-encrypted <c>ConnectionString</c> attribute values
    /// produced by <c>Encrypt-Config.ps1</c> and stored in <c>.bite</c> source files.
    ///
    /// Encrypted attribute format:
    ///   <c>WFAES::{Base64( [12-byte nonce][ciphertext][16-byte GCM tag] )}</c>
    ///
    /// Key Vault is never called from this class; all cryptography is purely
    /// in-memory using the key bytes cached at cold start by
    /// <see cref="KeyVaultSecretManager"/>.
    ///
    /// Decryption walks the full key ring from <see cref="KeyVaultSecretManager.GetAllKeyBytes"/>.
    /// The primary key is always tried first (zero overhead in steady state); previous keys
    /// are only attempted on GCM tag mismatch, in most-recently-retired-first order as
    /// returned by the key ring. A warning is logged whenever a fallback key is used so
    /// operators can track rotation progress.
    ///
    /// The static method <see cref="IsAesEncrypted"/> is registered into
    /// <see cref="Warewolf.Security.Encryption.DpapiWrapper.AesDecryptHook"/>
    /// at startup so the existing <c>DbSource</c> loading path transparently
    /// decrypts AES-encrypted values without any changes to those classes.
    /// </summary>
    public sealed class FileDecryptionHelper
    {
        /// <summary>
        /// Prefix that uniquely identifies a value encrypted by
        /// <c>Encrypt-Config.ps1</c>.  Not valid base64, so
        /// <see cref="Warewolf.Security.Encryption.DpapiWrapper.CanBeDecrypted"/>
        /// will pass over it unless the AES hook is registered.
        /// </summary>
        public const string WfAesPrefix = "WFAES::";

        const int NonceSize = 12;   // AES-GCM standard nonce size
        const int TagSize   = 16;   // AES-GCM standard tag size

        readonly IReadOnlyList<(string KeyId, byte[] KeyBytes)> _keyRing;
        readonly ILogger<FileDecryptionHelper>                   _logger;

        /// <param name="secretManager">
        /// Must already be initialised (<see cref="KeyVaultSecretManager.InitializeAsync"/>
        /// called) before this constructor runs.
        /// </param>
        /// <param name="logger">Logger for fallback-key warnings during a rotation window.</param>
        public FileDecryptionHelper(KeyVaultSecretManager secretManager, ILogger<FileDecryptionHelper> logger)
        {
            const string executionId = "FileDecryptionHelper-Constructor";

            if (secretManager is null) throw new ArgumentNullException(nameof(secretManager));
            _logger  = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyRing = secretManager.GetAllKeyBytes();

            var keyIds = string.Join(", ", _keyRing.Select(k => $"'{k.KeyId}'"));
            Dev2Logger.Info($"FileDecryptionHelper initialised with {_keyRing.Count} key(s) in ring: [{keyIds}].", executionId);
            _logger.LogInformation(
                "Decryption | Initialised with {KeyCount} key(s) in ring: [{KeyIds}]. " +
                "{FallbackNote}",
                _keyRing.Count,
                keyIds,
                _keyRing.Count > 1
                    ? "Key rotation fallback is active — previous key(s) will be tried on GCM mismatch."
                    : "Single-key mode — no previous keys loaded.");
        }

        /// <summary>
        /// Returns <c>true</c> when <paramref name="value"/> carries the
        /// <c>WFAES::</c> prefix written by <c>Encrypt-Config.ps1</c>.
        /// </summary>
        public static bool IsAesEncrypted(string? value)
            => !string.IsNullOrEmpty(value)
            && value.StartsWith(WfAesPrefix, StringComparison.Ordinal);

        /// <summary>
        /// Decrypts a <c>WFAES::</c>-prefixed connection-string value.
        /// Returns the input unchanged when it is not AES-encrypted, allowing
        /// the caller to pass any attribute value without conditional checks.
        ///
        /// Tries all keys in the ring if the primary key fails GCM verification,
        /// logging a warning whenever a fallback key succeeds.
        ///
        /// This method is wired as <see cref="Warewolf.Security.Encryption.DpapiWrapper.AesDecryptHook"/>
        /// in <c>Program.cs</c>, so it is called automatically whenever
        /// <c>DbSource</c> (or any other class) invokes
        /// <c>DpapiWrapper.CanBeDecrypted / Decrypt</c> on an AES-encrypted value.
        ///
        /// Decrypted bytes are never written to disk.
        /// </summary>
        /// <exception cref="CryptographicException">
        /// Thrown when no key in the ring produces a valid GCM tag (data tampered
        /// or key ring is incomplete).
        /// </exception>
        public string DecryptConnectionString(string encryptedValue)
        {
            const string executionId = "FileDecryptionHelper-Decrypt";

            if (!IsAesEncrypted(encryptedValue))
                return encryptedValue;

            ReadOnlySpan<byte> data =
                Convert.FromBase64String(encryptedValue[WfAesPrefix.Length..]);

            if (data.Length < NonceSize + TagSize)
                throw new CryptographicException(
                    $"WFAES payload too short ({data.Length} bytes). " +
                    "The .bite file may be corrupted. Re-run Encrypt-Config.ps1.");

            var nonce      = data[..NonceSize];
            var tag        = data[^TagSize..];
            var ciphertext = data[NonceSize..^TagSize];
            var plaintext  = new byte[ciphertext.Length];

            CryptographicException? lastException = null;
            var isPrimary = true;
            var attempted = 0;

            foreach (var (keyId, keyBytes) in _keyRing)
            {
                try
                {
                    if (!isPrimary)
                    {
                        // NOTE: with more than two keys in the ring this is not necessarily
                        // the primary that failed — report how many have been tried instead
                        // of blaming the primary on every fallback attempt.
                        Dev2Logger.Info($"FileDecryptionHelper no match after {attempted} key(s) — attempting fallback key '{keyId}'.", executionId);
                        _logger.LogInformation(
                            "Decryption | No match after {AttemptedCount} key(s). Attempting fallback key '{FallbackKeyId}'.",
                            attempted, keyId);
                    }

                    using var aes = new AesGcm(keyBytes, TagSize);
                    aes.Decrypt(nonce, ciphertext, tag, plaintext);

                    if (isPrimary)
                    {
                        Dev2Logger.Debug($"FileDecryptionHelper decrypted successfully using primary key '{keyId}'.", executionId);
                    }
                    else
                    {
                        Dev2Logger.Warn($"FileDecryptionHelper decrypted using fallback key '{keyId}'. Resource should be re-encrypted with the current key once rotation is complete.", executionId);
                        _logger.LogWarning(
                            "Decryption | Succeeded using fallback key '{FallbackKeyId}'. " +
                            "This resource is still encrypted with a retired key — re-encrypt with the current key once rotation is complete.",
                            keyId);
                    }

                    return Encoding.UTF8.GetString(plaintext);
                }
                catch (CryptographicException ex)
                {
                    lastException = ex;
                    attempted++;
                    Dev2Logger.Debug($"FileDecryptionHelper key '{keyId}' did not match — trying next key in ring.", executionId);
                    isPrimary = false;
                }
            }

            // All keys exhausted
            var triedKeyIds = string.Join(", ", _keyRing.Select(k => $"'{k.KeyId}'"));
            Dev2Logger.Error($"FileDecryptionHelper could not decrypt value — all keys tried: [{triedKeyIds}].", lastException!, executionId);
            throw new CryptographicException(
                $"AES-GCM decryption failed for all {_keyRing.Count} key(s) in the ring " +
                $"[{triedKeyIds}]. The .bite file may be corrupted or the key ring is " +
                "missing the key that encrypted this value.",
                lastException);
        }
    }
}
