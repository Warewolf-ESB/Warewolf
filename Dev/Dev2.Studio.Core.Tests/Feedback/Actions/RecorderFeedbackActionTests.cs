using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.AppResources.Exceptions;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Feedback.Actions
{
    [TestClass]    
    public class RecorderFeedbackActionTests
    {
        #region Class Members

        #endregion Class Members

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
        public void StartFeedback_Expected_RecordingStarted()
        {
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StartRecording(It.IsAny<string>())).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker, new Mock<IWindowManager>());
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.StartFeedback();

            feedbackRecorder.Verify(r => r.StartRecording(It.IsAny<string>()), Times.Exactly(1));
        }

        [TestMethod]
        public void StartFeedbackEWhereMessageDialogClosedUsingTheXButton_Expected_RecordingDoesntStart()
        {
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StartRecording(It.IsAny<string>())).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.None);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker, new Mock<IWindowManager>());
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.StartFeedback();

            feedbackRecorder.Verify(r => r.StartRecording(It.IsAny<string>()), Times.Exactly(0));
        }

        [TestMethod]
        public void StartFeedback_Where_FeedbackRecordingAlreadyRunningAndYesIsAnsweredToKillPrompt_Expected_KillAllRecordingTasks()
        {
            int callCount = 0;
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StartRecording(It.IsAny<string>())).Callback(() =>
                {
                    callCount++;

                    if(callCount == 1)
                    {
                        throw new FeedbackRecordingInprogressException();
                    }

                    if(callCount > 2)
                    {
                        throw new Exception("Error in test mock logic, this point should never be reached. This exception's purpose is to end a potential infinite loop.");
                    }
                }
            ).Verifiable();
            feedbackRecorder.Setup(r => r.KillAllRecordingTasks()).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker, new Mock<IWindowManager>());
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.StartFeedback();

            feedbackRecorder.Verify(r => r.StartRecording(It.IsAny<string>()), Times.Exactly(2));
            feedbackRecorder.Verify(r => r.KillAllRecordingTasks(), Times.Exactly(1));
            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(3));
        }

        [TestMethod]
        public void StartFeedback_Where_FeedbackRecordingAlreadyRunningAndNoIsAnsweredToKillPrompt_Expected_StartRecording()
        {
            int callCount = 0;
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StartRecording(It.IsAny<string>())).Callback(() =>
            {
                callCount++;

                if(callCount == 1)
                {
                    throw new FeedbackRecordingInprogressException();
                }

                if(callCount > 2)
                {
                    throw new Exception("Error in test mock logic, this point should never be reached. This exception's purpose is to end a potential infinite loop.");
                }
            }
            ).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker, new Mock<IWindowManager>());
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.StartFeedback();

            feedbackRecorder.Verify(r => r.StartRecording(It.IsAny<string>()), Times.Exactly(1));
            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void StartFeedback_Where_FeedbackRecordingAlreadyRunningAndCancelIsAnsweredToKillPrompt_Expected_StartRecording()
        {
            int callCount = 0;
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StartRecording(It.IsAny<string>())).Callback(() =>
            {
                callCount++;

                if(callCount == 1)
                {
                    throw new FeedbackRecordingInprogressException();
                }

                if(callCount > 2)
                {
                    throw new Exception("Error in test mock logic, this point should never be reached. This exception's purpose is to end a potential infinite loop.");
                }
            }
            ).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.No);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker, new Mock<IWindowManager>());
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.StartFeedback();

            feedbackRecorder.Verify(r => r.StartRecording(It.IsAny<string>()), Times.Exactly(1));
            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void FinshFeedback_Expected_RecordingStoppedAndEmailFeedbackInvoked()
        {
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.StopRecording()).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            feedbackInvoker.Setup(r => r.InvokeFeedback(It.IsAny<IFeedbackAction>())).Verifiable();

            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.Yes);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker, new Mock<IWindowManager>());
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);
            Mock<IEnvironmentModel> mockEnvironment = Dev2MockFactory.SetupEnvironmentModel();
            Mock<IEnvironmentConnection> mockConn = Dev2MockFactory.SetupIEnvironmentConnection(new Exception());
            mockConn.Setup(mock => mock.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(@"""2013/06/07 02:23:22 PM :: ERROR -> Procedure or function 'fn_Greeting' expects parameter '@Name', which was not supplied.\r\n   at System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\r\n   at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)\r\n   at System.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)\r\n   at System.Data.SqlClient.SqlDataReader.TryConsumeMetaData()\r\n   at System.Data.SqlClient.SqlDataReader.get_MetaData()\r\n   at System.Data.SqlClient.SqlCommand.FinishExecuteReader(SqlDataReader ds, RunBehavior runBehavior, String resetOptionsString)\r\n   at System.Data.SqlClient.SqlCommand.RunExecuteReaderTds(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, Boolean async, Int32 timeout, Task& task, Boolean asyncWrite)\r\n   at System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method, TaskCompletionSource`1 completion, Int32 timeout, Task& task, Boolean asyncWrite)\r\n   at System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method)\r\n   at System.Data.SqlClient.SqlCommand.ExecuteReader(CommandBehavior behavior, String method)\r\n   at System.Data.SqlClient.SqlCommand.ExecuteDbDataReader(CommandBehavior behavior)\r\n   at System.Data.Common.DbCommand.System.Data.IDbCommand.ExecuteReader(CommandBehavior behavior)\r\n   at System.Data.Common.DbDataAdapter.FillInternal(DataSet dataset, DataTable[] datatables, Int32 startRecord, Int32 maxRecords, String srcTable, IDbCommand command, CommandBehavior behavior)\r\n   at System.Data.Common.DbDataAdapter.Fill(DataSet dataSet, Int32 startRecord, Int32 maxRecords, String srcTable, IDbCommand command, CommandBehavior behavior)\r\n   at System.Data.Common.DbDataAdapter.Fill(DataSet dataSet)\r\n   at Dev2.Runtime.ServiceModel.Esb.Brokers.MsSqlBroker.ExecuteSelect(IDbCommand command) in c:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Runtime.Services\\ServiceModel\\Esb\\Brokers\\MsSqlBroker.cs:line 105\r\n   at Dev2.Runtime.ServiceModel.Esb.Brokers.AbstractDatabaseBroker.TestService(DbService dbService) in c:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Runtime.Services\\ServiceModel\\Esb\\Brokers\\AbstractDatabaseBroker.cs:line 94\r\n2013/06/07 02:23:22 PM :: INFO -> Error retrieving shape from service output.\r\n2013/06/07 02:23:22 PM :: INFO -> Error retrieving shape from service output. Stacktrace : Error retrieving shape from service output.\r\n2013/06/07 02:23:25 PM :: ERROR -> Transactional Error : This SqlTransaction has completed; it is no longer usable.\r\n   at System.Data.SqlClient.SqlTransaction.ZombieCheck()\r\n   at System.Data.SqlClient.SqlTransaction.Rollback()\r\n   at Dev2.Runtime.ServiceModel.Esb.Brokers.AbstractDatabaseBroker.TestService(DbService dbService) in c:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Runtime.Services\\ServiceModel\\Esb\\Brokers\\AbstractDatabaseBroker.cs:line 121\r\n2013/06/07 04:14:38 PM :: INFO -> Preloading assemblies...  \r\n2013/06/07 04:14:38 PM :: INFO -> done.\r\n2013/06/07 04:14:38 PM :: INFO -> SLM garbage collection manager disabled.\r\n2013/06/07 04:14:38 PM :: INFO -> Starting DataList Server...  \r\n2013/06/07 04:14:38 PM :: INFO -> done.\r\n2013/06/07 04:14:38 PM :: INFO -> \r\n2013/06/07 04:14:38 PM :: INFO -> Loading resource catalog...  \r\n2013/06/07 04:14:39 PM :: ERROR -> Resource 'Cities Database' from file 'C:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Server\\bin\\Debug\\Sources\\Cities Database.xml' wasn't loaded because a resource with the same name has already been loaded from file 'Cities Database'.\r\n2013/06/07 04:14:39 PM :: ERROR -> Resource 'EndPointInterrogator' from file 'C:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Server\\bin\\Debug\\Services\\EndPointInterrogator.xml' wasn't loaded because a resource with the same name has already been loaded from file 'EndPointInterrogator'.\r\n2013/06/07 04:14:39 PM :: ERROR -> Resource 'Dev2HorizontialLinkMenu' from file 'C:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Server\\bin\\Debug\\Services\\Dev2HorizontialMenu.xml' wasn't loaded because a resource with the same name has already been loaded from file 'Dev2HorizontialLinkMenu'.\r\n2013/06/07 04:14:39 PM :: ERROR -> Resource 'Textbox.wiz' from file 'C:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Server\\bin\\Debug\\Services\\Textbox.wiz.xml' wasn't loaded because a resource with the same name has already been loaded from file 'Textbox.wiz'.\r\n2013/06/07 04:14:41 PM :: INFO -> done.\r\n2013/06/07 04:14:41 PM :: INFO -> Loading server workspace...  \r\n2013/06/07 04:14:41 PM :: INFO -> done.\r\n2013/06/07 04:14:41 PM :: INFO -> Opening Execution Channel...  \r\n2013/06/07 04:14:41 PM :: INFO -> done.\r\n2013/06/07 04:14:41 PM :: INFO -> \r\n2013/06/07 04:14:41 PM :: INFO -> Opening DataList Channel...  \r\n2013/06/07 04:14:41 PM :: INFO -> done.\r\n2013/06/07 04:14:41 PM :: INFO -> \r\n2013/06/07 04:14:41 PM :: INFO -> Studio Server listening on 127.0.0.1:77\r\n2013/06/07 04:14:41 PM :: INFO -> Studio Server listening on 192.168.104.25:77\r\n2013/06/07 04:14:41 PM :: INFO -> \r\nWeb Server Started\r\n2013/06/07 04:14:41 PM :: INFO -> Web server listening at http://*:1234/\r\n2013/06/07 04:14:41 PM :: INFO -> Web server listening at https://*:1236/\r\n2013/06/07 04:14:41 PM :: INFO -> Loading settings provider...  \r\n2013/06/07 04:14:41 PM :: INFO -> done.\r\n2013/06/07 04:14:41 PM :: INFO -> Configure logging...  \r\n2013/06/07 04:14:41 PM :: INFO -> done.\r\n2013/06/07 04:14:41 PM :: INFO -> Press <ENTER> to terminate service and/or web server if started\r\n2013/06/07 04:14:54 PM :: ERROR -> Resource 'Cities Database' from file 'C:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Server\\bin\\Debug\\Workspaces\\c65db9c9-fb35-4468-aefd-4c5c48fe3b49\\Sources\\Cities Database.xml' wasn't loaded because a resource with the same name has already been loaded from file 'Cities Database'.\r\n2013/06/07 04:14:54 PM :: ERROR -> Resource 'Dev2HorizontialLinkMenu' from file 'C:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Server\\bin\\Debug\\Workspaces\\c65db9c9-fb35-4468-aefd-4c5c48fe3b49\\Services\\Dev2HorizontialMenu.xml' wasn't loaded because a resource with the same name has already been loaded from file 'Dev2HorizontialLinkMenu'.\r\n2013/06/07 04:14:54 PM :: ERROR -> Resource 'EndPointInterrogator' from file 'C:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Server\\bin\\Debug\\Workspaces\\c65db9c9-fb35-4468-aefd-4c5c48fe3b49\\Services\\EndPointInterrogator.xml' wasn't loaded because a resource with the same name has already been loaded from file 'EndPointInterrogator'.\r\n2013/06/07 04:14:54 PM :: ERROR -> Resource 'Textbox.wiz' from file 'C:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Server\\bin\\Debug\\Workspaces\\c65db9c9-fb35-4468-aefd-4c5c48fe3b49\\Services\\Textbox.wiz.xml' wasn't loaded because a resource with the same name has already been loaded from file 'Textbox.wiz'.\r\n2013/06/07 04:14:57 PM :: ERROR -> fileTest -> Cache MISS\r\n2013/06/07 04:14:57 PM :: ERROR -> Resource 'Cities Database' from file 'C:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Server\\bin\\Debug\\Workspaces\\c65db9c9-fb35-4468-aefd-4c5c48fe3b49\\Sources\\Cities Database.xml' wasn't loaded because a resource with the same name has already been loaded from file 'Cities Database'.\r\n2013/06/07 04:14:57 PM :: ERROR -> Resource 'EndPointInterrogator' from file 'C:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Server\\bin\\Debug\\Workspaces\\c65db9c9-fb35-4468-aefd-4c5c48fe3b49\\Services\\EndPointInterrogator.xml' wasn't loaded because a resource with the same name has already been loaded from file 'EndPointInterrogator'.\r\n2013/06/07 04:14:57 PM :: ERROR -> Resource 'Dev2HorizontialLinkMenu' from file 'C:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Server\\bin\\Debug\\Workspaces\\c65db9c9-fb35-4468-aefd-4c5c48fe3b49\\Services\\Dev2HorizontialMenu.xml' wasn't loaded because a resource with the same name has already been loaded from file 'Dev2HorizontialLinkMenu'.\r\n2013/06/07 04:14:57 PM :: ERROR -> Resource 'Textbox.wiz' from file 'C:\\Development\\DEV2 SCRUM Project\\Branches\\BUG_9598\\Dev2.Server\\bin\\Debug\\Workspaces\\c65db9c9-fb35-4468-aefd-4c5c48fe3b49\\Services\\Textbox.wiz.xml' wasn't loaded because a resource with the same name has already been loaded from file 'Textbox.wiz'.\r\n"""));
            mockEnvironment.Setup(mock => mock.Connection).Returns(mockConn.Object);

            recorderFeedbackAction.FinishFeedBack(mockEnvironment.Object);

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
            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker, new Mock<IWindowManager>());
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.FinishFeedBack();

            feedbackRecorder.Verify(r => r.StopRecording(), Times.Exactly(1));
            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(1));
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
            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker, new Mock<IWindowManager>());
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.FinishFeedBack();

            feedbackRecorder.Verify(r => r.StopRecording(), Times.Exactly(1));
            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(1));
        }

        [TestMethod]
        public void CancelFeedback_Expected_KillAllInvoked()
        {
            Mock<IFeedBackRecorder> feedbackRecorder = new Mock<IFeedBackRecorder>();
            feedbackRecorder.Setup(r => r.KillAllRecordingTasks()).Verifiable();

            Mock<IFeedbackInvoker> feedbackInvoker = new Mock<IFeedbackInvoker>();
            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker, new Mock<IWindowManager>());
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
            Mock<IPopupController> popup = Dev2MockFactory.CreateIPopup(MessageBoxResult.OK);

            ImportService.CurrentContext = CompositionInitializer.InitializeForFeedbackActionTests(popup, feedbackRecorder, feedbackInvoker, new Mock<IWindowManager>());
            RecorderFeedbackAction recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            recorderFeedbackAction.CancelFeedback();

            feedbackRecorder.Verify(r => r.KillAllRecordingTasks(), Times.Exactly(1));
            popup.Verify(p => p.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>()), Times.Exactly(1));

        }

        #endregion Test Methods

        // ReSharper restore InconsistentNaming
    }
}
