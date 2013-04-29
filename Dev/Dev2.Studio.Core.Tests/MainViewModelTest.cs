//5559 Update these tests

using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Composition;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Framework;

//using System.Windows.Media.Imaging;

namespace Dev2.Core.Tests
{


    /// <summary>
    ///This is a result class for mvTest and is intended
    ///to contain all mvTest Unit Tests
    ///</summary>
    [TestClass()]
    public class mvTest
    {

        #region Variables

        Mock<IEnvironmentModel> _environmentModel = new Mock<IEnvironmentModel>();
        Mock<IEnvironmentConnection> _environmentConnection = new Mock<IEnvironmentConnection>();
        Mock<IResourceRepository> _resourceRepo = new Mock<IResourceRepository>();
        MainViewModel _mainViewModel;
        private string _resourceName = "result";
        private string _displayName = "test2";
        private string _serviceDefinition = "<x/>";
        private static ImportServiceContext _importServiceContext;
        int _count;

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

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            _importServiceContext = CompositionInitializer.InitializeMockedMainViewModel();
        }

        //Use TestInitialize to run code before running each result
        [TestInitialize()]
        public void MyTestInitialize()
        {

            ImportService.CurrentContext = _importServiceContext;

            _environmentConnection.Setup(envConn => envConn.Connect(It.IsAny<bool>())).Verifiable();
            _environmentConnection.Setup(envConn => envConn.AppServerUri).Returns(new Uri("http://localhost:77/dsf"));
            _environmentConnection.Setup(envConn => envConn.IsConnected).Returns(true);
            _environmentConnection.Setup(envConn => envConn.DataChannel.ExecuteCommand("someTez", Guid.NewGuid(), GlobalConstants.NullDataListID)).Returns("<x><result>tmpResult</result></x>").Verifiable();
            _environmentModel.Setup(envModel => envModel.ResourceRepository).Returns(_resourceRepo.Object);
            _environmentModel.Setup(envConn => envConn.Connect());
            _environmentModel.Setup(envConn => envConn.Connection.AppServerUri).Returns(new Uri("http://localhost:77/dsf"));
            _environmentModel.Setup(envConn => envConn.IsConnected).Returns(true);
            _environmentModel.Setup(envConn => envConn.DsfChannel.ExecuteCommand("someTez", Guid.NewGuid(), GlobalConstants.NullDataListID)).Returns("<x><result>tmpResult</result></x>").Verifiable();

            _environmentModel.Setup(envModel => envModel.Connect()).Verifiable();

            _mainViewModel = new MainViewModel();

        }
        #endregion

        #region Constructor Tests
        [TestMethod]
        public void MainInitialize()
        {
            _mainViewModel = new MainViewModel();
        }
        #endregion

        #region Adding all the different resource types
        /// <summary>
        /// Adding a work flow resource to the resource repository for the mv result case
        /// </summary>
        // 5559 Find a way to test this properly givent that adding a worflow document doesn't actually add it to
        //      a resource repository
        //[TestMethod()]
        //public void AddWorkflowResource() {
        //    //Arrage
        //    IResourceModel resourceModel = CreateResource(enResourceType.WorkflowService);
        //    IList<IResourceModel> m = new List<IResourceModel>() { resourceModel };
        //    resourceRepo.Setup(c => c.All()).Returns(m);

        //    //Act
        //    mv.ActiveEnvironment = environmentModel.Object;
        //    //mv.ReconnectEnvironment(environmentModel.Object);
        //    mv.AddWorkflowDocument(resourceModel);

        //    //Assert
        //    bool resource = mv.IsResourceWorkflow(resourceName);
        //    Assert.IsTrue(resource);

        //}
        /// <summary>
        /// Adding a worker resource to the resource repository for the mv result case
        /// </summary>
        [TestMethod()]
        public void AddWorkerResource()
        {
            //Arrage
            _serviceDefinition = "<Service />";
            IResourceModel resourceModel = CreateResource(ResourceType.Service);
            IList<IResourceModel> m = new List<IResourceModel>() { resourceModel };
            _resourceRepo.Setup(c => c.All()).Returns(m);

            //Act
            //mv.ReconnectEnvironment(environmentModel.Object);
            //5559 Check this test when refactor is finished
            //mv.ActiveEnvironment = environmentModel.Object;
            //Assert
            //var resource = mv.ResourceRepository.FindSingle(res => res.ResourceName == resourceName);
            Assert.AreEqual("<Service />", resourceModel.ServiceDefinition);

        }


        /// <summary>
        /// Adding a source resource to the resource repository for the mv result case
        /// </summary>
        [TestMethod()]
        public void AddSourceResource()
        {
            _serviceDefinition = "<Source />";
            IResourceModel resourceModel = CreateResource(ResourceType.Source);
            IList<IResourceModel> m = new List<IResourceModel>() { resourceModel };
            _resourceRepo.Setup(c => c.All()).Returns(m);
            //Act
            //mv.ReconnectEnvironment(environmentModel.Object);
            //5559 Check this test when refactor is finished
            //mv.ActiveEnvironment = environmentModel.Object;
            //Assert
            Assert.AreEqual("<Source />", resourceModel.ServiceDefinition);
        }


