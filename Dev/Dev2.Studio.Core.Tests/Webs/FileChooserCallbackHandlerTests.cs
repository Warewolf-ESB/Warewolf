
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Util;
using Dev2.Webs.Callbacks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Webs
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FileChooserCallbackHandlerTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            EventPublishers.Aggregator = null;
            AppSettings.LocalHost = "http://localhost:3142";
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileChooserCallbackHandler_Constructor_FileChooserMessageIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
#pragma warning disable 168
            var handler = new FileChooserCallbackHandler(null);
#pragma warning restore 168

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        public void FileChooserCallbackHandler_Save_ValueIsEmpty_ClearsMessageSelectedFiles()
        {
            //------------Setup for test--------------------------
            var message = new FileChooserMessage { SelectedFiles = new[] { "E:\\Data\\tasks1.txt", "E:\\Data\\tasks2.txt" } };
            var handler = new FileChooserCallbackHandler(message);

            //------------Execute Test---------------------------
            handler.Save(string.Empty);

            //------------Assert Results-------------------------
            Assert.IsNull(message.SelectedFiles);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        public void FileChooserCallbackHandler_Save_ValueIsNotNull_UpdatesMessageSelectedFiles()
        {
            //------------Setup for test--------------------------
            var message = new FileChooserMessage();
            var handler = new FileChooserCallbackHandler(message);

            //------------Execute Test---------------------------
            handler.Save("\"{'filePaths':['E:\\\\\\\\Data\\\\\\\\tasks1.txt','E:\\\\\\\\Data\\\\\\\\tasks2.txt']}\"");

            //------------Assert Results-------------------------
            Assert.IsNotNull(message.SelectedFiles);

            var selectedFiles = message.SelectedFiles.ToList();
            Assert.AreEqual(2, selectedFiles.Count);
            Assert.AreEqual("E:\\Data\\tasks1.txt", selectedFiles[0]);
            Assert.AreEqual("E:\\Data\\tasks2.txt", selectedFiles[1]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        public void FileChooserCallbackHandler_Save_ValueIsAnyStringAndCloseWindowIsTrue_DoesInvokeClose()
        {
            //------------Setup for test--------------------------
            var message = new FileChooserMessage { SelectedFiles = new[] { "E:\\Data\\tasks1.txt", "E:\\Data\\tasks2.txt" } };
            var handler = new TestFileChooserCallbackHandler(message);

            //------------Execute Test---------------------------
            handler.Save(It.IsAny<string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(1, handler.CloseHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        public void FileChooserCallbackHandler_Save_ValueIsAnyStringAndCloseWindowIsFalse_DoesNotInvokeClose()
        {
            //------------Setup for test--------------------------
            var message = new FileChooserMessage { SelectedFiles = new[] { "E:\\Data\\tasks1.txt", "E:\\Data\\tasks2.txt" } };
            var handler = new TestFileChooserCallbackHandler(message);

            //------------Execute Test---------------------------
            handler.Save(It.IsAny<string>(), false);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, handler.CloseHitCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        [ExpectedException(typeof(NotImplementedException))]
        public void FileChooserCallbackHandler_Save_ValueAndEnvironmentModel_ThrowsNotImplementedException()
        {
            //------------Setup for test--------------------------
            var message = new FileChooserMessage();
            var handler = new FileChooserCallbackHandler(message);

            //------------Execute Test---------------------------
            handler.Save("aaa", new Mock<IEnvironmentModel>().Object);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FileChooserCallbackHandler_Save")]
        [ExpectedException(typeof(NotImplementedException))]
        public void FileChooserCallbackHandler_Save_JsonObjAndEnvironmentModel_ThrowsNotImplementedException()
        {
            //------------Setup for test--------------------------
            var message = new FileChooserMessage();
            var handler = new TestFileChooserCallbackHandler(message);

            //------------Execute Test---------------------------
            handler.TestSave(new Mock<IEnvironmentModel>().Object, new object());

            //------------Assert Results-------------------------
        }
    }

    public class TestFileChooserCallbackHandler : FileChooserCallbackHandler
    {
        public TestFileChooserCallbackHandler(FileChooserMessage message)
            : base(message)
        {
        }

        public void TestSave(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            Save(environmentModel, jsonObj);
        }

        public int CloseHitCount { get; private set; }
        public override void Close()
        {
            CloseHitCount++;
            base.Close();
        }

    }
}
