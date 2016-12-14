using System;
using System.Collections.Generic;

using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManagePluginSourceModelTests
    {
        #region Fields

        private Mock<IStudioUpdateManager> _updateRepositoryMock;
        private Mock<IQueryManager> _queryProxyMock;

        private string _serverName;

        private ManagePluginSourceModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _queryProxyMock = new Mock<IQueryManager>();
            _serverName = Guid.NewGuid().ToString();
            _target = new ManagePluginSourceModel(_updateRepositoryMock.Object, _queryProxyMock.Object, _serverName);
        }

        #endregion Test initialize

        #region Test methods

        [TestMethod]
        public void TestGetDllListings()
        {
            //arrange
            var listingMock = new Mock<IFileListing>();
            var expectedResult = new List<IFileListing>();
            _queryProxyMock.Setup(it => it.GetDllListings(listingMock.Object)).Returns(expectedResult);

            //act
            var result = _target.GetDllListings(listingMock.Object);

            //assert
            Assert.AreSame(expectedResult, result);
            _queryProxyMock.Verify(it => it.GetDllListings(listingMock.Object));
        }


        [TestMethod]
        public void TestSavePlugin()
        {
            //arrange
            var resourceMock = new Mock<IPluginSource>();

            //act
            _target.Save(resourceMock.Object);

            //assert
            _updateRepositoryMock.Verify(it => it.Save(resourceMock.Object));
        }

        #endregion Test methods

        #region Test properties

        [TestMethod]
        public void TestServerName()
        {
            //act
            var value = _target.ServerName;

            //assert
            Assert.AreEqual(_serverName, value);
        }

        [TestMethod]
        public void TestPluginSourceServerNameBrackets()
        {
            //arrange  
            _target = new ManagePluginSourceModel(_updateRepositoryMock.Object, _queryProxyMock.Object, _serverName + "(sthInBrackets)");

            //act
            var value = _target.ServerName;

            //assert
            Assert.AreEqual(_serverName, value);
        }

        #endregion Test properties
    }
}