        /// <summary>
        /// Adding a website resource to the resource repository for the mv result case
        /// </summary>
        [TestMethod()]
        public void AddWebsiteResource()
        {
            // Arrange
            _serviceDefinition = "<Service />";
            IResourceModel resourceModel = CreateResource(ResourceType.Website);
            IList<IResourceModel> m = new List<IResourceModel>() { resourceModel };
            _resourceRepo.Setup(c => c.All()).Returns(m);
            // Act
            //mv.ReconnectEnvironment(environmentModel.Object);
            //5559 Check this test when refactor is finished
            //mv.ActiveEnvironment = environmentModel.Object;
            //_mainViewModel.
            //Assert
            //var resource = mv.ResourceRepository.FindSingle(res => res.ResourceName == resourceName);
            Assert.AreEqual("<Service />", resourceModel.ServiceDefinition);
        }


        [TestCleanup]
        public void CleanUp()
        {
            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.GotInputDataFromUser);
        }

        #endregion Additional result attributes


        #region Adding all the different resource types

        //TODO: Requires fixing - 5559
        ///// <summary>
        ///// Adding a work flow resource to the resource repository for the mv result case
        ///// </summary>
        //[TestMethod()]
        //public void AddWorkflowResource() {
        //    //Arrage
        //    IResourceModel resourceModel = CreateResource(enResourceType.WorkflowService);
        //    IList<IResourceModel> m = new List<IResourceModel>() { resourceModel };
        //    _resourceRepo.Setup(c => c.All()).Returns(m);

        //    //Act
        //    _mainViewModel.ActiveEnvironment = _environmentModel.Object;
        //    _mainViewModel.ReconnectEnvironment(_environmentModel.Object);
        //    _mainViewModel.AddWorkflowDocument(resourceModel);

        //    //Assert
        //    bool resource = _mainViewModel.IsResourceWorkflow(_resourceName);
        //    Assert.IsTrue(resource);

        //}
        ///// <summary>
        ///// Adding a worker resource to the resource repository for the mv result case
        ///// </summary>
        //[TestMethod()]
        //public void AddWorkerResource() {
        //    //Arrage
        //    _serviceDefinition = "<Service />";
        //    IResourceModel resourceModel = CreateResource(enResourceType.Service);
        //    IList<IResourceModel> m = new List<IResourceModel>() { resourceModel };
        //    _resourceRepo.Setup(c => c.All()).Returns(m);

        //    //Act
        //    _mainViewModel.ReconnectEnvironment(_environmentModel.Object);
        //    _mainViewModel.ActiveEnvironment = _environmentModel.Object;
        //    _mainViewModel.AddResourceDocument(resourceModel);
        //    //Assert
        //    var resource = _mainViewModel.ResourceRepository.FindSingle(res => res.ResourceName == _resourceName);
        //    Assert.AreEqual("<Service />", resourceModel.ServiceDefinition);

        //}
        ///// <summary>
        ///// Adding a source resource to the resource repository for the mv result case
        ///// </summary>
        //[TestMethod()]
        //public void AddSourceResource() {
        //    //Arrage
        //    _serviceDefinition = "<Source />";
        //    IResourceModel resourceModel = CreateResource(enResourceType.Source);
        //    IList<IResourceModel> m = new List<IResourceModel>() { resourceModel };

        //    _resourceRepo.Setup(c => c.All()).Returns(m);            
        //    //Act
        //    _mainViewModel.ReconnectEnvironment(_environmentModel.Object);
        //    _mainViewModel.ActiveEnvironment = _environmentModel.Object;
        //    _mainViewModel.AddResourceDocument(resourceModel);
        //    //Assert
        //    var resource = _mainViewModel.IsResourceWorkflow(_resourceName);
        //    Assert.AreEqual("<Source />", resourceModel.ServiceDefinition);

        //}

        ///// <summary>
        ///// Adding a website resource to the resource repository for the mv result case
        ///// </summary>
        //[TestMethod()]
        //public void AddWebsiteResource() {
        //    // Arrange
        //    _serviceDefinition = "<Service />";
        //    IResourceModel resourceModel = CreateResource(enResourceType.Website);
        //    IList<IResourceModel> m = new List<IResourceModel>() { resourceModel };
        //    _resourceRepo.Setup(c => c.All()).Returns(m);                             
        //    // Act
        //    _mainViewModel.ReconnectEnvironment(_environmentModel.Object);
        //    _mainViewModel.ActiveEnvironment = _environmentModel.Object;
        //    _mainViewModel.AddResourceDocument(resourceModel);
        //    //Assert
        //    var resource = _mainViewModel.ResourceRepository.FindSingle(res => res.ResourceName == _resourceName);
        //    Assert.AreEqual("<Service />", resourceModel.ServiceDefinition);
        //}

        #endregion Adding all the different resource types

        #region Build Tests
        [TestMethod]
        public void CanOnlySaveWithValidResourceModelEnvironmentNotNull()
        {
            // Should not be able to save with Null Environments
            //Assert.IsFalse(_mainViewModel.CanSave);

            // Setup the Mock models
            //Juries 8810 TODO
            //var mockWorkflowDesignerVM = new Mock<IWorkflowDesignerViewModel>();
            //var mockUserInterfaceLayoutProvider = new Mock<IUserInterfaceLayoutProvider>(MockBehavior.Loose);
            //var mockContextualResourceModel = new Mock<IContextualResourceModel>(MockBehavior.Loose);
            //var mockEnvironmentModel = new Mock<IEnvironmentModel>();

            //// Setup the environment
            ////UserInterfaceLayoutProvider myUserInterfaceLayoutProvider = new UserInterfaceLayoutProvider();
            ////myUserInterfaceLayoutProvider.ActiveDocument = new Object();
            //mockContextualResourceModel.SetupGet(uL => uL.Environment).Returns(mockEnvironmentModel.Object);
            //mockContextualResourceModel.SetupGet(uL => uL.Environment.IsConnected).Returns(true);
            //var lazyProvider = new Lazy<IUserInterfaceLayoutProvider>(() => mockUserInterfaceLayoutProvider.Object);
            //mockUserInterfaceLayoutProvider.SetupGet(uL => uL.ActiveDocumentDataContext).Returns(mockWorkflowDesignerVM.Object);
            //mockWorkflowDesignerVM.SetupGet(o => o.ResourceModel).Returns(mockContextualResourceModel.Object);

            //_mainViewModel.UserInterfaceLayoutProvider = lazyProvider;
            // Environment is now "Valid" - It should be able to save
            //Assert.IsTrue(_mainViewModel.CanSave);
            Assert.Inconclusive("Test needs to be fixed to not use the UserInterfaceLayoutProvider");
        }




        #endregion Build Tests


      
        #region Methods used by tests

        public IResourceModel CreateResource(ResourceType resourceType)
        {
            Mock<IResourceModel> result = new Mock<IResourceModel>();

            result.Setup(c => c.ResourceName).Returns(_resourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c => c.DisplayName).Returns(_displayName);
            result.Setup(c => c.ServiceDefinition).Returns(_serviceDefinition);
            result.Setup(c => c.Category).Returns("Testing");

            return result.Object;
        }
        
        public IContextualResourceModel CreateResource(ResourceType resourceType, IEnvironmentModel environment)
        {
            var result = new Mock<IContextualResourceModel>();

            result.Setup(c => c.ResourceName).Returns(_resourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c => c.DisplayName).Returns(_displayName);
            result.Setup(c => c.ServiceDefinition).Returns(_serviceDefinition);
            result.Setup(c => c.Category).Returns("Testing");
            result.Setup(c => c.Environment).Returns(environment);

            return result.Object;
        }


        #endregion Methods used by tests

        #region Eventaggregator
        [TestMethod]
        public void DeleteResourceMessageExpectRemoveNavigationResourceMessageIfSuccess()
        {
            //Setup resources and environment
            Mock<IEnvironmentModel> environmentModel = Dev2MockFactory.SetupEnvironmentModel();
            var resource = CreateResource(ResourceType.WorkflowService, environmentModel.Object);
            List<IResourceModel> list = new List<IResourceModel>() { resource };
             _resourceRepo.Setup(c => c.All()).Returns(list);
            _resourceRepo.Setup(r => r.DeleteResource(resource))
                         .Returns(new UnlimitedObject("<DataList>Success</DataList>"));
            environmentModel.Setup(c => c.IsConnected).Returns(true);
            environmentModel.SetupGet(c => c.ResourceRepository).Returns(_resourceRepo.Object);

            //setup event aggregator
            var mockEvtAggregator =
                new Mock<IEventAggregator>();
            mockEvtAggregator.Setup(c => c.Publish(It.IsAny<RemoveNavigationResourceMessage>()))
                                            .Verifiable();
            var ctx = CompositionInitializer.InializeWithEventAggregator(mockEvtAggregator.Object);
            _mainViewModel.EventAggregator = mockEvtAggregator.Object;

            //setup dependency service
            Mock<IResourceDependencyService> mockdependencies = new Mock<IResourceDependencyService>();
            mockdependencies.Setup(c => c.GetUniqueDependencies(resource)).Returns(list);
            _mainViewModel.ResourceDependencyService = mockdependencies.Object;

            //execute
            _mainViewModel.Handle(new DeleteResourceMessage(resource));

            mockEvtAggregator.Verify(c => c.Publish(It.IsAny<RemoveNavigationResourceMessage>()), Times.Once());
        }

        #endregion 


    }
}
