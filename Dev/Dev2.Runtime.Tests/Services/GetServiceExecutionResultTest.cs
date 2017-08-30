using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetServiceExecutionResultTest
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetServiceExecutionResult();

            //------------Execute Test---------------------------
            var resId = getLogDataService.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetServiceExecutionResult();

            //------------Execute Test---------------------------
            var resId = getLogDataService.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Administrator, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_HandlesType")]
        public void GetLogDataService_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetServiceExecutionResult();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("GetServiceExecutionResult", getLogDataService.HandlesType());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        [DeploymentItem(@"TextFiles\LogFileWithFlatResultsNEwFormat.txt", "TextFiles")]
        public void GetLogDataService_Execute_WithExecutionId_ShouldFilterLogData()
        {
            //------------Setup for test--------------------------
            const string LogFilePath = @"TextFiles\LogFileWithFlatResultsNEwFormat.txt";
            var getLogDataService = new GetServiceExecutionResult { ServerLogFilePath = LogFilePath };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(getLogDataService);
            //------------Execute Test---------------------------
            var stringBuilders = new Dictionary<string, StringBuilder> { { "ExecutionId", new StringBuilder("06385e0f-ac27-4cf0-af55-7642c3c08ba3") } };
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var logEntry = serializer.Deserialize<LogEntry>(logEntriesJson.ToString());
            Assert.AreEqual("{  \"Message\": \"Hello World.\"}", logEntry.Result);

        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        [DeploymentItem(@"TextFiles\LogFileWithFlatResultsNEwFormat.txt", "TextFiles")]
        public void GetLogDataService_Execute_WithExecutionId_ShouldFilterLogData_NewFormat()
        {
            //------------Setup for test--------------------------
            const string LogFilePath = @"TextFiles\LogFileWithFlatResultsNEwFormat.txt";
            var getLogDataService = new GetServiceExecutionResult { ServerLogFilePath = LogFilePath };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(getLogDataService);
            //------------Execute Test---------------------------
            var stringBuilders = new Dictionary<string, StringBuilder> { { "ExecutionId", new StringBuilder("06385e0f-ac27-4cf0-af55-7642c3c08ba3") } };
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var logEntry = serializer.Deserialize<LogEntry>(logEntriesJson.ToString());
            Assert.IsNotNull(logEntry);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_GetLogEntryValues")]
        public void GetLogEntryValues_GivenLogEntry_ExpectCorrectResult()
        {
            //------------Setup for test--------------------------
            const string logEntry = @"2017-07-13 08:02:52,799 DEBUG - [52cea226-d594-49eb-9c37-0598bd2803f5] - Request URL [ http://RSAKLFPETERB:3142/Examples\Loop Constructs - Select and Apply.XML ]";
            LogDataServiceBase dataServiceBase = new LogDataServiceBase();
            //---------------Assert Precondition----------------
            Assert.AreEqual(dataServiceBase.GetAuthorizationContextForService(), AuthorizationContext.Administrator);
            //------------Execute Test---------------------------
            var privateObject = new PrivateObject(dataServiceBase);
            var invoke = (string[])privateObject.Invoke("GetLogEntryValues", BindingFlags.NonPublic | BindingFlags.Instance, logEntry);
            Assert.AreEqual(5, invoke.Length);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_GetLogEntryValues")]
        public void GetLogEntryValues_GivenLogEntry_ExpectCorrectResult_NewFormat()
        {
            //------------Setup for test--------------------------
            const string logEntry = @"2017-07-13 10:16:55,613 DEBUG - [03659971-6b50-42e7-af3e-1177fc2e86ed] - Mapping Inputs from Environment";
            LogDataServiceBase dataServiceBase = new LogDataServiceBase();
            //---------------Assert Precondition----------------
            Assert.AreEqual(dataServiceBase.GetAuthorizationContextForService(), AuthorizationContext.Administrator);
            //------------Execute Test---------------------------
            var privateObject = new PrivateObject(dataServiceBase);
            var invoke = (string[])privateObject.Invoke("GetLogEntryValues", BindingFlags.NonPublic | BindingFlags.Instance, logEntry);
            Assert.AreEqual(5, invoke.Length);
        }
    }
}