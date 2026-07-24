/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Security.Cryptography;
using System.Text;

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Encrypts values with AES-256-GCM into the <c>WFAES::</c> format produced by
    /// <c>Encrypt-Config.ps1</c> and consumed by <see cref="FileDecryptionHelper"/>:
    ///
    ///   <c>WFAES::{Base64( [12-byte nonce][ciphertext][16-byte GCM tag] )}</c>
    ///
    /// This is the encryption counterpart of <see cref="FileDecryptionHelper"/> — the
    /// nonce/tag layout and UTF-8 plaintext encoding are byte-for-byte compatible, so a
    /// value encrypted here decrypts through the same
    /// <see cref="Warewolf.Security.Encryption.DpapiWrapper.AesDecryptHook"/> path (and
    /// vice-versa for values encrypted by the deployment scripts).
    ///
    /// Registered as <see cref="Warewolf.Security.Encryption.DpapiWrapper.AesEncryptHook"/>
    /// at startup so every <c>DpapiWrapper.Encrypt</c> call in this host — e.g.
    /// <c>SuspendExecutionActivity</c> persisting a workflow environment, or
    /// <c>HangfireScheduler</c> writing a <c>ManuallyResumedState</c> — transparently
    /// produces Key Vault-backed AES ciphertext instead of machine-bound Windows DPAPI.
    ///
    /// Key Vault is never called from this class; all cryptography is purely in-memory
    /// using the key bytes cached at cold start by <see cref="KeyVaultSecretManager"/>.
    /// A fresh random 96-bit nonce is generated per encryption (never reused for a key),
    /// as required by GCM.
    /// </summary>
    public sealed class FileEncryptionHelper
    {
        const int NonceSize = 12;   // AES-GCM standard nonce size — must match FileDecryptionHelper
        const int TagSize   = 16;   // AES-GCM standard tag size   — must match FileDecryptionHelper

        readonly byte[] _keyBytes;

        /// <param name="secretManager">
        /// Must already be initialised (<see cref="KeyVaultSecretManager.InitializeAsync"/>
        /// called) before this constructor runs.
        /// </param>
        public FileEncryptionHelper(KeyVaultSecretManager secretManager)
        {
            if (secretManager is null) throw new ArgumentNullException(nameof(secretManager));
            _keyBytes = secretManager.GetKeyBytes();
        }

        /// <summary>
        /// Encrypts <paramref name="plainText"/> into a <c>WFAES::</c>-prefixed value.
        /// An already-encrypted (<c>WFAES::</c>-prefixed) input is returned unchanged so
        /// the method is idempotent — callers can pass any value without conditional checks.
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="plainText"/> is null.</exception>
        public string Encrypt(string plainText)
        {
            if (plainText is null) throw new ArgumentNullException(nameof(plainText));

            if (FileDecryptionHelper.IsAesEncrypted(plainText))
                return plainText;

            var plaintextBytes = Encoding.UTF8.GetBytes(plainText);

            var payload = new byte[NonceSize + plaintextBytes.Length + TagSize];
            var nonce      = payload.AsSpan(0, NonceSize);
            var ciphertext = payload.AsSpan(NonceSize, plaintextBytes.Length);
            var tag        = payload.AsSpan(NonceSize + plaintextBytes.Length, TagSize);

            RandomNumberGenerator.Fill(nonce);

            using var aes = new AesGcm(_keyBytes, TagSize);
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            return FileDecryptionHelper.WfAesPrefix + Convert.ToBase64String(payload);
        }
    }
}
