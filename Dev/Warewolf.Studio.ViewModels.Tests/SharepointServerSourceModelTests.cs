using System;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class SharepointServerSourceModelTests
    {
        #region Fields

        Mock<IStudioUpdateManager> _updateRepositoryMock;

        string _serverName;

        SharepointServerSourceModel _target;
        Mock<IQueryManager> _queryManager;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _queryManager = new Mock<IQueryManager>();
            _serverName = Guid.NewGuid().ToString();
            _target = new SharepointServerSourceModel(_updateRepositoryMock.Object,_queryManager.Object, _serverName);
        }

        #endregion Test initialize

        #region Test methods

        [TestMethod]
        [Timeout(500)]
        public void Sharepoint_TestTestConnection()
        {
            //arrange
            var resourceMock = new Mock<ISharepointServerSource>();

            //act
            _target.TestConnection(resourceMock.Object);

            //assert
            _updateRepositoryMock.Verify(it => it.TestConnection(resourceMock.Object));
        }

        [TestMethod]
        [Timeout(250)]
        public void SharepointServerSourceTestSave()
        {
            //arrange
            var resourceMock = new Mock<ISharepointServerSource>();

            //act
            _target.Save(resourceMock.Object);

            //assert
            _updateRepositoryMock.Verify(it => it.Save(resourceMock.Object));
        }

        #endregion Test methods

        #region Test properties

        [TestMethod]
        [Timeout(250)]
        public void TestSharepointServerSourceModelServerName()
        {
            //arrange
            var expectedValue = "someValue";

            //act
            _target.ServerName = expectedValue;
            var value = _target.ServerName;

            //assert
            Assert.AreEqual(expectedValue, value);
        }

        #endregion Test properties
    }
}