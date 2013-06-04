using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.ProperMoqs;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.ActivityViewModels;
using Dev2.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.ViewModelTests
{
    /// <summary>
    /// Summary description for DsfActivityViewModelTests
    /// </summary>
    [TestClass]
    public class DsfActivityViewModelTests
    {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which providesinformation about and functionality for the current test run.
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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.DoesActivityHaveWizard);
            //MediatorMessageTrapper.RegisterMessageToTrash(MediatorMessages.DoesActivityHaveWizard, true);
        }
        //
        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {

        }
        //

        internal static ImportServiceContext SetupMefStuff(Mock<IEventAggregator> aggregator)
        {
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                new FullStudioAggregateCatalog()
            });

            var mainViewModel = new Mock<IMainViewModel>();
            ImportService.AddExportedValueToContainer(mainViewModel.Object);
            ImportService.AddExportedValueToContainer<IEventAggregator>(aggregator.Object);

            return importServiceContext;
        }
        #endregion

        [TestMethod]
        public void DsfActivityViewModelWhereModelItemIsCorrect_Expected_ViewModelWithPropertiesSet()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            DsfActivity act = DsfActivityFactory.CreateDsfActivity(mockRes.Object, null, true);
            ModelItem modelItem = TestModelItemFactory.CreateModelItem(act);
            DsfActivityViewModel vm = new DsfActivityViewModel(modelItem, mockRes.Object);

            Assert.IsTrue(vm.HasWizard == false && vm.HelpLink == "http://d");
            vm.Dispose();
        }

        [TestMethod]
        public void DsfActivityViewModelWhereModelItemIsNull_Expected_ViewModelWithNoPropertiesSet()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            DsfActivityViewModel vm = new DsfActivityViewModel(null, mockRes.Object);
            Assert.IsTrue(vm.HelpLink == null && vm.IconPath == null);
            vm.Dispose();
        }

        [TestMethod]
        public void DsfActivityViewModelWhereModelItemHasNoHelpLink_Expected_ViewModelHasHelpLinkFalse()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            DsfActivity act = DsfActivityFactory.CreateDsfActivity(mockRes.Object, null, true);
            act.HelpLink = string.Empty;
            ModelItem modelItem = TestModelItemFactory.CreateModelItem(act);
            DsfActivityViewModel vm = new DsfActivityViewModel(modelItem, mockRes.Object);
            
            Assert.IsTrue(vm.HasHelpLink == false && vm.HelpLink == "");
            vm.Dispose();
        }

        [TestMethod]
        public void DsfActivityViewModelWhereModelItemHasProperties_Expected_ViewModelPropertyCollectionPopulated()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            DsfActivity act = DsfActivityFactory.CreateDsfActivity(mockRes.Object, null, true);
            ModelItem modelItem = TestModelItemFactory.CreateModelItem(act);
            DsfActivityViewModel vm = new DsfActivityViewModel(modelItem, mockRes.Object);

            Assert.IsTrue(vm.PropertyCollection.Count == 3);
            vm.Dispose();
        }


    }
}
