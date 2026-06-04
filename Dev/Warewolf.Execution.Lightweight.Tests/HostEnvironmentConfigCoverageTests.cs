/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests
{
    /// <summary>
    /// Coverage for <see cref="HostEnvironmentConfig"/>. Each test snapshots and
    /// restores every environment variable the loader touches so tests are
    /// safe to run in any order. They are also marked DoNotParallelize because
    /// they mutate process-wide environment state.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class HostEnvironmentConfigCoverageTests
    {
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
            _snapshot = new System.Collections.Generic.Dictionary<string, string?>();
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

        // ── Defaults ─────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Load_AllVarsAbsent_AppliesDefaults()
        {
            var cfg = HostEnvironmentConfig.Load();

            Assert.AreEqual(
                Path.Combine(AppContext.BaseDirectory, "Resources"),
                cfg.WorkflowsDirectory);
            Assert.IsNull(cfg.VaultName);
            Assert.AreEqual(HostEnvironmentConfig.DefaultSecretName, cfg.SecretName);
            Assert.AreEqual(Environment.MachineName, cfg.InstanceId);
            Assert.IsFalse(cfg.SkipFailureToRetrieveSecret);
            Assert.IsNull(cfg.TenantId);
            Assert.IsNull(cfg.ManagedIdentityClientId);
            Assert.IsNull(cfg.DebugKeyVaultSecret);
            Assert.IsFalse(cfg.IsDevelopment);
            Assert.IsFalse(cfg.EncryptionEnabled);
        }

        // ── Overrides ────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Load_WorkflowsDirectoryOverride_IsUsed()
        {
            Environment.SetEnvironmentVariable("WorkflowsDirectory", @"C:\custom\path");
            Assert.AreEqual(@"C:\custom\path", HostEnvironmentConfig.Load().WorkflowsDirectory);
        }

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Load_VaultNameSet_EnablesEncryptionAndBuildsUri()
        {
            Environment.SetEnvironmentVariable("AZURE_KEYVAULT_NAME", "my-vault");
            var cfg = HostEnvironmentConfig.Load();
            Assert.AreEqual("my-vault", cfg.VaultName);
            Assert.IsTrue(cfg.EncryptionEnabled);
            Assert.AreEqual("https://my-vault.vault.azure.net/", cfg.VaultUri);
        }

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Load_VaultNameWhitespace_KeepsEncryptionDisabled()
        {
            Environment.SetEnvironmentVariable("AZURE_KEYVAULT_NAME", "   ");
            var cfg = HostEnvironmentConfig.Load();
            Assert.IsFalse(cfg.EncryptionEnabled);
        }

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Load_SecretNameOverride_IsUsed()
        {
            Environment.SetEnvironmentVariable("KEYVAULT_SECRET_NAME", "custom-secret");
            Assert.AreEqual("custom-secret", HostEnvironmentConfig.Load().SecretName);
        }

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Load_WebsiteInstanceIdOverride_IsUsed()
        {
            Environment.SetEnvironmentVariable("WEBSITE_INSTANCE_ID", "instance-42");
            Assert.AreEqual("instance-42", HostEnvironmentConfig.Load().InstanceId);
        }

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Load_SkipFailureToRetrieveSecret_AcceptsTrueCaseInsensitive()
        {
            Environment.SetEnvironmentVariable("SkipFailureToRetrieveSecret", "TRUE");
            Assert.IsTrue(HostEnvironmentConfig.Load().SkipFailureToRetrieveSecret);

            Environment.SetEnvironmentVariable("SkipFailureToRetrieveSecret", "True");
            Assert.IsTrue(HostEnvironmentConfig.Load().SkipFailureToRetrieveSecret);
        }

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Load_SkipFailureToRetrieveSecret_OtherValuesAreFalse()
        {
            Environment.SetEnvironmentVariable("SkipFailureToRetrieveSecret", "false");
            Assert.IsFalse(HostEnvironmentConfig.Load().SkipFailureToRetrieveSecret);

            Environment.SetEnvironmentVariable("SkipFailureToRetrieveSecret", "yes");
            Assert.IsFalse(HostEnvironmentConfig.Load().SkipFailureToRetrieveSecret);
        }

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Load_TenantAndClientId_AreCopiedAsIs()
        {
            Environment.SetEnvironmentVariable("AZURE_TENANT_ID", "tenant-1");
            Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", "client-1");
            var cfg = HostEnvironmentConfig.Load();
            Assert.AreEqual("tenant-1", cfg.TenantId);
            Assert.AreEqual("client-1", cfg.ManagedIdentityClientId);
        }

        // ── IsDevelopment ────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Load_AzureFunctionsEnvironmentDevelopment_SetsIsDevelopment()
        {
            Environment.SetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT", "Development");
            Assert.IsTrue(HostEnvironmentConfig.Load().IsDevelopment);
        }

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Load_AspNetCoreEnvironmentDevelopment_SetsIsDevelopment()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "development");
            Assert.IsTrue(HostEnvironmentConfig.Load().IsDevelopment);
        }

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Load_BothEnvironmentVarsProduction_IsNotDevelopment()
        {
            Environment.SetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT", "Production");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            Assert.IsFalse(HostEnvironmentConfig.Load().IsDevelopment);
        }

        // ── DebugKeyVaultSecret ─────────────────────────────────────────────

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void DebugKeyVaultSecret_OnlyReadInDevelopment()
        {
            // Production: never read, regardless of var value.
            Environment.SetEnvironmentVariable("DEBUG_AZURE_KEYVAULT_SECRET", "secret-value");
            Assert.IsNull(HostEnvironmentConfig.Load().DebugKeyVaultSecret);

            // Development + set: returned.
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Assert.AreEqual("secret-value", HostEnvironmentConfig.Load().DebugKeyVaultSecret);
        }

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void DebugKeyVaultSecret_EmptyOrWhitespace_NormalisedToNull()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            Environment.SetEnvironmentVariable("DEBUG_AZURE_KEYVAULT_SECRET", "");
            Assert.IsNull(HostEnvironmentConfig.Load().DebugKeyVaultSecret);

            Environment.SetEnvironmentVariable("DEBUG_AZURE_KEYVAULT_SECRET", "   ");
            Assert.IsNull(HostEnvironmentConfig.Load().DebugKeyVaultSecret);
        }

        // ── CredentialOptions ───────────────────────────────────────────────

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void CredentialOptions_MirrorsConfigFields()
        {
            Environment.SetEnvironmentVariable("AZURE_TENANT_ID", "tid");
            Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", "cid");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            var cfg = HostEnvironmentConfig.Load();
            var opts = cfg.CredentialOptions;
            Assert.AreEqual(cfg.IsDevelopment, opts.IsDevelopment);
            Assert.AreEqual(cfg.TenantId, opts.TenantId);
            Assert.AreEqual(cfg.ManagedIdentityClientId, opts.ManagedIdentityClientId);
        }

        // ── Constants ───────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("HostEnvironmentConfig_Coverage")]
        public void Constants_AreStable()
        {
            Assert.AreEqual("dp-keyring-v1", HostEnvironmentConfig.DefaultSecretName);
            Assert.AreEqual("https://{0}.vault.azure.net/", HostEnvironmentConfig.VaultUriTemplate);
        }
    }
}
