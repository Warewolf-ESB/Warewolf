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
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Common.Tests.Core
{
    [TestClass]
    public class ComPluginSourceDefinitionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ComPluginSourceDefinition))]
        public void ComPluginSourceDefinition_Validate_FileSystemAssemblyName()
        {
            const string expectedComName = "testComName";
            const string expectedClsId = "testClsId";
            const bool expectedIs32Bit = false;
            var expectedResourceID = Guid.NewGuid();
            const string expectedSavePath = "";
            const string expectedResourceName = "testResource";

            var mockComPlugin = new Mock<IComPlugin>();
            mockComPlugin.Setup(comPlugin => comPlugin.ComName).Returns(expectedComName);
            mockComPlugin.Setup(comPlugin => comPlugin.ClsId).Returns(expectedClsId);
            mockComPlugin.Setup(comPlugin => comPlugin.Is32Bit).Returns(expectedIs32Bit);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceID).Returns(expectedResourceID);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceName).Returns(expectedResourceName);

            var fileListing = new DllListing
            {
                Name = expectedComName,
                ClsId = expectedClsId,
                Children = new Collection<IFileListing>(),
                IsDirectory = false
            };

            var comPluginSourceDefinition = new ComPluginSourceDefinition(mockComPlugin.Object);

            Assert.AreEqual(fileListing, comPluginSourceDefinition.SelectedDll);
            Assert.AreEqual(expectedResourceID, comPluginSourceDefinition.Id);
            Assert.AreEqual(expectedSavePath, comPluginSourceDefinition.ResourcePath);
            Assert.AreEqual(expectedClsId, comPluginSourceDefinition.ClsId);
            Assert.AreEqual(expectedIs32Bit, comPluginSourceDefinition.Is32Bit);
            Assert.AreEqual(expectedResourceName, comPluginSourceDefinition.ResourceName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ComPluginSourceDefinition))]
        public void ComPluginSourceDefinition_Equals_ComPluginSource_Expected_True()
        {
            const string expectedComName = "testComName";
            const string expectedClsId = "testClsId";
            const bool expectedIs32Bit = false;
            var expectedResourceID = Guid.NewGuid();
            const string expectedResourceName = "testResource";

            var mockComPlugin = new Mock<IComPlugin>();
            mockComPlugin.Setup(comPlugin => comPlugin.ComName).Returns(expectedComName);
            mockComPlugin.Setup(comPlugin => comPlugin.ClsId).Returns(expectedClsId);
            mockComPlugin.Setup(comPlugin => comPlugin.Is32Bit).Returns(expectedIs32Bit);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceID).Returns(expectedResourceID);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceName).Returns(expectedResourceName);

            var comPluginSourceDefinition = new ComPluginSourceDefinition(mockComPlugin.Object);
            var comPluginSourceDefinitionDup = new ComPluginSourceDefinition(mockComPlugin.Object);

            var isEqual = comPluginSourceDefinition.Equals(comPluginSourceDefinitionDup);
            Assert.IsTrue(isEqual);
            Assert.IsTrue(comPluginSourceDefinition == comPluginSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ComPluginSourceDefinition))]
        public void ComPluginSourceDefinition_Equals_ComPluginSource_Expected_False()
        {
            const string expectedComName = "testComName";
            const string expectedClsId = "testClsId";
            const bool expectedIs32Bit = false;
            var expectedResourceID = Guid.NewGuid();
            const string expectedResourceName = "testResource";

            var mockComPlugin = new Mock<IComPlugin>();
            mockComPlugin.Setup(comPlugin => comPlugin.ComName).Returns(expectedComName);
            mockComPlugin.Setup(comPlugin => comPlugin.ClsId).Returns(expectedClsId);
            mockComPlugin.Setup(comPlugin => comPlugin.Is32Bit).Returns(expectedIs32Bit);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceID).Returns(expectedResourceID);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceName).Returns(expectedResourceName);

            var comPluginSourceDefinition = new ComPluginSourceDefinition(mockComPlugin.Object);

            var mockComPluginDup = new Mock<IComPlugin>();
            mockComPluginDup.Setup(comPlugin => comPlugin.ComName).Returns("NewComName");
            mockComPluginDup.Setup(comPlugin => comPlugin.ClsId).Returns(expectedClsId);
            mockComPluginDup.Setup(comPlugin => comPlugin.Is32Bit).Returns(expectedIs32Bit);
            mockComPluginDup.Setup(comPlugin => comPlugin.ResourceID).Returns(expectedResourceID);
            mockComPluginDup.Setup(comPlugin => comPlugin.ResourceName).Returns(expectedResourceName);

            var comPluginSourceDefinitionDup = new ComPluginSourceDefinition(mockComPluginDup.Object);

            var isEqual = comPluginSourceDefinition.Equals(comPluginSourceDefinitionDup);
            Assert.IsFalse(isEqual);
            Assert.IsTrue(comPluginSourceDefinition != comPluginSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ComPluginSourceDefinition))]
        public void ComPluginSourceDefinition_Equals_Object_Null_Expected_False()
        {
            var comPluginSourceDefinition = new ComPluginSourceDefinition();

            const object comPluginSource = null;

            var isEqual = comPluginSourceDefinition.Equals(comPluginSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ComPluginSourceDefinition))]
        public void ComPluginSourceDefinition_Equals_Object_Expected_True()
        {
            const string expectedComName = "testComName";
            const string expectedClsId = "testClsId";
            const bool expectedIs32Bit = false;
            var expectedResourceID = Guid.NewGuid();
            const string expectedResourceName = "testResource";

            var mockComPlugin = new Mock<IComPlugin>();
            mockComPlugin.Setup(comPlugin => comPlugin.ComName).Returns(expectedComName);
            mockComPlugin.Setup(comPlugin => comPlugin.ClsId).Returns(expectedClsId);
            mockComPlugin.Setup(comPlugin => comPlugin.Is32Bit).Returns(expectedIs32Bit);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceID).Returns(expectedResourceID);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceName).Returns(expectedResourceName);

            var comPluginSourceDefinition = new ComPluginSourceDefinition(mockComPlugin.Object);

            object comPluginSource = comPluginSourceDefinition;

            var isEqual = comPluginSourceDefinition.Equals(comPluginSource);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ComPluginSourceDefinition))]
        public void ComPluginSourceDefinition_Equals_Object_Expected_False()
        {
            const string expectedComName = "testComName";
            const string expectedClsId = "testClsId";
            const bool expectedIs32Bit = false;
            var expectedResourceID = Guid.NewGuid();
            const string expectedResourceName = "testResource";

            var mockComPlugin = new Mock<IComPlugin>();
            mockComPlugin.Setup(comPlugin => comPlugin.ComName).Returns(expectedComName);
            mockComPlugin.Setup(comPlugin => comPlugin.ClsId).Returns(expectedClsId);
            mockComPlugin.Setup(comPlugin => comPlugin.Is32Bit).Returns(expectedIs32Bit);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceID).Returns(expectedResourceID);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceName).Returns(expectedResourceName);

            var comPluginSourceDefinition = new ComPluginSourceDefinition(mockComPlugin.Object);

            var mockComPluginDup = new Mock<IComPlugin>();
            mockComPluginDup.Setup(comPlugin => comPlugin.ComName).Returns("NewComName");
            mockComPluginDup.Setup(comPlugin => comPlugin.ClsId).Returns(expectedClsId);
            mockComPluginDup.Setup(comPlugin => comPlugin.Is32Bit).Returns(expectedIs32Bit);
            mockComPluginDup.Setup(comPlugin => comPlugin.ResourceID).Returns(expectedResourceID);
            mockComPluginDup.Setup(comPlugin => comPlugin.ResourceName).Returns(expectedResourceName);

            var comPluginSourceDefinitionDup = new ComPluginSourceDefinition(mockComPluginDup.Object);

            object comPluginSource = comPluginSourceDefinitionDup;

            var isEqual = comPluginSourceDefinition.Equals(comPluginSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ComPluginSourceDefinition))]
        public void ComPluginSourceDefinition_Equals_Object_GetType_Expected_False()
        {
            const string expectedComName = "testComName";
            const string expectedClsId = "testClsId";
            const bool expectedIs32Bit = false;
            var expectedResourceID = Guid.NewGuid();
            const string expectedResourceName = "testResource";

            var mockComPlugin = new Mock<IComPlugin>();
            mockComPlugin.Setup(comPlugin => comPlugin.ComName).Returns(expectedComName);
            mockComPlugin.Setup(comPlugin => comPlugin.ClsId).Returns(expectedClsId);
            mockComPlugin.Setup(comPlugin => comPlugin.Is32Bit).Returns(expectedIs32Bit);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceID).Returns(expectedResourceID);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceName).Returns(expectedResourceName);

            var comPluginSourceDefinition = new ComPluginSourceDefinition(mockComPlugin.Object);

            var comPluginSource = new object();

            var isEqual = comPluginSourceDefinition.Equals(comPluginSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ComPluginSourceDefinition))]
        public void ComPluginSourceDefinition_GetHashCode_Not_Equal_To_Zero()
        {
            const string expectedComName = "testComName";
            const string expectedClsId = "testClsId";
            const bool expectedIs32Bit = false;
            var expectedResourceID = Guid.NewGuid();
            const string expectedResourceName = "testResource";

            var mockComPlugin = new Mock<IComPlugin>();
            mockComPlugin.Setup(comPlugin => comPlugin.ComName).Returns(expectedComName);
            mockComPlugin.Setup(comPlugin => comPlugin.ClsId).Returns(expectedClsId);
            mockComPlugin.Setup(comPlugin => comPlugin.Is32Bit).Returns(expectedIs32Bit);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceID).Returns(expectedResourceID);
            mockComPlugin.Setup(comPlugin => comPlugin.ResourceName).Returns(expectedResourceName);

            var comPluginSourceDefinition = new ComPluginSourceDefinition(mockComPlugin.Object);

            var hashCode = comPluginSourceDefinition.GetHashCode();

            Assert.AreNotEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ComPluginSourceDefinition))]
        public void ComPluginSourceDefinition_GetHashCode_Expect_Zero()
        {
            var comPluginSourceDefinition = new ComPluginSourceDefinition();

            var hashCode = comPluginSourceDefinition.GetHashCode();

            Assert.AreEqual(0, hashCode);
        }
    }
}
