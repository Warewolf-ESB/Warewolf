/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Assembly-level fixture for the integration test suite.
    ///
    /// Disables the WorkflowExecutor licence/subscription gate for the lifetime
    /// of the test process so the in-process end-to-end tests
    /// (WorkflowExecutorEndToEndTests etc.) can drive real .bite workflows
    /// through the executor without a production subscription being available.
    ///
    /// The gate is controlled by the <c>WAREWOLF_LICENSE_CHECK_ENABLED</c>
    /// environment variable and defaults to enabled — so tests must explicitly
    /// opt out, matching the convention used by WorkflowExecutorLicenseGateTests
    /// in the unit-test assembly.
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
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Environment.SetEnvironmentVariable(LicenseGateEnvVar, _previousLicenseGateValue);
        }
    }
}
