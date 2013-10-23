using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Collections;
using Dev2.Communication;
using Dev2.Core.Tests.Utils;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Core.ViewModels.ActivityViewModels;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
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
        //
        //        internal static ImportServiceContext SetupMefStuff()
        //        {
        //            var importServiceContext = new ImportServiceContext();
        //            ImportService.CurrentContext = importServiceContext;
        //
        //            ImportService.Initialize(new List<ComposablePartCatalog>
        //            {
        //                new FullStudioAggregateCatalog()
        //            });
        //
        //            var mainViewModel = new Mock<IMainViewModel>();
        //            ImportService.AddExportedValueToContainer(mainViewModel.Object);
        //
        //            return importServiceContext;
        //        }
        #endregion

        #region CTOR

        [TestMethod]
        [TestCategory("DsfActivityViewModel_Constructor")]
        [Description("DsfActivityViewModel constructor throws null argument exception when model item is null.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DsfActivityViewModel_UnitTest_ConstructorWithNullModelItem_ThrowsArgumentNullException()
        {
            //SetupMefStuff();
            var model = new DsfActivityViewModel(null, null, null);
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_Constructor")]
        [Description("DsfActivityViewModel constructor throws null argument exception when root model is null.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DsfActivityViewModel_UnitTest_ConstructorWithNullRootModel_ThrowsArgumentNullException()
        {
            //SetupMefStuff();
            var model = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid(), Guid.Empty, Guid.NewGuid()).Object, null, null);
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_Constructor")]
        [Description("DsfActivityViewModel constructor throws null argument exception when environment repository is null.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DsfActivityViewModel_UnitTest_ConstructorWithNullEnvironmentRepository_ThrowsArgumentNullException()
        {
            //SetupMefStuff();
            var model = new DsfActivityViewModel(CreateModelItem(Guid.NewGuid(), Guid.Empty, Guid.NewGuid()).Object, CreateResourceModel(Guid.NewGuid()).Object, null);
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_Constructor")]
        [Description("DsfActivityViewModel constructor with any args and no errors in resource model must add default NoError instance.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfActivityViewModel_UnitTest_ConstructorWithAnyArgs_AddsNoError()
        {
            var model = CreateActivityViewModel(Guid.NewGuid());

            Assert.AreEqual(1, model.Errors.Count);
            Assert.AreSame(DsfActivityViewModel.NoError, model.Errors[0], model.Errors[0].Message);
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_Constructor")]
        [Description("DsfActivityViewModel constructor must set the LastValidationMemo.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfActivityViewModel_UnitTest_ConstructorWithAnyArgs_SetsLastValidationMemo()
        {
            var instanceID = Guid.NewGuid();
            var error = new ErrorInfo { InstanceID = instanceID, Message = "Error occurred", ErrorType = ErrorType.Critical, FixType = FixType.None };

            var model = CreateActivityViewModel(instanceID, error);

            Assert.IsNotNull(model.LastValidationMemo);
            Assert.AreEqual(instanceID, model.LastValidationMemo.InstanceID);

            Assert.AreEqual(1, model.Errors.Count);
            Assert.AreSame(error, model.Errors[0], model.Errors[0].Message);
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_Constructor")]
        [Description("DsfActivityViewModel constructor with any args must create subscription to connection server events.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfActivityViewModel_UnitTest_ConstructorWithAnyArgs_CreatesServerEventsSubscription()
        {
            var model = CreateActivityViewModel(Guid.NewGuid());
            model.OnDesignValidationReceived += (sender, memo) => Assert.IsTrue(true);

            model.ResourceModel.Environment.Connection.ServerEvents.Publish(new DesignValidationMemo());
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_Constructor")]
        [Description("DsfActivityViewModel constructor sets IsDeleted to true and removes other errors when the resource model has an error where the FixType is Delete.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_ConstructorWithResourceContainingDeletedError_InitializesPropertiesCorrectly()
        // ReSharper restore InconsistentNaming
        {
            var instanceID = Guid.NewGuid();
            var error1 = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.ReloadMapping, FixData = "xxxxx" };
            var error2 = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.Delete, FixData = null };

            var vm = CreateActivityViewModel(instanceID, error1, error2);

            Assert.IsTrue(vm.IsDeleted, "Constructor did not set IsDeleted to true when the resource model has any errors where the FixType is Delete.");
            Assert.AreEqual(1, vm.LastValidationMemo.Errors.Count, "Constructor did not remove non delete errors.");
            Assert.IsTrue(vm.IsWorstErrorReadOnly, "Constructor did set IsWorstErrorReadOnly to true for Delete.");
            Assert.IsFalse(vm.IsEditable, "Constructor did set IsEditable to false for Delete.");
        }


        [TestMethod]
        [TestCategory("DsfActivityViewModel_Constructor")]
        [Description("DsfActivityViewModel constructor sets IsDeleted to true and removes other errors when the resource model has an error where the FixType is Delete.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_ConstructorWithResourceNotFoundInRepository_InitializesPropertiesCorrectly()
        // ReSharper restore InconsistentNaming
        {
            var instanceID = Guid.NewGuid();

            var vm = CreateActivityViewModel(instanceID, true, null);

            Assert.IsTrue(vm.IsDeleted, "Constructor did not set IsDeleted to true when the resource model has any errors where the FixType is Delete.");
            Assert.AreEqual(1, vm.LastValidationMemo.Errors.Count, "Constructor did not remove non delete errors.");
            Assert.IsTrue(vm.IsWorstErrorReadOnly, "Constructor did set IsWorstErrorReadOnly to true for Delete.");
            Assert.IsFalse(vm.IsEditable, "Constructor did set IsEditable to false for Delete.");
        }


        [TestMethod]
        [TestCategory("DsfActivityViewModel_Constructor")]
        [Owner("Huggs")]
        public void DsfActivityViewModel_UnitTest_ConstructorWithEnvironmentIDEmpty_ShouldLoadResourceFromRootModelEnvironment()
        {
            //-------------------------------------------Setup --------------------------------------------------------------------------
            var instanceID = Guid.NewGuid();
            //SetupMefStuff();
            Mock<IResourceRepository> resourceRepository;
            var rootModel = CreateResourceModel(Guid.NewGuid(), out resourceRepository, null);
            var resourceModel = CreateResourceModel(Guid.NewGuid(), true, null);
            resourceRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Returns(resourceModel.Object);
            var modelItem = CreateModelItem(instanceID, resourceModel.Object.ID, Guid.Empty, null);
            var envRepository = new Mock<IEnvironmentRepository>();
            //------------------------------------------Execute ---------------------------------------------------------------------------------------
            var vm = new DsfActivityViewModel(modelItem.Object, rootModel.Object, envRepository.Object);
            //-----------------------------------------Assertions -----------------------------------------------------------------------------------------
            Assert.IsFalse(vm.IsDeleted, "Constructor did not set IsDeleted to true when the resource model has any errors where the FixType is Delete.");
            Assert.AreEqual(0, vm.LastValidationMemo.Errors.Count, "Constructor has no errors.");
            Assert.IsNotNull(vm.ResourceModel);
            Assert.AreEqual(resourceModel.Object, vm.ResourceModel);
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_Constructor")]
        [Owner("Huggs")]
        public void DsfActivityViewModel_UnitTest_ConstructorWithEnvironmentID_ShouldLoadResourceFromResourceModelEnvironment()
        {
            //-------------------------------------------Setup --------------------------------------------------------------------------
            var instanceID = Guid.NewGuid();
            // SetupMefStuff();
            Mock<IResourceRepository> resourceRepository;
            var rootModel = CreateResourceModel(Guid.NewGuid(), true, null);
            var resourceModel = CreateResourceModel(Guid.NewGuid(), out resourceRepository, null);
            resourceRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Returns(resourceModel.Object);
            var modelItem = CreateModelItem(instanceID, resourceModel.Object.ID, resourceModel.Object.Environment.ID, null);
            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            //------------------------------------------Execute ---------------------------------------------------------------------------------------
            var vm = new DsfActivityViewModel(modelItem.Object, rootModel.Object, envRepository.Object);
            //-----------------------------------------Assertions -----------------------------------------------------------------------------------------
            Assert.IsFalse(vm.IsDeleted, "Constructor did not set IsDeleted to true when the resource model has any errors where the FixType is Delete.");
            Assert.AreEqual(0, vm.LastValidationMemo.Errors.Count, "Constructor has no errors.");
            Assert.IsNotNull(vm.ResourceModel);
            Assert.AreEqual(resourceModel.Object, vm.ResourceModel);
        }

        [TestMethod]
        public void DsfActivityViewModel_UnitTest_ConstructorWithModelItemHasProperties_PropertiesPopulated()
        {
            //SetupMefStuff();

            var mockRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService);
            var act = DsfActivityFactory.CreateDsfActivity(mockRes.Object, null, true);
            var modelItem = TestModelItemFactory.CreateModelItem(act);
            var vm = new DsfActivityViewModel(modelItem, CreateResourceModel(Guid.NewGuid()).Object, new Mock<IEnvironmentRepository>().Object);

            Assert.IsTrue(vm.Properties.Count == 3);
            vm.Dispose();
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfActivityViewModel_Constructor")]
        public void DsfActivityViewModel_Constructor_ResourceIDUsedToFindResource()
        {
            var resourceID = Guid.NewGuid();
            var rootModel = CreateResourceModel(resourceID);
            var modelProperties = new[] { CreateModelProperty("ResourceID", resourceID).Object };
            var modelItem = CreateModelItem(Guid.NewGuid(), rootModel.Object.ID, rootModel.Object.Environment.ID, modelProperties);
            var mockedEnvironmentRepository = new Mock<IEnvironmentRepository>();
            var mockedEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockedResRepo = new Mock<IResourceRepository>();
            mockedEnvironmentModel.Setup(env => env.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            Expression<Func<IResourceModel, bool>> actual = null;
            mockedResRepo.Setup(repo => repo.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Callback<Expression<Func<IResourceModel, bool>>>(f =>
            {
                actual = f;
            });
            mockedEnvironmentModel.Setup(env => env.ResourceRepository).Returns(mockedResRepo.Object);
            mockedEnvironmentRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(mockedEnvironmentModel.Object);

            // ReSharper disable once ObjectCreationAsStatement
            //------------Execute Test---------------------------
            new DsfActivityViewModel(modelItem.Object, rootModel.Object, mockedEnvironmentRepository.Object);

            // Assert Resource ID Used To Find Resource
            Assert.IsTrue((actual.Body as BinaryExpression).Right.Type.FullName.Contains("Guid"), "Resource ID was not used to identify resource");
        }

        #endregion

        #region Design Validation Service

        [TestMethod]
        [TestCategory("DsfActivityViewModel_DesignValidationService")]
        [Description("Published design validation memo with errors must be added to the errors list.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfActivityViewModel_UnitTest_DesignValidationServicePublishingErrors_UpdatesErrors()
        {
            var instanceID = Guid.NewGuid();
            var model = CreateActivityViewModel(instanceID);

            var memo = new DesignValidationMemo { InstanceID = instanceID };
            memo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, Message = "Critical error." });
            memo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Warning, Message = "Warning error." });

            model.OnDesignValidationReceived += (s, m) =>
            {
                Assert.AreEqual(m.Errors.Count, model.Errors.Count);
                Assert.AreEqual(ErrorType.Critical, model.WorstError);

                foreach(var error in m.Errors)
                {
                    var modelError = model.Errors.FirstOrDefault(me => me.ErrorType == error.ErrorType && me.Message == error.Message);
                    Assert.AreSame(error, modelError);
                }
                Assert.AreEqual(m.Errors.Count, model.Errors.Count);
            };

            model.ResourceModel.Environment.Connection.ServerEvents.Publish(memo);
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_DesignValidationService")]
        [Description("Published design validation memo without errors must not be added to the errors list.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfActivityViewModel_UnitTest_DesignValidationServicePublishingNoErrors_UpdatesErrorsWithNoError()
        {
            var instanceID = Guid.NewGuid();
            var model = CreateActivityViewModel(instanceID);

            var memo = new DesignValidationMemo { InstanceID = instanceID };

            model.OnDesignValidationReceived += (s, m) =>
            {
                Assert.AreEqual(1, model.Errors.Count);
                Assert.AreSame(DsfActivityViewModel.NoError, model.Errors[0]);
            };

            model.ResourceModel.Environment.Connection.ServerEvents.Publish(memo);
        }

        #endregion

        #region OpenParent

        [TestMethod]
        [TestCategory("DsfActivityViewModel_OpenParent")]
        [Description("DsfActivityViewModel OpenParent must not do anything if IsDeleted is true.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_OpenParentWhenDeleted_DoesNothing()
        // ReSharper restore InconsistentNaming
        {
            var eventAggregator = new Mock<IEventAggregator>();
            eventAggregator.Setup(e => e.Publish(It.IsAny<EditActivityMessage>())).Verifiable();

            var instanceID = Guid.NewGuid();
            var error1 = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.ReloadMapping, FixData = "xxxxx" };
            var error2 = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.Delete, FixData = null };

            var vm = CreateActivityViewModel(instanceID, error1, error2);

            Assert.IsTrue(vm.IsDeleted);

            vm.OpenParentCommand.Execute(null);

            eventAggregator.Verify(e => e.Publish(It.IsAny<EditActivityMessage>()), Times.Never(), "EditActivityMessage was published for deleted activity.");
        }

        #endregion

        #region Fix Errors

        [TestMethod]
        [TestCategory("DsfActivityViewModel_FixErrors")]
        [Description("FixErrors when WorstError is None must do nothing.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_FixNoError_DoesNothing()
        // ReSharper restore InconsistentNaming
        {
            var model = CreateActivityViewModel(Guid.NewGuid());
            Assert.IsFalse(model.ShowMapping, "FixErrors pre-condition for None error failed.");

            model.FixErrorsCommand.Execute(null);

            Assert.IsFalse(model.ShowMapping, "FixErrors for None did not do nothing.");
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_FixErrors")]
        [Description("FixErrors when FixType is ReloadMapping must reload mapping.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_FixReloadMapping_Done()
        // ReSharper restore InconsistentNaming
        {
            var xml = @"<Args>
          <Input>[
          {""Name"":""n1"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":false,""RawValue"":"""",""EmptyToNull"":false},
          {""Name"":""n2"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":false,""RawValue"":"""",""EmptyToNull"":false},
          {""Name"":""n3"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":false,""RawValue"":"""",""EmptyToNull"":false}]</Input>
          <Output>[{""Name"":""result"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":false,""RawValue"":"""",""EmptyToNull"":false}]</Output>
        </Args>";

            var inputs = new List<IDev2Definition>();
            var outputs = new List<IDev2Definition>();

            var inputMapping = CreateModelProperty("InputMapping", DataMappingListFactory.GenerateMapping(inputs, enDev2ArgumentType.Input));
            var outputMapping = CreateModelProperty("OutputMapping", DataMappingListFactory.GenerateMapping(outputs, enDev2ArgumentType.Output));

            inputMapping.Setup(p => p.SetValue(It.IsAny<object>())).Verifiable();
            outputMapping.Setup(p => p.SetValue(It.IsAny<object>())).Verifiable();

            var instanceID = Guid.NewGuid();
            var worstError = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.ReloadMapping, FixData = xml };

            var vm = CreateActivityViewModel(instanceID, new[] { inputMapping.Object, outputMapping.Object }, worstError);
            vm.FixErrorsCommand.Execute(null);

            var actualInputs = vm.DataMappingViewModel.Inputs;
            var actualOutputs = vm.DataMappingViewModel.Outputs;

            Assert.AreEqual(3, actualInputs.Count, "Fix errors returned an incorrect number of outputmappings");
            Assert.AreEqual("n1", actualInputs[0].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
            Assert.AreEqual(string.Empty, actualInputs[0].DefaultValue, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect DefaultValue value");
            Assert.AreEqual(string.Empty, actualInputs[0].RecordSetName, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect RecordSetName value");
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

            // Called: 1 x ActivityViewModel constructor and 1 x DataMappingViewModel constructor
            inputMapping.Verify(p => p.SetValue(It.IsAny<object>()), Times.Exactly(2));
            outputMapping.Verify(p => p.SetValue(It.IsAny<object>()), Times.Exactly(2));

            // Always expect at least one error in the activity's error list - the no error
            Assert.AreEqual(ErrorType.None, vm.WorstError, "Fix errors failed to clear the error.");
            Assert.AreEqual(1, vm.Errors.Count, "Fix errors failed to remove the worst error from the activity.");

            Assert.AreEqual(0, vm.RootModel.Errors.Count, "Fix errors failed to remove the worst error from the activity's root model.");
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_FixErrors")]
        [Description("FixErrors when FixType is MappingRequired must get a value for mapping to be fixed.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_FixMappingRequired_Done()
        // ReSharper restore InconsistentNaming
        {
            var xml = @"<Args>
          <Input>[
          {""Name"":""n1"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":true,""RawValue"":"""",""EmptyToNull"":false}]</Input>          
        </Args>";

            // Inputs must NOT have MapsTo
            var inputs = new List<IDev2Definition> { new Dev2Definition("n1", "", "", false, "", false, "") };
            var outputs = new List<IDev2Definition>();

            var inputMapping = CreateModelProperty("InputMapping", DataMappingListFactory.GenerateMapping(inputs, enDev2ArgumentType.Input));
            var outputMapping = CreateModelProperty("OutputMapping", DataMappingListFactory.GenerateMapping(outputs, enDev2ArgumentType.Output));

            var instanceID = Guid.NewGuid();
            var worstError = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.IsRequiredChanged, FixData = xml };
            
            var vm = CreateActivityViewModel(instanceID, new[] { inputMapping.Object, outputMapping.Object }, worstError);

            vm.FixErrorsCommand.Execute(null);
            var actualInputs = vm.DataMappingViewModel.Inputs;

            //asserts
            Assert.AreEqual(1, actualInputs.Count, "Fix errors returned an incorrect number of outputmappings");
            Assert.AreEqual("n1", actualInputs[0].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
            Assert.AreEqual(string.Empty, actualInputs[0].MapsTo, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect MapsTo value");

            Assert.IsTrue(vm.ShowMapping, "Fix errors failed to show the mapping.");

            // Simulate fixing error...
            vm.DataMappingViewModel.Inputs[0].MapsTo = "somevalue";
            vm.ShowMapping = false;

            Assert.IsFalse(string.IsNullOrEmpty(vm.DataMappingViewModel.Inputs[0].MapsTo), "Test did not simulate fixing error by setting MapsTo.");
            Assert.IsFalse(vm.ShowMapping, "Fix errors failed to show the mapping.");

            // Always expect at least one error in the activity's error list - the no error
            Assert.AreEqual(ErrorType.None, vm.WorstError, "Fix errors failed to clear the error.");
            Assert.AreEqual(1, vm.Errors.Count, "Fix errors failed to remove the worst error from the activity.");

            Assert.AreEqual(0, vm.RootModel.Errors.Count, "Fix errors failed to remove the worst error from the activity's root model.");
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
            var xml = @"<Args>
          <Input>[
          {""Name"":""n1"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":true,""RawValue"":"""",""EmptyToNull"":false}]</Input>          
        </Args>";

            // Inputs must have MapsTo
            var inputs = new List<IDev2Definition> { new Dev2Definition("n1", "n1", "somevalue", false, "somevalue", false, "") };
            var outputs = new List<IDev2Definition>();

            var inputMapping = CreateModelProperty("InputMapping", DataMappingListFactory.GenerateMapping(inputs, enDev2ArgumentType.Input));
            var outputMapping = CreateModelProperty("OutputMapping", DataMappingListFactory.GenerateMapping(outputs, enDev2ArgumentType.Output));

            var instanceID = Guid.NewGuid();
            var worstError = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.IsRequiredChanged, FixData = xml };

            var vm = CreateActivityViewModel(instanceID, new[] { inputMapping.Object, outputMapping.Object }, worstError);

            var actualInputs = vm.DataMappingViewModel.Inputs;

            Assert.AreEqual(1, actualInputs.Count, "Fix errors returned an incorrect number of outputmappings");
            Assert.AreEqual("n1", actualInputs[0].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
            Assert.AreNotEqual(string.Empty, actualInputs[0].MapsTo, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect MapsTo value");
            Assert.IsFalse(vm.ShowMapping, "Fix errors failed to show the mapping.");

            // Always expect at least one error in the activity's error list - the no error
            Assert.AreEqual(ErrorType.None, vm.WorstError, "Fix errors failed to clear the error.");
            Assert.AreEqual(1, vm.Errors.Count, "Fix errors failed to remove the worst error from the activity.");

            Assert.AreEqual(0, vm.RootModel.Errors.Count, "Fix errors failed to remove the worst error from the activity's root model.");
        }

        #endregion

        #region OnDesignValidationReceived

        [TestMethod]
        [TestCategory("DsfActivityViewModel_DesignValidationService")]
        [Description("Activity must receive memo's that match it's instance ID (unique ID).")]
        [Owner("Trevor Williams-Ros")]
        [Ignore] // Not valid for now - ActivityViewModel is never alive to receive events!
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_DesignValidationForThisActivity_Received()
        // ReSharper restore InconsistentNaming
        {
            var instanceID = Guid.NewGuid();

            var hitCount = 0;
            var model = CreateActivityViewModel(instanceID);
            model.OnDesignValidationReceived += (s, m) =>
            {
                hitCount++;
            };

            var memo = new DesignValidationMemo { InstanceID = instanceID };
            model.ResourceModel.Environment.Connection.ServerEvents.Publish(memo);

            Assert.AreEqual(1, hitCount, "Activity did not receive a memo matching it's instance ID.");
            Assert.AreSame(memo, model.LastValidationMemo, "Activity did not update LastValidationMemo with a memo matching it's instance ID.");
        }

        [TestMethod]
        [TestCategory("DsfActivityViewModel_DesignValidationService")]
        [Description("Activity must not receive memo's for other activities i.e. a different instance ID (unique ID).")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityViewModel_UnitTest_DesignValidationForOtherActivity_NotReceived()
        // ReSharper restore InconsistentNaming
        {
            var instanceID = Guid.NewGuid();
            var instanceID2 = Guid.NewGuid();

            var hitCount = 0;
            var model = CreateActivityViewModel(instanceID);
            model.OnDesignValidationReceived += (s, m) =>
            {
                hitCount++;
            };

            var expected = model.LastValidationMemo;

            var memo = new DesignValidationMemo { InstanceID = instanceID2 };
            model.ResourceModel.Environment.Connection.ServerEvents.Publish(memo);

            Assert.AreEqual(0, hitCount, "Activity received memo for a different instance ID.");
            Assert.AreNotSame(memo, model.LastValidationMemo, "Activity updated LastValidationMemo with memo for a different instance ID.");
            Assert.AreSame(expected, model.LastValidationMemo, "Activity updated LastValidationMemo with memo for a different instance ID.");
        }

        #endregion

        ///////////////////////////////////////////////
        // Static Helpers
        ///////////////////////////////////////////////

        #region CreateModelItem

        public static Mock<ModelItem> CreateModelItem(Guid uniqueID, Guid serviceID, Guid environmentID, params ModelProperty[] modelProperties)
        {
            var startIndex = 0;
            if(modelProperties == null)
            {
                modelProperties = new ModelProperty[3];
            }
            else
            {
                startIndex = modelProperties.Length;
                Array.Resize(ref modelProperties, startIndex + 3);
            }

            modelProperties[startIndex++] = CreateModelProperty("UniqueID", uniqueID.ToString()).Object;
            modelProperties[startIndex++] = CreateModelProperty("ResourceID", serviceID).Object;
            modelProperties[startIndex++] = CreateModelProperty("EnvironmentID", new InArgument<Guid>(environmentID)).Object;

            var properties = new Mock<ModelPropertyCollection>();

            foreach(var modelProperty in modelProperties)
            {
                properties.Protected().Setup<ModelProperty>("Find", modelProperty.Name, true).Returns(modelProperty);
            }

            var modelItem = new Mock<ModelItem>();
            modelItem.Setup(mi => mi.Properties).Returns(properties.Object);
            modelItem.Setup(mi => mi.ItemType).Returns(typeof(DsfActivity));
            return modelItem;
        }

        #endregion

        #region CreateModelProperty

        static Mock<ModelProperty> CreateModelProperty(string name, object value)
        {
            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.Name).Returns(name);
            prop.Setup(p => p.ComputedValue).Returns(value ?? "");
            return prop;
        }

        #endregion

        #region CreateResourceModel

        static Mock<IContextualResourceModel> CreateResourceModel(Guid resourceID, params IErrorInfo[] resourceErrors)
        {
            return CreateResourceModel(resourceID, false, resourceErrors);
        }

        static Mock<IContextualResourceModel> CreateResourceModel(Guid resourceID, bool resourceRepositoryReturnsNull, params IErrorInfo[] resourceErrors)
        {
            Mock<IResourceRepository> resourceRepository;
            Mock<IContextualResourceModel> resourceModel = CreateResourceModel(resourceID, out resourceRepository, resourceErrors);
            resourceRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>())).Returns(resourceRepositoryReturnsNull ? null : resourceModel.Object);
            return resourceModel;
        }

        static Mock<IContextualResourceModel> CreateResourceModel(Guid resourceID, out Mock<IResourceRepository> resourceRepository, params IErrorInfo[] resourceErrors)
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(conn => conn.ServerEvents).Returns(new EventPublisher());

            var environmentID = Guid.NewGuid();
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.Connection).Returns(connection.Object);
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(true);

            var errors = new ObservableReadOnlyList<IErrorInfo>();
            if(resourceErrors != null)
            {
                foreach(var resourceError in resourceErrors)
                {
                    errors.Add(resourceError);
                }
            }

            var model = new Mock<IContextualResourceModel>();
            model.Setup(r => r.ResourceName).Returns("TestResource");
            model.Setup(r => r.ServerID).Returns(Guid.NewGuid());
            model.Setup(r => r.ServiceDefinition).Returns("<root/>");
            model.Setup(m => m.Errors).Returns(errors);
            model.Setup(m => m.ID).Returns(resourceID);
            model.Setup(m => m.Environment).Returns(environment.Object);
            model.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);
            model.Setup(m => m.RemoveError(It.IsAny<IErrorInfo>())).Callback((IErrorInfo error) => errors.Remove(error));

            resourceRepository = new Mock<IResourceRepository>();

            environment.Setup(e => e.ResourceRepository).Returns(resourceRepository.Object);
            return model;
        }
        #endregion

        #region CreateActivityViewModel

        static DsfActivityViewModel CreateActivityViewModel(Guid instanceID, params IErrorInfo[] resourceErrors)
        {
            return CreateActivityViewModel(instanceID, null, resourceErrors);
        }

        static DsfActivityViewModel CreateActivityViewModel(Guid instanceID, ModelProperty[] modelProperties, params IErrorInfo[] resourceErrors)
        {
            return CreateActivityViewModel(instanceID, false, modelProperties, resourceErrors);
        }

        static DsfActivityViewModel CreateActivityViewModel(Guid instanceID, bool resourceRepositoryReturnsNull, ModelProperty[] modelProperties, params IErrorInfo[] resourceErrors)
        {
            // SetupMefStuff();

            var rootModel = CreateResourceModel(Guid.NewGuid(), resourceRepositoryReturnsNull, resourceErrors);
            var resourceModel = CreateResourceModel(Guid.NewGuid(), resourceRepositoryReturnsNull);
            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);
            var modelItem = CreateModelItem(instanceID, resourceModel.Object.ID, resourceModel.Object.Environment.ID, modelProperties);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);

            return new DsfActivityViewModel(modelItem.Object, rootModel.Object, envRepository.Object);
        }

        #endregion
    }
}
