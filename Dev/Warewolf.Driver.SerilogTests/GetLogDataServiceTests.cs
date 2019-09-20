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
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management.Services;
using Moq;
using Newtonsoft.Json;
using Warewolf.Storage;
using Dev2;
using Warewolf.Logger;
using Serilog.Events;
using System.Threading;
using Dev2.Common;
using Serilog;
using System.IO;
using Warewolf.Auditing;

namespace Warewolf.Driver.Serilog.Tests
{
    //public class PublishedDataSingleton
    //{
    //    static IDSFDataObject _dSFDataObject;

    //    private static readonly PublishedData publishedData = new PublishedData();

    //    public PublishedDataSingleton()
    //    {

    //    }

    //    public static PublishedData Instance()
    //    {
    //        File.Delete(publishedData.DbPath);
    //        var testMsgTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {NewLine} {Exception}";

    //        var mockSerilogConfig = new Mock<ISeriLogConfig>();
    //        ILogger testLogger = new LoggerConfiguration().WriteTo.SQLite(publishedData.DbPath).CreateLogger();

    //        //// setup
    //        var mockedDataObject1 = SetupDataObjectWithAssignedInputs(publishedData.ExpectedWorkflowId, publishedData.ExpectedWorkflowName, publishedData.ExpectedExecutionId1);
    //        var mockedDataObject2 = SetupDataObjectWithAssignedInputs(publishedData.ExpectedWorkflowId, publishedData.ExpectedWorkflowName, publishedData.ExpectedExecutionId2);

    //        var InfoObj1 = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Information.ToString(), "testDetail evetlevel: Info", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
    //        var InfoObj2 = AuditStateLogger(mockedDataObject2.Object, LogEventLevel.Information.ToString(), "testDetail evetlevel: Info", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
    //        var warnObj = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Warning.ToString(), "testDetail evetlevel: warn", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
    //        var errorObj = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Error.ToString(), "testDetail evetlevel: error", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);
    //        var fatalObj = AuditStateLogger(mockedDataObject1.Object, LogEventLevel.Fatal.ToString(), "testDetail evetlevel: fatal", new Mock<IDev2Activity>().Object, new Mock<IDev2Activity>().Object);

    //        var infoAuditContent1 = JsonConvert.SerializeObject(InfoObj1);
    //        var infoAuditContent2 = JsonConvert.SerializeObject(InfoObj2);
    //        var warnAuditContent = JsonConvert.SerializeObject(warnObj);
    //        var errorAuditContent = JsonConvert.SerializeObject(errorObj);
    //        var fatalAuditContent = JsonConvert.SerializeObject(fatalObj);

    //        mockSerilogConfig.Setup(o => o.ConnectionString).Returns(publishedData.DbPath);
    //        mockSerilogConfig.Setup(o => o.Logger).Returns(testLogger);


    //        var loggerSource = new SeriLoggerSource
    //        {
    //            ConnectionString = publishedData.DbPath
    //        };
    //        using (var loggerConnection = loggerSource.NewConnection(mockSerilogConfig.Object))
    //        {
    //            var loggerPublisher = loggerConnection.NewPublisher();
    //            loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent1);
    //            Thread.Sleep(1000);
    //            publishedData.ExpectedStartDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
    //            loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent1);
    //            loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent2);
    //            loggerPublisher.Warn(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Warning.ToString(), warnAuditContent);
    //            loggerPublisher.Error(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Error.ToString(), errorAuditContent, Environment.NewLine, "Test error exeption message");
    //            loggerPublisher.Fatal(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Fatal.ToString(), fatalAuditContent, Environment.NewLine, "Test fatal exeption message");
    //            publishedData.ExpectedCompletedDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
    //            Thread.Sleep(1000);
    //            loggerPublisher.Info(outputTemplate: testMsgTemplate, DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), LogEventLevel.Information.ToString(), infoAuditContent1);
    //        };

    //        return publishedData;
    //    }

    //    private static Audit AuditStateLogger(IDSFDataObject mockedDataObject, string logEventLevel, string detail, IDev2Activity previousActivity, IDev2Activity nextActivity)
    //    {
    //        return new Audit(mockedDataObject, logEventLevel, detail, previousActivity, nextActivity);
    //    }

    //    private static Mock<IDSFDataObject> SetupDataObjectWithAssignedInputs(Guid resourceId, string workflowName, Guid executionId)
    //    {
    //        // mocks
    //        var mockedDataObject = new Mock<IDSFDataObject>();
    //        mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
    //        mockedDataObject.Setup(o => o.ServiceName).Returns(() => workflowName);
    //        mockedDataObject.Setup(o => o.ResourceID).Returns(() => resourceId);
    //        mockedDataObject.Setup(o => o.ExecutionID).Returns(() => executionId);
    //        var principal = new Mock<IPrincipal>();
    //        principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
    //        mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
    //        mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);
    //        _dSFDataObject = mockedDataObject.Object;
    //        return mockedDataObject;
    //    }
    //}

