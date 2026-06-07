/*
 * Assembly-level test initialiser.
 *
 * Ensures the F_RealConfig_* tests in SecurityAuthTests always have a secure.config
 * to load, so they run deterministically instead of being skipped (Assert.Inconclusive).
 *
 * Resolution precedence (first match wins):
 *   1. WAREWOLF_TEST_SECURE_CONFIG already points at an existing file
 *      (e.g. a pipeline secret written by TestRun.ps1) — use it as-is.
 *   2. A real Warewolf server config is installed at the standard path
 *      (C:\ProgramData\Warewolf\Server Settings\secure.config) — let the tests
 *      validate the real config directly.
 *   3. Neither exists — generate a minimal synthetic secure.config and point
 *      WAREWOLF_TEST_SECURE_CONFIG at it.
 *
 * The synthetic config grants the Warewolf Administrators group global access and
 * the Public group global View, which is everything the F_RealConfig_* assertions need.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Warewolf.Execution.Lightweight.Tests.Security;

namespace Warewolf.Execution.Lightweight.Tests
{
    [TestClass]
    public static class CiTestSetup
    {
        private static string? _generatedConfigPath;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext _)
        {
            // 1. An explicit config (pipeline secret / developer override) takes precedence.
            var existing = Environment.GetEnvironmentVariable("WAREWOLF_TEST_SECURE_CONFIG");
            if (!string.IsNullOrWhiteSpace(existing) && File.Exists(existing))
                return;

            // 2. A real server config installed on this machine is validated directly.
            const string realConfigPath = @"C:\ProgramData\Warewolf\Server Settings\secure.config";
            if (File.Exists(realConfigPath))
                return;

            // 3. Otherwise generate a synthetic fallback so the F_RealConfig_* tests still run.
            var key = SecureConfigBuilder.NewSecretKey();
            var settings = SecureConfigBuilder.AllPublicGlobal(key);

            _generatedConfigPath = Path.Combine(Path.GetTempPath(), "ci-secure.config");
            File.WriteAllText(_generatedConfigPath, SecureConfigBuilder.Encrypt(settings));

            Environment.SetEnvironmentVariable("WAREWOLF_TEST_SECURE_CONFIG", _generatedConfigPath);
            Console.WriteLine($"[CiTestSetup] Generated synthetic secure.config -> {_generatedConfigPath}");
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            if (_generatedConfigPath is not null)
            {
                try { File.Delete(_generatedConfigPath); } catch { }
            }
        }
    }
}
