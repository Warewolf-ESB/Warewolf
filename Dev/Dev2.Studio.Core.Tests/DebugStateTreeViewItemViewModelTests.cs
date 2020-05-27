/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DebugStateTreeViewItemViewModelTests
    {
        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
        }

        // BUG 8373: TWR
        [TestMethod]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        [Owner("Trevor Williams-Ros")]
        
        public void DebugStateTreeViewItemViewModel_Constructor_IsExpanded_False()
        {
            //Setup
            var serverID = Guid.NewGuid();
            const string ServerName = "Myserver";

            var env = new Mock<IServer>();
            env.Setup(e => e.EnvironmentID).Returns(serverID);
            env.Setup(e => e.Name).Returns(ServerName);

            var env2 = new Mock<IServer>();
            env2.Setup(e => e.EnvironmentID).Returns(Guid.NewGuid());

            var envRep = new Mock<IServerRepository>();
            envRep.Setup(e => e.All()).Returns(() => new[] { env.Object, env2.Object });

            var content = new DebugState { ServerID = serverID };

            //Execute
            var vm = new DebugStateTreeViewItemViewModelMock(envRep.Object) { Content = content };

            //Assert
            Assert.IsFalse(vm.IsExpanded, "The debug state tree viewmodel should be collapsed if not explicitly expanded in constructor");
        }

        // BUG 8373: TWR
        [TestMethod]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        [Owner("Trevor Williams-Ros")]
        public void DebugStateTreeViewItemViewModel_Constructor_EnvironmentRepository_SetsDebugStateServer()
        {
            var environmentID = Guid.NewGuid();
            const string ServerName = "Myserver";

            var env = new Mock<IServer>();
            env.Setup(e => e.EnvironmentID).Returns(environmentID);
            env.Setup(e => e.Name).Returns(ServerName);

            var env2 = new Mock<IServer>();
            env2.Setup(e => e.EnvironmentID).Returns(Guid.NewGuid());

            var envRep = new Mock<IServerRepository>();
            envRep.Setup(e => e.All()).Returns(() => new[] { env.Object, env2.Object });

            var content = new DebugState { EnvironmentID = environmentID };
            
            new DebugStateTreeViewItemViewModelMock(envRep.Object) { Content = content };
            
            Assert.AreEqual(ServerName, content.Server);
        }
        [TestMethod]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        [Owner("Leon Rajindrapersadh")]
        public void DebugStateTreeViewItemViewModel_Constructor_EnvironmentRepository_SetsDebugStateServer_IfNameSet()
        {
            var environmentID = Guid.NewGuid();
            const string ServerName = "Myserver";

            var env = new Mock<IServer>();
            env.Setup(e => e.EnvironmentID).Returns(environmentID);
            env.Setup(e => e.Name).Returns(ServerName);

            var env2 = new Mock<IServer>();
            env2.Setup(e => e.EnvironmentID).Returns(Guid.NewGuid());

            var envRep = new Mock<IServerRepository>();
            envRep.Setup(e => e.All()).Returns(() => new[] { env.Object, env2.Object });

            var content = new DebugState { EnvironmentID = environmentID ,Server = "BobsServer"};
            
            new DebugStateTreeViewItemViewModelMock(envRep.Object) { Content = content };
            
            Assert.AreEqual("BobsServer", content.Server);
        }
        // BUG 8373: TWR
        [TestMethod]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        [Owner("Trevor Williams-Ros")]
        public void DebugStateTreeViewItemViewModel_Constructor_CanDetectRemoteServerName()
        {
            var serverID = Guid.NewGuid();
            const string ServerName = "Myserver";

            var env = new Mock<IServer>();
            env.Setup(e => e.EnvironmentID).Returns(serverID);
            env.Setup(e => e.Name).Returns(ServerName);


            var env2ID = Guid.NewGuid();

            var env2 = new Mock<IServer>();
            env2.Setup(e => e.EnvironmentID).Returns(env2ID);
            env2.Setup(e => e.Name).Returns("Unknown Remote Server");

            var envRep = new Mock<IServerRepository>();
            envRep.Setup(e => e.All()).Returns(() => new[] { env.Object, env2.Object });

            var content = new DebugState { ServerID = serverID, Server = env2ID.ToString() };
            var vm = new DebugStateTreeViewItemViewModelMock(envRep.Object) { Content = content };
            Assert.AreEqual("Unknown Remote Server", vm.Content.Server);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        public void DebugStateTreeViewItemViewModel_Constructor_NullContent_NoExceptionThrown()
        {
            //------------Setup for test--------------------------
            var envRep = new Mock<IServerRepository>();
            envRep.Setup(r => r.All()).Returns(new List<IServer>());

            //------------Execute Test---------------------------
            var vm = new DebugStateTreeViewItemViewModelMock(envRep.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, vm.Inputs.Count);
            Assert.AreEqual(0, vm.Outputs.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DebugStateTreeViewItemViewModel_Constructor_NullEnvironmentRepository_ExceptionThrown()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var vm = new DebugStateTreeViewItemViewModelMock(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, vm.Inputs.Count);
            Assert.AreEqual(0, vm.Outputs.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        public void DebugStateTreeViewItemViewModel_Constructor_ContentIsMiddleStep_AssignsNameToContentServer()
        {
            Verify_Constructor_AssignsNameToContentServer(StateType.Append);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        public void DebugStateTreeViewItemViewModel_Constructor_ContentIsFirstStep_AssignsNameToContentServer()
        {
            Verify_Constructor_AssignsNameToContentServer(StateType.Start);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        public void DebugStateTreeViewItemViewModel_Constructor_ContentIsLastStep_AssignsNameToContentServer()
        {
            Verify_Constructor_AssignsNameToContentServer(StateType.End);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        public void DebugStateTreeViewItemViewModel_Constructor_ContentServerIsRemote_AssignsUnknownNameToContentServer()
        {
            Verify_Constructor_AssignsNameToContentServer(StateType.Append, true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        public void DebugStateTreeViewItemViewModel_Constructor_ContentWithItems_BindsInputsAndOutputs()
        {
            //------------Setup for test--------------------------
            var envRep = CreateEnvironmentRepository();

            var expected = new DebugState { DisplayName = "IsSelectedTest", ID = Guid.NewGuid(), ActivityType = ActivityType.Step };
            expected.Inputs.Add(new DebugItem(new[] { new DebugItemResult(), new DebugItemResult { GroupName = "group1", GroupIndex = 1 } }));
            expected.Outputs.Add(new DebugItem(new[] { new DebugItemResult(), new DebugItemResult { GroupName = "group1", GroupIndex = 1 } }));

            //------------Execute Test---------------------------
            var vm = new DebugStateTreeViewItemViewModelMock(envRep.Object) { Content = expected };

            //------------Assert Results-------------------------
            Assert.AreEqual(1, vm.Inputs.Count);
            Assert.AreEqual(1, vm.Outputs.Count);
        }


        [TestMethod]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        [Owner("Trevor Williams-Ros")]
        public void DebugStateTreeViewItemViewModel_Constructor_ActivityTypeIsNotWorkflow_PublishesSelectionEventWithActivitySelectionTypeAdd()
        {
            Verify_IsSelected_PublishesDebugSelectionChangedEventArgs(ActivityType.Step, ActivitySelectionType.Add, 2);
        }

        [TestMethod]
        [TestCategory("DebugStateTreeViewItemViewModel_Constructor")]
        [Owner("Trevor Williams-Ros")]
        [DoNotParallelize]
        public void DebugStateTreeViewItemViewModel_Constructor_ActivityTypeIsWorkflow_DoesNotPublishSelectionEventWithActivitySelectionTypeAdd()
        {
            Verify_IsSelected_PublishesDebugSelectionChangedEventArgs(ActivityType.Workflow, ActivitySelectionType.Add, 0);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugStateTreeViewItemViewModel_AppendError")]
        public void DebugStateTreeViewItemViewModel_AppendError_ContentHasError_AppendsErrorToContentError()
        {
            Verify_AppendError(true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugStateTreeViewItemViewModel_AppendError")]
        public void DebugStateTreeViewItemViewModel_AppendError_ContentHasNoError_AppendsErrorToContentError()
        {
            Verify_AppendError(false);
        }

        [TestMethod]
        [TestCategory("DebugStateTreeViewItemViewModel_IsSelected")]
        [Owner("Trevor Williams-Ros")]
        public void DebugStateTreeViewItemViewModel_IsSelected_SetsSelectionTypeToSingle()
        {
            //------------Setup for test--------------------------
            var content = new DebugState { DisplayName = "Error Test", ID = Guid.NewGuid(), ActivityType = ActivityType.Workflow };

            var envRep = CreateEnvironmentRepository();
            var vm = new DebugStateTreeViewItemViewModelMock(envRep.Object) { Content = content, SelectionType = ActivitySelectionType.Add, IsSelected = true };

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(ActivitySelectionType.Single, vm.SelectionType);
        }

        [TestMethod]
        [TestCategory("DebugStateTreeViewItemViewModel_IsSelected")]
        [Owner("Trevor Williams-Ros")]
        public void DebugStateTreeViewItemViewModel_IsSelected_PublishesSelectionEventWithSameActivitySelectionType()
        {
            Verify_IsSelected_PublishesDebugSelectionChangedEventArgs(ActivityType.Service, ActivitySelectionType.None, 2, true);
            Verify_IsSelected_PublishesDebugSelectionChangedEventArgs(ActivityType.Service, ActivitySelectionType.Single, 2, true);
            Verify_IsSelected_PublishesDebugSelectionChangedEventArgs(ActivityType.Service, ActivitySelectionType.Add, 2, true);
            Verify_IsSelected_PublishesDebugSelectionChangedEventArgs(ActivityType.Service, ActivitySelectionType.Remove, 2);
        }

        static void Verify_IsSelected_PublishesDebugSelectionChangedEventArgs(ActivityType activityType, ActivitySelectionType expectedSelectionType, int expectedCount, bool setIsSelected = false)
        {
            var expected = new DebugState { DisplayName = "IsSelectedTest", ID = Guid.NewGuid(), ActivityType = activityType };

            var events = new List<DebugSelectionChangedEventArgs>();

            var selectionChangedEvents = EventPublishers.Studio.GetEvent<DebugSelectionChangedEventArgs>();
            selectionChangedEvents.Subscribe(events.Add);

            var envRep = CreateEnvironmentRepository();

            var vm = new DebugStateTreeViewItemViewModelMock(envRep.Object) { Content = expected };

            if(setIsSelected)
            {
                // clear constructor events
                events.Clear();

                // events are only triggered when property changes to true
                vm.IsSelected = false;

                vm.SelectionType = expectedSelectionType;
                vm.IsSelected = true;
            }
            else
            {
                vm.IsSelected = false;
                vm.SelectionType = expectedSelectionType;
            }

            EventPublishers.Studio.RemoveEvent<DebugSelectionChangedEventArgs>();

            Assert.AreEqual(expectedCount, events.Count);
            if(events.Count > 0)
            {
                var foundEvent = events.Find(args => args.SelectionType == expectedSelectionType);
                Assert.IsNotNull(foundEvent);
                Assert.AreSame(expected, foundEvent.DebugState);
            }
        }

        static void Verify_Constructor_AssignsNameToContentServer(StateType stateType, bool contentServerIsSource = false)
        {
            //------------Setup for test--------------------------
            var environmentID = Guid.NewGuid();
            const string serverName = "TestEnvironment";

            var env = new Mock<IServer>();
            env.Setup(e => e.EnvironmentID).Returns(environmentID);
            env.Setup(e => e.Name).Returns(serverName);

            var envRep = CreateEnvironmentRepository(env.Object);

            var content = new DebugState { Server = (!contentServerIsSource ? Guid.Empty : Guid.NewGuid()).ToString(), EnvironmentID = environmentID, StateType = stateType, DisplayName = "IsSelectedTest", ID = Guid.NewGuid(), ActivityType = ActivityType.Workflow };
            content.OriginalInstanceID = content.ID;


            //------------Execute Test---------------------------
            
            new DebugStateTreeViewItemViewModelMock(envRep.Object) { Content = content };
            

            //------------Assert Results-------------------------
            Assert.AreEqual(serverName, content.Server);
        }

        static void Verify_AppendError(bool contentHasError)
        {
            //------------Setup for test--------------------------
            const string AppendError = "Appended text";
            const string ContentError = "Content text";

            var content = new DebugState { DisplayName = "Error Test", ID = Guid.NewGuid(), ActivityType = ActivityType.Workflow };

            var expectedProps = new[] { "Content.ErrorMessage", "Content.HasError", "Content", "HasError" };
            var actualProps = new List<string>();

            var envRep = CreateEnvironmentRepository();
            var vm = new DebugStateTreeViewItemViewModelMock(envRep.Object) { Content = content };
            vm.PropertyChanged += (sender, args) => actualProps.Add(args.PropertyName);

            //------------Execute Test---------------------------
            vm.Content.HasError = contentHasError;
            vm.Content.ErrorMessage = ContentError;
            vm.AppendError(AppendError);

            //------------Assert Results-------------------------
            if(contentHasError)
            {
                Assert.AreEqual(ContentError + AppendError, content.ErrorMessage);
            }
            else
            {
                Assert.AreEqual(AppendError, content.ErrorMessage);
            }
            Assert.IsTrue(content.HasError);
            Assert.IsTrue(vm.HasError != null && vm.HasError.Value);

            CollectionAssert.AreEqual(expectedProps, actualProps);
        }

        static Mock<IServerRepository> CreateEnvironmentRepository(params IServer[] environments)
        {
            var source = new Mock<IServer>();
            source.Setup(e => e.EnvironmentID).Returns(Guid.Empty);
            source.Setup(e => e.Name).Returns("localhost");

            var envRep = new Mock<IServerRepository>();
            envRep.Setup(r => r.All()).Returns(environments ?? new IServer[0]);
            envRep.Setup(r => r.Source).Returns(source.Object);
            return envRep;
        }
    }

    public class DebugStateTreeViewItemViewModelMock : DebugStateTreeViewItemViewModel
    {
        public DebugStateTreeViewItemViewModelMock(IServerRepository serverRepository)
            : base(serverRepository)
        {
        }
    }

    
}
