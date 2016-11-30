/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.ViewModels.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable CyclomaticComplexity

namespace Dev2.Core.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    // ReSharper disable InconsistentNaming
    public partial class DebugOutputViewModelTest
    {
        static Mock<IResourceRepository> _resourceRepo = new Mock<IResourceRepository>();
        private static Mock<IEnvironmentModel> _environmentModel;
        private static IEnvironmentRepository _environmentRepo;
        private static Mock<IContextualResourceModel> _firstResource;
        const string _resourceName = "TestResource";
        const string _displayName = "test2";
        const string _serviceDefinition = "<x/>";
        private static readonly Guid _serverID = Guid.NewGuid();
        private static readonly Guid _workspaceID = Guid.NewGuid();
        private static readonly Guid _firstResourceID = Guid.NewGuid();
        public static Mock<IPopupController> _popupController;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            CreateFullExportsAndVm();
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DebugOutputViewModel_DefaultPropertyStates()
        {
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);
            Assert.IsNotNull(vm);
            Assert.IsFalse(vm.IsProcessing);
            Assert.IsFalse(vm.IsConfiguring);
            Assert.IsFalse(vm.IsStopping);
            Assert.IsFalse(vm.ShowOptions);
            Assert.IsFalse(vm.ShowVersion);
            Assert.IsTrue(vm.ShowInputs);
            Assert.IsTrue(vm.ShowDebugStatus);
            Assert.IsTrue(vm.ShowDuration);
            Assert.IsTrue(vm.ExpandAllMode);
            Assert.IsTrue(vm.HighlightError);
            Assert.IsTrue(vm.ShowOutputs);
            Assert.IsTrue(vm.ShowServer);
            Assert.IsTrue(vm.ShowTime);
            Assert.IsTrue(vm.ShowType);

            Assert.IsNotNull(vm.DebugImage);
            Assert.IsNotNull(vm.ClearSearchTextCommand);
            Assert.IsNotNull(vm.ExpandAllCommand);
            Assert.IsNotNull(vm.ShowOptionsCommand);
            Assert.IsNotNull(vm.SkipOptionsCommandExecute);

            Assert.IsTrue(string.IsNullOrEmpty(vm.SearchText));

            Assert.AreEqual(default(int), vm.DepthMin);
            Assert.AreEqual(default(int), vm.DepthMax);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEnvironmentIdAndServerLocalhost_AddItemToTreeShould_SetServerAsRemoteEnvironmentModelName()
        {
            var endTime = DateTime.Now;
            var localhost = "localhost";
            var vm = DebugOutputViewModelMock();
            var privateObject = new PrivateObject(vm);
            Assert.IsNotNull(privateObject);

            var contentItems = privateObject.GetField("_contentItems") as List<IDebugState>;
            Assert.IsNotNull(contentItems);
            var countBefore = contentItems.Count;
            Assert.IsTrue(countBefore > 0);
            foreach (var contentItem in contentItems)
                Assert.AreEqual(default(DateTime), contentItem.EndTime);
            var content = new Mock<IDebugState>();
            content.SetupProperty(state => state.StateType, StateType.None);
            content.SetupProperty(state => state.EndTime, endTime);
            content.SetupProperty(state => state.SessionID, Guid.NewGuid());
            content.SetupProperty(state => state.EnvironmentID, Guid.NewGuid());

            content.SetupProperty(state => state.Server, localhost);
            Assert.IsTrue(string.Equals(localhost, content.Object.Server));
            vm.AddItemToTree(content.Object);
            Assert.IsFalse(string.Equals(localhost, content.Object.Server));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEsbServiceInvoker_AppendShould_Return()
        {
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);
            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("Something");

            var mainViewModel = new Mock<IMainViewModel>();
            var mockHelpWindowViewModel = new Mock<IHelpWindowViewModel>();
            mainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpWindowViewModel.Object);
            vm.HighlightError = false;
            var dbgS = new Mock<IDebugState>();
            dbgS.SetupProperty(state => state.SessionID, vm.SessionID);
            dbgS.SetupProperty(state => state.Name, "EsbServiceInvoker");
            vm.Append(dbgS.Object);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenStartState_AppendShould_Return()
        {
            var vm = DebugOutputViewModelMock();
            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("Something");

            var mainViewModel = new Mock<IMainViewModel>();
            var mockHelpWindowViewModel = new Mock<IHelpWindowViewModel>();
            mainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpWindowViewModel.Object);

            var dbgS = new Mock<IDebugState>();
            dbgS.SetupProperty(state => state.SessionID, vm.SessionID);
            dbgS.SetupProperty(state => state.StateType, StateType.Start);
            dbgS.SetupProperty(state => state.EnvironmentID, Guid.NewGuid());
            vm.ShowVersion = true;
            vm.Append(dbgS.Object);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Given_AppendShould_()
        {
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);
            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("Something");

            var mainViewModel = new Mock<IMainViewModel>();
            var mockHelpWindowViewModel = new Mock<IHelpWindowViewModel>();
            mainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpWindowViewModel.Object);
            var dbgS = new Mock<IDebugState>();
            dbgS.SetupProperty(state => state.SessionID, vm.SessionID);
            dbgS.SetupProperty(state => state.StateType, StateType.Duration);
            vm.Append(dbgS.Object);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Given_ShowOptionCommandShould_ShowOptions()
        {
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object)
            { SkipOptionsCommandExecute = true };
            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("Something");

            var mainViewModel = new Mock<IMainViewModel>();
            var mockHelpWindowViewModel = new Mock<IHelpWindowViewModel>();
            mainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpWindowViewModel.Object);

            var dbgS = new Mock<IDebugState>();
            dbgS.SetupProperty(state => state.SessionID, vm.SessionID);
            dbgS.SetupProperty(state => state.StateType, StateType.Start);
            Assert.IsTrue(vm.SkipOptionsCommandExecute);
            vm.ShowOptionsCommand.Execute(null);
            Assert.IsFalse(vm.SkipOptionsCommandExecute);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenShowOptions_ShowOptionCommandShould_ShowOptions()
        {
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);
            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("Something");

            var mainViewModel = new Mock<IMainViewModel>();
            var mockHelpWindowViewModel = new Mock<IHelpWindowViewModel>();
            mainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpWindowViewModel.Object);

            var dbgS = new Mock<IDebugState>();
            dbgS.SetupProperty(state => state.SessionID, vm.SessionID);
            dbgS.SetupProperty(state => state.StateType, StateType.Start);
            vm.SkipOptionsCommandExecute = vm.ShowOptions;
            vm.ShowOptionsCommand.Execute(null);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenPayload_ExpandAllShould_BeSucessful()
        {
            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock1.Setup(s => s.DisconnectedID).Returns(Guid.NewGuid());
            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);
            mock2.SetupGet(m => m.StateType).Returns(StateType.Append);
            mock2.SetupGet(m => m.HasError).Returns(true);
            mock2.SetupGet(m => m.ErrorMessage).Returns("Error Test");
            mock2.Setup(s => s.DisconnectedID).Returns(Guid.NewGuid());
            mock1.SetupSet(s => s.ErrorMessage = It.IsAny<string>())
                .Callback<string>(s => Assert.IsTrue(s.Equals("Error Test")));
            mock1.SetupSet(s => s.HasError = It.IsAny<bool>()).Callback<bool>(s => Assert.IsTrue(s.Equals(true)));

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);

            mock1.Setup(m => m.SessionID).Returns(vm.SessionID);
            mock2.Setup(m => m.SessionID).Returns(vm.SessionID);

            vm.Append(mock1.Object);
            vm.Append(mock2.Object);
            vm.ShowType = false;
            Assert.AreEqual(1, vm.RootItems.Count);
            var root = vm.RootItems.First() as DebugStateTreeViewItemViewModel;
            Assert.IsNotNull(root);
            Assert.IsTrue(root.HasError.GetValueOrDefault(false));
            var mockDebugTree = new Mock<IDebugTreeViewItemViewModel>();
            vm.ExpandAll(mockDebugTree);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenRootItems_DisposeShould_ClearRootItems()
        {
            var vm = DebugOutputViewModelMock();
            Assert.IsNotNull(vm);
            Assert.AreEqual(1, vm.RootItems.Count);
            vm.Dispose();
            Assert.AreEqual(0, vm.RootItems.Count);
        }


        [TestMethod]
        public void DebugOutputViewModel_CanOpenNonNullOrEmptyMoreLinkLineItem()
        {
            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("More");

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);
            Assert.IsTrue(vm.CanOpenMoreLink(lineItem.Object).Equals(true));
        }

        [TestMethod]
        public void DebugOutputViewModel_CantOpenEmptyMoreLinkLineItem()
        {
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);

            var lineItem = new Mock<IDebugLineItem>();
            lineItem.SetupGet(l => l.MoreLink).Returns("");
            Assert.IsTrue(vm.CanOpenMoreLink(lineItem.Object).Equals(false));
        }

        [TestMethod]
        public void DebugOutputViewModel_CantOpenNullMoreLinkLineItem()
        {
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);

            Assert.IsTrue(vm.CanOpenMoreLink(null).Equals(false));
        }


        [TestMethod]
        public void DebugOutputViewModel_AppendErrorExpectErrorMessageAppended()
        {
            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock1.Setup(s => s.DisconnectedID).Returns(Guid.NewGuid());
            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);
            mock2.SetupGet(m => m.StateType).Returns(StateType.Append);
            mock2.SetupGet(m => m.HasError).Returns(true);
            mock2.SetupGet(m => m.ErrorMessage).Returns("Error Test");
            mock2.Setup(s => s.DisconnectedID).Returns(Guid.NewGuid());
            mock1.SetupSet(s => s.ErrorMessage = It.IsAny<string>())
                .Callback<string>(s => Assert.IsTrue(s.Equals("Error Test")));
            mock1.SetupSet(s => s.HasError = It.IsAny<bool>()).Callback<bool>(s => Assert.IsTrue(s.Equals(true)));

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);

            mock1.Setup(m => m.SessionID).Returns(vm.SessionID);
            mock2.Setup(m => m.SessionID).Returns(vm.SessionID);

            vm.Append(mock1.Object);
            vm.Append(mock2.Object);

            Assert.AreEqual(1, vm.RootItems.Count);
            var root = vm.RootItems.First() as DebugStateTreeViewItemViewModel;
            Assert.IsNotNull(root);
            Assert.IsTrue(root.HasError.GetValueOrDefault(false));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenSuccessAsSearchText_ShouldReturnSuccessFromMessage()
        {
            var vm = DebugOutputViewModelMock();
            Assert.AreEqual(1, vm.RootItems.Count);
            vm.SearchText = "Success";
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenNewValueForDepthMinAndMax_ShouldSetNewValue()
        {
            var vm = DebugOutputViewModelMock();
            var initialValForMinAndMax = default(int);
            var newAssignedVal = 250;
            Assert.AreEqual(initialValForMinAndMax, vm.DepthMin);
            Assert.AreEqual(initialValForMinAndMax, vm.DepthMax);
            vm.DepthMax = newAssignedVal;
            vm.DepthMin = newAssignedVal;
            Assert.IsTrue(vm.DepthMin != initialValForMinAndMax && vm.DepthMax != initialValForMinAndMax);
            Assert.IsTrue(vm.DepthMin == newAssignedVal && vm.DepthMax == newAssignedVal);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Clear_ShouldResetTheRoot()
        {
            var vm = DebugOutputViewModelMock();
            Assert.AreEqual(1, vm.RootItems.Count);
            vm.Clear();
            Assert.AreEqual(0, vm.RootItems.Count);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenStateTypeIsDuration_AddItemToTree_ShouldNotAddItem()
        {
            var endTime = DateTime.Now;
            var vm = DebugOutputViewModelMock();
            var privateObject = new PrivateObject(vm);
            Assert.IsNotNull(privateObject);

            var contentItems = privateObject.GetField("_contentItems") as List<IDebugState>;
            Assert.IsNotNull(contentItems);
            var countBefore = contentItems.Count;
            Assert.IsTrue(countBefore > 0);
            foreach (var contentItem in contentItems)
                Assert.AreEqual(default(DateTime), contentItem.EndTime);
            var content = new Mock<IDebugState>();
            content.SetupProperty(state => state.StateType, StateType.Duration);
            content.SetupProperty(state => state.EndTime, endTime);
            vm.AddItemToTree(content.Object);
            Assert.AreEqual(countBefore, contentItems.Count);
        }

        [TestMethod]
        public void DebugOutputViewModel_AppendNestedDebugstatesExpectNestedInRootItems()
        {
            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);

            mock2.SetupGet(m => m.ID).Returns(Guid.NewGuid());
            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);

            mock1.Setup(m => m.SessionID).Returns(vm.SessionID);
            mock1.Setup(m => m.DisconnectedID).Returns(Guid.NewGuid());
            mock2.Setup(m => m.SessionID).Returns(vm.SessionID);
            mock2.Setup(m => m.DisconnectedID).Returns(Guid.NewGuid());
            vm.Append(mock1.Object);
            vm.Append(mock2.Object);
            Assert.AreEqual(1, vm.RootItems.Count);
            var root = vm.RootItems.First() as DebugStateTreeViewItemViewModel;
            Assert.IsNotNull(root);
            Assert.AreEqual(root.Content, mock1.Object, "Root item incorrectly appended");

            var firstChild = root.Children.First() as DebugStateTreeViewItemViewModel;
            Assert.IsNotNull(firstChild);
            Assert.IsTrue(firstChild.Content.ParentID.Equals(_firstResourceID));
        }

        [TestMethod]
        public void DebugOutputViewModel_AppendWhenDebugStateStoppedShouldNotWriteItems()
        {
            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);

            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object)
            { DebugStatus = DebugStatus.Stopping };
            vm.Append(mock1.Object);
            vm.Append(mock2.Object);
            Assert.AreEqual(0, vm.RootItems.Count);
        }

        [TestMethod]
        public void DebugOutputViewModel_AppendWhenDebugStateFinishedShouldNotWriteItems()
        {
            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);

            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object)
            { DebugStatus = DebugStatus.Finished };
            vm.Append(mock1.Object);
            vm.Append(mock2.Object);
            Assert.AreEqual(0, vm.RootItems.Count);
        }

        [TestMethod]
        [TestCategory("DebugOutputViewModel_AppendItem")]
        [Description("DebugOutputViewModel AppendItem must not append item when DebugStatus is finished.")]
        [Owner("Jurie Smit")]
        public void DebugOutputViewModel_AppendWhenDebugStateFinishedShouldNotWriteItems_ItemIsMessage()
        {

            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.Message).Returns("Some message");
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock1.SetupGet(m => m.StateType).Returns(StateType.Message);
            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.Message).Returns("Some message");
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);
            mock1.Setup(m => m.SessionID).Returns(vm.SessionID);
            mock2.Setup(m => m.SessionID).Returns(vm.SessionID);
            vm.DebugStatus = DebugStatus.Finished;
            vm.Append(mock1.Object);
            vm.Append(mock2.Object);
            Assert.AreEqual(1, vm.RootItems.Count);

            var root = vm.RootItems[0] as DebugStringTreeViewItemViewModel;

            Assert.IsNotNull(root);
        }

        [TestMethod]
        [TestCategory("DebugOutputViewModel_OpenItem")]
        [Description("DebugOutputViewModel OpenItem must set the DebugState's properties.")]
        [Owner("Trevor Williams-Ros")]
        public void DebugOutputViewModel_OpenItemWithRemoteEnvironment_OpensResourceFromRemoteEnvironment()
        {

            const string ResourceName = "TestResource";
            var environmentID = Guid.NewGuid();
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(viewModel => viewModel.OpenResource(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Verifiable();
            CustomContainer.Register(mockShellViewModel.Object);
            var envList = new List<IEnvironmentModel>();
            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(e => e.All()).Returns(envList);

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ID).Returns(environmentID);
            env.Setup(e => e.IsConnected).Returns(true);

            // If we get here then we've found the environment based on the environment ID!
            env.Setup(
                e => e.ResourceRepository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false))
                .Verifiable();

            envList.Add(env.Object);

            var model = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepository.Object,
                new Mock<IDebugOutputFilterStrategy>().Object);

            var debugState = new DebugState
            {
                ActivityType = ActivityType.Workflow,
                DisplayName = ResourceName,
                EnvironmentID = environmentID
            };

            model.OpenItemCommand.Execute(debugState);

            mockShellViewModel.Verify(viewModel => viewModel.OpenResource(It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DebugOutputViewModel_OpenItem")]
        public void DebugOutputViewModel_OpenItem_NullEnvironmentIDInDebugState_NoUnhandledExceptions()
        {
            //------------Execute Test---------------------------
            var envRepo = GetEnvironmentRepository();
            new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo,
                new Mock<IDebugOutputFilterStrategy>().Object).OpenItemCommand.Execute(new DebugState
                {
                    ActivityType = ActivityType.Workflow
                });
            new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo,
                new Mock<IDebugOutputFilterStrategy>().Object).OpenItemCommand.Execute(null);

            // Assert NoUnhandledExceptions
            Assert.IsTrue(true, "There where unhandled exceptions");
        }

        [TestMethod]
        [TestCategory("DebugOutputViewModel_AppendItem")]
        [Description("DebugOutputViewModel appendItem must set Debugstatus to finished when DebugItem is final step.")]
        [Owner("Jurie Smit")]
        public void DebugOutputViewModel_AppendItemFinalStep_DebugStatusFinished()
        {
            //*********************Setup********************
            var mock1 = new Mock<IDebugState>();
            mock1.Setup(m => m.IsFinalStep()).Returns(true);
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);
            mock1.Setup(m => m.SessionID).Returns(vm.SessionID);
            vm.DebugStatus = DebugStatus.Ready;

            //*********************Test********************
            vm.Append(mock1.Object);

            //*********************Assert*******************
            Assert.AreEqual(vm.DebugStatus, DebugStatus.Finished);
        }

        [TestMethod]
        [TestCategory("DebugOutputViewModel_AppendItem")]
        [Description("DebugOutputViewModel appendItem must set Debugstatus to finished when DebugItem is final step.")]
        [Owner("Jurie Smit")]
        public void DebugOutputViewModel_AppendItemNotFinalStep_DebugStatusUnchanced()
        {
            //*********************Setup********************
            var mock1 = new Mock<IDebugState>();
            mock1.Setup(m => m.IsFinalStep()).Returns(false);
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object)
            { DebugStatus = DebugStatus.Ready };

            //*********************Test********************
            vm.Append(mock1.Object);

            //*********************Assert*******************
            Assert.AreEqual(vm.DebugStatus, DebugStatus.Ready);
        }

        #region PendingQueue

        // BUG 9735 - 2013.06.22 - TWR : added
        [TestMethod]
        public void DebugOutputViewModel_PendingQueueExpectedQueuesMessagesAndFlushesWhenFinishedProcessing()
        {

            var envRepo = GetEnvironmentRepository();

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo,
                new Mock<IDebugOutputFilterStrategy>().Object)
            { DebugStatus = DebugStatus.Executing };
            for (var i = 0; i < 10; i++)
            {
                var state = new Mock<IDebugState>();
                var stateType = i % 2 == 0 ? StateType.Message : StateType.After;
                state.Setup(s => s.StateType).Returns(stateType);
                state.Setup(s => s.SessionID).Returns(vm.SessionID);
                state.Setup(s => s.DisconnectedID).Returns(Guid.NewGuid());
                vm.Append(state.Object);
            }

            Assert.AreEqual(5, vm.PendingItemCount);
            Assert.AreEqual(5, vm.ContentItemCount);

            vm.DebugStatus = DebugStatus.Finished;

            Assert.AreEqual(0, vm.PendingItemCount);
            Assert.AreEqual(10, vm.ContentItemCount);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_WhenDebugIsInFinishedState_MessageIsAddedBeforeAndAfterLastItem()
        {

            var envRepo = GetEnvironmentRepository();

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo,
                new Mock<IDebugOutputFilterStrategy>().Object)
            { DebugStatus = DebugStatus.Finished };

            var state = new Mock<IDebugState>();
            state.Setup(s => s.StateType).Returns(StateType.After);
            state.Setup(s => s.SessionID).Returns(vm.SessionID);
            state.Setup(s => s.DisconnectedID).Returns(Guid.NewGuid());
            vm.Append(state.Object);

            state = new Mock<IDebugState>();
            state.Setup(s => s.StateType).Returns(StateType.Message);
            state.Setup(s => s.Message).Returns("Some random message");
            state.Setup(s => s.SessionID).Returns(vm.SessionID);
            state.Setup(s => s.DisconnectedID).Returns(Guid.NewGuid());
            vm.Append(state.Object);

            Assert.AreEqual(3, vm.RootItems.Count);
            Assert.IsInstanceOfType(vm.RootItems[0], typeof(DebugStringTreeViewItemViewModel));
            Assert.IsInstanceOfType(vm.RootItems[1], typeof(DebugStateTreeViewItemViewModel));
            Assert.IsInstanceOfType(vm.RootItems[2], typeof(DebugStringTreeViewItemViewModel));
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_WhenDebugIsInStoppingState_ZeroItemsAddedToTree()
        {

            var envRepo = GetEnvironmentRepository();

            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo,
                new Mock<IDebugOutputFilterStrategy>().Object)
            { DebugStatus = DebugStatus.Stopping };

            var state = new Mock<IDebugState>();
            state.Setup(s => s.StateType).Returns(StateType.After);
            state.Setup(s => s.SessionID).Returns(vm.SessionID);
            vm.Append(state.Object);

            Assert.AreEqual(0, vm.RootItems.Count);
        }

        #endregion

        static void CreateFullExportsAndVm()
        {
            CreateEnvironmentModel();
            _environmentRepo = GetEnvironmentRepository();
            _popupController = new Mock<IPopupController>();

        }

        public static Mock<IContextualResourceModel> CreateResource(ResourceType resourceType)
        {
            var result = new Mock<IContextualResourceModel>();

            result.Setup(c => c.ResourceName).Returns(_resourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c => c.DisplayName).Returns(_displayName);
            result.Setup(c => c.WorkflowXaml).Returns(new StringBuilder(_serviceDefinition));
            result.Setup(c => c.Category).Returns("Testing");
            result.Setup(c => c.Environment).Returns(_environmentModel.Object);
            result.Setup(c => c.ServerID).Returns(_serverID);
            result.Setup(c => c.ID).Returns(_firstResourceID);

            return result;
        }

        public static IEnvironmentRepository GetEnvironmentRepository()
        {
            var models = new List<IEnvironmentModel> { _environmentModel.Object };
            var mock = new Mock<IEnvironmentRepository>();
            mock.Setup(s => s.All()).Returns(models);
            mock.Setup(s => s.IsLoaded).Returns(true);
            mock.Setup(repository => repository.Source).Returns(_environmentModel.Object);
            mock.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>()))
                .Returns(_environmentModel.Object);
            _environmentRepo = mock.Object;
            return _environmentRepo;
        }

        static void CreateEnvironmentModel()
        {
            _environmentModel = CreateMockEnvironment();
            _resourceRepo = new Mock<IResourceRepository>();

            _firstResource = CreateResource(ResourceType.WorkflowService);
            var coll = new Collection<IResourceModel> { _firstResource.Object };
            _resourceRepo.Setup(c => c.All()).Returns(coll);


            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);
        }

        public static Mock<IEnvironmentModel> CreateMockEnvironment(params string[] sources)
        {
            var rand = new Random();
            var connection = CreateMockConnection(rand, sources);

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.Connection).Returns(connection.Object);
            env.Setup(e => e.IsConnected).Returns(true);
            env.Setup(e => e.ID).Returns(Guid.NewGuid());

            env.Setup(e => e.Name).Returns($"Server_{rand.Next(1, 100)}");

            return env;
        }

        public static Mock<IEnvironmentConnection> CreateMockConnection(Random rand, params string[] sources)
        {

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerID).Returns(new Guid());
            connection.Setup(c => c.AppServerUri)
                .Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}/dsf"));
            connection.Setup(c => c.WebServerUri)
                .Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}"));
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(new StringBuilder($"<XmlData>{string.Join("\n", sources)}</XmlData>"));

            return connection;
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_IsStopping")]
        public void DebugOutputViewModel_IsStopping_ReflectsDebugStatus()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo,
                new Mock<IDebugOutputFilterStrategy>().Object)
            { DebugStatus = DebugStatus.Stopping };

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsStopping);

            viewModel.DebugStatus = DebugStatus.Ready;
            Assert.IsFalse(viewModel.IsStopping);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_IsProcessing")]
        public void DebugOutputViewModel_IsProcessing_ReflectsDebugStatus()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo,
                new Mock<IDebugOutputFilterStrategy>().Object)
            { DebugStatus = DebugStatus.Executing };

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsProcessing);

            viewModel.DebugStatus = DebugStatus.Configure;
            Assert.IsTrue(viewModel.IsProcessing);

            viewModel.DebugStatus = DebugStatus.Ready;
            Assert.IsFalse(viewModel.IsProcessing);

            viewModel.DebugStatus = DebugStatus.Finished;
            Assert.IsFalse(viewModel.IsProcessing);

            viewModel.DebugStatus = DebugStatus.Stopping;
            Assert.IsFalse(viewModel.IsProcessing);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_DebugText")]
        public void DebugOutputViewModel_DebugText_ReflectsDebugStatus()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo,
                new Mock<IDebugOutputFilterStrategy>().Object)
            { DebugStatus = DebugStatus.Executing };

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual("Stop", viewModel.DebugText);

            viewModel.DebugStatus = DebugStatus.Ready;
            Assert.AreEqual("Debug", viewModel.DebugText);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_DebugStatus")]
        public void DebugOutputViewModel_DebugStatus_Executing_PublishesDebugSelectionChangedEventArgs()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo,
                new Mock<IDebugOutputFilterStrategy>().Object);

            var expectedProps = new[]
            {"IsStopping", "IsProcessing", "IsConfiguring", "DebugImage", "DebugText", "ProcessingText"};
            var actualProps = new List<string>();
            viewModel.PropertyChanged += (sender, args) => actualProps.Add(args.PropertyName);

            var events = new List<DebugSelectionChangedEventArgs>();

            var selectionChangedEvents = EventPublishers.Studio.GetEvent<DebugSelectionChangedEventArgs>();
            selectionChangedEvents.Subscribe(events.Add);

            //------------Execute Test---------------------------
            viewModel.DebugStatus = DebugStatus.Executing;

            EventPublishers.Studio.RemoveEvent<DebugSelectionChangedEventArgs>();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(ActivitySelectionType.None, events[0].SelectionType);
            Assert.IsNull(events[0].DebugState);

            CollectionAssert.AreEqual(expectedProps, actualProps);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_SelectAll")]
        public void DebugOutputViewModel_SelectAll_Execute_PublishesClearSelection()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo,
                new Mock<IDebugOutputFilterStrategy>().Object);

            var events = new List<DebugSelectionChangedEventArgs>();

            var selectionChangedEvents = EventPublishers.Studio.GetEvent<DebugSelectionChangedEventArgs>();
            selectionChangedEvents.Subscribe(events.Add);

            //------------Execute Test---------------------------
            viewModel.SelectAllCommand.Execute(null);

            EventPublishers.Studio.RemoveEvent<DebugSelectionChangedEventArgs>();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(ActivitySelectionType.None, events[0].SelectionType);
            Assert.IsNull(events[0].DebugState);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_SelectAll")]
        public void DebugOutputViewModel_SelectAll_Execute_AllDebugStateItemsSelected()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo,
                new Mock<IDebugOutputFilterStrategy>().Object);

            viewModel.RootItems.Add(CreateItemViewModel<DebugStringTreeViewItemViewModel>(envRepo, 0, false, null));

            for (var i = 1; i < 6; i++)
            {
                var item = CreateItemViewModel<DebugStateTreeViewItemViewModel>(envRepo, i, false, null);
                for (var j = 0; j < 2; j++)
                {
                    CreateItemViewModel<DebugStateTreeViewItemViewModel>(envRepo, j, false, item);
                }
                viewModel.RootItems.Add(item);
            }

            //------------Execute Test---------------------------
            viewModel.SelectAllCommand.Execute(null);


            //------------Assert Results-------------------------

            // Items are selected only if they are DebugStateTreeViewItemViewModel's
            foreach (var item in viewModel.RootItems)
            {
                Assert.AreEqual(item is DebugStateTreeViewItemViewModel, item.IsSelected);
                foreach (var child in item.Children)
                {
                    Assert.AreEqual(child is DebugStateTreeViewItemViewModel, child.IsSelected);
                }
            }
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DebugOutputViewModel_Constructor")]
        public void DebugOutputViewModel_Constructor_ViewModelSessionIDAndEnvironmentRepoProperlyInitialized()
        {
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();

            //------------Execute Test---------------------------
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object,
                new Mock<IDebugOutputFilterStrategy>().Object);

            // Assert View Model Session ID And Environment Repo Properly Initialized
            Assert.AreNotEqual(Guid.Empty, debugOutputViewModel.SessionID, "Session ID not properly initialized");
            Assert.AreEqual(mockedEnvRepo.Object, debugOutputViewModel.EnvironmentRepository,
                "Environment Repo not initialized");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DebugOutputViewModel_ProcessingText")]
        public void DebugOutputViewModel_ProcessingText_ShouldReturnDescriptionOfDebugStatus()
        {
            //------------Setup for test--------------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object,
                new Mock<IDebugOutputFilterStrategy>().Object)
            { DebugStatus = DebugStatus.Finished };
            //------------Execute Test---------------------------
            string processingText = debugOutputViewModel.ProcessingText;
            //------------Assert Results-------------------------
            Assert.AreEqual("Ready", processingText);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddNewTestCommand_GivenIsNew_ShouldNotExecute()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object, resourceModel.Object) { DebugStatus = DebugStatus.Finished };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(debugOutputViewModel.AddNewTestCommand);
            //---------------Execute Test ----------------------
            var canExecute = debugOutputViewModel.AddNewTestCommand.CanExecute(null);
            //---------------Test Result -----------------------
            Assert.IsFalse(canExecute);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddNewTestCommand_GivenIsNewNoresourceCommand_ShouldNotExecute()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object) { DebugStatus = DebugStatus.Finished };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(debugOutputViewModel.AddNewTestCommand);
            //---------------Execute Test ----------------------
            var canExecute = debugOutputViewModel.AddNewTestCommand.CanExecute(null);
            //---------------Test Result -----------------------
            Assert.IsFalse(canExecute);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ShowDebugStatus_GivenHasChanged_ShouldFirePropertyChanged()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object) { DebugStatus = DebugStatus.Finished };
            bool wasCalled=false;
            debugOutputViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ShowDebugStatus")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(debugOutputViewModel.ShowDebugStatus);
            //---------------Execute Test ----------------------
            debugOutputViewModel.ShowDebugStatus = false;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ShowAssertResult_GivenHasChanged_ShouldFirePropertyChanged()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object) { DebugStatus = DebugStatus.Finished };
            bool wasCalled=false;
            debugOutputViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ShowAssertResult")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(debugOutputViewModel.ShowAssertResult);
            //---------------Execute Test ----------------------
            debugOutputViewModel.ShowAssertResult = false;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ShowOutputs_GivenHasChanged_ShouldFirePropertyChanged()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object) { DebugStatus = DebugStatus.Finished };
            bool wasCalled=false;
            debugOutputViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ShowOutputs")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(debugOutputViewModel.ShowOutputs);
            //---------------Execute Test ----------------------
            debugOutputViewModel.ShowOutputs = false;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ShowInputs_GivenHasChanged_ShouldFirePropertyChanged()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object) { DebugStatus = DebugStatus.Finished };
            bool wasCalled=false;
            debugOutputViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ShowInputs")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(debugOutputViewModel.ShowInputs);
            //---------------Execute Test ----------------------
            debugOutputViewModel.ShowInputs = false;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ShowDuration_GivenHasChanged_ShouldFirePropertyChanged()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object) { DebugStatus = DebugStatus.Finished };
            bool wasCalled=false;
            debugOutputViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ShowDuration")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(debugOutputViewModel.ShowDuration);
            //---------------Execute Test ----------------------
            debugOutputViewModel.ShowDuration = false;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ShowTime_GivenHasChanged_ShouldFirePropertyChanged()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object) { DebugStatus = DebugStatus.Finished };
            bool wasCalled=false;
            debugOutputViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ShowTime")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(debugOutputViewModel.ShowTime);
            //---------------Execute Test ----------------------
            debugOutputViewModel.ShowTime = false;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ShowServer_GivenHasChanged_ShouldFirePropertyChanged()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object) { DebugStatus = DebugStatus.Finished };
            bool wasCalled=false;
            debugOutputViewModel.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "ShowServer")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(debugOutputViewModel.ShowServer);
            //---------------Execute Test ----------------------
            debugOutputViewModel.ShowServer = false;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_GivenIsResourceModel_ShouldSetField()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var debugOutpuViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object, resourceModel.Object) ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(debugOutpuViewModel.AddNewTestCommand);
            //---------------Execute Test ----------------------
            var fieldInfo = typeof(DebugOutputViewModel).GetField("_contextualResourceModel", BindingFlags.NonPublic | BindingFlags.Instance);
            // ReSharper disable once PossibleNullReferenceException
            var value =(IContextualResourceModel) fieldInfo.GetValue(debugOutpuViewModel);
            //---------------Test Result -----------------------
            Assert.IsNotNull(value);
            Assert.IsTrue(ReferenceEquals(value, resourceModel.Object));
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CanAddNewTest_GivenNoContextualResourceModel_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mock = new Mock<IEventAggregator>();
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            mock.Setup(aggregator => aggregator.Publish(newTestFromDebugMessage)).Verifiable();

            var debugOutpuViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object);
            var methodInfo = typeof(DebugOutputViewModel).GetMethod("CanAddNewTest", BindingFlags.Instance | BindingFlags.NonPublic);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(debugOutpuViewModel, new object[] { });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CanAddNewTest_GivenContextualResourceModelNoRootItems_ShouldReturnfalse()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mock = new Mock<IEventAggregator>();
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            mock.Setup(aggregator => aggregator.Publish(newTestFromDebugMessage)).Verifiable();

            var debugOutpuViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object, resourceModel.Object);
            var methodInfo = typeof(DebugOutputViewModel).GetMethod("CanAddNewTest", BindingFlags.Instance | BindingFlags.NonPublic);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(debugOutpuViewModel, new object[] { });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CanAddNewTest_GivenContextualResourceModelAndRootItems_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mock = new Mock<IEventAggregator>();
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            mock.Setup(aggregator => aggregator.Publish(newTestFromDebugMessage)).Verifiable();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(true);
            var debugOutpuViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object, resourceModel.Object);
            var methodInfo = typeof(DebugOutputViewModel).GetMethod("CanAddNewTest", BindingFlags.Instance | BindingFlags.NonPublic);
            debugOutpuViewModel.RootItems.Add(new DebugStateTreeViewItemViewModel(new Mock<IEnvironmentRepository>().Object));
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var invoke = methodInfo.Invoke(debugOutpuViewModel, new object[] { });
            //---------------Test Result -----------------------
            Assert.IsTrue(bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Hagshen Naidu")]
        public void CanAddNewTest_GivenIsTestView_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mock = new Mock<IEventAggregator>();
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            mock.Setup(aggregator => aggregator.Publish(newTestFromDebugMessage)).Verifiable();
            resourceModel.Setup(model => model.IsWorkflowSaved).Returns(false);
            var debugOutpuViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object, resourceModel.Object);
            debugOutpuViewModel.IsTestView = true;
            debugOutpuViewModel.RootItems.Add(new DebugStateTreeViewItemViewModel(new Mock<IEnvironmentRepository>().Object));
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var execute = debugOutpuViewModel.AddNewTestCommand.CanExecute(null);
            //---------------Test Result -----------------------
            Assert.IsTrue(execute);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddNewTest_GivenEventAggregator_ShouldPublishCorrectly()
        {
            //---------------Set up test pack-------------------
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var mock = new Mock<IEventAggregator>();
            var newTestFromDebugMessage = new NewTestFromDebugMessage();
            mock.Setup(aggregator => aggregator.Publish(newTestFromDebugMessage)).Verifiable();

            var debugOutpuViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object, resourceModel.Object);
            var methodInfo = typeof(DebugOutputViewModel).GetMethod("AddNewTest", BindingFlags.Instance | BindingFlags.NonPublic);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            methodInfo.Invoke(debugOutpuViewModel, new object[] { mock.Object });
            //---------------Test Result -----------------------
            mock.Verify(aggregator => aggregator.Publish(It.IsAny<IMessage>()), Times.Once);
        }

        [TestMethod]
        public void TestUpdateHelpDescriptor()
        {
            //arrange
            var mockedEnvRepo = new Mock<IEnvironmentRepository>();
            var debugOutputViewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, mockedEnvRepo.Object,
                new Mock<IDebugOutputFilterStrategy>().Object)
            { DebugStatus = DebugStatus.Finished };

            var mainViewModelMock = new Mock<IMainViewModel>();
            var helpViewModelMock = new Mock<IHelpWindowViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);
            var helpText = "someText";

            //act
            debugOutputViewModel.UpdateHelpDescriptor(helpText);

            //assert
            helpViewModelMock.Verify(it => it.UpdateHelpText(helpText));
        }

        #region Setup Methods

        static IDebugTreeViewItemViewModel CreateItemViewModel<T>(IEnvironmentRepository envRepository, int n,
            bool isSelected, IDebugTreeViewItemViewModel parent)
            where T : IDebugTreeViewItemViewModel
        {
            if (typeof(T) == typeof(DebugStateTreeViewItemViewModel))
            {
                var content = new DebugState
                {
                    DisplayName = "State " + n,
                    ID = Guid.NewGuid(),
                    ActivityType = ActivityType.Step
                };
                var viewModel = new DebugStateTreeViewItemViewModel(envRepository) { Parent = parent, Content = content };
                if (!isSelected)
                {
                    // viewModel.IsSelected is true when the ActivityType is Step
                    viewModel.IsSelected = false;
                }
                return viewModel;
            }

            return new DebugStringTreeViewItemViewModel
            {
                Content = "String " + n,
                IsSelected = isSelected
            };
        }

        private static DebugOutputViewModel DebugOutputViewModelMock()
        {
            var mock1 = new Mock<IDebugState>();
            var mock2 = new Mock<IDebugState>();
            mock1.SetupGet(m => m.ID).Returns(_firstResourceID);
            mock1.SetupGet(m => m.ServerID).Returns(_serverID);
            mock1.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock1.Setup(s => s.DisconnectedID).Returns(Guid.NewGuid());
            mock2.SetupGet(m => m.ServerID).Returns(_serverID);
            mock2.SetupGet(m => m.WorkspaceID).Returns(_workspaceID);
            mock2.SetupGet(m => m.ParentID).Returns(_firstResourceID);
            mock2.SetupGet(m => m.StateType).Returns(StateType.Append);
            mock2.SetupGet(m => m.HasError).Returns(true);
            mock2.SetupGet(m => m.ErrorMessage).Returns("Error Test");
            mock2.Setup(s => s.DisconnectedID).Returns(Guid.NewGuid());
            mock1.SetupSet(s => s.ErrorMessage = It.IsAny<string>())
                .Callback<string>(s => Assert.IsTrue(s.Equals("Error Test")));
            mock1.SetupSet(s => s.HasError = It.IsAny<bool>()).Callback<bool>(s => Assert.IsTrue(s.Equals(true)));
            var vm = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, GetEnvironmentRepository(),
                new Mock<IDebugOutputFilterStrategy>().Object);

            mock1.Setup(m => m.SessionID).Returns(vm.SessionID);
            mock2.Setup(m => m.SessionID).Returns(vm.SessionID);

            vm.Append(mock1.Object);
            vm.Append(mock2.Object);
            return vm;
        }

        #endregion
    }
}