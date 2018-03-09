using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Search;
using Dev2.Tests.Activities.ActivityTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.ResourceManagement;

namespace Dev2.Tests.Runtime.Search
{
    [TestClass]
    public class ActivitySearcherTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullResourceCatalog_ShouldThrowException()
        {
            var searcher = new ActivitySearcher(null);
        }

        [TestMethod]        
        public void GetSearchResults_WhenToolTitleHasValue_ShouldReturnResult()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockResource = new Mock<IResource>();
            mockResource.Setup(r => r.ResourceID).Returns(Guid.Empty);
            mockResource.Setup(r => r.ResourceName).Returns("Test Resource");
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("Folder");
            mockResourceCatalog.Setup(res => res.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockResource.Object);
            var searcher = new ActivitySearcher(mockResourceCatalog.Object);
            var searchValue = new SearchValue
            {
                SearchInput = "Set",
                SearchOptions = new SearchOptions
                {
                    IsAllSelected = false,
                    IsToolTitleSelected = true                    
                }
            };
            var mockResourceActivityCache = new Mock<IResourceActivityCache>();
            var cache = new System.Collections.Concurrent.ConcurrentDictionary<Guid, IDev2Activity>();
            var startAct = new TestActivity();
            startAct.DisplayName = "Start Tool";
            startAct.NextNodes = new List<IDev2Activity>
            {
                 new TestActivity
                 {
                     DisplayName = "Set a value"
                 }
            };
            cache.TryAdd(Guid.Empty, startAct);
            mockResourceActivityCache.Setup(c => c.Cache).Returns(cache);
            mockResourceCatalog.Setup(res => res.GetResourceActivityCache(It.IsAny<Guid>())).Returns(mockResourceActivityCache.Object);
            var searchResults  = searcher.GetSearchResults(searchValue);
            Assert.AreEqual(1, searchResults.Count);
            var searchResult = searchResults[0];
            Assert.AreEqual("Set a value", searchResult.Match);
            Assert.AreEqual("Test Resource", searchResult.Name);
            Assert.AreEqual("Folder", searchResult.Path);
            Assert.AreEqual(Common.Interfaces.Search.SearchItemType.ToolTitle, searchResult.Type);
        }

        [TestMethod]
        public void GetSearchResults_WhenToolTitleDoesNotHaveValue_ShouldNotReturnResult()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockResource = new Mock<IResource>();
            mockResource.Setup(r => r.ResourceID).Returns(Guid.Empty);
            mockResource.Setup(r => r.ResourceName).Returns("Test Resource");
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("Folder");
            mockResourceCatalog.Setup(res => res.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockResource.Object);
            var searcher = new ActivitySearcher(mockResourceCatalog.Object);
            var searchValue = new SearchValue
            {
                SearchInput = "Bob",
                SearchOptions = new SearchOptions
                {
                    IsAllSelected = false,
                    IsToolTitleSelected = true
                }
            };
            var mockResourceActivityCache = new Mock<IResourceActivityCache>();
            var cache = new System.Collections.Concurrent.ConcurrentDictionary<Guid, IDev2Activity>();
            var startAct = new TestActivity();
            startAct.DisplayName = "Start Tool";
            startAct.NextNodes = new List<IDev2Activity>
            {
                 new TestActivity
                 {
                     DisplayName = "Set a value"
                 }
            };
            cache.TryAdd(Guid.Empty, startAct);
            mockResourceActivityCache.Setup(c => c.Cache).Returns(cache);
            mockResourceCatalog.Setup(res => res.GetResourceActivityCache(It.IsAny<Guid>())).Returns(mockResourceActivityCache.Object);
            var searchResults = searcher.GetSearchResults(searchValue);
            Assert.AreEqual(0, searchResults.Count);            
        }
    }
}
