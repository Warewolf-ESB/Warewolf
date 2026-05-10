/*
 * Assembly-level test initialiser.
 *
 * When the environment variable WAREWOLF_GENERATE_CI_CONFIG=1 is set (as it is
 * by Run-Coverage.ps1 when no WAREWOLF_SECURE_CONFIG_CONTENT secret is provided),
 * this class generates a minimal synthetic secure.config file and writes its path
 * into WAREWOLF_TEST_SECURE_CONFIG so that the F_RealConfig_* tests in
 * SecurityAuthTests can run instead of being skipped.
 *
 * The generated config contains only what those tests require:
 *   - A non-empty SecretKey
 *   - A "Warewolf Administrators" entry with global View permission
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
            var generate = Environment.GetEnvironmentVariable("WAREWOLF_GENERATE_CI_CONFIG");
            if (!string.Equals(generate, "1", StringComparison.Ordinal))
                return;

            // Only generate if WAREWOLF_TEST_SECURE_CONFIG isn't already pointing at
            // an existing file (e.g. a pipeline secret written by Run-Coverage.ps1).
            var existing = Environment.GetEnvironmentVariable("WAREWOLF_TEST_SECURE_CONFIG");
            if (!string.IsNullOrWhiteSpace(existing) && File.Exists(existing))
                return;

            var key = SecureConfigBuilder.NewSecretKey();
            var settings = SecureConfigBuilder.AllPublicGlobal(key);

            _generatedConfigPath = Path.Combine(Path.GetTempPath(), "ci-secure.config");
            File.WriteAllText(_generatedConfigPath, SecureConfigBuilder.Encrypt(settings));

            Environment.SetEnvironmentVariable("WAREWOLF_TEST_SECURE_CONFIG", _generatedConfigPath);
            Console.WriteLine($"[CiTestSetup] Generated CI secure.config -> {_generatedConfigPath}");
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
