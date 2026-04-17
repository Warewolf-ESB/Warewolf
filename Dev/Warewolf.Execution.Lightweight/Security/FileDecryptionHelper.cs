/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
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

        readonly byte[] _keyBytes;

        /// <param name="secretManager">
        /// Must already be initialised (<see cref="KeyVaultSecretManager.InitializeAsync"/>
        /// called) before this constructor runs.
        /// </param>
        public FileDecryptionHelper(KeyVaultSecretManager secretManager)
        {
            if (secretManager is null) throw new ArgumentNullException(nameof(secretManager));
            _keyBytes = secretManager.GetKeyBytes();
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
        /// This method is wired as <see cref="Warewolf.Security.Encryption.DpapiWrapper.AesDecryptHook"/>
        /// in <c>Program.cs</c>, so it is called automatically whenever
        /// <c>DbSource</c> (or any other class) invokes
        /// <c>DpapiWrapper.CanBeDecrypted / Decrypt</c> on an AES-encrypted value.
        ///
        /// Decrypted bytes are never written to disk.
        /// </summary>
        /// <exception cref="CryptographicException">
        /// Thrown when the GCM tag does not match (data tampered or wrong key).
        /// </exception>
        public string DecryptConnectionString(string encryptedValue)
        {
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

            var plaintext = new byte[ciphertext.Length];

            using var aes = new AesGcm(_keyBytes, TagSize);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
    }
}
