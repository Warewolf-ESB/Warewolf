using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchDebugItemFileTests
    {
        #region Static Class Init

        static string _testDir;

        [ClassInitialize]
        public static void MyClassInit(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
        }

        #endregion

        

        #region Execute

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FetchDebugItemFileExecuteWithNullValuesExpectedException()
        {

            var esb = new FetchDebugItemFile();
            var actual = esb.Execute(null, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FetchDebugItemFileExecuteWithNoDebugItemFileInValuesExpectedException()
        {

            var esb = new FetchDebugItemFile();
            var actual = esb.Execute(new Dictionary<string, string>{{"DebugFilePath",null}}, null);
            Assert.AreEqual(string.Empty, actual);
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FetchDebugItemFileExecuteWithNullDebugItemFileExpectedException()
        {

            var esb = new FetchDebugItemFile();
            var actual = esb.Execute(new Dictionary<string, string>{{"DebugItemFilePath",null}}, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FetchDebugItemFileExecuteWithNonExistingDebugItemFileExpectedReturnsEmptyString()
        {

            var esb = new FetchDebugItemFile();
            var actual = esb.Execute(new Dictionary<string, string>{{"DebugItemFilePath",""}}, null);
            Assert.AreEqual(string.Empty, actual);
        }


        [TestMethod]
        public void FetchDebugItemFileExecuteWithExistingLogExpectedReturnsContentsOfLog()
        {
            const string Expected = "Hello world";
            var serverLogPath = Path.Combine(_testDir, string.Format("ServerLog_{0}.txt", Guid.NewGuid()));
            File.WriteAllText(serverLogPath, Expected);

            var esb = new FetchDebugItemFile();
            var actual = esb.Execute(new Dictionary<string, string> { { "DebugItemFilePath", serverLogPath } }, null);
            StringAssert.Contains(actual,Expected );
        }

        #endregion

        #region HandlesType

        [TestMethod]
        public void FetchDebugItemFileHandlesTypeExpectedReturnsFetchCurrentServerLogService()
        {
            var esb = new FetchDebugItemFile();
            var result = esb.HandlesType();
            Assert.AreEqual("FetchDebugItemFileService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void FFetchDebugItemFileCreateServiceEntryExpectedReturnsDynamicService()
        {
            var esb = new FetchDebugItemFile();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><DebugItemFilePath ColumnIODirection=\"Input\"></DebugItemFilePath><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}