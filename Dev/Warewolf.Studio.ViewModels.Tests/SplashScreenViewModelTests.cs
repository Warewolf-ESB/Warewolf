using System;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class SplashScreenViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SplashViewModel_Constructor")]
        public void SplashViewModel_Construtor_ServerPassedIn_ShouldSetServerProperty()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            var externalProcess = new Mock<IExternalProcessExecutor>();
            
            //------------Execute Test---------------------------
            var splashViewModel = new SplashViewModel(server.Object, externalProcess.Object);

            //------------Assert Results-------------------------
            Assert.IsNotNull(splashViewModel);
            Assert.IsNotNull(splashViewModel.Server);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SplashViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SplashViewModel_Constructor_NullServer_ArgumentNullException()
        {
            //------------Setup for test--------------------------
            var externalProcess = new Mock<IExternalProcessExecutor>();
            //------------Execute Test---------------------------
            var splashViewModel = new SplashViewModel(null, externalProcess.Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SplashViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SplashViewModel_Constructor_NullExternalProcessServer_ArgumentNullException()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            //------------Execute Test---------------------------
            var splashViewModel = new SplashViewModel(server.Object, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SplashViewModel_Server")]
        public void SplashViewModel_Server_GetServerVersion_VersionError()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            var externalProcess = new Mock<IExternalProcessExecutor>();
            var studioVer = "Version " + Utils.FetchVersionInfo();
            const string Ver = "1.0.0";
            server.Setup(server1 => server1.GetServerVersion()).Returns(Ver);
            var splashViewModel = new SplashViewModel(server.Object, externalProcess.Object);
            //------------Execute Test---------------------------
            string studioVersion = splashViewModel.StudioVersion;
            string serverVersion = splashViewModel.ServerVersion;

            //------------Assert Results-------------------------
            var correctStudioDescription = studioVersion;
            const string CorrectServerDescription = "Version " + Ver;

            server.Verify(server1 => server1.GetServerVersion(), Times.Once());
            Assert.AreEqual(correctStudioDescription, studioVer);
            Assert.AreEqual(CorrectServerDescription, serverVersion);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SplashViewModel_URL")]
        public void SplashViewModel_URL_Contributors_DevWebPageFound()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            string calledUrl = "";
            var externalProcess = new Mock<IExternalProcessExecutor>();
            externalProcess.Setup(externalProcess1 => externalProcess1.OpenInBrowser(It.IsAny<Uri>())).Callback<Uri>(url => calledUrl = url.ToString());
            var splashViewModel = new SplashViewModel(server.Object, externalProcess.Object);
            //------------Execute Test---------------------------
            splashViewModel.DevUrlCommand.Execute(null);
            //------------Assert Results-------------------------
            externalProcess.Verify(executor => executor.OpenInBrowser(It.IsAny<Uri>()), Times.Once());
            Assert.AreEqual("http://dev2.co.za/", calledUrl);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SplashViewModel_URL")]
        public void SplashViewModel_URL_Contributors_WarewolfWebPageFound()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            string calledUrl = "";
            var externalProcess = new Mock<IExternalProcessExecutor>();
            externalProcess.Setup(externalProcess1 => externalProcess1.OpenInBrowser(It.IsAny<Uri>())).Callback<Uri>(url => calledUrl = url.ToString());
            var splashViewModel = new SplashViewModel(server.Object, externalProcess.Object);
            //------------Execute Test---------------------------
            splashViewModel.WarewolfUrlCommand.Execute(null);
            //------------Assert Results-------------------------
            externalProcess.Verify(executor => executor.OpenInBrowser(It.IsAny<Uri>()), Times.Once());
            Assert.AreEqual("http://warewolf.io/", calledUrl);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SplashViewModel_URL")]
        public void SplashViewModel_URL_Contributors_ContributorWebPageFound()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            string calledUrl = "";
            var externalProcess = new Mock<IExternalProcessExecutor>();
            externalProcess.Setup(externalProcess1 => externalProcess1.OpenInBrowser(It.IsAny<Uri>())).Callback<Uri>(url => calledUrl = url.ToString());
            var splashViewModel = new SplashViewModel(server.Object, externalProcess.Object);
            //------------Execute Test---------------------------
            splashViewModel.ContributorsCommand.Execute(null);
            //------------Assert Results-------------------------
            externalProcess.Verify(executor => executor.OpenInBrowser(It.IsAny<Uri>()),Times.Once());
            Assert.AreEqual("http://warewolf.io/contributors.php", calledUrl);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SplashViewModel_URL")]
        public void SplashViewModel_URL_Community_CommunityWebPageFound()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            string calledUrl = "";
            var externalProcess = new Mock<IExternalProcessExecutor>();
            externalProcess.Setup(externalProcess1 => externalProcess1.OpenInBrowser(It.IsAny<Uri>())).Callback<Uri>(url => calledUrl = url.ToString());
            var splashViewModel = new SplashViewModel(server.Object, externalProcess.Object);
            //------------Execute Test---------------------------
            splashViewModel.CommunityCommand.Execute(null);
            //------------Assert Results-------------------------
            externalProcess.Verify(executor => executor.OpenInBrowser(It.IsAny<Uri>()), Times.Once());
            Assert.AreEqual("http://community.warewolf.io/", calledUrl);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SplashViewModel_URL")]
        public void SplashViewModel_URL_ExpertHelp_ExpertHelpWebPageFound()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            string calledUrl = "";
            var externalProcess = new Mock<IExternalProcessExecutor>();
            externalProcess.Setup(externalProcess1 => externalProcess1.OpenInBrowser(It.IsAny<Uri>())).Callback<Uri>(url => calledUrl = url.ToString());
            var splashViewModel = new SplashViewModel(server.Object, externalProcess.Object);
            //------------Execute Test---------------------------
            splashViewModel.ExpertHelpCommand.Execute(null);
            //------------Assert Results-------------------------
            externalProcess.Verify(executor => executor.OpenInBrowser(It.IsAny<Uri>()), Times.Once());
            Assert.AreEqual("http://warewolf.io/knowledge-base/", calledUrl);
        }
    }
}