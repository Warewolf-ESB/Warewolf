using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Caliburn.Micro;
using Dev2.Communication;
using Dev2.Composition;
using Dev2.Core.Tests.Utils;
using Dev2.Core.Tests.ViewModelTests.ViewModelMocks;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Services;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.ActivityViewModels;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
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
            DsfActivityViewModel vm = new DsfActivityViewModel(modelItem, mockRes.Object, CreateResourceModel(Guid.NewGuid()).Object, new Mock<IDesignValidationService>().Object);

            Assert.IsTrue(vm.HasWizard == false && vm.HelpLink == "http://d");
            vm.Dispose();
        }

        [TestMethod]
        public void DsfActivityViewModelWhereModelItemIsNull_Expected_ViewModelWithNoPropertiesSet()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            DsfActivityViewModel vm = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid()).Object, mockRes.Object, CreateResourceModel(Guid.NewGuid()).Object, new Mock<IDesignValidationService>().Object);
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
            DsfActivityViewModel vm = new DsfActivityViewModel(modelItem, mockRes.Object, CreateResourceModel(Guid.NewGuid()).Object, new Mock<IDesignValidationService>().Object);
            
            Assert.IsTrue(vm.HasHelpLink == false && vm.HelpLink == "");
            vm.Dispose();
        }

        [TestMethod]
        public void DsfActivityViewModelWhereModelItemHasProperties_Expected_ViewModelPropertiesPopulated()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            DsfActivity act = DsfActivityFactory.CreateDsfActivity(mockRes.Object, null, true);
            ModelItem modelItem = TestModelItemFactory.CreateModelItem(act);
            DsfActivityViewModel vm = new DsfActivityViewModel(modelItem, mockRes.Object, CreateResourceModel(Guid.NewGuid()).Object, new Mock<IDesignValidationService>().Object);

            Assert.IsTrue(vm.Properties.Count == 3);
            vm.Dispose();
        }

        [TestMethod]
        public void DSFActivityFactoryDoesNotThrowExceptionWithServiceActivityAndNullSourceMethod()
        {
            var activity = new DsfServiceActivity();
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.Service);
            mockRes.Setup(r => r.ServiceDefinition).Returns(StringResources.xmlNullSourceMethodServiceDef);
            DsfActivity act = DsfActivityFactory.CreateDsfActivity(mockRes.Object, activity, true);

            //If no exception - pass
            Assert.IsTrue(true);
        }

        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DsfActivityViewModelConstructorWithNullModelItemExpectedThrowsArgumentNullException()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            var model = new DsfActivityViewModel(null, null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DsfActivityViewModelConstructorWithNullResourceModelExpectedThrowsArgumentNullException()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            var model = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid()).Object, null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DsfActivityViewModelConstructorWithNullRootModelExpectedThrowsArgumentNullException()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            var model = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DsfActivityViewModelConstructorWithNullValidationServiceExpectedThrowsArgumentNullException()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            var model = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, null);
        }

        [TestMethod]
        public void DsfActivityViewModelConstructorWithAnyArgsExpectedAddsNoError()
        {
            SetupMefStuff(new Mock<IEventAggregator>());

            var validationService = new Mock<IDesignValidationService>();
            var model = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, validationService.Object);

            Assert.AreEqual(1, model.Errors.Count);
            Assert.AreSame(DsfActivityViewModel.NoError, model.Errors[0]);
        }

        [TestMethod]
        public void DsfActivityViewModelConstructorWithAnyArgsExpectedSetsLastValidationMemo()
        {
            SetupMefStuff(new Mock<IEventAggregator>());

            var instanceID = Guid.NewGuid();
            var resourceID = Guid.NewGuid();
            var resourceName = "TestResource";
            var serverID = Guid.NewGuid();

            var resourceModel = CreateResourceModel(resourceID);
            resourceModel.Setup(r => r.ResourceName).Returns(resourceName);
            resourceModel.Setup(r => r.ServerID).Returns(serverID);

            var error = new ErrorInfo { InstanceID = instanceID, Message = "Error occurred", ErrorType = ErrorType.Critical, FixType = FixType.None };
            var rootModel = CreateResourceModel(Guid.NewGuid(), error);

            var validationService = new Mock<IDesignValidationService>();
            var model = new DsfActivityViewModel(CreateModelItem(instanceID).Object, resourceModel.Object, rootModel.Object, validationService.Object);

            Assert.IsNotNull(model.LastValidationMemo);
            Assert.AreEqual(instanceID, model.LastValidationMemo.InstanceID);
            Assert.AreEqual(serverID, model.LastValidationMemo.ServerID);
            Assert.AreEqual(resourceName, model.LastValidationMemo.ServiceName);

            Assert.AreEqual(1, model.Errors.Count);
            Assert.AreSame(error, model.Errors[0]);
        }

        [TestMethod]
        public void DsfActivityViewModelConstructorWithValidValidationServiceExpectedCreatesSubscription()
        {
            SetupMefStuff(new Mock<IEventAggregator>());

            var validationService = new Mock<IDesignValidationService>();
            validationService.Setup(s => s.Subscribe(It.IsAny<Guid>(), It.IsAny<Action<DesignValidationMemo>>())).Verifiable();

            var model = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, validationService.Object);

            validationService.Verify(s => s.Subscribe(It.IsAny<Guid>(), It.IsAny<Action<DesignValidationMemo>>()), Times.Once());
        }

        #endregion

        #region Dispose

        [TestMethod]
        [TestCategory("DsfActivityViewModel_DesignValidationService")]
        [Description("Dispose must dispose the validation service.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfActivityViewModel_UnitTest_Dispose_DisposesValidationService()
        {
            SetupMefStuff(new Mock<IEventAggregator>());

            var validationService = new Mock<IDesignValidationService>();
            validationService.Setup(s => s.Dispose()).Verifiable();

            var model = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, validationService.Object);
            model.Dispose();

            validationService.Verify(s => s.Dispose(), Times.Once());
        }

        #endregion

        [TestMethod]
        [TestCategory("DsfActivityViewModel_DesignValidationService")]
        [Description("Published design validation memo with errors must be added to the errors list.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfActivityViewModel_UnitTest_DesignValidationServicePublishingErrors_UpdatesErrors()
        {
            SetupMefStuff(new Mock<IEventAggregator>());

            var eventPublisher = new EventPublisher();
            var validationService = new DesignValidationService(eventPublisher);

            var instanceID = Guid.NewGuid();
            var modelItem = CreateModelItem(instanceID);

            var model = new DsfActivityViewModel(modelItem.Object, CreateResourceModel(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, validationService);

            var memo = new DesignValidationMemo { InstanceID = instanceID };
            memo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, Message = "Critical error." });
            memo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Warning, Message = "Warning error." });

            validationService.Subscribe(instanceID, m =>
            {
                Assert.AreEqual(m.Errors.Count, model.Errors.Count);
                Assert.AreEqual(ErrorType.Critical, model.WorstError);

                foreach(var error in m.Errors)
                {
                    var modelError = model.Errors.FirstOrDefault(me => me.ErrorType == error.ErrorType && me.Message == error.Message);
                    Assert.AreSame(error, modelError);
                }
                Assert.AreEqual(m.Errors.Count, model.Errors.Count);
            });

            eventPublisher.Publish(memo);
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_DesignValidationService")]
        [Description("Published design validation memo without errors must not be added to the errors list.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfActivityViewModel_UnitTest_DesignValidationServicePublishingNoErrors_UpdatesErrorsWithNoError()
        {
            SetupMefStuff(new Mock<IEventAggregator>());

            var eventPublisher = new EventPublisher();
            var validationService = new DesignValidationService(eventPublisher);

            var instanceID = Guid.NewGuid();
            var modelItem = CreateModelItem(instanceID);

            var model = new DsfActivityViewModel(modelItem.Object, CreateResourceModel(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, validationService);

            var memo = new DesignValidationMemo { InstanceID = instanceID };

            validationService.Subscribe(instanceID, m =>
            {
                Assert.AreEqual(1, model.Errors.Count);
                Assert.AreSame(DsfActivityViewModel.NoError, model.Errors[0]);
            });

            eventPublisher.Publish(memo);
        }


        #region Fix Errors

        [TestMethod]
        [TestCategory("DsfActivityViewModel_FixErrors")]
        [Description("FixErrors when WorstError is None must do nothing.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_FixNoError_DoesNothing()
        // ReSharper restore InconsistentNaming
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            var modelItem = CreateModelItem(Guid.NewGuid());
            var resourceModel = CreateResourceModel(Guid.NewGuid());
            var rootModel = CreateResourceModel(Guid.NewGuid());

            var vm = new DsfActivityViewModel(modelItem.Object, resourceModel.Object, rootModel.Object, new Mock<IDesignValidationService>().Object) { WorstError = ErrorType.Critical, DataMappingViewModel = new DataMappingViewModel(new Mock<IWebActivity>().Object) };
            Assert.IsFalse(vm.ShowMapping, "FixErrors pre-condition for no error failed.");

            vm.FixErrorsCommand.Execute(null);

            Assert.IsFalse(vm.ShowMapping, "FixErrors did not do nothing.");
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_FixErrors")]
        [Description("FixErrors when FixType is ReloadMapping must reload mapping.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_FixReloadMapping_Done()
        // ReSharper restore InconsistentNaming
        {
            //init
            SetupMefStuff(new Mock<IEventAggregator>());
            var xml = @"<Args>
          <Input>[
          {""Name"":""n1"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":false,""RawValue"":"""",""EmptyToNull"":false},
          {""Name"":""n2"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":false,""RawValue"":"""",""EmptyToNull"":false},
          {""Name"":""n3"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":false,""RawValue"":"""",""EmptyToNull"":false}]</Input>
          <Output>[{""Name"":""result"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":false,""RawValue"":"""",""EmptyToNull"":false}]</Output>
        </Args>";

            var inputMapping = new Mock<ModelProperty>();
            inputMapping.Setup(p => p.Name).Returns("InputMapping");
            inputMapping.Setup(p => p.SetValue(It.IsAny<object>())).Verifiable("Fix errors did not update the model item input mappings.");

            var outputMapping = new Mock<ModelProperty>();
            outputMapping.Setup(p => p.Name).Returns("OutputMapping");
            outputMapping.Setup(p => p.SetValue(It.IsAny<object>())).Verifiable("Fix errors did not update the model item output mappings.");

            var instanceID = Guid.NewGuid();
            var worstError = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.ReloadMapping, FixData = xml };

            var modelItem = CreateModelItem(instanceID, inputMapping.Object, outputMapping.Object);
            var resourceModel = CreateResourceModel(Guid.NewGuid());

            var rootModel = new ResourceModel(ResourceModelTest.CreateMockEnvironment().Object)
            {
                ID = Guid.NewGuid()
            };
            rootModel.AddError(worstError);

            //exe
            var vm = new DsfActivityViewModel(modelItem.Object, resourceModel.Object, rootModel, new Mock<IDesignValidationService>().Object) { WorstError = ErrorType.Critical, DataMappingViewModel = new DataMappingViewModel(new Mock<IWebActivity>().Object) };
            vm.FixErrorsCommand.Execute(null);
            var actualInputs = vm.DataMappingViewModel.Inputs;
            var actualOutputs = vm.DataMappingViewModel.Outputs;

            //asserts
            Assert.AreEqual(3, actualInputs.Count, "Fix errors returned an incorrect number of outputmappings");
            Assert.AreEqual("n1", actualInputs[0].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
            Assert.AreEqual(string.Empty, actualInputs[0].MapsTo, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect MapsTo value");
            Assert.AreEqual(string.Empty, actualInputs[0].DefaultValue, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect DefaultValue value");
            Assert.AreEqual(string.Empty, actualInputs[0].RecordSetName, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect RecordSetName value");
            Assert.AreEqual(string.Empty, actualInputs[0].Value, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Value value");
            Assert.AreEqual("n2", actualInputs[1].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
            Assert.AreEqual(string.Empty, actualInputs[1].MapsTo, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect MapsTo value");
            Assert.AreEqual(string.Empty, actualInputs[1].DefaultValue, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect DefaultValue value");
            Assert.AreEqual(string.Empty, actualInputs[1].RecordSetName, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect RecordSetName value");
            Assert.AreEqual(string.Empty, actualInputs[1].Value, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Value value");
            Assert.AreEqual("n3", actualInputs[2].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
            Assert.AreEqual(string.Empty, actualInputs[2].MapsTo, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect MapsTo value");
            Assert.AreEqual(string.Empty, actualInputs[2].DefaultValue, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect DefaultValue value");
            Assert.AreEqual(string.Empty, actualInputs[2].RecordSetName, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect RecordSetName value");
            Assert.AreEqual(string.Empty, actualInputs[2].Value, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Value value");

            Assert.AreEqual(1, actualOutputs.Count, "Fix errors returned an incorrect number of inputmappings");
            Assert.AreEqual("result", actualOutputs[0].Name, "Fix errors failed to fix a mapping error. The first input mapping contains an incorrect Name value");
            Assert.AreEqual(string.Empty, actualOutputs[0].MapsTo, "Fix errors failed to fix a mapping error. The first input mapping contains an incorrect MapsTo value");
            Assert.AreEqual(string.Empty, actualOutputs[0].DefaultValue, "Fix errors failed to fix a mapping error. The first input mapping contains an incorrect DefaultValue value");
            Assert.AreEqual(string.Empty, actualOutputs[0].RecordSetName, "Fix errors failed to fix a mapping error. The first input mapping contains an incorrect RecordSetName value");
            Assert.AreEqual(string.Empty, actualOutputs[0].Value, "Fix errors failed to fix a mapping error. The first input mapping contains an incorrect Value value");

            Assert.IsTrue(vm.ShowMapping, "Fix errors failed to show the mapping.");

            inputMapping.Verify(p => p.SetValue(It.IsAny<object>()), Times.Once());
            outputMapping.Verify(p => p.SetValue(It.IsAny<object>()), Times.Once());

            // Always expect at least one error in the activity's error list - the no error
            Assert.AreEqual(ErrorType.None, vm.WorstError, "Fix errors failed to clear the error.");
            Assert.AreEqual(1, vm.Errors.Count, "Fix errors failed to remove the worst error from the activity.");

            Assert.AreEqual(0, rootModel.Errors.Count, "Fix errors failed to remove the worst error from the activity's root model.");
        }

        #endregion

        #region OnDesignValidationReceived

        [TestMethod]
        [TestCategory("DsfActivityViewModel_DesignValidationService")]
        [Description("Activity must receive memo's that match it's instance ID (unique ID).")]
        [Owner("Trevor Williams-Ros")]
        [Ignore] // Not valid for now
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_DesignValidationForThisActivity_Received()
        // ReSharper restore InconsistentNaming
        {
            SetupMefStuff(new Mock<IEventAggregator>());

            var instanceID = Guid.NewGuid();

            var modelItem = CreateModelItem(instanceID);
            var resourceModel = CreateResourceModel(Guid.NewGuid());
            var rootModel = CreateResourceModel(Guid.NewGuid());

            var eventPublisher = new EventPublisher();
            var validationService = new DesignValidationService(eventPublisher);
            var vm = new MockDsfActivityViewModel(modelItem.Object, resourceModel.Object, rootModel.Object, validationService);

            var memo = new DesignValidationMemo { InstanceID = instanceID };
            eventPublisher.Publish(memo);

            Assert.AreEqual(1, vm.OnDesignValidationReceivedHitCount, "Activity did not receive a memo matching it's instance ID.");
            Assert.AreSame(memo, vm.LastValidationMemo);
        }


        [TestMethod]
        [TestCategory("DsfActivityViewModel_DesignValidationService")]
        [Description("Activity must not receive memo's for other activities i.e. a different instance ID (unique ID).")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_DesignValidationForOtherActivity_NotReceived()
        // ReSharper restore InconsistentNaming
        {
            SetupMefStuff(new Mock<IEventAggregator>());

            var instanceID = Guid.NewGuid();
            var instanceID2 = Guid.NewGuid();

            var modelItem = CreateModelItem(instanceID);
            var resourceModel = CreateResourceModel(Guid.NewGuid());
            var rootModel = CreateResourceModel(Guid.NewGuid());

            var eventPublisher = new EventPublisher();
            var validationService = new DesignValidationService(eventPublisher);
            var vm = new MockDsfActivityViewModel(modelItem.Object, resourceModel.Object, rootModel.Object, validationService);
            var expected = vm.LastValidationMemo;

            var memo = new DesignValidationMemo { InstanceID = instanceID2 };
            eventPublisher.Publish(memo);

            Assert.AreEqual(0, vm.OnDesignValidationReceivedHitCount, "Activity received memo for a different instance ID.");
            Assert.AreNotSame(memo, vm.LastValidationMemo);
            Assert.AreSame(expected, vm.LastValidationMemo);
        }

        #endregion


        ///////////////////////////////////////////////
        // Static Helpers
        ///////////////////////////////////////////////

        #region CreateModelItem

        static Mock<ModelItem> CreateModelItem(Guid uniqueID, params ModelProperty[] modelProperties)
        {
            var propUniqueID = new Mock<ModelProperty>();
            propUniqueID.Setup(p => p.ComputedValue).Returns(uniqueID.ToString());

            var properties = new Mock<ModelPropertyCollection>();
            properties.Protected().Setup<ModelProperty>("Find", "UniqueID", true).Returns(propUniqueID.Object);

            if(modelProperties != null)
            {
                foreach(var modelProperty in modelProperties)
                {
                    properties.Protected().Setup<ModelProperty>("Find", modelProperty.Name, true).Returns(modelProperty);
                }
            }

            var modelItem = new Mock<ModelItem>();
            modelItem.Setup(mi => mi.Properties).Returns(properties.Object);
            modelItem.Setup(mi => mi.ItemType).Returns(typeof(DsfActivity));
            return modelItem;
        }

        #endregion

        #region CreateResourceModel

        static Mock<IContextualResourceModel> CreateResourceModel(Guid resourceID, params IErrorInfo[] resourceErrors)
        {
            var errors = new ObservableReadOnlyList<IErrorInfo>();
            if(resourceErrors != null)
            {
                foreach(var resourceError in resourceErrors)
                {
                    errors.Add(resourceError);
                }
            }
            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.Errors).Returns(errors);
            model.Setup(m => m.ID).Returns(resourceID);
            model.Setup(m => m.Environment).Returns(new Mock<IEnvironmentModel>().Object);
            model.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);

            //model.Setup(m => m.GetErrors(It.IsAny<Guid>())).Callback(id => errors.Where(e => e.InstanceID == id).ToList());

            return model;
        }

        #endregion
        [TestMethod]
        [TestCategory("DsfActivityFactory_CreateDsfActivity")]
        [Description("DsfActivityFactory must assign the resource and environment ID.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfActivityFactory_UnitTest_ResourceAndEnvironmentIDAssigned_Done()
        {
            var expectedResourceID = Guid.NewGuid();
            var expectedEnvironmentID = Guid.NewGuid();

            var activity = new DsfActivity();

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.ID).Returns(expectedEnvironmentID);

            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.ResourceType).Returns(ResourceType.Service);
            model.Setup(m => m.ID).Returns(expectedResourceID);
            model.Setup(m => m.Environment).Returns(environment.Object);
            model.Setup(m => m.ServiceDefinition).Returns("<root/>");

            DsfActivityFactory.CreateDsfActivity(model.Object, activity, false);

            var actualResourceID = Guid.Parse(activity.ResourceID.Expression.ToString());
            var actualEnvironmentID = Guid.Parse(activity.EnvironmentID.Expression.ToString());

            Assert.AreEqual(expectedResourceID, actualResourceID, "DsfActivityFactory did not assign the resource ID.");
            Assert.AreEqual(expectedEnvironmentID, actualEnvironmentID, "DsfActivityFactory did not assign the environment ID.");
        }
    }
}
