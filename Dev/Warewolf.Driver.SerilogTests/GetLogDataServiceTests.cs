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

namespace Warewolf.Driver.Serilog.Tests
{
    [TestClass]
    public class GetLogDataServiceTests
    {


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
            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            var webSocketWrapperMock = new Mock<IWebSocketWrapper>();
            var commandMessage = "";
            var onMessage = new Action<string, IWebSocketWrapper>((s,w) => { });
            webSocketWrapperMock.Setup(ws => ws.SendMessage(It.IsAny<string>())).Callback((string s) =>
            {
                commandMessage = s;
            }).Verifiable();

            webSocketWrapperMock.Setup(c => c.Connect()).Returns(webSocketWrapperMock.Object);
            webSocketWrapperMock.Setup(ws => ws.OnMessage(onMessage));

           

            webSocketFactoryMock.Setup(c => c.New()).Returns(webSocketWrapperMock.Object);
            
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
