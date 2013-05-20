using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dev2;
using Dev2.Composition;
using Dev2.Core.Tests;
using Dev2.Core.Tests.Environments;
using Dev2.DynamicServices;
using Dev2.Network;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels;
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
        readonly Mock<IEnvironmentConnection> _environmentConnection = new Mock<IEnvironmentConnection>();
        readonly Mock<IEnvironmentModel> _environmentModel = new Mock<IEnvironmentModel>();
        readonly Mock<IStudioClientContext> _dataChannel = new Mock<IStudioClientContext>();
        readonly Mock<IResourceModel> _resourceModel = new Mock<IResourceModel>();
        ResourceRepository _repo;
        readonly Mock<IResourceModel> _model = new Mock<IResourceModel>();
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


            _repo.Save(_model.Object);
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

            _repo.Save(_model.Object);
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

            _repo.Save(_model.Object);
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

            _repo.Save(_model.Object);
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
                .Returns(string.Format("<Payload><ReservedName>{0}</ReservedName><ReservedName>{1}</ReservedName></Payload>", reserved1,reserved2));

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(_model.Object);
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
            _model.Setup(c => c.ResourceType).Returns(ResourceType.Source);

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //Act
            _repo.Save(_resourceModel.Object);
            _repo.Save(_model.Object);
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
            _model.Setup(c => c.ResourceType).Returns(ResourceType.HumanInterfaceProcess);

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
            _model.Setup(c => c.ResourceType).Returns(ResourceType.HumanInterfaceProcess);

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //Act
            _repo.Save(_resourceModel.Object);
            _repo.Save(_model.Object);
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
            _model.Setup(c => c.ResourceType).Returns(ResourceType.WorkflowService);

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //Act
            _repo.Save(_resourceModel.Object);
            _repo.Save(_model.Object);
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
            _model.Setup(c => c.ResourceName).Returns("TestName");

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(_model.Object);
            _repo.Load();
            _model.Setup(c => c.ResourceName).Returns("NewName");
            _repo.Save(_model.Object);
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
            _model.Setup(c => c.DisplayName).Returns("result");
            _model.Setup(c => c.ResourceName).Returns("result");
            model2.Setup(c => c.DisplayName).Returns("result");
            model2.Setup(c => c.ResourceName).Returns("result");

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);


            //Act
            _repo.Save(_model.Object);
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
            var reloadedResources = _repo.ReloadResource("TestWorkflowService",ResourceType.WorkflowService,new ResourceModelEqualityComparerForTest());
            //------------Assert Results-------------------------
            Assert.AreEqual(2,reloadedResources.Count);
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
            Assert.AreEqual(newGuid,resourceModels.ToList()[0].ID);
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
            _model.Setup(c => c.ResourceName).Returns("TestName");
            const string expectedValueForResourceDefinition = "This is the resource definition";
            _model.Setup(c => c.ToServiceDefinition()).Returns(expectedValueForResourceDefinition);
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
            _repo.Save(_model.Object);
            //------------Execute Test---------------------------
            var buildUnlimitedPackage = _repo.BuildUnlimitedPackage(_model.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(buildUnlimitedPackage.ResourceDefinition);
            StringAssert.Contains(buildUnlimitedPackage.ResourceDefinition,expectedValueForResourceDefinition);
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
            _repo.Save(_model.Object);
            _repo.ForceLoad();

            //------------Assert Results-------------------------
            Assert.AreEqual(1,_repo.All().Count(r => r.ConnectionString == TestResourceStringsTest.ResourceToHydrateConnectionString1));
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
            _repo.Save(_model.Object);
            _repo.ForceLoad();
            var resources = _repo.All().Cast<IContextualResourceModel>();
            var servers = resources.Where(r => r.ServerResourceType == "Server");

            //------------Assert Results-------------------------
            Assert.AreEqual(2, servers.Count());
        
        }

        #endregion
    }
    public class ResourceModelEqualityComparerForTest:IEqualityComparer<IResourceModel>
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
