/*
 * Tests for FileEncryptionHelper — the AES-256-GCM encryption counterpart of
 * FileDecryptionHelper, registered as DpapiWrapper.AesEncryptHook at cold start
 * so suspend/resume persistence produces Key Vault-backed WFAES:: ciphertext
 * instead of machine-bound Windows DPAPI.
 *
 * Pins:
 *   - ctor guard
 *   - WFAES:: output format decryptable by FileDecryptionHelper (same key)
 *   - fresh nonce per call (same plaintext → different ciphertext)
 *   - idempotency on already-encrypted input
 *   - tamper detection (GCM tag) on the decrypt side
 *   - DpapiWrapper routing with both hooks registered (Encrypt / Decrypt /
 *     CanBeDecrypted / EncryptIfDecrypted / DecryptIfEncrypted)
 */

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using Warewolf.Execution.Lightweight.Security;
using Warewolf.Security.Encryption;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    [TestClass]
    public class FileEncryptionHelperTests
    {
        static readonly byte[] _key32 = Enumerable.Range(0, 32).Select(i => (byte)(255 - i)).ToArray();
        static readonly string _keyBase64 = Convert.ToBase64String(_key32);

        static KeyVaultSecretManager NewInitialisedManager()
        {
            var json =
                $$"""{"version":1,"keyId":"unit-test-encrypt","key":"{{_keyBase64}}","created":"2026-01-01"}""";
            var mgr = new KeyVaultSecretManager(
                vaultUri:    "https://example.vault.azure.net/",
                secretName:  "warewolf-aes-key",
                credential:  null,
                logger:      NullLogger<KeyVaultSecretManager>.Instance,
                debugSecret: json);
            mgr.InitializeAsync().GetAwaiter().GetResult();
            return mgr;
        }

        static (FileEncryptionHelper Encryptor, FileDecryptionHelper Decryptor) NewHelperPair()
        {
            var mgr = NewInitialisedManager();
            return (new FileEncryptionHelper(mgr),
                    new FileDecryptionHelper(mgr, NullLogger<FileDecryptionHelper>.Instance));
        }

        static KeyVaultSecretManager NewManagerWithKey(byte[] key)
        {
            var json = $$"""{"version":1,"keyId":"unit-test-alt","key":"{{Convert.ToBase64String(key)}}","created":"2026-01-01"}""";
            var mgr = new KeyVaultSecretManager(
                "https://example.vault.azure.net/", "warewolf-aes-key", null,
                NullLogger<KeyVaultSecretManager>.Instance, json);
            mgr.InitializeAsync().GetAwaiter().GetResult();
            return mgr;
        }

        // ── Constructor guard ─────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Constructor_NullSecretManager_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new FileEncryptionHelper(null!));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Encrypt_NullPlaintext_Throws()
        {
            var (encryptor, _) = NewHelperPair();
            Assert.ThrowsException<ArgumentNullException>(() => encryptor.Encrypt(null!));
        }

        // ── Format + round-trip ───────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Encrypt_ProducesWfAesValue_DecryptableByFileDecryptionHelper()
        {
            var (encryptor, decryptor) = NewHelperPair();
            const string plaintext = "Data Source=sql.example.com;Initial Catalog=Hangfire;User ID=u;Password=p##$$";

            var encrypted = encryptor.Encrypt(plaintext);

            Assert.IsTrue(FileDecryptionHelper.IsAesEncrypted(encrypted), "Output must carry the WFAES:: prefix.");
            Assert.AreNotEqual(plaintext, encrypted);
            Assert.AreEqual(plaintext, decryptor.DecryptConnectionString(encrypted),
                "A value encrypted by FileEncryptionHelper must decrypt through the standard decrypt path.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Encrypt_RoundTripsUnicodeAndEmptyValues()
        {
            var (encryptor, decryptor) = NewHelperPair();

            foreach (var plaintext in new[] { "", "простой текст ✓ 日本語 { \"json\": true }", new string('x', 64 * 1024) })
            {
                var roundTripped = decryptor.DecryptConnectionString(encryptor.Encrypt(plaintext));
                Assert.AreEqual(plaintext, roundTripped);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Encrypt_SamePlaintextTwice_ProducesDifferentCiphertexts()
        {
            var (encryptor, _) = NewHelperPair();
            const string plaintext = "nonce-uniqueness";

            Assert.AreNotEqual(encryptor.Encrypt(plaintext), encryptor.Encrypt(plaintext),
                "GCM requires a fresh nonce per encryption — identical outputs would mean nonce reuse.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Encrypt_AlreadyEncryptedValue_ReturnedUnchanged()
        {
            var (encryptor, _) = NewHelperPair();

            var encrypted = encryptor.Encrypt("idempotency");
            Assert.AreSame(encrypted, encryptor.Encrypt(encrypted),
                "Encrypting a WFAES:: value must be a pass-through (idempotent).");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Encrypt_TamperedPayload_FailsDecryptionWithCryptographicException()
        {
            var (encryptor, decryptor) = NewHelperPair();

            var encrypted = encryptor.Encrypt("tamper-me");
            var payload = Convert.FromBase64String(encrypted[FileDecryptionHelper.WfAesPrefix.Length..]);
            payload[payload.Length / 2] ^= 0xFF; // flip bits mid-payload
            var tampered = FileDecryptionHelper.WfAesPrefix + Convert.ToBase64String(payload);

            var ex = Assert.ThrowsException<CryptographicException>(
                () => decryptor.DecryptConnectionString(tampered),
                "The GCM tag must reject any modified payload.");
            Assert.IsInstanceOfType<AuthenticationTagMismatchException>(ex.InnerException,
                "The wrapped inner exception must be the GCM tag-mismatch failure.");
        }

        // ── External interop + rotated/cross-host key semantics ────────────────────

        // A known-answer vector produced OUTSIDE this assembly using the SAME AES-256-GCM
        // WFAES layout Encrypt-Config.ps1 emits ([12 nonce][ciphertext][16 tag],
        // AesGcm(key,16), WFAES:: + base64), encrypted with the test key (_key32).
        // Pinning it proves the decrypt path is byte-compatible with the PowerShell
        // encryptor — reordering the nonce/tag would break THIS even though the C#
        // self-round-trip still passed.
        const string InteropVector =
            "WFAES::f2Fy5PSg45L314+5Ezbh/jMF9eC37rv4lyO370Mwtkhg1jNU0yqa4bLS752sia/0HCww073rOuxwf4VPR6YIluNkRraNG/0N4Pwwnb9G5pzUXC3BJ2y3RlC3BQBhce8pYiZ6UCzWNB2v+A==";
        const string InteropPlaintext =
            "Data Source=interop.example;Initial Catalog=Hangfire;User ID=u;Password=p@ss##";

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Decrypt_ExternalWfAesVector_MatchesEncryptConfigPs1Format()
        {
            var (_, decryptor) = NewHelperPair();

            Assert.AreEqual(InteropPlaintext, decryptor.DecryptConnectionString(InteropVector),
                "A WFAES:: value produced by the Encrypt-Config.ps1 byte layout (external producer) must decrypt unchanged.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Decrypt_WithRotatedKey_FailsCleanly_NeverSilentlyWrong()
        {
            // Encrypt with one key, decrypt with a DIFFERENT key — the exact "Key Vault key
            // was rotated / cross-host key mismatch" scenario. GCM must fail with a clean,
            // explicit exception, NEVER return wrong plaintext. This is why rotating the AES
            // key invalidates already-suspended jobs (see KeyRotationRunbook.md).
            var (encryptor, _) = NewHelperPair();                                   // encrypts with _key32
            var encrypted = encryptor.Encrypt("secret-under-old-key");

            var otherKey = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();   // a different key
            var otherDecryptor = new FileDecryptionHelper(
                NewManagerWithKey(otherKey), NullLogger<FileDecryptionHelper>.Instance);

            var ex = Assert.ThrowsException<CryptographicException>(
                () => otherDecryptor.DecryptConnectionString(encrypted),
                "A rotated/mismatched key must fail with a clean GCM tag error — old-key data is unrecoverable, never silently mis-decrypted.");
            Assert.IsInstanceOfType<AuthenticationTagMismatchException>(ex.InnerException,
                "The wrapped inner exception must be the GCM tag-mismatch failure.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Decrypt_NonWfAesValue_PassesThroughUnchanged()
        {
            // A Windows-DPAPI (or any non-WFAES) value has no WFAES:: prefix, so the AES
            // decrypt path returns it verbatim — never mis-parsed as AES. The genuine
            // cross-host DPAPI failure surfaces later from the DPAPI layer itself
            // (environment-specific: DPAPI cannot unprotect a foreign machine's blob on Azure).
            var (_, decryptor) = NewHelperPair();
            const string dpapiStyle = "AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAA"; // base64, no WFAES:: prefix

            Assert.IsFalse(FileDecryptionHelper.IsAesEncrypted(dpapiStyle));
            Assert.AreEqual(dpapiStyle, decryptor.DecryptConnectionString(dpapiStyle),
                "A non-WFAES value must pass through the AES decrypt hook untouched (no silent AES mis-decrypt of a DPAPI value).");
        }
    }

    /// <summary>
    /// DpapiWrapper static-hook routing with BOTH hooks registered — mirrors the engine's
    /// cold-start wiring in KeyVaultStartupExtensions. Serialised because the hooks are
    /// process-global state.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class DpapiWrapperAesHookRoutingTests
    {
        Func<string, string>? _savedEncryptHook;
        Func<string, string>? _savedDecryptHook;

        [TestInitialize]
        public void SaveHooks()
        {
            _savedEncryptHook = DpapiWrapper.AesEncryptHook;
            _savedDecryptHook = DpapiWrapper.AesDecryptHook;
        }

        [TestCleanup]
        public void RestoreHooks()
        {
            DpapiWrapper.AesEncryptHook = _savedEncryptHook;
            DpapiWrapper.AesDecryptHook = _savedDecryptHook;
        }

        static void WireHooksLikeEngineStartup()
        {
            var json = $$"""{"version":1,"keyId":"hook-test","key":"{{Convert.ToBase64String(Enumerable.Range(0, 32).Select(i => (byte)i).ToArray())}}","created":"2026-01-01"}""";
            var mgr = new KeyVaultSecretManager(
                "https://example.vault.azure.net/", "warewolf-aes-key", null,
                NullLogger<KeyVaultSecretManager>.Instance, json);
            mgr.InitializeAsync().GetAwaiter().GetResult();

            DpapiWrapper.AesDecryptHook =
                new FileDecryptionHelper(mgr, NullLogger<FileDecryptionHelper>.Instance).DecryptConnectionString;
            DpapiWrapper.AesEncryptHook = new FileEncryptionHelper(mgr).Encrypt;
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DpapiWrapper_WithBothHooks_EncryptDecryptRoundTripsAsWfAes()
        {
            WireHooksLikeEngineStartup();
            const string plaintext = "suspended-environment-json";

            var encrypted = DpapiWrapper.Encrypt(plaintext);

            Assert.IsTrue(encrypted.StartsWith(FileDecryptionHelper.WfAesPrefix, StringComparison.Ordinal),
                "With the encrypt hook registered, DpapiWrapper.Encrypt must produce WFAES:: (not DPAPI base64).");
            Assert.IsTrue(encrypted.CanBeDecrypted());
            Assert.AreEqual(plaintext, DpapiWrapper.Decrypt(encrypted));
            Assert.AreEqual(plaintext, DpapiWrapper.DecryptIfEncrypted(encrypted));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DpapiWrapper_EncryptIfDecrypted_PassesThroughWfAesValues()
        {
            WireHooksLikeEngineStartup();

            var encrypted = DpapiWrapper.Encrypt("once-only");

            Assert.AreSame(encrypted, DpapiWrapper.EncryptIfDecrypted(encrypted),
                "EncryptIfDecrypted must not double-encrypt a WFAES:: value.");
        }
    }
}
