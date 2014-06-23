using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.Settings;
using Dev2.Runtime.Configuration.Tests.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Configuration.Tests.Settings
{
    [TestClass]    
    public class LoggingSettingsTests
    {
        [TestMethod]
        public void LoggingSettingsConstructionEmptyExpectEmptyWorkflowsAndFalseSettings()
        {
            var config = new Configuration.Settings.Configuration(XmlResource.Fetch("Settings"));
            var logging = config.Logging;

            Assert.IsTrue(logging.ServiceInput == "");
            Assert.IsTrue(logging.LogFileDirectory == "");
            Assert.IsFalse(logging.LogAll);
            Assert.IsTrue(logging.NestedLevelCount == 0);
            Assert.IsFalse(logging.IsOutputLogged);
            Assert.IsFalse(logging.IsInputLogged);
            Assert.IsFalse(logging.IsDataAndTimeLogged);
            Assert.IsFalse(logging.IsDurationLogged);
            Assert.IsFalse(logging.IsTypeLogged);
            Assert.IsFalse(logging.IsVersionLogged);
            Assert.IsFalse(logging.IsLoggingEnabled);
            Assert.IsTrue(logging.Workflows.Count == 0);
        }

        [TestMethod]
        public void LoggingSettingsConstructionWithoutPostWorkflowExpectWorkflowsAndSettingsSet()
        {
            var config = new Configuration.Settings.Configuration(XmlResource.Fetch("NonEmptySettings"));
            var logging = config.Logging;

            Assert.IsTrue(logging.ServiceInput == "TestInput");
            Assert.IsTrue(logging.LogFileDirectory == "TestDir");
            Assert.IsFalse(logging.LogAll);
            Assert.IsTrue(logging.NestedLevelCount == 2);
            Assert.IsTrue(logging.IsOutputLogged);
            Assert.IsTrue(logging.IsInputLogged);
            Assert.IsTrue(logging.IsDataAndTimeLogged);
            Assert.IsTrue(logging.IsDurationLogged);
            Assert.IsTrue(logging.IsTypeLogged);
            Assert.IsTrue(logging.IsVersionLogged);
            Assert.IsTrue(logging.IsLoggingEnabled);
            Assert.IsTrue(logging.Workflows.Count == 1);
            Assert.IsFalse(logging.RunPostWorkflow);
            Assert.IsNull(logging.PostWorkflow);
        }

        [TestMethod]
        public void LoggingSettingsConstructionWithPostWorkflowExpectSettingsSet()
        {
            var config = new Configuration.Settings.Configuration(XmlResource.Fetch("SettingsWithPostWorkflow"));
            var logging = config.Logging;

            Assert.IsTrue(logging.RunPostWorkflow);
            Assert.IsNotNull(logging.PostWorkflow);
            Assert.IsTrue(logging.Workflows.Any(wf => wf.ResourceID == logging.PostWorkflow.ResourceID));
            Assert.IsFalse(logging.IsInitializing);
        }

        [TestMethod]
        public void ToXmlSerializesCorrectly()
        {
            var logging = new LoggingSettings("localhost");

            logging.ServiceInput = "TestInput";
            logging.LogFileDirectory = "TestDir";
            logging.LogAll = true;
            logging.NestedLevelCount = 2;
            logging.IsOutputLogged = true;
            logging.IsInputLogged = true;
            logging.IsDataAndTimeLogged = true;
            logging.IsDurationLogged = true;
            logging.IsTypeLogged = true;
            logging.IsVersionLogged = true;
            logging.IsLoggingEnabled = true;
            var workflow = new WorkflowDescriptor(XmlResource.Fetch("Workflow"));
            logging.Workflows.Add(workflow);
            logging.PostWorkflow = workflow;

            var actual = logging.ToXml().ToString();
            var expected = XmlResource.Fetch("LoggingSettings").ToString();
            Assert.AreEqual(actual, expected);
        }
    }
}
