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
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Triggers;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Triggers;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Trigger;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchTriggerQueuesTest
    {

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var fetchTriggerQueues = new FetchTriggerQueues();

            //------------Execute Test---------------------------
            var resId = fetchTriggerQueues.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetAuthorizationContextForService")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var fetchTriggerQueues = new FetchTriggerQueues();

            //------------Execute Test---------------------------
            var resId = fetchTriggerQueues.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FetchTriggerQueues_HandlesType")]
        public void FetchTriggerQueues_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var fetchTriggerQueues = new FetchTriggerQueues();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("FetchTriggerQueues", fetchTriggerQueues.HandlesType());
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FetchTriggerQueues_Execute")]
        public void FetchTriggerQueues_Execute_ExpectTestList()
        {
            //------------Setup for test--------------------------
            var fetchTriggerQueues = new FetchTriggerQueues();
           
            var triggerQueue1 = new TriggerQueue();
            triggerQueue1.WorkflowName = "My WF";
            var triggerQueue2 = new TriggerQueue();
            triggerQueue2.TriggerId = Guid.NewGuid();
            var triggerQueue3 = new TriggerQueue();
            triggerQueue3.QueueName = "My Queue Name";
            var listOfTriggerQueues = new List<ITriggerQueue>
            {
                triggerQueue1,triggerQueue2,triggerQueue3
            };
            var repo = new Mock<ITriggersCatalog>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.Queues).Returns(listOfTriggerQueues).Verifiable();

            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceID = Guid.NewGuid();
            inputs.Add("resourceID", new StringBuilder(resourceID.ToString()));
            fetchTriggerQueues.TriggersCatalog = repo.Object;
            //------------Execute Test---------------------------
            var res = fetchTriggerQueues.Execute(inputs, ws.Object);
            var msg = serializer.Deserialize<CompressedExecuteMessage>(res);
            var triggerQueues = serializer.Deserialize<List<ITriggerQueue>>(msg.GetDecompressedMessage());
            //------------Assert Results-------------------------
            repo.Verify(a => a.Queues);
            Assert.AreEqual(listOfTriggerQueues.Count, triggerQueues.Count);
            Assert.AreEqual(listOfTriggerQueues[0].WorkflowName, triggerQueues[0].WorkflowName);
            Assert.AreEqual(listOfTriggerQueues[1].TriggerId, triggerQueues[1].TriggerId);
            Assert.AreEqual(listOfTriggerQueues[2].QueueName, triggerQueues[2].QueueName);
        }
    }
}