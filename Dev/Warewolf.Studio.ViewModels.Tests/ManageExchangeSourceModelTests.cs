using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageExchangeSourceModelTests
    {
        #region Fields

        Mock<IStudioUpdateManager> _updateRepositoryMock;
        Mock<IQueryManager> _queryProxyMock;

        string _serverName;

        ManageExchangeSourceModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _queryProxyMock = new Mock<IQueryManager>();
            _serverName = Guid.NewGuid().ToString();
            _target = new ManageExchangeSourceModel(_updateRepositoryMock.Object, _queryProxyMock.Object, _serverName);
        }

        #endregion Test initialize

        #region Test methods

        [TestMethod]
        [Timeout(500)]
        public void Exchange_TestTestConnection()
        {
            //arrange
            var resourceMock = new Mock<IExchangeSource>();

            //act
            _target.TestConnection(resourceMock.Object);

            //assert
            _updateRepositoryMock.Verify(it => it.TestConnection(resourceMock.Object));
        }

        [TestMethod]
        [Timeout(500)]
        public void ExchangeSourceTestSave()
        {
            //arrange
            var resourceMock = new Mock<IExchangeSource>();

            //act
            _target.Save(resourceMock.Object);

            //assert
            _updateRepositoryMock.Verify(it => it.Save(resourceMock.Object));
        }

        #endregion Test methods

        #region Test properties

        [TestMethod]
        [Timeout(100)]
        public void TestServerName()
        {
            //act
            var value = _target.ServerName;

            //assert
            Assert.AreEqual(_serverName, value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestEmailSourceServerNameBrackets()
        {
            //arrange  
            _target = new ManageExchangeSourceModel(_updateRepositoryMock.Object, _queryProxyMock.Object, _serverName+"(sthInBrackets)");
            
            //act
            var value = _target.ServerName;

            //assert
            Assert.AreEqual(_serverName, value);
        }

        #endregion Test properties
    }
}