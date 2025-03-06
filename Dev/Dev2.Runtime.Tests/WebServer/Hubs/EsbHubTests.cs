/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Dynamic;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Communication;
using Dev2.Explorer;
using Dev2.Runtime.WebServer.Hubs;
#if NETFRAMEWORK
using Microsoft.AspNet.SignalR.Hubs;
#else
using Microsoft.AspNetCore.SignalR;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer.Hubs
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    
    public class EsbHubTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EsbHub_AddItemMessage")]
        [TestCategory("COMIPCSaxonCSandStudioTests")]
        public void EsbHub_AddItemMessage_ItemHasData_ItemAddedMessageIsPublished()
        {
            //------------Setup for test--------------------------
            var hub = new MockEsbHub();
#if NETFRAMEWORK
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
#else
            var mockClients = new Mock<IHubCallerClients>();
#endif
            hub.Clients = mockClients.Object;
#if NETFRAMEWORK
            dynamic all = new ExpandoObject();
            var messagePublished = false;
            all.ItemAddedMessage = new Action<string>(serialisedItem =>
            {
                messagePublished = true;
            });
            mockClients.Setup(m => m.All).Returns((ExpandoObject)all);
#else
            IEsbMessage esbMessage = new EsbMessage
            {
                MessagePublished = false
            };

            var mockClientProxy = new Mock<IClientProxy>();
            mockClientProxy.Object.SendAsync("ItemAddedMessage", esbMessage.MessagePublished);
            mockClients.Setup(m => m.All).Returns(mockClientProxy.Object);
#endif
//------------Execute Test---------------------------

#if NETFRAMEWORK
            hub.AddItemMessage(new ServerExplorerItem
                {
                    DisplayName = "Testing",
                    ResourcePath = "Root\\Sub Folder",
                    WebserverUri = "http://localhost"
                });
#else
            var serverExplorerItem = new ServerExplorerItem
            {
                DisplayName = "Testing",
                ResourcePath = "Root\\Sub Folder",
                WebserverUri = "http://localhost"
            };

            hub.AddItemMessage(serverExplorerItem);
            esbMessage = hub.IsMessagePublished(serverExplorerItem, esbMessage);
#endif
            //------------Assert Results-------------------------
#if NETFRAMEWORK
            Assert.IsTrue(messagePublished);
#else
            Assert.IsTrue(esbMessage.MessagePublished);
#endif
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EsbHub_AddItemMessage")]
        public void EsbHub_AddItemMessage_ItemIsNull_ItemAddedMessageIsNotPublished()
        {
            //------------Setup for test--------------------------
            var hub = new MockEsbHub();
#if NETFRAMEWORK
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            dynamic all = new ExpandoObject();
            var messagePublished = false;
            all.ItemAddedMessage = new Action<string>(serialisedItem =>
            {
                messagePublished = true;
            });
            mockClients.Setup(m => m.All).Returns((ExpandoObject)all);

#else
            var mockClients = new Mock<IHubCallerClients>();
            hub.Clients = mockClients.Object;

            IEsbMessage esbMessage = new EsbMessage
            {
                MessagePublished = false
            };

            var mockClientProxy = new Mock<IClientProxy>();
            mockClientProxy.Object.SendAsync("ItemAddedMessage", esbMessage.MessagePublished);
            mockClients.Setup(m => m.All).Returns(mockClientProxy.Object);
#endif
            //------------Execute Test---------------------------
#if NETFRAMEWORK
            hub.AddItemMessage(null);
#else
            ServerExplorerItem serverExplorerItem = null;

            hub.AddItemMessage(serverExplorerItem);
            esbMessage = hub.IsMessagePublished(serverExplorerItem, esbMessage);
#endif
            //------------Assert Results-------------------------
#if NETFRAMEWORK
            Assert.IsFalse(messagePublished);
#else
            Assert.IsFalse(esbMessage.MessagePublished);
#endif
        }
    }

    public class MockEsbHub : EsbHub
    {
        public MockEsbHub()
        {
            SetupEvents();
        }

        public void TestOnCompilerMessageReceived(IList<ICompileMessageTO> messages)
        {
            OnCompilerMessageReceived(messages);
        }

        public List<IMemo> WriteEventProviderMemos { get; private set; }
    }
}
