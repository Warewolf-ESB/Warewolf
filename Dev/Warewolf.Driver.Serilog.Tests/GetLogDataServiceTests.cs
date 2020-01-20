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
using Warewolf.Interfaces.Auditing;
using System.Linq;
using Dev2.Workspaces;
using Dev2.Common.Serializers;
using System.Runtime.Serialization;
using Dev2.Common.Interfaces.Enums;

namespace Warewolf.Driver.Serilog.Tests
{
    [TestClass]
    public class GetLogDataServiceTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(GetLogDataService))]
        public void GetLogDataService_GetAuthorizationContextForService_Returns_Administrator()
        {
            //------------Setup for test-------------------------
            var getLogData = new GetLogDataService();
            //------------Execute Test---------------------------
            var authorizationContextForService = getLogData.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Administrator, authorizationContextForService);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(GetLogDataService))]
        public void GetLogDataService_CreateServiceEntry_Returns_GetExecutionHistory()
        {
            //------------Setup for test-------------------------
            var getLogData = new GetLogDataService();
            //------------Execute Test---------------------------
            var dynamicService = getLogData.CreateServiceEntry();
            var handleType = getLogData.HandlesType();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsFalse(string.IsNullOrEmpty(handleType));
            Assert.AreEqual(handleType, dynamicService.Name);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(GetLogDataService))]
        public void GetLogDataService_GivenEmptyArgs_Returns_ExecuteMessage()
        {

            //------------Setup for test-------------------------
            var webSocketFactoryMock = new Mock<IWebSocketPool>();
            var webSocketWrapperMock = new Mock<IWebSocketWrapper>();
            var commandMessage = "";
            var onMessage = new Action<string, IWebSocketWrapper>((s, w) => { });
            webSocketWrapperMock.Setup(ws => ws.SendMessage(It.IsAny<string>())).Callback((string s) =>
            {
                commandMessage = s;
            }).Verifiable();

            webSocketWrapperMock.Setup(c => c.Connect()).Returns(webSocketWrapperMock.Object);
            webSocketWrapperMock.Setup(ws => ws.OnMessage(onMessage));



            webSocketFactoryMock.Setup(c => c.Acquire(It.IsAny<string>())).Returns(webSocketWrapperMock.Object);

            var getLogData = new GetLogDataService(webSocketFactoryMock.Object, TimeSpan.FromMilliseconds(1));
            var workspaceMock = new Mock<IWorkspace>();
            //------------Execute Test---------------------------
            var requestArgs = new Dictionary<string, StringBuilder>();
            var executeResults = getLogData.Execute(requestArgs, workspaceMock.Object);
            var jsonSerializer = new Dev2JsonSerializer();
            Assert.IsNotNull(executeResults);
            var deserializedResults = jsonSerializer.Deserialize<IList<IAudit>>(executeResults);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deserializedResults);
            Assert.AreEqual(0, deserializedResults.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(GetLogDataService))]
        public void GetLogDataService_Execute_Should_ExecuteSendMessage()
        {
            //------------------------------Arrange----------------------------------
            var stringBuilders = new Dictionary<string, StringBuilder>
            {
               { "ResourceID", new StringBuilder(GlobalConstants.DefaultLoggingSourceId) }
            };
            var webSocketFactoryMock = new Mock<IWebSocketPool>();
            var webSocketWrapperMock = new Mock<IWebSocketWrapper>();
            var commandMessage = "";
            var onMessage = new Action<string, IWebSocketWrapper>((s,w) => { });
            webSocketWrapperMock.Setup(ws => ws.SendMessage(It.IsAny<string>())).Callback((string s) =>
            {
                commandMessage = s;
            }).Verifiable();

            webSocketWrapperMock.Setup(c => c.Connect()).Returns(webSocketWrapperMock.Object);
            webSocketWrapperMock.Setup(ws => ws.OnMessage(onMessage));

           

            webSocketFactoryMock.Setup(c => c.Acquire(It.IsAny<string>())).Returns(webSocketWrapperMock.Object);
            
            //------------------------------Act--------------------------------------
            var getLogDataService = new GetLogDataService(webSocketFactoryMock.Object,TimeSpan.FromMilliseconds(1));
            getLogDataService.Execute(stringBuilders, null);
            //------------------------------Assert-----------------------------------

            var auditCommand = JsonConvert.DeserializeObject<AuditCommand>(commandMessage);
            Assert.IsNotNull(auditCommand);
            Assert.AreEqual("LogQuery", auditCommand.Type);
            Assert.AreEqual(1, auditCommand.Query.Count);
            Assert.AreEqual("ResourceID", auditCommand.Query.First().Key);
            Assert.AreEqual(GlobalConstants.DefaultLoggingSourceId, auditCommand.Query.First().Value.ToString());

            webSocketWrapperMock.Verify(s => s.OnMessage(It.IsAny<Action<string, IWebSocketWrapper>>()),Times.Once);
        }
    }
}
