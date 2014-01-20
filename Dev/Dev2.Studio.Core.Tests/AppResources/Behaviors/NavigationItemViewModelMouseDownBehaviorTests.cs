using System;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Providers.Events;
using Dev2.Services.Security;
using Dev2.Studio.AppResources.Behaviors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            var behavior = new NavigationItemViewModelMouseDownBehavior(null);

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
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_TreeNodeNull_DoesNothing()
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(null, 1);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_SelectOnRightClick_TreeNodeIsSelectedMatches()
        {
            Verify_OnMouseDown_SelectOnRightClick(true);
            Verify_OnMouseDown_SelectOnRightClick(false);
        }

        static void Verify_OnMouseDown_SelectOnRightClick(bool selectOnRightClick)
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);
            behavior.SelectOnRightClick = selectOnRightClick;

            var treeNode = new Mock<ITreeNode>();
            treeNode.SetupProperty(n => n.IsSelected);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(treeNode.Object, 1);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            Assert.AreEqual(selectOnRightClick, treeNode.Object.IsSelected);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_SetActiveEnvironmentOnClick_PublishEventsCorrectly()
        {
            Verify_OnMouseDown_SetActiveEnvironmentOnClick(publishCount: 0, setActiveEnvironmentOnClick: false, treeNodeHasEnvironment: true);
            Verify_OnMouseDown_SetActiveEnvironmentOnClick(publishCount: 0, setActiveEnvironmentOnClick: true, treeNodeHasEnvironment: false);
            Verify_OnMouseDown_SetActiveEnvironmentOnClick(publishCount: 1, setActiveEnvironmentOnClick: true, treeNodeHasEnvironment: true);
        }

        static void Verify_OnMouseDown_SetActiveEnvironmentOnClick(int publishCount, bool setActiveEnvironmentOnClick, bool treeNodeHasEnvironment)
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<SetActiveEnvironmentMessage>())).Verifiable();

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);
            behavior.SetActiveEnvironmentOnClick = setActiveEnvironmentOnClick;

            var treeNode = new Mock<ITreeNode>();
            treeNode.SetupProperty(n => n.IsSelected);
            treeNode.Setup(n => n.EnvironmentModel).Returns(treeNodeHasEnvironment ? new Mock<IEnvironmentModel>().Object : null);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(treeNode.Object, 1);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            eventPublisher.Verify(p => p.Publish(It.IsAny<SetActiveEnvironmentMessage>()), Times.Exactly(publishCount));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_TreecodeIsNotResourceTreeViewModel_PublishEventsCorrectly()
        {
            //------------Setup for test--------------------------
            object publisheObject = null;
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<SetSelectedIContextualResourceModel>())).Verifiable();
            eventPublisher.Setup(p => p.Publish(It.IsAny<object>())).Callback((object obj) => publisheObject = obj);

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);

            var treeNode = new Mock<ITreeNode>();

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(treeNode.Object, 1);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            eventPublisher.Verify(p => p.Publish(It.IsAny<SetSelectedIContextualResourceModel>()), Times.Exactly(1));
            var publishedMessage = publisheObject as SetSelectedIContextualResourceModel;
            Assert.IsNotNull(publishedMessage);
            Assert.IsFalse(publishedMessage.DidDoubleClickOccur);
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

            var treeNode = new Mock<ResourceTreeViewModel>(eventPublisher.Object, null, resourceModel.Object);
            treeNode.Object.DataContext = null;

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(treeNode.Object, 1);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            eventPublisher.Verify(p => p.Publish(It.IsAny<object>()), Times.Never());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_UserIsNotAuthorized_DoesNothing()
        {
            Verify_OnMouseDown_UserIsNotAuthorized(editHitCount: 0, isAuthorized: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_UserIsAuthorized_InvokesLogic()
        {
            Verify_OnMouseDown_UserIsNotAuthorized(editHitCount: 1, isAuthorized: true);
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

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment).Returns(environmentModel.Object);
            resourceModel.Setup(r => r.IsAuthorized(AuthorizationContext.View)).Returns(isAuthorized);

            var editCommand = new Mock<ICommand>();
            editCommand.Setup(c => c.CanExecute(It.IsAny<object>())).Returns(true);
            editCommand.Setup(c => c.Execute(It.IsAny<object>())).Verifiable();

            var treeNode = new Mock<ResourceTreeViewModel>(eventPublisher.Object, null, resourceModel.Object);
            treeNode.Setup(r => r.EditCommand).Returns(editCommand.Object);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(treeNode.Object, ClickCount);

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

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);
            behavior.OpenOnDoubleClick = openOnDoubleClick;

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment).Returns(environmentModel.Object);
            resourceModel.Setup(r => r.IsAuthorized(AuthorizationContext.View)).Returns(true);

            var treeNode = new Mock<ResourceTreeViewModel>(eventPublisher.Object, null, resourceModel.Object);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(treeNode.Object, clickCount);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            eventPublisher.Verify(p => p.Publish(It.IsAny<object>()), Times.Never());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_OpenOnDoubleClickIsTrueAndClickCountIsOne_PublishesEvent()
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<SetSelectedIContextualResourceModel>())).Verifiable();

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);
            behavior.OpenOnDoubleClick = true;

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.Environment).Returns(environmentModel.Object);
            resourceModel.Setup(r => r.IsAuthorized(AuthorizationContext.View)).Returns(true);

            var treeNode = new Mock<ResourceTreeViewModel>(eventPublisher.Object, null, resourceModel.Object);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(treeNode.Object, 1);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            eventPublisher.Verify(p => p.Publish(It.IsAny<SetSelectedIContextualResourceModel>()), Times.Once());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("NavigationItemViewModelMouseDownBehavior_OnMouseDown")]
        public void NavigationItemViewModelMouseDownBehavior_OnMouseDown_OpenOnDoubleClickIsTrueAndClickCountIsTwo_PublishesEvent()
        {
            Verify_OnMouseDown_OpenOnDoubleClickIsTrueAndClickCountIsTwo(dontAllowDoubleClick: true);
            Verify_OnMouseDown_OpenOnDoubleClickIsTrueAndClickCountIsTwo(dontAllowDoubleClick: false);
        }

        static void Verify_OnMouseDown_OpenOnDoubleClickIsTrueAndClickCountIsTwo(bool dontAllowDoubleClick)
        {
            //------------Setup for test--------------------------
            var eventPublisher = new Mock<IEventAggregator>();
            eventPublisher.Setup(p => p.Publish(It.IsAny<SetSelectedIContextualResourceModel>())).Verifiable();

            var behavior = new TestNavigationItemViewModelMouseDownBehavior(eventPublisher.Object);
            behavior.OpenOnDoubleClick = true;
            behavior.DontAllowDoubleClick = dontAllowDoubleClick;

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

            var treeNode = new Mock<ResourceTreeViewModel>(eventPublisher.Object, null, resourceModel.Object);
            treeNode.Setup(r => r.EditCommand).Returns(editCommand.Object);

            //------------Execute Test---------------------------
            var result = behavior.TestOnMouseDown(treeNode.Object, 2);

            //------------Assert Results-------------------------
            Assert.AreNotEqual(dontAllowDoubleClick, result);
            eventPublisher.Verify(p => p.Publish(It.IsAny<SetSelectedIContextualResourceModel>()), Times.Once());
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

        public bool TestOnMouseDown(ITreeNode treenode, int clickCount)
        {
            return OnMouseDown(treenode, clickCount);
        }
    }
}
