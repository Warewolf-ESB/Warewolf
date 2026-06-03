/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Integration.Tests.InProcess;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// The single assembly-level fixture for the integration test suite (MSTest permits
    /// only one <c>[AssemblyInitialize]</c> per assembly).
    ///
    /// Responsibilities:
    /// <list type="bullet">
    ///   <item>
    ///     Disables the WorkflowExecutor licence/subscription gate for the lifetime of the
    ///     test process so in-process end-to-end tests can drive real .bite workflows through
    ///     the executor without a production subscription. Controlled by
    ///     <c>WAREWOLF_LICENSE_CHECK_ENABLED</c> (defaults to enabled, so tests opt out).
    ///   </item>
    ///   <item>
    ///     Starts the in-process <see cref="HttpbinEmulator"/> (WireMock on port 4000) that the
    ///     Web GET/POST workflow fixtures call instead of the live httpbin.org service.
    ///   </item>
    /// </list>
    /// </summary>
    [TestClass]
    public static class IntegrationTestAssemblyInit
    {
        const string LicenseGateEnvVar = "WAREWOLF_LICENSE_CHECK_ENABLED";
        static string? _previousLicenseGateValue;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext _)
        {
            _previousLicenseGateValue = Environment.GetEnvironmentVariable(LicenseGateEnvVar);
            Environment.SetEnvironmentVariable(LicenseGateEnvVar, "false");

            HttpbinEmulator.Start();
            ElasticsearchEmulator.Start();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            ElasticsearchEmulator.Stop();
            HttpbinEmulator.Stop();
            Environment.SetEnvironmentVariable(LicenseGateEnvVar, _previousLicenseGateValue);
        }
    }
}
