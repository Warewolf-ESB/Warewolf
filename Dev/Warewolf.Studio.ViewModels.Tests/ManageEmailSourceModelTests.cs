using System;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageEmailSourceModelTests
    {
        #region Fields

        Mock<IStudioUpdateManager> _updateRepositoryMock;
        Mock<IQueryManager> _queryProxyMock;

        string _serverName;

        ManageEmailSourceModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _queryProxyMock = new Mock<IQueryManager>();
            _serverName = Guid.NewGuid().ToString();
            _target = new ManageEmailSourceModel(_updateRepositoryMock.Object, _queryProxyMock.Object, _serverName);
        }

        #endregion Test initialize

        #region Test methods

        [TestMethod]
        [Timeout(250)]
        public void EmailSourceTestTestConnection()
        {
            //arrange
            var resourceMock = new Mock<IEmailServiceSource>();

            //act
            _target.TestConnection(resourceMock.Object);

            //assert
            _updateRepositoryMock.Verify(it => it.TestConnection(resourceMock.Object));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSave()
        {
            //arrange
            var resourceMock = new Mock<IEmailServiceSource>();

            //act
            _target.Save(resourceMock.Object);

            //assert
            _updateRepositoryMock.Verify(it => it.Save(resourceMock.Object));
        }

        #endregion Test methods

        #region Test properties

        [TestMethod]
        [Timeout(100)]
        public void TestEmailSourceModelServerName()
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
            _target = new ManageEmailSourceModel(_updateRepositoryMock.Object, _queryProxyMock.Object, _serverName+"(sthInBrackets)");
            
            //act
            var value = _target.ServerName;

            //assert
            Assert.AreEqual(_serverName, value);
        }

        #endregion Test properties
    }
}