using Dev2.Common.Interfaces.Data;
using Dev2.Common.Search;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dev2.Tests.Runtime.Search
{
    [TestClass]
    public class VariableListSearcherTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullResourceCatalog_ExpectException()
        {
            var variableListSearcher = new VariableListSearcher(null);
            Assert.IsNull(variableListSearcher);
        }

        [TestMethod]
        public void Constructor_ResourceCatalogTestCatalog_ExpectNoException()
        {
            var variableListSearcher = new VariableListSearcher(new Mock<IResourceCatalog>().Object);
            Assert.IsNotNull(variableListSearcher);
        }

        [TestMethod]
        public void GetSearchResults_WhenScalarNameHasValue_ShouldReturnResult()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockResource = new Mock<IResource>();
            mockResource.Setup(r => r.ResourceID).Returns(Guid.Empty);
            mockResource.Setup(r => r.ResourceName).Returns("Test Resource");
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("Folder");
            mockResource.Setup(r => r.DataList).Returns(new StringBuilder("<DataList><scalar1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><scalar2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><Recset Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" ><Field1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" /><Field2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" /></Recset></DataList>"));
            mockResourceCatalog.Setup(res => res.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockResource.Object);
            mockResourceCatalog.Setup(res => res.GetResources(It.IsAny<Guid>())).Returns(new List<IResource>
            {
                mockResource.Object
            });
            var searchValue = new SearchValue
            {
                SearchInput = "1",
                SearchOptions = new SearchOptions
                {
                    IsAllSelected = false,
                    IsToolTitleSelected = false,
                    IsScalarNameSelected = true,
                }
            };

            var variableListSearcher = new VariableListSearcher(mockResourceCatalog.Object);
            var searchResults = variableListSearcher.GetSearchResults(searchValue);
            Assert.AreEqual(1, searchResults.Count);
            var searchResult = searchResults[0];
            Assert.AreEqual(Guid.Empty, searchResult.ResourceId);
            Assert.AreEqual("scalar1", searchResult.Match);
            Assert.AreEqual("Test Resource", searchResult.Name);
            Assert.AreEqual("Folder", searchResult.Path);
            Assert.AreEqual(Common.Interfaces.Search.SearchItemType.Scalar, searchResult.Type);
        }


        [TestMethod]
        public void GetSearchResults_WhenScalarNameDoesNotHaveValue_ShouldNotReturnResult()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockResource = new Mock<IResource>();
            mockResource.Setup(r => r.ResourceID).Returns(Guid.Empty);
            mockResource.Setup(r => r.ResourceName).Returns("Test Resource");
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("Folder");
            mockResource.Setup(r => r.DataList).Returns(new StringBuilder("<DataList><scalar1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><scalar2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><Recset Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" ><Field1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" /><Field2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" /></Recset></DataList>"));
            mockResourceCatalog.Setup(res => res.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockResource.Object);
            mockResourceCatalog.Setup(res => res.GetResources(It.IsAny<Guid>())).Returns(new List<IResource>
            {
                mockResource.Object
            });
            var searchValue = new SearchValue
            {
                SearchInput = "bob",
                SearchOptions = new SearchOptions
                {
                    IsAllSelected = false,
                    IsToolTitleSelected = false,
                    IsScalarNameSelected = true,
                }
            };

            var variableListSearcher = new VariableListSearcher(mockResourceCatalog.Object);
            var searchResults = variableListSearcher.GetSearchResults(searchValue);
            Assert.AreEqual(0, searchResults.Count);            
        }

        [TestMethod]
        public void GetSearchResults_WhenRecsetNameHasValue_ShouldReturnResult()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockResource = new Mock<IResource>();
            mockResource.Setup(r => r.ResourceID).Returns(Guid.Empty);
            mockResource.Setup(r => r.ResourceName).Returns("Test Resource");
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("Folder");
            mockResource.Setup(r => r.DataList).Returns(new StringBuilder("<DataList><scalar1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><scalar2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><Recset Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" ><Field1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" /><Field2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" /></Recset></DataList>"));
            mockResourceCatalog.Setup(res => res.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockResource.Object);
            mockResourceCatalog.Setup(res => res.GetResources(It.IsAny<Guid>())).Returns(new List<IResource>
            {
                mockResource.Object
            });
            var searchValue = new SearchValue
            {
                SearchInput = "set",
                SearchOptions = new SearchOptions
                {
                    IsAllSelected = false,
                    IsToolTitleSelected = false,
                    IsRecSetNameSelected = true,
                }
            };

            var variableListSearcher = new VariableListSearcher(mockResourceCatalog.Object);
            var searchResults = variableListSearcher.GetSearchResults(searchValue);
            Assert.AreEqual(2, searchResults.Count);
            var searchResult = searchResults[0];
            Assert.AreEqual(Guid.Empty, searchResult.ResourceId);
            Assert.AreEqual("Recset", searchResult.Match);
            Assert.AreEqual("Test Resource", searchResult.Name);
            Assert.AreEqual("Folder", searchResult.Path);
            Assert.AreEqual(Common.Interfaces.Search.SearchItemType.RecordSet, searchResult.Type);
            searchResult = searchResults[1];
            Assert.AreEqual(Guid.Empty, searchResult.ResourceId);
            Assert.AreEqual("Recset", searchResult.Match);
            Assert.AreEqual("Test Resource", searchResult.Name);
            Assert.AreEqual("Folder", searchResult.Path);
            Assert.AreEqual(Common.Interfaces.Search.SearchItemType.RecordSet, searchResult.Type);
        }


        [TestMethod]
        public void GetSearchResults_WhenObjectNameHasValue_ShouldReturnResult()
        {
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockResource = new Mock<IResource>();
            mockResource.Setup(r => r.ResourceID).Returns(Guid.Empty);
            mockResource.Setup(r => r.ResourceName).Returns("Test Resource");
            mockResource.Setup(r => r.GetResourcePath(It.IsAny<Guid>())).Returns("Folder");
            mockResource.Setup(r => r.DataList).Returns(new StringBuilder("<DataList><scalar1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><scalar2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"Input\" /><Recset Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" ><Field1 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" /><Field2 Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" /></Recset><Person IsJson=\"true\" Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" ><Name Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" /><LastName Description=\"\" IsEditable=\"True\" " +
                                                                          "ColumnIODirection=\"None\" /></Person></DataList>"));
            mockResourceCatalog.Setup(res => res.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockResource.Object);
            mockResourceCatalog.Setup(res => res.GetResources(It.IsAny<Guid>())).Returns(new List<IResource>
            {
                mockResource.Object
            });
            var searchValue = new SearchValue
            {
                SearchInput = "per",
                SearchOptions = new SearchOptions
                {
                    IsAllSelected = false,
                    IsToolTitleSelected = false,
                    IsObjectNameSelected = true,
                }
            };

            var variableListSearcher = new VariableListSearcher(mockResourceCatalog.Object);
            var searchResults = variableListSearcher.GetSearchResults(searchValue);
            Assert.AreEqual(1, searchResults.Count);
            var searchResult = searchResults[0];
            Assert.AreEqual(Guid.Empty, searchResult.ResourceId);
            Assert.AreEqual("@Person", searchResult.Match);
            Assert.AreEqual("Test Resource", searchResult.Name);
            Assert.AreEqual("Folder", searchResult.Path);
            Assert.AreEqual(Common.Interfaces.Search.SearchItemType.Object, searchResult.Type);
            
        }
    }
}
