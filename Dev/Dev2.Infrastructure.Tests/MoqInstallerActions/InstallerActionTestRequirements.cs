using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.MoqInstallerActions
{
    /// <summary>
    /// Shared preconditions for the MoqInstallerActions tests. These tests manage real Windows
    /// local groups (the "Warewolf Administrators" and "Administrators" groups) through the WinNT
    /// provider, which requires Windows with Administrator elevation. When those prerequisites are
    /// not met the tests are marked <see cref="Assert.Inconclusive(string)"/> rather than failed,
    /// so they never fail on Linux/macOS CI (where local-group management is not applicable and is
    /// handled by Warewolf.Lightweight.Execution) or in a non-elevated Windows process.
    /// </summary>
    internal static class InstallerActionTestRequirements
    {
        /// <summary>
        /// Skips (Inconclusive) the calling test unless it is running on Windows in an elevated
        /// (Administrator) process, which is required to create/modify local Windows groups.
        /// </summary>
        public static void RequireWindowsLocalGroupAdmin()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive(
                    "Skipped: this test manages Windows local groups via the WinNT provider and is " +
                    "not applicable on Linux/macOS, where Warewolf groups are managed by " +
                    "Warewolf.Lightweight.Execution. Prerequisite to run it: execute on Windows in " +
                    "an elevated (Administrator) process.");
                return;
            }

            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Assert.Inconclusive(
                    "Skipped: this test creates/modifies local Windows groups ('Warewolf " +
                    "Administrators' and 'Administrators'), which requires Administrator elevation. " +
                    "Re-run from an elevated (Run as administrator) process.");
            }
        }
    }
}
