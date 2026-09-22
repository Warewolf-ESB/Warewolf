/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests.Infrastructure
{
    /// <summary>
    /// Coverage for <see cref="SqlClientCompatibilityConfigurator"/> — the build 30775 fix for
    /// Microsoft.Data.SqlClient's registry-based alias lookup throwing a NullReferenceException
    /// on Linux (Flex Consumption is Linux-only) the moment the first SqlConnection opens.
    ///
    /// <see cref="AppContext.SetSwitch"/> mutates real, process-wide state with no seam to fake
    /// it, matching <c>ThreadPoolStartupConfiguratorTests</c>' own rationale for
    /// <see cref="DoNotParallelizeAttribute"/> here.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class SqlClientCompatibilityConfiguratorTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Configure_SetsDisableUseOfAliasSwitch()
        {
            SqlClientCompatibilityConfigurator.Configure();

            var switchWasSet = AppContext.TryGetSwitch(
                SqlClientCompatibilityConfigurator.DisableUseOfAliasSwitch, out var isEnabled);

            Assert.IsTrue(switchWasSet, "Expected the AppContext switch to have been set.");
            Assert.IsTrue(isEnabled, "Expected the alias-lookup switch to be disabled (true).");
        }
    }
}
