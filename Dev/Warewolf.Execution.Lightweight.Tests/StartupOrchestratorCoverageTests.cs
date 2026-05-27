/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class StartupOrchestratorCoverageTests
    {
        // The variables HostEnvironmentConfig.Load touches.
        private static readonly string[] AllVars =
        {
            "WorkflowsDirectory", "AZURE_KEYVAULT_NAME", "KEYVAULT_SECRET_NAME",
            "WEBSITE_INSTANCE_ID", "SkipFailureToRetrieveSecret",
            "AZURE_TENANT_ID", "AZURE_CLIENT_ID", "DEBUG_AZURE_KEYVAULT_SECRET",
            "AZURE_FUNCTIONS_ENVIRONMENT", "ASPNETCORE_ENVIRONMENT"
        };

        private System.Collections.Generic.Dictionary<string, string?> _snapshot = null!;

        [TestInitialize]
        public void Setup()
        {
            _snapshot = new();
            foreach (var v in AllVars)
            {
                _snapshot[v] = Environment.GetEnvironmentVariable(v);
                Environment.SetEnvironmentVariable(v, null);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var (k, v) in _snapshot)
                Environment.SetEnvironmentVariable(k, v);
        }

        // ── ClassifyKeyVaultException (private static) ───────────────────────

        private static (string Category, string Guidance) InvokeClassify(
            Exception ex, HostEnvironmentConfig config)
        {
            var mi = typeof(StartupOrchestrator)
                .GetMethod("ClassifyKeyVaultException",
                    BindingFlags.Static | BindingFlags.NonPublic)!;
            var result = mi.Invoke(null, new object[] { ex, config })!;

            var t = result.GetType();
            var cat = (string)t.GetField("Item1")!.GetValue(result)!;
            var guid = (string)t.GetField("Item2")!.GetValue(result)!;
            return (cat, guid);
        }

        private static HostEnvironmentConfig MakeConfig(string vault = "test-vault",
                                                        string secret = "my-secret")
        {
            Environment.SetEnvironmentVariable("AZURE_KEYVAULT_NAME", vault);
            Environment.SetEnvironmentVariable("KEYVAULT_SECRET_NAME", secret);
            return HostEnvironmentConfig.Load();
        }

        [TestMethod]
        [TestCategory("StartupOrchestrator_Coverage")]
        public void Classify_AuthenticationFailed_TaggedAuthenticationFailed()
        {
            var cfg = MakeConfig();
            var (cat, guidance) = InvokeClassify(
                new AuthenticationFailedException("nope"), cfg);
            Assert.AreEqual("AuthenticationFailed", cat);
            StringAssert.Contains(guidance, "test-vault");
        }

        [TestMethod]
        [TestCategory("StartupOrchestrator_Coverage")]
        public void Classify_RequestFailed404_TaggedSecretNotFound()
        {
            var cfg = MakeConfig();
            var (cat, guidance) = InvokeClassify(
                new RequestFailedException(status: 404, message: "not found"), cfg);
            Assert.AreEqual("SecretNotFound", cat);
            StringAssert.Contains(guidance, "my-secret");
        }

        [TestMethod]
        [TestCategory("StartupOrchestrator_Coverage")]
        public void Classify_RequestFailed403_TaggedAccessDenied()
        {
            var cfg = MakeConfig();
            var (cat, _) = InvokeClassify(
                new RequestFailedException(status: 403, message: "denied"), cfg);
            Assert.AreEqual("AccessDenied", cat);
        }

        [TestMethod]
        [TestCategory("StartupOrchestrator_Coverage")]
        public void Classify_RequestFailed5xx_TaggedKeyVaultUnavailable()
        {
            var cfg = MakeConfig();
            var (cat, guidance) = InvokeClassify(
                new RequestFailedException(status: 503, message: "down"), cfg);
            Assert.AreEqual("KeyVaultUnavailable", cat);
            StringAssert.Contains(guidance, "503");
        }

        [TestMethod]
        [TestCategory("StartupOrchestrator_Coverage")]
        public void Classify_RequestFailedOther_TaggedKeyVaultRequestFailed()
        {
            var cfg = MakeConfig();
            var (cat, guidance) = InvokeClassify(
                new RequestFailedException(status: 400, message: "bad"), cfg);
            Assert.AreEqual("KeyVaultRequestFailed", cat);
            StringAssert.Contains(guidance, "400");
        }

        [TestMethod]
        [TestCategory("StartupOrchestrator_Coverage")]
        public void Classify_InvalidOperation_TaggedInvalidKeyMaterial()
        {
            var cfg = MakeConfig();
            var (cat, _) = InvokeClassify(new InvalidOperationException("malformed"), cfg);
            Assert.AreEqual("InvalidKeyMaterial", cat);
        }

        [TestMethod]
        [TestCategory("StartupOrchestrator_Coverage")]
        public void Classify_UriFormat_TaggedInvalidVaultUri()
        {
            var cfg = MakeConfig();
            var (cat, _) = InvokeClassify(new UriFormatException("bad uri"), cfg);
            Assert.AreEqual("InvalidVaultUri", cat);
        }

        [TestMethod]
        [TestCategory("StartupOrchestrator_Coverage")]
        public void Classify_UnknownException_TaggedUnexpectedError()
        {
            var cfg = MakeConfig();
            var (cat, guidance) = InvokeClassify(new ApplicationException("?"), cfg);
            Assert.AreEqual("UnexpectedError", cat);
            StringAssert.Contains(guidance, nameof(ApplicationException));
        }

        // ── RunStartupAsync (encryption disabled — happy paths) ─────────────

        private static IHost BuildMinimalHost() =>
            new HostBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddLogging();
                })
                .Build();

        [TestMethod]
        [TestCategory("StartupOrchestrator_Coverage")]
        public async Task RunStartupAsync_EncryptionDisabled_ExistingDir_Completes()
        {
            var dir = Path.Combine(Path.GetTempPath(), "so_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            // Drop a couple of .bite files so the file-count branch runs.
            File.WriteAllText(Path.Combine(dir, "a.bite"), "<x/>");
            File.WriteAllText(Path.Combine(dir, "b.bite"), "<x/>");

            Environment.SetEnvironmentVariable("WorkflowsDirectory", dir);
            try
            {
                using var host = BuildMinimalHost();
                var cfg = HostEnvironmentConfig.Load();
                Assert.IsFalse(cfg.EncryptionEnabled);

                await StartupOrchestrator.RunStartupAsync(host, cfg);
                // No throw == success
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best-effort */ }
            }
        }

        [TestMethod]
        [TestCategory("StartupOrchestrator_Coverage")]
        public async Task RunStartupAsync_EncryptionDisabled_MissingDir_StillCompletes()
        {
            // Point at a directory that does not exist — the diagnostics else-branch runs
            // and warm-up swallows its own failure.
            var missing = Path.Combine(Path.GetTempPath(),
                "so_missing_" + Guid.NewGuid().ToString("N"));
            Environment.SetEnvironmentVariable("WorkflowsDirectory", missing);

            using var host = BuildMinimalHost();
            var cfg = HostEnvironmentConfig.Load();

            await StartupOrchestrator.RunStartupAsync(host, cfg);
        }

        [TestMethod]
        [TestCategory("StartupOrchestrator_Coverage")]
        public async Task RunStartupAsync_LoggerFactoryAvailable_ByDefault()
        {
            // Sanity-check that a default HostBuilder provides ILoggerFactory so the
            // call site logger acquisition does not throw.
            using var host = new HostBuilder().Build();
            var cfg = HostEnvironmentConfig.Load();
            await StartupOrchestrator.RunStartupAsync(host, cfg);
        }
    }
}
