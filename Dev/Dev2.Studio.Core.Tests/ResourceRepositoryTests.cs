/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Core.Tests;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.XML;
using Dev2.Data;
using Dev2.Data.ServiceModel;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Runtime.ESB.Management.Services;
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
using Warewolf.Studio.ViewModels;
// ReSharper disable ReturnTypeCanBeEnumerable.Local

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace BusinessDesignStudio.Unit.Tests
{
    /// <summary>
    /// Summary description for ResourceRepositoryTest
    /// </summary>
    [TestClass]
    public class ResourceRepositoryTests
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
            AppSettings.LocalHost = "http://myserver:3142/";
            _authService.Setup(s => s.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);

            _resourceModel.Setup(res => res.ResourceName).Returns("Resource");
            _resourceModel.Setup(res => res.DisplayName).Returns("My New Resource");
            _resourceModel.Setup(model => model.Category).Returns("MyFolder\\Resource");
            _resourceModel.Setup(res => res.ID).Returns(_resourceGuid);
            _resourceModel.Setup(res => res.WorkflowXaml).Returns(new StringBuilder("OriginalXaml"));
            _resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
            _environmentConnection.Setup(channel => channel.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder("<x><text>Im Happy</text></x>")).Verifiable();
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
            var resourceData = BuildSerializableResourceFromName("TestWF", "DbService");
            resourceData.Inputs = inputData;
            resourceData.Outputs = outputData;

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(resourceData, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(inputData, model.Inputs);
            Assert.AreEqual(outputData, model.Outputs);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_WhenDataIsNewWorkflow_NewWorkFlowNamesUpdated()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            var resourceData = BuildSerializableResourceFromName("Unsaved 1", "WorkflowService", true);

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(resourceData, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.IsTrue(NewWorkflowNames.Instance.Contains("Unsaved 1"));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void WorkFlowService_HydrateResourceModel_ServerDisconnected_ShowPopup()
        {
            var retVal = new StringBuilder();
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(false);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Callback((StringBuilder o, Guid workspaceID) =>
            {
                retVal = o;
            });

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);
            mockEnvironmentModel.Setup(e => e.Connection.DisplayName).Returns("localhost");

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false)).Returns(MessageBoxResult.OK);
            CustomContainer.Register(mockPopupController.Object);

            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);
            var resourceData = BuildSerializableResourceFromName("Unsaved 1", "WorkflowService", true);

            var model = ResourceRepository.HydrateResourceModel(resourceData, Guid.Empty);
            Assert.IsNull(model);

            mockPopupController.Verify(manager => manager.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false), Times.Once);
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

            var resourceData = BuildResourceObjectFromGuids(new[] { _resourceGuid }, "Server");

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if (callCnt == 0)
                    {
                        callCnt = 1;
                        return new StringBuilder(payload);
                    }

                    return resourceData;
                });
            conn.Setup(c => c.ExecuteCommandAsync(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if (callCnt == 0)
                    {
                        callCnt = 1;
                        return Task.FromResult(new StringBuilder(payload));
                    }

                    return Task.FromResult(resourceData);
                });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
            _repo.Save(resourceModel.Object);
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
            Setup();

            AppSettings.LocalHost = "https://localhost:3242/";
            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);

            var conn = SetupConnection();

            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommandAsync(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() =>
                    {
                        if (callCnt == 0)
                        {
                            callCnt = 1;
                            return Task.FromResult(new StringBuilder(payload));
                        }

                        return Task.FromResult(BuildResourceObjectFromGuids(new[] { new Guid() }));
                    });
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() =>
                    {
                        if (callCnt == 0)
                        {
                            callCnt = 1;
                            return new StringBuilder(payload);
                        }

                        return BuildResourceObjectFromGuids(new[] { new Guid() });
                    });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
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
            conn.Setup(c => c.ExecuteCommandAsync(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if (callCnt == 0)
                    {
                        callCnt = 1;
                        return Task.FromResult(new StringBuilder(payload));
                    }

                    return Task.FromResult(new StringBuilder("<Payload><Service Name=\"TestWorkflowService1\", <= Bad payload</Payload>"));
                });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
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
            conn.Setup(c => c.ExecuteCommandAsync(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if (callCnt == 0)
                    {
                        callCnt = 1;
                        return Task.FromResult(new StringBuilder(payload));
                    }

                    return Task.FromResult(resultObj);
                });
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if (callCnt == 0)
                    {
                        callCnt = 1;
                        return new StringBuilder(payload);
                    }

                    return resultObj;
                });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
            _repo.Save(resourceModel.Object);
            _repo.ForceLoad();
            int resources = _repo.All().Count;
            //Assert
            Assert.IsTrue(resources.Equals(2));
            var resource = _repo.All().First();

            Assert.IsTrue(resource.ResourceType == Dev2.Studio.Core.AppResources.Enums.ResourceType.WorkflowService);
            Assert.IsTrue(resource.ResourceName == "TestWorkflowService");
        }

        private Mock<IEnvironmentConnection> SetupConnection()
        {
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri)
                .Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}/dsf"));
            conn.Setup(c => c.WebServerUri)
                .Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}"));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            return conn;
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
            model.Setup(c => c.ResourceType).Returns(Dev2.Studio.Core.AppResources.Enums.ResourceType.Source);
            //model.SetupGet(p => p.ResourceName).Returns("My WF");
            //model.SetupGet(p => p.Category).Returns("Root");
            var conn = SetupConnection();

            var resourceData = BuildResourceObjectFromGuids(new[] { _resourceGuid }, "Server");
            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if (callCnt <= 1)
                    {
                        callCnt++;
                        return new StringBuilder(payload);
                    }

                    return resourceData;
                });
            conn.Setup(c => c.ExecuteCommandAsync(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
               .Returns(() =>
               {
                   if (callCnt <= 1)
                   {
                       callCnt++;
                       return Task.FromResult(new StringBuilder(payload));
                   }

                   return Task.FromResult(resourceData);
               });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //Act
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            resourceModel.Setup(p => p.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
            model.Setup(p => p.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
            _repo.Save(resourceModel.Object);
            _repo.Save(model.Object);
            _repo.Load();
            //Assert
            Assert.AreEqual(1, _repo.All().Count);
        }

        [TestMethod]
        [TestCategory("ResourceRepository_Load")]
        [Description("ResourceRepository Load must only do one server call to retrieve all resources")]
        [Owner("Trevor Williams-Ros")]
        public void ResourceRepository_UnitTest_Load_InvokesAddResourcesOnce()
        {

            var envConnection = new Mock<IEnvironmentConnection>();
            envConnection.Setup(e => e.WorkspaceID).Returns(Guid.NewGuid());
            envConnection.Setup(e => e.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder());

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
            conn.Setup(c => c.ExecuteCommandAsync(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() => Task.FromResult(new StringBuilder(payload)));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            model.Setup(p => p.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
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
            resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
            //------------Execute Test---------------------------
            repo.Save(resourceModel.Object);
            //------------Assert Results-------------------------
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

            var commController = new Mock<ICommunicationController>();
            commController.Setup(m => m.ExecuteCommandAsync<ExecuteMessage>(It.IsAny<IEnvironmentConnection>(), It.IsAny<Guid>()))
                          .Returns(Task.FromResult(new ExecuteMessage
                          {
                              HasError = true
                          }));
            repo.GetCommunicationController = someName => commController.Object;

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
            //------------Execute Test---------------------------
            repo.Save(resourceModel.Object);
            //------------Assert Results-------------------------
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

            var commController = new Mock<ICommunicationController>();
            commController.Setup(m => m.ExecuteCommandAsync<ExecuteMessage>(It.IsAny<IEnvironmentConnection>(), It.IsAny<Guid>()))
                          .Returns(Task.FromResult(new ExecuteMessage
                          {
                              HasError = false
                          }));
            repo.GetCommunicationController = someName => commController.Object;

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("Unsaved");
            resourceModel.SetupGet(p => p.Category).Returns("Unassigned");
            resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
            //------------Execute Test---------------------------
            repo.Save(resourceModel.Object);
            //------------Assert Results-------------------------
        }

        #endregion Save Tests


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceRepository_OnDeleteFromWorkspace")]
        public void WorkFlowService_OnDeleteFromWorkspace_Expected_InRepository_UnsaveWF()
        {
            var retVal = new StringBuilder();
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Callback((StringBuilder o, Guid workspaceID) =>
            {
                retVal = o;
            });

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var myItem = new ResourceModel(mockEnvironmentModel.Object) { ResourceName = "Unsaved" };
            mockEnvironmentModel.Object.ResourceRepository.Add(myItem);
            int exp = mockEnvironmentModel.Object.ResourceRepository.All().Count;
            ResourceRepository.DeleteResourceFromWorkspace(myItem);
            Assert.AreEqual(exp, mockEnvironmentModel.Object.ResourceRepository.All().Count);
            var retMsg = JsonConvert.DeserializeObject<ExecuteMessage>(retVal.ToString());
            Assert.IsNotNull(retMsg);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModel_SaveTests")]
        public void ResourceModel_SaveTests_ExecuteMessageIsSuccessful_NoException()
        {
            //------------Setup for test--------------------------
            var serviceTestModel = new ServiceTestModelTO
            {
                TestName = "Test Input",
                AuthenticationType = AuthenticationType.Public,
                Enabled = true,
                ErrorExpected = false,
                NoErrorExpected = true,
                Inputs = new List<IServiceTestInput> { new ServiceTestInputTO { Variable = "var", Value = "val" } },
                Outputs = new List<IServiceTestOutput> { new ServiceTestOutputTO { Variable = "var", Value = "val" } },
                ResourceId = Guid.NewGuid(),
                TestSteps = new List<IServiceTestStep> { new ServiceTestStep(Guid.NewGuid(),"type",new ObservableCollection<IServiceTestOutput>(),StepType.Mock) }                
            };
            var retVal = new StringBuilder();
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Callback((StringBuilder o, Guid workspaceID) =>
            {
                retVal = o;
            });

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var resourceRepository = new ResourceRepository(mockEnvironmentModel.Object);
            var resourceId = Guid.NewGuid();
            //------------Execute Test--------------------------- 

            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ID).Returns(resourceId);
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));

            resourceRepository.SaveTests(resourceModel.Object, new List<IServiceTestModelTO> { serviceTestModel });
            //------------Assert Results-------------------------
            var ser = new Dev2JsonSerializer();
            var retMsg = ser.Deserialize<EsbExecuteRequest>(retVal.ToString());
            Assert.IsNotNull(retMsg);
            Assert.AreEqual("SaveTests", retMsg.ServiceName);
            Assert.AreEqual(3, retMsg.Args.Count);
            Assert.AreEqual(resourceId.ToString(), retMsg.Args["resourceID"].ToString());
            Assert.AreEqual("Root", retMsg.Args["resourcePath"].ToString());
            var compressedMessage = ser.Deserialize<CompressedExecuteMessage>(retMsg.Args["testDefinitions"].ToString());
            Assert.IsNotNull(compressedMessage);
            var serviceTestModelTos = ser.Deserialize<List<ServiceTestModelTO>>(compressedMessage.GetDecompressedMessage());
            Assert.IsNotNull(serviceTestModelTos);
            Assert.AreEqual(1, serviceTestModelTos.Count);
            Assert.AreEqual(serviceTestModel.TestName, serviceTestModelTos[0].TestName);
            Assert.AreEqual(serviceTestModel.TestSteps.Count,serviceTestModelTos[0].TestSteps.Count);
            Assert.AreEqual(1,serviceTestModelTos[0].TestSteps.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Savetests_GivenEmptyCategory_ShouldUseResourceName()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModelTO
            {
                TestName = "Test Input",
                AuthenticationType = AuthenticationType.Public,
                Enabled = true,
                ErrorExpected = false,
                NoErrorExpected = true,
                Inputs = new List<IServiceTestInput> { new ServiceTestInputTO { Variable = "var", Value = "val" } },
                Outputs = new List<IServiceTestOutput> { new ServiceTestOutputTO { Variable = "var", Value = "val" } },
                ResourceId = Guid.NewGuid(),
                TestSteps = new List<IServiceTestStep> { new ServiceTestStep(Guid.NewGuid(), "type", new ObservableCollection<IServiceTestOutput>(), StepType.Mock) }
            };
            var retVal = new StringBuilder();
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Callback((StringBuilder o, Guid workspaceID) =>
            {
                retVal = o;
            });

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var resourceRepository = new ResourceRepository(mockEnvironmentModel.Object);
            var resourceId = Guid.NewGuid();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ID).Returns(resourceId);
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns(string.Empty);
            resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));

            resourceRepository.SaveTests(resourceModel.Object, new List<IServiceTestModelTO> { serviceTestModel });
            //---------------Test Result -----------------------
            var ser = new Dev2JsonSerializer();
            var retMsg = ser.Deserialize<EsbExecuteRequest>(retVal.ToString());
            Assert.IsNotNull(retMsg);
            Assert.AreEqual("SaveTests", retMsg.ServiceName);
            Assert.AreEqual(3, retMsg.Args.Count);
            Assert.AreEqual(resourceId.ToString(), retMsg.Args["resourceID"].ToString());
            Assert.AreEqual("My WF", retMsg.Args["resourcePath"].ToString());
            var compressedMessage = ser.Deserialize<CompressedExecuteMessage>(retMsg.Args["testDefinitions"].ToString());
            Assert.IsNotNull(compressedMessage);
            var serviceTestModelTos = ser.Deserialize<List<ServiceTestModelTO>>(compressedMessage.GetDecompressedMessage());
            Assert.IsNotNull(serviceTestModelTos);
            Assert.AreEqual(1, serviceTestModelTos.Count);
            Assert.AreEqual(serviceTestModel.TestName, serviceTestModelTos[0].TestName);
            Assert.AreEqual(serviceTestModel.TestSteps.Count, serviceTestModelTos[0].TestSteps.Count);
            Assert.AreEqual(1, serviceTestModelTos[0].TestSteps.Count);
        }

       

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ResourceModel_ExecuteTest")]
        public void ResourceModel_ExecuteTest_ExecuteMessageIsSuccessful_NoException()
        {
            //------------Setup for test--------------------------
            var serviceTestModel = new ServiceTestModelTO()
            {
                TestName = "Test Input",
                AuthenticationType = AuthenticationType.Public,
                Enabled = true,
                ErrorExpected = false,
                NoErrorExpected = true,
                Inputs = new List<IServiceTestInput> { new ServiceTestInput("var", "val") },
                Outputs = new List<IServiceTestOutput> { new ServiceTestOutput("var", "val", "", "") },
                ResourceId = Guid.NewGuid()


            };
            var retVal = new StringBuilder();
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Callback((StringBuilder o, Guid workspaceID) =>
            {
                retVal = o;
            });

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);
            mockEnvironmentModel.Setup(e => e.IsConnected).Returns(true);

            var resourceRepository = new ResourceRepository(mockEnvironmentModel.Object);
            var resourceId = Guid.NewGuid();
            //------------Execute Test--------------------------- 

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.SetupGet(p => p.Environment).Returns(mockEnvironmentModel.Object);
            resourceModel.SetupGet(p => p.ID).Returns(resourceId);
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));

            resourceRepository.ExecuteTest(resourceModel.Object, serviceTestModel.TestName);
            //------------Assert Results-------------------------
            var ser = new Dev2JsonSerializer();
            var retMsg = ser.Deserialize<EsbExecuteRequest>(retVal.ToString());
            Assert.IsNotNull(retMsg);
        }

        [TestMethod]
        public void WorkFlowService_OnDeleteFromWorkspace_Expected_InRepository()
        {
            var retVal = new StringBuilder();
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Callback((StringBuilder o, Guid workspaceID) =>
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
            ResourceRepository.DeleteResourceFromWorkspace(myItem);
            Assert.AreEqual(exp, mockEnvironmentModel.Object.ResourceRepository.All().Count);
            var retMsg = JsonConvert.DeserializeObject<ExecuteMessage>(retVal.ToString());
            Assert.IsNotNull(retMsg);
        }
        [TestMethod]
        public void NonExistantWorkFlowService_OnDelete_Expected_Failure()
        {
            var env = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var myRepo = new ResourceRepository(env.Object);
            var myItem = new ResourceModel(env.Object);

            var actual = myRepo.DeleteResource(myItem);
            Assert.AreEqual("Failure", actual.Message.ToString(), "Non existant resource deleted successfully");
        }

        [TestMethod]
        public void NonExistantWorkFlowService_DeleteFromWorkspace_Expected_Failure()
        {
            var env = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var myRepo = new ResourceRepository(env.Object);
            var myItem = new ResourceModel(env.Object);

            var actual = myRepo.DeleteResourceFromWorkspace(myItem);
            Assert.AreEqual("Failure", actual.Message.ToString(), "Non existant resource deleted successfully");
        }


        [TestMethod]
        public void NullResource_DeleteFromWorkspace_Expected_Failure()
        {
            var env = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var myRepo = new ResourceRepository(env.Object);

            var actual = myRepo.DeleteResourceFromWorkspace(null);
            Assert.AreEqual("Failure", actual.Message.ToString(), "Non existant resource deleted successfully");
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
            msg.HasError = false;
            var payload = JsonConvert.SerializeObject(msg);

            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var myItem = new ResourceModel(mockEnvironmentModel.Object) { ResourceName = "TestResource", Category = string.Empty };
            ResourceRepository.Add(myItem);
            int expectedCount = mockEnvironmentModel.Object.ResourceRepository.All().Count;
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
            msg.HasError = false;
            var payload = JsonConvert.SerializeObject(msg);

            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var myItem = new ResourceModel(mockEnvironmentModel.Object) { ResourceName = "TestResource", Category = string.Empty };

            ResourceRepository.Add(myItem);
            int expectedCount = mockEnvironmentModel.Object.ResourceRepository.All().Count;

            mockEnvironmentModel.Object.ResourceRepository.DeleteResource(myItem);

            Assert.AreEqual(expectedCount - 1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
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
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var myItem = new ResourceModel(mockEnvironmentModel.Object) { ResourceName = "Unsaved 10" };

            ResourceRepository.Add(myItem);
            int expectedCount = mockEnvironmentModel.Object.ResourceRepository.All().Count;

            mockEnvironmentModel.Object.ResourceRepository.DeleteResource(myItem);

            Assert.AreEqual(expectedCount - 1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
        }

        [TestMethod]
        [TestCategory("ResourceRepository_Delete")]
        [Owner("Hagashen Naidu")]
        // ReSharper disable InconsistentNaming
        public void ResourceRepository_DeleteResource_ResourceNameNotUnsavedUnassignedCategory_StudioResourceRepositoryRemoveItemCalled()
        // ReSharper restore InconsistentNaming
        {

            var msg = new ExecuteMessage();
            msg.HasError = false;
            var payload = JsonConvert.SerializeObject(msg);

            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            mockEnvironmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var myItem = new ResourceModel(mockEnvironmentModel.Object) { ResourceName = "my resource", Category = "Unassigned" };

            ResourceRepository.Add(myItem);
            int expectedCount = mockEnvironmentModel.Object.ResourceRepository.All().Count;

            mockEnvironmentModel.Object.ResourceRepository.DeleteResource(myItem);

            Assert.AreEqual(expectedCount - 1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
        }


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
            conn.Setup(c => c.AppServerUri).Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}/dsf"));
            conn.Setup(c => c.WebServerUri).Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}"));
            conn.Setup(c => c.IsConnected).Returns(false);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(exePayload));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            try
            {
                _repo.Save(_resourceModel.Object);
                _repo.Load();
            }
            //Assert
            catch (Exception iex)
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
            conn.Setup(c => c.AppServerUri).Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}/dsf"));
            conn.Setup(c => c.WebServerUri).Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}"));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(exePayload));
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
            Setup();
            ExecuteMessage msg = new ExecuteMessage();
            var exePayload = JsonConvert.SerializeObject(msg);
            Mock<IEnvironmentConnection> environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(prop => prop.AppServerUri).Returns(new Uri("http://localhost:77/dsf"));
            environmentConnection.Setup(prop => prop.IsConnected).Returns(true);

            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}/dsf"));
            conn.Setup(c => c.WebServerUri).Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}"));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(exePayload));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.Save(_resourceModel.Object);
            _repo.Load();
            int resources = _repo.All().Count(res => res.ResourceName == "Resource");
            //Assert
            Assert.IsTrue(resources == 1);

        }

        #endregion Missing Environment Information Tests

        #region Resource Dependancies Tests

        static readonly XElement TestDependencyGraph = XmlResource.Fetch("DependenciesGraphUniqueTest");

        #region CreateResourceList

        static List<IContextualResourceModel> CreateResourceList(IEnvironmentModel environmentModel)
        {
            return new List<IContextualResourceModel>
            {
                new ResourceModel(environmentModel)
                {
                    ResourceName = "Button"
                },
            };
        }

        #endregion


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

            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(payload).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            testEnvironmentModel2.Setup(e => e.Connection).Returns(mockConnection.Object);

            var resRepo = new ResourceRepository(testEnvironmentModel2.Object);
            var testResources = new List<IResourceModel>(CreateResourceList(testEnvironmentModel2.Object));
            foreach (var resourceModel in testResources)
            {
                resRepo.Add(resourceModel);
            }

            testEnvironmentModel2.Setup(e => e.ResourceRepository).Returns(resRepo);

            resRepo.GetDependenciesXml(new ResourceModel(testEnvironmentModel2.Object) { ResourceName = "Button" }, false);
            mockConnection.Verify(e => e.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()), Times.Exactly(1));
        }

        #endregion GetDependanciesAsXML Tests

        #region GetUniqueDependancies Tests

        [TestMethod]
        public void GetUniqueDependenciesWithNullModel()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(TestDependencyGraph.ToString())).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            testEnvironmentModel2.Setup(e => e.Connection).Returns(mockConnection.Object);

            var resRepo = new ResourceRepository(testEnvironmentModel2.Object);
            var testResources = new List<IResourceModel>(CreateResourceList(testEnvironmentModel2.Object));
            foreach (var resourceModel in testResources)
            {
                resRepo.Add(resourceModel);
            }

            testEnvironmentModel2.Setup(e => e.ResourceRepository).Returns(resRepo);

            resRepo.GetUniqueDependencies(null);
            mockConnection.Verify(e => e.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()), Times.Exactly(0));
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

            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(payload).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            testEnvironmentModel2.Setup(e => e.Connection).Returns(mockConnection.Object);

            var resRepo = new ResourceRepository(testEnvironmentModel2.Object);
            var testResources = new List<IResourceModel>(CreateResourceList(testEnvironmentModel2.Object));
            foreach (var resourceModel in testResources)
            {
                resRepo.Add(resourceModel);
            }

            testEnvironmentModel2.Setup(e => e.ResourceRepository).Returns(resRepo);

            resRepo.GetUniqueDependencies(new ResourceModel(testEnvironmentModel2.Object) { ResourceName = "Button" });
            mockConnection.Verify(e => e.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()), Times.Exactly(1));
        }

        [TestMethod]
        public void GetUniqueDependenciesWithModelReturnsEmptyListWhenNoResourcesMatch()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();

            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage(TestDependencyGraph.ToString());
            var payload = new StringBuilder(JsonConvert.SerializeObject(msg));

            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(payload).Verifiable();
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
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(() =>
            {
                if (cnt == 0)
                {
                    cnt = 1;
                    return new StringBuilder(JsonConvert.SerializeObject(msg));
                }

                return resourceObj;
            });
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            //------------Execute Test---------------------------
            var reloadedResources = _repo.ReloadResource(_resourceGuid, Dev2.Studio.Core.AppResources.Enums.ResourceType.WorkflowService, new ResourceModelEqualityComparerForTest(), false);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, reloadedResources.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceRepository_UpdateResourceRepositoryForDeploy")]
        public void ResourceRepository_UpdateResourceRepositoryForDeploy_FolderDoesNotExist_ExpectAddFolderAndResource()
        {
            //------------Setup for test--------------------------

            AppSettings.LocalHost = "http://localhost:3142/";


            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();

            var resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid, newGuid }, cat: "bpb\\TestWorkflowService");

            ExecuteMessage msg = new ExecuteMessage { HasError = false };

            int cnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(() =>
            {
                if (cnt == 0)
                {
                    cnt = 1;
                    return new StringBuilder(JsonConvert.SerializeObject(msg));
                }

                return resourceObj;
            });
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            var reloadedResources = _repo.ReloadResource(_resourceGuid, Dev2.Studio.Core.AppResources.Enums.ResourceType.WorkflowService, new ResourceModelEqualityComparerForTest(), false);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, reloadedResources.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceRepository_UpdateResourceRepositoryForDeploy")]
        public void ResourceRepository_UpdateResourceRepositoryForDeploy_OnRoot_ExpectNoAddOfFolder()
        {
            //------------Setup for test--------------------------

            AppSettings.LocalHost = "http://localhost:3142/";


            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();

            var resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid, newGuid }, cat: "TestWorkflowService");

            ExecuteMessage msg = new ExecuteMessage { HasError = false };

            int cnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(() =>
            {
                if (cnt == 0)
                {
                    cnt = 1;
                    return new StringBuilder(JsonConvert.SerializeObject(msg));
                }

                return resourceObj;
            });
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            var reloadedResources = _repo.ReloadResource(_resourceGuid, Dev2.Studio.Core.AppResources.Enums.ResourceType.WorkflowService, new ResourceModelEqualityComparerForTest(), false);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, reloadedResources.Count);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceRepository_UpdateResourceRepositoryForDeploy")]
        public void ResourceRepository_UpdateResourceRepositoryForDeploy_FolderDoesNotExist_Nested_ExpectAddFolderAndResource()
        {
            //------------Setup for test--------------------------

            AppSettings.LocalHost = "http://localhost:3142/";


            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();

            var resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid, newGuid }, cat: "bob\\dave\\dora");

            ExecuteMessage msg = new ExecuteMessage { HasError = false };

            int cnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(() =>
            {
                if (cnt == 0)
                {
                    cnt = 1;
                    return new StringBuilder(JsonConvert.SerializeObject(msg));
                }

                return resourceObj;
            });
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            var reloadedResources = _repo.ReloadResource(_resourceGuid, Dev2.Studio.Core.AppResources.Enums.ResourceType.WorkflowService, new ResourceModelEqualityComparerForTest(), false);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, reloadedResources.Count);
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

            var resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid }, "WorkflowService", errors);

            var msg = new ExecuteMessage { HasError = false };


            int cnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(

                () =>
                {
                    if (cnt == 0)
                    {
                        cnt = 1;
                        return new StringBuilder(JsonConvert.SerializeObject(msg));
                    }

                    return resourceObj;
                }
            );

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            var reloadedResources = _repo.ReloadResource(_resourceGuid, Dev2.Studio.Core.AppResources.Enums.ResourceType.WorkflowService, new ResourceModelEqualityComparerForTest(), false);

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

            var resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid }, "WorkflowService", errors);

            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(() => resourceObj);

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            _repo.LoadResourceFromWorkspace(_resourceGuid, Guid.Empty);

            //------------Assert Results-------------------------
            var resources = _repo.All().ToList();
            var actual = (IContextualResourceModel)resources[0];
            Assert.AreEqual(_resourceGuid, actual.ID);
            Assert.AreEqual(true, actual.IsValid);
        }

        #endregion ReloadResource Tests

        #region FindResourcesByID

        [TestMethod]
        public void FindResourcesByID_With_NullParameters_Expected_ReturnsEmptyList()
        {
            ResourceRepository resourceRepository = GetResourceRepository();
            var result = resourceRepository.FindResourcesByID(null, null, Dev2.Studio.Core.AppResources.Enums.ResourceType.Source);
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

            var result = resourceRepository.FindResourcesByID(env.Object, servers, Dev2.Studio.Core.AppResources.Enums.ResourceType.Source);

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
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            //------------Execute Test---------------------------
            var result = new ResourceRepository(env.Object).FetchResourceDefinition(env.Object, Guid.Empty, modelID, false);

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

            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            //------------Execute Test---------------------------
            var result = new ResourceRepository(env.Object).FetchResourceDefinition(env.Object, Guid.Empty, modelID, false);

            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, result.Message.ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ResourceRepository_LoadResourceTests")]
        public void ResourceRepository_LoadResourceTests_WhenNoTestsToFetch_ExpectNothing()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();
            con.Setup(c => c.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);



            var serviceTestModel = new List<IServiceTestModelTO>();
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var payload = jsonSerializer.Serialize(serviceTestModel);
            CompressedExecuteMessage message = new CompressedExecuteMessage();
            message.SetMessage(payload);
            var msgResult = jsonSerializer.Serialize(message);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(msgResult.ToStringBuilder);

            //------------Execute Test---------------------------
            var result = new ResourceRepository(env.Object);
            var serviceTestModels = result.LoadResourceTests(Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(0, serviceTestModels.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ResourceRepository_LoadResourceTestsForDeploy")]
        public void ResourceRepository_LoadResourceTestsForDeploy_WhenNoTestsToFetch_ExpectNothing()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();
            con.Setup(c => c.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);



            var serviceTestModel = new List<IServiceTestModelTO>();
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var payload = jsonSerializer.Serialize(serviceTestModel);
            CompressedExecuteMessage message = new CompressedExecuteMessage();
            message.SetMessage(payload);
            var msgResult = jsonSerializer.Serialize(message);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(msgResult.ToStringBuilder);

            //------------Execute Test---------------------------
            var result = new ResourceRepository(env.Object);
            var serviceTestModels = result.LoadResourceTestsForDeploy(Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(0, serviceTestModels.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ResourceRepository_DeleteResourceTests")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceRepository_DeleteResourceTests_WhenNoTestNameIsNull_ExpectNothing()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();
            con.Setup(c => c.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);



            var serviceTestModel = new ServiceTestModelTO();
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var payload = jsonSerializer.Serialize(serviceTestModel);
            CompressedExecuteMessage message = new CompressedExecuteMessage();
            message.SetMessage(payload);
            var msgResult = jsonSerializer.Serialize(message);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(msgResult.ToStringBuilder);

            //------------Execute Test---------------------------
            var result = new ResourceRepository(env.Object);
            result.DeleteResourceTest(Guid.NewGuid(), It.IsAny<string>());
            //------------Assert Results-------------------------
           
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ResourceRepository_DeleteResourceTests")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceRepository_DeleteResourceTests_WhenNoResourceIdIsNull_ExpectNothing()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();
            con.Setup(c => c.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);



            var serviceTestModel = new ServiceTestModelTO();
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var payload = jsonSerializer.Serialize(serviceTestModel);
            CompressedExecuteMessage message = new CompressedExecuteMessage();
            message.SetMessage(payload);

            var msgResult = jsonSerializer.Serialize(message);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(msgResult.ToStringBuilder);

            //------------Execute Test---------------------------
            var result = new ResourceRepository(env.Object);
            result.DeleteResourceTest(Guid.Empty, "Name");
            //------------Assert Results-------------------------
          
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ResourceRepository_DeleteResourceTests")]
        public void ResourceRepository_DeleteResourceTests_WhenResultHasError_ExpectNothing()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();
            con.Setup(c => c.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);



            var msg = new StringBuilder("Error occured");
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var payload = jsonSerializer.Serialize(msg);
            CompressedExecuteMessage message = new CompressedExecuteMessage();
            message.SetMessage(payload);
            message.HasError = true;

            var msgResult = jsonSerializer.Serialize(message);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(msgResult.ToStringBuilder);

            //------------Execute Test---------------------------
            try
            {
                var result = new ResourceRepository(env.Object);
                result.DeleteResourceTest(Guid.NewGuid(), "Name");
                Assert.Fail("Exception not thrown");
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual("Error occured", ex.Message);
            }

            //------------Assert Results-------------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ResourceRepository_LoadResourceTests")]
        public void ResourceRepository_LoadResourceTests_WhenTestFound_ExpectTestsBack()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();
            con.Setup(c => c.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);

            var serviceTestModel = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    TestName = "Test 1",
                    AuthenticationType = AuthenticationType.Windows,
                    Inputs = new List<IServiceTestInput>(),
                    Outputs = new List<IServiceTestOutput>(),
                    UserName = "nathi",
                    Password = "pass.word1"
                }
            };
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var payload = jsonSerializer.Serialize(serviceTestModel);
            CompressedExecuteMessage message = new CompressedExecuteMessage();
            message.SetMessage(payload);
            var msgResult = jsonSerializer.Serialize(message);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(msgResult.ToStringBuilder);

            //------------Execute Test---------------------------
            var result = new ResourceRepository(env.Object);
            var serviceTestModels = result.LoadResourceTests(Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestModels.ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ResourceRepository_LoadResourceTestsForDeploy")]
        public void ResourceRepository_LoadResourceTestsForDeploy_WhenTestFound_ExpectTestsBack()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();
            con.Setup(c => c.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);
            const string plainText = "pass.word1";
            var decrypt = SecurityEncryption.Encrypt(plainText);
            var serviceTestModel = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    TestName = "Test 1",
                    AuthenticationType = AuthenticationType.Windows,
                    Inputs = new List<IServiceTestInput>(),
                    Outputs = new List<IServiceTestOutput>(),
                    UserName = "nathi",
                    Password = decrypt
                }
            };
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var payload = jsonSerializer.Serialize(serviceTestModel);
            CompressedExecuteMessage message = new CompressedExecuteMessage();
            message.SetMessage(payload);
            var msgResult = jsonSerializer.Serialize(message);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(msgResult.ToStringBuilder);

            //------------Execute Test---------------------------
            var result = new ResourceRepository(env.Object);
            var serviceTestModels = result.LoadResourceTestsForDeploy(Guid.NewGuid());
            //------------Assert Results-------------------------
            var isEncryted = serviceTestModels.Single().Password == decrypt;
            var old = SecurityEncryption.Decrypt(serviceTestModels.Single().Password);

            Assert.IsTrue(isEncryted);
            Assert.IsTrue(old.Contains(plainText));
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ResourceRepository_LoadResourceTests")]
        public void ResourceRepository_LoadResourceTests_WhenError_ExpectException()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();
            con.Setup(c => c.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);

            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            CompressedExecuteMessage message = new CompressedExecuteMessage { HasError = true };
            var stringBuilder = new StringBuilder("An error occured");
            message.SetMessage(jsonSerializer.Serialize(stringBuilder));
            var msgResult = jsonSerializer.Serialize(message);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(msgResult.ToStringBuilder);

            //------------Execute Test---------------------------
            var result = new ResourceRepository(env.Object);
            try
            {
                result.LoadResourceTests(Guid.NewGuid());
            }
            catch (Exception ex)
            {
                //------------Assert Results-------------------------
                Assert.AreEqual("An error occured", ex.Message);
            }

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ResourceRepository_LoadResourceTestsForDeploy")]
        public void ResourceRepository_LoadResourceTestsForDeploy_WhenError_ExpectException()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();
            con.Setup(c => c.IsConnected).Returns(true);
            env.Setup(e => e.Connection).Returns(con.Object);

            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            CompressedExecuteMessage message = new CompressedExecuteMessage { HasError = true };
            var stringBuilder = new StringBuilder("An error occured");
            message.SetMessage(jsonSerializer.Serialize(stringBuilder));
            var msgResult = jsonSerializer.Serialize(message);
            con.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(msgResult.ToStringBuilder);

            //------------Execute Test---------------------------
            var result = new ResourceRepository(env.Object);
            try
            {
                result.LoadResourceTestsForDeploy(Guid.NewGuid());
            }
            catch (Exception ex)
            {
                //------------Assert Results-------------------------
                Assert.AreEqual("An error occured", ex.Message);
            }

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

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetResourceList_GivenDropboxSource_ShouldReturnDroboxSources()
        {
            //---------------Set up test pack-------------------
            var res = new DropBoxSource() { ResourceID = Guid.NewGuid() };
            var resList = new List<DropBoxSource> { res };
            var src = JsonConvert.SerializeObject(resList);
            var env = EnviromentRepositoryTest.CreateMockEnvironment(true, src);
            ResourceRepository resourceRepository = GetResourceRepository();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IList<DropBoxSource> result = resourceRepository.GetResourceList<DropBoxSource>(env.Object);
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetResourceList_GivenDropboxSource_ShouldCreateCorrectServiceName()
        {
            //---------------Set up test pack-------------------
            ResourceRepository resourceRepository = GetResourceRepository();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(resourceRepository);
            var invoke = privateObject.Invoke("CreateServiceName", typeof(DropBoxSource));
            //---------------Test Result -----------------------
            var serviceName = invoke.ToString();
            Assert.AreEqual("FetchDropBoxSources".ToString(CultureInfo.CurrentCulture), serviceName.ToString(CultureInfo.CurrentCulture));

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

            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(BuildResourceObjectFromGuids(new[] { newGuid }));
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
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(new StringBuilder($"<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{_resourceGuid}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{guid2}\"></Service></Payload>"));
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

            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(BuildResourceObjectFromGuids(new[] { newGuid }));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();

            const string modelDefinition = "model definition";
            var msg = MakeMsg(modelDefinition);
            var payload = JsonConvert.SerializeObject(msg);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload));

            //------------Execute Test---------------------------
            var result = _repo.FindSingle(model => model.ID == newGuid, true);

            //------------Assert Results-------------------------
            Assert.IsNotNull(result.WorkflowXaml);
            StringAssert.Contains(result.WorkflowXaml.ToString(), modelDefinition);

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


            var resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid, newGuid }, "Server");

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommandAsync(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() =>
        {
            if (callCnt == 0)
            {
                callCnt = 1;
                return Task.FromResult(new StringBuilder(payload));
            }

            return Task.FromResult(resourceObj);
        });
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() =>
        {
            if (callCnt == 0)
            {
                callCnt = 1;
                return new StringBuilder(payload);
            }

            return resourceObj;
        });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            //------------Execute Test---------------------------
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
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

            var resourceData = BuildResourceObjectFromGuids(new[] { id }, "WorkflowService", errors, false);

            var msg = new ExecuteMessage();
            var payload = JsonConvert.SerializeObject(msg);
            int callCnt = 0;
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if (callCnt == 0)
                    {
                        callCnt = 1;
                        return new StringBuilder(payload);
                    }

                    return resourceData;
                });
            conn.Setup(c => c.ExecuteCommandAsync(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(() =>
                {
                    if (callCnt == 0)
                    {
                        callCnt = 1;
                        return Task.FromResult(new StringBuilder(payload));
                    }

                    return Task.FromResult(resourceData);
                });

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            //------------Execute Test---------------------------
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.SetupGet(p => p.ResourceName).Returns("My WF");
            resourceModel.SetupGet(p => p.Category).Returns("Root");
            resourceModel.Setup(model => model.ToServiceDefinition(It.IsAny<bool>())).Returns(new StringBuilder("SomeXaml"));
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

            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(resourceObj);
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            var guid = Guid.NewGuid();

            resourceObj = BuildResourceObjectFromGuids(new[] { _resourceGuid, guid });

            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(resourceObj);
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
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
                .Returns(new StringBuilder($"<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{_resourceGuid}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{guid2}\"></Service></Payload>"));
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            var guid = Guid.NewGuid();
            guid2 = guid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()))
             .Returns(new StringBuilder($"<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{_resourceGuid}\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{guid2}\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>"));
            _repo.ForceLoad();
            //--------------------------------------------Execute--------------------------------------------------------------
            var isInCache = _repo.IsInCache(Guid.NewGuid());
            //--------------------------------------------Assert Results----------------------------------------------------
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
            srcRepo.Setup(sr => sr.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false))
                   .Returns(_resourceModel.Object);

            targetModel.Setup(tm => tm.ResourceRepository).Returns(targetRepo.Object);
            targetRepo.Setup(tr => tr.ReloadResource(It.IsAny<Guid>(), It.IsAny<Dev2.Studio.Core.AppResources.Enums.ResourceType>(), ResourceModelEqualityComparer.Current, It.IsAny<bool>())).Verifiable();

            IList<IResourceModel> deployModels = new List<IResourceModel>();

            var theModel = new ResourceModel(srcModel.Object) { ID = theID };
            deployModels.Add(theModel);

            Mock<IEventAggregator> mockEventAg = new Mock<IEventAggregator>();
            mockEventAg.Setup(m => m.Publish(It.IsAny<object>()));

            IDeployDto dto = new DeployDto { ResourceModels = deployModels };

            //------------Execute Test---------------------------
            _repo.DeployResources(srcModel.Object, targetModel.Object, dto);

            //------------Assert Results-------------------------
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
            srcRepo.Setup(sr => sr.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false))
                   .Returns(_resourceModel.Object);

            targetModel.Setup(tm => tm.ResourceRepository).Returns(targetRepo.Object);
            targetRepo.Setup(tr => tr.ReloadResource(It.IsAny<Guid>(), It.IsAny<Dev2.Studio.Core.AppResources.Enums.ResourceType>(), ResourceModelEqualityComparer.Current, It.IsAny<bool>())).Verifiable();

            var theModel = new ResourceModel(srcModel.Object) { ID = theID, ResourceName = "some resource" };
            Mock<IEventAggregator> mockEventAg = new Mock<IEventAggregator>();
            mockEventAg.Setup(m => m.Publish(It.IsAny<object>()));

            //------------Execute Test---------------------------
            _repo.DeployResource(theModel, It.IsAny<string>());
            //------------Assert Results-------------------------
            _environmentConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>()));
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
            srcRepo.Setup(sr => sr.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false))
                  .Returns(findModel);

            srcEnvModel.Setup(sm => sm.ResourceRepository).Returns(srcRepo.Object);

            targetRepo.Setup(tr => tr.ReloadResource(It.IsAny<Guid>(), It.IsAny<Dev2.Studio.Core.AppResources.Enums.ResourceType>(), ResourceModelEqualityComparer.Current, It.IsAny<bool>())).Verifiable();
            targetEnvModel.Setup(tm => tm.ResourceRepository).Returns(targetRepo.Object);

            Mock<IResourceModel> reloadedResource = new Mock<IResourceModel>();
            reloadedResource.Setup(res => res.ResourceName).Returns("Resource");
            reloadedResource.Setup(res => res.DisplayName).Returns("My New Resource");
            reloadedResource.Setup(res => res.ID).Returns(theID);
            reloadedResource.Setup(res => res.WorkflowXaml).Returns(new StringBuilder("NewXaml"));

            List<IResourceModel> reloadResources = new List<IResourceModel> { reloadedResource.Object };


            targetRepo.Setup(tr => tr.ReloadResource(It.IsAny<Guid>(), It.IsAny<Dev2.Studio.Core.AppResources.Enums.ResourceType>(), ResourceModelEqualityComparer.Current, It.IsAny<bool>())).Returns(reloadResources);

            IList<IResourceModel> deployModels = new List<IResourceModel>();

            var theModel = new ResourceModel(srcEnvModel.Object) { ID = theID };
            deployModels.Add(theModel);

            Mock<IEventAggregator> mockEventAg = new Mock<IEventAggregator>();
            mockEventAg.Setup(m => m.Publish(It.IsAny<object>()));

            IDeployDto dto = new DeployDto { ResourceModels = deployModels };

            //------------Execute Test---------------------------
            _repo.DeployResources(srcEnvModel.Object, targetEnvModel.Object, dto);

            //------------Assert Results-------------------------

        }

        private void Marshal(System.Action action)
        {
            
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

            repo.DeployResource(null, It.IsAny<string>());
        }



        #endregion

        #region Helper Methods


        private SerializableResource BuildSerializableResourceFromName(string name, string typeOf, bool isNewResource = false)
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
        /// Builds the resource object.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="theType">The type.</param>
        /// <param name="errors"></param>
        /// <param name="isValid"></param>
        /// <param name="cat"></param>
        /// <returns></returns>
        private StringBuilder BuildResourceObjectFromGuids(IEnumerable<Guid> ids, string theType = "WorkflowService", List<ErrorInfo> errors = null, bool isValid = true, string cat = "Test Category")
        {
            if (errors == null)
            {
                errors = new List<ErrorInfo>();
            }

            var theResources = ids.Select(id => new SerializableResource
            {
                ResourceCategory = cat,
                DataList = "",
                Errors = errors,
                IsValid = isValid,
                ResourceID = id,
                IsService = true,
                ResourceName = "TestWorkflowService",
                ResourceType = theType
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


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceRepository_HasDependencies")]
        public void FindResourcesByID_HasDependencies_ExpectFalse()
        {
            ResourceRepository resourceRepository = GetResourceRepository();
            var result = resourceRepository.HasDependencies(new Mock<IContextualResourceModel>().Object);
            Assert.IsFalse(result);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceRepository_HasDependencies")]
        public void FindResourcesByID_HasDependencies_ExpectFalseIfSelf()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();

            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage(TestDependencyGraph.ToString());
            var payload = new StringBuilder(JsonConvert.SerializeObject(msg));

            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(payload).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            testEnvironmentModel2.Setup(e => e.Connection).Returns(mockConnection.Object);

            var resRepo = new ResourceRepository(testEnvironmentModel2.Object);
            var testResources = new List<IResourceModel>(CreateResourceList(testEnvironmentModel2.Object));
            foreach (var resourceModel in testResources)
            {
                resRepo.Add(resourceModel);
            }

            testEnvironmentModel2.Setup(e => e.ResourceRepository).Returns(resRepo);


            var res = resRepo.HasDependencies(new ResourceModel(testEnvironmentModel2.Object) { ResourceName = "Button" });
            Assert.IsFalse(res);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceRepository_ReceivePermissionsModified")]
        public void ResourceRepository_ReceivePermissionsModified_HasDependencies_ExpectFalseIfSelf()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();

            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage(TestDependencyGraph.ToString());
            var payload = new StringBuilder(JsonConvert.SerializeObject(msg));

            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(payload).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            var auth = new Mock<IAuthorizationService>();
            auth.Setup(a => a.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);
            var mockSecurityService = new Mock<ISecurityService>();
            mockSecurityService.Setup(service => service.Permissions).Returns(new ReadOnlyCollection<WindowsGroupPermission>(new List<WindowsGroupPermission>()));
            auth.Setup(a => a.SecurityService).Returns(mockSecurityService.Object);
            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            testEnvironmentModel2.Setup(e => e.Connection).Returns(mockConnection.Object);
            testEnvironmentModel2.Setup(a => a.AuthorizationService).Returns(auth.Object);
            var resRepo = new ResourceRepository(testEnvironmentModel2.Object);
            var testResources = new List<IResourceModel>(CreateResourceList(testEnvironmentModel2.Object));
            foreach (var resourceModel in testResources)
            {
                resourceModel.ID = Guid.NewGuid();
                resRepo.Add(resourceModel);
            }

            testEnvironmentModel2.Setup(e => e.ResourceRepository).Returns(resRepo);

            WindowsGroupPermission perm = new WindowsGroupPermission { ResourceID = testResources.First().ID };
            PrivateObject p = new PrivateObject(resRepo);
            p.Invoke("ReceivePermissionsModified", new List<WindowsGroupPermission> { perm });
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceRepository_ReceivePermissionsModified")]
        public void ResourceRepository_ReceivePermissionsModified_ExpectUpdateIfNonEmptyGuid()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();

            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage(TestDependencyGraph.ToString());
            var payload = new StringBuilder(JsonConvert.SerializeObject(msg));

            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(payload).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            var auth = new Mock<IAuthorizationService>();
            auth.Setup(a => a.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);
            var mockSecurityService = new Mock<ISecurityService>();
            mockSecurityService.Setup(service => service.Permissions).Returns(new ReadOnlyCollection<WindowsGroupPermission>(new List<WindowsGroupPermission>()));
            auth.Setup(a => a.SecurityService).Returns(mockSecurityService.Object);
            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            testEnvironmentModel2.Setup(e => e.Connection).Returns(mockConnection.Object);
            testEnvironmentModel2.Setup(a => a.AuthorizationService).Returns(auth.Object);
            var resRepo = new ResourceRepository(testEnvironmentModel2.Object);
            var res = new Mock<IResourceModel>();
            res.Setup(a => a.ID).Returns(Guid.NewGuid());
            var testResources = new List<IResourceModel> { res.Object };
            foreach (var resourceModel in testResources)
            {
                resRepo.Add(resourceModel);
            }

            testEnvironmentModel2.Setup(e => e.ResourceRepository).Returns(resRepo);

            WindowsGroupPermission perm = new WindowsGroupPermission { ResourceID = testResources.First().ID };
            PrivateObject p = new PrivateObject(resRepo);
            p.Invoke("ReceivePermissionsModified", new List<WindowsGroupPermission> { perm });
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceRepository_ReceivePermissionsModified")]
        public void ResourceRepository_ReceivePermissionsModified_ExpectNotUpdateIfEmptyGuidWithResourcePerm()
        {
            var mockConnection = new Mock<IEnvironmentConnection>();

            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage(TestDependencyGraph.ToString());
            var payload = new StringBuilder(JsonConvert.SerializeObject(msg));
            var auth = new Mock<IAuthorizationService>();
            auth.Setup(a => a.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);
            var mockSecurityService = new Mock<ISecurityService>();

            var testEnvironmentModel2 = new Mock<IEnvironmentModel>();
            testEnvironmentModel2.Setup(e => e.Connection).Returns(mockConnection.Object);
            testEnvironmentModel2.Setup(a => a.AuthorizationService).Returns(auth.Object);
            var resRepo = new ResourceRepository(testEnvironmentModel2.Object);

            var res = new Mock<IResourceModel>();
            res.Setup(a => a.ID).Returns(Guid.NewGuid());
            var testResources = new List<IResourceModel> { res.Object };
            foreach (var resourceModel in testResources)
            {
                resRepo.Add(resourceModel);
            }

            WindowsGroupPermission perm = new WindowsGroupPermission { ResourceID = testResources.First().ID };
            WindowsGroupPermission perm2 = new WindowsGroupPermission { ResourceID = Guid.Empty, IsServer = true, Permissions = Permissions.Execute };

            mockSecurityService.Setup(service => service.Permissions).Returns(new ReadOnlyCollection<WindowsGroupPermission>(new List<WindowsGroupPermission> { perm }));
            auth.Setup(a => a.SecurityService).Returns(mockSecurityService.Object);

            mockConnection.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(payload).Verifiable();
            mockConnection.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            mockConnection.Setup(c => c.IsConnected).Returns(true);

            testEnvironmentModel2.Setup(e => e.ResourceRepository).Returns(resRepo);

            PrivateObject p = new PrivateObject(resRepo);
            p.Invoke("ReceivePermissionsModified", new List<WindowsGroupPermission> { perm, perm2 });
        }

        //Create resource repository without connected to any environment
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceRepository_SaveToServer")]
        public void ResourceRepository_SaveToServer_ExpectThatModelAdded()
        {
            //Arrange
            Setup();

            ExecuteMessage msg = new ExecuteMessage();
            var exePayload = JsonConvert.SerializeObject(msg);

            _environmentConnection.Setup(envConn => envConn.IsConnected).Returns(false);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}/dsf"));
            conn.Setup(c => c.WebServerUri).Returns(new Uri($"http://127.0.0.{rand.Next(1, 100)}:{rand.Next(1, 100)}"));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(exePayload));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.SaveToServer(_resourceModel.Object);
            PrivateObject p = new PrivateObject(_repo);
            Assert.AreEqual(1, ((List<IResourceModel>)p.GetField("_resourceModels")).Count);
        }

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void GetDatabaseTableColumns_DBTableHasSchema_ShouldAddSchemaToPayload()
        {
            //Arrange
            Setup();
            var conn = SetupConnection();

            StringBuilder sentPayLoad = new StringBuilder();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Callback((StringBuilder o, Guid g1) =>
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

        static ExecuteMessage MakeMsg(string msg)
        {
            var executeMessage = new ExecuteMessage();
            executeMessage.SetMessage(msg);
            var exePayload = JsonConvert.SerializeObject(executeMessage);
            return executeMessage;
        }

        static CompressedExecuteMessage MakeCompressedMsg(string msg)
        {
            var result = new CompressedExecuteMessage { HasError = false };
            var executeMessage = new ExecuteMessage();
            executeMessage.SetMessage(msg);
            var exePayload = JsonConvert.SerializeObject(executeMessage);
            result.SetMessage(exePayload);
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
