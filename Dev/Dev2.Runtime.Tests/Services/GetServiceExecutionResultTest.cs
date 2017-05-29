using System;
using System.Collections.Generic;
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
            Assert.AreEqual("GetServiceExecutionService", getLogDataService.HandlesType());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_WithExecutionId_ShouldFilterLogData()
        {
            //------------Setup for test--------------------------
            const string LogFilePath = @"TextFiles\LogFileWithFlatResults.txt";
            var getLogDataService = new GetServiceExecutionResult { ServerLogFilePath = LogFilePath };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(getLogDataService);
            //------------Execute Test---------------------------
            var stringBuilders = new Dictionary<string, StringBuilder> { { "ExecutionId", new StringBuilder("be4f2201-63ea-42ac-b90d-564e7077b533") } };
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var logEntry = serializer.Deserialize<LogEntry>(logEntriesJson.ToString());
            Assert.AreEqual("{  \"a\": \"a\"}", logEntry.Result);

        }
    }
}