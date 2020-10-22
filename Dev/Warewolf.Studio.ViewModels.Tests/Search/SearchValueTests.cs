using Dev2.Common.Interfaces.Search;
using Dev2.Common.Search;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Warewolf.Studio.ViewModels.Tests.Search
{
    [TestClass]
    public class SearchValueTests
    {
        [TestMethod]
        [Timeout(100)]
        [Owner("Pieter Terblanche")]
        public void SearchValue_OpenResourced_WorkflowType()
        {
            //------------Setup for test--------------------------
            var _resId = Guid.NewGuid();
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchValue = CreateSearchValue(_resId, _name, _path, _match);

            Assert.AreEqual(SearchItemType.WorkflowName, searchValue.Type);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Pieter Terblanche")]
        public void SearchValue_OpenResourced_TestType()
        {
            //------------Setup for test--------------------------
            var _resId = Guid.NewGuid();
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchValue = CreateSearchValue(_resId, _name, _path, _match);
            searchValue.Type = SearchItemType.TestName;

            Assert.AreEqual(SearchItemType.TestName, searchValue.Type);
        }

        private static SearchResult CreateSearchValue(Guid _resId, string _name = null, string _path = null, string _match = null)
        {
            var _selectedEnvironment = new Mock<IEnvironmentViewModel>();
            _selectedEnvironment.Setup(p => p.DisplayName).Returns("someResName");

            return new SearchResult(_resId, _name, _path, SearchItemType.WorkflowName, _match);
        }
    }
}
