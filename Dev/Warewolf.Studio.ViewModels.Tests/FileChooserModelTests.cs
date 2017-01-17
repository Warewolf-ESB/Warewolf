using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class FileChooserModelTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory(" FileChooserModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileChooserModel_Ctor_Null_ExpectException()
        {
            //------------Setup for test--------------------------
            new FileChooserModel(null);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(" FileChooserModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileChooserModel_CtorFilter_NullQueryManager_ExpectException()
        {
            //------------Setup for test--------------------------
            var mock1 = new Mock<IFile>();
            mock1.Setup(file => file.GetAttributes(It.IsAny<string>()));
            new FileChooserModel(null, "", mock1.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(" FileChooserModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileChooserModel_CtorFilter_NullFilter_ExpectException()
        {
            //------------Setup for test--------------------------
            var mock1 = new Mock<IFile>();
            mock1.Setup(file => file.GetAttributes(It.IsAny<string>()));
            new FileChooserModel(new Mock<IQueryManager>().Object, null, mock1.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(" FileChooserModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileChooserModel_CtorFilter_Null_NotNull()
        {
            //------------Setup for test--------------------------
            var mock1 = new Mock<IFile>();
            mock1.Setup(file => file.GetAttributes(It.IsAny<string>()));
            new FileChooserModel(new Mock<IQueryManager>().Object, null, mock1.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory(" FileChooserModel_Ctor")]
        public void FileChooserModel_Ctor_NotNull()
        {
            //------------Setup for test--------------------------
            new FileChooserModel(new Mock<IQueryManager>().Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory(" FileChooserModel_GetDrives")]
        public void FileChooserModel_GetDrivesExpectPassThrough()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            qm.Setup(a => a.FetchFiles()).Returns(new List<IFileListing>() { new FileListing() { Name = @"c:\" }, new FileListing() { Name = @"d:\" } });
            var emailAttachmentModel = new FileChooserModel(qm.Object);

            //------------Execute Test---------------------------
            var drives = emailAttachmentModel.FetchDrives();
            //------------Assert Results-------------------------

            Assert.AreEqual(drives.Count, 2);
            Assert.AreEqual(drives[0].Name, @"c:\");
            Assert.AreEqual(drives[1].Name, @"d:\");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(" FileChooserModel_GetDrives")]
        public void FileChooserModel_GetDrivesAndFilterExpectPassThrough()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var mock = new Mock<IFileListing>();
            var mock1 = new Mock<IFile>();
            mock1.Setup(file => file.GetAttributes(It.IsAny<string>())).Returns(FileAttributes.Compressed | FileAttributes.System);
            qm.Setup(a => a.FetchFiles(mock.Object)).Returns(new List<IFileListing>() { new FileListing() { FullName = @"c:home.js" }, new FileListing() { Name = @"d:\", FullName = @"d:\" } });
            var emailAttachmentModel = new FileChooserModel(qm.Object, "js", mock1.Object);



            //------------Execute Test---------------------------

            var files = emailAttachmentModel.FetchFiles(mock.Object);
            //------------Assert Results-------------------------

            Assert.AreEqual(files.Count, 1);
            Assert.AreEqual(files[0].FullName, @"c:home.js");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory(" FileChooserModel_GetFiles")]
        public void FileChooserModel_GetFilesExpectPassThrough()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var file = new FileListing() { Name = @"c:\" };
            qm.Setup(a => a.FetchFiles(file)).Returns(new List<IFileListing>() { new FileListing() { Name = @"c:\" } });
            var emailAttachmentModel = new FileChooserModel(qm.Object);

            //------------Execute Test---------------------------
            var drives = emailAttachmentModel.FetchFiles(file);
            //------------Assert Results-------------------------

            Assert.AreEqual(drives.Count, 1);
            Assert.AreEqual(drives[0].Name, @"c:\");
        }
    }
}
