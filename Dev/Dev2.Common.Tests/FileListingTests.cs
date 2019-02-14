/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class FileListingTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FileListing))]
        public void FileListing_Validate()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var expectedChildren = new Collection<IFileListing>
            {
                new FileListing { Name = "childNameOne" },
                new FileListing { Name = "childNameTwo" }
            };

            var mockFileListing = new Mock<IFileListing>();
            mockFileListing.Setup(fileList => fileList.Name).Returns(expectedName);
            mockFileListing.Setup(fileList => fileList.FullName).Returns(expectedFullName);
            mockFileListing.Setup(fileList => fileList.IsDirectory).Returns(expectedIsDirectory);

            var fileListing = new FileListing(mockFileListing.Object)
            {
                Children = expectedChildren
            };

            Assert.AreEqual(expectedName, fileListing.Name);
            Assert.AreEqual(expectedFullName, fileListing.FullName);
            Assert.AreEqual(expectedIsDirectory, fileListing.IsDirectory);
            Assert.AreEqual(2, fileListing.Children.Count);
            Assert.AreEqual("childNameOne", fileListing.Children.ToList()[0].Name);
            Assert.AreEqual("childNameTwo", fileListing.Children.ToList()[1].Name);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FileListing))]
        public void FileListing_ReferenceEquals_FileListing_Expected_True()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var mockFileListing = new Mock<IFileListing>();
            mockFileListing.Setup(fileList => fileList.Name).Returns(expectedName);
            mockFileListing.Setup(fileList => fileList.FullName).Returns(expectedFullName);
            mockFileListing.Setup(fileList => fileList.IsDirectory).Returns(expectedIsDirectory);

            var fileListing = new FileListing(mockFileListing.Object);

            var isEqual = fileListing.Equals(fileListing);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FileListing))]
        public void FileListing_Equals_FileListing_Expected_True()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var mockFileListing = new Mock<IFileListing>();
            mockFileListing.Setup(fileList => fileList.Name).Returns(expectedName);
            mockFileListing.Setup(fileList => fileList.FullName).Returns(expectedFullName);
            mockFileListing.Setup(fileList => fileList.IsDirectory).Returns(expectedIsDirectory);

            var fileListing = new FileListing(mockFileListing.Object);
            var fileListingDup = new FileListing(mockFileListing.Object);

            var isEqual = fileListing.Equals(fileListingDup);
            Assert.IsTrue(isEqual);
            Assert.IsTrue(fileListing == fileListingDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FileListing))]
        public void FileListing_Equals_FileListing_Expected_False()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var mockFileListing = new Mock<IFileListing>();
            mockFileListing.Setup(fileList => fileList.Name).Returns(expectedName);
            mockFileListing.Setup(fileList => fileList.FullName).Returns(expectedFullName);
            mockFileListing.Setup(fileList => fileList.IsDirectory).Returns(expectedIsDirectory);

            var fileListing = new FileListing(mockFileListing.Object);

            var mockFileListingDup = new Mock<IFileListing>();
            mockFileListingDup.Setup(fileList => fileList.Name).Returns("testNewName");
            mockFileListingDup.Setup(fileList => fileList.FullName).Returns(expectedFullName);
            mockFileListingDup.Setup(fileList => fileList.IsDirectory).Returns(expectedIsDirectory);

            var fileListingDup = new FileListing(mockFileListingDup.Object);

            var isEqual = fileListing.Equals(fileListingDup);
            Assert.IsFalse(isEqual);
            Assert.IsTrue(fileListing != fileListingDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FileListing))]
        public void FileListing_Equals_Object_Null_Expected_False()
        {
            var fileListing = new FileListing();

            const object fileListingObj = null;

            var isEqual = fileListing.Equals(fileListingObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FileListing))]
        public void FileListing_Equals_Object_Expected_True()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var expectedChildren = new Collection<IFileListing>
            {
                new FileListing { Name = "childNameOne" },
                new FileListing { Name = "childNameTwo" }
            };

            var mockFileListing = new Mock<IFileListing>();
            mockFileListing.Setup(fileList => fileList.Name).Returns(expectedName);
            mockFileListing.Setup(fileList => fileList.FullName).Returns(expectedFullName);
            mockFileListing.Setup(fileList => fileList.IsDirectory).Returns(expectedIsDirectory);

            var fileListing = new FileListing(mockFileListing.Object)
            {
                Children = expectedChildren
            };

            object fileListingObj = fileListing;

            var isEqual = fileListing.Equals(fileListingObj);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FileListing))]
        public void FileListing_Equals_Object_Expected_False()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var expectedChildren = new Collection<IFileListing>
            {
                new FileListing { Name = "childNameOne" },
                new FileListing { Name = "childNameTwo" }
            };

            var mockFileListing = new Mock<IFileListing>();
            mockFileListing.Setup(fileList => fileList.Name).Returns(expectedName);
            mockFileListing.Setup(fileList => fileList.FullName).Returns(expectedFullName);
            mockFileListing.Setup(fileList => fileList.IsDirectory).Returns(expectedIsDirectory);

            var fileListing = new FileListing(mockFileListing.Object)
            {
                Children = expectedChildren
            };

            var mockFileListingDup = new Mock<IFileListing>();
            mockFileListingDup.Setup(fileList => fileList.Name).Returns("testNewName");
            mockFileListingDup.Setup(fileList => fileList.FullName).Returns(expectedFullName);
            mockFileListingDup.Setup(fileList => fileList.IsDirectory).Returns(expectedIsDirectory);

            object fileListingObj = new FileListing(mockFileListingDup.Object)
            {
                Children = expectedChildren
            };

            var isEqual = fileListing.Equals(fileListingObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FileListing))]
        public void FileListing_Equals_Object_GetType_Expected_False()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var mockFileListing = new Mock<IFileListing>();
            mockFileListing.Setup(fileList => fileList.Name).Returns(expectedName);
            mockFileListing.Setup(fileList => fileList.FullName).Returns(expectedFullName);
            mockFileListing.Setup(fileList => fileList.IsDirectory).Returns(expectedIsDirectory);

            var fileListing = new FileListing(mockFileListing.Object);

            var fileListingObj = new object();

            var isEqual = fileListing.Equals(fileListingObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FileListing))]
        public void FileListing_GetHashCode_Not_Equal_To_Zero()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var expectedChildren = new Collection<IFileListing>
            {
                new FileListing { Name = "childNameOne" },
                new FileListing { Name = "childNameTwo" }
            };

            var mockFileListing = new Mock<IFileListing>();
            mockFileListing.Setup(fileList => fileList.Name).Returns(expectedName);
            mockFileListing.Setup(fileList => fileList.FullName).Returns(expectedFullName);
            mockFileListing.Setup(fileList => fileList.IsDirectory).Returns(expectedIsDirectory);

            var fileListing = new FileListing(mockFileListing.Object)
            {
                Children = expectedChildren
            };

            var hashCode = fileListing.GetHashCode();

            Assert.AreNotEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FileListing))]
        public void FileListing_GetHashCode_Expect_Zero()
        {
            var fileListing = new FileListing();

            var hashCode = fileListing.GetHashCode();

            Assert.AreEqual(0, hashCode);
        }
    }
}
