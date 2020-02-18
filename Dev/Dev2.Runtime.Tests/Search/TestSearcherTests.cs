using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Search;
using Dev2.Data;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Tests.Runtime.Search
{
    [TestClass]
    public class TestSearcherTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullResourceCatalog_ExpectException()
        {
            var testSearcher = new TestSearcher(null, new Mock<ITestCatalog>().Object);
            Assert.IsNull(testSearcher);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullTestCatalog_ExpectException()
        {
            var testSearcher = new TestSearcher(new Mock<IResourceCatalog>().Object, null);
            Assert.IsNull(testSearcher);
        }

        [TestMethod]
        public void Constructor_ResourceCatalogTestCatalog_ExpectNoException()
        {
            var testSearcher = new TestSearcher(new Mock<IResourceCatalog>().Object, new Mock<ITestCatalog>().Object);
            Assert.IsNotNull(testSearcher);
        }

        [TestMethod]
        public void GetSearchResults_WhenTestNameHasValue_ShouldReturnResult()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockResource = new Mock<IResource>();
            mockResource.Setup(r => r.ResourceID).Returns(Guid.Empty);
            mockResource.Setup(r => r.ResourceName).Returns("Test Resource");
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("Folder");
            mockResourceCatalog.Setup(res => res.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockResource.Object);
            
            var searchValue = new Common.Search.Search
            {
                SearchInput = "Set",
                SearchOptions = new SearchOptions
                {
                    IsAllSelected = false,
                    IsTestNameSelected = true
                }
            };

            var mockTestCatalog = new Mock<ITestCatalog>();
            mockTestCatalog.Setup(t => t.FetchAllTests()).Returns(new List<IServiceTestModelTO>
            {
                 new ServiceTestModelTO
                 {
                     TestName = "Bob Test"
                 },
                 new ServiceTestModelTO
                 {
                     TestName = "Set Test"
                 }
                 ,
                 new ServiceTestModelTO
                 {
                     TestName = "Test set value"
                 }
            });
            var searcher = new TestSearcher(mockResourceCatalog.Object,mockTestCatalog.Object);
            var searchResults = searcher.GetSearchResults(searchValue);
            Assert.AreEqual(2, searchResults.Count);
            var searchResult = searchResults[0];
            Assert.AreEqual(Guid.Empty, searchResult.ResourceId);
            Assert.AreEqual("Set Test", searchResult.Match);
            Assert.AreEqual("Test Resource", searchResult.Name);
            Assert.AreEqual("Folder", searchResult.Path);
            Assert.AreEqual(Common.Interfaces.Search.SearchItemType.TestName, searchResult.Type);
            searchResult = searchResults[1];
            Assert.AreEqual(Guid.Empty, searchResult.ResourceId);
            Assert.AreEqual("Test set value", searchResult.Match);
            Assert.AreEqual("Test Resource", searchResult.Name);
            Assert.AreEqual("Folder", searchResult.Path);
            Assert.AreEqual(Common.Interfaces.Search.SearchItemType.TestName, searchResult.Type);
        }


        [TestMethod]
        public void GetSearchResults_WhenTestNameHasValueAndResourceDoesNotExists_ShouldReturnEmptyResult()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(res => res.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(() => null);

            var searchValue = new Common.Search.Search
            {
                SearchInput = "Set",
                SearchOptions = new SearchOptions
                {
                    IsAllSelected = false,
                    IsTestNameSelected = true
                }
            };

            var mockTestCatalog = new Mock<ITestCatalog>();
            mockTestCatalog.Setup(t => t.FetchAllTests()).Returns(new List<IServiceTestModelTO>
            {
                 new ServiceTestModelTO
                 {
                     TestName = "Set Test"
                 }
                 ,
                 new ServiceTestModelTO
                 {
                     TestName = "Test set value"
                 }
            });
            var searcher = new TestSearcher(mockResourceCatalog.Object, mockTestCatalog.Object);
            var searchResults = searcher.GetSearchResults(searchValue);
            Assert.AreEqual(0, searchResults.Count);
        }
    }
}