    //public class PublishedData
    //{
    //    public string DbPath { get; set; } = @"C:\ProgramData\Warewolf\Audits\AuditDB.db";
    //    public string ExpectedWorkflowName { get; set; } = "LogExecuteCompleteState_Workflow";
    //    public Guid ExpectedWorkflowId { get; set; } = new Guid("{8f499212-d704-45bb-88a1-5598abe69001}");
    //    public Guid ExpectedExecutionId1 { get; set; } = new Guid("{4873493e-f800-4680-8e30-9dca9caf1111}");
    //    public Guid ExpectedExecutionId2 { get; set; } = new Guid("{4873493e-f800-4680-8e30-9dca9caf2222}");
    //    public string ExpectedStartDateTime { get; set; }
    //    public string ExpectedCompletedDateTime { get; set; }
    //}


    [TestClass]
    public class GetLogDataServiceTests
    {
        //  static PublishedData _publishedData = PublishedDataSingleton.Instance();
        static Mock<ISeriLogConfig> _mockSeriConfig = new Mock<ISeriLogConfig>();

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(GetLogDataService))]
        public void GetLogDataService_Execute_ShouldNotFilterLogData_WithIsSubExecution()
        {
            //------------------------------Arrange----------------------------------
            var stringBuilders = new Dictionary<string, StringBuilder>
            {
                { "ResourceID", new StringBuilder(GlobalConstants.DefaultLoggingSourceId) }
            };

            //------------------------------Act--------------------------------------
            var getLogDataService = new GetLogDataService();
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------------------------Assert-----------------------------------

            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<IList<Audit>>(logEntriesJson.ToString());

            Assert.AreEqual(expected: 7, actual: logEntriesObject.Count);

            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(expected: "0", actual: item.VersionNumber);
                //  Assert.AreEqual(expected: _publishedData.ExpectedWorkflowId.ToString(), actual: item.WorkflowID.ToString());
                //  Assert.AreEqual(expected: _publishedData.ExpectedWorkflowName, actual: item.WorkflowName);
                //  Assert.AreEqual(expected: "00000000-0000-0000-0000-000000000000".ToString(), actual: item.ServerID.ToString());
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(GetLogDataService))]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithIsSubExecution()
        {
            //------------------------------Arrange----------------------------------
            var stringBuilders = new Dictionary<string, StringBuilder>
            {
             //   { "ExecutionID", new StringBuilder(_publishedData.ExpectedExecutionId1.ToString()) },
            //    { "StartDateTime", new StringBuilder(_publishedData.ExpectedStartDateTime.ToString()) },
            //    { "CompletedDateTime", new StringBuilder(_publishedData.ExpectedCompletedDateTime.ToString()) },
                { "ResourceID", new StringBuilder(GlobalConstants.DefaultLoggingSourceId) }
            };

            //------------------------------Act--------------------------------------
            var getLogDataService = new GetLogDataService();
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);

            //------------------------------Assert-----------------------------------
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<IList<Audit>>(logEntriesJson.ToString());

            Assert.AreEqual(expected: 4, actual: logEntriesObject.Count);

            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(expected: "0", actual: item.VersionNumber);
                // Assert.AreEqual(expected: _publishedData.ExpectedWorkflowId.ToString(), actual: item.WorkflowID.ToString());
                // Assert.AreEqual(expected: _publishedData.ExpectedExecutionId1.ToString(), actual: item.ExecutionID.ToString());
                //  Assert.AreEqual(expected: _publishedData.ExpectedWorkflowName, actual: item.WorkflowName);
                Assert.AreEqual(expected: "00000000-0000-0000-0000-000000000000".ToString(), actual: item.ServerID.ToString());
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(GetLogDataService))]
        public void GetLogDataService_Execute_ShouldFilterLogData_OnEventLevel_WithIsSubExecution()
        {
            //------------------------------Arrange----------------------------------
            var stringBuilders = new Dictionary<string, StringBuilder>
            {
               { "ResourceID", new StringBuilder(GlobalConstants.DefaultLoggingSourceId) }
            };

            //------------------------------Act--------------------------------------
            var getLogDataService = new GetLogDataService();
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------------------------Assert-----------------------------------

            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<IList<Audit>>(logEntriesJson.ToString());

            Assert.AreEqual(expected: 1, actual: logEntriesObject.Count);

            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(expected: "0", actual: item.VersionNumber);
                Assert.AreEqual(expected: "00000000-0000-0000-0000-000000000000".ToString(), actual: item.ServerID.ToString());
            }
        }
    }
}
