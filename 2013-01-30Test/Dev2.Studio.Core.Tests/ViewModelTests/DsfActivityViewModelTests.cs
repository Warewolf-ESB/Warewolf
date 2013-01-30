using System.Activities.Presentation.Model;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.ActivityViewModels;
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
            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.DoesActivityHaveWizard);
            //MediatorMessageTrapper.RegisterMessageToTrash(MediatorMessages.DoesActivityHaveWizard, true);
        }
        //
        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {

        }
        //
        #endregion

        [TestMethod]
        public void DsfActivityViewModelWhereModelItemIsCorrect_Expected_ViewModelWithPropertiesSet()
        {
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            DsfActivity act = DsfActivityFactory.CreateDsfActivity(mockRes.Object, null, true);
            ModelItem modelItem = TestModelItemFactory.CreateModelItem(act);
            DsfActivityViewModel vm = new DsfActivityViewModel(modelItem);

            Assert.IsTrue(vm.HasWizard == false && vm.HelpLink == "http://d");
            vm.Dispose();
        }

        [TestMethod]
        public void DsfActivityViewModelWhereModelItemIsNull_Expected_ViewModelWithNoPropertiesSet()
        {
            DsfActivityViewModel vm = new DsfActivityViewModel(null);
            Assert.IsTrue(vm.HelpLink == null && vm.IconPath == null);
            vm.Dispose();
        }

        [TestMethod]
        public void DsfActivityViewModelWhereModelItemHasNoHelpLink_Expected_ViewModelHasHelpLinkFalse()
        {
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            DsfActivity act = DsfActivityFactory.CreateDsfActivity(mockRes.Object, null, true);
            act.HelpLink = string.Empty;
            ModelItem modelItem = TestModelItemFactory.CreateModelItem(act);
            DsfActivityViewModel vm = new DsfActivityViewModel(modelItem);

            Assert.IsTrue(vm.HasHelpLink == false && vm.HelpLink == "");
            vm.Dispose();
        }

        [TestMethod]
        public void DsfActivityViewModelWhereModelItemHasProperties_Expected_ViewModelPropertyCollectionPopulated()
        {
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            DsfActivity act = DsfActivityFactory.CreateDsfActivity(mockRes.Object, null, true);
            ModelItem modelItem = TestModelItemFactory.CreateModelItem(act);
            DsfActivityViewModel vm = new DsfActivityViewModel(modelItem);

            Assert.IsTrue(vm.PropertyCollection.Count == 3);
            vm.Dispose();
        }


    }
}
