using System;
using System.Activities.Expressions;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Core;
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
        // ReSharper disable InconsistentNaming

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
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty);

            DsfActivityFactory.CreateDsfActivity(model.Object, activity, false, environmentRepository,false);

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
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty);

            //------------Execute Test---------------------------
            DsfActivityFactory.CreateDsfActivity(model.Object, activity, false, environmentRepository, false);

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
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty);

            //------------Execute Test---------------------------
            DsfActivityFactory.CreateDsfActivity(model.Object, activity, false, environmentRepository, false);

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
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty);

            //------------Execute Test---------------------------
            DsfActivityFactory.CreateDsfActivity(model.Object, activity, false, environmentRepository, false);

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
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty);
            DsfActivityFactory.CreateDsfActivity(mockRes.Object, activity, true, environmentRepository, false);

            //If no exception - pass
            Assert.IsTrue(true);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfActivityFactory_CreateDsfActivity")]
        public void DsfActivityFactory_CreateDsfActivity_WhenRemoteEnviromentIsActiveAndActivityFromRemoteEnvironment_ExpectEnviromentIDEmptyGuid()
        {
            //------------Setup for test--------------------------
            var expectedResourceID = Guid.NewGuid();
            var expectedEnvironmentID = Guid.NewGuid();

            var activity = new DsfActivity();

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.ID).Returns(expectedEnvironmentID); // Set the active environment

            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.ResourceType).Returns(ResourceType.Service);
            model.Setup(m => m.ID).Returns(expectedResourceID);
            model.Setup(m => m.Environment).Returns(environment.Object);
            model.Setup(m => m.ServerResourceType).Returns("Workflow");
            var environmentRepository = SetupEnvironmentRepo(expectedEnvironmentID);

            //------------Execute Test---------------------------
            DsfActivityFactory.CreateDsfActivity(model.Object, activity, false, environmentRepository, false);

            //------------Assert Results-------------------------
            StringAssert.Contains(((Literal<string>)(activity.Type.Expression)).Value, "Workflow");
            Assert.AreEqual(Guid.Empty.ToString(), activity.EnvironmentID.Expression.ToString());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfActivityFactory_CreateDsfActivity")]
        public void DsfActivityFactory_CreateDsfActivity_WhenLocalEnviromentIsActiveAndActivityFromRemoteEnvironment_ExpectRemoteEnviromentID()
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
            model.Setup(m => m.ServerResourceType).Returns("Workflow");
            var environmentRepository = SetupEnvironmentRepo(Guid.Empty); // Set the active environment

            //------------Execute Test---------------------------
            DsfActivityFactory.CreateDsfActivity(model.Object, activity, false, environmentRepository, true);

            //------------Assert Results-------------------------
            StringAssert.Contains(((Literal<string>)(activity.Type.Expression)).Value, "Workflow");
            Assert.AreEqual(expectedEnvironmentID.ToString(), activity.EnvironmentID.Expression.ToString());
        }

        static IEnvironmentRepository SetupEnvironmentRepo(Guid environmentId)
        {
            var mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(environmentId);
            return GetEnvironmentRepository(mockEnvironment);
        }

        private static IEnvironmentRepository GetEnvironmentRepository(Mock<IEnvironmentModel> mockEnvironment)
        {

            var repo = new TestLoadEnvironmentRespository(mockEnvironment.Object) { IsLoaded = true };
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(repo);
            // ReSharper restore ObjectCreationAsStatement
            repo.ActiveEnvironment = mockEnvironment.Object;

            return repo;
        }

        // ReSharper restore InconsistentNaming
    }
}
