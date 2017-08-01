using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetLogDataServiceTest
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();

            //------------Execute Test---------------------------
            var resId = getLogDataService.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();

            //------------Execute Test---------------------------
            var resId = getLogDataService.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Administrator, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetLogDataService_ServerLogFilePath")]
        public void GetLogDataService_ServerLogFilePath_NotSet_ShouldReturnStandardValue()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();

            //------------Execute Test---------------------------
            var logFilePath = getLogDataService.ServerLogFilePath;
            //------------Assert Results-------------------------
            Assert.AreEqual(EnvironmentVariables.ServerLogFile, logFilePath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetLogDataService_ServerLogFilePath")]
        public void GetLogDataService_ServerLogFilePath_WhenSet_ShouldReturnSetValue()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService { ServerLogFilePath = "MyPath" };
            //------------Execute Test---------------------------
            var logFilePath = getLogDataService.ServerLogFilePath;
            //------------Assert Results-------------------------
            Assert.AreEqual("MyPath", logFilePath);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_WithLogData_ShouldReturnLogDataObject()
        {
            //------------Setup for test--------------------------
            const string logData = @"2017-05-18 10:41:03,500 [(null)] INFO  - Started Execution for Service Name:tttt Resource Id:a2352f24-fe9f-4e7d-bd0c-f5267b362d0b Mode:Debug
2017-05-18 10:41:03,519 [(null)] DEBUG - Getting Resource to Execute
2017-05-18 10:41:03,519 [(null)] DEBUG - Fetching Execution Plan for a2352f24-fe9f-4e7d-bd0c-f5267b362d0b for workspace 00000000-0000-0000-0000-000000000000
2017-05-18 10:41:03,611 [(null)] DEBUG - Got Resource to Execute
2017-05-18 10:41:03,625 [(null)] INFO  - Debug Already Started
2017-05-18 10:41:03,681 [(null)] INFO  - Completed Execution for Service Name:tttt Resource Id: a2352f24-fe9f-4e7d-bd0c-f5267b362d0b Mode:Debug
";
            var getLogDataService = new GetLogDataService();
            var logFilePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            getLogDataService.ServerLogFilePath = logFilePath;
            File.WriteAllText(logFilePath, logData);
            //------------Execute Test---------------------------
            var logEntriesJson = getLogDataService.Execute(new Dictionary<string, StringBuilder>(), null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.IsNotNull(logEntriesObject);

        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_WithLogDataContainingURl_ShouldReturnLogDataObjectWithUrl()
        {
            //------------Setup for test--------------------------
            const string logFilePath = @"TextFiles\LogFileWithFlatResultsNEwFormat.txt";
            var getLogDataService = new GetLogDataService { ServerLogFilePath = logFilePath };
            //------------Execute Test---------------------------
            var logEntriesJson = getLogDataService.Execute(new Dictionary<string, StringBuilder>(), null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.IsNotNull(logEntriesObject);
            var value = logEntriesObject[0].Url;
            Assert.IsFalse(string.IsNullOrEmpty(value));
            Assert.AreEqual("http://RSAKLFPETERB:3142/Examples\\Loop Constructs - Select and Apply.XML", value);

        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_WithStartDate_ShouldFilterLogData()
        {
            //------------Setup for test--------------------------
            const string logFilePath = @"TextFiles\LogFileWithFlatResultsNEwFormat.txt";
            var getLogDataService = new GetLogDataService { ServerLogFilePath = logFilePath };
            //---------------Assert Precondition----------------
            var logEntriesJson = getLogDataService.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(1, logEntriesObject.Count);
            //------------Execute Test---------------------------

            var stringBuilders = new Dictionary<string, StringBuilder>();
            var longDateString = DateTime.ParseExact("2017-05-25 08:14:22,519", GlobalConstants.LogFileDateFormat,  System.Globalization.CultureInfo.InvariantCulture);
            stringBuilders.Add("StartDateTime",longDateString.ToString(GlobalConstants.LogFileDateFormat).ToStringBuilder());
            logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(1, logEntriesObject.Count);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_WithEndDate_ShouldFilterLogData()
        {
            //------------Setup for test--------------------------
            const string logFilePath = @"TextFiles\LogFileWithFlatResultsNEwFormat.txt";
            var getLogDataService = new GetLogDataService { ServerLogFilePath = logFilePath };
            //---------------Assert Precondition----------------
            var logEntriesJson = getLogDataService.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(1, logEntriesObject.Count);
            //------------Execute Test---------------------------

            var stringBuilders = new Dictionary<string, StringBuilder>();
            var longDateString = DateTime.ParseExact("2017-05-25 08:14:12,420", GlobalConstants.LogFileDateFormat,  System.Globalization.CultureInfo.InvariantCulture);
            stringBuilders.Add("CompletedDateTime", longDateString.ToString(GlobalConstants.LogFileDateFormat).ToStringBuilder());
            logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(0, logEntriesObject.Count);

        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_WithStatus_ShouldFilterLogData()
        {
            //------------Setup for test--------------------------
            const string logFilePath = @"TextFiles\LogFileWithFlatResultsNEwFormat.txt";
            var getLogDataService = new GetLogDataService { ServerLogFilePath = logFilePath };
            //---------------Assert Precondition----------------
            var logEntriesJson = getLogDataService.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(1, logEntriesObject.Count);
            //------------Execute Test---------------------------

            var stringBuilders = new Dictionary<string, StringBuilder> {{"Status", "Failed".ToStringBuilder()}};
            logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(0, logEntriesObject.Count);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_WithUser_ShouldFilterLogData()
        {
            //------------Setup for test--------------------------
            const string logFilePath = @"TextFiles\LogFileWithFlatResultsNEwFormat.txt";
            var getLogDataService = new GetLogDataService { ServerLogFilePath = logFilePath };
            //---------------Assert Precondition----------------
            var logEntriesJson = getLogDataService.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(1, logEntriesObject.Count);
            //------------Execute Test---------------------------

            var stringBuilders = new Dictionary<string, StringBuilder> {{ "User", "DEV2\\nkosinathi.sangweni".ToStringBuilder()}};
            logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(1, logEntriesObject.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_WithExecutionId_ShouldFilterLogData()
        {
            //------------Setup for test--------------------------
            const string logFilePath = @"TextFiles\LogFileWithFlatResultsNEwFormat.txt";
            var getLogDataService = new GetLogDataService { ServerLogFilePath = logFilePath };
            //---------------Assert Precondition----------------
            var logEntriesJson = getLogDataService.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(1, logEntriesObject.Count);
            //------------Execute Test---------------------------

            var stringBuilders = new Dictionary<string, StringBuilder> {{ "ExecutionId", "674b2bf6-225d-446b-a645-aee95ed731af".ToStringBuilder()}};
            logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(1, logEntriesObject.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_WithExecutionTime_ShouldFilterLogData()
        {
            //------------Setup for test--------------------------
            const string logFilePath = @"TextFiles\LogFileWithFlatResultsNEwFormat.txt";
            var getLogDataService = new GetLogDataService { ServerLogFilePath = logFilePath };
            //---------------Assert Precondition----------------
            var logEntriesJson = getLogDataService.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(1, logEntriesObject.Count);
            //------------Execute Test---------------------------

            var stringBuilders = new Dictionary<string, StringBuilder> {{ "ExecutionTime", "601".ToStringBuilder()}};
            logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(1, logEntriesObject.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_WithBadUser_ShouldFilterLogData()
        {
            //------------Setup for test--------------------------
            const string logFilePath = @"TextFiles\LogFileWithFlatResultsNEwFormat.txt";
            var getLogDataService = new GetLogDataService { ServerLogFilePath = logFilePath };
            //---------------Assert Precondition----------------
            var logEntriesJson = getLogDataService.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(1, logEntriesObject.Count);
            //------------Execute Test---------------------------

            var stringBuilders = new Dictionary<string, StringBuilder> {{ "User", "BadUser".ToStringBuilder()}};
            logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            logEntriesObject = JsonConvert.DeserializeObject<List<LogEntry>>(logEntriesJson.ToString());
            Assert.AreEqual(0, logEntriesObject.Count);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("GetLogDataService_HandlesType")]
        public void GetLogDataService_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("GetLogDataService", getLogDataService.HandlesType());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("GetLogDataService_HandlesType")]
        public void GetLogDataService_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();


            //------------Execute Test---------------------------
            var a = getLogDataService.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification.ToString();
            Assert.AreEqual("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }

}