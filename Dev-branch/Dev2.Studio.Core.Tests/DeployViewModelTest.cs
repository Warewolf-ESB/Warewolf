using Dev2.Composition;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Deploy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DeployViewModelTest
    {
        #region Class Members

        private static ImportServiceContext _okayContext;
        private static ImportServiceContext _cancelContext;

        #endregion Class Members

        #region Initialization

        [ClassInitialize()]
        public static void MyTestClassInitialize(TestContext testContext)
        {
            _okayContext = CompositionInitializer.DeployViewModelOkayTest();
            _cancelContext = CompositionInitializer.DeployViewModelCancelTest();
        }

        #endregion Initialization

        #region Test Methods

        [TestMethod]
        public void Connect_OkayDialogResult_Expected_Server_IsAdded()
        {
            ImportService.CurrentContext = _okayContext;
 
            var servers = new List<IServer>();
            servers.Add(null);
            servers.Add(null);

            var serverProvider = new Mock<IServerProvider>();
            serverProvider.Setup(s => s.Load()).Returns(servers);

            var deployViewModel = new DeployViewModel(serverProvider.Object);
            deployViewModel.ConnectCommand.Execute(null);

            var actual = deployViewModel.Servers.Count;

            Assert.AreEqual(servers.Count + 1, actual);
        }

        [TestMethod]
        public void Connect_CancelDialogResult_Expected_Server_NotAdded()
        {
            ImportService.CurrentContext = _cancelContext;
            var servers = new List<IServer>();
            servers.Add(null);
            servers.Add(null);

            var serverProvider = new Mock<IServerProvider>();
            serverProvider.Setup(s => s.Load()).Returns(servers);

            var deployViewModel = new DeployViewModel(serverProvider.Object);
            deployViewModel.ConnectCommand.Execute(null);

            var actual = deployViewModel.Servers.Count;

            Assert.AreEqual(servers.Count, actual);
        }

        #endregion Test Methods

        static Mock<IServer> CreateServerMock()
        {
            var server = new Mock<IServer>();
            server.Setup(s => s.ID).Returns(Guid.NewGuid().ToString);
            server.Setup(s => s.AppUri).Returns(new Uri("http://127.0.0.1:77/dsf"));
            server.Setup(s => s.WebUri).Returns(new Uri("http://127.0.0.1:1234"));
            server.Setup(s => s.Alias).Returns("Test");
            server.Setup(s => s.AppAddress).Returns("http://127.0.0.1:77/dsf");

            return server;
        }
    }
}
