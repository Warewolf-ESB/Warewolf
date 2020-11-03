using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class DatabaseSourceViewModelBaseTests
    {
        #region Fields

        Mock<IStudioUpdateManager> _updateRepositoryMock;
        Mock<IQueryManager> _queryProxyMock;

        string _serverName;

        ManageDatabaseSourceModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _queryProxyMock = new Mock<IQueryManager>();
            _serverName = Guid.NewGuid().ToString();
            _target = new ManageDatabaseSourceModel(_updateRepositoryMock.Object, _queryProxyMock.Object, _serverName);
        }

        #endregion Test initialize

        #region Test properties

        [TestMethod]
        [Timeout(250)]
        public void TestDatabaseSourceViewModelBaseServerName()
        {
            //act
            var value = _target.ServerName;

            //assert
            Assert.AreEqual(_serverName, value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestDatabaseSourceServerNameWithBrackets()
        {
            //arrange
            _target = new ManageDatabaseSourceModel(_updateRepositoryMock.Object, _queryProxyMock.Object, _serverName+"(word in brackets)");

            //act
            var value = _target.ServerName;

            //assert
            Assert.AreEqual(_serverName, value);
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        [Timeout(100)]
        public void TestGetComputerNames()
        {
            //arrange
            var expectedValue = new List<string>();
            _queryProxyMock.Setup(it => it.GetComputerNames()).Returns(expectedValue);

            //act
            var value = _target.GetComputerNames();

            //assert
            _queryProxyMock.Verify(it=>it.GetComputerNames());
            Assert.AreSame(expectedValue, value);
        }

        [TestMethod]
        [Timeout(250)]
        public void TestTestDbConnection()
        {
            //arrange
            var dbResourceMock = new Mock<IDbSource>();
            var expectedValue = new List<string>();
            _updateRepositoryMock.Setup(it => it.TestDbConnection(dbResourceMock.Object)).Returns(expectedValue);

            //act
            var value = _target.TestDbConnection(dbResourceMock.Object);

            //assert
           _updateRepositoryMock.Verify(it=>it.TestDbConnection(dbResourceMock.Object));
            Assert.AreSame(expectedValue, value);
        }
		[TestMethod]
        [Timeout(250)]
		public void TestSqliteConnection()
		{
			//arrange
			var dbResourceMock = new Mock<ISqliteDBSource>();
			var expectedValue = new List<string>();
			_updateRepositoryMock.Setup(it => it.TestSqliteConnection(dbResourceMock.Object)).Returns(expectedValue);

			//act
			var value = _target.TestSqliteConnection(dbResourceMock.Object);

			//assert
			_updateRepositoryMock.Verify(it => it.TestSqliteConnection(dbResourceMock.Object));
			Assert.AreSame(expectedValue, value);
		}
		[TestMethod]
        [Timeout(250)]
        public void TestSave()
        {
            //arrange
            var dbResourceMock = new Mock<IDbSource>();

            //act
            _target.Save(dbResourceMock.Object);

            //assert
            _updateRepositoryMock.Verify(it => it.Save(dbResourceMock.Object));
        }

        #endregion Test methods
    }
}