using BusinessDesignStudio.Unit.Tests.Unlimited.UnitTest.BusinessDesignStudio;
using Dev2;
using Dev2.Composition;
using Dev2.Core.Tests;
using Dev2.DynamicServices;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessDesignStudio.Unit.Tests
{
    /// <summary>
    /// Summary description for ResourceRepositoryTest
    /// </summary>
    [TestClass]
    public class ResourceRepositoryTest
    {

        #region Variables
        // Global variables
        Mock<IEnvironmentConnection> environmentConnection = new Mock<IEnvironmentConnection>();
        Mock<IEnvironmentModel> environmentModel = new Mock<IEnvironmentModel>();
        Mock<IStudioClientContext> dataChannel = new Mock<IStudioClientContext>();
        Mock<IResourceModel> resourceModel = new Mock<IResourceModel>();
        ResourceRepository repo;
        Mock<IResourceModel> model = new Mock<IResourceModel>();
        Mock<IMainViewModel> mv = new Mock<IMainViewModel>();
        Mock<IFrameworkSecurityContext> security = new Mock<IFrameworkSecurityContext>();

        #endregion Variables

        #region Constructor and TestContext

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion Constructor and TestContext

        #region Additional result attributes
        //Use TestInitialize to run code before running each result 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            resourceModel.Setup(res => res.ResourceName).Returns("Resource");
            resourceModel.Setup(res => res.DisplayName).Returns("My New Resource");
            resourceModel.Setup(res => res.ServiceDefinition).Returns("My new Resource service definition");

            dataChannel.Setup(channel => channel.ExecuteCommand("SomeString", It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("<x><text>Im Happy</text></x>").Verifiable();

            environmentModel.Setup(model => model.LoadResources()).Verifiable();
            environmentModel.Setup(model => model.DsfChannel).Returns(dataChannel.Object);

            environmentConnection.Setup(prop => prop.Address).Returns(new Uri("http://localhost:77/dsf"));
            environmentConnection.Setup(prop => prop.DataChannel).Returns(dataChannel.Object);
            environmentConnection.Setup(prop => prop.IsConnected).Returns(true);

            string[] roles = new string[2];

            security.Setup(c => c.Roles).Returns(roles);

            repo = new ResourceRepository(environmentModel.Object);
            repo.SecurityContext = security.Object;
        }

        // Use TestCleanup to run code after each result has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Load Tests

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void Load_CreateAndLoadResource_SingleResource_Expected_ResourceReturned()
        {
            //Arrange
            repo.Save(model.Object);
            repo.Load();
            int resources = repo.All().Count;
            //Assert
            Assert.IsTrue(resources.Equals(1));

        }

        /// <summary>
        /// Create resource with source type
        /// </summary>
        [TestMethod]
        public void Load_MultipleResourceLoad_SourceServiceType_Expected_AllResourcesReturned()
        {
            //Arrange
            model.Setup(c => c.ResourceType).Returns(ResourceType.Source);
            //Act
            repo.Save(resourceModel.Object);
            repo.Save(model.Object);
            repo.Load();
            //Assert
            Assert.IsTrue(repo.All().Count.Equals(2));
        }

        /// <summary>
        /// Create resource with human Interface service type
        /// </summary>
        [TestMethod]
        public void LoadMultipleResourceLoad_HumanInterfaceServiceType_Expected_AllResourcesReturned()
        {
            //Arrange
            model.Setup(c => c.ResourceType).Returns(ResourceType.HumanInterfaceProcess);
            //Act
            repo.Save(resourceModel.Object);
            repo.Save(model.Object);
            repo.Load();
            //Assert
            Assert.IsTrue(repo.All().Count.Equals(2));
        }

        /// <summary>
        /// Create resource with workflow service type
        /// </summary>
        [TestMethod]
        public void Load_MultipleResourceLoad_WorkflowServiceType_Expected_AllResourcesReturned()
        {
            //Arrange
            model.Setup(c => c.ResourceType).Returns(ResourceType.WorkflowService);
            //Act
            repo.Save(resourceModel.Object);
            repo.Save(model.Object);
            repo.Load();
            //Assert
            Assert.IsTrue(repo.All().Count.Equals(2));
        }

        // Test requires checking - maybe we need more stringent policy in code.
        //Creating reource repository with null environment connection
        //[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        //public void Load_CreateResourceNullEnvironmentConnection_Expected_InvalidOperationException() {

        //    //Act
        //    environmentConnection.Setup(prop => prop.IsConnected).Returns(false);
        //    repo.Save(resourceModel.Object);
        //    repo.Load();
        //}

        #endregion Load Tests

        #region Save Tests

        //Updating the resources if there were ready exist in resource repository

        [TestMethod]
        public void UpdateResource()
        {
            //Arrange
            model.Setup(c => c.ResourceName).Returns("TestName");
            repo.Save(model.Object);
            repo.Load();
            model.Setup(c => c.ResourceName).Returns("NewName");
            repo.Save(model.Object);
            //Assert
            ICollection<IResourceModel> set = repo.All();
            int cnt = set.Count;

            IResourceModel[] setArray = set.ToArray();
            Assert.IsTrue(cnt == 1 && setArray[0].ResourceName == "NewName");
        }

        //Create a resource with the same resource name
        [TestMethod]
        public void SameResourceNAme()
        {
            Mock<IResourceModel> model2 = new Mock<IResourceModel>();
            model.Setup(c => c.DisplayName).Returns("result");
            model.Setup(c => c.ResourceName).Returns("result");
            model2.Setup(c => c.DisplayName).Returns("result");
            model2.Setup(c => c.ResourceName).Returns("result");
            //Act
            repo.Save(model.Object);
            repo.Save(model2.Object);
            repo.Load();

            Assert.IsTrue(repo.All().Count.Equals(1));
        }

        #endregion Save Tests

        #region Find Tests


        #endregion Find Tests

        #region RemoveResource Tests

        [TestMethod]
        public void WorkFlowService_OnDelete_Expected_NotInRepository()
        {
            var servers = new List<string> { EnviromentRepositoryTest.Server1ID };
            var env = new EnviromentRepositoryTest().CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var myRepo = new ResourceRepository(env.Object);
            var myItem = new ResourceModel(env.Object);
            myRepo.Add(myItem);
            int expected = myRepo.All().Count;
            myRepo.Remove(myItem);
            Assert.AreEqual(myRepo.All().Count,expected-1);
            myRepo.Add(myItem);
            Assert.AreEqual(1, myRepo.All().Count);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void NonExistantWorkFlowService_OnDelete_Expected_ThrewException()
        {
            var servers = new List<string> { EnviromentRepositoryTest.Server1ID };
            var env = new EnviromentRepositoryTest().CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var myRepo = new ResourceRepository(env.Object);
            var myItem = new ResourceModel(env.Object);
            myRepo.Remove(myItem);
        }

        #endregion RemoveResource Tests

        #region Missing Environment Information Tests

        //Create resource repository without connected to any environment
        [TestMethod]
        public void CreateResourceEnvironmentConnectionNotConnected()
        {
            //Arrange
            environmentConnection.Setup(envConn => envConn.IsConnected).Returns(false);

            repo.Save(resourceModel.Object);
            try
            {
                repo.Load();
            }
            //Assert
            catch(InvalidOperationException iex)
            {
                Assert.AreEqual("No connected environment found to perform operation on.", iex.Message);
            }
        }
        //Create resource with no address to connet to any environment
        [TestMethod]
        public void CreateResourceNoAddressEnvironmentConnection()
        {
            //Arrange
            Mock<IEnvironmentConnection> environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(prop => prop.DataChannel).Returns(dataChannel.Object);
            environmentConnection.Setup(prop => prop.IsConnected).Returns(true);

            repo.Save(resourceModel.Object);
            repo.Load();
            int resources = repo.All().Count(res => res.ResourceName == "Resource");
            //Assert
            Assert.IsTrue(resources == 1);

        }
        //Create resource with no data channel connected to
        [TestMethod]
        public void CreateResourceNoDataChannelEnvironmentConnection()
        {
            //Arrange
            Mock<IEnvironmentConnection> environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(prop => prop.Address).Returns(new Uri("http://localhost:77/dsf"));
            environmentConnection.Setup(prop => prop.IsConnected).Returns(true);

            repo.Save(resourceModel.Object);
            repo.Load();
            int resources = repo.All().Count(res => res.ResourceName == "Resource");
            //Assert
            Assert.IsTrue(resources == 1);

        }

        #endregion Missing Environment Information Tests


        #region ReloadResource Tests

        ///// <summary>
        ///// Create resource with source type
        ///// </summary>
        //[TestMethod]
        //public void ReloadResource_Where_cake_Expected_morecake()
        //{
        //    //Arrange
        //    model.Setup(c => c.ResourceType).Returns(enResourceType.WorkflowService);
        //    //Act
        //    repo.Load();
        //    repo.ReloadResource("Resource", enResourceType.WorkflowService, ResourceModelEqualityComparer.Current);
        //    //Assert
        //    Assert.IsTrue(repo.All().Count.Equals(2));
        //}

        #endregion ReloadResource Tests

        #region FindResourcesByID

        [TestMethod]
        public void FindResourcesByID_With_NullParameters_Expected_ReturnsEmptyList()
        {
            var result = ResourceRepository.FindResourcesByID(null, null, ResourceType.Source);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FindResourcesByID_With_NonNullParameters_Expected_ReturnsNonEmptyList()
        {
            var servers = new List<string> { EnviromentRepositoryTest.Server1ID };
            var env = new EnviromentRepositoryTest().CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);

            var result = ResourceRepository.FindResourcesByID(env.Object, servers, ResourceType.Source);

            Assert.AreNotEqual(0, result.Count);
        }

        #endregion

        #region FindSourcesByType

        [TestMethod]
        public void FindSourcesByType_With_NullParameters_Expected_ReturnsEmptyList()
        {
            var result = ResourceRepository.FindSourcesByType(null, enSourceType.Dev2Server);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FindSourcesByType_With_NonNullParameters_Expected_ReturnsNonEmptyList()
        {
            var env = new EnviromentRepositoryTest().CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);

            var result = ResourceRepository.FindSourcesByType(env.Object, enSourceType.Dev2Server);

            Assert.AreNotEqual(0, result.Count);
        }

        #endregion

        #region AddEnvironment

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddEnvironment_With_NullParameters_Expected_ThrowsArgumentNullException()
        {
            ResourceRepository.AddEnvironment(null, null);
        }

        [TestMethod]
        public void AddEnvironment_With_NonNullParameters_Expected_InvokesExecuteCommandOnTargetEnvironment()
        {
            var testEnv = new EnviromentRepositoryTest().CreateTestEnvironmentWithSecurityContext();

            var dsfChannel = new Mock<IStudioClientContext>();
            dsfChannel.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            dsfChannel.Setup(c => c.AccountID).Returns(It.IsAny<Guid>());

            var targetEnv = new Mock<IEnvironmentModel>();
            targetEnv.Setup(e => e.DsfChannel).Returns(dsfChannel.Object);

            ResourceRepository.AddEnvironment(targetEnv.Object, testEnv);

            dsfChannel.Verify(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        #endregion

        #region RemoveEnvironment

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveEnvironment_With_NullParameters_Expected_ThrowsArgumentNullException()
        {
            ResourceRepository.RemoveEnvironment(null, null);
        }

        [TestMethod]
        public void RemoveEnvironment_With_NonNullParameters_Expected_InvokesExecuteCommandOnTargetEnvironment()
        {
            var testEnv = new EnviromentRepositoryTest().CreateTestEnvironmentWithSecurityContext();

            var dsfChannel = new Mock<IStudioClientContext>();
            dsfChannel.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            dsfChannel.Setup(c => c.AccountID).Returns(It.IsAny<Guid>());

            var targetEnv = new Mock<IEnvironmentModel>();
            targetEnv.Setup(e => e.DsfChannel).Returns(dsfChannel.Object);

            ResourceRepository.RemoveEnvironment(targetEnv.Object, testEnv);

            dsfChannel.Verify(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        #endregion

    }

}
