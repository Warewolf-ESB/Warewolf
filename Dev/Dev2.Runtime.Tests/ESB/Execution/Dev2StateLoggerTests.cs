using System.IO;
using System.Security.Principal;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
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
        public void Dev2StateLogger_Ctor_Tests()
        {
            // mocks
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Some Workflow");
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);

            // setup
            var fileWrapper = new MyFileWrapper();
            var dev2StateLogger = new Dev2StateLogger(mockedDataObject.Object, fileWrapper);
            var activity = new Mock<IDev2Activity>();

            // test
            dev2StateLogger.LogPreExecuteState(activity.Object);

            // verify
            var text = fileWrapper.GetWrittenText();
            //Expect something like: "header:LogPreExecuteState\r\n{\"timestamp\":\"2018-06-19T16:05:29.6755408+02:00\",\"NextActivity\":null}\r\n{\"DsfDataObject\":{\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ClientID\":\"00000000-0000-0000-0000-000000000000\",\"ExecutingUser\":\"Mock<System.Security.Principal.IIdentity:00000001>.Object\",\"ExecutionID\":null,\"ExecutionOrigin\":0,\"ExecutionOriginDescription\":null,\"ExecutionToken\":\"Mock<Dev2.Common.Interfaces.IExecutionToken:00000001>.Object\",\"IsSubExecution\":false,\"IsRemoteWorkflow\":false,\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}}}}\r\n"
            Assert.IsTrue(text.Contains("LogPreExecuteState"));
            Assert.IsTrue(text.Contains("timestamp"));
            Assert.IsTrue(text.Contains("NextActivity"));
            Assert.IsTrue(text.Contains("scalars"));
            Assert.IsTrue(text.Contains("record_sets"));
            Assert.IsTrue(text.Contains("json_objects"));
            Assert.AreEqual("C:\\ProgramData\\Warewolf\\DetailedLogs\\00000000-0000-0000-0000-000000000000 - Some Workflow\\Detail.log",
                            fileWrapper.filePath);
        }
    }
    class MyFileWrapper : IFile
    {
        public string filePath;
        public MemoryStream stream;
        public string GetWrittenText()
        {
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public StreamWriter AppendText(string filePath)
        {
            this.filePath = filePath;
            stream = new MemoryStream();
            return new StreamWriter(stream);
        }

        #region not implemented
        public void AppendAllText(string path, string contents)
        {
            throw new System.NotImplementedException();
        }

        public void Copy(string source, string destination)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string tmpFileName)
        {
            throw new System.NotImplementedException();
        }

        public bool Exists(string path)
        {
            throw new System.NotImplementedException();
        }

        public FileAttributes GetAttributes(string path)
        {
            throw new System.NotImplementedException();
        }

        public void Move(string source, string destination)
        {
            throw new System.NotImplementedException();
        }

        public Stream OpenRead(string path)
        {
            throw new System.NotImplementedException();
        }

        public byte[] ReadAllBytes(string path)
        {
            throw new System.NotImplementedException();
        }

        public string ReadAllText(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            throw new System.NotImplementedException();
        }

        public void WriteAllBytes(string path, byte[] contents)
        {
            throw new System.NotImplementedException();
        }

        public void WriteAllText(string p1, string p2)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
