//5559 Update these tests

using Dev2.Common;
using Dev2.Composition;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
//using System.Windows.Media.Imaging;

namespace Dev2.Core.Tests {


    /// <summary>
    ///This is a result class for mvTest and is intended
    ///to contain all mvTest Unit Tests
    ///</summary>
    [TestClass()]
    public class mvTest {

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
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
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
        public void MyTestInitialize() {

            ImportService.CurrentContext = _importServiceContext;

            _environmentConnection.Setup(envConn => envConn.Connect()).Verifiable();
            _environmentConnection.Setup(envConn => envConn.Address).Returns(new Uri("http://localhost:77/dsf"));
            _environmentConnection.Setup(envConn => envConn.IsConnected).Returns(true);
            _environmentConnection.Setup(envConn => envConn.DataChannel.ExecuteCommand("someTez", Guid.NewGuid(), GlobalConstants.NullDataListID)).Returns("<x><result>tmpResult</result></x>").Verifiable();
            _environmentModel.Setup(envModel => envModel.Resources).Returns(_resourceRepo.Object);
            _environmentModel.Setup(envConn => envConn.Connect());
            _environmentModel.Setup(envConn => envConn.DsfAddress).Returns(new Uri("http://localhost:77/dsf"));
            _environmentModel.Setup(envConn => envConn.IsConnected).Returns(true);
            _environmentModel.Setup(envConn => envConn.DsfChannel.ExecuteCommand("someTez", Guid.NewGuid(), GlobalConstants.NullDataListID)).Returns("<x><result>tmpResult</result></x>").Verifiable();

            _environmentModel.Setup(envModel => envModel.Connect()).Verifiable();

            _mainViewModel = new MainViewModel();

        }
        #endregion

        #region Constructor Tests
        [TestMethod]
        public void MainInitialize() {
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
        public void AddWorkerResource() {
            //Arrage
            _serviceDefinition = "<Service />";
            IResourceModel resourceModel = CreateResource(ResourceType.Service);
            IList<IResourceModel> m = new List<IResourceModel>() { resourceModel };
            _resourceRepo.Setup(c => c.All()).Returns(m);

            //Act
            //mv.ReconnectEnvironment(environmentModel.Object);
            //5559 Check this test when refactor is finished
            //mv.ActiveEnvironment = environmentModel.Object;
            _mainViewModel.AddResourceDocument(resourceModel);
            //Assert
            //var resource = mv.ResourceRepository.FindSingle(res => res.ResourceName == resourceName);
            Assert.AreEqual("<Service />", resourceModel.ServiceDefinition);

        }

        
        /// <summary>
        /// Adding a source resource to the resource repository for the mv result case
        /// </summary>
        [TestMethod()]
        public void AddSourceResource() {
            //Arrage
            _serviceDefinition = "<Source />";
            IResourceModel resourceModel = CreateResource(ResourceType.Source);
            IList<IResourceModel> m = new List<IResourceModel>() { resourceModel };
            _resourceRepo.Setup(c => c.All()).Returns(m);            
            //Act
            //mv.ReconnectEnvironment(environmentModel.Object);
            //5559 Check this test when refactor is finished
            //mv.ActiveEnvironment = environmentModel.Object;
            _mainViewModel.AddResourceDocument(resourceModel);
            //Assert
            Assert.AreEqual("<Source />", resourceModel.ServiceDefinition);
        }

        
        /// <summary>
        /// Adding a website resource to the resource repository for the mv result case
        /// </summary>
        [TestMethod()]
        public void AddWebsiteResource() {
            // Arrange
            _serviceDefinition = "<Service />";
            IResourceModel resourceModel = CreateResource(ResourceType.Website);
            IList<IResourceModel> m = new List<IResourceModel>() { resourceModel };
            _resourceRepo.Setup(c => c.All()).Returns(m);                             
            // Act
            //mv.ReconnectEnvironment(environmentModel.Object);
            //5559 Check this test when refactor is finished
            //mv.ActiveEnvironment = environmentModel.Object;
            _mainViewModel.AddResourceDocument(resourceModel);

            //_mainViewModel.
            //Assert
            //var resource = mv.ResourceRepository.FindSingle(res => res.ResourceName == resourceName);
            Assert.AreEqual("<Service />", resourceModel.ServiceDefinition);
        }

        
        [TestCleanup]
        public void CleanUp() {
            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.GotInputDataFromUser);
        }

        #endregion Additional result attributes

        #region Initialize Tests


        #endregion Initialize Test

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

        /// <summary>
        /// Tests that a service is sucessfully built using the Build Command
        /// </summary>
        [TestMethod]
        public void Build_DeployFalse_Expected_SucessfullyBuiltService() {
        
        }

        /// <summary>
        /// Tests that a service is sucessfully built and deployed using the Build Command
        /// </summary>
        [TestMethod]
        public void Build_DeployTrue_Expected_SucessfullyBuiltServiceAndDeployedToServer() {

        }

        /// <summary>
        /// Tests that an invalid service cannot be built
        /// </summary>
        [TestMethod]
        public void Build_InvalidService_Expected_SucessfullyBuiltServiceAndDeployedToServer() {

        }

        /// <summary>
        /// Tests that an invalid service cannot be built and deployed
        /// </summary>
        [TestMethod]
        public void Build_InvalidServiceDeployTrue_Expected_SucessfullyBuiltServiceAndDeployedToServer() {

        }

