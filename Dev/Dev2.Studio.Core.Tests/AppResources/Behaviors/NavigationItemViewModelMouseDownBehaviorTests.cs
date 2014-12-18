
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Services.Security;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Threading;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Models;
using Dev2.Services.Security;
using Dev2.Studio.AppResources.Behaviors;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.AppResources.Behaviors
{
    [TestClass]
    public class NavigationItemViewModelMouseDownBehaviorTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NavigationItemViewModelMouseDownBehavior_Constructor_EventPublisherIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new NavigationItemViewModelMouseDownBehavior(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_Attach")]
        public void NavigationItemViewModelMouseDownBehavior_Attach_AssociatedObjectIsNull_DoesNotSubscribesToEvents()
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Subscribe(It.IsAny<object>())).Verifiable();

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);

            //------------Execute Test---------------------------
            behavior.Attach(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, behavior.SubscribeToEventsHitCount);
            eventPublisher.Verify(p => p.Subscribe(It.IsAny<object>()), Times.Never());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_Attach")]
        public void NavigationItemViewModelMouseDownBehavior_Attach_AssociatedObjectIsNotNull_SubscribesToEvents()
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Subscribe(It.IsAny<object>())).Verifiable();

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);

            //------------Execute Test---------------------------
            behavior.Attach(new FrameworkElement());

            //------------Assert Results-------------------------
            Assert.AreEqual(1, behavior.SubscribeToEventsHitCount);
            eventPublisher.Verify(p => p.Subscribe(It.IsAny<object>()));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_Detach")]
        public void NavigationItemViewModelMouseDownBehavior_Detach_AssociatedObjectIsNull_DoesNotSubscribesToEvents()
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Unsubscribe(It.IsAny<object>())).Verifiable();

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);

            //------------Execute Test---------------------------
            behavior.Detach();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, behavior.SubscribeToEventsHitCount);
            eventPublisher.Verify(p => p.Unsubscribe(It.IsAny<object>()), Times.Never());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_Detach")]
        public void NavigationItemViewModelMouseDownBehavior_Detach_AssociatedObjectIsNotNull_SubscribesToEvents()
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Unsubscribe(It.IsAny<object>())).Verifiable();

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);
            behavior.Attach(new FrameworkElement());

            //------------Execute Test---------------------------
            behavior.Detach();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, behavior.UnsubscribeFromEventsHitCount);
            eventPublisher.Verify(p => p.Unsubscribe(It.IsAny<object>()));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_SelectOnRightClick_TreeNodeIsSelectedMatches()
        {
            Verify_OnMouseDown_SelectOnRightClick(true);
            Verify_OnMouseDown_SelectOnRightClick(true);
        }

        static void Verify_OnMouseDown_SelectOnRightClick(bool selectOnRightClick)
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object) { SelectOnRightClick = selectOnRightClick };

            var explorerItemModel = new ExplorerItemModel(new Mock<IStudioResourceRepository>().Object, new Mock<IAsyncWorker>().Object, new Mock<IConnectControlSingleton>().Object);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(explorerItemModel, 1);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            Assert.AreEqual(selectOnRightClick, explorerItemModel.IsExplorerSelected);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_SetActiveEnvironmentOnClick_PublishEventsCorrectly()
        {
            TestEnvironmentRespository testEnvironmentRespository = new TestEnvironmentRespository(new Mock<IEnvironmentModel>().Object);
            // ReSharper disable once ObjectCreationAsStatement
            new EnvironmentRepository(testEnvironmentRespository);
            Verify_OnMouseDown_SetActiveEnvironmentOnClick(0, false);
            Verify_OnMouseDown_SetActiveEnvironmentOnClick(0, false);
            Verify_OnMouseDown_SetActiveEnvironmentOnClick(1, true);
        }

        static void Verify_OnMouseDown_SetActiveEnvironmentOnClick(int publishCount, bool setActiveEnvironmentOnClick)
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object) { SetActiveEnvironmentOnClick = setActiveEnvironmentOnClick };

            var explorerItemModel = new ExplorerItemModel(new Mock<IStudioResourceRepository>().Object, new Mock<IAsyncWorker>().Object, new Mock<IConnectControlSingleton>().Object);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(explorerItemModel, 1);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            eventPublisher.Verify(p => p.Publish(It.IsAny<SetActiveEnvironmentMessage>()), Times.Exactly(publishCount));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_TreecodeIsNotResourceTreeViewModel_PublishEventsCorrectly()
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);

            var explorerItemModel = new ExplorerItemModel(new Mock<IStudioResourceRepository>().Object, new Mock<IAsyncWorker>().Object, new Mock<IConnectControlSingleton>().Object);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(explorerItemModel, 1);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_TreeNodeIsResourceTreeViewModelWithNullDataContext_DoesNothing()
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<object>())).Verifiable();

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment).Returns(environmentModel.Object);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(null, 1);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            eventPublisher.Verify(p => p.Publish(It.IsAny<object>()), Times.Never());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_UserIsNotAuthorized_DoesNothing()
        {
            Verify_OnMouseDown_UserIsNotAuthorized(0, false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_UserIsAuthorized_InvokesLogic()
        {
            Verify_OnMouseDown_UserIsNotAuthorized(1, true);
        }

        void Verify_OnMouseDown_UserIsNotAuthorized(int editHitCount, bool isAuthorized)
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);

            // ensure EditCommand is ONLY being blocked by authorization check!
            const int ClickCount = 2;
            behavior.DontAllowDoubleClick = false;
            behavior.OpenOnDoubleClick = true;

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var editCommand = new Mock<ICommand>();
            editCommand.Setup(c => c.CanExecute(It.IsAny<object>())).Returns(true);
            editCommand.Setup(c => c.Execute(It.IsAny<object>())).Verifiable();

            Mock<IExplorerItemModel> mock = new Mock<IExplorerItemModel>();
            mock.Setup(model => model.Permissions).Returns(isAuthorized ? Permissions.Administrator : Permissions.None);
            mock.Setup(model => model.EditCommand).Returns(editCommand.Object);
            mock.Setup(model => model.CanEdit).Returns(true);
            var explorerItemModel = mock.Object;
            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(explorerItemModel, ClickCount);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            editCommand.Verify(c => c.Execute(It.IsAny<object>()), Times.Exactly(editHitCount));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_OpenOnDoubleClickIsFalse_DoesNothing()
        {
            Verify_OnMouseDown_OpenOnDoubleClick_DoesNothing(openOnDoubleClick: false, clickCount: 1);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_OpenOnDoubleClickIsTrueAndClickCountGreaterThan2_DoesNothing()
        {
            Verify_OnMouseDown_OpenOnDoubleClick_DoesNothing(openOnDoubleClick: true, clickCount: 3);
        }

        static void Verify_OnMouseDown_OpenOnDoubleClick_DoesNothing(int clickCount, bool openOnDoubleClick)
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<object>())).Verifiable();

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object) { OpenOnDoubleClick = openOnDoubleClick };

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment).Returns(environmentModel.Object);
            resourceModel.Setup(r => r.IsAuthorized(AuthorizationContext.View)).Returns(true);

            var explorerItemModel = new ExplorerItemModel(new Mock<IStudioResourceRepository>().Object, new Mock<IAsyncWorker>().Object, new Mock<IConnectControlSingleton>().Object) { Permissions = Permissions.Administrator };
            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(explorerItemModel, clickCount);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            eventPublisher.Verify(p => p.Publish(It.IsAny<object>()), Times.Never());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_OpenOnDoubleClickIsTrueAndClickCountIsOne_PublishesEvent()
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object) { OpenOnDoubleClick = true };

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment).Returns(environmentModel.Object);
            resourceModel.Setup(r => r.IsAuthorized(AuthorizationContext.View)).Returns(true);

            var explorerItemModel = new ExplorerItemModel(new Mock<IStudioResourceRepository>().Object, new Mock<IAsyncWorker>().Object, new Mock<IConnectControlSingleton>().Object);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(explorerItemModel, 1);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_OpenOnDoubleClickIsTrueAndClickCountIsTwo_PublishesEvent()
        {
            Verify_OnMouseDown_OpenOnDoubleClickIsTrueAndClickCountIsTwo(true);
            Verify_OnMouseDown_OpenOnDoubleClickIsTrueAndClickCountIsTwo(true);
        }

        static void Verify_OnMouseDown_OpenOnDoubleClickIsTrueAndClickCountIsTwo(bool dontAllowDoubleClick)
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object) { OpenOnDoubleClick = true, DontAllowDoubleClick = dontAllowDoubleClick };

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment).Returns(environmentModel.Object);
            resourceModel.Setup(r => r.IsAuthorized(AuthorizationContext.View)).Returns(true);

            var editCommand = new Mock<ICommand>();
            editCommand.Setup(c => c.CanExecute(It.IsAny<object>())).Returns(true);
            editCommand.Setup(c => c.Execute(It.IsAny<object>())).Verifiable();

            var explorerItemModel = new ExplorerItemModel(new Mock<IStudioResourceRepository>().Object, new Mock<IAsyncWorker>().Object, new Mock<IConnectControlSingleton>().Object);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(explorerItemModel, 2);

            //------------Assert Results-------------------------
            Assert.AreEqual(dontAllowDoubleClick, result);
            editCommand.Verify(c => c.Execute(It.IsAny<object>()), Times.Exactly(dontAllowDoubleClick ? 0 : 1));
        }
    }

    public class TestNavigationItemViewModelMouseDownBehavior : NavigationItemViewModelMouseDownBehavior
    {
        public TestNavigationItemViewModelMouseDownBehavior(IEventAggregator eventPublisher)
            : base(eventPublisher)
        {
        }

        public int SubscribeToEventsHitCount { get; private set; }
        protected override void SubscribeToEvents()
        {
            SubscribeToEventsHitCount++;
            base.SubscribeToEvents();
        }

        public int UnsubscribeFromEventsHitCount { get; private set; }
        protected override void UnsubscribeFromEvents()
        {
            UnsubscribeFromEventsHitCount++;
            base.UnsubscribeFromEvents();
        }

        public bool TestOnMouseDown(IExplorerItemModel treenode, int clickCount)
        {
            return OnMouseDown(treenode, clickCount);
        }
    }
}
