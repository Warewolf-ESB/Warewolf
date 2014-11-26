
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
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Feedback;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Feedback
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FeedbackInvokerTests
    {
        [TestInitialize]
        public void TestSetup()
        {
            CustomContainer.Clear();
        }

        #region Properties

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion Properties

        #region Test Methods

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void InvokeFeedback_Where_ActionCanProvideFeedback_Expected_StartFeedbackInvokedAndCurrentActionSet()
        {
            Mock<IFeedbackAction> feedbackAction = new Mock<IFeedbackAction>();
            feedbackAction.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction.Setup(f => f.StartFeedback()).Verifiable();

            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);


            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();

            feedbackInvoker.InvokeFeedback(feedbackAction.Object);

            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(0));
            feedbackAction.Verify(f => f.StartFeedback(), Times.Exactly(1));
            Assert.AreEqual(null, feedbackInvoker.CurrentAction);
        }

        [TestMethod]
        public void InvokeFeedback_Where_ActionCantProvideFeedback_Expected_NothingInvokedOrSet()
        {
            Mock<IFeedbackAction> feedbackAction = new Mock<IFeedbackAction>();
            feedbackAction.Setup(f => f.CanProvideFeedback).Returns(false);
            feedbackAction.Setup(f => f.StartFeedback()).Verifiable();

            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            CustomContainer.Register(popup.Object);
            CustomContainer.Register(feedbackAction.Object);

            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();

            feedbackInvoker.InvokeFeedback(feedbackAction.Object);

            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(1));
            feedbackAction.Verify(f => f.StartFeedback(), Times.Exactly(0));
            Assert.AreEqual(null, feedbackInvoker.CurrentAction);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvokeFeedback_Where_ActionIsNull_Expected_ArgumentNullException()
        {
            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);
            CustomContainer.Register(popup.Object);
            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();

            feedbackInvoker.InvokeFeedback(null);
        }

        [TestMethod]
        public void InvokeFeedback_Where_EmailAndRecorderActionsProvidedAndUserAnswersYesToRecordingPrompt_Expected_RecordingActionInvoked()
        {
            Mock<IFeedbackAction> feedbackAction1 = new Mock<IFeedbackAction>();
            feedbackAction1.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction1.Setup(f => f.Priority).Returns(2);
            feedbackAction1.Setup(f => f.StartFeedback()).Verifiable();

            Mock<IAsyncFeedbackAction> feedbackAction2 = new Mock<IAsyncFeedbackAction>();
            feedbackAction2.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction2.Setup(f => f.Priority).Returns(1);
            feedbackAction2.Setup(f => f.StartFeedback(It.IsAny<Action<Exception>>())).Verifiable();

            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);
            CustomContainer.Register(popup.Object);
            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();

            feedbackInvoker.InvokeFeedback(feedbackAction1.Object, feedbackAction2.Object);

            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(1));
            feedbackAction2.Verify(f => f.StartFeedback(It.IsAny<Action<Exception>>()), Times.Exactly(1));
            feedbackAction1.Verify(f => f.StartFeedback(), Times.Exactly(0));
            Assert.AreEqual(feedbackAction2.Object, feedbackInvoker.CurrentAction);
        }

        // 13 Feb 2013 - Added by Michael to verify Bug 8809
        [TestMethod]
        public void InvokeFeedback_Where_UserClicksFeedback_Expected_LimitedPopups()
        {
            Mock<IAsyncFeedbackAction> feedbackAction = new Mock<IAsyncFeedbackAction>();
            feedbackAction.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction.Setup(f => f.Priority).Returns(2);
            feedbackAction.Setup(f => f.StartFeedback()).Verifiable();



            Mock<IPopupController> yesPopup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);
            Mock<IPopupController> noPopup = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);

            CustomContainer.Register(yesPopup.Object);
            FeedbackInvoker theInvoker = new FeedbackInvoker { CurrentAction = feedbackAction.Object };
            // If it's already recording, display a box to confirm if the user wants to stop the recording, and click Yes
            theInvoker.InvokeFeedback(feedbackAction.Object, feedbackAction.Object);

            // If it's already recording, display a box to confirm if the user wants to stop the recording, and click No
            theInvoker.InvokeFeedback(feedbackAction.Object, feedbackAction.Object);

            // If it's not recording, display the information box, and click Yes
            theInvoker.Popup = noPopup.Object;
            theInvoker.CurrentAction = null;
            theInvoker.InvokeFeedback(feedbackAction.Object, feedbackAction.Object);
            // If it's not recording, display the information box, and click No
            theInvoker.CurrentAction = null;
            theInvoker.InvokeFeedback(feedbackAction.Object, feedbackAction.Object);

            // Check all popups showed the correct amount of times
            yesPopup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(2)); // Once for already recording, once for not recording, and clicking yes
            noPopup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(2)); // Once for already recording, once for not recording, and clicking no
        }

        [TestMethod]
        public void InvokeFeedback_Where_EmailAndRecorderActionsProvidedAndUserAnswersNoToRecordingPrompt_Expected_EmailActionInvoked()
        {
            Mock<IFeedbackAction> feedbackAction1 = new Mock<IFeedbackAction>();
            feedbackAction1.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction1.Setup(f => f.Priority).Returns(2);
            feedbackAction1.Setup(f => f.StartFeedback()).Verifiable();

            Mock<IAsyncFeedbackAction> feedbackAction2 = new Mock<IAsyncFeedbackAction>();
            feedbackAction2.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction2.Setup(f => f.Priority).Returns(1);
            feedbackAction2.Setup(f => f.StartFeedback(It.IsAny<Action<Exception>>())).Verifiable();

            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);
            CustomContainer.Register(popup.Object);
            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();

            feedbackInvoker.InvokeFeedback(feedbackAction1.Object, feedbackAction2.Object);

            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(1));
            feedbackAction2.Verify(f => f.StartFeedback(It.IsAny<Action<Exception>>()), Times.Exactly(0));
            feedbackAction1.Verify(f => f.StartFeedback(), Times.Exactly(1));
            Assert.AreEqual(null, feedbackInvoker.CurrentAction);
        }

        [TestMethod]
        public void InvokeFeedback_Where_EmailAndRecorderActionsProvidedAndUserAnswersCancelToRecordingPrompt_Expected_NoActionInvoked()
        {
            Mock<IFeedbackAction> feedbackAction1 = new Mock<IFeedbackAction>();
            feedbackAction1.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction1.Setup(f => f.Priority).Returns(2);
            feedbackAction1.Setup(f => f.StartFeedback()).Verifiable();

            Mock<IAsyncFeedbackAction> feedbackAction2 = new Mock<IAsyncFeedbackAction>();
            feedbackAction2.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction2.Setup(f => f.Priority).Returns(1);
            feedbackAction2.Setup(f => f.StartFeedback(It.IsAny<Action<Exception>>())).Verifiable();

            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Cancel);
            CustomContainer.Register(popup.Object);
            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();

            feedbackInvoker.InvokeFeedback(feedbackAction1.Object, feedbackAction2.Object);

            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(1));
            feedbackAction2.Verify(f => f.StartFeedback(It.IsAny<Action<Exception>>()), Times.Exactly(0));
            feedbackAction1.Verify(f => f.StartFeedback(), Times.Exactly(0));
            Assert.AreEqual(null, feedbackInvoker.CurrentAction);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvokeFeedback_Where_EmailActionIsNull_Expected_ArgumentNullException()
        {
            Mock<IAsyncFeedbackAction> feedbackAction2 = new Mock<IAsyncFeedbackAction>();
            feedbackAction2.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction2.Setup(f => f.Priority).Returns(1);
            feedbackAction2.Setup(f => f.StartFeedback(It.IsAny<Action<Exception>>())).Verifiable();

            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Cancel);
            CustomContainer.Register(popup.Object);
            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();

            feedbackInvoker.InvokeFeedback(null, feedbackAction2.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvokeFeedback_Where_RecorderActionIsNull_Expected_ArgumentNullException()
        {
            Mock<IFeedbackAction> feedbackAction1 = new Mock<IFeedbackAction>();
            feedbackAction1.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction1.Setup(f => f.Priority).Returns(2);
            feedbackAction1.Setup(f => f.StartFeedback()).Verifiable();

            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Cancel);
            CustomContainer.Register(popup.Object);
            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();

            feedbackInvoker.InvokeFeedback(feedbackAction1.Object, null);
        }

        [TestMethod]
        public void InvokeAsyncFeedback_Where_ActionCanProvideFeedback_Expected_StartFeedbackInvokedAndCurrentActionSet()
        {
            Mock<IAsyncFeedbackAction> feedbackAction = new Mock<IAsyncFeedbackAction>();
            feedbackAction.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction.Setup(f => f.StartFeedback(It.IsAny<Action<Exception>>())).Verifiable();

            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();

            feedbackInvoker.InvokeFeedback(feedbackAction.Object);

            popup.Verify(p => p.Show(), Times.Exactly(0));
            feedbackAction.Verify(f => f.StartFeedback(It.IsAny<Action<Exception>>()), Times.Exactly(1));
            Assert.AreEqual(feedbackAction.Object, feedbackInvoker.CurrentAction);
        }

        [TestMethod]
        public void InvokeAsyncFeedback_Where_FeedbackAlreadyInProgressAndUserOptsToCancelInprogressFeedBack_Expected_PromptAndFeedbackStarted()
        {
            Mock<IAsyncFeedbackAction> feedbackAction = new Mock<IAsyncFeedbackAction>();
            feedbackAction.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction.Setup(f => f.StartFeedback(It.IsAny<Action<Exception>>())).Verifiable();
            feedbackAction.Setup(f => f.CancelFeedback()).Verifiable();

            Mock<IAsyncFeedbackAction> feedbackAction1 = new Mock<IAsyncFeedbackAction>();
            feedbackAction1.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction1.Setup(f => f.StartFeedback(It.IsAny<Action<Exception>>())).Verifiable();

            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);
            CustomContainer.Register(popup.Object);
            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();

            feedbackInvoker.InvokeFeedback(feedbackAction.Object);
            feedbackInvoker.InvokeFeedback(feedbackAction1.Object);

            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(1));
            feedbackAction.Verify(f => f.StartFeedback(It.IsAny<Action<Exception>>()), Times.Exactly(1));
            feedbackAction.Verify(f => f.CancelFeedback(), Times.Exactly(1));
            feedbackAction1.Verify(f => f.StartFeedback(It.IsAny<Action<Exception>>()), Times.Exactly(1));
            Assert.AreEqual(feedbackAction1.Object, feedbackInvoker.CurrentAction);
        }

        [TestMethod]
        public void InvokeAsyncFeedback_Where_ActionCantProvideFeedback_NothingInvokedOrSet()
        {
            Mock<IAsyncFeedbackAction> feedbackAction = new Mock<IAsyncFeedbackAction>();
            feedbackAction.Setup(f => f.CanProvideFeedback).Returns(false);
            feedbackAction.Setup(f => f.StartFeedback(It.IsAny<Action<Exception>>())).Verifiable();

            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);
            CustomContainer.Register(popup.Object);
            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();

            feedbackInvoker.InvokeFeedback(feedbackAction.Object);

            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(1));
            feedbackAction.Verify(f => f.StartFeedback(It.IsAny<Action<Exception>>()), Times.Exactly(0));
            Assert.AreEqual(null, feedbackInvoker.CurrentAction);
        }

        #endregion Test Methods

        // ReSharper restore InconsistentNaming
    }
}
