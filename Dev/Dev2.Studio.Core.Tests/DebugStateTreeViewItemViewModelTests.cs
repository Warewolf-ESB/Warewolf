using System;
using System.Collections.Generic;
using Dev2.Composition;
using Dev2.Diagnostics;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DebugStateTreeViewItemViewModelTests
    {
        static ImportServiceContext _importContext;

        [ClassInitialize()]
        public static void Initialize(TestContext testContext)
        {
            _importContext = CompositionInitializer.DefaultInitialize();
        }

        // BUG 8373: TWR
        [TestMethod]
        public void DebugStateTreeViewModelDefaultExpectsIsExpandedFalse()
        {
            //Setup
            var serverID = Guid.NewGuid();
            const string ServerName = "Myserver";

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ID).Returns(serverID);
            env.Setup(e => e.Name).Returns(ServerName);

            var env2 = new Mock<IEnvironmentModel>();
            env2.Setup(e => e.ID).Returns(Guid.NewGuid());

            var envRep = new Mock<IEnvironmentRepository>();
            envRep.Setup(e => e.All()).Returns(() => new[] { env.Object, env2.Object });

            var content = new DebugState { ServerID = serverID };

            //Execute
            var vm = new DebugStateTreeViewItemViewModel(envRep.Object, content);

            //Assert
            Assert.IsFalse(vm.IsExpanded, "The debug state tree viewmodel should be collapsed if not explicitly expanded in constructor");
        }

        // BUG 8373: TWR
        [TestMethod]
        public void Constructor_With_EnvironmentRepository_Expected_SetsDebugStateServer()
        {
            var serverID = Guid.NewGuid();
            const string ServerName = "Myserver";

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ID).Returns(serverID);
            env.Setup(e => e.Name).Returns(ServerName);

            var env2 = new Mock<IEnvironmentModel>();
            env2.Setup(e => e.ID).Returns(Guid.NewGuid());

            var envRep = new Mock<IEnvironmentRepository>();
            envRep.Setup(e => e.All()).Returns(() => new[] { env.Object, env2.Object });

            var content = new DebugState { ServerID = serverID };
            var vm = new DebugStateTreeViewItemViewModel(envRep.Object, content);
            Assert.AreEqual(ServerName, content.Server);
        }

        // BUG 8373: TWR
        [TestMethod]
        public void ViewModelCanDetectRemoteServerName()
        {
            var serverID = Guid.NewGuid();
            const string ServerName = "Myserver";

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ID).Returns(serverID);
            env.Setup(e => e.Name).Returns(ServerName);


            var env2ID = Guid.NewGuid();

            var env2 = new Mock<IEnvironmentModel>();
            env2.Setup(e => e.ID).Returns(env2ID);
            env2.Setup(e => e.Name).Returns("RemoteServer");

            var envRep = new Mock<IEnvironmentRepository>();
            envRep.Setup(e => e.All()).Returns(() => new[] { env.Object, env2.Object });

            var content = new DebugState { ServerID = serverID, Server = env2ID.ToString() };
            var vm = new DebugStateTreeViewItemViewModel(envRep.Object, content);
            Assert.AreEqual("RemoteServer", vm.Content.Server);
        }

        [TestMethod]
        [TestCategory("DebugStateTreeViewItemViewModel_IsSelected")]
        [Description("DebugStateTreeViewItemViewModel IsSelected when true must select its activity.")]
        [Owner("Trevor Williams-Ros")]
        public void DebugStateTreeViewItemViewModel_UnitTest_IsSelectedWhenTrue_PublishesDebugStateSelectionChangedEvent()
        {
            var expected = new DebugState { DisplayName = "IsSelectedTest", ID = Guid.NewGuid() };

            var selectionChangedEvents = EventPublishers.Studio.GetEvent<DebugSelectionChangedEventArgs>();
            selectionChangedEvents.Subscribe(args =>
            {
                Assert.AreSame(expected, args.DebugState, "DebugStateTreeViewItemViewModel IsSelected did not publish DebugStateSelectionChangedEvent.");
            });

            var envRep = new Mock<IEnvironmentRepository>();
            envRep.Setup(r => r.All()).Returns(new List<IEnvironmentModel>());

            var vm = new DebugStateTreeViewItemViewModel(envRep.Object, expected);

            vm.IsSelected = true;

        }
    }
}
