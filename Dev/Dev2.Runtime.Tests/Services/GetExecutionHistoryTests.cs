/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Warewolf.Auditing;
using Warewolf.Interfaces.Auditing;
using Warewolf.Triggers;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetExecutionHistoryTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetExecutionHistory))]
        public void GetExecutionHistory_GetAuthorizationContextForService_Returns_Administrator()
        {
            //------------Setup for test-------------------------
            var getExecutionHistory = new GetExecutionHistory();
            //------------Execute Test---------------------------
            var authorizationContextForService = getExecutionHistory.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Administrator, authorizationContextForService);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetExecutionHistory))]
        public void GetExecutionHistory_CreateServiceEntry_Returns_GetExecutionHistory()
        {
            //------------Setup for test-------------------------
            var getExecutionHistory = new GetExecutionHistory();
            //------------Execute Test---------------------------
            var dynamicService = getExecutionHistory.CreateServiceEntry();
            var handleType = getExecutionHistory.HandlesType();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsFalse(string.IsNullOrEmpty(handleType));
            Assert.AreEqual(handleType, dynamicService.Name);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetExecutionHistory))]
        public void GetExecutionHistory_GivenEmptyArgs_Returns_ExecuteMessage()
        {

            //------------Setup for test-------------------------
            var getExecutionHistory = new GetExecutionHistory();
            var workspaceMock = new Mock<IWorkspace>();
            //------------Execute Test---------------------------
            var requestArgs = new Dictionary<string, StringBuilder>();
            var executeResults = getExecutionHistory.Execute(requestArgs, workspaceMock.Object);
            var jsonSerializer = new Dev2JsonSerializer();
            Assert.IsNotNull(executeResults);
            var deserializedResults = jsonSerializer.Deserialize<IList<IExecutionHistory>>(executeResults);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deserializedResults);
            Assert.AreEqual(0, deserializedResults.Count);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetExecutionHistory))]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetExecutionHistory_GivenNullArgs_Returns_InvalidDataContractException()
        {
            //------------Setup for test-------------------------
            var getExecutionHistory = new GetExecutionHistory();
            var workspaceMock = new Mock<IWorkspace>();
            //------------Execute Test---------------------------           
            var executeResults = getExecutionHistory.Execute(null, workspaceMock.Object);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetExecutionHistory))]
        public void GetExecutionHistory_Execute()
        {
            var stringBuilders = new Dictionary<string, StringBuilder>
            {
               { "ResourceId", new StringBuilder(GlobalConstants.DefaultLoggingSourceId) }
            };
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



            webSocketFactoryMock.Setup(c => c.Acquire(Config.Auditing.Endpoint)).Returns(webSocketWrapperMock.Object);

            //------------------------------Act--------------------------------------
            var getExecutionHistory = new GetExecutionHistory(webSocketFactoryMock.Object, TimeSpan.FromMilliseconds(1));
            getExecutionHistory.Execute(stringBuilders, null);
            //------------------------------Assert-----------------------------------

            var auditCommand = JsonConvert.DeserializeObject<AuditCommand>(commandMessage);
            Assert.IsNotNull(auditCommand);
            Assert.AreEqual("TriggerQuery", auditCommand.Type);
            Assert.AreEqual(1, auditCommand.Query.Count);
            Assert.AreEqual("ResourceId", auditCommand.Query.First().Key);
            Assert.AreEqual(GlobalConstants.DefaultLoggingSourceId, auditCommand.Query.First().Value.ToString());

            webSocketWrapperMock.Verify(s => s.OnMessage(It.IsAny<Action<string, IWebSocketWrapper>>()), Times.Once);
        }
    }
}
