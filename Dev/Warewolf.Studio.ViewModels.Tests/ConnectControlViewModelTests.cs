using System;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.AntiCorruptionLayer;
using Warewolf.Testing;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ConnectControlViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ConnectControlViewModel_EditServer")]
        public void ConnectControlViewModel_EditServer_ServerIDMatch_IsTrue()
        {
            var serverGuid = Guid.NewGuid();
            Uri uri = new Uri("http://bravo.com/");

            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockExplorerRepository = new Mock<IExplorerRepository>();
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();

            mockEnvironmentConnection.Setup(a => a.AppServerUri).Returns(uri);
            mockEnvironmentConnection.Setup(a => a.UserName).Returns("johnny");
            mockEnvironmentConnection.Setup(a => a.Password).Returns("bravo");
            mockEnvironmentConnection.Setup(a => a.WebServerUri).Returns(uri);

            mockExplorerRepository.Setup(repository => repository.CreateFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()));
            mockExplorerRepository.Setup(repository => repository.Rename(It.IsAny<IExplorerItemViewModel>(), It.IsAny<string>())).Returns(true);
            
            var server = new ServerForTesting(mockExplorerRepository);
            var server2 = new Server();
            server2.EnvironmentID = serverGuid;
            server2.EnvironmentConnection = mockEnvironmentConnection.Object;
            server.EnvironmentID = serverGuid;
            server.ResourceName = "mr_J_bravo";
            server.ResourcePath = "";
            mockShellViewModel.Setup(a => a.ActiveServer).Returns(server);
            mockShellViewModel.Setup(model => model.LocalhostServer).Returns(server);
            
            CustomContainer.Register<IServer>(server);
            CustomContainer.Register<IShellViewModel>(mockShellViewModel.Object);
            CustomContainer.Register<IMainViewModel>(mockMainViewModel.Object);

            bool passed = false;
            mockMainViewModel.Setup(a => a.EditServer(It.IsAny<IServerSource>())).Callback((IServerSource a) => { passed = a.ID == serverGuid; });
            //------------Setup for test--------------------------
            var connectControlViewModel = new ConnectControlViewModel(server, new EventAggregator());
            PrivateObject p = new PrivateObject(connectControlViewModel);
            p.SetField("_selectedConnection",server2);
            //------------Execute Test---------------------------
            connectControlViewModel.Edit();

            //------------Assert Results-------------------------
           Assert.IsTrue(passed);
        }
    }
}
