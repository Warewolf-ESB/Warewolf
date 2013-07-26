using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Caliburn.Micro;
using Dev2.Communication;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.ProperMoqs;
using Dev2.Core.Tests.Utils;
using Dev2.Core.Tests.ViewModelTests.ViewModelMocks;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Services;
using Dev2.Studio.Core.Activities.Services;
using Dev2.Studio.Core.Activities.Utils;
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
    [TestClass, System.Runtime.InteropServices.GuidAttribute("9FB555BD-9E1C-40F7-AED8-A45BF179309D")]
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

        static DsfActivityViewModel GetViewModel(DsfActivity act, Mock<IContextualResourceModel> mockRes)
        {
            ModelItem modelItem = TestModelItemFactory.CreateModelItem(act);
            var serviceName = ModelItemUtils.GetProperty("ServiceName", modelItem) as string;
            var webAct = WebActivityFactory.CreateWebActivity(modelItem, mockRes.Object, serviceName);

            var dataMappingViewModel = new DataMappingViewModel(webAct);
            DsfActivityViewModel vm = new DsfActivityViewModel(modelItem, mockRes.Object, new Mock<IDesignValidationService>().Object, dataMappingViewModel);
            return vm;
        }

        [TestMethod]
        public void DsfActivityViewModelWhereModelItemIsNull_Expected_ViewModelWithNoPropertiesSet()
        {
            SetupMefStuff(new Mock<IEventAggregator>());

            Mock<IContextualResourceModel> resourceModel = CreateResourceModel(Guid.NewGuid());
            IContextualResourceModel contextualResourceModel = resourceModel.Object;
            var modelItem = CreateModelItem(resourceModel);

            DsfActivityViewModel vm = new DsfActivityViewModel(modelItem.Object, contextualResourceModel,new Mock<IDesignValidationService>().Object,new Mock<IDataMappingViewModel>().Object);

            Assert.IsNull(vm.HelpLink);
            Assert.IsNotNull(vm.IconPath);
            vm.Dispose();
        }

        [TestMethod]
        public void DsfActivityViewModelWhereModelItemHasProperties_Expected_ViewModelPropertiesPopulated()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            Mock<IContextualResourceModel> mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            DsfActivity act = DsfActivityFactory.CreateDsfActivity(mockRes.Object, null, true);
            ModelItem modelItem = TestModelItemFactory.CreateModelItem(act);
            DsfActivityViewModel vm = new DsfActivityViewModel(modelItem, CreateResourceModel(Guid.NewGuid()).Object, new Mock<IDesignValidationService>().Object, new Mock<IDataMappingViewModel>().Object);

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
            var model = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object,  null,null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DsfActivityViewModelConstructorWithNullValidationServiceExpectedThrowsArgumentNullException()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            var model = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object,null,null);
        }

        [TestMethod]
        public void DsfActivityViewModelConstructorWithAnyArgsExpectedAddsNoError()
        {
            SetupMefStuff(new Mock<IEventAggregator>());

            var validationService = new Mock<IDesignValidationService>();
            var model = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, validationService.Object, new Mock<IDataMappingViewModel>().Object);

            Assert.AreEqual(1, model.Errors.Count);
            Assert.AreSame(DsfActivityViewModel.NoError, model.Errors[0],model.Errors[0].Message);
        }

        [TestMethod]
        public void DsfActivityViewModelConstructorWithAnyArgsExpectedSetsLastValidationMemo()
        {
            SetupMefStuff(new Mock<IEventAggregator>());

            var instanceID = Guid.NewGuid();
            var resourceID = Guid.NewGuid();
            var resourceName = "TestResource";
            var serverID = Guid.NewGuid();

            var error = new ErrorInfo { InstanceID = instanceID, Message = "Error occurred", ErrorType = ErrorType.Critical, FixType = FixType.None };
            var resourceModel = CreateResourceModel(resourceID,error);
            resourceModel.Setup(r => r.ResourceName).Returns(resourceName);
            resourceModel.Setup(r => r.ServerID).Returns(serverID);

            var validationService = new Mock<IDesignValidationService>();
            var model = new DsfActivityViewModel(CreateModelItem(instanceID).Object, resourceModel.Object, validationService.Object, new Mock<IDataMappingViewModel>().Object);

            Assert.IsNotNull(model.LastValidationMemo);
            Assert.AreEqual(instanceID, model.LastValidationMemo.InstanceID);

            Assert.AreEqual(1, model.Errors.Count);
            Assert.AreSame(error, model.Errors[0],model.Errors[0].Message);
        }

        [TestMethod]
        public void DsfActivityViewModelConstructorWithValidValidationServiceExpectedCreatesSubscription()
        {
            SetupMefStuff(new Mock<IEventAggregator>());

            var eventPublisher = new EventPublisher();
            var resourceModel = CreateResourceModel(Guid.NewGuid(), eventPublisher);
            var validationService = new Mock<IDesignValidationService>();
            var model = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid()).Object, resourceModel.Object, validationService.Object, new Mock<IDataMappingViewModel>().Object);

            eventPublisher.Publish(new DesignValidationMemo());
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
            Mock<IContextualResourceModel> resourceModel = CreateResourceModel(Guid.NewGuid());
            IContextualResourceModel contextualResourceModel = resourceModel.Object;
            var modelItem = CreateModelItem(instanceID, resourceModel);

            var model = new DsfActivityViewModel(modelItem.Object, contextualResourceModel, validationService, new Mock<IDataMappingViewModel>().Object);

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
            Mock<IContextualResourceModel> resourceModel = CreateResourceModel(Guid.NewGuid());
            IContextualResourceModel contextualResourceModel = resourceModel.Object;
            var modelItem = CreateModelItem(instanceID, resourceModel);

            var model = new DsfActivityViewModel(modelItem.Object, contextualResourceModel, validationService, new Mock<IDataMappingViewModel>().Object);

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
            var resourceModel = CreateResourceModel(Guid.NewGuid());
            var modelItem = CreateModelItem(resourceModel);

            var vm = new DsfActivityViewModel(modelItem.Object, resourceModel.Object,new Mock<IDesignValidationService>().Object, new DataMappingViewModel(new Mock<IWebActivity>().Object));
            Assert.IsFalse(vm.ShowMapping, "FixErrors pre-condition for no error failed.");

            vm.FixErrorsCommand.Execute(null);

            Assert.IsFalse(vm.ShowMapping, "FixErrors did not do nothing.");
        }

        static Mock<ModelItem> CreateModelItem(Mock<IContextualResourceModel> resourceModel)
        {
            return CreateModelItem(Guid.NewGuid(), resourceModel);
        }
        
        static Mock<ModelItem> CreateModelItem(Guid uniqueID,Mock<IContextualResourceModel> resourceModel)
        {
            InArgument<Guid> argument = new InArgument<Guid>(resourceModel.Object.Environment.ID);
            var environmentIDProperty = new Mock<ModelProperty>();
            environmentIDProperty.Setup(property => property.Name).Returns("EnvironmentID");
            environmentIDProperty.Setup(property => property.ComputedValue).Returns(argument);
            var modelItem = CreateModelItem(uniqueID, new[] { environmentIDProperty.Object });
            return modelItem;
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
            var vm = new DsfActivityViewModel(modelItem.Object, rootModel, new Mock<IDesignValidationService>().Object, new DataMappingViewModel(new Mock<IWebActivity>().Object));
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

        [TestMethod]
        [TestCategory("DsfActivityViewModel_FixErrors")]
        [Description("FixErrors when FixType is MappingRequired must get a value for mapping to be fixed.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_FixMappingRequired_Done()
        // ReSharper restore InconsistentNaming
        {
            //init
            SetupMefStuff(new Mock<IEventAggregator>());
            var xml = @"<Args>
          <Input>[
          {""Name"":""n1"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":true,""RawValue"":"""",""EmptyToNull"":false}]</Input>          
        </Args>";

            var inputMapping = new Mock<ModelProperty>();
            inputMapping.Setup(p => p.Name).Returns("InputMapping");
            inputMapping.Setup(p => p.SetValue(It.IsAny<object>())).Verifiable("Fix errors did not update the model item input mappings.");

            var outputMapping = new Mock<ModelProperty>();
            outputMapping.Setup(p => p.Name).Returns("OutputMapping");
            outputMapping.Setup(p => p.SetValue(It.IsAny<object>())).Verifiable("Fix errors did not update the model item output mappings.");

            var instanceID = Guid.NewGuid();
            var worstError = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.IsRequiredChanged, FixData = xml,MessageType = CompileMessageType.MappingIsRequiredChanged};

            var modelItem = CreateModelItem(instanceID, inputMapping.Object, outputMapping.Object);
            var resourceModel = CreateResourceModel(Guid.NewGuid());

            var rootModel = new ResourceModel(ResourceModelTest.CreateMockEnvironment().Object)
            {
                ID = Guid.NewGuid()
            };
            rootModel.AddError(worstError);

            //exe

            var dataMappingViewModel = new DataMappingViewModel(new Mock<IWebActivity>().Object);
            var inputOutputViewModels = new ObservableCollection<IInputOutputViewModel>(){new InputOutputViewModel("n1","n1","","",false,"")};
            dataMappingViewModel.Inputs = inputOutputViewModels;
            var vm = new DsfActivityViewModel(modelItem.Object,  rootModel, new Mock<IDesignValidationService>().Object, dataMappingViewModel);
            vm.FixErrorsCommand.Execute(null);
            var actualInputs = vm.DataMappingViewModel.Inputs;

            //asserts
            Assert.AreEqual(1, actualInputs.Count, "Fix errors returned an incorrect number of outputmappings");
            Assert.AreEqual("n1", actualInputs[0].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
            Assert.AreEqual(string.Empty, actualInputs[0].MapsTo, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect MapsTo value");


            Assert.IsTrue(vm.ShowMapping, "Fix errors failed to show the mapping.");
            dataMappingViewModel.Inputs[0].MapsTo = "somevalue";
            vm.ShowMapping = false;
            Assert.IsFalse(vm.ShowMapping, "Fix errors failed to show the mapping.");
            // Always expect at least one error in the activity's error list - the no error
            Assert.AreEqual(ErrorType.None, vm.WorstError, "Fix errors failed to clear the error.");
            Assert.AreEqual(1, vm.Errors.Count, "Fix errors failed to remove the worst error from the activity.");

            Assert.AreEqual(0, rootModel.Errors.Count, "Fix errors failed to remove the worst error from the activity's root model.");
        }
//
//        [TestMethod]
//        [TestCategory("DsfActivityViewModel_FixErrors")]
//        [Description("FixErrors when FixType is MappingRequired must get a value for mapping to be fixed.")]
//        [Owner("Trevor Williams-Ros")]
//        // ReSharper disable InconsistentNaming
//        public void DsfActivityViewModel_UnitTest_FixMappingRequired_Done()
//        // ReSharper restore InconsistentNaming
//        {
//            //init
//            SetupMefStuff(new Mock<IEventAggregator>());
//            var xml = @"<Args>
//          <Input>[
//          {""Name"":""n1"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":true,""RawValue"":"""",""EmptyToNull"":false}]</Input>          
//        </Args>";
//
//            var inputMapping = new Mock<ModelProperty>();
//            inputMapping.Setup(p => p.Name).Returns("InputMapping");
//            inputMapping.Setup(p => p.SetValue(It.IsAny<object>())).Verifiable("Fix errors did not update the model item input mappings.");
//
//            var outputMapping = new Mock<ModelProperty>();
//            outputMapping.Setup(p => p.Name).Returns("OutputMapping");
//            outputMapping.Setup(p => p.SetValue(It.IsAny<object>())).Verifiable("Fix errors did not update the model item output mappings.");
//
//            var instanceID = Guid.NewGuid();
//            var worstError = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.IsRequiredChanged, FixData = xml,MessageType = CompileMessageType.MappingIsRequiredChanged};
//
//            var modelItem = CreateModelItem(instanceID, inputMapping.Object, outputMapping.Object);
//            var resourceModel = CreateResourceModel(Guid.NewGuid());
//
//            var rootModel = new ResourceModel(ResourceModelTest.CreateMockEnvironment().Object)
//            {
//                ID = Guid.NewGuid()
//            };
//            rootModel.AddError(worstError);
//
//            //exe
//
//            var dataMappingViewModel = new DataMappingViewModel(new Mock<IWebActivity>().Object);
//            var inputOutputViewModels = new ObservableCollection<IInputOutputViewModel>(){new InputOutputViewModel("n1","n1","","",false,"")};
//            dataMappingViewModel.Inputs = inputOutputViewModels;
//            var vm = new DsfActivityViewModel(modelItem.Object,  rootModel, new Mock<IDesignValidationService>().Object, dataMappingViewModel);
//            vm.FixErrorsCommand.Execute(null);
//            var actualInputs = vm.DataMappingViewModel.Inputs;
//
//            //asserts
//            Assert.AreEqual(1, actualInputs.Count, "Fix errors returned an incorrect number of outputmappings");
//            Assert.AreEqual("n1", actualInputs[0].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
//            Assert.AreEqual(string.Empty, actualInputs[0].MapsTo, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect MapsTo value");
//
//
//            Assert.IsTrue(vm.ShowMapping, "Fix errors failed to show the mapping.");
//            dataMappingViewModel.Inputs[0].MapsTo = "somevalue";
//            vm.ShowMapping = false;
//            Assert.IsFalse(vm.ShowMapping, "Fix errors failed to show the mapping.");
//            // Always expect at least one error in the activity's error list - the no error
//            Assert.AreEqual(ErrorType.None, vm.WorstError, "Fix errors failed to clear the error.");
//            Assert.AreEqual(1, vm.Errors.Count, "Fix errors failed to remove the worst error from the activity.");
//
//            Assert.AreEqual(0, rootModel.Errors.Count, "Fix errors failed to remove the worst error from the activity's root model.");
//        } 
        
        [TestMethod]
        [TestCategory("DsfActivityViewModel_FixErrors")]
        [Description("FixErrors when FixType is MappingRequired must get a value for mapping to be fixed.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_FixMappingRequiredWhenMappingValid_ShouldRemoveError()
        // ReSharper restore InconsistentNaming
        {
            //init
            SetupMefStuff(new Mock<IEventAggregator>());
            var xml = @"<Args>
          <Input>[
          {""Name"":""n1"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":true,""RawValue"":"""",""EmptyToNull"":false}]</Input>          
        </Args>";

            var inputMapping = new Mock<ModelProperty>();
            inputMapping.Setup(p => p.Name).Returns("InputMapping");
            inputMapping.Setup(p => p.SetValue(It.IsAny<object>())).Verifiable("Fix errors did not update the model item input mappings.");

            var outputMapping = new Mock<ModelProperty>();
            outputMapping.Setup(p => p.Name).Returns("OutputMapping");
            outputMapping.Setup(p => p.SetValue(It.IsAny<object>())).Verifiable("Fix errors did not update the model item output mappings.");

            var instanceID = Guid.NewGuid();
            var worstError = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.IsRequiredChanged, FixData = xml,MessageType = CompileMessageType.MappingIsRequiredChanged};

            var modelItem = CreateModelItem(instanceID, inputMapping.Object, outputMapping.Object);
            var resourceModel = CreateResourceModel(Guid.NewGuid());

            var rootModel = new ResourceModel(ResourceModelTest.CreateMockEnvironment().Object)
            {
                ID = Guid.NewGuid()
            };
            rootModel.AddError(worstError);

            //exe

            var dataMappingViewModel = new DataMappingViewModel(new Mock<IWebActivity>().Object);
            var inputOutputViewModels = new ObservableCollection<IInputOutputViewModel>(){new InputOutputViewModel("n1","n1","somevalue","",true,"")};
            dataMappingViewModel.Inputs = inputOutputViewModels;
            var vm = new DsfActivityViewModel(modelItem.Object, rootModel, new Mock<IDesignValidationService>().Object, dataMappingViewModel);
            var actualInputs = vm.DataMappingViewModel.Inputs;

            //asserts
            Assert.AreEqual(1, actualInputs.Count, "Fix errors returned an incorrect number of outputmappings");
            Assert.AreEqual("n1", actualInputs[0].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
            Assert.AreNotEqual(string.Empty, actualInputs[0].MapsTo, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect MapsTo value");
            Assert.IsFalse(vm.ShowMapping, "Fix errors failed to show the mapping.");
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
            var vm = new MockDsfActivityViewModel(modelItem.Object, resourceModel.Object,validationService,null);

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

            Mock<IContextualResourceModel> resourceModel = CreateResourceModel(Guid.NewGuid());
            IContextualResourceModel contextualResourceModel = resourceModel.Object;
            var modelItem = CreateModelItem(instanceID, resourceModel);

            var eventPublisher = new EventPublisher();
            var validationService = new DesignValidationService(eventPublisher);
            var vm = new MockDsfActivityViewModel(modelItem.Object, resourceModel.Object, validationService, new Mock<IDataMappingViewModel>().Object);
            var expected = vm.LastValidationMemo;

            var memo = new DesignValidationMemo { InstanceID = instanceID2 };
            eventPublisher.Publish(memo);

            Assert.AreEqual(0, vm.OnDesignValidationReceivedHitCount, "Activity received memo for a different instance ID.");
            Assert.AreNotSame(memo, vm.LastValidationMemo);
            Assert.AreSame(expected, vm.LastValidationMemo);
        }

        #endregion

        #region Designer Management Service

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructDesignerManagementService_Where_ResourceModelIsNull_Expected_Exception()
        {
            Mock<IResourceRepository> resourceRepository = Dev2MockFactory.SetupFrameworkRepositoryResourceModelMock();
            new DesignerManagementService(null, resourceRepository.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructDesignerManagementService_Where_ResourceRepositoryIsNull_Expected_Exception()
        {
            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock();
            new DesignerManagementService(resourceModel.Object, null);
        }

        [TestMethod]
        public void GetResourceModel_Where_ResourceModelExistsForModelItem_Expected_MatchingResourceModel()
        {
            SetupMefStuff(new Mock<IEventAggregator>());
            Mock<IContextualResourceModel> resourceModel = Dev2MockFactory.SetupResourceModelMock();
            Mock<IResourceRepository> resourceRepository = Dev2MockFactory.SetupFrameworkRepositoryResourceModelMock(resourceModel, new List<IResourceModel>());

            var designerManagementService = new DesignerManagementService(resourceModel.Object, resourceRepository.Object);

            IContextualResourceModel expected = resourceModel.Object;
            IContextualResourceModel actual = designerManagementService.GetRootResourceModel();

            Assert.AreEqual(expected, actual);
        }
        
        #endregion

        ///////////////////////////////////////////////
        // Static Helpers
        ///////////////////////////////////////////////

        #region CreateModelItem

        public static Mock<ModelItem> CreateModelItem(Guid uniqueID, params ModelProperty[] modelProperties)
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
            return CreateResourceModel(resourceID, null, resourceErrors);
        }

        static Mock<IContextualResourceModel> CreateResourceModel(Guid resourceID, EventPublisher eventPublisher, params IErrorInfo[] resourceErrors)
        {
            var errors = new ObservableReadOnlyList<IErrorInfo>();
            if(resourceErrors != null)
            {
                foreach(var resourceError in resourceErrors)
                {
                    errors.Add(resourceError);
                }
            }
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(conn => conn.ServerEvents).Returns(eventPublisher ?? new EventPublisher());
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.Connection).Returns(connection.Object);
            Guid newGuid = Guid.NewGuid();
            environment.Setup(e => e.ID).Returns(newGuid);
            //environment.Setup(e => e.Connection.ServerEvents).Returns(eventPublisher ?? new EventPublisher());
            environment.Setup(e => e.ResourceRepository).Returns(new Mock<IResourceRepository>().Object);

            var source = new Mock<IEnvironmentModel>();
            var repo = new TestLoadEnvironmentRespository(source.Object,environment.Object);
            new EnvironmentRepository(repo);
            
            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.Errors).Returns(errors);
            model.Setup(m => m.ID).Returns(resourceID);
            model.Setup(m => m.Environment).Returns(environment.Object);
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
