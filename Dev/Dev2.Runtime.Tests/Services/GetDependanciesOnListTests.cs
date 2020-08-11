/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Runtime.Interfaces;
using Dev2.Studio.Interfaces;
using Warewolf.Trigger.Queue;
using Warewolf.Triggers;
using Warewolf.UnitTestAttributes;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetDependanciesOnListTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetDependanciesOnList))]
        public void GetDependanciesOnList_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var getDependanciesOnList = new GetDependanciesOnList();

            //------------Execute Test---------------------------
            var resId = getDependanciesOnList.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetDependanciesOnList))]
        public void GetDependanciesOnList_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var getDependanciesOnList = new GetDependanciesOnList();
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("GetDependanciesOnList", getDependanciesOnList.HandlesType());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetDependanciesOnList))]
        public void GetDependanciesOnList_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var getDependanciesOnList = new GetDependanciesOnList();
            //------------Execute Test---------------------------
            var dynamicService = getDependanciesOnList.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetDependanciesOnList))]
        public void GetDependanciesOnList_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var getDependanciesOnList = new GetDependanciesOnList();
            //------------Execute Test---------------------------
            var resId = getDependanciesOnList.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

      /*  [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetDependanciesOnList))]
        public void GetDependanciesOnList_Execute_ExpectList()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var resourceId = Guid.NewGuid();
            var resourceIDs = new List<Guid>() {resourceId};

            var requestArgs = new Dictionary<string, StringBuilder>();
            requestArgs.Add("ResourceIds", serializer.SerializeToBuilder(resourceIDs.Select(a => a.ToString()).ToList()));
            requestArgs.Add("GetDependsOnMe", new StringBuilder("false"));

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

            var getDependanciesOnList = new GetDependanciesOnList {TriggersCatalog = repo.Object};
            //------------Execute Test---------------------------

            var executeResults = getDependanciesOnList.Execute(requestArgs, ws.Object);
            var msg = serializer.Deserialize<CompressedExecuteMessage>(executeResults);
            Assert.IsNotNull(msg);
            var testModels = serializer.Deserialize<List<IServiceTestModelTO>>(msg.GetDecompressedMessage());
            //------------Assert Results-------------------------
            repo.Verify(a => a.Fetch(It.IsAny<Guid>()));
            Assert.AreEqual(listOfTests[0].TestName, testModels[0].TestName);
        }*/
    }
}