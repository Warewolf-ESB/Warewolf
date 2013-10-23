using System;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Factories
{
    [TestClass][ExcludeFromCodeCoverage]
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
            model.Setup(m => m.ServiceDefinition).Returns("<root/>");

            DsfActivityFactory.CreateDsfActivity(model.Object, activity, false);

            var actualResourceID = Guid.Parse(activity.ResourceID.Expression.ToString());
            var actualEnvironmentID = Guid.Parse(activity.EnvironmentID.Expression.ToString());

            Assert.AreEqual(expectedResourceID, actualResourceID, "DsfActivityFactory did not assign the resource ID.");
            Assert.AreEqual(expectedEnvironmentID, actualEnvironmentID, "DsfActivityFactory did not assign the environment ID.");
        }

        [TestMethod]
        [TestCategory("DsfActivityFactory_CreateDsfActivity")]
        [Description("DsfActivityFactory must not throw exception when a null source method is supplied.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfActivityFactory_UnitTest_ServiceActivityAndNullSourceMethod_DoesNotThrowException()
        {
            var activity = new DsfServiceActivity();
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.Service);
            mockRes.Setup(r => r.ServiceDefinition).Returns(StringResources.xmlNullSourceMethodServiceDef);
            DsfActivity act = DsfActivityFactory.CreateDsfActivity(mockRes.Object, activity, true);

            //If no exception - pass
            Assert.IsTrue(true);
        }
    }
}
