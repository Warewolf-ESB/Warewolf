using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Studio.Core;

namespace Dev2.Core.Tests {
    [TestClass]  
    public class MediatorTests {

        [TestCleanup]
        public void MediatorTestCleanup() {
            
        }

        int _testVal = 0;
        [TestMethod]
        public void RegisterToRecieve() {
            _testVal = 0;
            string result = Mediator.RegisterToReceiveMessage(MediatorMessages.UpdateIntelisense, param => Test(_testVal));

            Assert.IsTrue(!string.IsNullOrEmpty(result));
            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.UpdateIntelisense);
        }

        [TestMethod]
        public void RecieveAndSend() {
            _testVal = 0;
            string result = Mediator.RegisterToReceiveMessage(MediatorMessages.UpdateIntelisense, param => Test(_testVal));            
            Mediator.SendMessage(MediatorMessages.UpdateIntelisense, _testVal);

            Assert.IsTrue(_testVal == 1);
            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.UpdateIntelisense);
        }

        [TestMethod]
        public void DeRegister() {
            _testVal = 0;
            string result = Mediator.RegisterToReceiveMessage(MediatorMessages.UpdateIntelisense, param => Test(_testVal));
            Mediator.DeRegister(MediatorMessages.UpdateIntelisense, result);
            Mediator.SendMessage(MediatorMessages.UpdateIntelisense, _testVal);

            Assert.IsTrue(_testVal == 0);
        }

        [TestMethod]
        public void DerRegisterAllMessageOfType_Expected_AllMessageRemovedFromMap() {
            string result = Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, param => Test(_testVal));
            string result2 = Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, param => Test(_testVal));
            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddWorkflowDesigner);
            Mediator.SendMessage(MediatorMessages.AddWorkflowDesigner, _testVal);

            Assert.IsTrue(_testVal == 0);
        }


        void Test(int testVal) {
            testVal++;
            _testVal = testVal;
        }

    }


}
