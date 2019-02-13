/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Diagnostics.Logging;
using Dev2.Runtime.Configuration.Settings;
using Dev2.Runtime.Configuration.Tests.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class WorkflowLogggerTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowLoggger))]
        public void WorkflowLoggger_LoggingSettings()
        {
            var config = new Configuration(XmlResource.Fetch("Settings"));
            WorkflowLoggger.LoggingSettings = config.Logging;
            Assert.AreEqual("Logging", WorkflowLoggger.LoggingSettings.DisplayName);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowLoggger))]
        public void WorkflowLoggger_GetDefaultLogDirectoryPath()
        {
            var result = WorkflowLoggger.GetDefaultLogDirectoryPath();
            StringAssert.Contains(result, "Source\\repos\\Warewolf\\Dev\\Dev2.Diagnostics.Tests\\bin\\Debug\\Logs");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowLoggger))]
        public void WorkflowLoggger_GetDirectoryPath()
        {
            var config = new Configuration(XmlResource.Fetch("Settings"));
            WorkflowLoggger.LoggingSettings = config.Logging;
            var result = WorkflowLoggger.GetDirectoryPath(config.Logging);
            StringAssert.Contains(result, "Source\\repos\\Warewolf\\Dev\\Dev2.Diagnostics.Tests\\bin\\Debug\\Logs");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowLoggger))]
        public void WorkflowLoggger_ShouldLog_IsLoggingEnabled_True()
        {
            var config = new Configuration(XmlResource.Fetch("Settings"));
            WorkflowLoggger.LoggingSettings = config.Logging;

            config.Logging.IsLoggingEnabled = true;
            config.Logging.LogAll = true;
            WorkflowLoggger.UpdateSettings(config.Logging);
           
            var result = WorkflowLoggger.ShouldLog(new System.Guid());
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WorkflowLoggger))]
        public void WorkflowLoggger_ShouldLog_IsLoggingEnabled_False()
        {
            var config = new Configuration(XmlResource.Fetch("Settings"));
            WorkflowLoggger.LoggingSettings = config.Logging;

            config.Logging.IsLoggingEnabled = false;
            config.Logging.LogAll = false;
            WorkflowLoggger.UpdateSettings(config.Logging);
            Assert.IsFalse(WorkflowLoggger.ShouldLog(new System.Guid()));
        }
    }
}
