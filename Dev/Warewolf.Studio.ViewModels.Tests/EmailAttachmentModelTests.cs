using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming
// ReSharper disable ObjectCreationAsStatement

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class EmailAttachmentModelTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EmailAttachmentModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailAttachmentModel_Ctor_Null_ExpectException()
        {
            //------------Setup for test--------------------------
            new EmailAttachmentModel(null);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EmailAttachmentModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailAttachmentModel_CtorFilter_NullQueryManager_ExpectException()
        {
            //------------Setup for test--------------------------
            var mock1 = new Mock<IFile>();
            mock1.Setup(file => file.GetAttributes(It.IsAny<string>()));
            new EmailAttachmentModel(null, "", mock1.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EmailAttachmentModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailAttachmentModel_CtorFilter_NullFilter_ExpectException()
        {
            //------------Setup for test--------------------------
            var mock1 = new Mock<IFile>();
            mock1.Setup(file => file.GetAttributes(It.IsAny<string>()));
            new EmailAttachmentModel(new Mock<IQueryManager>().Object, null, mock1.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EmailAttachmentModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailAttachmentModel_CtorFilter_Null_NotNull()
        {
            //------------Setup for test--------------------------
            var mock1 = new Mock<IFile>();
            mock1.Setup(file => file.GetAttributes(It.IsAny<string>()));
            new EmailAttachmentModel(new Mock<IQueryManager>().Object, null, mock1.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EmailAttachmentModel_Ctor")]
        public void EmailAttachmentModel_Ctor_NotNull()
        {
            //------------Setup for test--------------------------
            new EmailAttachmentModel(new Mock<IQueryManager>().Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EmailAttachmentModel_GetDrives")]
        public void EmailAttachmentModel_GetDrivesExpectPassThrough()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            qm.Setup(a => a.FetchFiles()).Returns(new List<IFileListing>() { new FileListing() { Name = @"c:\" }, new FileListing() { Name = @"d:\" } });
            var emailAttachmentModel = new EmailAttachmentModel(qm.Object);

            //------------Execute Test---------------------------
            var drives = emailAttachmentModel.FetchDrives();
            //------------Assert Results-------------------------

            Assert.AreEqual(drives.Count,2);
            Assert.AreEqual(drives[0].Name,@"c:\");
            Assert.AreEqual(drives[1].Name, @"d:\");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("EmailAttachmentModel_GetDrives")]
        public void EmailAttachmentModel_GetDrivesAndFilterExpectPassThrough()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var mock = new Mock<IFileListing>();
            var mock1 = new Mock<IFile>();
            mock1.Setup(file => file.GetAttributes(It.IsAny<string>())).Returns(FileAttributes.Compressed | FileAttributes.System);
            qm.Setup(a => a.FetchFiles(mock.Object)).Returns(new List<IFileListing>() { new FileListing() {  FullName = @"c:home.js" }, new FileListing() { Name = @"d:\", FullName = @"d:\" } });
            var emailAttachmentModel = new EmailAttachmentModel(qm.Object,"js", mock1.Object);
           
            
         
            //------------Execute Test---------------------------
           
            var files = emailAttachmentModel.FetchFiles(mock.Object);
            //------------Assert Results-------------------------

            Assert.AreEqual(files.Count,1);
            Assert.AreEqual(files[0].FullName, @"c:home.js");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("EmailAttachmentModel_GetFiles")]
        public void EmailAttachmentModel_GetFilesExpectPassThrough()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var file = new FileListing() { Name = @"c:\" };
            qm.Setup(a => a.FetchFiles(file)).Returns(new List<IFileListing>() { new FileListing() { Name = @"c:\" } });
            var emailAttachmentModel = new EmailAttachmentModel(qm.Object);

            //------------Execute Test---------------------------
            var drives = emailAttachmentModel.FetchFiles(file);
            //------------Assert Results-------------------------

            Assert.AreEqual(drives.Count, 1);
            Assert.AreEqual(drives[0].Name, @"c:\");
        }

    }
}