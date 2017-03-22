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

        private Mock<IStudioUpdateManager> _updateRepositoryMock;
        private Mock<IQueryManager> _queryProxyMock;

        private string _serverName;

        private ManageExchangeSourceModel _target;

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
        public void TestTestConnection()
        {
            //arrange
            var resourceMock = new Mock<IExchangeSource>();

            //act
            _target.TestConnection(resourceMock.Object);

            //assert
            _updateRepositoryMock.Verify(it => it.TestConnection(resourceMock.Object));
        }

        [TestMethod]
        public void TestSave()
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
        public void TestServerName()
        {
            //act
            var value = _target.ServerName;

            //assert
            Assert.AreEqual(_serverName, value);
        }

        [TestMethod]
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