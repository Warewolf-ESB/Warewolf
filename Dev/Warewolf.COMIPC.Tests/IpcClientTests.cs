using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using WarewolfCOMIPC.Client;

namespace WarewolfCOMIPC.Test
{
    [TestClass]
    public class IpcClientTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructo_GivenPipeStream_ShouldResult()
        {
            //---------------Set up test pack-------------------
            var pipeMock = new Mock<INamedPipeClientStreamWrapper>();

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var client = new IpcClient(pipeMock.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(client);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetIPCExecutor_GivenPipeStream_ShouldResult()
        {
            //---------------Set up test pack-------------------
            var pipeMock = new Mock<INamedPipeClientStreamWrapper>();
            var client = new IpcClient(pipeMock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(client);
            //---------------Execute Test ----------------------
            var ipcExecutor = IpcClient.GetIPCExecutor(pipeMock.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(ipcExecutor);
            Assert.IsFalse(ReferenceEquals(client,ipcExecutor));

        }

        private const string AdodbConnectionClassId = "00000514-0000-0010-8000-00AA006D2EA4";

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Invoke_GivenGetType_ShouldReturnResult()
        {
            //---------------Set up test pack-------------------
            var pipeMock = new Mock<INamedPipeClientStreamWrapper>();
            var memoryStream = new MemoryStream();
            var serializeObject = JsonConvert.SerializeObject(GetType());
            memoryStream.WriteByte(Encoding.ASCII.GetBytes(serializeObject)[0]);
            pipeMock.Setup(wrapper => wrapper.GetInternalStream()).Returns(memoryStream);
            var client = IpcClient.GetIPCExecutor(pipeMock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(client);
            //---------------Execute Test ----------------------
            var invoke = client.Invoke(Guid.Parse(AdodbConnectionClassId), "ToString", Execute.GetType, new ParameterInfoTO[] { });
            //---------------Test Result -----------------------
            Assert.IsNull(invoke);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Invoke_GivenGetMethods_ShouldReturnResult()
        {
            //---------------Set up test pack-------------------
            var pipeMock = new Mock<INamedPipeClientStreamWrapper>();
            var memoryStream = new MemoryStream();
            var serializeObject = JsonConvert.SerializeObject(GetType().GetMethods()[0]);
            memoryStream.WriteByte(Encoding.ASCII.GetBytes(serializeObject)[0]);
            pipeMock.Setup(wrapper => wrapper.GetInternalStream()).Returns(memoryStream);
            var client = IpcClient.GetIPCExecutor(pipeMock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(client);
            //---------------Execute Test ----------------------
            var invoke = client.Invoke(Guid.Parse(AdodbConnectionClassId), "ToString", Execute.GetMethods, new ParameterInfoTO[] { }) as List<MethodInfoTO>;
            //---------------Test Result -----------------------
            Assert.IsNotNull(invoke);
            CollectionAssert.AllItemsAreNotNull(invoke);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Invoke_GivenExecuteSpecifiedMethod_ShouldReturnResult()
        {
            //---------------Set up test pack-------------------
            var pipeMock = new Mock<INamedPipeClientStreamWrapper>();
            var memoryStream = new MemoryStream();
            var serializeObject = JsonConvert.SerializeObject(GetType());
            var buffer = Encoding.ASCII.GetBytes(serializeObject);
            memoryStream.Write(buffer,0,buffer.Length);
            pipeMock.Setup(wrapper => wrapper.GetInternalStream()).Returns(memoryStream);
            var client = IpcClient.GetIPCExecutor(pipeMock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(client);
            //---------------Execute Test ----------------------
            var invoke = client.Invoke(Guid.Parse(AdodbConnectionClassId), "ToString", Execute.ExecuteSpecifiedMethod, new ParameterInfoTO[] { });
            //---------------Test Result -----------------------
            Assert.IsNotNull(invoke);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Invoke_GivenGetNamespaces_ShouldReturnResult()
        {
            //---------------Set up test pack-------------------
            var pipeMock = new Mock<INamedPipeClientStreamWrapper>();
            var memoryStream = new MemoryStream();
            var serializeObject = JsonConvert.SerializeObject(GetType());
            memoryStream.WriteByte(Encoding.ASCII.GetBytes(serializeObject)[0]);
            pipeMock.Setup(wrapper => wrapper.GetInternalStream()).Returns(memoryStream);
            var client = IpcClient.GetIPCExecutor(pipeMock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(client);
            //---------------Execute Test ----------------------
            var invoke = client.Invoke(Guid.Parse(AdodbConnectionClassId), "ToString", Execute.GetNamespaces, new ParameterInfoTO[] { });
            //---------------Test Result -----------------------
            Assert.IsNull(invoke);
        }

        

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dispose_PassThrough()
        {
            //---------------Set up test pack-------------------
            var pipeMock = new Mock<INamedPipeClientStreamWrapper>();
            pipeMock.Setup(wrapper => wrapper.Dispose()).Verifiable();
            pipeMock.Setup(wrapper => wrapper.Close()).Verifiable();
            
            //---------------Assert Precondition----------------
         
            //---------------Execute Test ----------------------
            using (var client = IpcClient.GetIPCExecutor(pipeMock.Object))
            {
                Assert.IsNotNull(client);
            }
            //---------------Test Result -----------------------
            pipeMock.VerifyAll();
        }

    }
}
