﻿using Dev2.Composition;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Feedback;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Dev2.Core.Tests.Feedback
{
    [TestClass]
    public class FeedbackInvokerTests
    {
        #region Class Members

        private TestContext testContextInstance;

        #endregion Class Members

        #region Properties

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion Properties

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup()
        {
        }

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
        }

        #endregion

        #region Test Methods

        [TestMethod]
        public void InvokeFeedback_Where_ActionCanProvideFeedback_Expected_StartFeedbackInvokedAndCurrentActionSet()
        {
            Mock<IFeedbackAction> feedbackAction = new Mock<IFeedbackAction>();
            feedbackAction.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction.Setup(f => f.StartFeedback()).Verifiable();

            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackInvokerTests(popup);
            
            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();
            ImportService.SatisfyImports(feedbackInvoker);

            feedbackInvoker.InvokeFeedback(feedbackAction.Object);

            popup.Verify(p => p.Show(), Times.Exactly(0));
            feedbackAction.Verify(f => f.StartFeedback(), Times.Exactly(1));
            Assert.AreEqual(null, feedbackInvoker.CurrentAction);
        }

        [TestMethod]
        public void InvokeFeedback_Where_ActionCantProvideFeedback_Expected_NothingInvokedOrSet()
        {
            Mock<IFeedbackAction> feedbackAction = new Mock<IFeedbackAction>();
            feedbackAction.Setup(f => f.CanProvideFeedback).Returns(false);
            feedbackAction.Setup(f => f.StartFeedback()).Verifiable();

            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackInvokerTests(popup);

            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();
            ImportService.SatisfyImports(feedbackInvoker);

            feedbackInvoker.InvokeFeedback(feedbackAction.Object);

            popup.Verify(p => p.Show(), Times.Exactly(1));
            feedbackAction.Verify(f => f.StartFeedback(), Times.Exactly(0));
            Assert.AreEqual(null, feedbackInvoker.CurrentAction);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvokeFeedback_Where_ActionIsNull_Expected_ArgumentNullException()
        {
            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackInvokerTests(popup);

            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();
            ImportService.SatisfyImports(feedbackInvoker);

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

            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackInvokerTests(popup);

            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();
            ImportService.SatisfyImports(feedbackInvoker);

            feedbackInvoker.InvokeFeedback(feedbackAction1.Object, feedbackAction2.Object);

            popup.Verify(p => p.Show(), Times.Exactly(1));
            feedbackAction2.Verify(f => f.StartFeedback(It.IsAny<Action<Exception>>()), Times.Exactly(1));
            feedbackAction1.Verify(f => f.StartFeedback(), Times.Exactly(0));
            Assert.AreEqual(feedbackAction2.Object, feedbackInvoker.CurrentAction);
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

            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackInvokerTests(popup);

            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();
            ImportService.SatisfyImports(feedbackInvoker);

            feedbackInvoker.InvokeFeedback(feedbackAction1.Object, feedbackAction2.Object);

            popup.Verify(p => p.Show(), Times.Exactly(1));
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

            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Cancel);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackInvokerTests(popup);

            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();
            ImportService.SatisfyImports(feedbackInvoker);

            feedbackInvoker.InvokeFeedback(feedbackAction1.Object, feedbackAction2.Object);

            popup.Verify(p => p.Show(), Times.Exactly(1));
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

            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Cancel);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackInvokerTests(popup);

            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();
            ImportService.SatisfyImports(feedbackInvoker);

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

            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Cancel);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackInvokerTests(popup);

            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();
            ImportService.SatisfyImports(feedbackInvoker);

            feedbackInvoker.InvokeFeedback(feedbackAction1.Object, null);
        }

        [TestMethod]
        public void InvokeAsyncFeedback_Where_ActionCanProvideFeedback_Expected_StartFeedbackInvokedAndCurrentActionSet()
        {
            Mock<IAsyncFeedbackAction> feedbackAction = new Mock<IAsyncFeedbackAction>();
            feedbackAction.Setup(f => f.CanProvideFeedback).Returns(true);
            feedbackAction.Setup(f => f.StartFeedback(It.IsAny<Action<Exception>>())).Verifiable();

            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackInvokerTests(popup);

            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();
            ImportService.SatisfyImports(feedbackInvoker);

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

            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackInvokerTests(popup);

            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();
            ImportService.SatisfyImports(feedbackInvoker);

            feedbackInvoker.InvokeFeedback(feedbackAction.Object);
            feedbackInvoker.InvokeFeedback(feedbackAction1.Object);

            popup.Verify(p => p.Show(), Times.Exactly(1));
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

            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackInvokerTests(popup);

            FeedbackInvoker feedbackInvoker = new FeedbackInvoker();
            ImportService.SatisfyImports(feedbackInvoker);

            feedbackInvoker.InvokeFeedback(feedbackAction.Object);

            popup.Verify(p => p.Show(), Times.Exactly(1));
            feedbackAction.Verify(f => f.StartFeedback(It.IsAny<Action<Exception>>()), Times.Exactly(0));
            Assert.AreEqual(null, feedbackInvoker.CurrentAction);
        }

        #endregion Test Methods
    }
}
