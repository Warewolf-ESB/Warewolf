using System;
using System.Activities.Expressions;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Factories
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DsfActivityFactoryTests
    {
        [TestMethod]
        [TestCategory("DsfActivityFactory_CreateDsfActivity")]
        [Description("DsfActivityFactory must assign the resource and environment ID.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfActivityFactory_UnitTest_ResourceAndEnvironmentIDAssigned_Done()
        {
            var expectedResourceID = Guid.NewGuid();
            var expectedEnvironmentID = Guid.NewGuid();

            var activity = new DsfActivity();

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.ID).Returns(expectedEnvironmentID);

            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.ResourceType).Returns(ResourceType.Service);
            model.Setup(m => m.ID).Returns(expectedResourceID);
            model.Setup(m => m.Environment).Returns(environment.Object);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder("<root/>"));

            DsfActivityFactory.CreateDsfActivity(model.Object, activity, false);

            var actualResourceID = Guid.Parse(activity.ResourceID.Expression.ToString());
            var actualEnvironmentID = Guid.Parse(activity.EnvironmentID.Expression.ToString());

            Assert.AreEqual(expectedResourceID, actualResourceID, "DsfActivityFactory did not assign the resource ID.");
            Assert.AreEqual(expectedEnvironmentID, actualEnvironmentID, "DsfActivityFactory did not assign the environment ID.");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DsfActivityFactory_CreateDsfActivity")]
        public void DsfActivityFactory_CreateDsfActivity_NullWorkflowXamlServerResourceTypeWebService_TypeIsWebService()
        {
            //------------Setup for test--------------------------
            var expectedResourceID = Guid.NewGuid();
            var expectedEnvironmentID = Guid.NewGuid();

            var activity = new DsfActivity();

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.ID).Returns(expectedEnvironmentID);

            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.ResourceType).Returns(ResourceType.Service);
            model.Setup(m => m.ID).Returns(expectedResourceID);
            model.Setup(m => m.Environment).Returns(environment.Object);
            model.Setup(m => m.ServerResourceType).Returns("WebService");

            //------------Execute Test---------------------------
            DsfActivityFactory.CreateDsfActivity(model.Object, activity, false);

            //------------Assert Results-------------------------
            Assert.AreEqual("WebService", ((Literal<string>)(activity.Type.Expression)).Value);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DsfActivityFactory_CreateDsfActivity")]
        public void DsfActivityFactory_CreateDsfActivity_NullWorkflowXamlServerResourceTypeDbService_TypeIsDbService()
        {
            //------------Setup for test--------------------------
            var expectedResourceID = Guid.NewGuid();
            var expectedEnvironmentID = Guid.NewGuid();

            var activity = new DsfActivity();

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.ID).Returns(expectedEnvironmentID);

            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.ResourceType).Returns(ResourceType.Service);
            model.Setup(m => m.ID).Returns(expectedResourceID);
            model.Setup(m => m.Environment).Returns(environment.Object);
            model.Setup(m => m.ServerResourceType).Returns("DbService");

            //------------Execute Test---------------------------
            DsfActivityFactory.CreateDsfActivity(model.Object, activity, false);

            //------------Assert Results-------------------------
            Assert.AreEqual("DbService", ((Literal<string>)(activity.Type.Expression)).Value);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DsfActivityFactory_CreateDsfActivity")]
        public void DsfActivityFactory_CreateDsfActivity_NullWorkflowXamlServerResourceTypePluginService_TypeIsPluginService()
        {
            //------------Setup for test--------------------------
            var expectedResourceID = Guid.NewGuid();
            var expectedEnvironmentID = Guid.NewGuid();

            var activity = new DsfActivity();

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.ID).Returns(expectedEnvironmentID);

            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.ResourceType).Returns(ResourceType.Service);
            model.Setup(m => m.ID).Returns(expectedResourceID);
            model.Setup(m => m.Environment).Returns(environment.Object);
            model.Setup(m => m.ServerResourceType).Returns("PluginService");

            //------------Execute Test---------------------------
            DsfActivityFactory.CreateDsfActivity(model.Object, activity, false);

            //------------Assert Results-------------------------
            Assert.AreEqual("PluginService", ((Literal<string>)(activity.Type.Expression)).Value);
        }

        [TestMethod]
        [TestCategory("DsfActivityFactory_CreateDsfActivity")]
        [Description("DsfActivityFactory must not throw exception when a null source method is supplied.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfActivityFactory_UnitTest_ServiceActivityAndNullSourceMethod_DoesNotThrowException()
        {
            var activity = new DsfServiceActivity();
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.Service);
            mockRes.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResources.xmlNullSourceMethodServiceDef));
            DsfActivityFactory.CreateDsfActivity(mockRes.Object, activity, true);

            //If no exception - pass
            Assert.IsTrue(true);
        }
    }
}
