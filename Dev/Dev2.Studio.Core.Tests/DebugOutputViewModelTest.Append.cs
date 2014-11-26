
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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Security;
using Dev2.Diagnostics.Debug;
using Dev2.Services.Security;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.ViewModels.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests
{
    public partial class DebugOutputViewModelTest
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_NullContent_NotAdded()
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo, new Mock<IDebugOutputFilterStrategy>().Object);

            //------------Execute Test---------------------------
            viewModel.Append(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, viewModel.ContentItemCount);
            Assert.AreEqual(0, viewModel.RootItems.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_SearchTextDoesNotMatchContent_NotAdded()
        {
            DebugOutputViewModel_Append_SearchText("Moo", "Baa", false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_SearchTextDoesMatchContent_Added()
        {
            DebugOutputViewModel_Append_SearchText("Moo", "Moo", true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_SearchTextIsEmpty_Added()
        {
            DebugOutputViewModel_Append_SearchText("", "Moo", true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_SearchTextIsNull_Added()
        {
            DebugOutputViewModel_Append_SearchText(null, "Moo", true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_SearchTextIsWhiteSpace_Added()
        {
            DebugOutputViewModel_Append_SearchText("  ", "Moo", true);
        }

        static void DebugOutputViewModel_Append_SearchText(string searchText, string contentText, bool isAdded)
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var filterStrat = new Mock<IDebugOutputFilterStrategy>();
            filterStrat.Setup(e => e.Filter(It.IsAny<Object>(), It.IsAny<String>())).Returns(searchText == contentText);
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo, filterStrat.Object) { SearchText = searchText };

            var content = new DebugState { DisplayName = contentText, ID = Guid.NewGuid(), StateType = StateType.All, ActivityType = ActivityType.Step, SessionID = viewModel.SessionID };

            //------------Execute Test---------------------------
            viewModel.Append(content);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ContentItemCount);
            Assert.AreEqual(isAdded ? 1 : 0, viewModel.RootItems.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_ContentStateTypeIsMessage_StringTreeViewItemAdded()
        {
            DebugOutputViewModel_Append_ContentStateType(StateType.Message, typeof(DebugStringTreeViewItemViewModel), false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_ContentStateTypeIsNotMessage_StateTreeViewItemAdded()
        {
            DebugOutputViewModel_Append_ContentStateType(StateType.All, typeof(DebugStateTreeViewItemViewModel), false);
        }

        static void DebugOutputViewModel_Append_ContentStateType(StateType stateType, Type expectedType, bool isExpanded)
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo, new Mock<IDebugOutputFilterStrategy>().Object);

            var content = new DebugState { DisplayName = "Content", ID = Guid.NewGuid(), StateType = stateType, ActivityType = ActivityType.Step, Message = "The message", SessionID = viewModel.SessionID };

            //------------Execute Test---------------------------
            viewModel.Append(content);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ContentItemCount);
            Assert.AreEqual(1, viewModel.RootItems.Count);
            Assert.AreEqual(viewModel.RootItems[0].IsExpanded, isExpanded);
            Assert.IsInstanceOfType(viewModel.RootItems[0], expectedType);

            if(expectedType == typeof(DebugStringTreeViewItemViewModel))
            {
                Assert.AreEqual(0, viewModel.RootItems[0].Depth);
                var viewContent = ((DebugStringTreeViewItemViewModel)viewModel.RootItems[0]).Content;
                Assert.AreEqual(content.Message, viewContent);
            }
            else
            {
                Assert.IsTrue(viewModel.RootItems[0].Depth >= 0);
                var viewContent = ((DebugStateTreeViewItemViewModel)viewModel.RootItems[0]).Content;
                Assert.AreSame(content, viewContent);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_ContentIsDebugStateAndParentIDIsEmpty_ItemAddedAtRootAndIsNotExpanded()
        {
            DebugOutputViewModel_Append_ContentIsDebugState(Guid.NewGuid(), Guid.Empty, "Content");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_ContentIsDebugStateAndIDDoesEqualParentID_ItemAddedAtRootAndIsNotExpanded()
        {
            var id = Guid.NewGuid();
            DebugOutputViewModel_Append_ContentIsDebugState(id, id, "Content");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_ItemHasSameID_ShouldAddAsNewItemIntoTree()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo, new Mock<IDebugOutputFilterStrategy>().Object);
            var content = new DebugState { DisplayName = "Content", ID = id, ParentID = id, StateType = StateType.All, ActivityType = ActivityType.Step, SessionID = viewModel.SessionID };
            viewModel.Append(content);
            var content2 = new DebugState { DisplayName = "Content2", ID = id, ParentID = id, StateType = StateType.All, ActivityType = ActivityType.Step, SessionID = viewModel.SessionID };
            //------------Execute Test---------------------------
            viewModel.Append(content2);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, viewModel.RootItems.Count);
            var child = viewModel.RootItems[0] as DebugStateTreeViewItemViewModel;
            Assert.IsNotNull(child);
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if(child != null)
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                Assert.AreEqual("Content", child.Content.DisplayName);
            }
            var child2 = viewModel.RootItems[1] as DebugStateTreeViewItemViewModel;
            Assert.IsNotNull(child2);
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if(child2 != null)
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                Assert.AreEqual("Content2", child2.Content.DisplayName);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_ItemRemoteHasNoPermission_ShouldNotAddAsNewItemIntoTree()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var envRepo = GetEnvironmentRepository();

            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(service => service.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.None);
            _environmentModel.Setup(model => model.AuthorizationService).Returns(mockAuthorizationService.Object);
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo, new Mock<IDebugOutputFilterStrategy>().Object);
            var content = new DebugState { DisplayName = "Content", ID = id, ParentID = id, StateType = StateType.All, ActivityType = ActivityType.Step, SessionID = viewModel.SessionID };
            viewModel.Append(content);
            var content2 = new DebugState { DisplayName = "Content2", ID = id, ParentID = id, StateType = StateType.All, ActivityType = ActivityType.Step, SessionID = viewModel.SessionID, EnvironmentID = Guid.NewGuid() };
            //------------Execute Test---------------------------
            viewModel.Append(content2);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.RootItems.Count);

        }

        void DebugOutputViewModel_Append_ContentIsDebugState(Guid contentID, Guid contentParentID, string displayName)
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo, new Mock<IDebugOutputFilterStrategy>().Object);

            var content = new DebugState { DisplayName = displayName, ID = contentID, ParentID = contentParentID, StateType = StateType.All, ActivityType = ActivityType.Step, SessionID = viewModel.SessionID };

            //------------Execute Test---------------------------
            viewModel.Append(content);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ContentItemCount);
            Assert.AreEqual(1, viewModel.RootItems.Count);

            var child = viewModel.RootItems[0];
            Assert.AreEqual(0, child.Depth);
            Assert.IsNull(child.Parent);
            Assert.IsFalse(child.IsExpanded);
        }



        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_ContentIsDebugStateWithoutErrors_ItemAddedWithoutErrors()
        {
            DebugOutputViewModel_Append_ContentIsDebugStateErrors(false, false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_ContentIsDebugStateWithErrors_ItemAddedWithErrors()
        {
            DebugOutputViewModel_Append_ContentIsDebugStateErrors(true, true);
        }

        void DebugOutputViewModel_Append_ContentIsDebugStateErrors(bool parentContentHasErrors, bool childContentHasErrors)
        {
            //------------Setup for test--------------------------
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo, new Mock<IDebugOutputFilterStrategy>().Object);

            var parentContent = new DebugState { HasError = parentContentHasErrors, DisplayName = "Content", ID = Guid.NewGuid(), ParentID = Guid.Empty, StateType = StateType.All, ActivityType = ActivityType.Step, SessionID = viewModel.SessionID };
            var childContent = new DebugState { HasError = childContentHasErrors, DisplayName = "Content", ID = Guid.NewGuid(), ParentID = parentContent.ID, StateType = StateType.All, ActivityType = ActivityType.Step, SessionID = viewModel.SessionID };

            //------------Execute Test---------------------------
            viewModel.Append(parentContent);
            viewModel.Append(childContent);

            //------------Assert Results-------------------------
            Assert.AreEqual(2, viewModel.ContentItemCount);
            Assert.AreEqual(1, viewModel.RootItems.Count);

            var childItem = viewModel.RootItems[0].Children[0];

            Assert.AreEqual(childContentHasErrors, childItem.HasError);
            Assert.AreEqual(parentContentHasErrors, childItem.Parent.HasError);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_TypeIsStartAndNotFirstStep_NothingAppended()
        {
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo, new Mock<IDebugOutputFilterStrategy>().Object);
            var content = new DebugState { DisplayName = "Content", ID = Guid.NewGuid(), ParentID = Guid.Empty, StateType = StateType.Start, ActivityType = ActivityType.Step, SessionID = viewModel.SessionID };

            //------------Execute Test---------------------------
            viewModel.Append(content);

            // Assert Nothing Appended
            Assert.AreEqual(0, viewModel.RootItems.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DebugOutputViewModel_Append")]
        public void DebugOutputViewModel_Append_TypeIsEndAndNotLastStep_NothingAppended()
        {
            var envRepo = GetEnvironmentRepository();
            var viewModel = new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo, new Mock<IDebugOutputFilterStrategy>().Object);
            var content = new DebugState { DisplayName = "Content", ID = Guid.NewGuid(), ParentID = Guid.Empty, StateType = StateType.End, ActivityType = ActivityType.Step, SessionID = viewModel.SessionID };

            //------------Execute Test---------------------------
            viewModel.Append(content);

            // Assert Nothing Appended
            Assert.AreEqual(0, viewModel.RootItems.Count);
        }
    }
}
