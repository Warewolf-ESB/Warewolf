/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class ConfigTests
    {
        // Regression test for WOLF-8501: Config's static field initializers (Server, Studio,
        // Auditing, Legacy, Persistence, Chatbot) all resolve their SettingsPath via
        // Config.AppDataPath/UserDataPath -> GetDirectory(), which falls back through an
        // environment variable, then ConfigurationManager.AppSettings, then a special folder,
        // then the temp directory. In hosts without a classic app-config system (e.g. an Azure
        // Functions isolated-worker process), the ConfigurationManager.AppSettings fallback can
        // throw; that must not crash Config's static initializer for the whole process. These
        // tests exercise the full GetDirectory() fallback chain end-to-end and assert it always
        // returns a valid, rooted path without throwing.
        [TestMethod]
        [TestCategory(nameof(Config))]
        public void Config_AppDataPath_DoesNotThrow_AndReturnsRootedWarewolfPath()
        {
            var path = Config.AppDataPath;

            Assert.IsFalse(string.IsNullOrWhiteSpace(path));
            Assert.IsTrue(Path.IsPathFullyQualified(path));
            Assert.IsTrue(path.EndsWith("Warewolf", System.StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        [TestCategory(nameof(Config))]
        public void Config_UserDataPath_DoesNotThrow_AndReturnsRootedWarewolfPath()
        {
            var path = Config.UserDataPath;

            Assert.IsFalse(string.IsNullOrWhiteSpace(path));
            Assert.IsTrue(Path.IsPathFullyQualified(path));
            Assert.IsTrue(path.EndsWith("Warewolf", System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
