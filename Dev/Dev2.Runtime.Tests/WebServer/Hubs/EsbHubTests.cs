
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Communication;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Explorer;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.WebServer.Hubs;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Dev2.Tests.Runtime.WebServer.Hubs
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class EsbHubTests
    {
        /// <summary>
        /// Valid server created, access servers account provider with auto account creation turned off. Manually created a new account,
        /// Expect a non null account given the correct username
        /// </summary>
        [TestMethod]
        public void StudioNetworkServerObservesMessagesFromCompileMessageRepo()
        {
            var server = new MockEsbHub();
            var message = new CompileMessageTO { ServiceID = Guid.NewGuid(), ServiceName = "Test Service", MessageType = CompileMessageType.ResourceSaved };

            //exe
            CompileMessageRepo.Instance.AddMessage(Guid.NewGuid(), new[] { message });

            var memo = (DesignValidationMemo)server.WriteEventProviderMemos[0];

            Assert.IsNotNull(memo);
        }

        [TestMethod]
        [TestCategory("StudioNetworkServerUnitTest")]
        [Description("Test for StudioNetworkServer's 'UpdateMappingChangedMemo' method: A valid memo and message is passed to StudioNetworkServer and the memo is updated with the message error")]
        [Owner("Ashley Lewis")]
        public void StudioNetworkServer_StudioNetworkServerUnitTest_UpdateMappingChangedMemo_MemoUpdated()
        {
            //init
            var server = new MockEsbHub();
            var message = new CompileMessageTO { UniqueID = Guid.NewGuid(), ServiceID = Guid.NewGuid(), WorkspaceID = Guid.NewGuid(), MessageType = CompileMessageType.MappingChange, MessagePayload = "Test Error Message", ServiceName = "Test Service" };

            //exe
            server.TestOnCompilerMessageReceived(new[] { message });

            //asserts
            Assert.AreEqual(2, server.WriteEventProviderMemos.Count);
            foreach(var memo1 in server.WriteEventProviderMemos)
            {
                var memo = (DesignValidationMemo)memo1;
                Assert.AreEqual(message.ServiceID, memo.ServiceID, "Memo service name not updated with compiler message service name");
                Assert.AreEqual(message.WorkspaceID, memo.WorkspaceID, "Memo workspace ID not updated");
                Assert.IsFalse(memo.IsValid, "Error memo not invalidated");
                Assert.AreEqual(1, memo.Errors.Count, "The wrong number of errors was added to the memo");
                Assert.AreEqual(message.MessagePayload, memo.Errors[0].FixData, "The wrong error fix data was added to the memo");
                Assert.AreEqual(message.UniqueID, memo.Errors[0].InstanceID, "The error instacen ID was added to the memo");
            }

            var serviceMemo = (DesignValidationMemo)server.WriteEventProviderMemos[0];
            var unqiueMemo = (DesignValidationMemo)server.WriteEventProviderMemos[1];
            Assert.AreEqual(message.UniqueID, unqiueMemo.InstanceID, "Memo ID not updated with compiler message unique ID");
            Assert.AreEqual(message.ServiceID, serviceMemo.InstanceID, "Memo ID not updated with compiler message service ID");
        }

        [TestMethod]
        [TestCategory("StudioNetworkServerUnitTest")]
        [Description("Test for StudioNetworkServer's 'UpdateResourceSavedMemo' method: A valid memo and message is passed to StudioNetworkServer and the memo is updated with the message error")]
        [Owner("Ashley Lewis")]
        public void StudioNetworkServer_StudioNetworkServerUnitTest_UpdateResourceSavedMemo_MemoUpdated()
        {
            //init
            var server = new MockEsbHub();
            var message = new CompileMessageTO { ServiceID = Guid.NewGuid(), WorkspaceID = Guid.NewGuid(), MessageType = CompileMessageType.ResourceSaved, ServiceName = "Test Service" };

            //exe
            server.TestOnCompilerMessageReceived(new[] { message });


            //asserts
            Assert.AreEqual(1, server.WriteEventProviderMemos.Count);

            var memo = (DesignValidationMemo)server.WriteEventProviderMemos[0];
            Assert.AreEqual(message.ServiceID, memo.InstanceID, "Memo ID not updated with compiler message service ID");
            Assert.AreEqual(message.ServiceID, memo.ServiceID, "Memo service name not updated with compiler message service name");
            Assert.AreEqual(message.WorkspaceID, memo.WorkspaceID, "Memo workspace ID not updated");
            Assert.IsTrue(memo.IsValid, "Resource saved with invalid memo");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("EsbHub_AddItemMessage")]
        public void EsbHub_AddItemMessage_ItemHasData_ItemAddedMessageIsPublished()
        {
            //------------Setup for test--------------------------
            var hub = new MockEsbHub();
            var mockClients = new Mock<IHubCallerConnectionContext>();
            hub.Clients = mockClients.Object;
            dynamic all = new ExpandoObject();
            bool messagePublished = false;
            string actualString = "";
            all.ItemAddedMessage = new Action<string>(serialisedItem =>
            {
                actualString = serialisedItem;
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
            var mockClients = new Mock<IHubCallerConnectionContext>();
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
        protected override void WriteEventProviderClientMessage(IMemo memo)
        {
            if(WriteEventProviderMemos == null)
            {
                WriteEventProviderMemos = new List<IMemo>();
            }
            WriteEventProviderMemos.Add(memo);
        }
    }
}
