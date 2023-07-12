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
    public class DllListingTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DllListing))]
        public void DllListing_Validate()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;
            const string expectedClsId = "testClsId";
            const bool expectedIs32Bit = false;

            var expectedChildren = new Collection<IFileListing>
            {
                new FileListing { Name = "childNameOne" },
                new FileListing { Name = "childNameTwo" }
            };

            var mockDllListingModel = new Mock<IDllListingModel>();
            mockDllListingModel.Setup(dllListModel => dllListModel.Name).Returns(expectedName);
            mockDllListingModel.Setup(dllListModel => dllListModel.FullName).Returns(expectedFullName);
            mockDllListingModel.Setup(dllListModel => dllListModel.IsDirectory).Returns(expectedIsDirectory);
            mockDllListingModel.Setup(dllListModel => dllListModel.ClsId).Returns(expectedClsId);
            mockDllListingModel.Setup(dllListModel => dllListModel.Is32Bit).Returns(expectedIs32Bit);

            var dllListing = new DllListing(mockDllListingModel.Object)
            {
                Children = expectedChildren
            };

            Assert.AreEqual(expectedName, dllListing.Name);
            Assert.AreEqual(expectedFullName, dllListing.FullName);
            Assert.AreEqual(expectedIsDirectory, dllListing.IsDirectory);
            Assert.AreEqual(2, dllListing.Children.Count);
            Assert.AreEqual("childNameOne", dllListing.Children.ToList()[0].Name);
            Assert.AreEqual("childNameTwo", dllListing.Children.ToList()[1].Name);
            Assert.AreEqual(expectedClsId, dllListing.ClsId);
            Assert.AreEqual(expectedIs32Bit, dllListing.Is32Bit);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DllListing))]
        public void DllListing_ReferenceEquals_DllListing_Expected_True()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var mockDllListingModel = new Mock<IDllListingModel>();
            mockDllListingModel.Setup(dllListModel => dllListModel.Name).Returns(expectedName);
            mockDllListingModel.Setup(dllListModel => dllListModel.FullName).Returns(expectedFullName);
            mockDllListingModel.Setup(dllListModel => dllListModel.IsDirectory).Returns(expectedIsDirectory);

            var dllListing = new DllListing(mockDllListingModel.Object);

            var isEqual = dllListing.Equals(dllListing);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DllListing))]
        public void DllListing_Equals_FileListing_Expected_True()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var mockDllListingModel = new Mock<IDllListingModel>();
            mockDllListingModel.Setup(dllListModel => dllListModel.Name).Returns(expectedName);
            mockDllListingModel.Setup(dllListModel => dllListModel.FullName).Returns(expectedFullName);
            mockDllListingModel.Setup(dllListModel => dllListModel.IsDirectory).Returns(expectedIsDirectory);

            var dllListing = new DllListing(mockDllListingModel.Object);
            var dllListingDup = new DllListing(mockDllListingModel.Object);

            var isEqual = dllListing.Equals(dllListingDup);
            Assert.IsTrue(isEqual);
            Assert.IsTrue(dllListing == dllListingDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DllListing))]
        public void DllListing_Equals_FileListing_Expected_False()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var mockDllListingModel = new Mock<IDllListingModel>();
            mockDllListingModel.Setup(dllListModel => dllListModel.Name).Returns(expectedName);
            mockDllListingModel.Setup(dllListModel => dllListModel.FullName).Returns(expectedFullName);
            mockDllListingModel.Setup(dllListModel => dllListModel.IsDirectory).Returns(expectedIsDirectory);

            var dllListing = new DllListing(mockDllListingModel.Object);

            var mockDllListingModelDup = new Mock<IDllListingModel>();
            mockDllListingModelDup.Setup(dllListModel => dllListModel.Name).Returns("testNewName");
            mockDllListingModelDup.Setup(dllListModel => dllListModel.FullName).Returns(expectedFullName);
            mockDllListingModelDup.Setup(dllListModel => dllListModel.IsDirectory).Returns(expectedIsDirectory);

            var dllListingDup = new DllListing(mockDllListingModelDup.Object);

            var isEqual = dllListing.Equals(dllListingDup);
            Assert.IsFalse(isEqual);
            Assert.IsTrue(dllListing != dllListingDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DllListing))]
        public void DllListing_Equals_Object_Null_Expected_False()
        {
            var dllListing = new DllListing();

            const object dllListingObj = null;

            var isEqual = dllListing.Equals(dllListingObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DllListing))]
        public void DllListing_Equals_Object_Expected_True()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var expectedChildren = new Collection<IFileListing>
            {
                new DllListing { Name = "childNameOne" },
                new DllListing { Name = "childNameTwo" }
            };

            var mockDllListingModel = new Mock<IDllListingModel>();
            mockDllListingModel.Setup(dllListModel => dllListModel.Name).Returns(expectedName);
            mockDllListingModel.Setup(dllListModel => dllListModel.FullName).Returns(expectedFullName);
            mockDllListingModel.Setup(dllListModel => dllListModel.IsDirectory).Returns(expectedIsDirectory);

            var dllListing = new DllListing(mockDllListingModel.Object)
            {
                Children = expectedChildren
            };

            object dllListingObj = dllListing;

            var isEqual = dllListing.Equals(dllListingObj);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DllListing))]
        public void DllListing_Equals_Object_Expected_False()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var expectedChildren = new Collection<IFileListing>
            {
                new FileListing { Name = "childNameOne" },
                new FileListing { Name = "childNameTwo" }
            };

            var mockDllListingModel = new Mock<IDllListingModel>();
            mockDllListingModel.Setup(dllListModel => dllListModel.Name).Returns(expectedName);
            mockDllListingModel.Setup(dllListModel => dllListModel.FullName).Returns(expectedFullName);
            mockDllListingModel.Setup(dllListModel => dllListModel.IsDirectory).Returns(expectedIsDirectory);

            var dllListing = new DllListing(mockDllListingModel.Object)
            {
                Children = expectedChildren
            };

            var mockDllListingModelDup = new Mock<IDllListingModel>();
            mockDllListingModelDup.Setup(dllListModel => dllListModel.Name).Returns("testNewName");
            mockDllListingModelDup.Setup(dllListModel => dllListModel.FullName).Returns(expectedFullName);
            mockDllListingModelDup.Setup(dllListModel => dllListModel.IsDirectory).Returns(expectedIsDirectory);

            object dllListingObj = new DllListing(mockDllListingModelDup.Object)
            {
                Children = expectedChildren
            };

            var isEqual = dllListing.Equals(dllListingObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DllListing))]
        public void DllListing_Equals_Object_GetType_Expected_False()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var mockDllListingModel = new Mock<IDllListingModel>();
            mockDllListingModel.Setup(dllListModel => dllListModel.Name).Returns(expectedName);
            mockDllListingModel.Setup(dllListModel => dllListModel.FullName).Returns(expectedFullName);
            mockDllListingModel.Setup(dllListModel => dllListModel.IsDirectory).Returns(expectedIsDirectory);

            var dllListing = new DllListing(mockDllListingModel.Object);

            var dllListingObj = new object();

            var isEqual = dllListing.Equals(dllListingObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DllListing))]
        public void DllListing_GetHashCode_Not_Equal_To_Zero()
        {
            const string expectedName = "testName";
            const string expectedFullName = "testFullName";
            const bool expectedIsDirectory = false;

            var expectedChildren = new Collection<IFileListing>
            {
                new FileListing { Name = "childNameOne" },
                new FileListing { Name = "childNameTwo" }
            };

            var mockDllListingModel = new Mock<IDllListingModel>();
            mockDllListingModel.Setup(dllListModel => dllListModel.Name).Returns(expectedName);
            mockDllListingModel.Setup(dllListModel => dllListModel.FullName).Returns(expectedFullName);
            mockDllListingModel.Setup(dllListModel => dllListModel.IsDirectory).Returns(expectedIsDirectory);

            var dllListing = new DllListing(mockDllListingModel.Object)
            {
                Children = expectedChildren
            };

            var hashCode = dllListing.GetHashCode();

            Assert.AreNotEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DllListing))]
        public void DllListing_GetHashCode_Expect_Zero()
        {
            var dllListing = new DllListing();

            var hashCode = dllListing.GetHashCode();

            Assert.AreEqual(0, hashCode);
            //Assert.AreEqual(-641118530, hashCode);
        }
    }
}
