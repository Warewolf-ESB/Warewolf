using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using Caliburn.Micro;
using Dev2.Activities;
using Dev2.Composition;
using Dev2.Core.Tests.ProperMoqs;
using Dev2.Core.Tests.Utils;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.ActivityViewModels;
using Dev2.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.ViewModelTests
{
    /// <summary>
    /// Summary description for DsfActivityViewModelTests
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class DsfSendEmailActivityViewModelTests
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
        public void DsfSendEmailActivityViewModelFetchEmailSourcesExpectedThreeEmailSourceInList()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            DsfSendEmailActivity activity =  new DsfSendEmailActivity();
            MockDsfSendEmailActivityViewModel mockViewModel = new MockDsfSendEmailActivityViewModel(activity);
            Mock<IEnvironmentModel> mockEnv = Dev2MockFactory.SetupEnvironmentModel();

            mockViewModel.UpdateEnvironmentResourcesCallback(mockEnv.Object);
            Assert.IsTrue(mockViewModel.EmailSourceList.Count == 3);           
        }

        [TestMethod]
        public void DsfSendEmailActivityViewModelFetchEmailSourcesWithNoSelectedEmailSourceExpectedFirstEmailSourceSetAsSelected()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            DsfSendEmailActivity activity = new DsfSendEmailActivity();
            MockDsfSendEmailActivityViewModel mockViewModel = new MockDsfSendEmailActivityViewModel(activity);
            Mock<IEnvironmentModel> mockEnv = Dev2MockFactory.SetupEnvironmentModel();

            mockViewModel.UpdateEnvironmentResourcesCallback(mockEnv.Object);
            Assert.IsTrue(mockViewModel.SelectedEmailSource == mockViewModel.EmailSourceList[1]);
        }

        [TestMethod]
        public void DsfSendEmailActivityViewModelFetchEmailSourcesWithSecondSelectedEmailSourceExpectedSecondEmailSourceSetAsSelected()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            DsfSendEmailActivity activity = new DsfSendEmailActivity();
            MockDsfSendEmailActivityViewModel mockViewModel = new MockDsfSendEmailActivityViewModel(activity);
            Mock<IEnvironmentModel> mockEnv = Dev2MockFactory.SetupEnvironmentModel();

            mockViewModel.UpdateEnvironmentResourcesCallback(mockEnv.Object);

            activity = new DsfSendEmailActivity { SelectedEmailSource = mockViewModel.EmailSourceList[1] };
            mockViewModel = new MockDsfSendEmailActivityViewModel(activity);
            mockViewModel.UpdateEnvironmentResourcesCallback(mockEnv.Object);

            Assert.IsTrue(mockViewModel.SelectedEmailSource == mockViewModel.EmailSourceList[1]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSendEmailActivityViewModel_Constructor")]
        public void DsfSendEmailActivityViewModel_Constructor_KeepsUserData_UserDataStillPersent()
        {
            //------------Setup for test--------------------------
            DsfSendEmailActivity activity = new DsfSendEmailActivity { FromAccount = "test@mydomain.com" };

            var emailSourceForTesting = new EmailSource
            {
                UserName = "MyUser",
                Password = "MyPassword",
                EnableSsl = false,
                Host = "mx.mydomain.com",
                Port = 25,
                TestFromAddress = "noreply@mydomain.com"
            };
            emailSourceForTesting.TestFromAddress = "bob@mydomain.com";
            emailSourceForTesting.ResourceID = Guid.NewGuid();

            //------------Execute Test---------------------------
            var dsfSendEmailActivityViewModel = new DsfSendEmailActivityViewModel(activity);
            dsfSendEmailActivityViewModel.SelectedEmailSource = emailSourceForTesting;

            //------------Assert Results-------------------------
            Assert.AreEqual("test@mydomain.com", dsfSendEmailActivityViewModel.FromAccount);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfSendEmailActivityViewModel_Constructor")]
        public void DsfSendEmailActivityViewModel_Constructor_HydratesFromSource_SourceDataOverwritesBlankUserData()
        {
            //------------Setup for test--------------------------
            DsfSendEmailActivity activity = new DsfSendEmailActivity { FromAccount = "" };

            var emailSourceForTesting = new EmailSource
            {
                UserName = "MyUser",
                Password = "MyPassword",
                EnableSsl = false,
                Host = "mx.mydomain.com",
                Port = 25,
                TestFromAddress = "noreply@mydomain.com"
            };
            emailSourceForTesting.TestFromAddress = "bob@mydomain.com";
            emailSourceForTesting.ResourceID = Guid.NewGuid();

            //------------Execute Test---------------------------
            var dsfSendEmailActivityViewModel = new DsfSendEmailActivityViewModel(activity);
            dsfSendEmailActivityViewModel.SelectedEmailSource = emailSourceForTesting;

            //------------Assert Results-------------------------
            Assert.AreEqual("MyUser", dsfSendEmailActivityViewModel.FromAccount);
        }
    }
}
