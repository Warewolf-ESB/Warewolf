using System;
using System.Collections.Generic;
using Dev2.Communication;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Explorer;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.WebServer.Hubs;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
            const string expectedString = "{\"DisplayName\":\"Testing\",\"ResourceId\":\"00000000-0000-0000-0000-000000000000\",\"ServerId\":\"d53bbcc5-4794-4dfa-b096-3aa815692e66\",\"WebserverUri\":\"http://localhost\",\"ResourceType\":0,\"Children\":[],\"Permissions\":0,\"ResourcePath\":\"Root\\\\Sub Folder\",\"Parent\":null}";
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
            Assert.AreEqual(expectedString, actualString);
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
        public void TestOnCompilerMessageReceived(IList<CompileMessageTO> messages)
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
