using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            Assert.AreEqual(EnvironmentVariables.ServerLogFile,logFilePath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetLogDataService_ServerLogFilePath")]
        public void GetLogDataService_ServerLogFilePath_WhenSet_ShouldReturnSetValue()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            getLogDataService.ServerLogFilePath = "MyPath";
            //------------Execute Test---------------------------
            var logFilePath = getLogDataService.ServerLogFilePath;
            //------------Assert Results-------------------------
            Assert.AreEqual("MyPath",logFilePath);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_WithLogData_ShouldReturnLogDataObject()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            var logFilePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            getLogDataService.ServerLogFilePath = logFilePath;
            File.WriteAllText(logFilePath,"");
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
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