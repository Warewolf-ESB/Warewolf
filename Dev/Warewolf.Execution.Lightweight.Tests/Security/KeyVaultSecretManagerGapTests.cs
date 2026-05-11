/*
 * Gap tests for KeyVaultSecretManager.
 *
 * The 82% baseline left the not-initialised guards (L135-137 / L142-144 of
 * KeyVaultSecretManager.cs) and the parse-failure branches in InitializeAsync
 * (L80-84 / L119-120) uncovered. These tests exercise them via the public
 * DEBUG path (debugSecret constructor argument) which lets us skip Key Vault
 * entirely.
 *
 * Constructor null-argument guards are also pinned — they are the engine's
 * only defence against being wired up with a missing dependency.
 */

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    [TestClass]
    public class KeyVaultSecretManagerGapTests
    {
        // Build a manager that bypasses Key Vault by passing a debugSecret. The
        // credential is irrelevant when debugSecret is set, so we can pass null.
        static KeyVaultSecretManager NewWithDebugSecret(string debugSecret) =>
            new(vaultUri:    "https://example.vault.azure.net/",
                secretName:  "warewolf-aes-key",
                credential:  null,
                logger:      NullLogger<KeyVaultSecretManager>.Instance,
                debugSecret: debugSecret);

        // Canonical 32-byte AES key, base64-encoded.
        static string CanonicalKeyJson =>
            """{"version":1,"keyId":"test","key":"AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=","created":"2026-01-01"}""";

        // ── Constructor guards ────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Constructor_NullVaultUri_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new KeyVaultSecretManager(null!, "secret", credential: null,
                    logger: NullLogger<KeyVaultSecretManager>.Instance, debugSecret: "x"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Constructor_NullSecretName_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new KeyVaultSecretManager("https://x", null!, credential: null,
                    logger: NullLogger<KeyVaultSecretManager>.Instance, debugSecret: "x"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Constructor_NullLogger_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new KeyVaultSecretManager("https://x", "s", credential: null, logger: null!,
                    debugSecret: "x"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Constructor_NullCredentialWithoutDebugSecret_Throws()
        {
            // Production path: with no debugSecret, a real TokenCredential is required.
            Assert.ThrowsException<ArgumentNullException>(() =>
                new KeyVaultSecretManager("https://x", "s", credential: null,
                    logger: NullLogger<KeyVaultSecretManager>.Instance, debugSecret: null));
        }

        // ── GetKeyBytes guards ────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetKeyBytes_BeforeInitialize_Throws()
        {
            var mgr = NewWithDebugSecret(CanonicalKeyJson);

            var ex = Assert.ThrowsException<InvalidOperationException>(() => mgr.GetKeyBytes());
            StringAssert.Contains(ex.Message, "not initialised");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task GetKeyBytes_AfterInitialize_ReturnsThirtyTwoBytes()
        {
            var mgr = NewWithDebugSecret(CanonicalKeyJson);
            await mgr.InitializeAsync();

            var bytes = mgr.GetKeyBytes();
            Assert.AreEqual(32, bytes.Length, "AES-256 keys must be exactly 32 bytes.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task GetKeyBytes_WrongKeyLength_Throws()
        {
            // 16-byte key (AES-128, not the required AES-256).
            var shortKey = Convert.ToBase64String(new byte[16]);
            var json     = $$"""{"version":1,"keyId":"short","key":"{{shortKey}}","created":""}""";

            var mgr = NewWithDebugSecret(json);
            await mgr.InitializeAsync();

            var ex = Assert.ThrowsException<InvalidOperationException>(() => mgr.GetKeyBytes());
            StringAssert.Contains(ex.Message, "32 bytes");
        }

        // ── InitializeAsync parse failures ────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task InitializeAsync_MissingKey_Throws()
        {
            // Valid JSON shape but with an empty "key" field.
            var json = """{"version":1,"keyId":"k","key":"","created":""}""";
            var mgr  = NewWithDebugSecret(json);

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => mgr.InitializeAsync());
            StringAssert.Contains(ex.Message, "empty key material");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task InitializeAsync_LegacyUnquotedJson_IsAutoRepaired()
        {
            // Old Encrypt-Config.ps1 emitted JSON without quoted keys/values.
            // The auto-repair path should still parse it.
            var legacy = "{version:1,keyId:test,key:" +
                         Convert.ToBase64String(new byte[32]) +
                         ",created:2026-01-01}";
            var mgr = NewWithDebugSecret(legacy);

            await mgr.InitializeAsync();
            Assert.AreEqual(32, mgr.GetKeyBytes().Length);
        }

        // ── KeyId surface ─────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void KeyId_BeforeInitialize_ReturnsSentinel()
        {
            var mgr = NewWithDebugSecret(CanonicalKeyJson);
            Assert.AreEqual("(not initialised)", mgr.KeyId);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task KeyId_AfterInitialize_ReturnsParsedKeyId()
        {
            var mgr = NewWithDebugSecret(CanonicalKeyJson);
            await mgr.InitializeAsync();

            Assert.AreEqual("test", mgr.KeyId);
        }
    }
}
