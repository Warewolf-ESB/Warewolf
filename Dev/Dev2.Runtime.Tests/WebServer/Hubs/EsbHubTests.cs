/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Explorer;
using Dev2.Runtime.WebServer.Hubs;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer.Hubs
{
    [TestClass]
    
    public class EsbHubTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EsbHub_AddItemMessage")]
        public void EsbHub_AddItemMessage_ItemHasData_ItemAddedMessageIsPublished()
        {
            //------------Setup for test--------------------------
            var hub = new MockEsbHub();
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            hub.Clients = mockClients.Object;
            dynamic all = new ExpandoObject();
            bool messagePublished = false;
            all.ItemAddedMessage = new Action<string>(serialisedItem =>
            {
                messagePublished = true;
            });
            mockClients.Setup(m => m.All).Returns((ExpandoObject)all);
            //------------Execute Test---------------------------
            hub.AddItemMessage(new ServerExplorerItem
                {
                    DisplayName = "Testing",
                    ResourcePath = "Root\\Sub Folder",
                    WebserverUri = "http://localhost"
                });
            //------------Assert Results-------------------------
            Assert.IsTrue(messagePublished);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EsbHub_AddItemMessage")]
        public void EsbHub_AddItemMessage_ItemIsNull_ItemAddedMessageIsNotPublished()
        {
            //------------Setup for test--------------------------
            var hub = new MockEsbHub();
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            hub.Clients = mockClients.Object;
            dynamic all = new ExpandoObject();
            bool messagePublished = false;
            all.ItemAddedMessage = new Action<string>(serialisedItem =>
            {
                messagePublished = true;
            });
            mockClients.Setup(m => m.All).Returns((ExpandoObject)all);
            //------------Execute Test---------------------------
            hub.AddItemMessage(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(messagePublished);
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
