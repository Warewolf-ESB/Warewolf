using System;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Composition;
using Dev2.Core.Tests;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.XML;
using Dev2.DynamicServices;
using Dev2.Network;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
        readonly Mock<IEnvironmentConnection> _environmentConnection = CreateEnvironmentConnection();
        readonly Mock<IEnvironmentModel> _environmentModel = ResourceModelTest.CreateMockEnvironment();
        readonly Mock<IStudioClientContext> _dataChannel = new Mock<IStudioClientContext>();
        readonly Mock<IResourceModel> _resourceModel = new Mock<IResourceModel>();
        ResourceRepository _repo;
        readonly Mock<IFrameworkSecurityContext> _securityContext = new Mock<IFrameworkSecurityContext>();
        private Guid _resourceGuid = Guid.NewGuid();
        private Guid _serverID = Guid.NewGuid();
        private Guid _workspaceID = Guid.NewGuid();

        #endregion Variables

        #region Additional result attributes
        //Use TestInitialize to run code before running each result 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            _resourceModel.Setup(res => res.ResourceName).Returns("Resource");
            _resourceModel.Setup(res => res.DisplayName).Returns("My New Resource");
            _resourceModel.Setup(res => res.ServiceDefinition).Returns("My new Resource service definition");
            _resourceModel.Setup(res => res.ID).Returns(_resourceGuid);
            _resourceModel.Setup(res => res.WorkflowXaml).Returns("OriginalXaml");

            _dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("<x><text>Im Happy</text></x>").Verifiable();
            _dataChannel.Setup(channel => channel.ServerID).Returns(_serverID);
            _dataChannel.Setup(channel => channel.WorkspaceID).Returns(_workspaceID);

            _securityContext.Setup(s => s.Roles).Returns(new string[2]);

            _environmentConnection.Setup(prop => prop.AppServerUri).Returns(new Uri("http://localhost:77/dsf"));
            _environmentConnection.Setup(prop => prop.DataChannel).Returns(_dataChannel.Object);
            _environmentConnection.Setup(prop => prop.IsConnected).Returns(true);
            _environmentConnection.Setup(c => c.SecurityContext).Returns(_securityContext.Object);
            var mock = new Mock<IStudioNetworkMessageAggregator>();
            _environmentConnection.Setup(connection => connection.MessageAggregator).Returns(mock.Object);

            _environmentModel.Setup(m => m.LoadResources()).Verifiable();
            _environmentModel.Setup(m => m.DsfChannel).Returns(_dataChannel.Object);
            _environmentModel.Setup(e => e.Connection).Returns(_environmentConnection.Object);
            //_environmentModel.Setup(e => e.IsConnected).Returns(true);
            //_environmentModel.Setup(e => e.ID).Returns(Guid.NewGuid());

            _repo = new ResourceRepository(_environmentModel.Object) { IsLoaded = true }; // Prevent clearing of internal list and call to connection!
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
            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);


            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.Load();
            int resources = _repo.All().Count;
            //Assert
            Assert.IsTrue(resources.Equals(1));


        }

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void ForceLoadSuccessfullLoadExpectIsLoadedTrue()
        {
            //Arrange
            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload></Payload>"));

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();

            Assert.IsTrue(_repo.IsLoaded);
        }

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void ForceLoadWithExceptionOnLoadExpectsIsLoadedFalse()
        {
            //Arrange
            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns("<Payload><Service Name=\"TestWorkflowService1\", <= Bad payload</Payload>");

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();

            //Assert
            Assert.IsFalse(_repo.IsLoaded);

        }

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void ForceLoadWith2WorkflowsExpectResourcesLoaded()
        {
            //Arrange
            var conn = SetupConnection();

            var guid1 = Guid.NewGuid().ToString();
            var guid2 = Guid.NewGuid().ToString();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\"></Service></Payload>", guid1, guid2));

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();
            int resources = _repo.All().Count;
            //Assert
            Assert.IsTrue(resources.Equals(2));
            var resource = _repo.All().First();

            Assert.IsTrue(resource.ResourceType == ResourceType.WorkflowService);
            Assert.IsTrue(resource.ResourceName == "TestWorkflowService1");
        }

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void LoadWorkflowExpectsFromCache()
        {
            //Arrange
            var conn = SetupConnection();

            var guid2 = Guid.NewGuid().ToString();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.ForceLoad();

            int resources = _repo.All().Count;

            guid2 = Guid.NewGuid().ToString();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
             .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>", _resourceGuid, guid2));

            _repo.ForceLoad();

            //Assert
            Assert.IsTrue(resources.Equals(2));
            var resource = _repo.All().First();
            var resource2 = _repo.All().Last();

            Assert.IsTrue(resource.WorkflowXaml == "ChangedDefinition");
            Assert.IsTrue(resource2.WorkflowXaml == "ChangedDefinition");
        }

        private Mock<IEnvironmentConnection> SetupConnection()
        {
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            _environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri)
                .Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri)
                .Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            return conn;
        }

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void ForceLoadWith2ReservedResourcesExpectsServicesAdded()
        {
            //Arrange
            var conn = SetupConnection();

            const string reserved1 = "TestName1";
            const string reserved2 = "TestName2";

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><ReservedName>{0}</ReservedName><ReservedName>{1}</ReservedName></Payload>", reserved1, reserved2));

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();

            Assert.IsTrue(_repo.IsReservedService(reserved1));
            Assert.IsTrue(_repo.IsReservedService(reserved2));
        }

        /// <summary>
        /// Create resource with source type
        /// </summary>
        [TestMethod]
        public void Load_MultipleResourceLoad_SourceServiceType_Expected_AllResourcesReturned()
        {
            //Arrange
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.ResourceType).Returns(ResourceType.Source);

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //Act
            _repo.Save(_resourceModel.Object);
            _repo.Save(model.Object);
            _repo.Load();
            //Assert
            Assert.IsTrue(_repo.All().Count.Equals(2));
        }

        /// <summary>
        /// Create resource with human Interface service type
        /// </summary>
        [TestMethod]
        public void UpdateResourcesExpectsWorkspacesLoadedBypassCache()
        {
            //Arrange
            new Mock<IResourceModel>().Setup(c => c.ResourceType).Returns(ResourceType.HumanInterfaceProcess);

            var conn = SetupConnection();


            var guid2 = Guid.NewGuid().ToString();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedXaml\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            //Act
            _repo.Save(_resourceModel.Object);
            _repo.Load();
            //Assert
            var resource = _repo.All().First();
            Assert.IsTrue(resource.WorkflowXaml.Equals("OriginalXaml"));

            var workspaceItemMock = new Mock<IWorkspaceItem>();
            workspaceItemMock.Setup(s => s.ServerID).Returns(_serverID);
            workspaceItemMock.Setup(s => s.WorkspaceID).Returns(_workspaceID);

            _repo.UpdateWorkspace(new List<IWorkspaceItem> { workspaceItemMock.Object });
            resource = _repo.All().First();
            Assert.IsTrue(resource.WorkflowXaml.Equals("ChangedXaml"));
        }

        /// <summary>
        /// Create resource with human Interface service type
        /// </summary>
        [TestMethod]
        public void LoadMultipleResourceLoad_HumanInterfaceServiceType_Expected_AllResourcesReturned()
        {
            //Arrange
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.ResourceType).Returns(ResourceType.HumanInterfaceProcess);

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //Act
            _repo.Save(_resourceModel.Object);
            _repo.Save(model.Object);
            _repo.Load();
            //Assert
            Assert.IsTrue(_repo.All().Count.Equals(2));
        }

        /// <summary>
        /// Create resource with workflow service type
        /// </summary>
        [TestMethod]
        public void Load_MultipleResourceLoad_WorkflowServiceType_Expected_AllResourcesReturned()
        {
            //Arrange
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.ResourceType).Returns(ResourceType.WorkflowService);

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //Act
            _repo.Save(_resourceModel.Object);
            _repo.Save(model.Object);
            _repo.Load();
            //Assert
            Assert.IsTrue(_repo.All().Count.Equals(2));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Load_CreateResourceNullEnvironmentConnection_Expected_InvalidOperationException()
        {
            _environmentConnection.Setup(prop => prop.IsConnected).Returns(false);
            _repo.Save(_resourceModel.Object);
        }

        #endregion Load Tests

        #region Save Tests

        //Updating the resources if there were ready exist in resource repository

        [TestMethod]
        public void UpdateResource()
        {
            //Arrange
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.ResourceName).Returns("TestName");

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(model.Object);
            _repo.Load();
            model.Setup(c => c.ResourceName).Returns("NewName");
            _repo.Save(model.Object);
            //Assert
            ICollection<IResourceModel> set = _repo.All();
            int cnt = set.Count;

            IResourceModel[] setArray = set.ToArray();
            Assert.IsTrue(cnt == 1 && setArray[0].ResourceName == "NewName");
        }

        //Create a resource with the same resource name
        [TestMethod]
        public void SameResourceName()
        {
            Mock<IResourceModel> model2 = new Mock<IResourceModel>();
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.DisplayName).Returns("result");
            model.Setup(c => c.ResourceName).Returns("result");
            model2.Setup(c => c.DisplayName).Returns("result");
            model2.Setup(c => c.ResourceName).Returns("result");

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);


            //Act
            _repo.Save(model.Object);
            _repo.Save(model2.Object);
            _repo.Load();

            Assert.IsTrue(_repo.All().Count.Equals(1));
        }

        #endregion Save Tests

        #region RemoveResource Tests

        [TestMethod]
        public void WorkFlowService_OnDelete_Expected_NotInRepository()
        {
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.SetupGet(x => x.Connection.SecurityContext).Returns(_securityContext.Object);
            mockEnvironmentModel.SetupGet(x => x.IsConnected).Returns(true);
            mockEnvironmentModel.SetupGet(x => x.Connection.ServerEvents).Returns(new EventPublisher());

            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.AppServerUri).Returns(new Uri(StringResources.Uri_WebServer));
            mockEnvironmentModel.Setup(environmentModel => environmentModel.DsfChannel).Returns(Dev2MockFactory.SetupIFrameworkDataChannel_EmptyReturn().Object);

            mockEnvironmentModel.Setup(model1 => model1.Connection.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");
            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var servers = new List<string> { EnviromentRepositoryTest.Server1ID };
            var myItem = new ResourceModel(mockEnvironmentModel.Object);
            myItem.ResourceName = "TestResource";
            mockEnvironmentModel.Object.ResourceRepository.Add(myItem);
            int exp = mockEnvironmentModel.Object.ResourceRepository.All().Count;

            mockEnvironmentModel.Object.ResourceRepository.Remove(myItem);
            Assert.AreEqual(exp - 1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
            mockEnvironmentModel.Object.ResourceRepository.Add(myItem);
            Assert.AreEqual(1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void NonExistantWorkFlowService_OnDelete_Expected_ThrewException()
        {
            var servers = new List<string> { EnviromentRepositoryTest.Server1ID };
            var env = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
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
            _environmentConnection.Setup(envConn => envConn.IsConnected).Returns(false);
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            _environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(false);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(_resourceModel.Object);
            try
            {
                _repo.Load();
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
            environmentConnection.Setup(prop => prop.DataChannel).Returns(_dataChannel.Object);
            environmentConnection.Setup(prop => prop.IsConnected).Returns(true);
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(_resourceModel.Object);
            _repo.Load();
            int resources = _repo.All().Count(res => res.ResourceName == "Resource");
            //Assert
            Assert.IsTrue(resources == 1);

        }
        //Create resource with no data channel connected to
        [TestMethod]
        public void CreateResourceNoDataChannelEnvironmentConnection()
        {
            //Arrange
            Mock<IEnvironmentConnection> environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(prop => prop.AppServerUri).Returns(new Uri("http://localhost:77/dsf"));
            environmentConnection.Setup(prop => prop.IsConnected).Returns(true);

            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(_resourceModel.Object);
            _repo.Load();
            int resources = _repo.All().Count(res => res.ResourceName == "Resource");
            //Assert
            Assert.IsTrue(resources == 1);

        }

        #endregion Missing Environment Information Tests

        #region ReloadResource Tests

        ///// <summary>
        ///// Create resource with source type
        ///// </summary>
        [TestMethod]
        public void ReloadResourcesWhereNothingLoadedExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            var reloadedResources = _repo.ReloadResource("TestWorkflowService", ResourceType.WorkflowService, new ResourceModelEqualityComparerForTest());
            //------------Assert Results-------------------------
            Assert.AreEqual(2, reloadedResources.Count);
        }


        [TestMethod]
        public void ResourceRepositoryReloadResourcesWithValidArgsExpectedSetsProperties()
        {
            //------------Setup for test--------------------------
            var conn = SetupConnection();
            var serverID = Guid.NewGuid();
            var version = new Version(3, 1, 0, 0);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\" ServerID=\"{1}\" IsValid=\"true\" Version=\"{2}\"><ErrorMessages>{3}</ErrorMessages></Service></Payload>", _resourceGuid, serverID, version,
                "<ErrorMessage Message=\"MappingChange\" ErrorType=\"Critical\" FixType=\"None\" StackTrace=\"SomethingWentWrong\" />"));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            var reloadedResources = _repo.ReloadResource("TestWorkflowService", ResourceType.WorkflowService, new ResourceModelEqualityComparerForTest());

            //------------Assert Results-------------------------
            Assert.AreEqual(1, reloadedResources.Count);
            var resources = _repo.All().ToList();
            var actual = (IContextualResourceModel)resources[0];
            Assert.AreEqual(_resourceGuid, actual.ID);
            Assert.AreEqual(serverID, actual.ServerID);
            Assert.AreEqual(version, actual.Version);
            Assert.AreEqual(true, actual.IsValid);

            Assert.IsNotNull(actual.Errors);
            Assert.AreEqual(1, actual.Errors.Count);
            var error = actual.Errors[0];
            Assert.AreEqual(ErrorType.Critical, error.ErrorType);
            Assert.AreEqual(FixType.None, error.FixType);
            Assert.AreEqual("MappingChange", error.Message);
            Assert.AreEqual("SomethingWentWrong", error.StackTrace);
        }

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
            var env = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);

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
            var env = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);

            var result = ResourceRepository.FindSourcesByType(env.Object, enSourceType.Dev2Server);

            Assert.AreNotEqual(0, result.Count);
        }

        #endregion

        #region Find

        [TestMethod]
        public void FindWithValidFunctionExpectResourceReturned()
        {
            //------------Setup for test--------------------------
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();
            //------------Execute Test---------------------------
            var resourceModels = _repo.Find(model => model.ID == newGuid);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, resourceModels.Count);
            Assert.AreEqual(newGuid, resourceModels.ToList()[0].ID);
        }

        [TestMethod]
        public void FindWithNullFunctionExpectNullReturned()
        {
            //------------Setup for test--------------------------
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();
            //------------Execute Test---------------------------
            var resourceModels = _repo.Find(null);
            //------------Assert Results-------------------------
            Assert.IsNull(resourceModels);
        }
        #endregion

        #region IsWorkflow
        [TestMethod]
        public void IsWorkflowValidWorkflowExpectTrue()
        {
            //------------Setup for test--------------------------
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();
            //------------Execute Test---------------------------
            var isWorkFlow = _repo.IsWorkflow("TestWorkflowService1");
            //------------Assert Results-------------------------
            Assert.IsTrue(isWorkFlow);
        }

        [TestMethod]
        public void IsWorkflowNotValidWorkflowExpectFalse()
        {
            //------------Setup for test--------------------------
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();
            //------------Execute Test---------------------------
            var isWorkFlow = _repo.IsWorkflow("TestWorkflowService");
            //------------Assert Results-------------------------
            Assert.IsFalse(isWorkFlow);
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
            var testEnv = EnviromentRepositoryTest.CreateMockEnvironment();

            var targetEnv = new Mock<IEnvironmentModel>();
            var rand = new Random();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            targetEnv.Setup(e => e.Connection).Returns(connection.Object);
            ResourceRepository.AddEnvironment(targetEnv.Object, testEnv.Object);

            connection.Verify(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        void DoesNothing()
        {
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
            var testEnv = EnviromentRepositoryTest.CreateMockEnvironment();

            var targetEnv = new Mock<IEnvironmentModel>();
            var rand = new Random();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            targetEnv.Setup(e => e.Connection).Returns(connection.Object);


            ResourceRepository.RemoveEnvironment(targetEnv.Object, testEnv.Object);

            connection.Verify(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        #endregion

        #region BuildUnlimitedPackage

        [TestMethod]
        public void BuildUnlimitedPackageWhereResourceExpectResourceDefinitionInPackage()
        {
            //------------Setup for test--------------------------
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.ResourceName).Returns("TestName");
            const string expectedValueForResourceDefinition = "This is the resource definition";
            model.Setup(c => c.ToServiceDefinition()).Returns(expectedValueForResourceDefinition);
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            _environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.Save(model.Object);
            //------------Execute Test---------------------------
            var buildUnlimitedPackage = _repo.BuildUnlimitedPackage(model.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(buildUnlimitedPackage.ResourceDefinition);
            StringAssert.Contains(buildUnlimitedPackage.ResourceDefinition, expectedValueForResourceDefinition);
        }

        [TestMethod]
        public void BuildUnlimitedPackageWhereResourceIsSourceExpectResourceDefinitionInPackage()
        {
            //------------Setup for test--------------------------
            const string ExpectedValueForResourceDefinition = "This is the resource definition";
            var resource = new ResourceModel(_environmentModel.Object);
            resource.ResourceType = ResourceType.Source;
            resource.ServiceDefinition = ExpectedValueForResourceDefinition;
            resource.ResourceName = "TestName";

            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            _environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            //------------Execute Test---------------------------
            var buildUnlimitedPackage = _repo.BuildUnlimitedPackage(resource);
            //------------Assert Results-------------------------
            Assert.IsNotNull(buildUnlimitedPackage.ResourceDefinition);
            StringAssert.Contains(buildUnlimitedPackage.ResourceDefinition, ExpectedValueForResourceDefinition);
        }

        [TestMethod]
        public void BuildUnlimitedPackageWhereResourceIsServiceExpectResourceDefinitionInPackage()
        {
            //------------Setup for test--------------------------
            const string ExpectedValueForResourceDefinition = "This is the resource definition";
            var resource = new ResourceModel(_environmentModel.Object);
            resource.ResourceType = ResourceType.Service;
            resource.ServiceDefinition = ExpectedValueForResourceDefinition;
            resource.ResourceName = "TestName";

            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            _environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            //------------Execute Test---------------------------
            var buildUnlimitedPackage = _repo.BuildUnlimitedPackage(resource);
            //------------Assert Results-------------------------
            Assert.IsNotNull(buildUnlimitedPackage.ResourceDefinition);
            StringAssert.Contains(buildUnlimitedPackage.ResourceDefinition, ExpectedValueForResourceDefinition);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildUnlimitedPackageWhereNullResourceExpectException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            _repo.BuildUnlimitedPackage(null);
            //------------Assert Results-------------------------
        }

        #endregion

        #region IsLoaded


        #endregion IsLoaded

        #region Constructor

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructWhereNullWizardEngineExpectArgumentNullException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            new ResourceRepository(_environmentModel.Object, null);
            //------------Assert Results-------------------------
            //See expected exception attribute
        }
        #endregion

        #region HydrateResourceTest

        [TestMethod]
        public void HydrateResourceHydratesConnectionString()
        {
            //------------Setup for test--------------------------
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format(TestResourceStringsTest.ResourcesToHydrate, _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, _repo.All().Count(r => r.ConnectionString == TestResourceStringsTest.ResourceToHydrateConnectionString1));
            Assert.AreEqual(1, _repo.All().Count(r => r.ConnectionString == TestResourceStringsTest.ResourceToHydrateConnectionString2));
        }

        [TestMethod]
        public void HydrateResourceHydratesResourceType()
        {
            //------------Setup for test--------------------------
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format(TestResourceStringsTest.ResourcesToHydrate, _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();
            var resources = _repo.All().Cast<IContextualResourceModel>();
            var servers = resources.Where(r => r.ServerResourceType == "Server");

            //------------Assert Results-------------------------
            Assert.AreEqual(2, servers.Count());

        }


        [TestMethod]
        [TestCategory("ResourceRepositoryUnitTest")]
        [Description("HydrateResourceModel must hydrate the resource's errors.")]
        [Owner("Trevor Williams-Ros")]
        public void ResourceRepositoryHydrateResourceModel_ResourceRepositoryUnitTest_ResourceErrors_Hydrated()
        {
            //------------Setup for test--------------------------
            var conn = SetupConnection();
            var resourceXml = XmlResource.Fetch("ResourceWithErrors").ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resourceXml);
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();
            var resources = _repo.All();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, resources.Count, "HydrateResourceModel failed to load the resource.");

            var resource = resources.First();
            Assert.IsFalse(resource.IsValid, "HydrateResourceModel failed to hydrate IsValid.");
            Assert.AreEqual(2, resource.Errors.Count);

            var err = resource.Errors.FirstOrDefault(e => e.InstanceID == Guid.Parse("edadb62e-83f4-44bf-a260-7639d6b43169"));
            Assert.IsNotNull(err, "Error not hydrated.");
            Assert.AreEqual(ErrorType.Critical, err.ErrorType, "HydrateResourceModel failed to hydrate the ErrorType.");
            Assert.AreEqual(FixType.ReloadMapping, err.FixType, "HydrateResourceModel failed to hydrate the FixType.");
            Assert.AreEqual("Mapping out of date", err.Message, "HydrateResourceModel failed to hydrate the Message.");
            Assert.AreEqual("", err.StackTrace, "HydrateResourceModel failed to hydrate the StackTrace.");
            Assert.AreEqual("<Args><Input>[{\"Name\":\"n1\",\"MapsTo\":\"\",\"Value\":\"\",\"IsRecordSet\":false,\"RecordSetName\":\"\",\"IsEvaluated\":false,\"DefaultValue\":\"\",\"IsRequired\":false,\"RawValue\":\"\",\"EmptyToNull\":false},{\"Name\":\"n2\",\"MapsTo\":\"\",\"Value\":\"\",\"IsRecordSet\":false,\"RecordSetName\":\"\",\"IsEvaluated\":false,\"DefaultValue\":\"\",\"IsRequired\":false,\"RawValue\":\"\",\"EmptyToNull\":false}]</Input><Output>[{\"Name\":\"result\",\"MapsTo\":\"\",\"Value\":\"\",\"IsRecordSet\":false,\"RecordSetName\":\"\",\"IsEvaluated\":false,\"DefaultValue\":\"\",\"IsRequired\":false,\"RawValue\":\"\",\"EmptyToNull\":false}]</Output></Args>", err.FixData, "HydrateResourceModel failed to hydrate the FixData.");
        }

        #endregion

        #region IsInCache

        [TestMethod]
        public void IsInCacheExpectsWhenResourceInCacheReturnsTrue()
        {
            //--------------------------Setup-------------------------------------------
            var conn = SetupConnection();
            var guid2 = Guid.NewGuid().ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            int resources = _repo.All().Count;
            var guid = Guid.NewGuid();
            guid2 = guid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
             .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            //--------------------------------------------Execute--------------------------------------------------------------
            var isInCache = _repo.IsInCache(guid);
            //--------------------------------------------Assert Results----------------------------------------------------
            Assert.IsTrue(isInCache);
        }

        [TestMethod]
        public void IsInCacheExpectsWhenResourceNotInCacheReturnsFalse()
        {
            //--------------------------Setup-------------------------------------------
            var conn = SetupConnection();
            var guid2 = Guid.NewGuid().ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            int resources = _repo.All().Count;
            var guid = Guid.NewGuid();
            guid2 = guid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
             .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            //--------------------------------------------Execute--------------------------------------------------------------
            var isInCache = _repo.IsInCache(Guid.NewGuid());
            //--------------------------------------------Assert Results----------------------------------------------------
            Assert.IsFalse(isInCache);
        }
        #endregion

        #region RemoveFromCache

        [TestMethod]
        public void RemoveFromCacheExpectsWhenResourceInCacheRemovesFromCache()
        {
            //--------------------------Setup-------------------------------------------
            var conn = SetupConnection();
            var guid2 = Guid.NewGuid().ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            int resources = _repo.All().Count;
            var guid = Guid.NewGuid();
            guid2 = guid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
             .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            //--------------------------------------------Assert Precondtion----------------------------------------------
            var isInCache = _repo.IsInCache(guid);
            Assert.IsTrue(isInCache);
            //--------------------------------------------Execute--------------------------------------------------------------
            _repo.RemoveFromCache(guid);
            //--------------------------------------------Assert Results----------------------------------------------------
            isInCache = _repo.IsInCache(guid);
            Assert.IsFalse(isInCache);
        }

        [TestMethod]
        public void RemoveFromCacheExpectsWhenResourceNotInCacheDoesNothing()
        {
            //--------------------------Setup-------------------------------------------
            var conn = SetupConnection();
            var guid2 = Guid.NewGuid().ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            int resources = _repo.All().Count;
            var guid = Guid.NewGuid();
            guid2 = guid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
             .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            //--------------------------------------------Assert Precondition-------------------------------------------
            var newGuid = Guid.NewGuid();
            var isInCache = _repo.IsInCache(newGuid);
            Assert.IsFalse(isInCache);
            //--------------------------------------------Execute--------------------------------------------------------------
            _repo.RemoveFromCache(newGuid);
            //--------------------------------------------Assert Results----------------------------------------------------
            isInCache = _repo.IsInCache(newGuid);
            Assert.IsFalse(isInCache);
        }

        #endregion

        #region DeployResource

        // BUG 9703 - 2013.06.21 - TWR - added

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceRepositoryDeployResourceWithNullExpectedThrowsArgumentNullException()
        {
            var repoConn = new Mock<IEnvironmentConnection>();
            repoConn.Setup(c => c.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);

            var repoEnv = new Mock<IEnvironmentModel>();
            repoEnv.Setup(e => e.Connection).Returns(repoConn.Object);

            var repo = new ResourceRepository(repoEnv.Object, new Mock<IWizardEngine>().Object);

            repo.DeployResource(null);
        }

        // BUG 9703 - 2013.06.21 - TWR - added
        [TestMethod]
        public void ResourceRepositoryDeployResourceWithNewResourceExpectedCreatesAndAddsNewResourceWithRepositoryEnvironment()
        {
            var repoConn = new Mock<IEnvironmentConnection>();
            repoConn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            repoConn.Setup(c => c.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);
            repoConn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("<XmlData></XmlData>");

            // DO NOT USE Mock EnvironmentModel's - otherwise EnvironmentModel.IEquatable will fail!
            var repoEnv = new EnvironmentModel(Guid.NewGuid(), repoConn.Object, new Mock<IWizardEngine>().Object, false);

            var resourceConn = new Mock<IEnvironmentConnection>();
            resourceConn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            var resourceEnv = new EnvironmentModel(Guid.NewGuid(), resourceConn.Object, new Mock<IWizardEngine>().Object, false);

            var newResource = new ResourceModel(resourceEnv)
            {
                ID = Guid.NewGuid(),
                Category = "Test",
                ResourceName = "TestResource"
            };

            repoEnv.ResourceRepository.DeployResource(newResource);

            var actual = repoEnv.ResourceRepository.FindSingle(r => r.ID == newResource.ID) as IContextualResourceModel;

            Assert.IsNotNull(actual);
            Assert.AreNotSame(newResource, actual);
            Assert.AreSame(repoEnv, actual.Environment);
        }

        // BUG 9703 - 2013.06.21 - TWR - added
        [TestMethod]
        public void ResourceRepositoryDeployResourceWithExistingResourceExpectedCreatesAndAddsNewResourceWithRepositoryEnvironment()
        {
            var repoConn = new Mock<IEnvironmentConnection>();
            repoConn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            repoConn.Setup(c => c.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);
            repoConn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("<XmlData></XmlData>");

            // DO NOT USE Mock EnvironmentModel's - otherwise EnvironmentModel.IEquatable will fail!
            var repoEnv = new EnvironmentModel(Guid.NewGuid(), repoConn.Object, new Mock<IWizardEngine>().Object, false);

            var resourceConn = new Mock<IEnvironmentConnection>();
            resourceConn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            var resourceEnv = new EnvironmentModel(Guid.NewGuid(), resourceConn.Object, new Mock<IWizardEngine>().Object, false);

            var oldResource = new ResourceModel(repoEnv)
            {
                ID = Guid.NewGuid(),
                Category = "Test",
                ResourceName = "TestResource"
            };
            repoEnv.ResourceRepository.Add(oldResource);

            var newResource = new ResourceModel(resourceEnv)
            {
                ID = oldResource.ID,
                Category = "Test",
                ResourceName = oldResource.ResourceName
            };

            repoEnv.ResourceRepository.DeployResource(newResource);

            var actual = repoEnv.ResourceRepository.FindSingle(r => r.ID == newResource.ID) as IContextualResourceModel;

            Assert.IsNotNull(actual);
            Assert.AreNotSame(newResource, actual);
            Assert.AreSame(repoEnv, actual.Environment);
        }

        #endregion

        #region Rename Category

        [TestMethod]
        [TestCategory("ResourceRepositoryUnitTest")]
        [Description("Test for ResourceRepository's rename category function")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ResourceRepository_ResourceRepositoryUnitTest_RenameCategory_ExecuteCommandIsCalledOnce()
        // ReSharper restore InconsistentNaming
        {
            //MEF!!!
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            mockEnvironment.Setup(c => c.Connection.SecurityContext);
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            mockEnvironment.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);
            var vm = new ResourceRepository(mockEnvironment.Object);
            vm.RenameCategory("Test Category", "New Test Category", ResourceType.WorkflowService);

            mockEnvironmentConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        #endregion


        static Mock<IEnvironmentConnection> CreateEnvironmentConnection()
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(new EventPublisher());
            return connection;
        }
    }

    public class ResourceModelEqualityComparerForTest : IEqualityComparer<IResourceModel>
    {

        public bool Equals(IResourceModel x, IResourceModel y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(IResourceModel obj)
        {
            return obj.GetHashCode();
        }
    }
}
