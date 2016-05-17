using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageOAuthSourceViewModelTests
    {
        private Mock<IManageOAuthSourceModel> updateManager;
        private Mock<IOAuthSource> oAuthSource;

        private ManageOAuthSourceViewModel _manageOAuthSourceViewModel;

        [TestInitialize]
        public void TestInitialize()
        {
            updateManager = new Mock<IManageOAuthSourceModel>();
            oAuthSource = new Mock<IOAuthSource>();
            oAuthSource.SetupProperty(p => p.ResourceName, "Test");

            //Mock< IManageOAuthSourceModel>

            _manageOAuthSourceViewModel = new ManageOAuthSourceViewModel(updateManager.Object, oAuthSource.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructorNullIManageOAuthSourceModel()
        {
            Task<IRequestServiceNameViewModel> requestServiceNameViewModel = new Task<IRequestServiceNameViewModel>(null);
            IManageOAuthSourceModel nullParam = null;
            new ManageOAuthSourceViewModel(nullParam, requestServiceNameViewModel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructorNullIRequestServiceNameViewModel()
        {
            Task<IRequestServiceNameViewModel> nullParam = null;
            new ManageOAuthSourceViewModel(updateManager.Object, nullParam);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructor2NullIManageOAuthSourceModel()
        {
            IManageOAuthSourceModel nullParam = null;
            new ManageOAuthSourceViewModel(nullParam, oAuthSource.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructorNullIOAuthSource()
        {
            IOAuthSource nullParam = null;
            new ManageOAuthSourceViewModel(updateManager.Object, nullParam);
        }

        [TestMethod]
        public void TestManageOAuthSourceViewModelConstructor()
        {
            new ManageOAuthSourceViewModel(updateManager.Object, oAuthSource.Object);
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
        public void TestOkCommandExecute()
        {
            //arrange
            _manageOAuthSourceViewModel.Testing = false;
            _manageOAuthSourceViewModel.TestFailed = true;
            _manageOAuthSourceViewModel.TestPassed = true;

            //act
            _manageOAuthSourceViewModel.OkCommand.Execute(null);

            //assert
            Assert.IsTrue(_manageOAuthSourceViewModel.Testing);
            Assert.IsFalse(_manageOAuthSourceViewModel.TestFailed);
            Assert.IsFalse(_manageOAuthSourceViewModel.TestPassed);
        }

        [TestMethod]
        public void TestTestCommand()
        {
            //arrange
            Uri uri = new Uri("https://example.com");

            //act
            _manageOAuthSourceViewModel.GetAuthTokens(uri);

            //assert
            Assert.AreEqual(_manageOAuthSourceViewModel.TestMessage, "Waiting for user details...");
        }

        [TestMethod]
        public void TestTestCommandWithDropBoxUri()
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
        public void TestTestCommandWithDropBoxUriWithFakeToken()
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
        public void TestTestCommandWithDropBoxUriWithInvalidToken()
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
            Assert.IsTrue(_manageOAuthSourceViewModel.TestFailed);
            Assert.IsFalse(_manageOAuthSourceViewModel.TestPassed);
            Assert.AreEqual(_manageOAuthSourceViewModel.AccessToken, "");
            Assert.AreEqual(_manageOAuthSourceViewModel.TestMessage, "Authentication failed");
        }
    }
}