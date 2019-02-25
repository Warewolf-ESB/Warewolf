/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Search;
using Dev2.Data;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class SearchTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("Search")]
        public void Search_Ctor()
        {
            var searchValue = new Search.Search
            {
                SearchInput = "Set",
                SearchOptions = new SearchOptions()
            };
            Assert.IsNotNull(searchValue);
            Assert.IsFalse(searchValue.SearchOptions.IsAllSelected);
            Assert.AreEqual("Set", searchValue.SearchInput);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("Search")]
        public void Search_GetSearchResults()
        {
            var searchValue = new Search.Search
            {
                SearchInput = "Set",
                SearchOptions = new SearchOptions
                {
                    IsAllSelected = false,
                    IsTestNameSelected = true
                }
            };
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockResource = new Mock<IResource>();
            mockResource.Setup(r => r.ResourceID).Returns(Guid.Empty);
            mockResource.Setup(r => r.ResourceName).Returns("Test Resource");
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("Folder");
            mockResourceCatalog.Setup(res => res.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockResource.Object);
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

            var searchers = new List<ISearcher>
            {
                new TestSearcher(mockResourceCatalog.Object, mockTestCatalog.Object)
            };

            var searchResults = searchValue.GetSearchResults(searchers);

            Assert.AreEqual(2, searchResults.Count);
            var searchResult = searchResults[0];
            Assert.AreEqual(Guid.Empty, searchResult.ResourceId);
            Assert.AreEqual("Set Test", searchResult.Match);
            Assert.AreEqual("Test Resource", searchResult.Name);
            Assert.AreEqual("Folder", searchResult.Path);

            searchResult = searchResults[1];
            Assert.AreEqual(Guid.Empty, searchResult.ResourceId);
            Assert.AreEqual("Test set value", searchResult.Match);
            Assert.AreEqual("Test Resource", searchResult.Name);
            Assert.AreEqual("Folder", searchResult.Path);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("Search")]
        public void Search_SearchResult_Ctor()
        {
            var _resId = Guid.NewGuid();
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchVal = new SearchResult(_resId, _name, _path, SearchItemType.WorkflowName, _match);

            Assert.AreEqual(_resId, searchVal.ResourceId);
            Assert.AreEqual(_name, searchVal.Name);
            Assert.AreEqual(_path, searchVal.Path);
            Assert.AreEqual(_match, searchVal.Match);
            Assert.AreEqual(SearchItemType.WorkflowName, searchVal.Type);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("Search")]
        public void Search_SearchResult_Blank_Ctor()
        {
            var result = new SearchResult();
            Assert.IsNull(result.Match);
            Assert.IsNull(result.Name);
            Assert.IsNull(result.Path);
            Assert.AreEqual(Guid.Empty,result.ResourceId);
            Assert.AreEqual(SearchItemType.WorkflowName, result.Type);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("Search")]
        public void Search_SearchResult_Equals_ReturnFalse()
        {
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchVal = new SearchResult(Guid.NewGuid(), _name, _path, SearchItemType.WorkflowName, _match);
            var otherSearchVal = new SearchResult(Guid.NewGuid(), _name, _path, SearchItemType.WorkflowName, _match);
            Assert.IsFalse(searchVal.Equals(otherSearchVal));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("Search")]
        public void Search_SearchResult_Equals_ReturnTrue()
        {
            var _id = Guid.NewGuid();
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchVal = new SearchResult(_id, _name, _path, SearchItemType.WorkflowName, _match);
            var otherSearchVal = new SearchResult(_id, _name, _path, SearchItemType.WorkflowName, _match);
            Assert.IsTrue(searchVal.Equals(otherSearchVal));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("Search")]
        public void Search_SearchResult_Equals_OtherisNull_ReturnFalse()
        {
            var _id = Guid.NewGuid();
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchVal = new SearchResult(_id, _name, _path, SearchItemType.WorkflowName, _match);
            var otherSearchVal = new SearchResult();
            otherSearchVal = null;
            Assert.IsFalse(searchVal.Equals(otherSearchVal));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("Search")]
        public void Search_SearchResult_Equals_Object_ReturnFalse()
        {
            var _id = Guid.NewGuid();
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchVal = new SearchResult(_id, _name, _path, SearchItemType.WorkflowName, _match);
            var otherSearchVal = new object();
            Assert.IsFalse(searchVal.Equals(otherSearchVal));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("Search")]
        public void Search_SearchResult_Equals_Object_ReturnTrue()
        {
            var _id = Guid.NewGuid();
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchVal = new SearchResult(_id, _name, _path, SearchItemType.WorkflowName, _match);
            var other = new object();
            other = searchVal;
            Assert.IsTrue(searchVal.Equals(other));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("Search")]
        public void Search_SearchResult_GetHashCode()
        {
            var _id = Guid.NewGuid();
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchVal = new SearchResult(_id, _name, _path, SearchItemType.WorkflowName, _match);
           
            Assert.AreNotEqual(0,searchVal.GetHashCode());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("Search")]
        public void Search_SearchOptions_Set_Properties()
        {
            var searchValue = new Search.Search
            {
                SearchInput = "Hello World",
                SearchOptions = new SearchOptions
                {
                    IsAllSelected = false,
                    IsWorkflowNameSelected = true,
                    IsMatchCaseSelected = false,
                    IsMatchWholeWordSelected = true,
                    IsTestNameSelected = true,
                    IsScalarNameSelected = false,
                    IsObjectNameSelected = false,
                    IsRecSetNameSelected = false,
                    IsToolTitleSelected = false,
                    IsOutputVariableSelected = false,
                    IsInputVariableSelected = true
                }
            };
            Assert.IsTrue(searchValue.SearchOptions.IsVariableSelected);
            Assert.IsFalse(searchValue.SearchOptions.IsAllSelected);
            Assert.IsTrue(searchValue.SearchOptions.IsWorkflowNameSelected);
            Assert.IsFalse(searchValue.SearchOptions.IsMatchCaseSelected);
            Assert.IsTrue(searchValue.SearchOptions.IsMatchWholeWordSelected);
            Assert.IsTrue(searchValue.SearchOptions.IsTestNameSelected);
            Assert.IsFalse(searchValue.SearchOptions.IsScalarNameSelected);
            Assert.IsFalse(searchValue.SearchOptions.IsObjectNameSelected);
            Assert.IsFalse(searchValue.SearchOptions.IsRecSetNameSelected);
            Assert.IsFalse(searchValue.SearchOptions.IsToolTitleSelected);
            Assert.IsFalse(searchValue.SearchOptions.IsOutputVariableSelected);
            Assert.IsTrue(searchValue.SearchOptions.IsInputVariableSelected);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("Search")]
        public void Search_SearchOptions_IsVariableSelected_ExpectFalse()
        {
            var searchValue = new Search.Search
            {
                SearchInput = "Hello World",
                SearchOptions = new SearchOptions
                {
                    IsScalarNameSelected = false,
                    IsObjectNameSelected = false,
                    IsRecSetNameSelected = false,
                    IsToolTitleSelected = false,
                    IsOutputVariableSelected = false,
                    IsInputVariableSelected = false
                }
            };
            Assert.IsFalse(searchValue.SearchOptions.IsVariableSelected);
        }
    }
}
