/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ResourceCatalogImpl;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    [TestCategory(nameof(ResourceSaveProvider))]
    public class ResourceSaveProviderTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        public void ResourceSaveProvider_SaveResources_GivenSomethingFails_ShouldExpectFail()
        {
            var destinationPath = EnvironmentVariables.ResourcePath + "\\Dev2\\Tests\\ResourceCatalog\\Duplicate_Destination";
            var test_exceptionMessage = "test_message: false exception ResourceSaveProvider";

            var workspaceId = GlobalConstants.ServerWorkspaceID;
            var resourceID = Guid.NewGuid();
            var expected = new DbSource { ResourceID = resourceID, ResourceName = "TestSource", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase };

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResources(workspaceId))
                .Returns(new List<Common.Interfaces.Data.IResource>
                {
                    expected
                });
            mockResourceCatalog.Setup(o => o.Parse(workspaceId, expected))
                .Throws(new Exception(test_exceptionMessage));
            var mockServerVersionRepository = new Mock<IServerVersionRepository>();

            var resources = new List<DuplicateResourceTO>
            {
                new DuplicateResourceTO
                {
                    DestinationPath = destinationPath,
                    OldResourceID = Guid.Empty,
                    ResourceContents =  expected.ToStringBuilder(),
                    NewResource = expected
                }
            };

            var sut = new ResourceSaveProvider(mockResourceCatalog.Object, mockServerVersionRepository.Object);

            var result = sut.SaveResources(workspaceId, resources, true);

            Assert.IsNotNull(result);
            Assert.AreEqual($"The following error occurred while executing the save callback '{expected.ResourceID}'.' message {test_exceptionMessage}", result.Message);
            Assert.AreEqual(ExecStatus.Fail, result.Status);

            mockServerVersionRepository.Verify(o => o.StoreVersion(expected, string.Empty, string.Empty, workspaceId, destinationPath), Times.Once());
            mockResourceCatalog.Verify(o => o.AddToActivityCache(expected), Times.Once);
            mockResourceCatalog.Verify(o => o.RemoveFromResourceActivityCache(workspaceId, expected), Times.Once);
            mockResourceCatalog.Verify(o => o.Parse(workspaceId, expected), Times.Once);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        public void ResourceSaveProvider_SaveResources_GivenNothingFails_ShouldExpectSuccess()
        {
            var destinationPath = EnvironmentVariables.ResourcePath + "\\Dev2\\Tests\\ResourceCatalog\\Duplicate_Destination";

            var workspaceId = GlobalConstants.ServerWorkspaceID;
            var resourceID = Guid.NewGuid();
            var expected = new DbSource { ResourceID = resourceID, ResourceName = "TestSource", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase };
            
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResources(workspaceId))
                .Returns(new List<Common.Interfaces.Data.IResource> 
                {
                    expected
                });
            var mockServerVersionRepository = new Mock<IServerVersionRepository>();

            var resources = new List<DuplicateResourceTO>
            {
                new DuplicateResourceTO
                {
                    DestinationPath = destinationPath,
                    OldResourceID = Guid.Empty,
                    ResourceContents =  expected.ToStringBuilder(),
                    NewResource = expected
                }
            };

            var sut = new ResourceSaveProvider(mockResourceCatalog.Object, mockServerVersionRepository.Object);

            var result = sut.SaveResources(workspaceId, resources, true);

            Assert.IsNotNull(result);
            Assert.AreEqual("Updated DbSource 'TestSource'", result.Message);
            Assert.AreEqual(ExecStatus.Success, result.Status);

            mockServerVersionRepository.Verify(o => o.StoreVersion(expected, string.Empty, string.Empty, workspaceId, destinationPath), Times.Once());
            mockResourceCatalog.Verify(o => o.AddToActivityCache(expected), Times.Once);
            mockResourceCatalog.Verify(o => o.RemoveFromResourceActivityCache(workspaceId, expected), Times.Once);
            mockResourceCatalog.Verify(o => o.Parse(workspaceId, expected), Times.Once);
        }
    }
}
