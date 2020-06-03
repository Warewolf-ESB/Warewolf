/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Common.Interfaces.Wrappers;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.Upload
{
    [TestClass]

    public class DropBoxUploadTests
    {
        Mock<IDropBoxUpload> CreateDropboxUploadMock()
        {
            var mock = new Mock<IDropBoxUpload>();
            var fileMetadata = new DropboxUploadSuccessResult(new FileMetadata());
            mock.Setup(upload => upload.ExecuteTask(It.IsAny<IDropboxClient>()))
                 .Returns(fileMetadata);
            return mock;
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamndla Dube")]
        [TestCategory(nameof(DropBoxUpload))]
        public void DropBoxUpload_CreateDropBoxActivity_GivenIsNew_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropBoxUpload = CreateDropboxUploadMock().Object;
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropBoxUpload);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamndla Dube")]
        [TestCategory(nameof(DropBoxUpload))]
        public void DropBoxUpload_ExecuteTask_GivenDropBoxUpload_ShouldReturnFileMetadata()
        {
            //---------------Set up test pack-------------------
            var dropBoxUpload = CreateDropboxUploadMock();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxUpload);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            dropBoxUpload.Object.ExecuteTask(It.IsAny<IDropboxClient>());
            dropBoxUpload.Verify(upload => upload.ExecuteTask(It.IsAny<IDropboxClient>()));
        }
        
      
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamndla Dube")]
        [TestCategory(nameof(DropBoxUpload))]
        [ExpectedException(typeof(ArgumentException))]
        public void DropBoxUpload_CreateNewDropboxUpload_GivenMissingFromPath_ExpectArgumentException()
        {
            //---------------Set up test pack-------------------
            //---------------Execute Test ----------------------
            new DropBoxUpload(false, "random.txt", "");
            //---------------Test Result -----------------------
        } 
        
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamndla Dube")]
        [TestCategory(nameof(DropBoxUpload))]
        public void DropBoxUpload_ExecuteTask_IsDropboxFailureResult_ExpectTrue()
        {
            //---------------Set up test pack-------------------
            var dropBoxUpload = new DropBoxUpload(false, "", "random.txt");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxUpload);
            //---------------Execute Test ----------------------
            var metadata = dropBoxUpload.ExecuteTask(It.IsAny<IDropboxClient>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(metadata);
            Assert.IsInstanceOfType(metadata,typeof(DropboxFailureResult));
        }

       
    }
}