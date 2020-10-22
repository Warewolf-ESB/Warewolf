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
    public class ManagePluginServiceModelTests
    {
        #region Fields

        ManagePluginServiceModel _target;

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

            _target = new ManagePluginServiceModel(_mockStudioUpdateManager.Object, _mockQueryManager.Object, _shellViewModelMock.Object, _localhostServerMock.Object);
        }

        #endregion Test initialize

        [TestMethod]
        [Timeout(100)]
        public void TestRetrieveSources()
        {
            var expectedResult = new ObservableCollection<IPluginSource>();
            _mockQueryManager.Setup(it => it.FetchPluginSources()).Returns(expectedResult);

            var result = _target.RetrieveSources();

            Assert.AreEqual(expectedResult.Count, result.Count);
            _mockQueryManager.Verify(it => it.FetchPluginSources());
        }

        [TestMethod]
        [Timeout(100)]
        public void TestGetActions()
        {
            var expectedResult = new ObservableCollection<IPluginAction>();
            var mockPluginSource = new Mock<IPluginSource>();
            var mockNamespace = new Mock<INamespaceItem>();
            _mockQueryManager.Setup(it => it.PluginActions(mockPluginSource.Object, mockNamespace.Object)).Returns(expectedResult);

            var result = _target.GetActions(mockPluginSource.Object, mockNamespace.Object);

            Assert.AreEqual(expectedResult.Count, result.Count);
            _mockQueryManager.Verify(it => it.PluginActions(mockPluginSource.Object, mockNamespace.Object));
        }

        [TestMethod]
        [Timeout(1000)]
        public void TestGetActionsWithReturns()
        {
            var expectedResult = new ObservableCollection<IPluginAction>();
            var mockPluginSource = new Mock<IPluginSource>();
            var mockNamespace = new Mock<INamespaceItem>();
            _mockQueryManager.Setup(it => it.PluginActionsWithReturns(mockPluginSource.Object, mockNamespace.Object)).Returns(expectedResult);

            var result = _target.GetActionsWithReturns(mockPluginSource.Object, mockNamespace.Object);

            Assert.AreEqual(expectedResult.Count, result.Count);
            _mockQueryManager.Verify(it => it.PluginActionsWithReturns(mockPluginSource.Object, mockNamespace.Object));
        }

        [TestMethod]
        [Timeout(1000)]
        public void TestGetConstructors()
        {
            var expectedResult = new ObservableCollection<IPluginConstructor>();
            var mockPluginSource = new Mock<IPluginSource>();
            var mockNamespace = new Mock<INamespaceItem>();
            _mockQueryManager.Setup(it => it.PluginConstructors(mockPluginSource.Object, mockNamespace.Object)).Returns(expectedResult);

            var result = _target.GetConstructors(mockPluginSource.Object, mockNamespace.Object);

            Assert.AreEqual(expectedResult.Count, result.Count);
            _mockQueryManager.Verify(it => it.PluginConstructors(mockPluginSource.Object, mockNamespace.Object));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestGetNameSpaces()
        {
            var expectedResult = new ObservableCollection<INamespaceItem>();
            var mockPluginSource = new Mock<IPluginSource>();
            _mockQueryManager.Setup(it => it.FetchNamespaces(mockPluginSource.Object)).Returns(expectedResult);

            var result = _target.GetNameSpaces(mockPluginSource.Object);

            Assert.AreSame(expectedResult, result);
            _mockQueryManager.Verify(it => it.FetchNamespaces(mockPluginSource.Object));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestGetNameSpacesWithJsonRetunrs()
        {
            var expectedResult = new ObservableCollection<INamespaceItem>();
            var mockPluginSource = new Mock<IPluginSource>();
            _mockQueryManager.Setup(it => it.FetchNamespacesWithJsonRetunrs(mockPluginSource.Object)).Returns(expectedResult);

            var result = _target.GetNameSpacesWithJsonRetunrs(mockPluginSource.Object);

            Assert.AreSame(expectedResult, result);
            _mockQueryManager.Verify(it => it.FetchNamespacesWithJsonRetunrs(mockPluginSource.Object));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestCreateNewSource()
        {
            try
            {
                _target.CreateNewSource();
            }
            catch (Exception ex)
            {
                Assert.Fail("Create new plugin source failed. Exception: " + ex.Message);
            }
        }

        [TestMethod]
        [Timeout(100)]
        public void TestEditPluginSource()
        {
            var mockPluginSource = new Mock<IPluginSource>();
            mockPluginSource.Setup(source => source.Name).Returns("PluginSource");
            mockPluginSource.Setup(source => source.FileSystemAssemblyName).Returns("EditPluginSource");
            mockPluginSource.Setup(source => source.SelectedDll).Returns(new Mock<IDllListingModel>().Object);

            _target.EditSource(mockPluginSource.Object);
        }

        [TestMethod]
        [Timeout(250)]
        public void TestService()
        {
            var mockPluginInputValues = new Mock<IPluginService>();
            var expectedResult = string.Empty;
            _mockStudioUpdateManager.Setup(it => it.TestPluginService(mockPluginInputValues.Object)).Returns(expectedResult);


            var result = _target.TestService(mockPluginInputValues.Object);

            Assert.AreSame(expectedResult, result);
            _mockStudioUpdateManager.Verify(it => it.TestPluginService(mockPluginInputValues.Object));
        }
    }
}
