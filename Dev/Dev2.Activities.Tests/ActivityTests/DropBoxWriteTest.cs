using System.Collections.Generic;
using System.IO;
using Dev2.Activities;
using Dev2.Common.Interfaces.Wrappers;
using DropNet;
using DropNet.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DropBoxWriteTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDropBoxWriteActivity_Execute")]
        
        // ReSharper disable InconsistentNaming
        public void DsfDropBoxWriteActivity_Execute_Sucess()

        {
            //------------Setup for test--------------------------
            var dsfDropBoxWriteActivity = new DsfDropBoxWriteActivity();
            var client = new Mock<IDropNetClient>();
            var file = new Mock<IFile>();
            client.Setup(a => a.UploadFile("/", "meerkat", It.IsAny<byte[]>(), true, null)).Returns(new MetaData());
            file.Setup(a => a.ReadAllBytes("monkey")).Returns(new byte[0]);
            dsfDropBoxWriteActivity.DropNetClient = client.Object;
            dsfDropBoxWriteActivity.File = file.Object;
            //------------Execute Test---------------------------
            PrivateObject p = new PrivateObject(dsfDropBoxWriteActivity);
            var result =p.Invoke("PerformExecution", new object[] { new Dictionary<string, string> { { "SourceFile", "monkey" }, { "DestinationPath", "meerkat" } } });

            //------------Assert Results-------------------------
            client.Verify(a => a.UploadFile("/", "meerkat", It.IsAny<byte[]>(), true, null));
            Assert.AreEqual(result.ToString(),"Success");
        }





        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDropBoxWriteActivity_Execute")]
        public void DsfDropBoxWriteActivity_Execute_Failure()
        {
            //------------Setup for test--------------------------
            var dsfDropBoxWriteActivity = new DsfDropBoxWriteActivity();
            var client = new Mock<IDropNetClient>();
            var file = new Mock<IFile>();
            client.Setup(a => a.UploadFile("/", "meerkat", It.IsAny<byte[]>(), true, null)).Returns((MetaData)null);
            file.Setup(a => a.ReadAllBytes("monkey")).Returns(new byte[0]);
            dsfDropBoxWriteActivity.DropNetClient = client.Object;
            dsfDropBoxWriteActivity.File = file.Object;
            //------------Execute Test---------------------------
            PrivateObject p = new PrivateObject(dsfDropBoxWriteActivity);
            var result = p.Invoke("PerformExecution", new object[] { new Dictionary<string, string> { { "SourceFile", "monkey" }, { "DestinationPath", "meerkat" } } });

            //------------Assert Results-------------------------
            client.Verify(a => a.UploadFile("/", "meerkat", It.IsAny<byte[]>(), true, null));
            Assert.AreEqual(result.ToString(), "Failure");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDropBoxWriteActivity_Execute"),ExpectedException(typeof(IOException))]
        public void DsfDropBoxWriteActivity_Execute_IOError()
        {
            //------------Setup for test--------------------------
            var dsfDropBoxWriteActivity = new DsfDropBoxWriteActivity();
            var client = new Mock<IDropNetClient>();
            var file = new Mock<IFile>();
            client.Setup(a => a.UploadFile("/", "meerkat", It.IsAny<byte[]>(), true, null)).Returns((MetaData)null);
            file.Setup(a => a.ReadAllBytes("monkey")).Throws(new IOException("bob"));
            dsfDropBoxWriteActivity.DropNetClient = client.Object;
            dsfDropBoxWriteActivity.File = file.Object;
            //------------Execute Test---------------------------
            PrivateObject p = new PrivateObject(dsfDropBoxWriteActivity);
            var result = p.Invoke("PerformExecution", new object[] { new Dictionary<string, string> { { "SourceFile", "monkey" }, { "DestinationPath", "meerkat" } } });

            //------------Assert Results-------------------------
            client.Verify(a => a.UploadFile("/", "meerkat", It.IsAny<byte[]>(), true, null),Times.Never());
           
        }
        // ReSharper restore InconsistentNaming
    }
}
