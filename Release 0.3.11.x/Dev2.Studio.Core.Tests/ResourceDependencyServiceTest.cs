using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using Dev2.Composition;
using Dev2.Core.Tests.XML;
using Dev2.Providers.Events;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;

namespace Dev2.Core.Tests
{

    [TestClass][ExcludeFromCodeCoverage]
    public class ResourceDependencyServiceTest
    {
        static readonly XElement TestDependencyGraph = XmlResource.Fetch("DependenciesGraphUniqueTest");

        #region CreateResourceList

        static List<IContextualResourceModel> CreateResourceList(IEnvironmentModel environmentModel)
        {
            return new List<IContextualResourceModel>
            {
                new ResourceModel(environmentModel) { ResourceName = "Button" },
            };
        }

        #endregion

        #region GetDependanciesOnList Tests

        [TestMethod]
        public void GetDependanciesOnListWithNullEnvModel()
        {
            var service = new ResourceDependencyService();
            var result = service.GetDependanciesOnList(new List<IContextualResourceModel>(), null);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetDependanciesOnListWithNullModelReturnsEmptyList()
        {
            var dataChannel2 = new Mock<IStudioClientContext>();
            dataChannel2.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            dataChannel2.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(StringResourcesTest.ResourceDependencyTestJsonReturn);

            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            var testResources = new List<IResourceModel>(CreateResourceList(testEnvironmentModel2.Object));
            testEnvironmentModel2.Setup(e => e.ResourceRepository.All()).Returns(testResources);
            testEnvironmentModel2.Setup(e => e.DsfChannel).Returns(dataChannel2.Object);

            var service = new ResourceDependencyService();
            var resources = new List<IContextualResourceModel>();

            var result = service.GetDependanciesOnList(resources, testEnvironmentModel2.Object);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetDependanciesOnListWithModel()
        {
            var dataChannel2 = new Mock<IStudioClientContext>();
            dataChannel2.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            dataChannel2.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(StringResourcesTest.ResourceDependencyTestJsonReturn);

            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            var testResources = new List<IResourceModel>(CreateResourceList(testEnvironmentModel2.Object));
            testEnvironmentModel2.Setup(e => e.ResourceRepository.All()).Returns(testResources);
            testEnvironmentModel2.Setup(e => e.DsfChannel).Returns(dataChannel2.Object);

            var service = new ResourceDependencyService();
            var resources = new List<IContextualResourceModel> { new ResourceModel(testEnvironmentModel2.Object) { ResourceName = "Button" } };

            service.GetDependanciesOnList(resources, testEnvironmentModel2.Object);
            dataChannel2.Verify(e => e.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
        }

        #endregion GetDependanciesOnList Tests

        #region GetDependanciesAsXML Tests

        [TestMethod]
        public void GetDependenciesXmlWithNullModelReturnsEmptyString()
        {
            var service = new ResourceDependencyService();
            var result = service.GetDependenciesXml(null);
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void GetDependenciesXmlWithModel()
        {
            var environmentModel = new Mock<IEnvironmentModel>();

            var resources = new List<IResourceModel>(CreateResourceList(environmentModel.Object));

            var dataChannel = new Mock<IStudioClientContext>();
            dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(TestDependencyGraph.ToString());

            environmentModel.Setup(e => e.DsfChannel).Returns(dataChannel.Object);
            environmentModel.Setup(e => e.ResourceRepository.All()).Returns(resources);

            var service = new ResourceDependencyService();
            service.GetDependenciesXml(new ResourceModel(environmentModel.Object) { ResourceName = "Button" });
            dataChannel.Verify(e => e.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
        }

        #endregion GetDependanciesAsXML Tests

        #region GetUniqueDependancies Tests

        [TestMethod]
        public void GetUniqueDependenciesWithNullModel()
        {
            var environmentModel = new Mock<IEnvironmentModel>();

            var resources = new List<IResourceModel>();

            var dataChannel = new Mock<IStudioClientContext>();
            dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(TestDependencyGraph.ToString());

            environmentModel.Setup(e => e.DsfChannel).Returns(dataChannel.Object);
            environmentModel.Setup(e => e.ResourceRepository.All()).Returns(resources);

            var service = new ResourceDependencyService();
            service.GetUniqueDependencies(null);
            dataChannel.Verify(e => e.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(0));
        }

        [TestMethod]
        public void GetUniqueDependenciesWithNullModelReturnsEmptyList()
        {
            var service = new ResourceDependencyService();
            var result = service.GetUniqueDependencies(null);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetUniqueDependenciesWithModel()
        {
            var environmentModel = new Mock<IEnvironmentModel>();

            var resources = new List<IResourceModel>(CreateResourceList(environmentModel.Object));

            var dataChannel = new Mock<IStudioClientContext>();
            dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(TestDependencyGraph.ToString());

            environmentModel.Setup(e => e.DsfChannel).Returns(dataChannel.Object);
            environmentModel.Setup(e => e.ResourceRepository.All()).Returns(resources);

            var service = new ResourceDependencyService();
            service.GetUniqueDependencies(new ResourceModel(environmentModel.Object) { ResourceName = "Button" });
            dataChannel.Verify(e => e.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
        }

        [TestMethod]
        public void GetUniqueDependenciesWithModelReturnsUniqueItems()
        {
            var environmentModel = new Mock<IEnvironmentModel>();

            var testExpectedResources = CreateResourceList(environmentModel.Object);
            var resources = new List<IResourceModel>(testExpectedResources);

            var dataChannel = new Mock<IStudioClientContext>();
            dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(TestDependencyGraph.ToString());

            environmentModel.Setup(e => e.DsfChannel).Returns(dataChannel.Object);
            environmentModel.Setup(e => e.ResourceRepository.All()).Returns(resources);

            var service = new ResourceDependencyService();
            var result = service.GetUniqueDependencies(new ResourceModel(environmentModel.Object) { ResourceName = "Button" });
            Assert.IsTrue(result.SequenceEqual(testExpectedResources));
        }

        [TestMethod]
        public void GetUniqueDependenciesWithModelReturnsEmptyListWhenNoResourcesMatch()
        {
            var environmentModel = new Mock<IEnvironmentModel>();

            var dataChannel = new Mock<IStudioClientContext>();
            dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(TestDependencyGraph.ToString());

            environmentModel.Setup(e => e.DsfChannel).Returns(dataChannel.Object);
            environmentModel.Setup(e => e.ResourceRepository.All()).Returns(new IResourceModel[0]);

            var service = new ResourceDependencyService();
            var result = service.GetUniqueDependencies(new ResourceModel(environmentModel.Object) { ResourceName = "Button" });
            Assert.AreEqual(0, result.Count);
        }

        #endregion GetUniqueDependancies Tests
    }
}
