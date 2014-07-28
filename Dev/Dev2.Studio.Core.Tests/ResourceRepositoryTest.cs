using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Communication;
using Dev2.Composition;
using Dev2.Controller;
using Dev2.Core.Tests;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.XML;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using ResourceType = Dev2.Studio.Core.AppResources.Enums.ResourceType;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace BusinessDesignStudio.Unit.Tests
{
    /// <summary>
    /// Summary description for ResourceRepositoryTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ResourceRepositoryTest
    {

        #region Variables

        readonly Mock<IAuthorizationService> _authService = new Mock<IAuthorizationService>();

        // Global variables
        readonly Mock<IEnvironmentConnection> _environmentConnection = CreateEnvironmentConnection();
        readonly Mock<IEnvironmentModel> _environmentModel = ResourceModelTest.CreateMockEnvironment();
        //readonly Mock<IStudioClientContext> _dataChannel = new Mock<IStudioClientContext>();
        readonly Mock<IResourceModel> _resourceModel = new Mock<IResourceModel>();
        ResourceRepository _repo;
        private readonly Guid _resourceGuid = Guid.NewGuid();
        private readonly Guid _serverID = Guid.NewGuid();
        private readonly Guid _workspaceID = Guid.NewGuid();

        #endregion Variables

        #region Additional result attributes
        //Use TestInitialize to run code before running each result 
        [TestInitialize]
        public void MyTestInitialize()
        {
            Setup();
        }

        void Setup()
        {
            _authService.Setup(s => s.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);

            _resourceModel.Setup(res => res.ResourceName).Returns("Resource");
            _resourceModel.Setup(res => res.DisplayName).Returns("My New Resource");
            _resourceModel.Setup(model => model.Category).Returns("MyFolder");
            _resourceModel.Setup(res => res.ID).Returns(_resourceGuid);
            _resourceModel.Setup(res => res.WorkflowXaml).Returns(new StringBuilder("OriginalXaml"));

            _environmentConnection.Setup(channel => channel.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder("<x><text>Im Happy</text></x>")).Verifiable();
            _environmentConnection.Setup(channel => channel.ServerID).Returns(_serverID);
            _environmentConnection.Setup(channel => channel.WorkspaceID).Returns(_workspaceID);


            _environmentConnection.Setup(prop => prop.AppServerUri).Returns(new Uri("http://localhost:77/dsf"));
            _environmentConnection.Setup(prop => prop.IsConnected).Returns(true);

            _environmentModel.Setup(m => m.LoadResources()).Verifiable();
            _environmentModel.Setup(e => e.Connection).Returns(_environmentConnection.Object);
            _environmentModel.Setup(e => e.AuthorizationService).Returns(_authService.Object);

            _repo = new ResourceRepository(_environmentModel.Object) { IsLoaded = true }; // Prevent clearing of internal list and call to connection!
        }

        #endregion

        #region Hydrate Resource Model

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsWorkflow_InputAndOutputMappingsAreValid()
        {
            const string inputData = "inputs";
            const string outputData = "outputs";
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            var resourceData = BuildSerializableResourceFromName("TestWF", Dev2.Data.ServiceModel.ResourceType.DbService);
            resourceData.Inputs = inputData;
            resourceData.Outputs = outputData;

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, resourceData, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(inputData, model.Inputs);
            Assert.AreEqual(outputData, model.Outputs);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsDbService_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            var resourceData = BuildSerializableResourceFromName("TestWF", Dev2.Data.ServiceModel.ResourceType.DbService);

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, resourceData, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/DatabaseService-32.png");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsDbSource_IconPathIsValid()
        {

            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            var resourceData = BuildSerializableResourceFromName("TestWF", Dev2.Data.ServiceModel.ResourceType.DbSource);

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, resourceData, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/DatabaseService-32.png");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsEmailSource_IconPathIsValid()
        {

            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            var resourceData = BuildSerializableResourceFromName("TestWF", Dev2.Data.ServiceModel.ResourceType.EmailSource);

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, resourceData, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/ToolSendEmail-32.png");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsPluginSource_IconPathIsValid()
        {

            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            var resourceData = BuildSerializableResourceFromName("TestWF", Dev2.Data.ServiceModel.ResourceType.PluginSource);

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, resourceData, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/PluginService-32.png");

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsWebService_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            var resourceData = BuildSerializableResourceFromName("TestWF", Dev2.Data.ServiceModel.ResourceType.WebService);

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, resourceData, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/WebService-32.png");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsWebSource_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            var resourceData = BuildSerializableResourceFromName("TestWF", Dev2.Data.ServiceModel.ResourceType.WebSource);

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, resourceData, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/WebService-32.png");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsWorkflowService_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            var resourceData = BuildSerializableResourceFromName("TestWF", Dev2.Data.ServiceModel.ResourceType.WorkflowService);

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, resourceData, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsServer_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            var resourceData = BuildSerializableResourceFromName("TestWF", Dev2.Data.ServiceModel.ResourceType.Server);

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, resourceData, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/ExplorerWarewolfConnection-32.png");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_WhenDataIsNewWorkflow_NewWorkFlowNamesUpdated()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            var resourceData = BuildSerializableResourceFromName("Unsaved 1", Dev2.Data.ServiceModel.ResourceType.WorkflowService, true);

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, resourceData, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsTrue(NewWorkflowNames.Instance.Contains("Unsaved 1"));
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_Permissions_Hydrated()
        {
            //------------Setup for test--------------------------
            const Permissions TestPermissions = Permissions.Contribute | Permissions.Execute;

            var resourceRepository = GetResourceRepository(TestPermissions);
            var resourceData = BuildSerializableResourceFromName("TestWF", Dev2.Data.ServiceModel.ResourceType.Server);

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, resourceData, Guid.Empty);

            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.UserPermissions, TestPermissions);
        }

        #endregion

        #region Load Tests

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void Load_CreateAndLoadResource_SingleResource_Expected_ResourceReturned()
        {
            //Arrange
            Setup();
            var conn = SetupConnection();

            var resourceData = BuildResourceObjectFromGuids(new[] { _resourceGuid }, Dev2.Data.ServiceModel.ResourceType.Server);

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if(callCnt == 0)
                    {
                        callCnt = 1;
                        return new StringBuilder(payload);
                    }

                    return resourceData;
                });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;


            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            _repo.Save(resourceModel.Object);
            _repo.Load();
            int resources = _repo.All().Count;
            //Assert
            Assert.IsTrue(resources.Equals(1));

            mockStudioResourceRepository.Verify(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()), Times.Once());
            _repo.GetStudioResourceRepository = null;
        }

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void ForceLoadSuccessfullLoadExpectIsLoadedTrue()
        {
            //Arrange
            Setup();

            AppSettings.LocalHost = "https://localhost:3242/";
            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);

            var conn = SetupConnection();

            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(() =>
                    {
                        if(callCnt == 0)
                        {
                            callCnt = 1;
                            return new StringBuilder(payload);
                        }

                        return BuildResourceObjectFromGuids(new[] { new Guid() });
                    });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            _repo.Save(resourceModel.Object);
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
            Setup();
            var conn = SetupConnection();

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if(callCnt == 0)
                    {
                        callCnt = 1;
                        return new StringBuilder(payload);
                    }

                    return new StringBuilder("<Payload><Service Name=\"TestWorkflowService1\", <= Bad payload</Payload>");
                });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            _repo.Save(resourceModel.Object);
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
            Setup();
            var conn = SetupConnection();

            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            var resultObj = BuildResourceObjectFromGuids(new[] { guid1, guid2 });

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if(callCnt == 0)
                    {
                        callCnt = 1;
                        return new StringBuilder(payload);
                    }

                    return resultObj;
                });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            _repo.Save(resourceModel.Object);
            _repo.ForceLoad();
            int resources = _repo.All().Count;
            //Assert
            Assert.IsTrue(resources.Equals(2));
            var resource = _repo.All().First();

            Assert.IsTrue(resource.ResourceType == ResourceType.WorkflowService);
            Assert.IsTrue(resource.ResourceName == "TestWorkflowService");
        }

        private Mock<IEnvironmentConnection> SetupConnection()
        {
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
            Setup();
            var conn = SetupConnection();

            const string Reserved1 = "TestName1";
            const string Reserved2 = "TestName2";

            var resourceData = BuildResourceObjectFromNames(new[] { Reserved1, Reserved2 }, Dev2.Data.ServiceModel.ResourceType.ReservedService);

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if(callCnt == 0)
                    {
                        callCnt = 1;
                        return new StringBuilder(payload);
                    }

                    return new StringBuilder(resourceData);
                });


            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            _repo.Save(resourceModel.Object);
            _repo.ForceLoad();

            Assert.IsTrue(_repo.IsReservedService(Reserved1));
            Assert.IsTrue(_repo.IsReservedService(Reserved2));
        }

        /// <summary>
        /// Create resource with source type
        /// </summary>
        [TestMethod]
        public void Load_MultipleResourceLoad_SourceServiceType_Expected_AllResourcesReturned()
        {
            //Arrange
            Setup();
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.ResourceType).Returns(ResourceType.Source);
            //model.SetupGet(p => p.ResourceName).Returns("My WF");
            //model.SetupGet(p => p.Category).Returns("Root");
            var conn = SetupConnection();

            var resourceData = BuildResourceObjectFromGuids(new[] { _resourceGuid }, Dev2.Data.ServiceModel.ResourceType.Server);
            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if(callCnt <= 1)
                    {
                        callCnt++;
                        return new StringBuilder(payload);
                    }

                    return resourceData;
                });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            //Act
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            _repo.Save(resourceModel.Object);
            _repo.Save(model.Object);
            _repo.Load();
            //Assert
            Assert.IsTrue(_repo.All().Count.Equals(2));

            mockStudioResourceRepository.Verify(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()), Times.Once());
            _repo.GetStudioResourceRepository = null;
        }


        /// <summary>
        /// Create resource with human Interface service type
        /// </summary>
        [TestMethod]
        public void LoadMultipleResourceLoad_HumanInterfaceServiceType_Expected_AllResourcesReturned()
        {
            //Arrange
            Setup();
            var model = new Mock<IResourceModel>();
            model.SetupGet(p => p.ResourceName).Returns("My WF");
            model.SetupGet(p => p.Category).Returns("Root");
            model.Setup(c => c.ResourceType).Returns(ResourceType.HumanInterfaceProcess);

            var conn = SetupConnection();

            var resourceData = BuildResourceObjectFromGuids(new[] { _resourceGuid }, Dev2.Data.ServiceModel.ResourceType.Server);
            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if(callCnt <= 1)
                    {
                        callCnt++;
                        return new StringBuilder(payload);
                    }

                    return resourceData;
                });



            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            //Act
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF1");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            _repo.Save(resourceModel.Object);
            _repo.Save(model.Object);
            _repo.Load();
            //Assert
            Assert.IsTrue(_repo.All().Count.Equals(2));

            mockStudioResourceRepository.Verify(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()), Times.Exactly(2));
            _repo.GetStudioResourceRepository = null;
        }

        /// <summary>
        /// Create resource with workflow service type
        /// </summary>
        [TestMethod]
        public void Load_MultipleResourceLoad_WorkflowServiceType_Expected_AllResourcesReturned()
        {
            //Arrange
            Setup();
            var model = new Mock<IResourceModel>();
            model.SetupGet(p => p.ResourceName).Returns("My WF");
            model.SetupGet(p => p.Category).Returns("Root");
            model.Setup(c => c.ResourceType).Returns(ResourceType.WorkflowService);

            var conn = SetupConnection();

            var resourceData = BuildResourceObjectFromGuids(new[] { _resourceGuid });
            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if(callCnt <= 1)
                    {
                        callCnt++;
                        return new StringBuilder(payload);
                    }

                    return resourceData;
                });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            //Act
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF1");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            _repo.Save(resourceModel.Object);
            _repo.Save(model.Object);
            _repo.Load();
            //Assert
            Assert.IsTrue(_repo.All().Count.Equals(2));
        }

        [TestMethod]
        [TestCategory("ResourceRepository_Load")]
        [Description("ResourceRepository Load must only do one server call to retrieve all resources")]
        [Owner("Trevor Williams-Ros")]
        public void ResourceRepository_UnitTest_Load_InvokesAddResourcesOnce()
        {

            var envConnection = new Mock<IEnvironmentConnection>();
            envConnection.Setup(e => e.WorkspaceID).Returns(Guid.NewGuid());
            envConnection.Setup(e => e.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.Connection).Returns(envConnection.Object);

            var resourceRepo = new TestResourceRepository(envModel.Object);
            resourceRepo.Load();

            Assert.AreEqual(1, resourceRepo.LoadResourcesHitCount, "ResourceRepository Load did more than one server call.");
        }

        #endregion Load Tests

        #region Save Tests

        [TestMethod]
        public void UpdateResource()
        {
            //Arrange
            Setup();
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.ResourceName).Returns("TestName");

            var conn = SetupConnection();

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(() => new StringBuilder(payload));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            _repo.Save(model.Object);
            _repo.Load();
            model.Setup(c => c.ResourceName).Returns("NewName");
            model.Setup(c => c.Category).Returns("NewCar");
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
            Setup();
            Mock<IResourceModel> model2 = new Mock<IResourceModel>();
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.DisplayName).Returns("result");
            model.Setup(c => c.ResourceName).Returns("result");
            model.Setup(c => c.Category).Returns("TestCat");
            model2.Setup(c => c.DisplayName).Returns("result");
            model2.Setup(c => c.ResourceName).Returns("result");
            model2.Setup(c => c.Category).Returns("TestCat");

            var conn = SetupConnection();

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(() => new StringBuilder(payload));

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;


            //Act
            _repo.Save(model.Object);
            _repo.Save(model2.Object);
            _repo.Load();

            Assert.IsTrue(_repo.All().Count.Equals(1));
            mockStudioResourceRepository.Verify(m => m.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_Save")]
        public void ResourceRepository_Save_ExecuteMessageIsSuccessful_CallsUpdatesOnTheStudioResourceRepository()
        {
            //------------Setup for test--------------------------
            var repo = new ResourceRepository(_environmentModel.Object)
                {
                    IsLoaded = true
                };

            var studioRepository = new Mock<IStudioResourceRepository>();
            studioRepository.Setup(m => m.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()))
                            .Verifiable();
            repo.GetStudioResourceRepository = () => studioRepository.Object;

            var commController = new Mock<ICommunicationController>();
            commController.Setup(m => m.ExecuteCommand<ExecuteMessage>(It.IsAny<IEnvironmentConnection>(), It.IsAny<Guid>()))
                          .Returns(new ExecuteMessage
                              {
                                  HasError = false
                              });
            repo.GetCommunicationController = someName => commController.Object;

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            //------------Execute Test---------------------------
            repo.Save(resourceModel.Object);
            //------------Assert Results-------------------------
            studioRepository.Verify(m => m.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_Save")]
        public void ResourceRepository_Save_ExecuteMessageIsNotSuccessful_DoesNotCallUpdatesOnTheStudioResourceRepository()
        {
            //------------Setup for test--------------------------
            var repo = new ResourceRepository(_environmentModel.Object)
            {
                IsLoaded = true
            };

            var studioRepository = new Mock<IStudioResourceRepository>();
            studioRepository.Setup(m => m.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()))
                            .Verifiable();
            repo.GetStudioResourceRepository = () => studioRepository.Object;

            var commController = new Mock<ICommunicationController>();
            commController.Setup(m => m.ExecuteCommand<ExecuteMessage>(It.IsAny<IEnvironmentConnection>(), It.IsAny<Guid>()))
                          .Returns(new ExecuteMessage
                          {
                              HasError = true
                          });
            repo.GetCommunicationController = someName => commController.Object;

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            //------------Execute Test---------------------------
            repo.Save(resourceModel.Object);
            //------------Assert Results-------------------------
            studioRepository.Verify(m => m.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()), Times.Never());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_Save")]
        public void ResourceRepository_Save_CategoryIsUnassignedAndResourceNameContainsUnsaved_DoesNotCallUpdatesOnTheStudioResourceRepository()
        {
            //------------Setup for test--------------------------
            var repo = new ResourceRepository(_environmentModel.Object)
            {
                IsLoaded = true
            };

            var studioRepository = new Mock<IStudioResourceRepository>();
            studioRepository.Setup(m => m.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()))
                            .Verifiable();
            repo.GetStudioResourceRepository = () => studioRepository.Object;

            var commController = new Mock<ICommunicationController>();
            commController.Setup(m => m.ExecuteCommand<ExecuteMessage>(It.IsAny<IEnvironmentConnection>(), It.IsAny<Guid>()))
                          .Returns(new ExecuteMessage
                          {
                              HasError = false
                          });
            repo.GetCommunicationController = someName => commController.Object;

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("Unsaved");
            resourceModel.SetupGet(p => p.Category).Returns("Unassigned");
            //------------Execute Test---------------------------
            repo.Save(resourceModel.Object);
            //------------Assert Results-------------------------
            studioRepository.Verify(m => m.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()), Times.Never());
        }

        #endregion Save Tests

        #region RemoveResource Tests

        [TestMethod]
        public void WorkFlowService_OnDelete_Expected_NotInRepository()
        {
            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);

            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var myItem = new ResourceModel(mockEnvironmentModel.Object) { ResourceName = "TestResource" };
            mockEnvironmentModel.Object.ResourceRepository.Add(myItem);
            int exp = mockEnvironmentModel.Object.ResourceRepository.All().Count;
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            ResourceRepository.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;
            ResourceRepository.Remove(myItem);
            Assert.AreEqual(exp - 1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
            mockEnvironmentModel.Object.ResourceRepository.Add(myItem);
            Assert.AreEqual(1, mockEnvironmentModel.Object.ResourceRepository.All().Count);

            mockStudioResourceRepository.Verify(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        public void WorkFlowService_OnDeleteFromWorkspace_Expected_InRepository()
        {
            var retVal = new StringBuilder();
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Callback((StringBuilder o, Guid dataListID, Guid workspaceID) =>
            {
                retVal = o;
            });

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var myItem = new ResourceModel(mockEnvironmentModel.Object) { ResourceName = "TestResource" };
            mockEnvironmentModel.Object.ResourceRepository.Add(myItem);
            int exp = mockEnvironmentModel.Object.ResourceRepository.All().Count;
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            ResourceRepository.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;
            ResourceRepository.DeleteResourceFromWorkspace(myItem);
            Assert.AreEqual(exp, mockEnvironmentModel.Object.ResourceRepository.All().Count);
            var retMsg = JsonConvert.DeserializeObject<ExecuteMessage>(retVal.ToString());
            Assert.IsNotNull(retMsg);
            mockStudioResourceRepository.Verify(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        public void NonExistantWorkFlowService_OnDelete_Expected_Failure()
        {
            var env = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var myRepo = new ResourceRepository(env.Object);
            var myItem = new ResourceModel(env.Object);

            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            myRepo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            var actual = myRepo.DeleteResource(myItem);
            Assert.AreEqual("Failure", actual.Message.ToString(), "Non existant resource deleted successfully");
            mockStudioResourceRepository.Verify(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        public void NonExistantWorkFlowService_DeleteFromWorkspace_Expected_Failure()
        {
            var env = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var myRepo = new ResourceRepository(env.Object);
            var myItem = new ResourceModel(env.Object);

            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            myRepo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            var actual = myRepo.DeleteResourceFromWorkspace(myItem);
            Assert.AreEqual("Failure", actual.Message.ToString(), "Non existant resource deleted successfully");
            mockStudioResourceRepository.Verify(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never());
        }


        [TestMethod]
        public void NullResource_DeleteFromWorkspace_Expected_Failure()
        {
            var env = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var myRepo = new ResourceRepository(env.Object);

            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            myRepo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            var actual = myRepo.DeleteResourceFromWorkspace(null);
            Assert.AreEqual("Failure", actual.Message.ToString(), "Non existant resource deleted successfully");
            mockStudioResourceRepository.Verify(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [TestCategory("ResourceRepository_Delete")]
        [Description("Unassigned resources can be deleted")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ResourceRepository_UnitTest_DeleteUnassignedResource_ResourceDeletedFromRepository()
        // ReSharper restore InconsistentNaming
        {

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);

            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var myItem = new ResourceModel(mockEnvironmentModel.Object) { ResourceName = "TestResource", Category = string.Empty };
            ResourceRepository.Add(myItem);
            int expectedCount = mockEnvironmentModel.Object.ResourceRepository.All().Count;
            ResourceRepository.GetStudioResourceRepository = () => new Mock<IStudioResourceRepository>().Object;
            mockEnvironmentModel.Object.ResourceRepository.DeleteResource(myItem);

            Assert.AreEqual(expectedCount - 1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
        }

        [TestMethod]
        [TestCategory("ResourceRepository_Delete")]
        [Owner("Hagashen Naidu")]
        // ReSharper disable InconsistentNaming
        public void ResourceRepository_DeleteResource_StudioResourceRepositoryRemoveItemCalled()
        // ReSharper restore InconsistentNaming
        {

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);

            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var myItem = new ResourceModel(mockEnvironmentModel.Object) { ResourceName = "TestResource", Category = string.Empty };

            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            ResourceRepository.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            ResourceRepository.Add(myItem);
            int expectedCount = mockEnvironmentModel.Object.ResourceRepository.All().Count;

            mockEnvironmentModel.Object.ResourceRepository.DeleteResource(myItem);

            Assert.AreEqual(expectedCount - 1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
            mockStudioResourceRepository.Verify(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [TestCategory("ResourceRepository_Delete")]
        [Owner("Hagashen Naidu")]
        // ReSharper disable InconsistentNaming
        public void ResourceRepository_DeleteResource_ResourceNameUnsaved_StudioResourceRepositoryRemoveItemNeverCalled()
        // ReSharper restore InconsistentNaming
        {

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);

            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var myItem = new ResourceModel(mockEnvironmentModel.Object) { ResourceName = "Unsaved 10" };

            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            ResourceRepository.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            ResourceRepository.Add(myItem);
            int expectedCount = mockEnvironmentModel.Object.ResourceRepository.All().Count;

            mockEnvironmentModel.Object.ResourceRepository.DeleteResource(myItem);

            Assert.AreEqual(expectedCount - 1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
            mockStudioResourceRepository.Verify(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [TestCategory("ResourceRepository_Delete")]
        [Owner("Hagashen Naidu")]
        // ReSharper disable InconsistentNaming
        public void ResourceRepository_DeleteResource_ResourceNameNotUnsavedUnassignedCategory_StudioResourceRepositoryRemoveItemCalled()
        // ReSharper restore InconsistentNaming
        {

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);

            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var myItem = new ResourceModel(mockEnvironmentModel.Object) { ResourceName = "my resource", Category = "Unassigned" };

            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            ResourceRepository.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            ResourceRepository.Add(myItem);
            int expectedCount = mockEnvironmentModel.Object.ResourceRepository.All().Count;

            mockEnvironmentModel.Object.ResourceRepository.DeleteResource(myItem);

            Assert.AreEqual(expectedCount - 1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
            mockStudioResourceRepository.Verify(repository => repository.DeleteItem(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        #endregion RemoveResource Tests

        #region Missing Environment Information Tests

        //Create resource repository without connected to any environment
        [TestMethod]
        public void CreateResourceEnvironmentConnectionNotConnected()
        {
            //Arrange
            Setup();

            ExecuteMessage msg = new ExecuteMessage();
            var exePayload = JsonConvert.SerializeObject(msg);

            _environmentConnection.Setup(envConn => envConn.IsConnected).Returns(false);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(false);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(exePayload));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            try
            {
                _repo.Save(_resourceModel.Object);
                _repo.Load();
            }
            //Assert
            catch(Exception iex)
            {
                Assert.AreEqual("No connected environment found to perform operation on.", iex.Message);
            }
        }
        //Create resource with no address to connet to any environment
        [TestMethod]
        public void CreateResourceNoAddressEnvironmentConnection()
        {
            //Arrange
            Setup();

            ExecuteMessage msg = new ExecuteMessage();
            var exePayload = JsonConvert.SerializeObject(msg);

            Mock<IEnvironmentConnection> environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(prop => prop.IsConnected).Returns(true);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(exePayload));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            _repo.Save(_resourceModel.Object);
            _repo.Load();
            int resources = _repo.All().Count(res => res.ResourceName == "Resource");
            //Assert
            Assert.IsTrue(resources == 1);

            mockStudioResourceRepository.Verify(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()), Times.Once());
            _repo.GetStudioResourceRepository = null;

        }
        //Create resource with no data channel connected to
        [TestMethod]
        public void CreateResourceNoDataChannelEnvironmentConnection()
        {
            //Arrange
            Setup();
            ExecuteMessage msg = new ExecuteMessage();
            var exePayload = JsonConvert.SerializeObject(msg);
            Mock<IEnvironmentConnection> environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(prop => prop.AppServerUri).Returns(new Uri("http://localhost:77/dsf"));
            environmentConnection.Setup(prop => prop.IsConnected).Returns(true);

            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(exePayload));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;
            _repo.Save(_resourceModel.Object);
            _repo.Load();
            int resources = _repo.All().Count(res => res.ResourceName == "Resource");
            //Assert
            Assert.IsTrue(resources == 1);
            mockStudioResourceRepository.Verify(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()), Times.Once());
            _repo.GetStudioResourceRepository = null;

        }

        #endregion Missing Environment Information Tests

        #region Resource Dependancies Tests

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
            ResourceRepository resourceRepository = new ResourceRepository(new Mock<IEnvironmentModel>().Object);
            var result = resourceRepository.GetDependanciesOnList(new List<IContextualResourceModel>(), null);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetDependanciesOnListWithNullModelReturnsEmptyList()
        {

            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            var testResources = new List<IResourceModel>(CreateResourceList(testEnvironmentModel2.Object));
            testEnvironmentModel2.Setup(e => e.ResourceRepository.All()).Returns(testResources);

            var resourceRepo = new ResourceRepository(new Mock<IEnvironmentModel>().Object);
            var resources = new List<IContextualResourceModel>();

            var result = resourceRepo.GetDependanciesOnList(resources, testEnvironmentModel2.Object);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetDependanciesOnListWithModel()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(StringResourcesTest.ResourceDependencyTestJsonReturn)).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            testEnvironmentModel2.Setup(e => e.Connection).Returns(mockConnection.Object);

            var resRepo = new ResourceRepository(testEnvironmentModel2.Object);
            var testResources = new List<IResourceModel>(CreateResourceList(testEnvironmentModel2.Object));
            foreach(var resourceModel in testResources)
            {
                resRepo.Add(resourceModel);
            }

            testEnvironmentModel2.Setup(e => e.ResourceRepository).Returns(resRepo);

            var resources = new List<IContextualResourceModel> { new ResourceModel(testEnvironmentModel2.Object) { ResourceName = "Button" } };

            resRepo.GetDependanciesOnList(resources, testEnvironmentModel2.Object);
            mockConnection.Verify(e => e.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
        }

        #endregion GetDependanciesOnList Tests

        #region GetDependanciesAsXML Tests

        [TestMethod]
        public void GetDependenciesXmlWithNullModelReturnsEmptyString()
        {
            ResourceRepository resourceRepository = new ResourceRepository(new Mock<IEnvironmentModel>().Object);
            var result = resourceRepository.GetDependenciesXml(null, false);
            Assert.IsTrue(string.IsNullOrEmpty(result.Message.ToString()));
        }

        [TestMethod]
        public void GetDependenciesXmlWithModel()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();

            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage(TestDependencyGraph.ToString());
            var payload = new StringBuilder(JsonConvert.SerializeObject(msg));

            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(payload).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            testEnvironmentModel2.Setup(e => e.Connection).Returns(mockConnection.Object);

            var resRepo = new ResourceRepository(testEnvironmentModel2.Object);
            var testResources = new List<IResourceModel>(CreateResourceList(testEnvironmentModel2.Object));
            foreach(var resourceModel in testResources)
            {
                resRepo.Add(resourceModel);
            }

            testEnvironmentModel2.Setup(e => e.ResourceRepository).Returns(resRepo);

            resRepo.GetDependenciesXml(new ResourceModel(testEnvironmentModel2.Object) { ResourceName = "Button" }, false);
            mockConnection.Verify(e => e.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
        }

        #endregion GetDependanciesAsXML Tests

        #region GetUniqueDependancies Tests

        [TestMethod]
        public void GetUniqueDependenciesWithNullModel()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(TestDependencyGraph.ToString())).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            testEnvironmentModel2.Setup(e => e.Connection).Returns(mockConnection.Object);

            var resRepo = new ResourceRepository(testEnvironmentModel2.Object);
            var testResources = new List<IResourceModel>(CreateResourceList(testEnvironmentModel2.Object));
            foreach(var resourceModel in testResources)
            {
                resRepo.Add(resourceModel);
            }

            testEnvironmentModel2.Setup(e => e.ResourceRepository).Returns(resRepo);

            resRepo.GetUniqueDependencies(null);
            mockConnection.Verify(e => e.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(0));
        }

        [TestMethod]
        public void GetUniqueDependenciesWithNullModelReturnsEmptyList()
        {
            ResourceRepository resourceRepository = new ResourceRepository(new Mock<IEnvironmentModel>().Object);
            var result = resourceRepository.GetUniqueDependencies(null);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetUniqueDependenciesWithModel()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();

            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage(TestDependencyGraph.ToString());
            var payload = new StringBuilder(JsonConvert.SerializeObject(msg));

            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(payload).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            testEnvironmentModel2.Setup(e => e.Connection).Returns(mockConnection.Object);

            var resRepo = new ResourceRepository(testEnvironmentModel2.Object);
            var testResources = new List<IResourceModel>(CreateResourceList(testEnvironmentModel2.Object));
            foreach(var resourceModel in testResources)
            {
                resRepo.Add(resourceModel);
            }

            testEnvironmentModel2.Setup(e => e.ResourceRepository).Returns(resRepo);

            resRepo.GetUniqueDependencies(new ResourceModel(testEnvironmentModel2.Object) { ResourceName = "Button" });
            mockConnection.Verify(e => e.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Exactly(1));
        }

        [TestMethod]
        public void GetUniqueDependenciesWithModelReturnsUniqueItems()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();

            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage(TestDependencyGraph.ToString());
            var payload = new StringBuilder(JsonConvert.SerializeObject(msg));

            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(payload).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(mockConnection.Object);

            var resRepo = new ResourceRepository(environmentModel.Object);

            var testExpectedResources = CreateResourceList(environmentModel.Object);
            var resources = new List<IResourceModel>(testExpectedResources);
            foreach(var resourceModel in resources)
            {
                resRepo.Add(resourceModel);
            }

            environmentModel.Setup(e => e.ResourceRepository).Returns(resRepo);

            var result = resRepo.GetUniqueDependencies(new ResourceModel(environmentModel.Object) { ResourceName = "Button" });
            Assert.IsTrue(result.SequenceEqual(testExpectedResources));
        }

        [TestMethod]
        public void GetUniqueDependenciesWithModelReturnsEmptyListWhenNoResourcesMatch()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();

            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage(TestDependencyGraph.ToString());
            var payload = new StringBuilder(JsonConvert.SerializeObject(msg));

            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(payload).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(mockConnection.Object);

            var resRepo = new ResourceRepository(environmentModel.Object);

            environmentModel.Setup(e => e.ResourceRepository).Returns(resRepo);

            var result = resRepo.GetUniqueDependencies(new ResourceModel(environmentModel.Object) { ResourceName = "Button" });
            Assert.AreEqual(0, result.Count);
        }

        #endregion GetUniqueDependancies Tests

        #endregion

        #region ReloadResource Tests

        ///// <summary>
        ///// Create resource with source type
        ///// </summary>
        [TestMethod]
        public void ReloadResourcesWhereNothingLoadedExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            AppSettings.LocalHost = "http://localhost:3142/";


            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();

            var resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid, newGuid });

            ExecuteMessage msg = new ExecuteMessage { HasError = false };

            int cnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(() =>
            {
                if(cnt == 0)
                {
                    cnt = 1;
                    return new StringBuilder(JsonConvert.SerializeObject(msg));
                }

                return resourceObj;
            });
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            //------------Execute Test---------------------------
            var reloadedResources = _repo.ReloadResource(_resourceGuid, ResourceType.WorkflowService, new ResourceModelEqualityComparerForTest(), false);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, reloadedResources.Count);
            mockStudioResourceRepository.Verify(m => m.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()), Times.Exactly(2));
        }


        [TestMethod]
        public void ResourceRepositoryReloadResourcesWithValidArgsExpectedSetsProperties()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();

            List<ErrorInfo> errors = new List<ErrorInfo>
                {
                    new ErrorInfo
                        {
                            ErrorType = ErrorType.Critical,
                            Message = "MappingChange",
                            StackTrace = "SomethingWentWrong",
                            FixType = FixType.None
                        }
                };

            var resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid }, Dev2.Data.ServiceModel.ResourceType.WorkflowService, errors);

            var msg = new ExecuteMessage { HasError = false };


            int cnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(

                () =>
                {
                    if(cnt == 0)
                    {
                        cnt = 1;
                        return new StringBuilder(JsonConvert.SerializeObject(msg));
                    }

                    return resourceObj;
                }
            );


            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            //------------Execute Test---------------------------
            var reloadedResources = _repo.ReloadResource(_resourceGuid, ResourceType.WorkflowService, new ResourceModelEqualityComparerForTest(), false);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, reloadedResources.Count);
            var resources = _repo.All().ToList();
            var actual = (IContextualResourceModel)resources[0];
            Assert.AreEqual(_resourceGuid, actual.ID);
            Assert.AreEqual(true, actual.IsValid);

            Assert.IsNotNull(actual.Errors);
            Assert.AreEqual(1, actual.Errors.Count);
            var error = actual.Errors[0];
            Assert.AreEqual(ErrorType.Critical, error.ErrorType);
            Assert.AreEqual(FixType.None, error.FixType);
            Assert.AreEqual("MappingChange", error.Message);
            Assert.AreEqual("SomethingWentWrong", error.StackTrace);
            mockStudioResourceRepository.Verify(m => m.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()), Times.Exactly(1));
        }

        [TestMethod]
        public void ResourceRepositoryLoadWorkspaceWithValidArgsExpectedSetsProperties()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();

            List<ErrorInfo> errors = new List<ErrorInfo>
                {
                    new ErrorInfo
                        {
                            ErrorType = ErrorType.Critical,
                            Message = "MappingChange",
                            StackTrace = "SomethingWentWrong",
                            FixType = FixType.None
                        }
                };

            var resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid }, Dev2.Data.ServiceModel.ResourceType.WorkflowService, errors);



            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(
                () => resourceObj);


            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;

            //------------Execute Test---------------------------
            _repo.LoadResourceFromWorkspace(_resourceGuid, Guid.Empty);

            //------------Assert Results-------------------------
            var resources = _repo.All().ToList();
            var actual = (IContextualResourceModel)resources[0];
            Assert.AreEqual(_resourceGuid, actual.ID);
            Assert.AreEqual(true, actual.IsValid);

            mockStudioResourceRepository.Verify(m => m.ItemAddedMessageHandler(It.IsAny<IExplorerItem>()), Times.Exactly(1));
        }

        #endregion ReloadResource Tests

        #region FindResourcesByID

        [TestMethod]
        public void FindResourcesByID_With_NullParameters_Expected_ReturnsEmptyList()
        {
            ResourceRepository resourceRepository = GetResourceRepository();
            var result = resourceRepository.FindResourcesByID(null, null, ResourceType.Source);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FindResourcesByID_With_NonNullParameters_Expected_ReturnsNonEmptyList()
        {
            var servers = new List<string> { EnviromentRepositoryTest.Server1ID };
            ResourceRepository resourceRepository = GetResourceRepository();

            var res = new SerializableResource { ResourceID = EnviromentRepositoryTest.Server2ID, ResourceName = "Resource" };
            var resList = new List<SerializableResource> { res };
            var src = JsonConvert.SerializeObject(resList);

            var env = EnviromentRepositoryTest.CreateMockEnvironment(true, src);

            var result = resourceRepository.FindResourcesByID(env.Object, servers, ResourceType.Source);

            Assert.AreNotEqual(0, result.Count);
        }

        #endregion

        #region FetchResourceDefinition

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceRepository_FetchResourceDefinition")]
        public void ResourceRepository_FetchResourceDefinition_WhenDefinitionToFetch_ExpectValidXAML()
        {
            //------------Setup for test--------------------------
            Guid modelID = new Guid();
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();
            con.Setup(c => c.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);

            var msg = MakeMsg("model definition");
            var payload = JsonConvert.SerializeObject(msg);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            //------------Execute Test---------------------------
            var result = new ResourceRepository(env.Object).FetchResourceDefinition(env.Object, Guid.Empty, modelID);

            //------------Assert Results-------------------------
            Assert.AreEqual("model definition", result.Message.ToString());

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceRepository_FetchResourceDefinition")]
        public void ResourceRepository_FetchResourceDefinition_WhenNoDefinitionToFetch_ExpectNothing()
        {
            //------------Setup for test--------------------------
            Guid modelID = new Guid();
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();
            con.Setup(c => c.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);

            var msg = MakeMsg(string.Empty);
            var payload = JsonConvert.SerializeObject(msg);

            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            //------------Execute Test---------------------------
            var result = new ResourceRepository(env.Object).FetchResourceDefinition(env.Object, Guid.Empty, modelID);

            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, result.Message.ToString());
        }

        #endregion

        #region FindSourcesByType

        [TestMethod]
        public void FindSourcesByType_With_NullParameters_Expected_ReturnsEmptyList()
        {
            ResourceRepository resourceRepository = GetResourceRepository();
            var result = resourceRepository.FindSourcesByType<IEnvironmentModel>(null, enSourceType.Dev2Server);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FindSourcesByType_With_NonNullParameters_Expected_ReturnsNonEmptyList()
        {

            var res = new EmailSource { ResourceID = Guid.NewGuid() };
            var resList = new List<EmailSource> { res };
            var src = JsonConvert.SerializeObject(resList);

            var env = EnviromentRepositoryTest.CreateMockEnvironment(true, src);
            ResourceRepository resourceRepository = GetResourceRepository();
            List<EmailSource> result = resourceRepository.FindSourcesByType<EmailSource>(env.Object, enSourceType.EmailSource);

            Assert.AreNotEqual(0, result.Count);
        }

        #endregion

        #region Find

        [TestMethod]
        public void FindWithValidFunctionExpectResourceReturned()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(BuildResourceObjectFromGuids(new[] { newGuid }));
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
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new StringBuilder(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2)));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();
            //------------Execute Test---------------------------
            var resourceModels = _repo.Find(null);
            //------------Assert Results-------------------------
            Assert.IsNull(resourceModels);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceRepository_FindSingle")]
        public void ResourceRepository_FindSingle_WhenLoadDefinition_ExpectValidXAML()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(BuildResourceObjectFromGuids(new[] { newGuid }));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();

            const string modelDefinition = "model definition";
            var msg = MakeMsg(modelDefinition);
            var payload = JsonConvert.SerializeObject(msg);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            //------------Execute Test---------------------------
            var result = _repo.FindSingle(model => model.ID == newGuid, true);

            //------------Assert Results-------------------------
            Assert.IsNotNull(result.WorkflowXaml);
            StringAssert.Contains(result.WorkflowXaml.ToString(), modelDefinition);

        }
        #endregion

        #region IsWorkflow
        [TestMethod]
        public void IsWorkflowValidWorkflowExpectTrue()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();

            var resourceObj = BuildResourceObjectFromGuids(new[] { newGuid });

            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resourceObj);
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();
            //------------Execute Test---------------------------
            var isWorkFlow = _repo.IsWorkflow("TestWorkflowService");
            //------------Assert Results-------------------------
            Assert.IsTrue(isWorkFlow);
        }

        [TestMethod]
        public void IsWorkflowNotValidWorkflowExpectFalse()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new StringBuilder(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2)));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();
            //------------Execute Test---------------------------
            var isWorkFlow = _repo.IsWorkflow("TestWorkflowService");
            //------------Assert Results-------------------------
            Assert.IsFalse(isWorkFlow);
        }
        #endregion

        #region RemoveEnvironment

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveEnvironment_With_NullParameters_Expected_ThrowsArgumentNullException()
        {
            ResourceRepository resourceRepository = GetResourceRepository();
            resourceRepository.RemoveEnvironment(null, null);
        }

        [TestMethod]
        public void RemoveEnvironment_With_NonNullParameters_Expected_InvokesExecuteCommandOnTargetEnvironment()
        {
            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            var payload = new StringBuilder(JsonConvert.SerializeObject(msg));

            var testEnv = EnviromentRepositoryTest.CreateMockEnvironment();
            ResourceRepository resourceRepository = GetResourceRepository();
            var targetEnv = new Mock<IEnvironmentModel>();
            var rand = new Random();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(payload);
            targetEnv.Setup(e => e.Connection).Returns(connection.Object);

            resourceRepository.RemoveEnvironment(targetEnv.Object, testEnv.Object);

            connection.Verify(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        #endregion

        #region IsLoaded


        #endregion IsLoaded

        #region Constructor
        #endregion

        #region HydrateResourceTest

        [TestMethod]
        public void HydrateResourceHydratesResourceType()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();


            var resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid, newGuid }, Dev2.Data.ServiceModel.ResourceType.Server);

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(() =>
        {
            if(callCnt == 0)
            {
                callCnt = 1;
                return new StringBuilder(payload);
            }

            return resourceObj;
        });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;
            //------------Execute Test---------------------------
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            _repo.Save(resourceModel.Object);
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
        public void ResourceRepository_HydrateResourceModel_ResourceRepositoryUnitTest_ResourceErrors_Hydrated()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();

            var id = Guid.NewGuid();

            List<ErrorInfo> errors = new List<ErrorInfo>
            {
                new ErrorInfo
                    {
                        InstanceID = new Guid("edadb62e-83f4-44bf-a260-7639d6b43169"),
                        ErrorType = ErrorType.Critical,
                        Message = "Mapping out of date",
                        StackTrace = "",
                        FixType = FixType.ReloadMapping,
                        FixData = "<Args><Input>[{\"Name\":\"n1\",\"MapsTo\":\"\",\"Value\":\"\",\"IsRecordSet\":false,\"RecordSetName\":\"\",\"IsEvaluated\":false,\"DefaultValue\":\"\",\"IsRequired\":false,\"RawValue\":\"\",\"EmptyToNull\":false},{\"Name\":\"n2\",\"MapsTo\":\"\",\"Value\":\"\",\"IsRecordSet\":false,\"RecordSetName\":\"\",\"IsEvaluated\":false,\"DefaultValue\":\"\",\"IsRequired\":false,\"RawValue\":\"\",\"EmptyToNull\":false}]</Input><Output>[{\"Name\":\"result\",\"MapsTo\":\"\",\"Value\":\"\",\"IsRecordSet\":false,\"RecordSetName\":\"\",\"IsEvaluated\":false,\"DefaultValue\":\"\",\"IsRequired\":false,\"RawValue\":\"\",\"EmptyToNull\":false}]</Output></Args>"
                    }
            };

            var resourceData = BuildResourceObjectFromGuids(new[] { id }, Dev2.Data.ServiceModel.ResourceType.WorkflowService, errors, false);

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if(callCnt == 0)
                    {
                        callCnt = 1;
                        return new StringBuilder(payload);
                    }

                    return resourceData;
                });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var mockStudioResourceRepository = new Mock<IStudioResourceRepository>();
            mockStudioResourceRepository.Setup(repository => repository.ItemAddedMessageHandler(It.IsAny<IExplorerItem>())).Verifiable();
            _repo.GetStudioResourceRepository = () => mockStudioResourceRepository.Object;
            //------------Execute Test---------------------------
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            _repo.Save(resourceModel.Object);
            _repo.ForceLoad();
            var resources = _repo.All();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, resources.Count, "HydrateResourceModel failed to load the resource.");

            var resource = resources.First();
            Assert.IsFalse(resource.IsValid, "HydrateResourceModel failed to hydrate IsValid.");
            Assert.AreEqual(1, resource.Errors.Count);

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
            Setup();
            var conn = SetupConnection();
            var guid2 = Guid.NewGuid();

            var resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid, guid2 });

            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resourceObj);
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var guid = Guid.NewGuid();

            resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid, guid });

            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resourceObj);
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
            Setup();
            var conn = SetupConnection();
            var guid2 = Guid.NewGuid().ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new StringBuilder(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2)));
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var guid = Guid.NewGuid();
            guid2 = guid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
             .Returns(new StringBuilder(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>", _resourceGuid, guid2)));
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
            Setup();
            var conn = SetupConnection();
            var guid2 = Guid.NewGuid();

            var resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid, guid2 });

            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resourceObj);
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var guid = Guid.NewGuid();

            resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid, guid });

            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resourceObj);
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
            Setup();
            var conn = SetupConnection();
            var guid2 = Guid.NewGuid().ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new StringBuilder(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2)));
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var guid = Guid.NewGuid();
            guid2 = guid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
             .Returns(new StringBuilder(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>", _resourceGuid, guid2)));
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

        #region DeployResources

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceRepository_DeployResources")]
        public void ResourceRepository_DeployResources_WhenNormalDeploy_ExpectRefreshOnTargetResourceRepo()
        {
            //------------Setup for test--------------------------
            Setup();
            var theID = Guid.NewGuid();

            Mock<IResourceRepository> srcRepo = new Mock<IResourceRepository>();
            Mock<IResourceRepository> targetRepo = new Mock<IResourceRepository>();

            Mock<IEnvironmentModel> srcModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentModel> targetModel = new Mock<IEnvironmentModel>();

            // config the repos
            srcModel.Setup(sm => sm.ResourceRepository).Returns(srcRepo.Object);
            srcRepo.Setup(sr => sr.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false))
                   .Returns(_resourceModel.Object);

            targetModel.Setup(tm => tm.ResourceRepository).Returns(targetRepo.Object);
            targetRepo.Setup(tr => tr.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), ResourceModelEqualityComparer.Current, It.IsAny<bool>())).Verifiable();

            IList<IResourceModel> deployModels = new List<IResourceModel>();

            var theModel = new ResourceModel(srcModel.Object) { ID = theID };
            deployModels.Add(theModel);

            Mock<IEventAggregator> mockEventAg = new Mock<IEventAggregator>();
            mockEventAg.Setup(m => m.Publish(It.IsAny<object>()));

            IDeployDto dto = new DeployDto { ResourceModels = deployModels };

            //------------Execute Test---------------------------
            _repo.DeployResources(srcModel.Object, targetModel.Object, dto, mockEventAg.Object);

            //------------Assert Results-------------------------
            targetRepo.Verify(tr => tr.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), ResourceModelEqualityComparer.Current, It.IsAny<bool>()));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceRepository_DeployResource")]
        public void ResourceRepository_DeployResource_WhenNormalDeploy_ExpectDeployCalled()
        {
            //------------Setup for test--------------------------
            Setup();
            var theID = Guid.NewGuid();

            Mock<IResourceRepository> srcRepo = new Mock<IResourceRepository>();
            Mock<IResourceRepository> targetRepo = new Mock<IResourceRepository>();

            Mock<IEnvironmentModel> srcModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentModel> targetModel = new Mock<IEnvironmentModel>();

            // config the repos
            srcModel.Setup(sm => sm.ResourceRepository).Returns(srcRepo.Object);
            srcRepo.Setup(sr => sr.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false))
                   .Returns(_resourceModel.Object);

            targetModel.Setup(tm => tm.ResourceRepository).Returns(targetRepo.Object);
            targetRepo.Setup(tr => tr.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), ResourceModelEqualityComparer.Current, It.IsAny<bool>())).Verifiable();

            var theModel = new ResourceModel(srcModel.Object) { ID = theID, ResourceName = "some resource" };
            Mock<IEventAggregator> mockEventAg = new Mock<IEventAggregator>();
            mockEventAg.Setup(m => m.Publish(It.IsAny<object>()));

            //------------Execute Test---------------------------
            _repo.DeployResource(theModel);
            //------------Assert Results-------------------------
            _environmentConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceRepository_DeployResources")]
        public void ResourceRepository_DeployResources_WhenNormalDeploy_ExpectUpdatedResource()
        {
            //------------Setup for test--------------------------
            Setup();
            var theID = Guid.NewGuid();

            Mock<IResourceRepository> srcRepo = new Mock<IResourceRepository>();
            Mock<IResourceRepository> targetRepo = new Mock<IResourceRepository>();

            Mock<IEnvironmentModel> srcEnvModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentModel> targetEnvModel = new Mock<IEnvironmentModel>();

            // config the repos
            IResourceModel findModel = new ResourceModel(targetEnvModel.Object);
            findModel.ID = theID;
            srcRepo.Setup(sr => sr.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false))
                  .Returns(findModel);

            srcEnvModel.Setup(sm => sm.ResourceRepository).Returns(srcRepo.Object);

            targetRepo.Setup(tr => tr.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), ResourceModelEqualityComparer.Current, It.IsAny<bool>())).Verifiable();
            targetEnvModel.Setup(tm => tm.ResourceRepository).Returns(targetRepo.Object);

            Mock<IResourceModel> reloadedResource = new Mock<IResourceModel>();
            reloadedResource.Setup(res => res.ResourceName).Returns("Resource");
            reloadedResource.Setup(res => res.DisplayName).Returns("My New Resource");
            reloadedResource.Setup(res => res.ID).Returns(theID);
            reloadedResource.Setup(res => res.WorkflowXaml).Returns(new StringBuilder("NewXaml"));

            List<IResourceModel> reloadResources = new List<IResourceModel> { reloadedResource.Object };


            targetRepo.Setup(tr => tr.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), ResourceModelEqualityComparer.Current, It.IsAny<bool>())).Returns(reloadResources);

            IList<IResourceModel> deployModels = new List<IResourceModel>();

            var theModel = new ResourceModel(srcEnvModel.Object) { ID = theID };
            deployModels.Add(theModel);

            Mock<IEventAggregator> mockEventAg = new Mock<IEventAggregator>();
            mockEventAg.Setup(m => m.Publish(It.IsAny<object>()));

            IDeployDto dto = new DeployDto { ResourceModels = deployModels };

            //------------Execute Test---------------------------
            _repo.DeployResources(srcEnvModel.Object, targetEnvModel.Object, dto, mockEventAg.Object);

            //------------Assert Results-------------------------
            targetRepo.Verify(tr => tr.ReloadResource(It.IsAny<Guid>(), It.IsAny<ResourceType>(), ResourceModelEqualityComparer.Current, It.IsAny<bool>()));

        }

        #endregion

        #region DeployResource

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceRepositoryDeployResourceWithNullExpectedThrowsArgumentNullException()
        {
            var repoConn = new Mock<IEnvironmentConnection>();

            var repoEnv = new Mock<IEnvironmentModel>();
            repoEnv.Setup(e => e.Connection).Returns(repoConn.Object);

            var repo = new ResourceRepository(repoEnv.Object);

            repo.DeployResource(null);
        }

        //// BUG 9703 - 2013.06.21 - TWR - added
        //[TestMethod]
        //public void ResourceRepositoryDeployResourceWithNewResourceExpectedCreatesAndAddsNewResourceWithRepositoryEnvironment()
        //{
        //    var repoConn = new Mock<IEnvironmentConnection>();
        //    repoConn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
        //    repoConn.Setup(c => c.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);
        //    repoConn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("<XmlData></XmlData>");

        //    // DO NOT USE Mock EnvironmentModel's - otherwise EnvironmentModel.IEquatable will fail!
        //    //new Mock<IWizardEngine>().Object
        //    var repoEnv = new EnvironmentModel(Guid.NewGuid(), repoConn.Object, false);

        //    var resourceConn = new Mock<IEnvironmentConnection>();
        //    resourceConn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
        //    resourceConn.Setup(c => c.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);
        //    //, new Mock<IWizardEngine>().Object
        //    var resourceEnv = new EnvironmentModel(Guid.NewGuid(), resourceConn.Object, false);

        //    var newResource = new ResourceModel(resourceEnv)
        //    {
        //        ID = Guid.NewGuid(),
        //        Category = "Test",
        //        ResourceName = "TestResource"
        //    };

        //    repoEnv.ResourceRepository.DeployResource(newResource);

        //    var actual = repoEnv.ResourceRepository.FindSingle(r => r.ID == newResource.ID) as IContextualResourceModel;

        //    Assert.IsNotNull(actual);
        //    Assert.AreNotSame(newResource, actual);
        //    Assert.AreSame(repoEnv, actual.Environment);
        //}

        //// BUG 9703 - 2013.06.21 - TWR - added
        //[TestMethod]
        //public void ResourceRepositoryDeployResourceWithExistingResourceExpectedCreatesAndAddsNewResourceWithRepositoryEnvironment()
        //{
        //    var repoConn = new Mock<IEnvironmentConnection>();
        //    repoConn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
        //    repoConn.Setup(c => c.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);
        //    repoConn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("<XmlData></XmlData>");

        //    // DO NOT USE Mock EnvironmentModel's - otherwise EnvironmentModel.IEquatable will fail!
        //    //, new Mock<IWizardEngine>().Object
        //    var repoEnv = new EnvironmentModel(Guid.NewGuid(), repoConn.Object, false);

        //    //var repo = new Mock<IResourceRepository>();
        //    //repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("resource xaml");

        //    var resourceConn = new Mock<IEnvironmentConnection>();
        //    resourceConn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
        //    resourceConn.Setup(c => c.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);
        //    //, new Mock<IWizardEngine>().Object
        //    var resourceEnv = new EnvironmentModel(Guid.NewGuid(), resourceConn.Object, false);

        //    var oldResource = new ResourceModel(repoEnv)
        //    {
        //        ID = Guid.NewGuid(),
        //        Category = "Test",
        //        ResourceName = "TestResource"
        //    };
        //    repoEnv.ResourceRepository.Add(oldResource);

        //    var newResource = new ResourceModel(resourceEnv)
        //    {
        //        ID = oldResource.ID,
        //        Category = "Test",
        //        ResourceName = oldResource.ResourceName
        //    };

        //    repoEnv.ResourceRepository.DeployResource(newResource);

        //    var actual = repoEnv.ResourceRepository.FindSingle(r => r.ID == newResource.ID) as IContextualResourceModel;

        //    Assert.IsNotNull(actual);
        //    Assert.AreNotSame(newResource, actual);
        //    Assert.AreSame(repoEnv, actual.Environment);
        //}

        #endregion

        #region Rename

        [TestMethod]
        [TestCategory("ResourceRepositoryUnitTest")]
        [Description("Test for ResourceRepository's rename function: Rename is called and connection is expected to be open with correct package to the server")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ResourceRepository_ResourceRepositoryUnitTest_RenameResource_ExecuteCommandExecutesTheRightXmlPayload()
        // ReSharper restore InconsistentNaming
        {
            //init
            var resID = Guid.NewGuid();
            const string newResName = "New Test Name";
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();

            var msg = MakeMsg("Renamed Resource");
            var payload = JsonConvert.SerializeObject(msg);

            mockEnvironmentConnection.Setup(c => c.IsConnected).Returns(true);
            mockEnvironmentConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            //mockEnvironment.Setup(c => c.Connection.SecurityContext);
            mockEnvironment.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);

            var vm = new ResourceRepository(mockEnvironment.Object);
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.Setup(res => res.ID).Returns(resID);
            string actualRenamedValue = null;
            resourceModel.SetupSet(res => res.ResourceName = It.IsAny<string>()).Callback<string>(value =>
                {
                    actualRenamedValue = value;
                });
            vm.Add(resourceModel.Object);


            //exe
            vm.Rename(resID.ToString(), newResName);

            //assert
            mockEnvironmentConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
            Assert.IsNotNull(actualRenamedValue, "Resource not renamed locally");
            Assert.AreEqual(newResName, actualRenamedValue);
        }

        [TestMethod]
        [TestCategory("ResourceRepositoryUnitTest")]
        [Description("Test for ResourceRepository's rename category function")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ResourceRepository_ResourceRepositoryUnitTest_RenameCategory_ExecuteCommandIsCalledOnce()
        // ReSharper restore InconsistentNaming
        {
            var msg = MakeMsg("Renamed Resource");
            var payload = JsonConvert.SerializeObject(msg);

            //MEF!!!!
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(c => c.IsConnected).Returns(true);
            mockEnvironmentConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));
            mockEnvironment.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);
            var vm = new ResourceRepository(mockEnvironment.Object);
            vm.RenameCategory("Test Category", "New Test Category", ResourceType.WorkflowService);

            mockEnvironmentConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [TestCategory("ResourceRepositoryUnitTest")]
        [Description("Test for ResourceRepository's rename function: Rename is called and connection is expected to be executed with correct package")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ResourceRepository_RenameResource_DashesInTheName_ExecuteCommandExecutesTheRightXmlPayload()
        // ReSharper restore InconsistentNaming
        {
            var resourceID = Guid.NewGuid().ToString();

            var msg = MakeMsg("Renamed Resource");
            var payload = JsonConvert.SerializeObject(msg);

            //init conn
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));
            mockEnvironmentConnection.Setup(c => c.IsConnected).Returns(true);
            mockEnvironment.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);

            //init repo
            var repo = new ResourceRepository(mockEnvironment.Object);

            //exe rename
            repo.Rename(resourceID, "New-Test-Name");

            //assert correct command sent to server
            mockEnvironmentConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        #endregion

        #region Helper Methods

        #region Get ServerLog TempPath

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResourceRepository_GetServerLogTempPath")]
        public void ResourceRepository_GetServerLogTempPath_ServerLogFileBlank_DoNotCreateTempFile()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(conn => conn.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());
            mockConnection.Setup(c => c.IsConnected).Returns(true);
            var mockEnv = new Mock<IEnvironmentModel>();
            mockEnv.Setup(svr => svr.Connection).Returns(mockConnection.Object);
            ResourceRepository resourceRepository = new ResourceRepository(mockEnv.Object);
            //------------Execute Test---------------------------
            var actual = resourceRepository.GetServerLogTempPath(mockEnv.Object);

            // Assert DoNotCreateTempFile
            Assert.IsNull(actual, "Path returned for blank log file");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceRepository_GetServerLogTempPath")]
        public void ResourceRepository_GetServerLogTempPath_ServerDisconnected_DoNotCreateTempFile()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(conn => conn.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());
            mockConnection.Setup(c => c.IsConnected).Returns(false); // causes exception which is now caught!

            var mockEnv = new Mock<IEnvironmentModel>();
            mockEnv.Setup(svr => svr.Connection).Returns(mockConnection.Object);
            ResourceRepository resourceRepository = new ResourceRepository(mockEnv.Object);

            //------------Execute Test---------------------------
            var actual = resourceRepository.GetServerLogTempPath(mockEnv.Object);

            // Assert DoNotCreateTempFile
            Assert.IsNull(actual, "Path returned for disconnected server");
        }

        #endregion


        private SerializableResource BuildSerializableResourceFromName(string name, Dev2.Data.ServiceModel.ResourceType typeOf, bool isNewResource = false)
        {

            SerializableResource sr = new SerializableResource
            {
                ResourceCategory = "Test Category",
                DataList = "",
                Errors = new List<ErrorInfo>(),
                IsValid = true,
                ResourceID = Guid.NewGuid(),
                ResourceName = name,
                ResourceType = typeOf,
                IsNewResource = isNewResource
            };

            return sr;

        }

        /// <summary>
        /// Builds the collection resource object.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="typeOf">The type of.</param>
        /// <returns></returns>
        private string BuildResourceObjectFromNames(string[] names, Dev2.Data.ServiceModel.ResourceType typeOf)
        {
            List<SerializableResource> theResources = new List<SerializableResource>();

            int cnt = names.Length;
            for(int i = 0; i < cnt; i++)
            {
                SerializableResource sr = new SerializableResource
                {
                    ResourceCategory = "Test Category",
                    DataList = "",
                    Errors = new List<ErrorInfo>(),
                    IsValid = true,
                    ResourceID = Guid.NewGuid(),
                    ResourceName = names[i],
                    ResourceType = typeOf
                };

                theResources.Add(sr);
            }

            var serviceObj = JsonConvert.SerializeObject(theResources);

            return serviceObj;
        }

        /// <summary>
        /// Builds the resource object.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="theType">The type.</param>
        /// <param name="errors"></param>
        /// <param name="isValid"></param>
        /// <returns></returns>
        private StringBuilder BuildResourceObjectFromGuids(IEnumerable<Guid> ids, Dev2.Data.ServiceModel.ResourceType theType = Dev2.Data.ServiceModel.ResourceType.WorkflowService, List<ErrorInfo> errors = null, bool isValid = true)
        {
            if(errors == null)
            {
                errors = new List<ErrorInfo>();
            }

            var theResources = ids.Select(id => new SerializableResource
            {
                ResourceCategory = "Test Category",
                DataList = "",
                Errors = errors,
                IsValid = isValid,
                ResourceID = id,
                ResourceName = "TestWorkflowService",
                ResourceType = theType,
            }).ToList();

            var serviceObj = JsonConvert.SerializeObject(theResources);

            return new StringBuilder(serviceObj);
        }

        private ResourceRepository GetResourceRepository(Permissions permissions = Permissions.Administrator)
        {
            var conn = SetupConnection();

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(e => e.AuthorizationService.GetResourcePermissions(It.IsAny<Guid>())).Returns(permissions);
            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var resourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            return resourceRepository;
        }

        #endregion


        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void GetDatabaseTableColumns_DBTableHasSchema_ShouldAddSchemaToPayload()
        {
            //Arrange
            Setup();
            var conn = SetupConnection();


            //int callCnt = 0;
            StringBuilder sentPayLoad = new StringBuilder();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Callback((StringBuilder o, Guid g1, Guid g2) =>
                {
                    sentPayLoad = o;
                });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.GetDatabaseTableColumns(new DbSource { DatabaseName = "TestDB", ConnectionString = "MyConn", Server = "AzureServer" }, new DbTable { Schema = "Master", TableName = "Transactions" });

            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var esbExecuteRequest = jsonSerializer.Deserialize<EsbExecuteRequest>(sentPayLoad);
            StringAssert.Contains(esbExecuteRequest.Args["Schema"].ToString(), "Master");
            StringAssert.Contains(esbExecuteRequest.Args["TableName"].ToString(), "Transactions");
            StringAssert.Contains(esbExecuteRequest.Args["Database"].ToString(), "TestDB");
        }


        static Mock<IEnvironmentConnection> CreateEnvironmentConnection()
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(new EventPublisher());
            return connection;
        }


        public static ExecuteMessage MakeMsg(string msg)
        {
            var result = new ExecuteMessage { HasError = false };
            result.SetMessage(msg);
            return result;
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
