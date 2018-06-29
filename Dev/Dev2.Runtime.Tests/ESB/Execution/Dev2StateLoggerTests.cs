using System;
using System.IO;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class Dev2StateLoggerTests
    {
        IFile fileWrapper;
        IDirectory directoryWrapper;
        Dev2JsonStateLogger dev2StateLogger;
        Mock<IDev2Activity> activity;
        DetailedLogFile detailedLog;

        [TestCleanup]
        public void Cleanup()
        {
            if (directoryWrapper == null)
            {
                directoryWrapper = new DirectoryWrapper();
            }
            directoryWrapper.Delete(EnvironmentVariables.DetailLogPath, true);
        }

        [TestMethod]
        public void Dev2StateLogger_LogPreExecuteState_Tests()
        {
            TestSetup(out fileWrapper, out directoryWrapper, out dev2StateLogger, out activity, out detailedLog);
            // test
            dev2StateLogger.LogPreExecuteState(activity.Object);
            dev2StateLogger.Dispose();
            // verify
            var text = fileWrapper.ReadAllText(detailedLog.LogFilePath);
            //Expect something like: "header:LogPreExecuteState\r\n{\"timestamp\":\"2018-06-19T16:05:29.6755408+02:00\",\"NextActivity\":null}\r\n{\"DsfDataObject\":{\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"ExecutingUser\":\"Mock<System.Security.Principal.IIdentity:00000001>.Object\",\"ExecutionID\":null,\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutionToken\":\"Mock<Dev2.Common.Interfaces.IExecutionToken:00000001>.Object\",\"IsSubExecution\":false,\"IsRemoteWorkflow\":false,\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}}}}\r\n"
            Assert.IsTrue(text.Contains("LogPreExecuteState"));
            Assert.IsTrue(text.Contains("timestamp"));
            Assert.IsTrue(text.Contains("NextActivity"));
            Assert.IsTrue(text.Contains("scalars"));
            Assert.IsTrue(text.Contains("record_sets"));
            Assert.IsTrue(text.Contains("json_objects"));
        }

        [TestMethod]
        public void Dev2StateLogger_LogPostExecuteState_Tests()
        {
            TestSetup(out fileWrapper, out directoryWrapper, out dev2StateLogger, out activity, out detailedLog);
            var previousActivity = new Mock<IDev2Activity>();
            var nextActivity = new Mock<IDev2Activity>();
            // test
            dev2StateLogger.LogPostExecuteState(previousActivity.Object, nextActivity.Object);
            dev2StateLogger.Dispose();
            // verify
            var text = fileWrapper.ReadAllText(detailedLog.LogFilePath);
            //Expect something like: "header:LogPostExecuteState\r\n{\"timestamp\":\"2018-06-19T16:05:29.6755408+02:00\",\"NextActivity\":null}\r\n{\"DsfDataObject\":{\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"ExecutingUser\":\"Mock<System.Security.Principal.IIdentity:00000001>.Object\",\"ExecutionID\":null,\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutionToken\":\"Mock<Dev2.Common.Interfaces.IExecutionToken:00000001>.Object\",\"IsSubExecution\":false,\"IsRemoteWorkflow\":false,\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}}}}\r\n"
            Assert.IsTrue(text.Contains("LogPostExecuteState"));
            Assert.IsTrue(text.Contains("timestamp"));
            Assert.IsTrue(text.Contains("NextActivity"));
            Assert.IsTrue(text.Contains("scalars"));
            Assert.IsTrue(text.Contains("record_sets"));
            Assert.IsTrue(text.Contains("json_objects"));
        }

        [TestMethod]
        public void Dev2StateLogger_LogExecuteException_Tests()
        {
            TestSetup(out fileWrapper, out directoryWrapper, out dev2StateLogger, out activity, out detailedLog);
            // setup
            var nextActivity = new Mock<IDev2Activity>();
            var exception = new NullReferenceException();
            // test
            dev2StateLogger.LogExecuteException(exception, nextActivity.Object);
            dev2StateLogger.Dispose();
            // verify
            var text = fileWrapper.ReadAllText(detailedLog.LogFilePath);
            //Expect something like: "header:LogExecuteException{ "timestamp":"2018-06-20T08:32:01.719266+02:00","PreviousActivity":null,"Exception":"Object reference not set to an instance of an object."}{ "DsfDataObject":{ "ServerID":"00000000-0000-0000-0000-000000000000","ParentID":"00000000-0000-0000-0000-000000000000","ClientID":"00000000-0000-0000-0000-000000000000","ExecutingUser":"Mock<System.Security.Principal.IIdentity:00000001>.Object","ExecutionID":null,"ExecutionOrigin":0,"ExecutionOriginDescription":null,"ExecutionToken":"Mock<Dev2.Common.Interfaces.IExecutionToken:00000001>.Object","IsSubExecution":false,"IsRemoteWorkflow":false,"Environment":{ "scalars":{ },"record_sets":{ },"json_objects":{ } } } }""
            Assert.IsTrue(text.Contains("LogExecuteException"));
            Assert.IsTrue(text.Contains("timestamp"));
            Assert.IsTrue(text.Contains("PreviousActivity"));
            Assert.IsTrue(text.Contains("Exception"));
            Assert.IsTrue(text.Contains("scalars"));
            Assert.IsTrue(text.Contains("record_sets"));
            Assert.IsTrue(text.Contains("json_objects"));
        }

        [TestMethod]
        public void Dev2StateLogger_LogExecuteCompleteState_Tests()
        {
            TestSetup(out fileWrapper, out directoryWrapper, out dev2StateLogger, out activity, out detailedLog);
            var nextActivity = new Mock<IDev2Activity>();
            var exception = new NullReferenceException();
            // test
            dev2StateLogger.LogExecuteCompleteState();
            dev2StateLogger.Dispose();
            // verify
            var text = fileWrapper.ReadAllText(detailedLog.LogFilePath);
            //Expect something like: "header:LogExecuteException{ "timestamp":"2018-06-20T08:32:01.719266+02:00","PreviousActivity":null,"Exception":"Object reference not set to an instance of an object."}{ "DsfDataObject":{ "ServerID":"00000000-0000-0000-0000-000000000000","ParentID":"00000000-0000-0000-0000-000000000000","ClientID":"00000000-0000-0000-0000-000000000000","ExecutingUser":"Mock<System.Security.Principal.IIdentity:00000001>.Object","ExecutionID":null,"ExecutionOrigin":0,"ExecutionOriginDescription":null,"ExecutionToken":"Mock<Dev2.Common.Interfaces.IExecutionToken:00000001>.Object","IsSubExecution":false,"IsRemoteWorkflow":false,"Environment":{ "scalars":{ },"record_sets":{ },"json_objects":{ } } } }""
            Assert.IsTrue(text.Contains("LogExecuteCompleteState"));
            Assert.IsTrue(text.Contains("timestamp"));
        }

        [TestMethod]
        public void Dev2StateLogger_LogStopExecutionState_Tests()
        {
            TestSetup(out fileWrapper, out directoryWrapper, out dev2StateLogger, out activity, out detailedLog);
            var nextActivity = new Mock<IDev2Activity>();
            var exception = new NullReferenceException();
            // test
            dev2StateLogger.LogStopExecutionState();
            dev2StateLogger.Dispose();
            // verify
            var text = fileWrapper.ReadAllText(detailedLog.LogFilePath);
            //Expect something like: "header:LogStopExecutionState\r\n{\"timestamp\":\"2018-06-25T09:50:55.1624974+02:00\"}\r\n{\"DsfDataObject\":{\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"ExecutingUser\":\"Mock<System.Security.Principal.IIdentity:00000001>.Object\",\"ExecutionID\":null,\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutionToken\":\"Mock<Dev2.Common.Interfaces.IExecutionToken:00000001>.Object\",\"IsSubExecution\":false,\"IsRemoteWorkflow\":false,\"Environment\":{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}}}\r\nheader:LogStopExecutionState\r\n{\"timestamp\":\"2018-06-25T09:52:02.3074228+02:00\"}\r\n{\"DsfDataObject\":{\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"ExecutingUser\":\"Mock<System.Security.Principal.IIdentity:00000001>.Object\",\"ExecutionID\":null,\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutionToken\":\"Mock<Dev2.Common.Interfaces.IExecutionToken:00000001>.Object\",\"IsSubExecution\":false,\"IsRemoteWorkflow\":false,\"Environment\":{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}}}\r\nheader:LogStopExecutionState\r\n{\"timestamp\":\"2018-06-25T09:52:31.2454735+02:00\"}\r\n{\"DsfDataObject\":{\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"ExecutingUser\":\"Mock<System.Security.Principal.IIdentity:00000001>.Object\",\"ExecutionID\":null,\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutionToken\":\"Mock<Dev2.Common.Interfaces.IExecutionToken:00000001>.Object\",\"IsSubExecution\":false,\"IsRemoteWorkflow\":false,\"Environment\":{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}}}\r\n"
            Assert.IsTrue(text.Contains("LogStopExecutionState"));
            Assert.IsTrue(text.Contains("timestamp"));
        }

        [TestMethod]
        public void Dev2StateLogger_Given_LogFile_AlreadyExists()
        {
            var streamWriter = new StreamWriter(new MemoryStream());
            var mockedStream = new Mock<IDev2StreamWriter>();
            mockedStream.Setup(p => p.StreamWriter).Returns(streamWriter);
            var mockedDataObject = SetupDataObject();
            var mockedFileWrapper = new Mock<IFile>();
            mockedFileWrapper.Setup(p => p.AppendText(It.IsAny<string>())).Returns(mockedStream.Object);
            mockedFileWrapper.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);
            mockedFileWrapper.Setup(p => p.GetLastWriteTime(It.IsAny<string>())).Returns(DateTime.Today.AddDays(-1));
            dev2StateLogger = GetDev2JsonStateLogger(mockedFileWrapper.Object, mockedDataObject);
            var nextActivity = new Mock<IDev2Activity>();
            // test
            dev2StateLogger.Dispose();
            // verify
            mockedFileWrapper.Verify(p => p.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());
            mockedFileWrapper.Verify(p => p.AppendText(It.IsAny<string>()), Times.AtLeastOnce());
        }

        [TestMethod]
        public void Dev2StateLogger_Given_LogFile_AlreadyExists_And_Is_More_Than_2_Days_Old()
        {
            var streamWriter = new StreamWriter(new MemoryStream());
            var mockedStream = new Mock<IDev2StreamWriter>();
            mockedStream.Setup(p => p.StreamWriter).Returns(streamWriter);
            var mockedDataObject = SetupDataObject();
            var mockedFileWrapper = new Mock<IFile>();
            var zipWrapper = new Mock<IZipFile>();
            zipWrapper.Setup(p => p.CreateFromDirectory(It.IsAny<string>(), It.IsAny<string>()));
            mockedFileWrapper.Setup(p => p.AppendText(It.IsAny<string>())).Returns(mockedStream.Object);
            mockedFileWrapper.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);
            mockedFileWrapper.Setup(p => p.GetLastWriteTime(It.IsAny<string>())).Returns(DateTime.Today.AddDays(-5));
            dev2StateLogger = GetDev2JsonStateLogger(mockedFileWrapper.Object, mockedDataObject, zipWrapper.Object);
            var nextActivity = new Mock<IDev2Activity>();
            // test
            dev2StateLogger.Dispose();
            // verify
            mockedFileWrapper.Verify(p => p.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());
            mockedFileWrapper.Verify(p => p.AppendText(It.IsAny<string>()), Times.AtLeastOnce());
            zipWrapper.Verify(p=>p.CreateFromDirectory(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());

        }

        [TestMethod]
        public void Dev2StateLogger_Given_LogFile_AlreadyExists_And_Is_More_Than_30_Days_Old()
        {            
            var streamWriter = new StreamWriter(new MemoryStream());
            var mockedStream = new Mock<IDev2StreamWriter>();
            mockedStream.Setup(p => p.StreamWriter).Returns(streamWriter);
            var mockedDataObject = SetupDataObject();
            var mockedFileWrapper = new Mock<IFile>();
            var zipWrapper = new Mock<IZipFile>();
            zipWrapper.Setup(p => p.CreateFromDirectory(It.IsAny<string>(), It.IsAny<string>()));
            mockedFileWrapper.Setup(p => p.AppendText(It.IsAny<string>())).Returns(mockedStream.Object);
            mockedFileWrapper.Setup(p => p.Exists(It.IsAny<string>())).Returns(true);
            mockedFileWrapper.Setup(p => p.GetLastWriteTime(It.IsAny<string>())).Returns(DateTime.Today.AddDays(-45));
            dev2StateLogger = GetDev2JsonStateLogger(mockedFileWrapper.Object, mockedDataObject, zipWrapper.Object);
            // test
            dev2StateLogger.Dispose();
            // verify
            mockedFileWrapper.Verify(p => p.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());
            mockedFileWrapper.Verify(p => p.AppendText(It.IsAny<string>()), Times.AtLeastOnce());
            mockedFileWrapper.Verify(p => p.Delete(It.IsAny<string>()), Times.AtLeastOnce());
        }


        private static void TestSetup(out IFile fileWrapper, out IDirectory directoryWrapper, out Dev2JsonStateLogger dev2StateLogger, out Mock<IDev2Activity> activity, out DetailedLogFile detailedLog)
        {
            // setup
            Mock<IDSFDataObject> mockedDataObject = SetupDataObject();
            fileWrapper = new FileWrapper();
            directoryWrapper = new DirectoryWrapper();
            activity = new Mock<IDev2Activity>();
            dev2StateLogger = GetDev2JsonStateLogger(fileWrapper, mockedDataObject);
            detailedLog = SetupDetailedLog(dev2StateLogger);
        }

        private static Dev2JsonStateLogger GetDev2JsonStateLogger(IFile fileWrapper, Mock<IDSFDataObject> mockedDataObject, IZipFile zipWrapper = null)
        {
            return new Dev2JsonStateLogger(mockedDataObject.Object, fileWrapper, zipWrapper);
        }

        private static Mock<IDSFDataObject> SetupDataObject()
        {
            // mocks
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Some Workflow");
            mockedDataObject.Setup(o => o.ResourceID).Returns(() => Guid.NewGuid());
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);
            return mockedDataObject;
        }
        private static DetailedLogFile SetupDetailedLog(Dev2JsonStateLogger dev2StateLogger)
        {
            var privateObject = new PrivateObject(dev2StateLogger);
            var detailedLog = privateObject.GetField("_detailedLogFile") as DetailedLogFile;
            return detailedLog;
        }
    }
}