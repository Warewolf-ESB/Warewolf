using Dev2.Composition;
using Dev2.Studio.AppResources.Exceptions;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Windows;

namespace Dev2.Core.Tests.Feedback.Actions
{
    [TestClass]
    public class RecorderFeedbackActionTests
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
        public void StartFeedback_Expected_RecordingStarted()
        {
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StartRecording(It.IsAny<string>())).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker);
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.StartFeedback();

            feedbackRecorder.Verify(r => r.StartRecording(It.IsAny<string>()), Times.Exactly(1));
        }

        [TestMethod]
        public void StartFeedback_Where_FeedbackRecordingAlreadyRunningAndYesIsAnsweredToKillPrompt_Expected_KillAllRecordingTasks()
        {
            int callCount = 0;
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StartRecording(It.IsAny<string>())).Callback(() =>
                {
                    callCount++;

                    if (callCount == 1)
                    {
                        throw new FeedbackRecordingInprogressException();
                    }

                    if (callCount > 2)
                    {
                        throw new Exception("Error in test mock logic, this point should never be reached. This exception's purpose is to end a potential infinite loop.");
                    }
                }
            ).Verifiable();
            feedbackRecorder.Setup(r => r.KillAllRecordingTasks()).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker);
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.StartFeedback();

            feedbackRecorder.Verify(r => r.StartRecording(It.IsAny<string>()), Times.Exactly(2));
            feedbackRecorder.Verify(r => r.KillAllRecordingTasks(), Times.Exactly(1));
            popup.Verify(p => p.Show(), Times.Exactly(3));
        }

        [TestMethod]
        public void StartFeedback_Where_FeedbackRecordingAlreadyRunningAndNoIsAnsweredToKillPrompt_Expected_StartRecording()
        {
            int callCount = 0;
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StartRecording(It.IsAny<string>())).Callback(() =>
            {
                callCount++;

                if (callCount == 1)
                {
                    throw new FeedbackRecordingInprogressException();
                }

                if (callCount > 2)
                {
                    throw new Exception("Error in test mock logic, this point should never be reached. This exception's purpose is to end a potential infinite loop.");
                }
            }
            ).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker);
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.StartFeedback();

            feedbackRecorder.Verify(r => r.StartRecording(It.IsAny<string>()), Times.Exactly(1));
            popup.Verify(p => p.Show(), Times.Exactly(2));
        }

        [TestMethod]
        public void StartFeedback_Where_FeedbackRecordingAlreadyRunningAndCancelIsAnsweredToKillPrompt_Expected_StartRecording()
        {
            int callCount = 0;
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StartRecording(It.IsAny<string>())).Callback(() =>
            {
                callCount++;

                if (callCount == 1)
                {
                    throw new FeedbackRecordingInprogressException();
                }

                if (callCount > 2)
                {
                    throw new Exception("Error in test mock logic, this point should never be reached. This exception's purpose is to end a potential infinite loop.");
                }
            }
            ).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker);
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.StartFeedback();

            feedbackRecorder.Verify(r => r.StartRecording(It.IsAny<string>()), Times.Exactly(1));
            popup.Verify(p => p.Show(), Times.Exactly(2));
        }

        [TestMethod]
        public void FinshFeedback_Expected_RecordingStoppedAndEmailFeedbackInvoked()
        {
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StopRecording()).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            feedbackInvoker.Setup(r => r.InvokeFeedback(It.IsAny<IFeedbackAction>())).Verifiable();

            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker);
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.FinishFeedBack();

            feedbackRecorder.Verify(r => r.StopRecording(), Times.Exactly(1));
            feedbackInvoker.Verify(r => r.InvokeFeedback(It.IsAny<EmailFeedbackAction>()), Times.Exactly(1));
        }

        [TestMethod]
        public void FinshFeedback_Where_NoProcessesToStop_Expected_RecordingStoppedInvoked()
        {
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StopRecording()).Callback(() => 
            {
                throw new FeedbackRecordingNoProcessesExcpetion();
            }).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker);
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.FinishFeedBack();

            feedbackRecorder.Verify(r => r.StopRecording(), Times.Exactly(1));
            popup.Verify(p => p.Show(), Times.Exactly(1));
        }

        [TestMethod]
        public void FinshFeedback_Where_TimeoutOccurs_Expected_RecordingStoppedInvoked()
        {
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StopRecording()).Callback(() =>
            {
                throw new FeedbackRecordingTimeoutException();
            }).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker);
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.FinishFeedBack();

            feedbackRecorder.Verify(r => r.StopRecording(), Times.Exactly(1));
            popup.Verify(p => p.Show(), Times.Exactly(1));
        }

        [TestMethod]
        public void CancelFeedback_Expected_KillAllInvoked()
        {
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.KillAllRecordingTasks()).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker);
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.CancelFeedback();

            feedbackRecorder.Verify(r => r.KillAllRecordingTasks(), Times.Exactly(1));
        }

        [TestMethod]
        public void CancelFeedback_Where_TimeOutOccurs_Expected_KillAllInvoked()
        {
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.KillAllRecordingTasks()).Callback(() =>
            {
                throw new FeedbackRecordingTimeoutException();
            }).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopUp> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker);
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.CancelFeedback();

            feedbackRecorder.Verify(r => r.KillAllRecordingTasks(), Times.Exactly(1));
            popup.Verify(p => p.Show(), Times.Exactly(1));

        }

        #endregion Test Methods
    }
}
