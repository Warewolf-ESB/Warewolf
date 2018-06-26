using System;
using System.Security.Principal;
using Dev2.Common.Interfaces;
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
        [TestMethod]
        public void Dev2StateLogger_LogPreExecuteState_Tests()
        {
            var fileName = "C:\\ProgramData\\Warewolf\\DetailedLogs\\00000000-0000-0000-0000-000000000000 - Some Workflow\\Detail.log";
            // mocks
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Some Workflow");
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);

            // setup
            var fileWrapper = new FileWrapper();
            var directoryWrapper = new DirectoryWrapper();
            var dev2StateLogger = new Dev2JsonStateLogger(mockedDataObject.Object, fileWrapper);
            var activity = new Mock<IDev2Activity>();

            // test
            dev2StateLogger.LogPreExecuteState(activity.Object);
            dev2StateLogger.Dispose();
            // verify
            var text = fileWrapper.ReadAllText(fileName);
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
            var fileName = "C:\\ProgramData\\Warewolf\\DetailedLogs\\00000000-0000-0000-0000-000000000000 - Some Workflow\\Detail.log";
            // mocks
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Some Workflow");
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);

            // setup
            var fileWrapper = new FileWrapper();
            var directoryWrapper = new DirectoryWrapper();
            var dev2StateLogger = new Dev2JsonStateLogger(mockedDataObject.Object, fileWrapper);
            var previousActivity = new Mock<IDev2Activity>();
            var nextActivity = new Mock<IDev2Activity>();

            // test
            dev2StateLogger.LogPostExecuteState(previousActivity.Object, nextActivity.Object);
            dev2StateLogger.Dispose();
            // verify
            var text = fileWrapper.ReadAllText(fileName);
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
            var fileName = "C:\\ProgramData\\Warewolf\\DetailedLogs\\00000000-0000-0000-0000-000000000000 - Some Workflow\\Detail.log";
            // mocks
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Some Workflow");
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);

            // setup
            var fileWrapper = new FileWrapper();
            var directoryWrapper = new DirectoryWrapper();
            var dev2StateLogger = new Dev2JsonStateLogger(mockedDataObject.Object, fileWrapper);
            var nextActivity = new Mock<IDev2Activity>();
            var exception = new NullReferenceException();
            // test
            dev2StateLogger.LogExecuteException(exception, nextActivity.Object);
            dev2StateLogger.Dispose();
            // verify
            var text = fileWrapper.ReadAllText(fileName);
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
            var fileName = "C:\\ProgramData\\Warewolf\\DetailedLogs\\00000000-0000-0000-0000-000000000000 - Some Workflow\\Detail.log";
            // mocks
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Some Workflow");
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);

            // setup
            var fileWrapper = new FileWrapper();
            var directoryWrapper = new DirectoryWrapper();
            var dev2StateLogger = new Dev2JsonStateLogger(mockedDataObject.Object, fileWrapper);            
            var nextActivity = new Mock<IDev2Activity>();
            var exception = new NullReferenceException();
            // test
            dev2StateLogger.LogExecuteCompleteState();
            dev2StateLogger.Dispose();
            // verify
            var text = fileWrapper.ReadAllText(fileName);
            //Expect something like: "header:LogExecuteException{ "timestamp":"2018-06-20T08:32:01.719266+02:00","PreviousActivity":null,"Exception":"Object reference not set to an instance of an object."}{ "DsfDataObject":{ "ServerID":"00000000-0000-0000-0000-000000000000","ParentID":"00000000-0000-0000-0000-000000000000","ClientID":"00000000-0000-0000-0000-000000000000","ExecutingUser":"Mock<System.Security.Principal.IIdentity:00000001>.Object","ExecutionID":null,"ExecutionOrigin":0,"ExecutionOriginDescription":null,"ExecutionToken":"Mock<Dev2.Common.Interfaces.IExecutionToken:00000001>.Object","IsSubExecution":false,"IsRemoteWorkflow":false,"Environment":{ "scalars":{ },"record_sets":{ },"json_objects":{ } } } }""
            Assert.IsTrue(text.Contains("LogExecuteCompleteState"));
            Assert.IsTrue(text.Contains("timestamp"));
        }

        [TestMethod]
        public void Dev2StateLogger_LogStopExecutionState_Tests()
        {
            var fileName = "C:\\ProgramData\\Warewolf\\DetailedLogs\\00000000-0000-0000-0000-000000000000 - Some Workflow\\Detail.log";
            // mocks
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Some Workflow");
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);

            // setup
            var fileWrapper = new FileWrapper();
            var dev2StateLogger = new Dev2JsonStateLogger(mockedDataObject.Object, fileWrapper);
            var nextActivity = new Mock<IDev2Activity>();
            var exception = new NullReferenceException();
            // test
            dev2StateLogger.LogStopExecutionState();
            dev2StateLogger.Dispose();
            // verify
            var text = fileWrapper.ReadAllText(fileName);
            //Expect something like: "header:LogStopExecutionState\r\n{\"timestamp\":\"2018-06-25T09:50:55.1624974+02:00\"}\r\n{\"DsfDataObject\":{\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"ExecutingUser\":\"Mock<System.Security.Principal.IIdentity:00000001>.Object\",\"ExecutionID\":null,\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutionToken\":\"Mock<Dev2.Common.Interfaces.IExecutionToken:00000001>.Object\",\"IsSubExecution\":false,\"IsRemoteWorkflow\":false,\"Environment\":{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}}}\r\nheader:LogStopExecutionState\r\n{\"timestamp\":\"2018-06-25T09:52:02.3074228+02:00\"}\r\n{\"DsfDataObject\":{\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"ExecutingUser\":\"Mock<System.Security.Principal.IIdentity:00000001>.Object\",\"ExecutionID\":null,\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutionToken\":\"Mock<Dev2.Common.Interfaces.IExecutionToken:00000001>.Object\",\"IsSubExecution\":false,\"IsRemoteWorkflow\":false,\"Environment\":{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}}}\r\nheader:LogStopExecutionState\r\n{\"timestamp\":\"2018-06-25T09:52:31.2454735+02:00\"}\r\n{\"DsfDataObject\":{\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"ExecutingUser\":\"Mock<System.Security.Principal.IIdentity:00000001>.Object\",\"ExecutionID\":null,\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutionToken\":\"Mock<Dev2.Common.Interfaces.IExecutionToken:00000001>.Object\",\"IsSubExecution\":false,\"IsRemoteWorkflow\":false,\"Environment\":{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}}}\r\n"
            Assert.IsTrue(text.Contains("LogStopExecutionState"));
            Assert.IsTrue(text.Contains("timestamp"));
        }


        [TestMethod]
        public void Dev2StateLogger_Given_Compress_True()
        {
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Some Workflow");
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);

            var fileWrapper = new FileWrapper();
            var directoryWrapper = new DirectoryWrapper();
            var dev2StateLogger = new Dev2JsonStateLogger(mockedDataObject.Object, fileWrapper);

        }
    }
}    