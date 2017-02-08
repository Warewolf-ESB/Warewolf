using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Threading;
using Dev2.Threading;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageOAuthSourceViewModelTests
    {
        private Mock<IManageOAuthSourceModel> _updateManager;
        private Mock<IOAuthSource> _oAuthSource;
        private Mock<IAsyncWorker> _asyncWorkerMock;
        private ManageOAuthSourceViewModel _manageOAuthSourceViewModel;

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
                                action(dbSource);
                            });
            _manageOAuthSourceViewModel = new ManageOAuthSourceViewModel(_updateManager.Object, _oAuthSource.Object, _asyncWorkerMock.Object) { Name = "Testing OAuth" };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructorNullIManageOAuthSourceModel()
        {
            Mock<IRequestServiceNameViewModel> requestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            Task<IRequestServiceNameViewModel> requestServiceNameViewModelTask = new Task<IRequestServiceNameViewModel>(() => requestServiceNameViewModel.Object);

            IManageOAuthSourceModel nullParam = null;
            new ManageOAuthSourceViewModel(nullParam, requestServiceNameViewModelTask);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructorNullIRequestServiceNameViewModel()
        {
            Task<IRequestServiceNameViewModel> nullParam = null;
            new ManageOAuthSourceViewModel(_updateManager.Object, nullParam);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructor2NullIManageOAuthSourceModel()
        {
            IManageOAuthSourceModel nullParam = null;
            new ManageOAuthSourceViewModel(nullParam, _oAuthSource.Object,new SynchronousAsyncWorker());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructorNullIOAuthSource()
        {
            IOAuthSource nullParam = null;
            new ManageOAuthSourceViewModel(_updateManager.Object, nullParam, new SynchronousAsyncWorker());
        }

        [TestMethod]
        public void TestManageOAuthSourceViewModelConstructor2()
        {
            Mock<IRequestServiceNameViewModel> requestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            Task<IRequestServiceNameViewModel> requestServiceNameViewModelTask = new Task<IRequestServiceNameViewModel>(() => requestServiceNameViewModel.Object);

            new ManageOAuthSourceViewModel(_updateManager.Object, requestServiceNameViewModelTask);
        }

        [TestMethod]
        public void TestManageOAuthSourceViewModelProperties()
        {
            Assert.AreEqual(_manageOAuthSourceViewModel.Name, "Testing OAuth");
        }

        [TestMethod]
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
        public void TestGetAuthTokens()
        {
            //arrange
            Uri uri = new Uri("https://example.com");

            //act
            _manageOAuthSourceViewModel.GetAuthTokens(uri);

            //assert
            Assert.AreEqual(_manageOAuthSourceViewModel.TestMessage, "Waiting for user details...");
        }

        [TestMethod]
        public void TestGetAuthTokensWithDropBoxUri()
        {
            //arrange
            Uri uri = new Uri("https://www.dropbox.com/1/oauth2/redirect_receiver/");
            _manageOAuthSourceViewModel.Testing = true;

            //act
            _manageOAuthSourceViewModel.GetAuthTokens(uri);

            //assert
            Assert.IsFalse(_manageOAuthSourceViewModel.Testing);
        }

        [TestMethod]
        public void TestGetAuthTokensWithDropBoxUriWithFakeToken()
        {
            //arrange
            Uri uri = new Uri("https://www.dropbox.com/1/oauth2/redirect_receiver/#access_token=TLYWNO2649cAAAAAAAAFQPJG4LFXob4OmPBPQO3FTkx8i8Unna1D6PpaiK26T8UZ&token_type=bearer&state=199080ca44f04b6690d931e1baf5b2bb&uid=6695762");
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
        public void TestGetAuthTokensWithDropBoxUriWithInvalidToken()
        {
            //arrange
            Uri uri = new Uri("https://www.dropbox.com/1/oauth2/redirect_receiver/#");
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
        public void TestToModelNullItem()
        {
            //arrange
            _manageOAuthSourceViewModel.Item = null;

            //act
            IOAuthSource result = _manageOAuthSourceViewModel.ToModel();

            //assert
            Assert.IsNotNull(_manageOAuthSourceViewModel.Item);
        }

        [TestMethod]
        public void TestToModelNullItemNullOAuthSource()
        {
            //arrange
            Mock<IRequestServiceNameViewModel> requestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            Task<IRequestServiceNameViewModel> requestServiceNameViewModelTask = new Task<IRequestServiceNameViewModel>(() => requestServiceNameViewModel.Object);
            _manageOAuthSourceViewModel = new ManageOAuthSourceViewModel(_updateManager.Object, requestServiceNameViewModelTask);

            _manageOAuthSourceViewModel.Item = null;
            _manageOAuthSourceViewModel.AppKey = "123";
            _manageOAuthSourceViewModel.AccessToken = "token";

            //act
            IOAuthSource result = _manageOAuthSourceViewModel.ToModel();

            //assert
            Assert.IsNotNull(_manageOAuthSourceViewModel.Item);
            Assert.AreEqual(result.AccessToken, "token");
            Assert.AreEqual(result.AppKey, "123");
        }

        [TestMethod]
        public void TestToModelDropBoxSourceItem()
        {
            //arrange
            _manageOAuthSourceViewModel.Item = new DropBoxSource();
            _manageOAuthSourceViewModel.SelectedOAuthProvider = "Dropbox";
            _manageOAuthSourceViewModel.AppKey = "123";
            _manageOAuthSourceViewModel.AccessToken = "token";

            //act
            IOAuthSource result = _manageOAuthSourceViewModel.ToModel();

            //assert
            Assert.IsNotNull(_manageOAuthSourceViewModel.Item);
            Assert.AreEqual(result.AppKey, "123");
            Assert.AreEqual(result.AccessToken, "token");
        }

        [TestMethod]
        public void TestToModelOtherItem()
        {
            //arrange
            _manageOAuthSourceViewModel.Item = new DropBoxSource();
            _manageOAuthSourceViewModel.SelectedOAuthProvider = "Other";

            //act
            IOAuthSource result = _manageOAuthSourceViewModel.ToModel();

            //assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TestUpdateHelpDescriptor()
        {
            //arrange
            Mock<IMainViewModel> mainViewModelMock = new Mock<IMainViewModel>();
            Mock<IHelpWindowViewModel> helpViewModelMock = new Mock<IHelpWindowViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);

            //act
            _manageOAuthSourceViewModel.UpdateHelpDescriptor("helpText");

            //assert
            helpViewModelMock.Verify(it => it.UpdateHelpText("helpText"));
        }

        [TestMethod]
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
    }
}