/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Factories;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.DynamicServices;
using Dev2.Runtime.ResourceCatalogImpl;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class ResourceLoadProviderTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void ResourceLoadProvider_FindByType_GivenWorkflowResource_ExpectWorkflowResource()
        {
            var workspaceResources = new ConcurrentDictionary<Guid, List<IResource>>();
            var id = GlobalConstants.ServerWorkspaceID;
            var expected = new Workflow();
            workspaceResources.GetOrAdd(id, (newId) => new List<IResource> { expected });
            var serverVersionRepository = new Mock<IServerVersionRepository>().Object;
            var managementServices = new List<DynamicService>();
            var provider = new ResourceLoadProvider(workspaceResources, serverVersionRepository, managementServices);


            var resources = provider.FindByType(typeof(Workflow).FullName);

            Assert.IsTrue(resources.Any(o => o == expected));

            var resources2 = provider.FindByType<Workflow>();

            Assert.IsTrue(resources2.Any(o => o == expected));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void ResourceLoadProvider_GetResourceContents_Given_ResourceFilePathIsNullOrEmpty_ExpectEmptyResourceContents()
        {
            CustomContainer.Register(new Mock<IWarewolfPerformanceCounterLocater>().Object);
            var workspaceResources = new ConcurrentDictionary<Guid, List<IResource>>();
            var id = GlobalConstants.ServerWorkspaceID;
            var expectedResource = new Workflow
            {
                FilePath = string.Empty
            };
            workspaceResources.GetOrAdd(id, (newId) => new List<IResource> { expectedResource });
            var serverVersionRepository = new Mock<IServerVersionRepository>().Object;
            var managementServices = new List<DynamicService>();

            var sut = new ResourceLoadProvider(workspaceResources, serverVersionRepository, managementServices);

            var result = sut.GetResourceContents(expectedResource);

            Assert.AreEqual(new StringBuilder().ToString(), result.ToString());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void ResourceLoadProvider_GetResourceContents_Given_ResourceFilePathIsNotExist_ExpectEmptyResourceContents()
        {
            CustomContainer.Register(new Mock<IWarewolfPerformanceCounterLocater>().Object);
            var workspaceResources = new ConcurrentDictionary<Guid, List<IResource>>();
            var id = GlobalConstants.ServerWorkspaceID;
            var filePath = "test/new/file-path";

            var mockDev2FileWrapper = new Mock<IFile>();
            mockDev2FileWrapper.Setup(o => o.Exists(filePath))
                .Returns(false);

            var mockResource = new Mock<IResource>();
            mockResource.Setup(o => o.FilePath)
                .Returns(filePath);
            var expectedResource = mockResource.Object;

            workspaceResources.GetOrAdd(id, (newId) => new List<IResource> { expectedResource });
            var serverVersionRepository = new Mock<IServerVersionRepository>().Object;
            var managementServices = new List<DynamicService>();

            var sut = new ResourceLoadProvider(workspaceResources, serverVersionRepository, dev2FileWrapper: mockDev2FileWrapper.Object);

            var result = sut.GetResourceContents(expectedResource);

            Assert.AreEqual(new StringBuilder().ToString(), result.ToString());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void ResourceLoadProvider_GetResourceContents_Given_ResourceFilePathIsExist_ExpectNonEmptyResourceContents()
        {
            CustomContainer.Register(new Mock<IWarewolfPerformanceCounterLocater>().Object);
            var workspaceResources = new ConcurrentDictionary<Guid, List<IResource>>();
            var id = GlobalConstants.ServerWorkspaceID;
            var filePath = "test/new/file-path";
            var data = "test data message";
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var frMemoryStream = new MemoryStream();
            var srMemoryStream = new MemoryStream(dataBytes);
            var srStreamReader = new StreamReader(srMemoryStream);

            var fileStreamArgs = new FileStreamArgs
            {
                FileAccess = FileAccess.Read,
                FileShare = FileShare.Read,
                FileMode = FileMode.Open,
                FilePath = filePath,
                IsAsync = true,
            };

            var mockFileStreamFactory = new Mock<IFileStreamFactory>();
            mockFileStreamFactory.Setup(o => o.New(fileStreamArgs))
                .Returns(frMemoryStream);

            var mockStreamReaderWrapper = new Mock<IStreamReaderWrapper>();
            mockStreamReaderWrapper.Setup(o => o.GetStream(It.IsAny<Stream>()))
                .Returns(srStreamReader);

            var mockStreamReaderFactory = new Mock<IStreamReaderFactory>();
            mockStreamReaderFactory.Setup(o => o.New())
                .Returns(mockStreamReaderWrapper.Object);

            var mockDev2FileWrapper = new Mock<IFile>();
            mockDev2FileWrapper.Setup(o => o.Exists(filePath))
                .Returns(true);
            
            var mockResource = new Mock<IResource>();
            mockResource.Setup(o => o.FilePath)
                .Returns(filePath);
            var expectedResource = mockResource.Object;

            workspaceResources.GetOrAdd(id, (newId) => new List<IResource> { expectedResource });
            var serverVersionRepository = new Mock<IServerVersionRepository>().Object;

            var sut = new ResourceLoadProvider(workspaceResources, serverVersionRepository, dev2FileWrapper: mockDev2FileWrapper.Object, fileStreamFactory: mockFileStreamFactory.Object, streamReaderFactory: mockStreamReaderFactory.Object);

            var result = sut.GetResourceContents(expectedResource);

            StringAssert.Contains(result.ToString(), data);
        }
    }
}
