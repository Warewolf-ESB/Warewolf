using System;
using System.Collections.Generic;

using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class SplashViewModelTests
    {
        #region Fields

        private Mock<IServer> _serverMock;
        private Mock<IExternalProcessExecutor> _externalProcessExecutorMock;

        private List<string> _changedProperties;
        private SplashViewModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _serverMock = new Mock<IServer>();
            _externalProcessExecutorMock = new Mock<IExternalProcessExecutor>();

            _changedProperties = new List<string>();
            _target = new SplashViewModel(_serverMock.Object, _externalProcessExecutorMock.Object);
            _target.PropertyChanged += (sender, args) => { _changedProperties.Add(args.PropertyName); };
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSplashViewModelServerNull()
        {
            //act
            new SplashViewModel(null, _externalProcessExecutorMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSplashViewModelExternalProcessExecutorNull()
        {
            //act
            new SplashViewModel(_serverMock.Object, null);
        }

        #endregion Test construction

        #region Test commands

        [TestMethod]
        public void TestContributorsCommandCanExecute()
        {
            //act
            var result = _target.ContributorsCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestContributorsCommandExecute()
        {
            //act
            _target.ContributorsCommand.Execute(null);

            //assert
            _externalProcessExecutorMock.Verify(it=>it.OpenInBrowser(_target.ContributorsUrl));
        }

        [TestMethod]
        public void TestCommunityCommandCanExecute()
        {
            //act
            var result = _target.CommunityCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCommunityCommandExecute()
        {
            //act
            _target.CommunityCommand.Execute(null);

            //assert
            _externalProcessExecutorMock.Verify(it => it.OpenInBrowser(_target.CommunityUrl));
        }

        [TestMethod]
        public void TestExpertHelpCommandCanExecute()
        {
            //act
            var result = _target.ExpertHelpCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestExpertHelpCommandExecute()
        {
            //act
            _target.ExpertHelpCommand.Execute(null);

            //assert
            _externalProcessExecutorMock.Verify(it => it.OpenInBrowser(_target.ExpertHelpUrl));
        }

        [TestMethod]
        public void TestWarewolfUrlCommandCanExecute()
        {
            //act
            var result = _target.WarewolfUrlCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestWarewolfUrlCommandExecute()
        {
            //act
            _target.WarewolfUrlCommand.Execute(null);

            //assert
            _externalProcessExecutorMock.Verify(it => it.OpenInBrowser(_target.WarewolfUrl));
        }

        #endregion Test commands

        #region Test properties

        [TestMethod]
        public void TestWarewolfUrl()
        {
            //arrange
            var expectedValue = new Uri("http://localhost/");

            //act
            _target.WarewolfUrl = expectedValue;
            var value = _target.WarewolfUrl;

            //assert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SplashViewModel_Copyright")]
        public void SplashViewModel_Copyright_Valid_ShouldContainCurrentYear()
        {
            //------------Setup for test--------------------------
            var currentYear = DateTime.Now.Year.ToString();

            //------------Execute Test---------------------------
            var copyRightText = _target.WarewolfCopyright;
            //------------Assert Results-------------------------
            StringAssert.Contains(copyRightText,currentYear);
        }

        [TestMethod]
        public void TestContributorsUrl()
        {
            //arrange
            var expectedValue = new Uri("http://localhost/");

            //act
            _target.ContributorsUrl = expectedValue;
            var value = _target.ContributorsUrl;

            //assert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        public void TestCommunityUrl()
        {
            //arrange
            var expectedValue = new Uri("http://localhost/");

            //act
            _target.CommunityUrl = expectedValue;
            var value = _target.CommunityUrl;

            //assert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        public void TestExpertHelpUrl()
        {
            //arrange
            var expectedValue = new Uri("http://localhost/");

            //act
            _target.ExpertHelpUrl = expectedValue;
            var value = _target.ExpertHelpUrl;

            //assert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        public void TestServer()
        {
            //arrange
            var expectedValueMock = new Mock<IServer>();

            //act
            _target.Server = expectedValueMock.Object;
            var value = _target.Server;

            //assert
            Assert.AreSame(expectedValueMock.Object, value);
        }

        [TestMethod]
        public void TestExternalProcessExecutor()
        {
            //arrange
            var expectedValueMock = new Mock<IExternalProcessExecutor>();

            //act
            _target.ExternalProcessExecutor = expectedValueMock.Object;
            var value = _target.ExternalProcessExecutor;

            //assert
            Assert.AreSame(expectedValueMock.Object, value);
        }

        [TestMethod]
        public void TestServerVersion()
        {
            //arrange
            var expectedValue = "someResourceName";
            _changedProperties.Clear();

            //act
            _target.ServerVersion = expectedValue;
            var value = _target.ServerVersion;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("ServerVersion"));
        }

//        [TestMethod]
//        public void TestServerInformationalVersion()
//        {
//            //arrange
//            var expectedValue = "someResourceName";
//            _changedProperties.Clear();
//
//            //act
//            _target.ServerInformationalVersion = expectedValue;
//            var value = _target.ServerInformationalVersion;
//
//            //asert
//            Assert.AreEqual(expectedValue, value);
//            Assert.IsTrue(_changedProperties.Contains("ServerInformationalVersion"));
//        }

        [TestMethod]
        public void TestStudioVersion()
        {
            //arrange
            var expectedValue = "someResourceName";
            _changedProperties.Clear();

            //act
            _target.StudioVersion = expectedValue;
            var value = _target.StudioVersion;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("StudioVersion"));
        }

        #endregion Test properties

    }
}