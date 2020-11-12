using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageDbServiceModelTests
    {
        #region Fields

        ManageDbServiceModel _target;

        Guid _localhostServerEnvironmentId;
        Mock<IShellViewModel> _shellViewModelMock;
        Mock<IServer> _localhostServerMock;
        Mock<IWindowsGroupPermission> _windowsGroupPermissionMock;

        Mock<IStudioUpdateManager> _mockStudioUpdateManager;
        Mock<IQueryManager> _mockQueryManager;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void ManageDbServiceModel_TestInitialize()
        {
            _shellViewModelMock = new Mock<IShellViewModel>();
            _localhostServerEnvironmentId = Guid.NewGuid();
            _localhostServerMock = new Mock<IServer>();
            _localhostServerMock.Setup(it => it.EnvironmentID).Returns(_localhostServerEnvironmentId);
            _windowsGroupPermissionMock = new Mock<IWindowsGroupPermission>();
            _localhostServerMock.Setup(it => it.Permissions).Returns(new List<IWindowsGroupPermission>()
            {
                _windowsGroupPermissionMock.Object
            });
            //_localhostServerMock.Setup(it => it.GetServerConnections()).Returns(new List<IServer>());
            _localhostServerMock.SetupGet(it => it.DisplayName).Returns("localhostServerResourceName");
            _shellViewModelMock.SetupGet(it => it.LocalhostServer).Returns(_localhostServerMock.Object);

            _mockStudioUpdateManager = new Mock<IStudioUpdateManager>();
            _mockQueryManager = new Mock<IQueryManager>();

            _target = new ManageDbServiceModel(_mockStudioUpdateManager.Object, _mockQueryManager.Object, _shellViewModelMock.Object, _localhostServerMock.Object);
        }

        #endregion Test initialize

        [TestMethod]
        [Timeout(100)]
        public void ManageDbServiceModel_TestRetrieveSources()
        {
            var expectedResult = new ObservableCollection<IDbSource>();
            _mockQueryManager.Setup(it => it.FetchDbSources()).Returns(expectedResult);

            var result =_target.RetrieveSources();

            Assert.AreEqual(expectedResult.Count, result.Count);
            _mockQueryManager.Verify(it => it.FetchDbSources());
        }

        [TestMethod]
        [Timeout(250)]
        public void DbService_TestGetActions()
        {
            var expectedResult = new ObservableCollection<IDbAction>();
            var mockDbSource = new Mock<IDbSource>();
            _mockQueryManager.Setup(it => it.FetchDbActions(mockDbSource.Object)).Returns(expectedResult);

            var result =_target.GetActions(mockDbSource.Object);

            Assert.AreSame(expectedResult, result);
            _mockQueryManager.Verify(it => it.FetchDbActions(mockDbSource.Object));
        }

        [TestMethod]
        [Timeout(250)]
        public void ManageDbServiceModel_TestCreateNewSource()
        {
            _target.CreateNewSource(enSourceType.SqlDatabase);
            _target.CreateNewSource(enSourceType.MySqlDatabase);
            _target.CreateNewSource(enSourceType.PostgreSQL);
            _target.CreateNewSource(enSourceType.Oracle);
            _target.CreateNewSource(enSourceType.ODBC);
        }

        [TestMethod]
        [Timeout(250)]
        public void ManageDbServiceModel_TestEditSqlSource()
        {
            var mockSqlSource = new Mock<IDbSource>();
            mockSqlSource.Setup(source => source.DbName).Returns("SqlServer");
            mockSqlSource.Setup(source => source.ServerName).Returns("EditSqlServer");
            mockSqlSource.Setup(source => source.Type).Returns(enSourceType.SqlDatabase);

            _target.EditSource(mockSqlSource.Object, mockSqlSource.Object.Type);
        }

        [TestMethod]
        [Timeout(100)]
        public void ManageDbServiceModel_TestEditMySqlSource()
        {
            var mockSqlSource = new Mock<IDbSource>();
            mockSqlSource.Setup(source => source.DbName).Returns("MySql");
            mockSqlSource.Setup(source => source.ServerName).Returns("EditMySql");
            mockSqlSource.Setup(source => source.Type).Returns(enSourceType.MySqlDatabase);

            _target.EditSource(mockSqlSource.Object, mockSqlSource.Object.Type);
        }

        [TestMethod]
        [Timeout(100)]
        public void ManageDbServiceModel_TestEditPostgreSqlSource()
        {
            var mockSqlSource = new Mock<IDbSource>();
            mockSqlSource.Setup(source => source.DbName).Returns("PostgreSQL");
            mockSqlSource.Setup(source => source.ServerName).Returns("EditPostgreSQL");
            mockSqlSource.Setup(source => source.Type).Returns(enSourceType.PostgreSQL);

            _target.EditSource(mockSqlSource.Object, mockSqlSource.Object.Type);
        }

        [TestMethod]
        [Timeout(100)]
        public void ManageDbServiceModel_TestEditOracleSource()
        {
            var mockSqlSource = new Mock<IDbSource>();
            mockSqlSource.Setup(source => source.DbName).Returns("Oracle");
            mockSqlSource.Setup(source => source.ServerName).Returns("EditOracle");
            mockSqlSource.Setup(source => source.Type).Returns(enSourceType.Oracle);

            _target.EditSource(mockSqlSource.Object, mockSqlSource.Object.Type);
        }

        [TestMethod]
        [Timeout(100)]
        public void ManageDbServiceModel_TestEditOdbcSource()
        {
            var mockSqlSource = new Mock<IDbSource>();
            mockSqlSource.Setup(source => source.DbName).Returns("ODBC");
            mockSqlSource.Setup(source => source.ServerName).Returns("EditODBC");
            mockSqlSource.Setup(source => source.Type).Returns(enSourceType.ODBC);

            _target.EditSource(mockSqlSource.Object, mockSqlSource.Object.Type);
        }

        [TestMethod]
        [Timeout(500)]
        public void ManageDbServiceModel_TestDbService()
        {
            var mockDataTable = new Mock<IDatabaseService>();
            var expectedResult = new DataTable();
            _mockStudioUpdateManager.Setup(it => it.TestDbService(mockDataTable.Object)).Returns(expectedResult);

            
            var result = _target.TestService(mockDataTable.Object);

            Assert.AreSame(expectedResult, result);
            _mockStudioUpdateManager.Verify(it => it.TestDbService(mockDataTable.Object));
        }
    }
}
