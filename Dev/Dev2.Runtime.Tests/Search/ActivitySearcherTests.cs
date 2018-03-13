using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Search;
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
            var searchResults = searcher.GetSearchResults(searchValue);
            Assert.AreEqual(1, searchResults.Count);
            var searchResult = searchResults[0];
            Assert.AreEqual(Guid.Empty, searchResult.ResourceId);
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


        [TestMethod]
        public void GetSearchResults_WhenComplexFlowHaveValue_ShouldReturnResult()
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
            var firstFlow = new TestActivity();
            firstFlow.DisplayName = "Start Tool";
            firstFlow.NextNodes = new List<IDev2Activity>
            {
                 new TestActivity
                 {
                     DisplayName = "Set a value",
                     NextNodes = new List<IDev2Activity>
                     {
                         new TestActivity
                         {
                             DisplayName = "Set Bob Name"
                         },
                         new TestActivity
                         {
                             DisplayName = "Retrive",
                             NextNodes = new List<IDev2Activity>
                             {
                                 new TestActivity
                                 {
                                     DisplayName = "Get Bob Name"
                                 }
                             }
                         }
                     }
                 }
            };
            var secondFlow = new TestActivity();
            secondFlow.DisplayName = "Start Tool";
            secondFlow.NextNodes = new List<IDev2Activity>
            {
                 new TestActivity
                 {
                     DisplayName = "Set a value"
                 }
            };
            cache.TryAdd(Guid.Empty, firstFlow);
            cache.TryAdd(Guid.NewGuid(), secondFlow);
            mockResourceActivityCache.Setup(c => c.Cache).Returns(cache);
            mockResourceCatalog.Setup(res => res.GetResourceActivityCache(It.IsAny<Guid>())).Returns(mockResourceActivityCache.Object);
            var searchResults = searcher.GetSearchResults(searchValue);
            Assert.AreEqual(2, searchResults.Count);
            Assert.AreEqual(Guid.Empty, searchResults[0].ResourceId);
            Assert.AreEqual("Get Bob Name", searchResults[0].Match);
            Assert.AreEqual(Guid.Empty, searchResults[1].ResourceId);
            Assert.AreEqual("Set Bob Name", searchResults[1].Match);
        }


        [TestMethod]
        public void GetSearchResults_WhenMatchInTwoResources_ShouldReturnResult()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var mockResource = new Mock<IResource>();
            mockResource.Setup(r => r.ResourceID).Returns(Guid.Empty);
            mockResource.Setup(r => r.ResourceName).Returns("Test Resource");
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("Folder");

            var otherResourceId = Guid.NewGuid();
            var mockResource2 = new Mock<IResource>();
            mockResource2.Setup(r => r.ResourceID).Returns(otherResourceId);
            mockResource2.Setup(r => r.ResourceName).Returns("Test Resource 2");
            mockResource2.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("Folder");

            mockResourceCatalog.Setup(res => res.GetResource(It.IsAny<Guid>(), Guid.Empty)).Returns(mockResource.Object);
            mockResourceCatalog.Setup(res => res.GetResource(It.IsAny<Guid>(), otherResourceId)).Returns(mockResource2.Object);

            var searcher = new ActivitySearcher(mockResourceCatalog.Object);
            var searchValue = new SearchValue
            {
                SearchInput = "Bob",
                SearchOptions = new SearchOptions
                {
                    IsAllSelected = false,
                    IsToolTitleSelected = true,
                }
            };
            var mockResourceActivityCache = new Mock<IResourceActivityCache>();
            var cache = new System.Collections.Concurrent.ConcurrentDictionary<Guid, IDev2Activity>();
            var firstFlow = new TestActivity();
            firstFlow.DisplayName = "Start Tool";
            firstFlow.NextNodes = new List<IDev2Activity>
            {
                 new TestActivity
                 {
                     DisplayName = "Set a value",
                     NextNodes = new List<IDev2Activity>
                     {
                         new TestActivity
                         {
                             DisplayName = "Set Bob Name"
                         },
                         new TestActivity
                         {
                             DisplayName = "Retrive",
                             NextNodes = new List<IDev2Activity>
                             {
                                 new TestActivity
                                 {
                                     DisplayName = "Get Bob Name"
                                 }
                             }
                         }
                     }
                 }
            };
            var secondFlow = new TestActivity();
            secondFlow.DisplayName = "Start Tool";
            secondFlow.NextNodes = new List<IDev2Activity>
            {
                 new TestActivity
                 {
                     DisplayName = "What's bobs name"
                 }
            };
            cache.TryAdd(Guid.Empty, firstFlow);
            cache.TryAdd(otherResourceId, secondFlow);
            mockResourceActivityCache.Setup(c => c.Cache).Returns(cache);
            mockResourceCatalog.Setup(res => res.GetResourceActivityCache(It.IsAny<Guid>())).Returns(mockResourceActivityCache.Object);
            var searchResults = searcher.GetSearchResults(searchValue);
            Assert.AreEqual(3, searchResults.Count);
            Assert.AreEqual(Guid.Empty, searchResults[0].ResourceId);
            Assert.AreEqual("Get Bob Name", searchResults[0].Match);
            Assert.AreEqual(Guid.Empty, searchResults[1].ResourceId);
            Assert.AreEqual("Set Bob Name", searchResults[1].Match);
            Assert.AreEqual(otherResourceId, searchResults[2].ResourceId);
            Assert.AreEqual("What's bobs name", searchResults[2].Match);
        }
    }
}
