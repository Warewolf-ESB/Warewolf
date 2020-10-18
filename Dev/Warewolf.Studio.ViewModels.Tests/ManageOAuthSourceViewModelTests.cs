using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageOAuthSourceViewModelTests
    {
        Mock<IManageOAuthSourceModel> _updateManager;
        Mock<IOAuthSource> _oAuthSource;
        Mock<IAsyncWorker> _asyncWorkerMock;
        ManageOAuthSourceViewModel _manageOAuthSourceViewModel;

        [TestInitialize]
        public void TestInitialize()
        {
            _updateManager = new Mock<IManageOAuthSourceModel>();
            _oAuthSource = new Mock<IOAuthSource>();
            _asyncWorkerMock = new Mock<IAsyncWorker>();
            _oAuthSource.SetupProperty(p => p.ResourceName, "Test");
            _updateManager.Setup(model => model.FetchSource(It.IsAny<Guid>()))
              .Returns(_oAuthSource.Object);
            _asyncWorkerMock.Setup(worker =>
                                   worker.Start(
                                            It.IsAny<Func<IOAuthSource>>(),
                                            It.IsAny<Action<IOAuthSource>>()))
                            .Callback<Func<IOAuthSource>, Action<IOAuthSource>>((func, action) =>
                            {
                                var dbSource = func.Invoke();
                                action?.Invoke(dbSource);
                            });
            _manageOAuthSourceViewModel = new ManageOAuthSourceViewModel(_updateManager.Object, _oAuthSource.Object, _asyncWorkerMock.Object) { Name = "Testing OAuth" };
        }

        [TestMethod]
        [Timeout(250)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructorNullIManageOAuthSourceModel()
        {
            var requestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var requestServiceNameViewModelTask = new Task<IRequestServiceNameViewModel>(() => requestServiceNameViewModel.Object);

            IManageOAuthSourceModel nullParam = null;
            new ManageOAuthSourceViewModel(nullParam, requestServiceNameViewModelTask);
        }

        [TestMethod]
        [Timeout(250)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructorNullIRequestServiceNameViewModel()
        {
            Task<IRequestServiceNameViewModel> nullParam = null;
            new ManageOAuthSourceViewModel(_updateManager.Object, nullParam);
        }

        [TestMethod]
        [Timeout(100)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructor2NullIManageOAuthSourceModel()
        {
            IManageOAuthSourceModel nullParam = null;
            new ManageOAuthSourceViewModel(nullParam, _oAuthSource.Object,new SynchronousAsyncWorker());
        }

        [TestMethod]
        [Timeout(100)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructorNullIOAuthSource()
        {
            IOAuthSource nullParam = null;
            new ManageOAuthSourceViewModel(_updateManager.Object, nullParam, new SynchronousAsyncWorker());
        }

        [TestMethod]
        [Timeout(100)]
        public void TestManageOAuthSourceViewModelConstructor2()
        {
            var requestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var requestServiceNameViewModelTask = new Task<IRequestServiceNameViewModel>(() => requestServiceNameViewModel.Object);

            new ManageOAuthSourceViewModel(_updateManager.Object, requestServiceNameViewModelTask);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestManageOAuthSourceViewModelProperties()
        {
            Assert.AreEqual(_manageOAuthSourceViewModel.Name, "Testing OAuth");
        }

        [TestMethod]
        [Timeout(100)]
        public void TestOkCommandCanExecuteTrue()
        {
            //arrange
            _manageOAuthSourceViewModel.TestPassed = true;
            _manageOAuthSourceViewModel.AccessToken = "token";

            //act
            var result = _manageOAuthSourceViewModel.OkCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Timeout(500)]
        public void TestOkCommandCanExecuteFalse()
        {
            //arrange
            _manageOAuthSourceViewModel.TestPassed = false;
            _manageOAuthSourceViewModel.AccessToken = "token";

            //act
            var result = _manageOAuthSourceViewModel.OkCommand.CanExecute(null);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestOkCommandExecuteNullSource()
        {
            //arrange
            _manageOAuthSourceViewModel.Item = new DropBoxSource() { ResourceName = "testing", ResourcePath = "" };

            //act
            _manageOAuthSourceViewModel.OkCommand.Execute(null);

            //assert
            Assert.IsNotNull(_manageOAuthSourceViewModel.Item);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestGetAuthTokens()
        {
            //arrange
            var uri = new Uri("https://example.com");

            //act
            _manageOAuthSourceViewModel.GetAuthTokens(uri);

            //assert
            Assert.AreEqual(_manageOAuthSourceViewModel.TestMessage, "Waiting for user details...");
        }

        [TestMethod]
        [Timeout(250)]
        public void TestGetAuthTokensWithDropBoxUri()
        {
            //arrange
            var uri = new Uri("https://www.dropbox.com/1/oauth2/redirect_receiver/");
            _manageOAuthSourceViewModel.Testing = true;

            //act
            _manageOAuthSourceViewModel.GetAuthTokens(uri);

            //assert
            Assert.IsFalse(_manageOAuthSourceViewModel.Testing);
        }

        [TestMethod]
        [Timeout(500)]
        public void TestGetAuthTokensWithDropBoxUriWithFakeToken()
        {
            //arrange
            var uri = new Uri("https://www.dropbox.com/1/oauth2/redirect_receiver/#access_token=TLYWNO2649cAAAAAAAAFQPJG4LFXob4OmPBPQO3FTkx8i8Unna1D6PpaiK26T8UZ&token_type=bearer&state=199080ca44f04b6690d931e1baf5b2bb&uid=6695762");
            _manageOAuthSourceViewModel.Testing = true;
            _manageOAuthSourceViewModel.TestFailed = false;
            _manageOAuthSourceViewModel.TestPassed = true;
            _manageOAuthSourceViewModel.AccessToken = "Test";

            //act
            _manageOAuthSourceViewModel.GetAuthTokens(uri);

            //assert
            Assert.IsFalse(_manageOAuthSourceViewModel.Testing);
            Assert.IsTrue(_manageOAuthSourceViewModel.TestFailed);
            Assert.IsFalse(_manageOAuthSourceViewModel.TestPassed);
            Assert.AreEqual(_manageOAuthSourceViewModel.AccessToken, "");
        }

        [TestMethod]
        [Timeout(250)]
        public void TestGetAuthTokensWithDropBoxUriWithInvalidToken()
        {
            //arrange
            var uri = new Uri("https://www.dropbox.com/1/oauth2/redirect_receiver/#");
            _manageOAuthSourceViewModel.Testing = true;
            _manageOAuthSourceViewModel.TestFailed = false;
            _manageOAuthSourceViewModel.TestPassed = true;
            _manageOAuthSourceViewModel.AccessToken = "Test";
            _manageOAuthSourceViewModel.TestMessage = "Test";

            //act
            _manageOAuthSourceViewModel.GetAuthTokens(uri);

            //assert
            Assert.IsFalse(_manageOAuthSourceViewModel.Testing);
            Assert.IsFalse(_manageOAuthSourceViewModel.HasAuthenticated);
            Assert.IsTrue(_manageOAuthSourceViewModel.TestFailed);
            Assert.IsFalse(_manageOAuthSourceViewModel.TestPassed);
            Assert.AreEqual(_manageOAuthSourceViewModel.AccessToken, "");
            Assert.AreEqual(_manageOAuthSourceViewModel.TestMessage, "Authentication failed");
        }

        [TestMethod]
        [Timeout(100)]
        public void TestTestCommandCanExecuteTrue()
        {
            //arrange
            _manageOAuthSourceViewModel.SelectedOAuthProvider = "Test";
            _manageOAuthSourceViewModel.AppKey = "123";

            //act
            var result = _manageOAuthSourceViewModel.TestCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestTestCommandCanExecuteFalse()
        {
            //arrange
            _manageOAuthSourceViewModel.SelectedOAuthProvider = null;
            _manageOAuthSourceViewModel.AppKey = null;

            //act
            var result = _manageOAuthSourceViewModel.TestCommand.CanExecute(null);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestTestCommandExecuteWebBrowserAuthUriNull()
        {
            //arrange
            _manageOAuthSourceViewModel.Testing = false;
            _manageOAuthSourceViewModel.TestFailed = true;
            _manageOAuthSourceViewModel.TestPassed = false;
            _manageOAuthSourceViewModel.WebBrowser = null;
            _manageOAuthSourceViewModel.AuthUri = null;

            //act
            _manageOAuthSourceViewModel.TestCommand.Execute(null);

            //assert
            Assert.IsFalse(_manageOAuthSourceViewModel.Testing);
            Assert.IsTrue(_manageOAuthSourceViewModel.TestFailed);
            Assert.IsFalse(_manageOAuthSourceViewModel.TestPassed);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestTestCommandExecuteWebBrowserAuthUriNotNull()
        {
            //arrange
            _manageOAuthSourceViewModel.Testing = false;
            _manageOAuthSourceViewModel.TestFailed = true;
            _manageOAuthSourceViewModel.TestPassed = true;
            _manageOAuthSourceViewModel.WebBrowser = new Mock<IWebBrowser>().Object;
            _manageOAuthSourceViewModel.AuthUri = new Uri("https://example.com");
            _manageOAuthSourceViewModel.AppKey = "123";

            //act
            _manageOAuthSourceViewModel.TestCommand.Execute(null);

            //assert
            Assert.IsTrue(_manageOAuthSourceViewModel.Testing);
            Assert.IsFalse(_manageOAuthSourceViewModel.TestFailed);
            Assert.IsFalse(_manageOAuthSourceViewModel.TestPassed);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestToModelNullItem()
        {
            //arrange
            _manageOAuthSourceViewModel.Item = null;

            //act
            var result = _manageOAuthSourceViewModel.ToModel();

            //assert
            Assert.IsNotNull(_manageOAuthSourceViewModel.Item);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestToModelNullItemNullOAuthSource()
        {
            //arrange
            var requestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var requestServiceNameViewModelTask = new Task<IRequestServiceNameViewModel>(() => requestServiceNameViewModel.Object);
            _manageOAuthSourceViewModel = new ManageOAuthSourceViewModel(_updateManager.Object, requestServiceNameViewModelTask);

            _manageOAuthSourceViewModel.Item = null;
            _manageOAuthSourceViewModel.AppKey = "123";
            _manageOAuthSourceViewModel.AccessToken = "token";

            //act
            var result = _manageOAuthSourceViewModel.ToModel();

            //assert
            Assert.IsNotNull(_manageOAuthSourceViewModel.Item);
            Assert.AreEqual(result.AccessToken, "token");
            Assert.AreEqual(result.AppKey, "123");
        }

        [TestMethod]
        [Timeout(100)]
        public void TestToModelDropBoxSourceItem()
        {
            //arrange
            _manageOAuthSourceViewModel.Item = new DropBoxSource();
            _manageOAuthSourceViewModel.SelectedOAuthProvider = "Dropbox";
            _manageOAuthSourceViewModel.AppKey = "123";
            _manageOAuthSourceViewModel.AccessToken = "token";

            //act
            var result = _manageOAuthSourceViewModel.ToModel();

            //assert
            Assert.IsNotNull(_manageOAuthSourceViewModel.Item);
            Assert.AreEqual(result.AppKey, "123");
            Assert.AreEqual(result.AccessToken, "token");
        }

        [TestMethod]
        [Timeout(100)]
        public void TestToModelOtherItem()
        {
            //arrange
            _manageOAuthSourceViewModel.Item = new DropBoxSource();
            _manageOAuthSourceViewModel.SelectedOAuthProvider = "Other";

            //act
            var result = _manageOAuthSourceViewModel.ToModel();

            //assert
            Assert.IsNull(result);
        }

        [TestMethod]
        [Timeout(250)]
        public void TestUpdateHelpDescriptor()
        {
            //arrange
            var mainViewModelMock = new Mock<IShellViewModel>();
            var helpViewModelMock = new Mock<IHelpWindowViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);

            //act
            _manageOAuthSourceViewModel.UpdateHelpDescriptor("helpText");

            //assert
            helpViewModelMock.Verify(it => it.UpdateHelpText("helpText"));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestFromModel()
        {
            //arrange
            IOAuthSource dropBoxSource = new DropBoxSource() { ResourcePath = "test path", ResourceName = "test resource", AppKey = "test app key", AccessToken = "test token" };
            _manageOAuthSourceViewModel.Types = new List<string>() { "test provider" };

            //act
            _manageOAuthSourceViewModel.FromModel(dropBoxSource);

            //assert
            Assert.AreEqual(_manageOAuthSourceViewModel.Path, "test path");
            Assert.AreEqual(_manageOAuthSourceViewModel.ResourceName, "test resource");
            Assert.AreEqual(_manageOAuthSourceViewModel.AppKey, "test app key");
            Assert.AreEqual(_manageOAuthSourceViewModel.AccessToken, "test token");
            Assert.AreEqual(_manageOAuthSourceViewModel.SelectedOAuthProvider, "test provider");
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSaveExceptionMessage()
        {
            //arrange
            _updateManager.Setup(u => u.Save(It.IsAny<IOAuthSource>())).Throws(new Exception("Test save exception"));
            _manageOAuthSourceViewModel = new ManageOAuthSourceViewModel(_updateManager.Object, _oAuthSource.Object, new SynchronousAsyncWorker()) { Name = "Testing OAuth" };
            _manageOAuthSourceViewModel.Item = new DropBoxSource() { ResourceName = "testing", ResourcePath = "" };

            //act
            _manageOAuthSourceViewModel.Save();

            //assert
            Assert.AreEqual(_manageOAuthSourceViewModel.TestMessage, "Test save exception");
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSaveConnection()
        {
            //arrange
            _updateManager.Setup(u => u.Save(It.IsAny<IOAuthSource>())).Throws(new Exception("Test save exception"));
            _manageOAuthSourceViewModel = new ManageOAuthSourceViewModel(_updateManager.Object, _oAuthSource.Object, new SynchronousAsyncWorker()) { Name = "Testing OAuth" };
            _manageOAuthSourceViewModel.Item = new DropBoxSource() { ResourceName = "testing", ResourcePath = "" };

            var manageOAuthSourceVM = new PrivateObject(_manageOAuthSourceViewModel);
            //act
            manageOAuthSourceVM.Invoke("SaveConnection");
            //assert
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSaveConnection_GiveN_AuthSource()
        {
            //arrange_updateManager.Setup(u => u.Save(It.IsAny<IOAuthSource>())).Throws(new Exception("Test save exception"));
            _manageOAuthSourceViewModel = new ManageOAuthSourceViewModel(_updateManager.Object, _oAuthSource.Object, new SynchronousAsyncWorker()) { Name = "Testing OAuth" };
            _manageOAuthSourceViewModel.Item = new DropBoxSource() { ResourceName = "testing", ResourcePath = "" };

            var mockShellVM = new Mock<IShellViewModel>();
            var shellVMExplorer = new Mock<IExplorerViewModel>();
            var requestServiceNameVM = new Mock<IRequestServiceNameViewModel>();
            var mockEnvironment = new Mock<IEnvironmentViewModel>();
            var explorerVM = new Mock<IExplorerViewModel>();
            var mockEnvironments = new System.Collections.ObjectModel.ObservableCollection<IEnvironmentViewModel>();

            mockShellVM.Setup(p => p.ExplorerViewModel).Returns(shellVMExplorer.Object);
            mockEnvironment.Setup(p => p.ResourceId).Returns(It.IsAny<Guid>);
            var mockServer = new Mock<IServer>();
            mockServer.Setup(p => p.EnvironmentID).Returns(Guid.NewGuid());
            mockEnvironment.Setup(p => p.Server).Returns(mockServer.Object);
            mockEnvironments.Add(mockEnvironment.Object);
            shellVMExplorer.Setup(p => p.Environments).Returns(mockEnvironments);            
            explorerVM.Setup(p => p.Environments).Returns(mockEnvironments);
            requestServiceNameVM.Setup(p => p.ShowSaveDialog()).Returns(System.Windows.MessageBoxResult.OK);
            requestServiceNameVM.Setup(p => p.ResourceName).Returns(new Dev2.Common.SaveDialog.ResourceName("Some Awesome Path", "Cool Resource Name"));
            requestServiceNameVM.Setup(p => p.SingleEnvironmentExplorerViewModel).Returns(explorerVM.Object);
            var task = Task.FromResult(requestServiceNameVM.Object);
            _manageOAuthSourceViewModel.RequestServiceNameViewModel = task;
            var manageOAuthSourceVM = new PrivateObject(_manageOAuthSourceViewModel);
            manageOAuthSourceVM.SetField("_oAuthSource", null);
            CustomContainer.Register(mockShellVM.Object);
            Assert.IsNull(_manageOAuthSourceViewModel.Path);
            //act
            manageOAuthSourceVM.Invoke("SaveConnection");
            var returnedAuthSource = manageOAuthSourceVM.GetField("_oAuthSource") as IOAuthSource;
            Assert.IsNotNull(returnedAuthSource);
            Assert.AreEqual("Cool Resource Name", returnedAuthSource.ResourceName);
            Assert.AreEqual("Some Awesome Path", returnedAuthSource.ResourcePath);
            Assert.AreEqual("Some Awesome Path", _manageOAuthSourceViewModel.Path);
            //assert
        }

    }
}