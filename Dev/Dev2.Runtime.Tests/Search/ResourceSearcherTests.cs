using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Search;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Search
{
    [TestClass]
    public class ResourceSearcherTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullResourceCatalog_ShouldThrowException()
        {
            var searcher = new ResourceSearcher(null);
        }

        [TestMethod]
        public void Constructor_ResourceCatalogTestCatalog_ExpectNoException()
        {
            var searcher = new ResourceSearcher(new Mock<IResourceCatalog>().Object);
            Assert.IsNotNull(searcher);
        }

        [TestMethod]
        public void GetSearchResults_WhenResourceNameHasValue_ShouldReturnResult()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockResource = new Mock<IResource>();
            mockResource.Setup(r => r.ResourceID).Returns(Guid.Empty);
            mockResource.Setup(r => r.ResourceName).Returns("Test Resource");
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("Folder");            
            mockResourceCatalog.Setup(res => res.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockResource.Object);
            mockResourceCatalog.Setup(res => res.GetResources(It.IsAny<Guid>())).Returns(new List<IResource>
            {
                mockResource.Object
            });
            var searchOptions = new SearchOptions();
            searchOptions.UpdateAllStates(false);
            searchOptions.IsWorkflowNameSelected = true;
            var searchValue = new Common.Search.Search
            {
                SearchInput = "Tes",
                SearchOptions = searchOptions
            };

            var variableListSearcher = new ResourceSearcher(mockResourceCatalog.Object);
            var searchResults = variableListSearcher.GetSearchResults(searchValue);
            Assert.AreEqual(1, searchResults.Count);
            var searchResult = searchResults[0];
            Assert.AreEqual(Guid.Empty, searchResult.ResourceId);
            Assert.AreEqual("Test Resource", searchResult.Match);
            Assert.AreEqual("Test Resource", searchResult.Name);
            Assert.AreEqual("Folder", searchResult.Path);
            Assert.AreEqual(Common.Interfaces.Search.SearchItemType.WorkflowName, searchResult.Type);
        }

    }
}
