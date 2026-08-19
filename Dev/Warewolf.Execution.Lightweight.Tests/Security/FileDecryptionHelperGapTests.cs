/*
 * Gap tests for FileDecryptionHelper.
 *
 * Only IsAesEncrypted ran under coverage previously (21% baseline). The
 * round-trip path through DecryptConnectionString is dormant in CI because
 * KeyVault is unavailable. We supply a KeyVaultSecretManager constructed via
 * its DEBUG path (debugSecret arg) so the helper can be exercised without any
 * Azure dependency.
 *
 * The tests pin:
 *   - happy-path round-trip (encrypt with the same key → decrypt back)
 *   - WFAES:: prefix detection edge cases
 *   - short / tampered / wrong-key payload all raise CryptographicException
 *   - non-AES-encrypted strings flow through unchanged
 */

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    [TestClass]
    public class FileDecryptionHelperGapTests
    {
        const int NonceSize = 12;
        const int TagSize   = 16;

        // 32-byte key, base64-encoded — fixed so we can deterministically
        // encrypt/decrypt in tests.
        static readonly byte[] _key32 = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();
        static readonly string _keyBase64 = Convert.ToBase64String(_key32);

        static KeyVaultSecretManager NewInitialisedManager()
        {
            var json =
                $$"""{"version":1,"keyId":"unit-test","key":"{{_keyBase64}}","created":"2026-01-01"}""";
            var mgr = new KeyVaultSecretManager(
                vaultUri:    "https://example.vault.azure.net/",
                secretName:  "warewolf-aes-key",
                credential:  null,
                logger:      NullLogger<KeyVaultSecretManager>.Instance,
                debugSecret: json);
            mgr.InitializeAsync().GetAwaiter().GetResult();
            return mgr;
        }

        static FileDecryptionHelper NewHelper() =>
            new(NewInitialisedManager(), NullLogger<FileDecryptionHelper>.Instance);

        // Mirror of the encryption side of Encrypt-Config.ps1: emit
        //   WFAES:: + base64( nonce || ciphertext || tag )
        static string EncryptValue(string plaintext, byte[] key)
        {
            var nonce      = new byte[NonceSize];
            var ciphertext = new byte[Encoding.UTF8.GetByteCount(plaintext)];
            var tag        = new byte[TagSize];
            RandomNumberGenerator.Fill(nonce);

            using var aes = new AesGcm(key, TagSize);
            aes.Encrypt(nonce, Encoding.UTF8.GetBytes(plaintext), ciphertext, tag);

            var combined = new byte[NonceSize + ciphertext.Length + TagSize];
            Buffer.BlockCopy(nonce,      0, combined, 0,                              NonceSize);
            Buffer.BlockCopy(ciphertext, 0, combined, NonceSize,                      ciphertext.Length);
            Buffer.BlockCopy(tag,        0, combined, NonceSize + ciphertext.Length,  TagSize);

            return FileDecryptionHelper.WfAesPrefix + Convert.ToBase64String(combined);
        }

        // ── Constructor guards ────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Constructor_NullSecretManager_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new FileDecryptionHelper(null!, NullLogger<FileDecryptionHelper>.Instance));
        }

        // ── IsAesEncrypted (was the only covered line) ───────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsAesEncrypted_DetectsPrefixedValues()
        {
            Assert.IsTrue (FileDecryptionHelper.IsAesEncrypted("WFAES::abc"));
            Assert.IsFalse(FileDecryptionHelper.IsAesEncrypted("plain"));
            Assert.IsFalse(FileDecryptionHelper.IsAesEncrypted(""));
            Assert.IsFalse(FileDecryptionHelper.IsAesEncrypted(null));
            Assert.IsFalse(FileDecryptionHelper.IsAesEncrypted("wfaes::lower"),
                "Prefix detection is intentionally case-sensitive (Ordinal).");
        }

        // ── DecryptConnectionString round-trips ──────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DecryptConnectionString_RoundTripsKnownPlaintext()
        {
            var helper    = NewHelper();
            var encrypted = EncryptValue("Server=.;Database=Test;User Id=sa;Password=x", _key32);

            var actual = helper.DecryptConnectionString(encrypted);

            Assert.AreEqual("Server=.;Database=Test;User Id=sa;Password=x", actual);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DecryptConnectionString_NonEncryptedValue_PassesThroughUnchanged()
        {
            var helper = NewHelper();

            Assert.AreEqual("not-encrypted", helper.DecryptConnectionString("not-encrypted"));
            Assert.AreEqual("",              helper.DecryptConnectionString(""));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DecryptConnectionString_TooShortPayload_ThrowsCryptographicException()
        {
            var helper = NewHelper();

            // 8 bytes of payload
            var tooShort = FileDecryptionHelper.WfAesPrefix +
                           Convert.ToBase64String(new byte[8]);

            var ex = Assert.ThrowsException<CryptographicException>(
                () => helper.DecryptConnectionString(tooShort));
            StringAssert.Contains(ex.Message, "too short");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DecryptConnectionString_TamperedTag_ThrowsCryptographicException()
        {
            var helper    = NewHelper();
            var encrypted = EncryptValue("secret", _key32);

            // Flip the last byte of the base64 payload — that decodes to a tag-bit
            // flip and AES-GCM verification must reject it.
            var bytes = Convert.FromBase64String(encrypted[FileDecryptionHelper.WfAesPrefix.Length..]);
            bytes[^1] ^= 0x01;
            var tampered = FileDecryptionHelper.WfAesPrefix + Convert.ToBase64String(bytes);

            var ex = Assert.ThrowsException<CryptographicException>(
                () => helper.DecryptConnectionString(tampered));
            Assert.IsInstanceOfType<AuthenticationTagMismatchException>(ex.InnerException,
                "The wrapped inner exception must be the GCM tag-mismatch failure.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DecryptConnectionString_WrongKey_ThrowsCryptographicException()
        {
            // Encrypt with a DIFFERENT key, then attempt to decrypt with the
            // canonical one — GCM authentication tag must reject this.
            var otherKey  = Enumerable.Range(64, 32).Select(i => (byte)i).ToArray();
            var encrypted = EncryptValue("secret", otherKey);

            var helper = NewHelper();

            var ex = Assert.ThrowsException<CryptographicException>(
                () => helper.DecryptConnectionString(encrypted));
            Assert.IsInstanceOfType<AuthenticationTagMismatchException>(ex.InnerException,
                "The wrapped inner exception must be the GCM tag-mismatch failure.");
        }
    }
}
