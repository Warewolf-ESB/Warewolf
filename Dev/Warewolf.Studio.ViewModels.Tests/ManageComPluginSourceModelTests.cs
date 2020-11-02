using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageComPluginSourceModelTests
    {
        #region Fields

        Mock<IStudioUpdateManager> _updateRepositoryMock;
        Mock<IQueryManager> _queryProxyMock;

        string _serverName;

        ManageComPluginSourceModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _queryProxyMock = new Mock<IQueryManager>();
            _serverName = Guid.NewGuid().ToString();
            _target = new ManageComPluginSourceModel(_updateRepositoryMock.Object, _queryProxyMock.Object, _serverName);
        }

        #endregion Test initialize

        #region Test methods

        [TestMethod]
        [Timeout(250)]
        public void TestGetCOMDllListings()
        {
            //arrange
            var listingMock = new Mock<IFileListing>();
            var expectedResult = new List<IFileListing>();
            _queryProxyMock.Setup(it => it.GetComDllListings(listingMock.Object)).Returns(expectedResult);

            //act
            var result = _target.GetComDllListings(listingMock.Object);

            //assert
            Assert.AreSame(expectedResult, result);
            _queryProxyMock.Verify(it => it.GetComDllListings(listingMock.Object));
        }


        [TestMethod]
        [Timeout(100)]
        public void TestSavePlugin()
        {
            //arrange
            var resourceMock = new Mock<IComPluginSource>();

            //act
            _target.Save(resourceMock.Object);

            //assert
            _updateRepositoryMock.Verify(it => it.Save(resourceMock.Object));
        }

        #endregion Test methods

        #region Test properties

        [TestMethod]
        [Timeout(250)]
        public void TestComPluginSourceModelServerName()
        {
            //act
            var value = _target.ServerName;

            //assert
            Assert.AreEqual(_serverName, value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestComPluginSourceServerNameBrackets()
        {
            //arrange  
            _target = new ManageComPluginSourceModel(_updateRepositoryMock.Object, _queryProxyMock.Object, _serverName + "(sthInBrackets)");

            //act
            var value = _target.ServerName;

            //assert
            Assert.AreEqual(_serverName, value);
        }

        #endregion Test properties
    }
}