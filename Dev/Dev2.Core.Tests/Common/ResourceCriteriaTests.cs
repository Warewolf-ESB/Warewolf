/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Data;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ResourceCriteriaTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCriteria))]
        public void ResourceCriteria_ResourceID_SetProperty_Expect_SetValue()
        {
            //-----------------Arrange-------------------
            var resourceCriteria = new ResourceCriteria();

            var resourceID = Guid.NewGuid();
            //-----------------Act-----------------------
            resourceCriteria.ResourceID = resourceID;
            //-----------------Assert--------------------
            Assert.AreEqual(resourceID, resourceCriteria.ResourceID);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCriteria))]
        public void ResourceCriteria_ResourceName_SetProperty_Expect_SetValue()
        {
            //-----------------Arrange-------------------
            var resourceCriteria = new ResourceCriteria();

            var resourceName = "TestResourceName";
            //-----------------Act-----------------------
            resourceCriteria.ResourceName = resourceName;
            //-----------------Assert--------------------
            Assert.AreEqual(resourceName, resourceCriteria.ResourceName);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCriteria))]
        public void ResourceCriteria_ResourcePath_SetProperty_Expect_SetValue()
        {
            //-----------------Arrange-------------------
            var resourceCriteria = new ResourceCriteria();

            var resourcePath = "TestResourcePath";
            //-----------------Act-----------------------
            resourceCriteria.ResourcePath = resourcePath;
            //-----------------Assert--------------------
            Assert.AreEqual(resourcePath, resourceCriteria.ResourcePath);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCriteria))]
        public void ResourceCriteria_ResourceType_SetProperty_Expect_SetValue()
        {
            //-----------------Arrange-------------------
            var resourceCriteria = new ResourceCriteria();

            var resourceType = "TestResourceType";
            //-----------------Act-----------------------
            resourceCriteria.ResourceType = resourceType;
            //-----------------Assert--------------------
            Assert.AreEqual(resourceType, resourceCriteria.ResourceType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCriteria))]
        public void ResourceCriteria_VersionInfo_SetProperty_Expect_SetValue()
        {
            //-----------------Arrange-------------------
            var mockVersionInfo = new Mock<IVersionInfo>();

            var resourceCriteria = new ResourceCriteria();
            //-----------------Act-----------------------
            resourceCriteria.VersionInfo = mockVersionInfo.Object;
            //-----------------Assert--------------------
            Assert.AreEqual(mockVersionInfo.Object, resourceCriteria.VersionInfo);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCriteria))]
        public void ResourceCriteria_WorkspaceId_SetProperty_Expect_SetValue()
        {
            //-----------------Arrange-------------------
            var workspaceId = Guid.NewGuid();

            var resourceCriteria = new ResourceCriteria();
            //-----------------Act-----------------------
            resourceCriteria.WorkspaceId = workspaceId;
            //-----------------Assert--------------------
            Assert.AreEqual(workspaceId, resourceCriteria.WorkspaceId);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCriteria))]
        public void ResourceCriteria_IsUpgraded_SetProperty_Expect_SetValue()
        {
            //-----------------Arrange-------------------
            var isUpgraded = false;

            var resourceCriteria = new ResourceCriteria();
            //-----------------Act-----------------------
            resourceCriteria.IsUpgraded = isUpgraded;
            //-----------------Assert--------------------
            Assert.AreEqual(isUpgraded, resourceCriteria.IsUpgraded);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCriteria))]
        public void ResourceCriteria_IsNewResource_SetProperty_Expect_SetValue()
        {
            //-----------------Arrange-------------------
            var isNewResource = true;

            var resourceCriteria = new ResourceCriteria();
            //-----------------Act-----------------------
            resourceCriteria.IsNewResource = isNewResource;
            //-----------------Assert--------------------
            Assert.IsTrue(resourceCriteria.IsNewResource);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCriteria))]
        public void ResourceCriteria_GuidCsv_SetProperty_Expect_SetValue()
        {
            //-----------------Arrange-------------------
            var guidCsv = "TestGuidCsv";

            var resourceCriteria = new ResourceCriteria();
            //-----------------Act-----------------------
            resourceCriteria.GuidCsv = guidCsv;
            //-----------------Assert--------------------
            Assert.AreEqual(guidCsv, resourceCriteria.GuidCsv);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCriteria))]
        public void ResourceCriteria_FilePath_SetProperty_Expect_SetValue()
        {
            //-----------------Arrange-------------------
            var filePath = "TestFilePath";

            var resourceCriteria = new ResourceCriteria();
            //-----------------Act-----------------------
            resourceCriteria.FilePath = filePath;
            //-----------------Assert--------------------
            Assert.AreEqual(filePath, resourceCriteria.FilePath);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCriteria))]
        public void ResourceCriteria_FetchAll_SetProperty_Expect_SetValue()
        {
            //-----------------Arrange-------------------
            var fetchAll = true;

            var resourceCriteria = new ResourceCriteria();
            //-----------------Act-----------------------
            resourceCriteria.FetchAll = fetchAll;
            //-----------------Assert--------------------
            Assert.AreEqual(fetchAll, resourceCriteria.FetchAll);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCriteria))]
        public void ResourceCriteria_AuthorRoles_SetProperty_Expect_SetValue()
        {
            //-----------------Arrange-------------------
            var authorRoles = "testAuthorRoles";

            var resourceCriteria = new ResourceCriteria();
            //-----------------Act-----------------------
            resourceCriteria.AuthorRoles = authorRoles;
            //-----------------Assert--------------------
            Assert.AreEqual(authorRoles, resourceCriteria.AuthorRoles);
        }
    }
}
