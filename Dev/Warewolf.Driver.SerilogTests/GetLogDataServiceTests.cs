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
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Container;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Logging;
using Dev2.Communication;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management.Services;
using Moq;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.IO;
using System.Linq;
using Warewolf.Driver.Serilog;
using Warewolf.Storage;
using Dev2;
using Warewolf.Logger;
using System.Net.Http;

namespace Warewolf.Driver.Serilog.Tests
{
    [TestClass]
    public class GetLogDataServiceTests
    {

        Mock<IDev2Activity> _activity;
        IDSFDataObject _dSFDataObject;


        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("GetLogDataService_FromDB_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithIsSubExecution()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            var executionId = Guid.NewGuid();
            //// setup
            var mockedDataObject = SetupDataObjectWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, executionId);
            var result = new Audit();
            var Content = JsonConvert.SerializeObject(result);

            var seriConfig = new TestSeriLogSQLiteConfig();
            var loggerSource = new SeriLoggerSource();
            using (var loggerConnection = loggerSource.NewConnection(seriConfig))
            {
                var loggerPublisher = loggerConnection.NewPublisher();
                loggerPublisher.Info(Content);
            }        

            //---------------Assert Precondition----------------
            var stringBuilders = new Dictionary<string, StringBuilder>();            
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var serializer = new Dev2JsonSerializer();
            var logEntriesObject = serializer.Deserialize<List<Audit>>(logEntriesJson.ToString());
            foreach (var item in logEntriesObject)
            {

                Assert.AreEqual("0", item.IsSubExecution.ToString());
            }
        }
        private Audit AuditStateLogger(Mock<IDSFDataObject> mockedDataObject)
        {
            return new Audit();
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
