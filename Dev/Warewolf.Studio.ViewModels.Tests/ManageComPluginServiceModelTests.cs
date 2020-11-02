using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageComPluginServiceModelTests
    {
        #region Fields

        ManageComPluginServiceModel _target;

        Guid _localhostServerEnvironmentId;
        Mock<IShellViewModel> _shellViewModelMock;
        Mock<IServer> _localhostServerMock;
        Mock<IWindowsGroupPermission> _windowsGroupPermissionMock;

        Mock<IStudioUpdateManager> _mockStudioUpdateManager;
        Mock<IQueryManager> _mockQueryManager;

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

            _target = new ManageComPluginServiceModel(_mockStudioUpdateManager.Object, _mockQueryManager.Object, _shellViewModelMock.Object, _localhostServerMock.Object);
        }

        #endregion Test initialize

        [TestMethod]
        [Timeout(100)]
        public void TestRetrieveSources()
        {
            var expectedResult = new ObservableCollection<IComPluginSource>();
            _mockQueryManager.Setup(it => it.FetchComPluginSources()).Returns(expectedResult);

            var result = _target.RetrieveSources();

            Assert.AreEqual(expectedResult.Count, result.Count);
            _mockQueryManager.Verify(it => it.FetchComPluginSources());
        }

        [TestMethod]
        [Timeout(250)]
        public void ComPlugin_TestGetActions()
        {
            var expectedResult = new ObservableCollection<IPluginAction>();
            var mockComPluginSource = new Mock<IComPluginSource>();
            var mockNamespace = new Mock<INamespaceItem>();
            _mockQueryManager.Setup(it => it.PluginActions(mockComPluginSource.Object, mockNamespace.Object)).Returns(expectedResult);

            var result = _target.GetActions(mockComPluginSource.Object, mockNamespace.Object);

            Assert.AreEqual(expectedResult.Count, result.Count);
            _mockQueryManager.Verify(it => it.PluginActions(mockComPluginSource.Object, mockNamespace.Object));
        }

        [TestMethod]
        [Timeout(250)]
        public void TestGetNameSpaces()
        {
            var expectedResult = new ObservableCollection<INamespaceItem>();
            var mockComPluginSource = new Mock<IComPluginSource>();
            _mockQueryManager.Setup(it => it.FetchNamespaces(mockComPluginSource.Object)).Returns(expectedResult);

            var result = _target.GetNameSpaces(mockComPluginSource.Object);

            Assert.AreSame(expectedResult, result);
            _mockQueryManager.Verify(it => it.FetchNamespaces(mockComPluginSource.Object));
        }

        [TestMethod]
        [Timeout(250)]
        public void ManageComPluginServiceModel_TestCreateNewSource()
        {
            try
            {
                _target.CreateNewSource();
            }
            catch (Exception ex)
            {
                Assert.Fail("Create new COM plugin source failed. Exception: " + ex.Message);
            }
        }

        [TestMethod]
        [Timeout(1000)]
        public void TestEditComPluginSource()
        {
            var mockPluginSource = new Mock<IComPluginSource>();
            mockPluginSource.Setup(source => source.Is32Bit).Returns(false);
            mockPluginSource.Setup(source => source.ResourceName).Returns("EditComPluginSource");
            mockPluginSource.Setup(source => source.SelectedDll).Returns(new Mock<IDllListingModel>().Object);

            _target.EditSource(mockPluginSource.Object);
        }

        [TestMethod]
        [Timeout(250)]
        public void TestService()
        {
            var mockPluginInputValues = new Mock<IComPluginService>();
            var expectedResult = string.Empty;
            _mockStudioUpdateManager.Setup(it => it.TestPluginService(mockPluginInputValues.Object)).Returns(expectedResult);


            var result = _target.TestService(mockPluginInputValues.Object);

            Assert.AreSame(expectedResult, result);
            _mockStudioUpdateManager.Verify(it => it.TestPluginService(mockPluginInputValues.Object));
        }
    }
}
