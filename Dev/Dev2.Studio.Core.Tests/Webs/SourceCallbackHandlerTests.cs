using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.Services.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace Dev2.Core.Tests.Webs
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SourceCallbackHandlerTests
    {

        #region Class/TestInitialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            var workspace = new Mock<IWorkspaceItemRepository>();
            CustomContainer.Register<IWorkspaceItemRepository>(workspace.Object);
        }

        [TestInitialize]
        public void TestInitialize()
        {
        }

        #endregion

        #region Save

        [TestMethod]
        public void SourceCallbackHandlerSaveWithValidArgsExpectedPublishesUpdateResourceMessage()
        {
            const string ResourceName = "TestSource";
            Guid resourceId = Guid.NewGuid();

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.Setup(r => r.ResourceName).Returns(ResourceName);

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), true))
                        .Returns(new List<IResourceModel> { resourceModel.Object });

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.ResourceRepository).Returns(resourceRepo.Object);

            var aggregator = new Mock<IEventAggregator>();
            EventPublishers.Aggregator = aggregator.Object;
            var envRepo = new Mock<IEnvironmentRepository>();
            var handler = new SourceCallbackHandlerMock(aggregator.Object, envRepo.Object);

            aggregator.Setup(e => e.Publish(It.IsAny<UpdateResourceMessage>()))
                            .Callback<Object>(m =>
                            {
                                var msg = (UpdateResourceMessage)m;
                                Assert.AreEqual(ResourceName, msg.ResourceModel.ResourceName);
                            })
                             .Verifiable();

            var jsonObj = JObject.Parse("{ 'ResourceID': '" + resourceId + "'}");
            handler.TestSave(envModel.Object, jsonObj);

            aggregator.Verify(e => e.Publish(It.IsAny<UpdateResourceMessage>()), Times.Once());
        }

        #endregion



    }
}
