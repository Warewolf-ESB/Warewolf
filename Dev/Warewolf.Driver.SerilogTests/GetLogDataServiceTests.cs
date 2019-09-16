/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management.Services;
using Moq;
using Newtonsoft.Json;
using Warewolf.Storage;
using Dev2;
using Warewolf.Logger;
using System.IO;
using Serilog.Events;
using System.Threading;
using Dev2.Common;

namespace Warewolf.Driver.Serilog.Tests
{
    [TestClass]
    public class GetLogDataServiceTests
    {
        IDSFDataObject _dSFDataObject;

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("GetLogDataService_FromDB_Execute")]
        public void GetLogDataService_Execute_ShouldNotFilterLogData_WithIsSubExecution()
        {
            //------------Setup for test--------------------------
            var expectedWorkflowId = new Guid("{8f499212-d704-45bb-88a1-5598abe69001}");
            var expectedExecutionId1 = new Guid("{4873493e-f800-4680-8e30-9dca9caf1111}");
            var expectedExecutionId2 = new Guid("{4873493e-f800-4680-8e30-9dca9caf2222}");
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var testMsgTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {NewLine} {Exception}";
            
            //// setup
            var mockedDataObject1 = SetupDataObjectWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId1);
            var mockedDataObject2 = SetupDataObjectWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId2);

            var InfoObj1 = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Information.ToString(), "testDetail evetlevel: Info", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
            var InfoObj2 = AuditStateLogger(mockedDataObject2.Object, LogEventLevel.Information.ToString(), "testDetail evetlevel: Info", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
            var warnObj = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Warning.ToString(), "testDetail evetlevel: warn", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
            var errorObj = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Error.ToString(), "testDetail evetlevel: error", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
            var fatalObj = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Fatal.ToString(), "testDetail evetlevel: fatal", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);

            var infoAuditContent1 = JsonConvert.SerializeObject(InfoObj1);
            var infoAuditContent2 = JsonConvert.SerializeObject(InfoObj2);
            var warnAuditContent = JsonConvert.SerializeObject(warnObj);
            var errorAuditContent = JsonConvert.SerializeObject(errorObj);
            var fatalAuditContent = JsonConvert.SerializeObject(fatalObj);

            var testSettings = new SeriLogSQLiteConfig.Settings
            {
                Path = Config.Server.AuditFilePath,
                Database = "AuditTestDB1.db",
                TableName = "Logs",
                RestrictedToMinimumLevel = LogEventLevel.Verbose,
                FormatProvider = null,
                StoreTimestampInUtc = false,
            };

            var seriConfig = new SeriLogSQLiteConfig(testSettings);

            File.Delete(seriConfig.ConnectionString);

            var loggerSource = new SeriLoggerSource();
            using (var loggerConnection = loggerSource.NewConnection(seriConfig))
            {
                var loggerPublisher = loggerConnection.NewPublisher();
                loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent1);
                Thread.Sleep(1000);

                loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent1);
                loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent2);
                loggerPublisher.Warn(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Warning.ToString(), warnAuditContent);
                loggerPublisher.Error(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Error.ToString(), errorAuditContent, Environment.NewLine, "Test error exeption message");
                loggerPublisher.Fatal(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Fatal.ToString(), fatalAuditContent, Environment.NewLine, "Test fatal exeption message");
                
                Thread.Sleep(1000);
                loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent1);
            }

            //---------------Assert Precondition----------------
            var stringBuilders = new Dictionary<string, StringBuilder>();

            var getLogDataService = new GetLogDataService(seriConfig);

            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<IList<Audit>>(logEntriesJson.ToString());

            Assert.AreEqual(expected: 7, actual: logEntriesObject.Count);

            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(expected: "0", actual: item.VersionNumber);
                Assert.AreEqual(expected: expectedWorkflowId.ToString(), actual: item.WorkflowID.ToString());
                Assert.AreEqual(expected: expectedWorkflowName, actual: item.WorkflowName);
                Assert.AreEqual(expected: "00000000-0000-0000-0000-000000000000".ToString(), actual: item.ServerID.ToString());
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("GetLogDataService_FromDB_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithIsSubExecution()
        {
            //------------Setup for test--------------------------
            var expectedWorkflowId = new Guid("{8f499212-d704-45bb-88a1-5598abe69001}");
            var expectedExecutionId1 = new Guid("{4873493e-f800-4680-8e30-9dca9caf1111}");
            var expectedExecutionId2 = new Guid("{4873493e-f800-4680-8e30-9dca9caf2222}");
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var testMsgTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {NewLine} {Exception}";

            //// setup
            var mockedDataObject1 = SetupDataObjectWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId1);
            var mockedDataObject2 = SetupDataObjectWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId2);
            
            var InfoObj1 = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Information.ToString(), "testDetail evetlevel: Info", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
            var InfoObj2 = AuditStateLogger(mockedDataObject2.Object, LogEventLevel.Information.ToString(), "testDetail evetlevel: Info", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
            var warnObj = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Warning.ToString(), "testDetail evetlevel: warn", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
            var errorObj = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Error.ToString(), "testDetail evetlevel: error", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
            var fatalObj = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Fatal.ToString(), "testDetail evetlevel: fatal", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);

            var infoAuditContent1 = JsonConvert.SerializeObject(InfoObj1);
            var infoAuditContent2 = JsonConvert.SerializeObject(InfoObj2);
            var warnAuditContent = JsonConvert.SerializeObject(warnObj);
            var errorAuditContent = JsonConvert.SerializeObject(errorObj);
            var fatalAuditContent = JsonConvert.SerializeObject(fatalObj);

            var testSettings = new SeriLogSQLiteConfig.Settings
            {
                Path = Config.Server.AuditFilePath,
                Database = "AuditTestDB2.db",
                TableName = "Logs",
                RestrictedToMinimumLevel = LogEventLevel.Verbose,
                FormatProvider = null,
                StoreTimestampInUtc = false,
            };

            var expectedCompletedDateTime = string.Empty;
            var expectedStartDateTime = string.Empty;

            var seriConfig = new SeriLogSQLiteConfig(testSettings);

            File.Delete(seriConfig.ConnectionString);

            var loggerSource = new SeriLoggerSource();
            using (var loggerConnection = loggerSource.NewConnection(seriConfig))
            {
                var loggerPublisher = loggerConnection.NewPublisher();
                loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent1);
                Thread.Sleep(1000);
                expectedStartDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent1);
                loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent2);
                loggerPublisher.Warn(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Warning.ToString(), warnAuditContent);
                loggerPublisher.Error(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Error.ToString(), errorAuditContent, Environment.NewLine, "Test error exeption message");
                loggerPublisher.Fatal(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Fatal.ToString(), fatalAuditContent, Environment.NewLine, "Test fatal exeption message");
                expectedCompletedDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                Thread.Sleep(1000);
                loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent1);
            }

            //---------------Assert Precondition----------------

            var stringBuilders = new Dictionary<string, StringBuilder>
            {
                { "ExecutionID", new StringBuilder(expectedExecutionId1.ToString()) },
                { "StartDateTime", new StringBuilder(expectedStartDateTime.ToString()) },
                { "CompletedDateTime", new StringBuilder(expectedCompletedDateTime.ToString()) }
            };

            var getLogDataService = new GetLogDataService(seriConfig);
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<IList<Audit>>(logEntriesJson.ToString());

            Assert.AreEqual(expected: 4, actual: logEntriesObject.Count);

            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(expected: "0", actual: item.VersionNumber);
                Assert.AreEqual(expected: expectedWorkflowId.ToString(), actual: item.WorkflowID.ToString());
                Assert.AreEqual(expected: expectedExecutionId1.ToString(), actual: item.ExecutionID.ToString());
                Assert.AreEqual(expected: expectedWorkflowName, actual: item.WorkflowName);
                Assert.AreEqual(expected: "00000000-0000-0000-0000-000000000000".ToString(), actual: item.ServerID.ToString());
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("GetLogDataService_FromDB_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_OnEventLevel_WithIsSubExecution()
        {
            //------------Setup for test--------------------------
            var nextActivity = new Mock<IDev2Activity>();
            var principal = new Mock<IPrincipal>();

            var expectedWorkflowId = new Guid("{8f499212-d704-45bb-88a1-5598abe69001}");
            var expectedExecutionId1 = new Guid("{4873493e-f800-4680-8e30-9dca9caf1111}");
            var expectedExecutionId2 = new Guid("{4873493e-f800-4680-8e30-9dca9caf2222}");
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var testMsgTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {NewLine} {Exception}";

            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);

            //// setup
            var mockedDataObject1 = SetupDataObjectWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId1);
            var mockedDataObject2 = SetupDataObjectWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId2);

            var InfoObj1 = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Information.ToString(), "testDetail evetlevel: Info", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
            var InfoObj2 = AuditStateLogger(mockedDataObject2.Object, LogEventLevel.Information.ToString(), "testDetail evetlevel: Info", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
            var warnObj = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Warning.ToString(), "testDetail evetlevel: warn", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
            var errorObj = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Error.ToString(), "testDetail evetlevel: error", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
            var fatalObj = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Fatal.ToString(), "testDetail evetlevel: fatal", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);

            var infoAuditContent1 = JsonConvert.SerializeObject(InfoObj1);
            var infoAuditContent2 = JsonConvert.SerializeObject(InfoObj2);
            var warnAuditContent = JsonConvert.SerializeObject(warnObj);
            var errorAuditContent = JsonConvert.SerializeObject(errorObj);
            var fatalAuditContent = JsonConvert.SerializeObject(fatalObj);

            var testSettings = new SeriLogSQLiteConfig.Settings
            {
                Path = Config.Server.AuditFilePath,
                Database = "AuditTestDB3.db",
                TableName = "Logs",
                RestrictedToMinimumLevel = LogEventLevel.Verbose,
                FormatProvider = null,
                StoreTimestampInUtc = false,
            };

            var expectedCompletedDateTime = string.Empty;
            var expectedStartDateTime = string.Empty;

            var seriConfig = new SeriLogSQLiteConfig(testSettings);

            File.Delete(testSettings.ConnectionString);

            var loggerSource = new SeriLoggerSource();
            using (var loggerConnection = loggerSource.NewConnection(seriConfig))
            {
                var loggerPublisher = loggerConnection.NewPublisher();
                loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent1);
                Thread.Sleep(1000);
                expectedStartDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent1);
                loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent2);
                loggerPublisher.Warn(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Warning.ToString(), warnAuditContent);
                loggerPublisher.Error(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Error.ToString(), errorAuditContent, Environment.NewLine, "Test error exeption message");
                loggerPublisher.Fatal(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Fatal.ToString(), fatalAuditContent, Environment.NewLine, "Test fatal exeption message");
                expectedCompletedDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                Thread.Sleep(1000);
                loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent1);
            }

            //---------------Assert Precondition----------------

            var stringBuilders = new Dictionary<string, StringBuilder>
            {
                { "ExecutionID", new StringBuilder(expectedExecutionId1.ToString()) },
                { "StartDateTime", new StringBuilder(expectedStartDateTime.ToString()) },
                { "CompletedDateTime", new StringBuilder(expectedCompletedDateTime.ToString()) },
                { "EventLevel", new StringBuilder(LogEventLevel.Information.ToString()) }
            };

            var getLogDataService = new GetLogDataService(seriConfig);
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);

            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<IList<Audit>>(logEntriesJson.ToString());

            Assert.AreEqual(expected: 1, actual: logEntriesObject.Count);

            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(expected: "0", actual: item.VersionNumber);
                Assert.AreEqual(expected: expectedWorkflowId.ToString(), actual: item.WorkflowID.ToString());
                Assert.AreEqual(expected: expectedExecutionId1.ToString(), actual: item.ExecutionID.ToString());
                Assert.AreEqual(expected: expectedWorkflowName, actual: item.WorkflowName);
                Assert.AreEqual(expected: "00000000-0000-0000-0000-000000000000".ToString(), actual: item.ServerID.ToString());
            }
        }

        private Audit AuditStateLogger(IDSFDataObject mockedDataObject, string logEventLevel, string detail, IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            return new Audit(mockedDataObject, logEventLevel, detail, previousActivity, nextActivity);
        }

        private Mock<IDSFDataObject> SetupDataObjectWithAssignedInputs(Guid resourceId, string workflowName, Guid executionId)
        {
            // mocks
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => workflowName);
            mockedDataObject.Setup(o => o.ResourceID).Returns(() => resourceId);
            mockedDataObject.Setup(o => o.ExecutionID).Returns(() => executionId);
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);
            _dSFDataObject = mockedDataObject.Object;
            return mockedDataObject;
        }
    }
}
