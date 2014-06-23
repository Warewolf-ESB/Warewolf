using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Communication;
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

        ExecuteMessage ConvertToMsg(StringBuilder msg)
        {
            var serialier = new Dev2JsonSerializer();
            var result = serialier.Deserialize<ExecuteMessage>(msg);
            return result;
        }


        #region Execute

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FetchDebugItemFileExecuteWithNullValuesExpectedException()
        {

            var esb = new FetchDebugItemFile();
            var actual = esb.Execute(null, null);
            var msg = ConvertToMsg(actual);
            Assert.AreEqual(string.Empty, msg.Message.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FetchDebugItemFileExecuteWithNoDebugItemFileInValuesExpectedException()
        {

            var esb = new FetchDebugItemFile();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "DebugFilePath", null } }, null);
            var msg = ConvertToMsg(actual);
            Assert.AreEqual(string.Empty, msg.Message.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FetchDebugItemFileExecuteWithNullDebugItemFileExpectedException()
        {

            var esb = new FetchDebugItemFile();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "DebugItemFilePath", null } }, null);
            var msg = ConvertToMsg(actual);
            Assert.AreEqual(string.Empty, msg.Message.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FetchDebugItemFileExecuteWithNonExistingDebugItemFileExpectedReturnsEmptyString()
        {

            var esb = new FetchDebugItemFile();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "DebugItemFilePath", new StringBuilder() } }, null);
            var msg = ConvertToMsg(actual);
            Assert.AreEqual(string.Empty, msg.Message.ToString());
        }


        [TestMethod]
        public void FetchDebugItemFileExecuteWithExistingLogExpectedReturnsContentsOfLog()
        {
            const string Expected = "Hello world";
            var serverLogPath = Path.Combine(_testDir, string.Format("ServerLog_{0}.txt", Guid.NewGuid()));
            File.WriteAllText(serverLogPath, Expected);

            var esb = new FetchDebugItemFile();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "DebugItemFilePath", new StringBuilder(serverLogPath) } }, null);
            var msg = ConvertToMsg(actual);
            StringAssert.Contains(msg.Message.ToString(), Expected);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FetchDebugItemFile_Execute")]
// ReSharper disable InconsistentNaming
        public void FetchDebugItemFile_Execute_FileHasMultiLines_ReturnedMessageWillBeMultiLines()
// ReSharper restore InconsistentNaming
        {
            var multiLines = new StringBuilder();
            multiLines.AppendLine("Line One");
            multiLines.AppendLine("Line Two");
            multiLines.AppendLine("Line Three");

            var serverLogPath = Path.Combine(_testDir, string.Format("ServerLog_{0}.txt", Guid.NewGuid()));
            var multiLineString = multiLines.ToString();    
            File.WriteAllText(serverLogPath, multiLineString);

            var esb = new FetchDebugItemFile();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "DebugItemFilePath", new StringBuilder(serverLogPath) } }, null);
            var msg = ConvertToMsg(actual);
            StringAssert.Contains(msg.Message.ToString(), multiLineString);
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