/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class EnvironmentVariablesTests
    {
        // AppDataPath is a best-effort GETTER: callers that genuinely need to write still fail on
        // their own write, but merely ASKING for the path must never take the process down.
        // It guarded Directory.CreateDirectory with catch (IOException) only — and
        // UnauthorizedAccessException is not an IOException, so on Linux (where CreateDirectory on
        // an unwritable path raises exactly that) the exception escaped the getter. Observed
        // 2026-09-21 on wwuat3: "Access to the path '/usr/share/Warewolf' is denied" propagated out
        // of FetchCurrentServerLog..ctor while ResourceCatalog.Instance built its management
        // services, breaking the XAML parse of every workflow holding a PublishRabbitMQActivity and
        // dead-lettering a whole load-test run.
        [TestMethod]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_AppDataPath_DoesNotThrow()
        {
            string path = null;

            try
            {
                path = EnvironmentVariables.AppDataPath;
            }
            catch (Exception e)
            {
                Assert.Fail($"AppDataPath must never throw — callers only asking for the path brought the host down. Got {e.GetType().Name}: {e.Message}");
            }

            Assert.IsFalse(string.IsNullOrWhiteSpace(path));
            Assert.IsTrue(Path.IsPathFullyQualified(path));
        }

        // The getter is allowed to hand back a directory it could not create (that is what the
        // swallowed exception means), but on a host where the data directory IS usable it must
        // actually have created it — otherwise every downstream Path.Combine writes into nothing.
        [TestMethod]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_AppDataPath_CreatesTheDirectoryWhenPermitted()
        {
            var path = EnvironmentVariables.AppDataPath;

            Assert.IsTrue(Directory.Exists(path), $"'{path}' was returned but never created; Config.GetDirectory is expected to resolve a writable base path.");
        }
    }
}
