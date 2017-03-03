using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageWebServiceModelTests
    {
        #region Fields

        private ManageWebServiceModel _target;

        private Guid _localhostServerEnvironmentId;
        private Mock<IShellViewModel> _shellViewModelMock;
        private Mock<IServer> _localhostServerMock;
        private Mock<IWindowsGroupPermission> _windowsGroupPermissionMock;

        private Mock<IStudioUpdateManager> _mockStudioUpdateManager;
        private Mock<IQueryManager> _mockQueryManager;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
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

            _target = new ManageWebServiceModel(_mockStudioUpdateManager.Object, _mockQueryManager.Object, _shellViewModelMock.Object, _localhostServerMock.Object);
        }

        #endregion Test initialize

        [TestMethod]
        public void TestRetrieveSources()
        {
            var expectedResult = new ObservableCollection<IWebServiceSource>();
            _mockQueryManager.Setup(it => it.FetchWebServiceSources()).Returns(expectedResult);

            var result = _target.RetrieveSources();

            Assert.AreEqual(expectedResult.Count, result.Count);
            _mockQueryManager.Verify(it => it.FetchWebServiceSources());
        }

        [TestMethod]
        public void TestCreateNewSource()
        {
            try
            {
                _target.CreateNewSource();
            }
            catch (Exception ex)
            {
                Assert.Fail("Create new web source failed. Exception: " + ex.Message);
            }
        }

        [TestMethod]
        public void TestEditWebServiceSource()
        {
            var mockWebServiceSource = new Mock<IWebServiceSource>();
            mockWebServiceSource.Setup(source => source.Name).Returns("WebSource");
            mockWebServiceSource.Setup(source => source.HostName).Returns("WebSourceHostName");

            _target.EditSource(mockWebServiceSource.Object);
        }

        [TestMethod]
        public void TestService()
        {
            var mockWebServiceSourceValue = new Mock<IWebService>();
            var expectedResult = string.Empty;
            _mockStudioUpdateManager.Setup(it => it.TestWebService(mockWebServiceSourceValue.Object)).Returns(expectedResult);

            var result = _target.TestService(mockWebServiceSourceValue.Object);

            Assert.AreSame(expectedResult, result);
            _mockStudioUpdateManager.Verify(it => it.TestWebService(mockWebServiceSourceValue.Object));
        }
    }
}
