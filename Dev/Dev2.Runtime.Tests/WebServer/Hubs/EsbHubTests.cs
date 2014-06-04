using System;
using System.Collections.Generic;
using Dev2.Communication;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.WebServer.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.WebServer.Hubs
{
    [TestClass]
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
        // ReSharper disable InconsistentNaming
        public void StudioNetworkServer_StudioNetworkServerUnitTest_UpdateMappingChangedMemo_MemoUpdated()
        // ReSharper restore InconsistentNaming
        {
            //init
            var server = new MockEsbHub();
            var message = new CompileMessageTO { UniqueID = Guid.NewGuid(), ServiceID = Guid.NewGuid(), WorkspaceID = Guid.NewGuid(), MessageType = CompileMessageType.MappingChange, MessagePayload = "Test Error Message", ServiceName = "Test Service" };

            //exe
            server.TestOnCompilerMessageReceived(new[] { message });

            //asserts
            Assert.AreEqual(2, server.WriteEventProviderMemos.Count);
            foreach(DesignValidationMemo memo in server.WriteEventProviderMemos)
            {
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
        // ReSharper disable InconsistentNaming
        public void StudioNetworkServer_StudioNetworkServerUnitTest_UpdateResourceSavedMemo_MemoUpdated()
        // ReSharper restore InconsistentNaming
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

    }

    public class MockEsbHub:EsbHub
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
