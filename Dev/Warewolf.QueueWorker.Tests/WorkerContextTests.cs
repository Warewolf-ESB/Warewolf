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
using Dev2.Common.Interfaces.Triggers;
using Dev2.Data.ServiceModel;
using Dev2.Triggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueueWorker;
using System;
using Warewolf.Common;

namespace Warewolf.QueueWorker.Tests
{
    [TestClass]
    public class WorkerContextTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(QueueWorker))]
        public void WorkerContext_GivenSource_ExpectHostname()
        {
            var mockArgs = new Mock<IArgs>();
            var mockResourceCatalogProxy = new Mock<IResourceCatalogProxy>();

            var rabbitSource = new RabbitMQSource
            {
                HostName = "somehost",
                ResourceName = "my somehost resource"
            };
            mockResourceCatalogProxy.Setup(o => o.GetResourceById<RabbitMQSource>(GlobalConstants.ServerWorkspaceID, It.IsAny<Guid>())).Returns(rabbitSource);
            var mockTriggerCatalog = new Mock<ITriggersCatalog>();

            var mockTriggerQueue = new Mock<ITriggerQueue>();
            mockTriggerCatalog.Setup(o => o.LoadQueueTriggerFromFile(It.IsAny<string>())).Returns(mockTriggerQueue.Object);
            IWorkerContext context = new WorkerContext(mockArgs.Object, mockResourceCatalogProxy.Object, mockTriggerCatalog.Object);

            Assert.AreEqual(rabbitSource, context.Source);
        }
    }
}
