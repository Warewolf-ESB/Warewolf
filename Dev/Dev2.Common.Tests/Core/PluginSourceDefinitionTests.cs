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
using Dev2.Common.Interfaces.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.ObjectModel;

namespace Dev2.Common.Tests.Core
{
    [TestClass]
    public class PluginSourceDefinitionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginSourceDefinition))]
        public void PluginSourceDefinition_Validate_FileSystemAssemblyName()
        {
            const string expectedAssemblyLocation = "testAssemblyPath";
            const string expectedAssemblyName = "testAssemblyName";
            var expectedResourceID = Guid.NewGuid();
            const string expectedSavePath = "Path\\ResourcePath";
            const string expectedResourceName = "testResource";
            const string expectedConfigFilePath = "testConfigFilePath";

            var mockPlugin = new Mock<IPlugin>();
            mockPlugin.Setup(plugin => plugin.AssemblyLocation).Returns(expectedAssemblyLocation);
            mockPlugin.Setup(plugin => plugin.AssemblyName).Returns(expectedAssemblyName);
            mockPlugin.Setup(plugin => plugin.ResourceID).Returns(expectedResourceID);
            mockPlugin.Setup(plugin => plugin.GetSavePath()).Returns(expectedSavePath);
            mockPlugin.Setup(plugin => plugin.ResourceName).Returns(expectedResourceName);
            mockPlugin.Setup(plugin => plugin.ConfigFilePath).Returns(expectedConfigFilePath);

            var fileListing = new DllListing
            {
                FullName = expectedAssemblyLocation,
                Name = expectedAssemblyName,
                Children = new Collection<IFileListing>(),
                IsDirectory = false
            };

            var pluginSourceDefinition = new PluginSourceDefinition(mockPlugin.Object);

            Assert.AreEqual(fileListing, pluginSourceDefinition.SelectedDll);
            Assert.AreEqual(expectedResourceID, pluginSourceDefinition.Id);
            Assert.AreEqual(expectedSavePath, pluginSourceDefinition.Path);
            Assert.AreEqual(expectedResourceName, pluginSourceDefinition.Name);
            Assert.AreEqual(expectedConfigFilePath, pluginSourceDefinition.ConfigFilePath);
            Assert.AreEqual(expectedAssemblyLocation, pluginSourceDefinition.FileSystemAssemblyName);
            Assert.AreEqual(string.Empty, pluginSourceDefinition.GACAssemblyName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginSourceDefinition))]
        public void PluginSourceDefinition_Validate_GACAssemblyName()
        {
            const string expectedAssemblyLocation = "GAC:testAssemblyPath";
            const string expectedAssemblyName = "testAssemblyName";
            var expectedResourceID = Guid.NewGuid();
            const string expectedSavePath = "Path\\ResourcePath";
            const string expectedResourceName = "testResource";
            const string expectedConfigFilePath = "testConfigFilePath";

            var mockPlugin = new Mock<IPlugin>();
            mockPlugin.Setup(plugin => plugin.AssemblyLocation).Returns(expectedAssemblyLocation);
            mockPlugin.Setup(plugin => plugin.AssemblyName).Returns(expectedAssemblyName);
            mockPlugin.Setup(plugin => plugin.ResourceID).Returns(expectedResourceID);
            mockPlugin.Setup(plugin => plugin.GetSavePath()).Returns(expectedSavePath);
            mockPlugin.Setup(plugin => plugin.ResourceName).Returns(expectedResourceName);
            mockPlugin.Setup(plugin => plugin.ConfigFilePath).Returns(expectedConfigFilePath);

            var fileListing = new DllListing
            {
                FullName = expectedAssemblyLocation,
                Name = expectedAssemblyName,
                Children = new Collection<IFileListing>(),
                IsDirectory = false
            };

            var pluginSourceDefinition = new PluginSourceDefinition(mockPlugin.Object);

            Assert.AreEqual(fileListing, pluginSourceDefinition.SelectedDll);
            Assert.AreEqual(expectedResourceID, pluginSourceDefinition.Id);
            Assert.AreEqual(expectedSavePath, pluginSourceDefinition.Path);
            Assert.AreEqual(expectedResourceName, pluginSourceDefinition.Name);
            Assert.AreEqual(expectedConfigFilePath, pluginSourceDefinition.ConfigFilePath);
            Assert.AreEqual(string.Empty, pluginSourceDefinition.FileSystemAssemblyName);
            Assert.AreEqual(expectedAssemblyLocation, pluginSourceDefinition.GACAssemblyName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginSourceDefinition))]
        public void PluginSourceDefinition_Equals_PluginSource_Expected_True()
        {
            const string expectedAssemblyLocation = "testAssemblyPath";
            const string expectedAssemblyName = "testAssemblyName";
            var expectedResourceID = Guid.NewGuid();
            const string expectedSavePath = "Path\\ResourcePath";
            const string expectedResourceName = "testResource";
            const string expectedConfigFilePath = "testConfigFilePath";

            var mockPlugin = new Mock<IPlugin>();
            mockPlugin.Setup(plugin => plugin.AssemblyLocation).Returns(expectedAssemblyLocation);
            mockPlugin.Setup(plugin => plugin.AssemblyName).Returns(expectedAssemblyName);
            mockPlugin.Setup(plugin => plugin.ResourceID).Returns(expectedResourceID);
            mockPlugin.Setup(plugin => plugin.GetSavePath()).Returns(expectedSavePath);
            mockPlugin.Setup(plugin => plugin.ResourceName).Returns(expectedResourceName);
            mockPlugin.Setup(plugin => plugin.ConfigFilePath).Returns(expectedConfigFilePath);

            var pluginSourceDefinition = new PluginSourceDefinition(mockPlugin.Object);
            var pluginSourceDefinitionDup = new PluginSourceDefinition(mockPlugin.Object);

            var isEqual = pluginSourceDefinition.Equals(pluginSourceDefinitionDup);
            Assert.IsTrue(isEqual);
            Assert.IsTrue(pluginSourceDefinition == pluginSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginSourceDefinition))]
        public void PluginSourceDefinition_Equals_PluginSource_Expected_False()
        {
            const string expectedAssemblyLocation = "testAssemblyPath";
            const string expectedAssemblyName = "testAssemblyName";
            var expectedResourceID = Guid.NewGuid();
            const string expectedSavePath = "Path\\ResourcePath";
            const string expectedResourceName = "testResource";
            const string expectedConfigFilePath = "testConfigFilePath";

            var mockPlugin = new Mock<IPlugin>();
            mockPlugin.Setup(plugin => plugin.AssemblyLocation).Returns(expectedAssemblyLocation);
            mockPlugin.Setup(plugin => plugin.AssemblyName).Returns(expectedAssemblyName);
            mockPlugin.Setup(plugin => plugin.ResourceID).Returns(expectedResourceID);
            mockPlugin.Setup(plugin => plugin.GetSavePath()).Returns(expectedSavePath);
            mockPlugin.Setup(plugin => plugin.ResourceName).Returns(expectedResourceName);
            mockPlugin.Setup(plugin => plugin.ConfigFilePath).Returns(expectedConfigFilePath);

            var pluginSourceDefinition = new PluginSourceDefinition(mockPlugin.Object);

            var mockPluginDup = new Mock<IPlugin>();
            mockPluginDup.Setup(plugin => plugin.AssemblyLocation).Returns("NewLocation");
            mockPluginDup.Setup(plugin => plugin.AssemblyName).Returns(expectedAssemblyName);
            mockPluginDup.Setup(plugin => plugin.ResourceID).Returns(expectedResourceID);
            mockPluginDup.Setup(plugin => plugin.GetSavePath()).Returns(expectedSavePath);
            mockPluginDup.Setup(plugin => plugin.ResourceName).Returns(expectedResourceName);
            mockPluginDup.Setup(plugin => plugin.ConfigFilePath).Returns(expectedConfigFilePath);

            var pluginSourceDefinitionDup = new PluginSourceDefinition(mockPluginDup.Object);

            var isEqual = pluginSourceDefinition.Equals(pluginSourceDefinitionDup);
            Assert.IsFalse(isEqual);
            Assert.IsTrue(pluginSourceDefinition != pluginSourceDefinitionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginSourceDefinition))]
        public void PluginSourceDefinition_Equals_Object_Null_Expected_False()
        {
            var pluginSourceDefinition = new PluginSourceDefinition();

            const object pluginSource = null;

            var isEqual = pluginSourceDefinition.Equals(pluginSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginSourceDefinition))]
        public void PluginSourceDefinition_Equals_Object_Expected_True()
        {
            const string expectedAssemblyLocation = "testAssemblyPath";
            const string expectedAssemblyName = "testAssemblyName";
            var expectedResourceID = Guid.NewGuid();
            const string expectedSavePath = "Path\\ResourcePath";
            const string expectedResourceName = "testResource";
            const string expectedConfigFilePath = "testConfigFilePath";

            var mockPlugin = new Mock<IPlugin>();
            mockPlugin.Setup(plugin => plugin.AssemblyLocation).Returns(expectedAssemblyLocation);
            mockPlugin.Setup(plugin => plugin.AssemblyName).Returns(expectedAssemblyName);
            mockPlugin.Setup(plugin => plugin.ResourceID).Returns(expectedResourceID);
            mockPlugin.Setup(plugin => plugin.GetSavePath()).Returns(expectedSavePath);
            mockPlugin.Setup(plugin => plugin.ResourceName).Returns(expectedResourceName);
            mockPlugin.Setup(plugin => plugin.ConfigFilePath).Returns(expectedConfigFilePath);

            var pluginSourceDefinition = new PluginSourceDefinition(mockPlugin.Object);

            object pluginSource = pluginSourceDefinition;

            var isEqual = pluginSourceDefinition.Equals(pluginSource);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginSourceDefinition))]
        public void PluginSourceDefinition_Equals_Object_Expected_False()
        {
            const string expectedAssemblyLocation = "testAssemblyPath";
            const string expectedAssemblyName = "testAssemblyName";
            var expectedResourceID = Guid.NewGuid();
            const string expectedSavePath = "Path\\ResourcePath";
            const string expectedResourceName = "testResource";
            const string expectedConfigFilePath = "testConfigFilePath";

            var mockPlugin = new Mock<IPlugin>();
            mockPlugin.Setup(plugin => plugin.AssemblyLocation).Returns(expectedAssemblyLocation);
            mockPlugin.Setup(plugin => plugin.AssemblyName).Returns(expectedAssemblyName);
            mockPlugin.Setup(plugin => plugin.ResourceID).Returns(expectedResourceID);
            mockPlugin.Setup(plugin => plugin.GetSavePath()).Returns(expectedSavePath);
            mockPlugin.Setup(plugin => plugin.ResourceName).Returns(expectedResourceName);
            mockPlugin.Setup(plugin => plugin.ConfigFilePath).Returns(expectedConfigFilePath);

            var pluginSourceDefinition = new PluginSourceDefinition(mockPlugin.Object);

            var mockPluginDup = new Mock<IPlugin>();
            mockPluginDup.Setup(plugin => plugin.AssemblyLocation).Returns("NewLocation");
            mockPluginDup.Setup(plugin => plugin.AssemblyName).Returns(expectedAssemblyName);
            mockPluginDup.Setup(plugin => plugin.ResourceID).Returns(expectedResourceID);
            mockPluginDup.Setup(plugin => plugin.GetSavePath()).Returns(expectedSavePath);
            mockPluginDup.Setup(plugin => plugin.ResourceName).Returns(expectedResourceName);
            mockPluginDup.Setup(plugin => plugin.ConfigFilePath).Returns(expectedConfigFilePath);

            var pluginSourceDefinitionDup = new PluginSourceDefinition(mockPluginDup.Object);

            object pluginSource = pluginSourceDefinitionDup;

            var isEqual = pluginSourceDefinition.Equals(pluginSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginSourceDefinition))]
        public void PluginSourceDefinition_Equals_Object_GetType_Expected_False()
        {
            const string expectedAssemblyLocation = "testAssemblyPath";
            const string expectedAssemblyName = "testAssemblyName";
            var expectedResourceID = Guid.NewGuid();
            const string expectedSavePath = "Path\\ResourcePath";
            const string expectedResourceName = "testResource";
            const string expectedConfigFilePath = "testConfigFilePath";

            var mockPlugin = new Mock<IPlugin>();
            mockPlugin.Setup(plugin => plugin.AssemblyLocation).Returns(expectedAssemblyLocation);
            mockPlugin.Setup(plugin => plugin.AssemblyName).Returns(expectedAssemblyName);
            mockPlugin.Setup(plugin => plugin.ResourceID).Returns(expectedResourceID);
            mockPlugin.Setup(plugin => plugin.GetSavePath()).Returns(expectedSavePath);
            mockPlugin.Setup(plugin => plugin.ResourceName).Returns(expectedResourceName);
            mockPlugin.Setup(plugin => plugin.ConfigFilePath).Returns(expectedConfigFilePath);

            var pluginSourceDefinition = new PluginSourceDefinition(mockPlugin.Object);

            var pluginSource = new object();

            var isEqual = pluginSourceDefinition.Equals(pluginSource);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginSourceDefinition))]
        public void PluginSourceDefinition_GetHashCode_Not_Equal_To_Zero()
        {
            const string expectedAssemblyLocation = "testAssemblyPath";
            const string expectedAssemblyName = "testAssemblyName";
            var expectedResourceID = Guid.NewGuid();
            const string expectedSavePath = "Path\\ResourcePath";
            const string expectedResourceName = "testResource";
            const string expectedConfigFilePath = "testConfigFilePath";

            var mockPlugin = new Mock<IPlugin>();
            mockPlugin.Setup(plugin => plugin.AssemblyLocation).Returns(expectedAssemblyLocation);
            mockPlugin.Setup(plugin => plugin.AssemblyName).Returns(expectedAssemblyName);
            mockPlugin.Setup(plugin => plugin.ResourceID).Returns(expectedResourceID);
            mockPlugin.Setup(plugin => plugin.GetSavePath()).Returns(expectedSavePath);
            mockPlugin.Setup(plugin => plugin.ResourceName).Returns(expectedResourceName);
            mockPlugin.Setup(plugin => plugin.ConfigFilePath).Returns(expectedConfigFilePath);

            var pluginSourceDefinition = new PluginSourceDefinition(mockPlugin.Object);

            var hashCode = pluginSourceDefinition.GetHashCode();

            Assert.AreNotEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginSourceDefinition))]
        public void PluginSourceDefinition_GetHashCode_Expect_Zero()
        {
            var pluginSourceDefinition = new PluginSourceDefinition();

            var hashCode = pluginSourceDefinition.GetHashCode();

            Assert.AreEqual(0, hashCode);
        }
    }
}
