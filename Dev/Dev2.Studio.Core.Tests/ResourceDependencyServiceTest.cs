using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Composition;
using Dev2.Core.Tests.XML;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{

    [TestClass]
    public class ResourceDependencyServiceTest
    {
        #region Test Variables

        private IContextualResourceModel _testRequestModel = null;
        private XElement _testDependencyGraph;
        private ICollection<IResourceModel> _testExpectedResources;
        private Mock<IEnvironmentModel> _testEnvironmentModel;
        private Mock<IStudioClientContext> _dataChannel;

        //static Mock<IEnvironmentModel> _environmentModel;

        #endregion Test Variables

        #region TestInitialize

        [TestInitialize]
        public void TestInitialize()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            _testEnvironmentModel = ResourceModelTest.CreateMockEnvironment();

            _testDependencyGraph = XmlResource.Fetch("DependenciesGraphUniqueTest");
            _testExpectedResources = new IResourceModel[]
            {
                 new ResourceModel(_testEnvironmentModel.Object) { ResourceName = "Button" },
                 new ResourceModel(_testEnvironmentModel.Object) { ResourceName = "ReplacePartWithErrorMsg" },
                 new ResourceModel(_testEnvironmentModel.Object) { ResourceName = "Checkbox" },
                 new ResourceModel(_testEnvironmentModel.Object) { ResourceName = "Radio Button" }
            };

            _dataChannel = new Mock<IStudioClientContext>();
            _dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            _dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(_testDependencyGraph.ToString());

            //_testEnvironmentModel.Setup(e => e.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            //_testEnvironmentModel.Setup(e => e.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(_testDependencyGraph.ToString());
            _testEnvironmentModel.Setup(e => e.DsfChannel).Returns(_dataChannel.Object);
            _testEnvironmentModel.Setup(e => e.ResourceRepository.All()).Returns(_testExpectedResources);

            _testRequestModel = new ResourceModel(_testEnvironmentModel.Object) { ResourceName = "Button" };
        }

        #endregion

        #region GetDependanciesOnList Tests

        [TestMethod]
        public void GetDependanciesOnListWithNullEnvModel()
        {
            var service = new ResourceDependencyService();
            service.GetDependanciesOnList(new List<IContextualResourceModel>(), null);
            _testEnvironmentModel.Verify(e => e.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(0));
        }


        [TestMethod]
        public void GetDependanciesOnListWithNullModelReturnsEmptyList()
        {
            Mock<IStudioClientContext> _dataChannel2 = new Mock<IStudioClientContext>();
            _dataChannel2.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            _dataChannel2.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(StringResourcesTest.ResourceDependencyTestJsonReturn);

            Mock<IEnvironmentModel> _testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            _testEnvironmentModel2.Setup(e => e.DsfChannel).Returns(_dataChannel2.Object);
            _testEnvironmentModel2.Setup(e => e.ResourceRepository.All()).Returns(_testExpectedResources);

            var service = new ResourceDependencyService();
            List<IContextualResourceModel> resources = new List<IContextualResourceModel>();

            var result = service.GetDependanciesOnList(resources, _testEnvironmentModel2.Object);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetDependanciesOnListWithModel()
        {
            Mock<IStudioClientContext> _dataChannel2 = new Mock<IStudioClientContext>();
            _dataChannel2.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            _dataChannel2.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(StringResourcesTest.ResourceDependencyTestJsonReturn);

            Mock<IEnvironmentModel> _testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            _testEnvironmentModel2.Setup(e => e.DsfChannel).Returns(_dataChannel2.Object);
            _testEnvironmentModel2.Setup(e => e.ResourceRepository.All()).Returns(_testExpectedResources);

            var service = new ResourceDependencyService();
            List<IContextualResourceModel> resources = new List<IContextualResourceModel>();
            resources.Add(_testRequestModel);
            service.GetDependanciesOnList(resources, _testEnvironmentModel2.Object);
            _dataChannel2.Verify(e => e.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
        }

        #endregion GetDependanciesOnList Tests

        #region GetDependanciesAsXML Tests

        [TestMethod]
        public void GetDependenciesXmlWithNullModel()
        {
            var service = new ResourceDependencyService();
            service.GetDependenciesXml(null);
            _testEnvironmentModel.Verify(e => e.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(0));
        }


        [TestMethod]
        public void GetDependenciesXmlWithNullModelReturnsEmptyString()
        {
            var service = new ResourceDependencyService();
            var result = service.GetDependenciesXml(null);
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void GetDependenciesXmlWithModel()
        {
            var service = new ResourceDependencyService();
            service.GetDependenciesXml(_testRequestModel);
            _dataChannel.Verify(e => e.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
        }

        #endregion GetDependanciesAsXML Tests

        #region GetUniqueDependancies Tests

        [TestMethod]
        public void GetUniqueDependenciesWithNullModel()
        {
            var service = new ResourceDependencyService();
            service.GetUniqueDependencies(null);
            _testEnvironmentModel.Verify(e => e.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(0));
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
            var service = new ResourceDependencyService();
            service.GetUniqueDependencies(_testRequestModel);
            _dataChannel.Verify(e => e.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
        }

        [TestMethod]
        public void GetUniqueDependenciesWithModelReturnsUniqueItems()
        {
            var service = new ResourceDependencyService();
            var result = service.GetUniqueDependencies(_testRequestModel);
            Assert.IsTrue(result.SequenceEqual(_testExpectedResources));
        }

        [TestMethod]
        public void GetUniqueDependenciesWithModelReturnsEmptyListWhenNoResourcesMatch()
        {
            _testEnvironmentModel.Setup(e => e.ResourceRepository.All()).Returns(new IResourceModel[0]);
            var service = new ResourceDependencyService();
            var result = service.GetUniqueDependencies(_testRequestModel);
            Assert.AreEqual(0, result.Count);
        }

        #endregion GetUniqueDependancies Tests
    }
}
