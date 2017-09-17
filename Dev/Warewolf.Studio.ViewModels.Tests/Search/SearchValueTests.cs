using Dev2.Studio.Interfaces;
using Dev2.ViewModels.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Warewolf.Studio.ViewModels.Tests.Search
{
    [TestClass]
    public class SearchValueTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchValue_WhenSet_ShouldFirePropertyChanged()
        {
            var _resId = Guid.NewGuid();
            //------------Setup for test--------------------------
            var searchValue = CreateSearchValue(_resId);

            var _wasCalled = false;
            searchValue.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ResourceId")
                {
                    _wasCalled = true;
                }
            };

            Assert.IsNotNull(searchValue.ResourceId);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchValue_Name_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var _resId = Guid.NewGuid();
            var _name = "workflowName";
            var searchValue = CreateSearchValue(_resId, _name);

            var _wasCalled = false;
            searchValue.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Name")
                {
                    _wasCalled = true;
                }
            };
            
            Assert.AreEqual(_name, searchValue.Name);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchValue_Path_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var _resId = Guid.NewGuid();
            var _path = "resourcePath";
            var searchValue = CreateSearchValue(_resId, null, _path);

            var _wasCalled = false;
            searchValue.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Path")
                {
                    _wasCalled = true;
                }
            };
            
            Assert.AreEqual(_path, searchValue.Path);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchValue_Type_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var _resId = Guid.NewGuid();
            var _type = "Workflow";
            var searchValue = CreateSearchValue(_resId, null, null, _type);

            var _wasCalled = false;
            searchValue.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Type")
                {
                    _wasCalled = true;
                }
            };
            
            Assert.AreEqual(_type, searchValue.Type);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchValue_Match_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var _resId = Guid.NewGuid();
            var _match = "Input";
            var searchValue = CreateSearchValue(_resId, null, null, null, _match);

            var _wasCalled = false;
            searchValue.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Match")
                {
                    _wasCalled = true;
                }
            };
            
            Assert.AreEqual(_match, searchValue.Match);
            Assert.IsTrue(_wasCalled);
        }

        private static SearchValue CreateSearchValue(Guid _resId, string _name = null, string _path = null, string _type = null, string _match = null)
        {
            var _selectedEnvironment = new Mock<IEnvironmentViewModel>();
            _selectedEnvironment.Setup(p => p.DisplayName).Returns("someResName");

            return new SearchValue(_resId, _name, _path, _type, _match, _selectedEnvironment.Object);
        }
    }
}
