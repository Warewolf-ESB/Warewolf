using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetDugStatesTest
    {
        static string _testDir;

        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
            _testDir = Path.Combine(context.DeploymentDirectory, "TestLogDirectory");
            Directory.CreateDirectory(_testDir);
        }

        #endregion

        #region Execution

        [TestMethod]
        public void GetDugStateExecuteWithNullFilePathExpectedReturnsError()
        {
            var workspace = new Mock<IWorkspace>();

            var values = new Dictionary<string, string> { { "FilePath", "" }, { "DirectoryPath", "xyz" } };
            var esb = new GetDebugState();
            var result = esb.Execute(values, workspace.Object);
            Assert.IsTrue(result.Contains("FilePath is required"));
        }

        [TestMethod]
        public void GetDugStateExecuteWithNullDirectoryExpectedReturnsError()
        {
            var workspace = new Mock<IWorkspace>();

            var values = new Dictionary<string, string> { { "FilePath", "xyz" }, { "DirectoryPath", "" } };
            var esb = new GetDebugState();
            var result = esb.Execute(values, workspace.Object);
            Assert.IsTrue(result.Contains("DirectoryPath is required"));
        }

        [TestMethod]
        public void GetDugStateExecuteWithNonExistingPathExpectedReturnsError()
        {
            var workspace = new Mock<IWorkspace>();

            var values = new Dictionary<string, string> { { "FilePath", "xyz" }, { "DirectoryPath", "xyz" } };
            var esb = new GetDebugState();
            var result = esb.Execute(values, workspace.Object);
            Assert.IsTrue(result.Contains("Could not find a part of the path "));
        }

        [TestMethod]
        public void GetDugStateExecuteWithNonExistingFileExpectedReturnsError()
        {
            var workspace = new Mock<IWorkspace>();

            var values = new Dictionary<string, string> { { "FilePath", "xyz" }, { "DirectoryPath", _testDir } };
            var esb = new GetDebugState();
            var result = esb.Execute(values, workspace.Object);
            Assert.IsTrue(result.Contains("Could not find file "));
        }

        [TestMethod]
        public void GetDugStateExecuteWithInvalidXML()
        {
            var workspace = new Mock<IWorkspace>();

            var fileName = Guid.NewGuid().ToString() + "_Test.log";
            var path = Path.Combine(_testDir, fileName);
            File.WriteAllText(path, "<Tag></Tag>");

            var values = new Dictionary<string, string> { { "FilePath", fileName }, { "DirectoryPath", _testDir } };
            var esb = new GetDebugState();
            var result = esb.Execute(values, workspace.Object);
            Assert.IsTrue(result.Contains("There is an error in XML document "));
        }
        
        [TestMethod]
        public void GetDugStateExecuteWithFileLockedReturnsError()
        {
            var workspace = new Mock<IWorkspace>();

            var fileName = Guid.NewGuid().ToString() + "_Test.log";
            var path = Path.Combine(_testDir, fileName);
            File.WriteAllText(path, TestResources.WorkflowLogXML);

            var fs = File.OpenRead(path);

            //execute
            string result;
            try
            {
                var values = new Dictionary<string, string> { { "FilePath", fileName }, { "DirectoryPath", _testDir } };
                var esb = new GetDebugState();
                result = esb.Execute(values, workspace.Object);
            }
            finally
            {
                fs.Close();
            }

            //assert
            Assert.IsTrue(result.Contains("The process cannot access the file "));
        }

        [TestMethod]
        public void GetDugStateExecuteWithValidXMLReturnsJSON()
        {
            var workspace = new Mock<IWorkspace>();

            var fileName = Guid.NewGuid().ToString() + "_Test.log";
            var path = Path.Combine(_testDir, fileName);
            File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"utf-8\"?><Workflow></Workflow>");

            var values = new Dictionary<string, string> { { "FilePath", fileName }, { "DirectoryPath", _testDir } };
            var esb = new GetDebugState();
            var result = esb.Execute(values, workspace.Object);
            Assert.IsTrue(result.Contains("<JSON>[]</JSON>"));
        }

        
        #endregion Execution

        #region HandlesType

        [TestMethod]
        public void GetDugStateHandlesTypeExpectedReturnsDeleteLogService()
        {
            var esb = new GetDebugState();
            var result = esb.HandlesType();
            Assert.AreEqual("DebugStateService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void GetDugStateCreateServiceEntryExpectedReturnsDynamicService()
        {
            var esb = new GetDebugState();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><DirectoryPath/><FilePath/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}