        [TestMethod]
        public void CanOnlySaveWithValidResourceModelEnvironmentNotNull()
        {
            // Should not be able to save with Null Environments
            Assert.IsFalse(_mainViewModel.CanSave);
            
            // Setup the Mock models
            var mockWorkflowDesignerVM = new Mock<IWorkflowDesignerViewModel>();
            var mockUserInterfaceLayoutProvider = new Mock<IUserInterfaceLayoutProvider>(MockBehavior.Loose);
            var mockContextualResourceModel = new Mock<IContextualResourceModel>(MockBehavior.Loose);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            
            // Setup the environment
            //UserInterfaceLayoutProvider myUserInterfaceLayoutProvider = new UserInterfaceLayoutProvider();
            //myUserInterfaceLayoutProvider.ActiveDocument = new Object();
            mockContextualResourceModel.SetupGet(uL => uL.Environment).Returns(mockEnvironmentModel.Object);
            mockContextualResourceModel.SetupGet(uL => uL.Environment.IsConnected).Returns(true);
            var lazyProvider = new Lazy<IUserInterfaceLayoutProvider>(() => mockUserInterfaceLayoutProvider.Object);
            mockUserInterfaceLayoutProvider.SetupGet(uL => uL.ActiveDocumentDataContext).Returns(mockWorkflowDesignerVM.Object);
            mockWorkflowDesignerVM.SetupGet(o => o.ResourceModel).Returns(mockContextualResourceModel.Object);
          
            _mainViewModel.UserInterfaceLayoutProvider = lazyProvider;
            // Environment is now "Valid" - It should be able to save
            Assert.IsTrue(_mainViewModel.CanSave);
        }




        #endregion Build Tests

        #region Run Tests

        ///// <summary>
        ///// Test that the run method functions as expected when passed a valid ResourceModel
        ///// </summary>
        //[TestMethod]
        //public void Run_ValidResourceModel_Expected_NoMediatorMessageSentToInterfaceProvider() {
        //    IContextualResourceModel resModel = new ResourceModel(_environmentModel.Object);
        //    Mock<IUserInterfaceLayoutProvider> uILP = SetupUserInterfaceForDebug();

        //    _mainViewModel.UserInterfaceLayoutProvider = uILP.Object;
        //    Mock<IEnvironmentModel> environmentModel = Dev2MockFactory.SetupEnvironmentModel();
        //    environmentModel.Setup(c => c.IsConnected).Returns(true);
        //    _mainViewModel.ActiveEnvironment = environmentModel.Object;
        //    _mainViewModel.Run(resModel);

        //    uILP.Verify(c => c.StartDebuggingSession(It.IsAny<Framework.Session.DebugTO>(), It.IsAny<IEnvironmentModel>()), Times.Once());

        //}

        /// <summary>
        /// Test that the run method does nothing when passed a null ResourceModel
        /// </summary>
        //5559 Check test
        //[TestMethod]
        //public void Run_NullResourceModel_Expected_NoMediatorMessageSentToInterfaceProvider() {
        //    Mock<IUserInterfaceLayoutProvider> uILP = SetupUserInterfaceForDebug();
        //    _mainViewModel.ActiveEnvironment = _environmentModel.Object;
        //    _mainViewModel.Run(null);
        //    DebugTO tO = new DebugTO();
        //    uILP.Verify(c => c.GetServiceInputDataFromUser(It.IsAny<IServiceDebugInfoModel>(), out tO), Times.Never());
        //    uILP.Verify(c => c.StartDebuggingSession(It.IsAny<DebugTO>(), It.IsAny<IEnvironmentModel>()), Times.Never());
        //}
        
        #endregion Run Tests

        #region PerformDebug Task

        #endregion PerformDebug Task

        #region ViewInBrowser Tests

        #endregion ViewInBrowser Tests

        #region AddNewResource Tests


        #endregion AddNewResource Tests

        #region Edit Tests


        #endregion Edit Tests

        #region Debug Tests


        #endregion Debug Tests

        #region SetActivePage Tests


        #endregion SetActivePage Tests

        #region IsResourceWorkflow Tests

        #endregion IsResourceWorkflow Tests

        #region Deploy Tests


        #endregion Deploy Tests

        #region Methods used by tests

        public IResourceModel CreateResource(ResourceType resourceType)
        {
            Mock<IResourceModel> result = new Mock<IResourceModel>();

            result.Setup(c=>c.ResourceName).Returns(_resourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c=>c.DisplayName).Returns(_displayName);
            result.Setup(c=>c.ServiceDefinition).Returns(_serviceDefinition);
            result.Setup(c=>c.Category).Returns("Testing");

            return result.Object;
        }

        

        private Mock<IUserInterfaceLayoutProvider> CreateUserInterfaceProvider() {
            Mock<IUserInterfaceLayoutProvider> userInterfaceProvider = new Mock<IUserInterfaceLayoutProvider>();

            return userInterfaceProvider;
        }

        //private Mock<IUserInterfaceLayoutProvider> SetupUserInterfaceForDebug() {
        //    Mock<IUserInterfaceLayoutProvider> uILP = CreateUserInterfaceProvider();
        //    Framework.Session.DebugTO debugTO = new Framework.Session.DebugTO();
        //    uILP.Setup(c => c.GetServiceInputDataFromUser(It.IsAny<IServiceDebugInfoModel>(), out debugTO)).Returns(Dev2.Studio.Core.ViewModels.Base.ViewModelDialogResults.Okay);
        //    uILP.Setup(c => c.StartDebuggingSession(It.IsAny<Framework.Session.DebugTO>(), It.IsAny<IEnvironmentModel>())).Verifiable();

        //    return uILP;
        //}

        private void RecieveMediatorMessage(object input) {
            _count += 1;
        }

        #endregion Methods used by tests
    }
}
