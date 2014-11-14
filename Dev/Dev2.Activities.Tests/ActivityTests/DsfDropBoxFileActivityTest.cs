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
    public class DsfDropBoxFileActivityTest
    {
        // ReSharper disable InconsistentNaming
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDropBoxFileActivity_Execute")]
        public void DsfDropBoxFileActivity_Execute_Sucess()

        {
            //------------Setup for test--------------------------
            var dsfDropBoxWriteActivity = new DsfDropBoxFileActivity();
            var client = new Mock<IDropNetClient>();
            var file = new Mock<IFile>();
            client.Setup(a => a.UploadFile("/", "meerkat", It.IsAny<byte[]>(), true, null)).Returns(new MetaData());
            file.Setup(a => a.ReadAllBytes("monkey")).Returns(new byte[0]);
            dsfDropBoxWriteActivity.DropNetClient = client.Object;
            dsfDropBoxWriteActivity.File = file.Object;
            dsfDropBoxWriteActivity.Operation = "Write File";
            //------------Execute Test---------------------------
            PrivateObject p = new PrivateObject(dsfDropBoxWriteActivity);
            var result = p.Invoke("PerformExecution", new object[] { new Dictionary<string, string> { { "SourceFile", "monkey" }, { "DestinationPath", "meerkat" }, { "Operation", "Write File" } } });

            //------------Assert Results-------------------------
            client.Verify(a => a.UploadFile("/", "meerkat", It.IsAny<byte[]>(), true, null));
            Assert.AreEqual(result.ToString(),"Success");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDropBoxFileActivity_Execute")]
        public void DsfDropBoxFileActivity_Execute_WriteToFolder_Sucess()
        {
            //------------Setup for test--------------------------
            var dsfDropBoxWriteActivity = new DsfDropBoxFileActivity();
            var client = new Mock<IDropNetClient>();
            var file = new Mock<IFile>();
            client.Setup(a => a.UploadFile("/bob/", "meerkat", It.IsAny<byte[]>(), true, null)).Returns(new MetaData());
            file.Setup(a => a.ReadAllBytes("monkey")).Returns(new byte[0]);
            dsfDropBoxWriteActivity.DropNetClient = client.Object;
            dsfDropBoxWriteActivity.File = file.Object;
            dsfDropBoxWriteActivity.Operation = "Write File";
            //------------Execute Test---------------------------
            PrivateObject p = new PrivateObject(dsfDropBoxWriteActivity);
            var result = p.Invoke("PerformExecution", new object[] { new Dictionary<string, string> { { "SourceFile", "monkey" }, { "DestinationPath", "/bob/meerkat" }, { "Operation", "Write File" } } });

            //------------Assert Results-------------------------
            client.Verify(a => a.UploadFile("/bob/", "meerkat", It.IsAny<byte[]>(), true, null));
            Assert.AreEqual(result.ToString(), "Success");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDropBoxFileActivity_Execute")]

        // ReSharper disable InconsistentNaming
        public void DsfDropBoxFileActivity_ExecuteRead_Sucess()
        {
            //------------Setup for test--------------------------
            var dsfDropBoxWriteActivity = new DsfDropBoxFileActivity();
            var client = new Mock<IDropNetClient>();
            var file = new Mock<IFile>();
            var output = new byte[0];
            client.Setup(a => a.GetFile( "meerkat")).Returns(output);
            file.Setup(a => a.ReadAllBytes("monkey")).Returns(new byte[0]);
            dsfDropBoxWriteActivity.DropNetClient = client.Object;
            dsfDropBoxWriteActivity.File = file.Object;
            dsfDropBoxWriteActivity.Operation = "Write File";
            //------------Execute Test---------------------------
            PrivateObject p = new PrivateObject(dsfDropBoxWriteActivity);
            var result = p.Invoke("PerformExecution", new object[] { new Dictionary<string, string> { { "SourceFile", "monkey" }, { "DestinationPath", "meerkat" }, { "Operation", "Read File" } } });

            //------------Assert Results-------------------------
            client.Verify(a => a.GetFile( "meerkat"));
            file.Verify(a=>a.WriteAllBytes("monkey",output));
            Assert.AreEqual(result.ToString(), "Success");
        }




        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDropBoxFileActivity_Execute")]
        // ReSharper disable InconsistentNaming
        public void DsfDropBoxFileActivity_NoOperation_Nothing_Happens()
        {
            //------------Setup for test--------------------------
            var dsfDropBoxWriteActivity = new DsfDropBoxFileActivity();
            var client = new Mock<IDropNetClient>();
            var file = new Mock<IFile>();
            var output = new byte[0];
            client.Setup(a => a.GetFile("meerkat")).Returns(output);
            file.Setup(a => a.ReadAllBytes("monkey")).Returns(new byte[0]);
            dsfDropBoxWriteActivity.DropNetClient = client.Object;
            dsfDropBoxWriteActivity.File = file.Object;

            //------------Execute Test---------------------------
            PrivateObject p = new PrivateObject(dsfDropBoxWriteActivity);
            var result = p.Invoke("PerformExecution", new object[] { new Dictionary<string, string> { { "SourceFile", "monkey" }, { "DestinationPath", "meerkat" }, { "Operation", "" } } });

            //------------Assert Results-------------------------
            client.Verify(a => a.GetFile("meerkat"),Times.Never());
            file.Verify(a => a.WriteAllBytes("monkey", output),Times.Never());
            Assert.AreEqual(result.ToString(), "Failure");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDropBoxFileActivity_Execute"),ExpectedException(typeof(IOException))]

        // ReSharper disable InconsistentNaming
        public void DsfDropBoxFileActivity_ExecuteRead_IOError()
        {
            //------------Setup for test--------------------------
            var dsfDropBoxWriteActivity = new DsfDropBoxFileActivity();
            var client = new Mock<IDropNetClient>();
            var file = new Mock<IFile>();
            var output = new byte[0];
            client.Setup(a => a.GetFile("meerkat")).Returns(output);
            file.Setup(a => a.WriteAllBytes("monkey", output)).Throws(new IOException());
            dsfDropBoxWriteActivity.DropNetClient = client.Object;
            dsfDropBoxWriteActivity.File = file.Object;
            dsfDropBoxWriteActivity.Operation = "Write File";
            //------------Execute Test---------------------------
            PrivateObject p = new PrivateObject(dsfDropBoxWriteActivity);
            p.Invoke("PerformExecution", new object[] { new Dictionary<string, string> { { "SourceFile", "monkey" }, { "DestinationPath", "meerkat" }, { "Operation", "Read File" } } });

        }




        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDropBoxFileActivity_Execute")]
        public void DsfDropBoxFileActivity_Execute_Failure()
        {
            //------------Setup for test--------------------------
            var dsfDropBoxWriteActivity = new DsfDropBoxFileActivity();
            var client = new Mock<IDropNetClient>();
            var file = new Mock<IFile>();
            client.Setup(a => a.UploadFile("/", "meerkat", It.IsAny<byte[]>(), true, null)).Returns((MetaData)null);
            file.Setup(a => a.ReadAllBytes("monkey")).Returns(new byte[0]);
            dsfDropBoxWriteActivity.DropNetClient = client.Object;
            dsfDropBoxWriteActivity.File = file.Object;
            dsfDropBoxWriteActivity.Operation = "Write File";
            //------------Execute Test---------------------------
            PrivateObject p = new PrivateObject(dsfDropBoxWriteActivity);
            var result = p.Invoke("PerformExecution", new object[] { new Dictionary<string, string> { { "SourceFile", "monkey" }, { "DestinationPath", "meerkat" }, { "Operation", "Write File" } } });

            //------------Assert Results-------------------------
            client.Verify(a => a.UploadFile("/", "meerkat", It.IsAny<byte[]>(), true, null));
            Assert.AreEqual(result.ToString(), "Failure");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDropBoxFileActivity_Execute"),ExpectedException(typeof(IOException))]
        public void DsfDropBoxFileActivity_Execute_IOError()
        {
            //------------Setup for test--------------------------
            var dsfDropBoxWriteActivity = new DsfDropBoxFileActivity();
            var client = new Mock<IDropNetClient>();
            var file = new Mock<IFile>();
            client.Setup(a => a.UploadFile("/", "meerkat", It.IsAny<byte[]>(), true, null)).Returns((MetaData)null);
            file.Setup(a => a.ReadAllBytes("monkey")).Throws(new IOException("bob"));
            dsfDropBoxWriteActivity.DropNetClient = client.Object;
            dsfDropBoxWriteActivity.File = file.Object;
            dsfDropBoxWriteActivity.Operation = "Write File";
            //------------Execute Test---------------------------
            PrivateObject p = new PrivateObject(dsfDropBoxWriteActivity);
            p.Invoke("PerformExecution", new object[] { new Dictionary<string, string> { { "SourceFile", "monkey" }, { "DestinationPath", "meerkat" }, { "Operation", "Write File" } } });

            //------------Assert Results-------------------------
            client.Verify(a => a.UploadFile("/", "meerkat", It.IsAny<byte[]>(), true, null),Times.Never());
           
        }
        // ReSharper restore InconsistentNaming

    }
}
