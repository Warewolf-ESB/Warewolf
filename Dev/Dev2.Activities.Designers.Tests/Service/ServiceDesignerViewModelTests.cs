
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Service;
using Dev2.Collections;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.Network;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Simulation;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Language.Flow;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
namespace Dev2.Activities.Designers.Tests.Service
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    [Ignore] //TODO: Fix so not dependant on resource file or localize resource file to test project
    public class ServiceDesignerViewModelTests
    {
        const string ExpectedName = "TestServiceName";

        #region CTOR

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        public void ServiceDesignerViewModel_Constructor_WhenRemoteActiveEnvironmentAndActivityHasEmptyGuidEnviromentID_ExpectNoValidationErrorsForMissingResource()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = GenerateServiceDesignerViewModel(ExpectedName);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, model.DesignValidationErrors.Count);
            var msg = model.DesignValidationErrors[0];
            StringAssert.Contains("Service Working Normally", msg.Message);
            Assert.AreEqual(ErrorType.None, msg.ErrorType);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        public void ServiceDesignerViewModel_Constructor_ErrorMemoDataIsNull_NoNullXmlException()
        {

            //------------Setup for test--------------------------
            var errors = new List<IErrorInfo>
                {
                    new ErrorInfo
                        {
                            FixData = null,
                            FixType = FixType.None,
                            ErrorType = ErrorType.None,
                            InstanceID = Guid.NewGuid(),
                            Message = "Message Data"
                        }
                };

            var rootModel = new Mock<IContextualResourceModel>();
            rootModel.Setup(m => m.Errors.Count).Returns(1);
            rootModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);

            //------------Execute Test---------------------------
            GenerateServiceDesignerViewModel(ExpectedName, rootModel);

            //------------Assert Results-------------------------

            // No exception it passed ;)
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        public void ServiceDesignerViewModel_Constructor_ImageSource_InitializedCorrectlyForType()
        {
            Verify_Constructor_ImageSource_InitializedCorrectlyForType("RemoteService", "RemoteWarewolf-32");
            Verify_Constructor_ImageSource_InitializedCorrectlyForType("DbService", "DatabaseService-32");
            Verify_Constructor_ImageSource_InitializedCorrectlyForType("WebService", "WebService-32");
            Verify_Constructor_ImageSource_InitializedCorrectlyForType("PluginService", "PluginService-32");
            Verify_Constructor_ImageSource_InitializedCorrectlyForType("Workflow", "Workflow-32", "");
            Verify_Constructor_ImageSource_InitializedCorrectlyForType("Workflow", "RemoteWarewolf-32", "x");

            Verify_Constructor_ImageSource_InitializedCorrectlyForType("BizRule", "ToolService-32");
            Verify_Constructor_ImageSource_InitializedCorrectlyForType("InvokeDynamicService", "ToolService-32");
            Verify_Constructor_ImageSource_InitializedCorrectlyForType("InvokeManagementDynamicService", "ToolService-32");
            Verify_Constructor_ImageSource_InitializedCorrectlyForType("InvokeServiceMethod", "ToolService-32");
            Verify_Constructor_ImageSource_InitializedCorrectlyForType("Switch", "ToolService-32");
            Verify_Constructor_ImageSource_InitializedCorrectlyForType("Unknown", "ToolService-32");

            Verify_Constructor_ImageSource_InitializedCorrectlyForType("xxx", "ToolService-32");
            Verify_Constructor_ImageSource_InitializedCorrectlyForType("", "ToolService-32");
            Verify_Constructor_ImageSource_InitializedCorrectlyForType(null, "ToolService-32");
        }

        void Verify_Constructor_ImageSource_InitializedCorrectlyForType(string type, string expectedImageSource, string serviceUri = null)
        {

            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var vm = GenerateServiceDesignerViewModel(ExpectedName, null, type, serviceUri);

            //------------Assert Results-------------------------
            Assert.AreEqual(expectedImageSource, vm.ImageSource);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        public void ServiceDesignerViewModel_Constructor_DisplayNameContainsDsf_DisplayNameIsServiceName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = GenerateServiceDesignerViewModel(ExpectedName);
            var modelItem = model.ModelItem;

            //------------Assert Results-------------------------
            var actual = modelItem.GetProperty<string>("DisplayName");

            Assert.AreEqual(ExpectedName, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        public void ServiceDesignerViewModel_Constructor_DisplayNameDoesNotContainDsf_DisplayNameIsNotChanged()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var model = GenerateServiceDesignerViewModel(ExpectedName);
            var modelItem = model.ModelItem;

            //------------Assert Results-------------------------
            var actual = modelItem.GetProperty<string>("DisplayName");

            Assert.AreEqual(ExpectedName, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        public void ServiceDesignerViewModel_ConstructorWithEmptyGuid_PropertiesInitialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = CreateServiceDesignerViewModelWithEmptyResourceID(Guid.Empty);

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.DataMappingViewModel);
            Assert.IsNotNull(viewModel.FixErrorsCommand);
            Assert.IsNotNull(viewModel.DesignValidationErrors);
            Assert.IsNotNull(viewModel.RootModel);
            Assert.IsNotNull(viewModel.ResourceModel);
            Assert.IsNotNull(viewModel.ImageSource);

            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        public void ServiceDesignerViewModel_Constructor_PropertiesInitialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = CreateServiceDesignerViewModel(Guid.NewGuid());

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.DataMappingViewModel);
            Assert.IsNotNull(viewModel.FixErrorsCommand);
            Assert.IsNotNull(viewModel.DesignValidationErrors);
            Assert.IsNotNull(viewModel.RootModel);
            Assert.IsNotNull(viewModel.ResourceModel);
            Assert.IsNotNull(viewModel.ImageSource);

            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        public void ServiceDesignerViewModel_Constructor_WhenIsItemDraggedTrue_ShouldBeExpandedAndSetIsItemDraggedFalse()
        {
            //------------Setup for test--------------------------
            IsItemDragged.Instance.IsDragged = true;
            //------------Execute Test---------------------------
            var viewModel = CreateServiceDesignerViewModel(Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.ShowLarge);
            Assert.IsFalse(IsItemDragged.Instance.IsDragged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        public void ServiceDesignerViewModel_Constructor_WhenIsItemDraggedFalse_ShouldNotBeExpandedAndSetIsItemDraggedFalse()
        {
            //------------Setup for test--------------------------
            IsItemDragged.Instance.IsDragged = false;
            //------------Execute Test---------------------------
            var viewModel = CreateServiceDesignerViewModel(Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.IsFalse(IsItemDragged.Instance.IsDragged);
            Assert.IsFalse(viewModel.ShowLarge);
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        [Description("ServiceDesignerViewModel constructor throws null argument exception when model item is null.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceDesignerViewModel_Constructor_NullModelItem_ThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ServiceDesignerViewModel(null, null, null, null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        [Description("ServiceDesignerViewModel constructor throws null argument exception when root model is null.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceDesignerViewModel_Constructor_NullRootModel_ThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ServiceDesignerViewModel(new Mock<ModelItem>().Object, null, null, null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        [Description("ServiceDesignerViewModel constructor throws null argument exception when environment repository is null.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceDesignerViewModel_Constructor_NullEnvironmentRepository_ThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ServiceDesignerViewModel(new Mock<ModelItem>().Object, new Mock<IContextualResourceModel>().Object, null, null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        [Description("ServiceDesignerViewModel constructor throws null argument exception when environment repository is null.")]
        [Owner("Leon Rajindrapersadh")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceDesignerViewModel_Constructor_NullAsyncWorker_ThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement

            new ServiceDesignerViewModel(new Mock<ModelItem>().Object, new Mock<IContextualResourceModel>().Object, new Mock<IEnvironmentRepository>().Object, new Mock<IEventAggregator>().Object, null);
            // ReSharper restore ObjectCreationAsStatement
        }


        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceDesignerViewModel_Constructor_NullEventPublisher_ThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ServiceDesignerViewModel(new Mock<ModelItem>().Object, new Mock<IContextualResourceModel>().Object, new Mock<IEnvironmentRepository>().Object, null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        [Description("ServiceDesignerViewModel constructor with any args and no errors in resource model must add default NoError instance.")]
        [Owner("Trevor Williams-Ros")]
        public void ServiceDesignerViewModel_Constructor_AnyArgs_AddsNoError()
        {
            var model = CreateServiceDesignerViewModel(Guid.NewGuid());

            Assert.AreEqual(1, model.DesignValidationErrors.Count);
            Assert.AreSame(ServiceDesignerViewModel.NoError, model.DesignValidationErrors[0], model.DesignValidationErrors[0].Message);
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        [Description("ServiceDesignerViewModel constructor must set the LastValidationMemo.")]
        [Owner("Trevor Williams-Ros")]
        public void ServiceDesignerViewModel_Constructor_AnyArgs_SetsLastValidationMemo()
        {
            var instanceID = Guid.NewGuid();
            var error = new ErrorInfo { InstanceID = instanceID, Message = "Error occurred", ErrorType = ErrorType.Critical, FixType = FixType.None };

            var model = CreateServiceDesignerViewModel(instanceID, error);

            Assert.IsNotNull(model.LastValidationMemo);
            Assert.AreEqual(instanceID, model.LastValidationMemo.InstanceID);

            Assert.AreEqual(1, model.DesignValidationErrors.Count);
            Assert.AreSame(error, model.DesignValidationErrors[0], model.DesignValidationErrors[0].Message);
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        [Description("ServiceDesignerViewModel constructor with any args must create subscription to connection server events.")]
        [Owner("Trevor Williams-Ros")]
        public void ServiceDesignerViewModel_Constructor_AnyArgs_CreatesServerEventsSubscription()
        {
            var model = CreateServiceDesignerViewModel(Guid.NewGuid());
            model.OnDesignValidationReceived += (sender, memo) => Assert.IsTrue(true);

            model.ResourceModel.Environment.Connection.ServerEvents.Publish(new DesignValidationMemo());
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        [Description("ServiceDesignerViewModel constructor sets IsDeleted to true and removes other errors when the resource model has an error where the FixType is Delete.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesignerViewModel_Constructor_ResourceContainingDeletedError_InitializesPropertiesCorrectly()
        // ReSharper restore InconsistentNaming
        {
            var instanceID = Guid.NewGuid();
            var error1 = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.ReloadMapping, FixData = "xxxxx" };
            var error2 = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.Delete, FixData = null };

            var vm = CreateServiceDesignerViewModel(instanceID, error1, error2);

            Assert.IsTrue(vm.IsDeleted, "Constructor did not set IsDeleted to true when the resource model has any errors where the FixType is Delete.");
            Assert.AreEqual(1, vm.LastValidationMemo.Errors.Count, "Constructor did not remove non delete errors.");
            Assert.IsTrue(vm.IsWorstErrorReadOnly, "Constructor did set IsWorstErrorReadOnly to true for Delete.");
            Assert.IsTrue(vm.IsFixed);
            Assert.IsFalse(vm.IsEditable, "Constructor did set IsEditable to false for Delete.");
        }


        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        [Description("ServiceDesignerViewModel constructor sets IsDeleted to true and removes other errors when the resource model has an error where the FixType is Delete.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesignerViewModel_Constructor_ResourceNotFoundInRepository_InitializesPropertiesCorrectly()
        // ReSharper restore InconsistentNaming
        {
            var instanceID = Guid.NewGuid();

            var vm = CreateServiceDesignerViewModel(instanceID, true, null);

            Assert.IsTrue(vm.IsDeleted, "Constructor did not set IsDeleted to true when the resource model has any errors where the FixType is Delete.");
            Assert.AreEqual(1, vm.LastValidationMemo.Errors.Count, "Constructor did not remove non delete errors.");
            Assert.IsTrue(vm.IsWorstErrorReadOnly, "Constructor did set IsWorstErrorReadOnly to true for Delete.");
            Assert.IsTrue(vm.IsFixed);
            Assert.IsFalse(vm.IsEditable, "Constructor did set IsEditable to false for Delete.");
        }


        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        [Owner("Huggs")]
        public void ServiceDesignerViewModel_Constructor_EnvironmentIDEmpty_ShouldLoadResourceFromRootModelEnvironment()
        {
            //------------Setup for test--------------------------
            var rootModel = new Mock<IContextualResourceModel>();
            rootModel.Setup(m => m.Errors.Count).Returns(0);
            rootModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(new List<IErrorInfo>());
            //------------Execute Test---------------------------
            var vm = GenerateServiceDesignerViewModel(ExpectedName, rootModel);
            var resourceModel = vm.ResourceModel;

            //-----------------------------------------Assertions -----------------------------------------------------------------------------------------
            Assert.IsFalse(vm.IsDeleted, "Constructor did not set IsDeleted to true when the resource model has any errors where the FixType is Delete.");
            Assert.AreEqual(0, vm.LastValidationMemo.Errors.Count, "Constructor has no errors.");
            Assert.IsNotNull(vm.ResourceModel);
            Assert.AreEqual(rootModel.Object, resourceModel);
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        [Owner("Huggs")]
        public void ServiceDesignerViewModel_Constructor_EnvironmentID_ShouldLoadResourceFromResourceModelEnvironment()
        {
            //------------Setup for test--------------------------
            var rootModel = new Mock<IContextualResourceModel>();
            rootModel.Setup(m => m.Errors.Count).Returns(0);
            rootModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(new List<IErrorInfo>());
            //------------Execute Test---------------------------
            var vm = GenerateServiceDesignerViewModel(ExpectedName, rootModel);
            var resourceModel = vm.ResourceModel;

            //-----------------------------------------Assertions -----------------------------------------------------------------------------------------
            Assert.IsFalse(vm.IsDeleted, "Constructor did not set IsDeleted to true when the resource model has any errors where the FixType is Delete.");
            Assert.AreEqual(0, vm.LastValidationMemo.Errors.Count, "Constructor has no errors.");
            Assert.IsNotNull(vm.ResourceModel);
            Assert.AreEqual(rootModel.Object, resourceModel);
        }

        [TestMethod]
        public void ServiceDesignerViewModel_Constructor_ModelItemHasProperties_PropertiesPopulated()
        {
            var friendlySourceName = CreateModelProperty("FriendlySourceName", "Hello");
            var type = CreateModelProperty("Type", "Workflow");
            var actionName = CreateModelProperty("ActionName", "dbo.pr_mydata");


            var vm = CreateServiceDesignerViewModel(Guid.NewGuid(), new[] { friendlySourceName.Object, type.Object, actionName.Object });

            Assert.IsTrue(vm.Properties.Count == 3);
        }

        #endregion

        #region Design Validation Service

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_DesignValidationService")]
        [Description("Published design validation memo with errors must be added to the errors list.")]
        [Owner("Trevor Williams-Ros")]
        public void ServiceDesignerViewModel_DesignValidation_ServicePublishingErrors_UpdatesErrors()
        {
            var instanceID = Guid.NewGuid();
            var model = CreateServiceDesignerViewModel(instanceID);

            var memo = new DesignValidationMemo { InstanceID = instanceID };
            memo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, Message = "Critical error.", InstanceID = instanceID });
            memo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Warning, Message = "Warning error.", InstanceID = instanceID });

            model.OnDesignValidationReceived += (s, m) =>
            {
                Assert.AreEqual(m.Errors.Count, model.DesignValidationErrors.Count);
                Assert.AreEqual(ErrorType.Critical, model.WorstError);

                foreach (var error in m.Errors)
                {
                    IErrorInfo currentError = error;
                    var modelError = model.DesignValidationErrors.FirstOrDefault(me => me.ErrorType == currentError.ErrorType && me.Message == currentError.Message);
                    Assert.AreSame(error, modelError);
                }
                Assert.AreEqual(m.Errors.Count, model.DesignValidationErrors.Count);
            };

            model.ResourceModel.Environment.Connection.ServerEvents.Publish(memo);
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_DesignValidationService")]
        [Description("Published design validation memo without errors must not be added to the errors list.")]
        [Owner("Trevor Williams-Ros")]
        public void ServiceDesignerViewModel_DesignValidation_ServicePublishingNoErrors_UpdatesErrorsWithNoError()
        {
            var instanceID = Guid.NewGuid();
            var model = CreateServiceDesignerViewModel(instanceID);

            var memo = new DesignValidationMemo { InstanceID = instanceID };

            model.OnDesignValidationReceived += (s, m) =>
            {
                Assert.AreEqual(1, model.DesignValidationErrors.Count);
                Assert.AreSame(ServiceDesignerViewModel.NoError, model.DesignValidationErrors[0]);
            };

            model.ResourceModel.Environment.Connection.ServerEvents.Publish(memo);
        }

        #endregion

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceDesignerViewModel_InitializeResourceModel")]
        public void ServiceDesignerViewModel_InitializeResourceModel_ServiceTypeHasSourceAndIsInvalidXml_NoErrorMessageAdded()
        {
            //------------Setup for test--------------------------
            Guid instanceID;
            Mock<IEnvironmentModel> environment;
            Mock<IContextualResourceModel> resourceModel;
            Guid sourceID;
            var mockRepo = SetupForSourceCheck(out instanceID, out environment, out resourceModel, out sourceID, true);
            mockRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns((IResourceModel)null);
            mockRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), true)).Returns(resourceModel.Object);
            environment.Setup(e => e.ResourceRepository).Returns(mockRepo.Object);
            environment.Setup(a => a.HasLoadedResources).Returns(true);
            resourceModel.Setup(contextualResourceModel => contextualResourceModel.ResourceType).Returns(Studio.Core.AppResources.Enums.ResourceType.Service);
            //------------Execute Test---------------------------
            var model = CreateServiceDesignerViewModel(instanceID, false, new Mock<IEventAggregator>().Object, null, resourceModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, model.DesignValidationErrors.Count);
            Assert.AreEqual(ErrorType.Critical, model.DesignValidationErrors[0].ErrorType);
            Assert.AreEqual(FixType.None, model.DesignValidationErrors[0].FixType);
            StringAssert.Contains(model.DesignValidationErrors[0].Message, "Source was not found.");

        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceDesignerViewModel_InitializeResourceModel")]
        public void ServiceDesignerViewModel_InitializeResourceModel_ServiceTypeHasSource_NoErrorMessageAdded()
        {
            //------------Setup for test--------------------------
            Guid instanceID;
            Mock<IEnvironmentModel> environment;
            Mock<IContextualResourceModel> resourceModel;
            Guid sourceID;
            var mockRepo = SetupForSourceCheck(out instanceID, out environment, out resourceModel, out sourceID);

            mockRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(new Mock<IResourceModel>().Object);
            mockRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), true)).Returns(resourceModel.Object);
            environment.Setup(e => e.ResourceRepository).Returns(mockRepo.Object);

            resourceModel.Setup(contextualResourceModel => contextualResourceModel.ResourceType).Returns(Studio.Core.AppResources.Enums.ResourceType.Service);
            //------------Execute Test---------------------------
            var model = CreateServiceDesignerViewModel(instanceID, false, new Mock<IEventAggregator>().Object, null, resourceModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, model.DesignValidationErrors.Count);
            Assert.AreEqual(ErrorType.None, model.DesignValidationErrors[0].ErrorType);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceDesignerViewModel_InitializeResourceModel")]
        public void ServiceDesignerViewModel_InitializeResourceModel_ServiceTypeHasNoSource_ErrorMessageAdded()
        {
            //------------Setup for test--------------------------
            Guid instanceID;
            Mock<IEnvironmentModel> environment;
            Mock<IContextualResourceModel> resourceModel;
            Guid sourceID;
            var mockRepo = SetupForSourceCheck(out instanceID, out environment, out resourceModel, out sourceID);
            mockRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns((IResourceModel)null);
            mockRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), true)).Returns(resourceModel.Object);
            environment.Setup(e => e.ResourceRepository).Returns(mockRepo.Object);
            environment.Setup(a => a.HasLoadedResources).Returns(true);
            resourceModel.Setup(contextualResourceModel => contextualResourceModel.ResourceType).Returns(Studio.Core.AppResources.Enums.ResourceType.Service);
            //------------Execute Test---------------------------
            var model = CreateServiceDesignerViewModel(instanceID, false, new Mock<IEventAggregator>().Object, null, resourceModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, model.DesignValidationErrors.Count);
            Assert.AreEqual(ErrorType.Critical, model.DesignValidationErrors[0].ErrorType);
            Assert.AreEqual(FixType.None, model.DesignValidationErrors[0].FixType);
            StringAssert.Contains(model.DesignValidationErrors[0].Message, "Source was not found.");

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceDesignerViewModel_InitializeResourceModel")]
        public void ServiceDesignerViewModel_HandleUpdateResourceMessage_SourceMatchesSourceID_ErrorMessageRemoved()
        {
            //------------Setup for test--------------------------
            Guid instanceID;
            Mock<IEnvironmentModel> environment;
            Mock<IContextualResourceModel> resourceModel;
            Guid sourceID;
            var mockRepo = SetupForSourceCheck(out instanceID, out environment, out resourceModel, out sourceID);
            mockRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns((IResourceModel)null);
            mockRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), true)).Returns(resourceModel.Object);
            environment.Setup(e => e.ResourceRepository).Returns(mockRepo.Object);
            environment.Setup(a => a.HasLoadedResources).Returns(true);

            resourceModel.Setup(contextualResourceModel => contextualResourceModel.ResourceType).Returns(Studio.Core.AppResources.Enums.ResourceType.Service);
            var model = CreateServiceDesignerViewModel(instanceID, false, new Mock<IEventAggregator>().Object, null, resourceModel);
            //------------Assert Preconditions--------------------
            Assert.AreEqual(1, model.DesignValidationErrors.Count);
            Assert.AreEqual(ErrorType.Critical, model.DesignValidationErrors[0].ErrorType);
            Assert.AreEqual(FixType.None, model.DesignValidationErrors[0].FixType);
            StringAssert.Contains(model.DesignValidationErrors[0].Message, "Source was not found.");
            //------------Execute Test---------------------------
            var sourceModelInMessage = new Mock<IContextualResourceModel>();
            sourceModelInMessage.Setup(contextualResourceModel => contextualResourceModel.ID).Returns(sourceID);
            model.Handle(new UpdateResourceMessage(sourceModelInMessage.Object));
            //------------Assert Results-------------------------
            Assert.AreEqual(1, model.DesignValidationErrors.Count);
            Assert.AreEqual(ErrorType.None, model.DesignValidationErrors[0].ErrorType);

        }

        #region OpenParent

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_OpenParent")]
        [Description("ServiceDesignerViewModel OpenParent must not do anything if IsDeleted is true.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesignerViewModel_OpenParent_WhenDeleted_DoesNothing()
        // ReSharper restore InconsistentNaming
        {
            var eventAggregator = new Mock<IEventAggregator>();
            //eventAggregator.Setup(e => e.Publish(It.IsAny<EditActivityMessage>())).Verifiable();

            var instanceID = Guid.NewGuid();
            var error1 = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.ReloadMapping, FixData = "xxxxx" };
            var error2 = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.Delete, FixData = null };

            var vm = CreateServiceDesignerViewModel(instanceID, false, eventAggregator.Object, null, error1, error2);

            Assert.IsTrue(vm.IsDeleted);

            // ------ Execute Test -----
            vm.ShowParent = true;

            eventAggregator.Verify(e => e.Publish(It.IsAny<EditActivityMessage>()), Times.Never(), "EditActivityMessage was published for deleted activity.");
        }

        #endregion

        #region Fix Errors

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceDesignerViewModel_FixErrorsCommand")]
        public void ServiceDesignerViewModel_FixErrorsCommand_ErrorMemoDataIsNull_NoNullXmlException()
        {
            //------------Setup for test--------------------------
            var rootModel = new Mock<IContextualResourceModel>();
            var errors = new List<IErrorInfo>
                {
                    new ErrorInfo
                        {
                            FixData = null,
                            FixType = FixType.ReloadMapping,
                            ErrorType = ErrorType.Critical,
                            InstanceID = Guid.NewGuid(),
                            Message = "Message Data"
                        }
                };

            rootModel.Setup(m => m.Errors.Count).Returns(0);
            rootModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);

            //------------Execute Test---------------------------
            var vm = GenerateServiceDesignerViewModel(ExpectedName, rootModel);
            vm.FixErrorsCommand.Execute(null);

            //------------Assert Results-------------------------

            // No exception, all is good ;)

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceDesignerViewModel_FixErrors")]
        public void ServiceDesignerViewModel_FixErrorsCommand_ErrorMemoDataIsInvalidXml_NoInputsOrOutputsPresent()
        {
            //------------Setup for test--------------------------
            var inputs = new List<IDev2Definition>();
            var outputs = new List<IDev2Definition>();

            var inputMapping = CreateModelProperty("InputMapping", DataMappingListFactory.GenerateMapping(inputs, enDev2ArgumentType.Input));
            var outputMapping = CreateModelProperty("OutputMapping", DataMappingListFactory.GenerateMapping(outputs, enDev2ArgumentType.Output));

            inputMapping.Setup(p => p.SetValue(It.IsAny<object>())).Verifiable();
            outputMapping.Setup(p => p.SetValue(It.IsAny<object>())).Verifiable();

            var instanceID = Guid.NewGuid();
            var worstError = new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Critical, FixType = FixType.ReloadMapping, FixData = "fix me" };

            var vm = CreateServiceDesignerViewModel(instanceID, new[] { inputMapping.Object, outputMapping.Object }, worstError);
            vm.FixErrorsCommand.Execute(null);

            var actualInputs = vm.DataMappingViewModel.Inputs;
            var actualOutputs = vm.DataMappingViewModel.Outputs;


            //------------Assert Results-------------------------

            // No exception, all is good ;)
            Assert.AreEqual(0, actualInputs.Count);
            Assert.AreEqual(0, actualOutputs.Count);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServiceDesignerViewModel_FixErrors")]
        public void ServiceDesignerViewModel_FixErrorsCommand_InputDefinitionsMalformed_NoInputsOneOutputPresent()
        {
            //------------Setup for test--------------------------
            const string xml = @"<Args>
                      <Input>[
                      {""Name"":""n1"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":false,""RawValue"":"""",""EmptyToNull"":false},
                      {""Name"":""n2"",""MapsTo"":"""",""Value"":"""",""IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":false,""RawValue"":"""",""EmptyToNull"":false},
                      {""Name"":""n3"",""MapsTo"":"""",""Value"":"""",IsRecordSet"":false,""RecordSetName"":"""",""IsEvaluated"":false,""DefaultValue"":"""",""IsRequired"":false,""RawValue"":"""",""EmptyToNull"":false}]</Input>
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

            var vm = CreateServiceDesignerViewModel(instanceID, new[] { inputMapping.Object, outputMapping.Object }, worstError);
            vm.FixErrorsCommand.Execute(null);

            var actualInputs = vm.DataMappingViewModel.Inputs;
            var actualOutputs = vm.DataMappingViewModel.Outputs;


            //------------Assert Results-------------------------

            // No exception, all is good ;)
            Assert.AreEqual(0, actualInputs.Count);
            Assert.AreEqual(1, actualOutputs.Count);

        }


        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_FixErrors")]
        [Description("FixErrors when WorstError is None must do nothing.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesignerViewModel_FixErrors_FixNoError_DoesNothing()
        // ReSharper restore InconsistentNaming
        {
            var model = CreateServiceDesignerViewModel(Guid.NewGuid());
            Assert.IsFalse(model.ShowLarge, "FixErrors pre-condition for None error failed.");

            model.FixErrorsCommand.Execute(null);

            Assert.IsFalse(model.ShowLarge, "FixErrors for None did not do nothing.");
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_FixErrors")]
        [Description("FixErrors when FixType is ReloadMapping must reload mapping.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesignerViewModel_FixErrors_FixReloadMapping_Done()
        // ReSharper restore InconsistentNaming
        {
            const string xml = @"<Args>
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

            var vm = CreateServiceDesignerViewModel(instanceID, new[] { inputMapping.Object, outputMapping.Object }, worstError);
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

            Assert.IsTrue(vm.ShowLarge, "Fix errors failed to show the mapping.");

            // Called: 1 x ActivityViewModel constructor and 1 x DataMappingViewModel constructor
            inputMapping.Verify(p => p.SetValue(It.IsAny<object>()), Times.Exactly(4));
            outputMapping.Verify(p => p.SetValue(It.IsAny<object>()), Times.Exactly(4));

            // Always expect at least one error in the activity's error list - the no error
            Assert.AreEqual(ErrorType.None, vm.WorstError, "Fix errors failed to clear the error.");
            Assert.AreEqual(1, vm.DesignValidationErrors.Count, "Fix errors failed to remove the worst error from the activity.");

            Assert.AreEqual(0, vm.RootModel.Errors.Count, "Fix errors failed to remove the worst error from the activity's root model.");
            Assert.IsTrue(vm.IsWorstErrorReadOnly);
            vm.DoneCompletedCommand.Execute(null);
            Assert.IsTrue(vm.IsFixed);
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_FixErrors")]
        [Owner("Hagashen Naidu")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesignerViewModel_FixErrors_DoesNotSetResourceModelValidWhenResourceStillHasErrors()
        // ReSharper restore InconsistentNaming
        {
            //---------------------------------Setup-------------------------------------------------------------------------------------------------------
            const string xml = @"<Args>
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
            IErrorInfo[] resourceErrors = { worstError, new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Warning, FixType = FixType.ReloadMapping, FixData = xml }, new ErrorInfo { InstanceID = instanceID, ErrorType = ErrorType.Warning, FixType = FixType.ReloadMapping, FixData = xml } };
            var vm = CreateServiceDesignerViewModel(instanceID, new[] { inputMapping.Object, outputMapping.Object }, resourceErrors);
            vm.RootModel.AddError(resourceErrors[0]);
            vm.RootModel.AddError(resourceErrors[1]);
            //-----------------------------Assert Preconditions----------------------------------------------------------------------------
            Assert.IsFalse(vm.IsWorstErrorReadOnly);
            Assert.IsFalse(vm.IsFixed);
            Assert.AreEqual(3, vm.RootModel.Errors.Count);
            //-----------------------------Execute-----------------------------------------------------------------------------------------
            vm.DesignValidationErrors.RemoveAt(2);
            vm.DesignValidationErrors.RemoveAt(1);
            vm.FixErrorsCommand.Execute(null);
            Assert.IsTrue(vm.IsWorstErrorReadOnly);
            vm.DoneCompletedCommand.Execute(null);
            Assert.IsTrue(vm.IsFixed);
            Assert.IsTrue(vm.RootModel.HasErrors);
            Assert.AreEqual(2, vm.RootModel.Errors.Count);
            Assert.IsFalse(vm.RootModel.IsValid);
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_FixErrors")]
        [Owner("Travis Frisinger")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesignerViewModel_FixErrors_RequiredMapping_Done()
        // ReSharper restore InconsistentNaming
        {
            const string xml = @"<Args>
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

            var vm = CreateServiceDesignerViewModel(instanceID, new[] { inputMapping.Object, outputMapping.Object }, worstError);

            vm.FixErrorsCommand.Execute(null);
            var actualInputs = vm.DataMappingViewModel.Inputs;

            //asserts
            Assert.AreEqual(1, actualInputs.Count, "Fix errors returned an incorrect number of outputmappings");
            Assert.AreEqual("n1", actualInputs[0].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
            Assert.AreEqual(string.Empty, actualInputs[0].MapsTo, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect MapsTo value");

            Assert.IsTrue(vm.ShowLarge, "Fix errors failed to show the mapping.");

            // Simulate fixing error...
            vm.DataMappingViewModel.Inputs[0].MapsTo = string.Empty;
            vm.DataMappingViewModel.Inputs[0].Required = true;
            vm.ShowLarge = false;

            Assert.IsFalse(vm.ShowLarge, "Fix errors failed to show the mapping.");
            Assert.AreEqual(1, vm.DesignValidationErrors.Count, "Fix errors failed to remove the worst error from the activity.");
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_FixErrors")]
        [Description("FixErrors when FixType is MappingRequired must get a value for mapping to be fixed.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesignerViewModel_FixErrors_Required_Done()
        // ReSharper restore InconsistentNaming
        {
            const string xml = @"<Args>
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

            var vm = CreateServiceDesignerViewModel(instanceID, new[] { inputMapping.Object, outputMapping.Object }, worstError);

            vm.FixErrorsCommand.Execute(null);
            var actualInputs = vm.DataMappingViewModel.Inputs;

            //asserts
            Assert.AreEqual(1, actualInputs.Count, "Fix errors returned an incorrect number of outputmappings");
            Assert.AreEqual("n1", actualInputs[0].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
            Assert.AreEqual(string.Empty, actualInputs[0].MapsTo, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect MapsTo value");

            Assert.IsTrue(vm.ShowLarge, "Fix errors failed to show the mapping.");

            // Simulate fixing error...
            vm.DataMappingViewModel.Inputs[0].MapsTo = "somevalue";
            vm.ShowLarge = false;

            Assert.IsFalse(string.IsNullOrEmpty(vm.DataMappingViewModel.Inputs[0].MapsTo), "Test did not simulate fixing error by setting MapsTo.");
            Assert.IsFalse(vm.ShowLarge, "Fix errors failed to show the mapping.");

            // Always expect at least one error in the activity's error list - the no error
            Assert.AreEqual(ErrorType.None, vm.WorstError, "Fix errors failed to clear the error.");
            Assert.AreEqual(1, vm.DesignValidationErrors.Count, "Fix errors failed to remove the worst error from the activity.");

            Assert.AreEqual(0, vm.RootModel.Errors.Count, "Fix errors failed to remove the worst error from the activity's root model.");
        }


        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_FixErrors")]
        [Description("FixErrors when FixType is MappingRequired must get a value for mapping to be fixed.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesignerViewModel_FixErrors_RequiredWhenMappingValid_ShouldRemoveError()
        // ReSharper restore InconsistentNaming
        {
            const string xml = @"<Args>
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

            var vm = CreateServiceDesignerViewModel(instanceID, new[] { inputMapping.Object, outputMapping.Object }, worstError);

            var actualInputs = vm.DataMappingViewModel.Inputs;

            Assert.AreEqual(1, actualInputs.Count, "Fix errors returned an incorrect number of outputmappings");
            Assert.AreEqual("n1", actualInputs[0].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
            Assert.AreNotEqual(string.Empty, actualInputs[0].MapsTo, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect MapsTo value");
            Assert.IsFalse(vm.ShowLarge, "Fix errors failed to show the mapping.");

            // Always expect at least one error in the activity's error list - the no error
            Assert.AreEqual(ErrorType.None, vm.WorstError, "Fix errors failed to clear the error.");
            Assert.AreEqual(1, vm.DesignValidationErrors.Count, "Fix errors failed to remove the worst error from the activity.");

            Assert.AreEqual(0, vm.RootModel.Errors.Count, "Fix errors failed to remove the worst error from the activity's root model.");
        }

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_FixErrors")]
        [Description("FixErrors when FixType is MappingRequired must get a value for mapping to be fixed.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesignerViewModel_FixErrors_MulitpleRequiredWhenMappingValid_ShouldRemoveRequiredMappingErrors()
        // ReSharper restore InconsistentNaming
        {
            const string xml = @"<Args>
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

            var vm = CreateServiceDesignerViewModel(instanceID, new[] { inputMapping.Object, outputMapping.Object }, worstError, worstError);

            var actualInputs = vm.DataMappingViewModel.Inputs;

            Assert.AreEqual(1, actualInputs.Count, "Fix errors returned an incorrect number of outputmappings");
            Assert.AreEqual("n1", actualInputs[0].Name, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect Name value");
            Assert.AreNotEqual(string.Empty, actualInputs[0].MapsTo, "Fix errors failed to fix a mapping error. The first output mapping contains an incorrect MapsTo value");
            Assert.IsFalse(vm.ShowLarge, "Fix errors failed to show the mapping.");

            // Always expect at least one error in the activity's error list - the no error
            Assert.AreEqual(ErrorType.None, vm.WorstError, "Fix errors failed to clear the error.");
            Assert.AreEqual(1, vm.DesignValidationErrors.Count, "Fix errors failed to remove the worst error from the activity.");

            Assert.AreEqual(0, vm.RootModel.Errors.Count, "Fix errors failed to remove the worst error from the activity's root model.");
        }

        #endregion

        #region OnDesignValidationReceived

        [TestMethod]
        [TestCategory("ServiceDesignerViewModel_DesignValidationService")]
        [Description("Activity must receive memo's that match it's instance ID (unique ID).")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesignerViewModel_DesignValidation_ForThisActivity_Received()
        // ReSharper restore InconsistentNaming
        {
            var instanceID = Guid.NewGuid();

            var hitCount = 0;
            var model = CreateServiceDesignerViewModel(instanceID);
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
        [TestCategory("ServiceDesignerViewModel_DesignValidationService")]
        [Description("Activity must not receive memo's for other activities i.e. a different instance ID (unique ID).")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesignerViewModel_DesignValidation_ForOtherActivity_NotReceived()
        // ReSharper restore InconsistentNaming
        {
            var instanceID = Guid.NewGuid();
            var instanceID2 = Guid.NewGuid();

            var hitCount = 0;
            var model = CreateServiceDesignerViewModel(instanceID);
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


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceDesignerViewModel_UpdateMappings")]
        public void ServiceDesignerViewModel_UpdateMappings_SetsInputsAndOutputs()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var resourceModel = CreateResourceModel(resourceID, false);
            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(resourceID);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            var activity = new DsfActivity { ResourceID = new InArgument<Guid>(resourceID), EnvironmentID = new InArgument<Guid>(Guid.Empty), UniqueID = Guid.NewGuid().ToString(), SimulationMode = SimulationMode.OnDemand };

            var modelItem = CreateModelItem(activity);

            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object);

            var inputMapping = viewModel.ModelItem.GetProperty<string>("InputMapping");
            var outputMapping = viewModel.ModelItem.GetProperty<string>("OutputMapping");

            Assert.AreEqual("<Inputs><Input Name=\"n1\" Source=\"[[n1]]\" /></Inputs>", inputMapping);
            Assert.AreEqual("<Outputs><Output Name=\"n1\" MapsTo=\"[[n1]]\" Value=\"[[n1]]\" /></Outputs>", outputMapping);


            //------------Execute Test---------------------------
            viewModel.DataMappingViewModel.Inputs[0].MapsTo = "[[a1]]";
            viewModel.DataMappingViewModel.Outputs[0].Value = "[[b1]]";
            viewModel.UpdateMappings();

            //------------Assert Results-------------------------
            inputMapping = viewModel.ModelItem.GetProperty<string>("InputMapping");
            outputMapping = viewModel.ModelItem.GetProperty<string>("OutputMapping");

            Assert.AreEqual("<Inputs><Input Name=\"n1\" Source=\"[[a1]]\" /></Inputs>", inputMapping);
            Assert.AreEqual("<Outputs><Output Name=\"n1\" MapsTo=\"[[n1]]\" Value=\"[[b1]]\" /></Outputs>", outputMapping);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServiceDesignerViewModel_UpdateMappings")]
        public void ServiceDesignerViewModel_WhenRemoteResource_ShouldSetupResourceModelFromModelItem()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(conn => conn.ServerEvents).Returns(new EventPublisher());

            var environmentID = Guid.NewGuid();
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.Connection).Returns(connection.Object);
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(true);
            environment.Setup(model => model.IsLocalHost).Returns(false);
            environment.Setup(e => e.IsLocalHostCheck()).Returns(false);

            var errors = new ObservableReadOnlyList<IErrorInfo>();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceName).Returns("TestResource");
            resourceModel.Setup(r => r.ServerID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.WorkflowXaml).Returns(new StringBuilder("<root/>"));
            resourceModel.Setup(m => m.Errors).Returns(errors);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.Environment).Returns(environment.Object);
            resourceModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);
            resourceModel.Setup(m => m.HasErrors).Returns(() => false);
            resourceModel.SetupProperty(m => m.IsValid);
            resourceModel.Setup(m => m.RemoveError(It.IsAny<IErrorInfo>())).Callback((IErrorInfo error) => errors.Remove(error));

            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(resourceID);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);


            var activity = new DsfActivity
            {
                ResourceID = new InArgument<Guid>(resourceID),
                EnvironmentID = new InArgument<Guid>(Guid.Empty),
                UniqueID = Guid.NewGuid().ToString(),
                SimulationMode = SimulationMode.OnDemand,
                InputMapping = "<Inputs><Input Name=\"n1\" Source=\"[[n1]]\" /></Inputs>",
                OutputMapping = "<Outputs><Output Name=\"n1\" MapsTo=\"[[n1]]\" Value=\"[[n1]]\" /></Outputs>"
            };

            var modelItem = CreateModelItem(activity);
            var resRepo = new Mock<IResourceRepository>();
            var srcRes = new Mock<IResourceModel>();
            srcRes.Setup(a => a.ResourceName).Returns("bob");
            resRepo.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(srcRes.Object);
            environment.Setup(a => a.ResourceRepository).Returns(resRepo.Object);
            //------------Execute Test---------------------------
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            var inputMapping = viewModel.InputMapping;
            var outputMapping = viewModel.OutputMapping;

            Assert.AreEqual("<Inputs><Input Name=\"n1\" Source=\"[[n1]]\" /></Inputs>", inputMapping);
            Assert.AreEqual("<Outputs><Output Name=\"n1\" MapsTo=\"[[n1]]\" Value=\"[[n1]]\" /></Outputs>", outputMapping);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServiceDesignerViewModel_UpdateMappings")]
        public void ServiceDesignerViewModel_WhenResourceHasSource_ShouldGetSourceName()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(conn => conn.ServerEvents).Returns(new EventPublisher());

            var environmentID = Guid.NewGuid();
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.Connection).Returns(connection.Object);
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(true);
            environment.Setup(model => model.IsLocalHost).Returns(false);
            environment.Setup(e => e.IsLocalHostCheck()).Returns(false);

            var errors = new ObservableReadOnlyList<IErrorInfo>();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceName).Returns("TestResource");
            resourceModel.Setup(r => r.ServerID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.WorkflowXaml).Returns(new StringBuilder("<root/>"));
            resourceModel.Setup(m => m.Errors).Returns(errors);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.Environment).Returns(environment.Object);
            resourceModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);
            resourceModel.Setup(m => m.HasErrors).Returns(() => false);
            resourceModel.SetupProperty(m => m.IsValid);
            resourceModel.Setup(m => m.RemoveError(It.IsAny<IErrorInfo>())).Callback((IErrorInfo error) => errors.Remove(error));

            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(resourceID);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);


            var activity = new DsfActivity
            {
                ResourceID = new InArgument<Guid>(resourceID),
                EnvironmentID = new InArgument<Guid>(Guid.Empty),
                UniqueID = Guid.NewGuid().ToString(),
                SimulationMode = SimulationMode.OnDemand,
                InputMapping = "<Inputs><Input Name=\"n1\" Source=\"[[n1]]\" /></Inputs>",
                OutputMapping = "<Outputs><Output Name=\"n1\" MapsTo=\"[[n1]]\" Value=\"[[n1]]\" /></Outputs>"
            };

            var modelItem = CreateModelItem(activity);
            var resRepo = new Mock<IResourceRepository>();
            var srcRes = new Mock<IResourceModel>();
            srcRes.Setup(a => a.DisplayName).Returns("bob");
            resRepo.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(srcRes.Object);
            environment.Setup(a => a.ResourceRepository).Returns(resRepo.Object);
            //------------Execute Test---------------------------
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            Assert.AreEqual("bob", viewModel.Properties.FirstOrDefault(a => a.Key == "Source :").Value);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServiceDesignerViewModel_UpdateMappings")]
        public void ServiceDesignerViewModel_WhenRemoteResource_ShouldSubscibeToResourcesLoaded()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(conn => conn.ServerEvents).Returns(new EventPublisher());

            var environmentID = Guid.NewGuid();


            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.Connection).Returns(connection.Object);
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(true);
            environment.Setup(model => model.IsLocalHost).Returns(false);
            environment.Setup(e => e.IsLocalHostCheck()).Returns(false);

            var errors = new ObservableReadOnlyList<IErrorInfo>();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceName).Returns("TestResource");
            resourceModel.Setup(r => r.ServerID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.WorkflowXaml).Returns(new StringBuilder("<root/>"));
            resourceModel.Setup(m => m.Errors).Returns(errors);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.Environment).Returns(environment.Object);
            resourceModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);
            resourceModel.Setup(m => m.HasErrors).Returns(() => false);
            resourceModel.SetupProperty(m => m.IsValid);
            resourceModel.Setup(m => m.RemoveError(It.IsAny<IErrorInfo>())).Callback((IErrorInfo error) => errors.Remove(error));

            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");

            var resources = new Mock<IResourceRepository>();
            // ReSharper disable MaximumChainedReferences
            resources.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), true))
                  .Callback((Expression<Func<IResourceModel, bool>> expression, bool b) => Assert.IsTrue(expression.ToString().Contains("c => (c.ID == ")))
                  .Returns(resourceModel.Object).Verifiable();
            // ReSharper restore MaximumChainedReferences
            environment.Setup(a => a.ResourceRepository).Returns(resources.Object);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(resourceID);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            var activity = new DsfActivity
            {
                ResourceID = new InArgument<Guid>(resourceID),
                EnvironmentID = new InArgument<Guid>(Guid.Empty),
                UniqueID = Guid.NewGuid().ToString(),
                SimulationMode = SimulationMode.OnDemand,
                InputMapping = "<Inputs><Input Name=\"n1\" Source=\"[[n1]]\" /></Inputs>",
                OutputMapping = "<Outputs><Output Name=\"n1\" MapsTo=\"[[n1]]\" Value=\"[[n1]]\" /></Outputs>"
            };

            var modelItem = CreateModelItem(activity);
            var worker = new Mock<IAsyncWorker>();
            // ReSharper disable MaximumChainedReferences
            worker.Setup(a => a.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Verifiable();
            // ReSharper restore MaximumChainedReferences
            //------------Execute Test---------------------------
            // ReSharper disable UnusedVariable
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object, worker.Object);
            // ReSharper restore UnusedVariable
            environment.Setup(a => a.IsConnected).Returns(true);
            connection.Setup(a => a.Verify(It.IsAny<Action<ConnectResult>>(), true)).Verifiable();
            environment.Raise((a => a.ResourcesLoaded += null), new ResourcesLoadedEventArgs { Model = environment.Object });

            //------------Assert Results-------------------------
            worker.Verify(a => a.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>()));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServiceDesignerViewModel_UpdateMappings")]
        public void ServiceDesignerViewModel_WhenRemoteEnvironmentLoads_ShouldSetVersionMemoIfIncorrect()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(conn => conn.ServerEvents).Returns(new EventPublisher());

            var environmentID = Guid.NewGuid();

            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.Connection).Returns(connection.Object);
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(true);
            environment.Setup(model => model.IsLocalHost).Returns(false);
            environment.Setup(e => e.IsLocalHostCheck()).Returns(false);

            var errors = new ObservableReadOnlyList<IErrorInfo>();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceName).Returns("TestResource");
            resourceModel.Setup(r => r.ServerID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.WorkflowXaml).Returns(new StringBuilder("<root/>"));
            resourceModel.Setup(m => m.Errors).Returns(errors);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.Environment).Returns(environment.Object);
            resourceModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);
            resourceModel.Setup(m => m.HasErrors).Returns(() => false);
            resourceModel.SetupProperty(m => m.IsValid);
            resourceModel.Setup(m => m.RemoveError(It.IsAny<IErrorInfo>())).Callback((IErrorInfo error) => errors.Remove(error));

            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");

            var resources = new Mock<IResourceRepository>();
            // ReSharper disable MaximumChainedReferences
            resources.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), true))
                  .Callback((Expression<Func<IResourceModel, bool>> expression, bool b) => Assert.IsTrue(expression.ToString().Contains("c => (c.ID == ")))
                  .Returns(resourceModel.Object).Verifiable();
            // ReSharper restore MaximumChainedReferences
            environment.Setup(a => a.ResourceRepository).Returns(resources.Object);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(resourceID);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            var activity = new DsfActivity
            {
                ResourceID = new InArgument<Guid>(resourceID),
                EnvironmentID = new InArgument<Guid>(Guid.Empty),
                UniqueID = Guid.NewGuid().ToString(),
                SimulationMode = SimulationMode.OnDemand,
                InputMapping = "<Inputs><Input Name=\"n1\" Source=\"[[n1]]\" /></Inputs>",
                OutputMapping = "<Outputs><Output Name=\"n1\" MapsTo=\"[[n1]]\" Value=\"[[n1]]\" /></Outputs>"
            };

            var modelItem = CreateModelItem(activity);
            var worker = new Mock<IAsyncWorker>();
            // ReSharper disable MaximumChainedReferences
            worker.Setup(a => a.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Callback(
                // ReSharper restore MaximumChainedReferences
                (
                    System.Action a, System.Action b) =>
                {
                    a.Invoke();
                    b.Invoke();
                }
                );
            //------------Execute Test---------------------------
            // ReSharper disable UnusedVariable
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object, worker.Object);

            //------------Events Setup---------------------------
            var mappingF = new Mock<IDataMappingViewModelFactory>();
            var mapping = new Mock<IDataMappingViewModel>();
            mapping.Setup(a => a.GetInputString(It.IsAny<IList<IInputOutputViewModel>>())).Returns("bob");
            // ReSharper disable MaximumChainedReferences
            mappingF.Setup(a => a.CreateModel(It.IsAny<IWebActivity>(), It.IsAny<NotifyCollectionChangedEventHandler>()))
                    .Returns(mapping.Object);
            // ReSharper restore MaximumChainedReferences
            viewModel.MappingFactory = mappingF.Object;
            // ReSharper restore UnusedVariable
            environment.Setup(a => a.IsConnected).Returns(true);
            connection.Setup(a => a.Verify(It.IsAny<Action<ConnectResult>>(), true)).Verifiable();
            environment.Raise((a => a.ResourcesLoaded += null), new ResourcesLoadedEventArgs { Model = environment.Object });

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.LastValidationMemo.Errors.First().Message.Contains("Incorrect Version. The remote workflow has changed.Please refresh"));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServiceDesignerViewModel_FixErrorss")]
        public void ServiceDesignerViewModel_WhenUserFixesErrors_ShouldRemoveVersionError()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(conn => conn.ServerEvents).Returns(new EventPublisher());

            var environmentID = Guid.NewGuid();


            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.Connection).Returns(connection.Object);
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(true);
            environment.Setup(model => model.IsLocalHost).Returns(false);
            environment.Setup(e => e.IsLocalHostCheck()).Returns(false);

            var errors = new ObservableReadOnlyList<IErrorInfo>();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceName).Returns("TestResource");
            resourceModel.Setup(r => r.ServerID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.WorkflowXaml).Returns(new StringBuilder("<root/>"));
            resourceModel.Setup(m => m.Errors).Returns(errors);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.Environment).Returns(environment.Object);
            resourceModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);
            resourceModel.Setup(m => m.HasErrors).Returns(() => false);
            resourceModel.SetupProperty(m => m.IsValid);
            resourceModel.Setup(m => m.RemoveError(It.IsAny<IErrorInfo>())).Callback((IErrorInfo error) => errors.Remove(error));

            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");

            var resources = new Mock<IResourceRepository>();
            // ReSharper disable MaximumChainedReferences
            resources.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), true))
                  .Callback((Expression<Func<IResourceModel, bool>> expression, bool b) => Assert.IsTrue(expression.ToString().Contains("c => (c.ID == ")))
                  .Returns(resourceModel.Object).Verifiable();
            // ReSharper restore MaximumChainedReferences
            environment.Setup(a => a.ResourceRepository).Returns(resources.Object);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(resourceID);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            var activity = new DsfActivity
            {
                ResourceID = new InArgument<Guid>(resourceID),
                EnvironmentID = new InArgument<Guid>(Guid.Empty),
                UniqueID = Guid.NewGuid().ToString(),
                SimulationMode = SimulationMode.OnDemand,
                InputMapping = "<Inputs><Input Name=\"n1\" Source=\"[[n1]]\" /></Inputs>",
                OutputMapping = "<Outputs><Output Name=\"n1\" MapsTo=\"[[n1]]\" Value=\"[[n1]]\" /></Outputs>"
            };

            var modelItem = CreateModelItem(activity);
            var worker = new Mock<IAsyncWorker>();
            // ReSharper disable MaximumChainedReferences
            worker.Setup(a => a.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Callback(
                // ReSharper restore MaximumChainedReferences
                (
                    System.Action a, System.Action b) =>
                {
                    a.Invoke();
                    b.Invoke();
                }
                );
            //------------Execute Test---------------------------
            // ReSharper disable UnusedVariable
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object, worker.Object);

            //------------Events Setup---------------------------
            var mappingF = new Mock<IDataMappingViewModelFactory>();
            var mapping = new Mock<IDataMappingViewModel>();
            mapping.Setup(a => a.GetInputString(It.IsAny<IList<IInputOutputViewModel>>())).Returns("bob");
            // ReSharper disable MaximumChainedReferences
            mappingF.Setup(a => a.CreateModel(It.IsAny<IWebActivity>(), It.IsAny<NotifyCollectionChangedEventHandler>()))
                    .Returns(mapping.Object);
            // ReSharper restore MaximumChainedReferences
            viewModel.MappingFactory = mappingF.Object;
            // ReSharper restore UnusedVariable
            environment.Setup(a => a.IsConnected).Returns(true);
            connection.Setup(a => a.Verify(It.IsAny<Action<ConnectResult>>(), true)).Verifiable();
            environment.Raise((a => a.ResourcesLoaded += null), new ResourcesLoadedEventArgs { Model = environment.Object });

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.LastValidationMemo.Errors.First().Message.Contains("Incorrect Version. The remote workflow has changed.Please refresh"));
            viewModel.FixErrorsCommand.Execute(this);
            Assert.IsTrue(viewModel.DesignValidationErrors.First().Message.Contains("Service Working Normally"));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServiceDesignerViewModel_UpdateMappings")]
        public void ServiceDesignerViewModel_WhenRemoteEnvironmentLoads_ShouldSetVersionMemoIfIncorrectWhen_OutPutDifferent()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(conn => conn.ServerEvents).Returns(new EventPublisher());

            var environmentID = Guid.NewGuid();


            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.Connection).Returns(connection.Object);
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(true);
            environment.Setup(model => model.IsLocalHost).Returns(false);
            environment.Setup(e => e.IsLocalHostCheck()).Returns(false);

            var errors = new ObservableReadOnlyList<IErrorInfo>();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceName).Returns("TestResource");
            resourceModel.Setup(r => r.ServerID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.WorkflowXaml).Returns(new StringBuilder("<root/>"));
            resourceModel.Setup(m => m.Errors).Returns(errors);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.Environment).Returns(environment.Object);
            resourceModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);
            resourceModel.Setup(m => m.HasErrors).Returns(() => false);
            resourceModel.SetupProperty(m => m.IsValid);
            resourceModel.Setup(m => m.RemoveError(It.IsAny<IErrorInfo>())).Callback((IErrorInfo error) => errors.Remove(error));

            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");

            var resources = new Mock<IResourceRepository>();
            // ReSharper disable MaximumChainedReferences
            resources.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), true))
                  .Callback((Expression<Func<IResourceModel, bool>> expression, bool b) => Assert.IsTrue(expression.ToString().Contains("c => (c.ID == ")))
                  .Returns(resourceModel.Object).Verifiable();
            // ReSharper restore MaximumChainedReferences
            environment.Setup(a => a.ResourceRepository).Returns(resources.Object);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(resourceID);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            var activity = new DsfActivity
            {
                ResourceID = new InArgument<Guid>(resourceID),
                EnvironmentID = new InArgument<Guid>(Guid.Empty),
                UniqueID = Guid.NewGuid().ToString(),
                SimulationMode = SimulationMode.OnDemand,
                InputMapping = "<Inputs><Input Name=\"n1\" Source=\"[[n1]]\" /></Inputs>",
                OutputMapping = "<Outputs><Output Name=\"n1\" MapsTo=\"[[n1]]\" Value=\"[[n1]]\" /></Outputs>"
            };

            var modelItem = CreateModelItem(activity);
            var worker = new Mock<IAsyncWorker>();
            // ReSharper disable MaximumChainedReferences
            worker.Setup(a => a.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Callback(
                // ReSharper restore MaximumChainedReferences
                (
                    System.Action a, System.Action b) =>
                {
                    a.Invoke();
                    b.Invoke();
                }
                );
            //------------Execute Test---------------------------
            // ReSharper disable UnusedVariable
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object, worker.Object);

            //------------Events Setup---------------------------
            var mappingF = new Mock<IDataMappingViewModelFactory>();
            var mapping = new Mock<IDataMappingViewModel>();
            mapping.Setup(a => a.GetOutputString(It.IsAny<IList<IInputOutputViewModel>>())).Returns("bob");
            // ReSharper disable MaximumChainedReferences
            mappingF.Setup(a => a.CreateModel(It.IsAny<IWebActivity>(), It.IsAny<NotifyCollectionChangedEventHandler>()))
                    .Returns(mapping.Object);
            // ReSharper restore MaximumChainedReferences
            viewModel.MappingFactory = mappingF.Object;
            // ReSharper restore UnusedVariable
            environment.Setup(a => a.IsConnected).Returns(true);
            connection.Setup(a => a.Verify(It.IsAny<Action<ConnectResult>>(), true)).Verifiable();
            environment.Raise((a => a.ResourcesLoaded += null), new ResourcesLoadedEventArgs { Model = environment.Object });

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.LastValidationMemo.Errors.First().Message.Contains("Incorrect Version. The remote workflow has changed.Please refresh"));

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServiceDesignerViewModel_UpdateMappings")]
        public void ServiceDesignerViewModel_WhenRemoteEnvironmentLoads_ShouldNotSetVersionMemoIfCorrect()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(conn => conn.ServerEvents).Returns(new EventPublisher());

            var environmentID = Guid.NewGuid();


            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.Connection).Returns(connection.Object);
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(true);
            environment.Setup(model => model.IsLocalHost).Returns(false);
            environment.Setup(model => model.IsLocalHostCheck()).Returns(false);

            var errors = new ObservableReadOnlyList<IErrorInfo>();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceName).Returns("TestResource");
            resourceModel.Setup(r => r.ServerID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.WorkflowXaml).Returns(new StringBuilder("<root/>"));
            resourceModel.Setup(m => m.Errors).Returns(errors);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.Environment).Returns(environment.Object);
            resourceModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);
            resourceModel.Setup(m => m.HasErrors).Returns(() => false);
            resourceModel.SetupProperty(m => m.IsValid);
            resourceModel.Setup(m => m.RemoveError(It.IsAny<IErrorInfo>())).Callback((IErrorInfo error) => errors.Remove(error));

            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");

            var resources = new Mock<IResourceRepository>();
            // ReSharper disable MaximumChainedReferences
            resources.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), true))
                  .Callback((Expression<Func<IResourceModel, bool>> expression, bool b) => Assert.IsTrue(expression.ToString().Contains("c => (c.ID == ")))
                  .Returns(resourceModel.Object).Verifiable();
            // ReSharper restore MaximumChainedReferences
            environment.Setup(a => a.ResourceRepository).Returns(resources.Object);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(resourceID);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            var activity = new DsfActivity
            {
                ResourceID = new InArgument<Guid>(resourceID),
                EnvironmentID = new InArgument<Guid>(Guid.Empty),
                UniqueID = Guid.NewGuid().ToString(),
                SimulationMode = SimulationMode.OnDemand,
                InputMapping = "<Inputs></Inputs>",
                OutputMapping = "<Outputs></Outputs>"
            };

            var modelItem = CreateModelItem(activity);
            var worker = new Mock<IAsyncWorker>();
            // ReSharper disable MaximumChainedReferences
            worker.Setup(a => a.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>())).Callback(
                // ReSharper restore MaximumChainedReferences
                (
                    System.Action a, System.Action b) =>
                {
                    a.Invoke();
                    b.Invoke();
                }
                );
            //------------Execute Test---------------------------
            // ReSharper disable UnusedVariable
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object, worker.Object);
            // ReSharper restore UnusedVariable
            environment.Setup(a => a.IsConnected).Returns(true);
            connection.Setup(a => a.Verify(It.IsAny<Action<ConnectResult>>(), true)).Verifiable();
            environment.Raise((a => a.ResourcesLoaded += null), new ResourcesLoadedEventArgs { Model = environment.Object });

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.LastValidationMemo.Errors.Count == 0);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServiceDesignerViewModel_UpdateMappings")]
        public void ServiceDesignerViewModel_WhenRemoteEnvironmentLoads_ShouldUseNameToFindEnvironment()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.Empty;

            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(conn => conn.ServerEvents).Returns(new EventPublisher());

            var environmentID = Guid.NewGuid();


            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.Connection).Returns(connection.Object);
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(true);
            environment.Setup(model => model.IsLocalHost).Returns(false);
            environment.Setup(e => e.IsLocalHostCheck()).Returns(false);

            var errors = new ObservableReadOnlyList<IErrorInfo>();

            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceName).Returns("TestResource").Verifiable();
            resourceModel.Setup(r => r.ServerID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.WorkflowXaml).Returns(new StringBuilder("<root/>"));
            resourceModel.Setup(m => m.Errors).Returns(errors);
            resourceModel.Setup(m => m.ID).Returns(resourceID);
            resourceModel.Setup(m => m.Environment).Returns(environment.Object);
            resourceModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);
            resourceModel.Setup(m => m.HasErrors).Returns(() => false);
            resourceModel.SetupProperty(m => m.IsValid);
            resourceModel.Setup(m => m.RemoveError(It.IsAny<IErrorInfo>())).Callback((IErrorInfo error) => errors.Remove(error));

            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");

            var resources = new Mock<IResourceRepository>();

            environment.Setup(a => a.ResourceRepository).Returns(resources.Object);
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(resourceID);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            var activity = new DsfActivity
            {
                ServiceName = "bob",
                ResourceID = new InArgument<Guid>(resourceID),
                EnvironmentID = new InArgument<Guid>(Guid.Empty),
                UniqueID = Guid.Empty.ToString(),
                SimulationMode = SimulationMode.OnDemand,
                InputMapping = "<Inputs><Input Name=\"n1\" Source=\"[[n1]]\" /></Inputs>",
                OutputMapping = "<Outputs><Output Name=\"n1\" MapsTo=\"[[n1]]\" Value=\"[[n1]]\" /></Outputs>"
            };

            var modelItem = CreateModelItem(activity);
            var worker = new Mock<IAsyncWorker>();
            // ReSharper disable MaximumChainedReferences
            worker.Setup(a => a.Start(It.IsAny<System.Action>(), It.IsAny<System.Action>()))
                .Callback((System.Action a, System.Action b) =>
                    // ReSharper restore MaximumChainedReferences
                {
                    a.Invoke();
                    b.Invoke();
                }
                );
            //------------Execute Test---------------------------
            // ReSharper disable UnusedVariable


            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object, worker.Object);
            var webFact = new Mock<IWebActivityFactory>();
            var wa = new Mock<IWebActivity>();
            // ReSharper disable MaximumChainedReferences
            webFact.Setup(
                a => a.CreateWebActivity(It.IsAny<Object>(), It.IsAny<IContextualResourceModel>(), It.IsAny<string>())).Returns(wa.Object).Verifiable();
            // ReSharper restore MaximumChainedReferences
            viewModel.ActivityFactory = webFact.Object;

            var mappingF = new Mock<IDataMappingViewModelFactory>();
            var mapping = new Mock<IDataMappingViewModel>();
            mapping.Setup(a => a.GetOutputString(It.IsAny<IList<IInputOutputViewModel>>())).Returns("bob");
            // ReSharper disable once MaximumChainedReferences
            mappingF.Setup(a => a.CreateModel(It.IsAny<IWebActivity>(), It.IsAny<NotifyCollectionChangedEventHandler>()))
                    .Returns(mapping.Object);
            viewModel.MappingFactory = mappingF.Object;

            // ReSharper restore UnusedVariable
            // ReSharper disable MaximumChainedReferences
            // ReSharper disable MaximumChainedReferences
            resources.Setup(a => a.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), true))
                .Returns(resourceModel.Object)
                .Callback((Expression<Func<IResourceModel, bool>> expression, bool b) => Assert.IsTrue(expression.ToString().Contains("c => (c.ResourceName == ")))

                .Verifiable();
            // ReSharper restore MaximumChainedReferences
            environment.Setup(a => a.IsConnected).Returns(true);

            environment.Raise((a => a.ResourcesLoaded += null), new ResourcesLoadedEventArgs { Model = environment.Object });

            //------------Assert Results-------------------------

            Assert.IsTrue(viewModel.LastValidationMemo.Errors.First().Message.Contains("Incorrect Version. The remote workflow has changed.Please refresh"));

        }



        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServiceDesignerViewModel_UpdateMappings")]
        public void ServiceDesignerViewModel_InitializeResourceIDNull_StillCorrectlySetsUp()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var resourceModel = CreateResourceModel(Guid.Empty, false);
            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");

            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(Guid.Empty);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            var activity = new DsfActivity { ResourceID = new InArgument<Guid>(resourceID), EnvironmentID = new InArgument<Guid>(Guid.Empty), UniqueID = Guid.NewGuid().ToString(), SimulationMode = SimulationMode.OnDemand };

            var modelItem = CreateModelItem(activity);

            //------------Execute Test---------------------------
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ResourceModel);
            var inputMapping = viewModel.ModelItem.GetProperty<string>("InputMapping");
            var outputMapping = viewModel.ModelItem.GetProperty<string>("OutputMapping");

            Assert.AreEqual("<Inputs><Input Name=\"n1\" Source=\"[[n1]]\" /></Inputs>", inputMapping);
            Assert.AreEqual("<Outputs><Output Name=\"n1\" MapsTo=\"[[n1]]\" Value=\"[[n1]]\" /></Outputs>", outputMapping);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceDesignerViewModel_Constructor")]
        public void ServiceDesignerViewModel_InitializeWhenEnvironmentModelOnOtherServer_StillCorrectlySetsUp()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var resourceModel = CreateResourceModel(Guid.Empty, false);
            ISetup<IContextualResourceModel, string> setupResourceModel = resourceModel.Setup(model => model.DataList);
            setupResourceModel.Returns("<DataList><n1/></DataList>");


            var rootModel = CreateResourceModel(Guid.Empty);

            var envRepository = new Mock<IEnvironmentRepository>();
            ISetup<IEnvironmentRepository, IEnvironmentModel> setupFindSingle = envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>()));
            setupFindSingle.Returns((IEnvironmentModel)null);
            ISetup<IEnvironmentRepository, IEnvironmentModel> setupActiveEnvironment = envRepository.Setup(r => r.ActiveEnvironment);
            setupActiveEnvironment.Returns(resourceModel.Object.Environment);

            var activity = new DsfActivity { ResourceID = new InArgument<Guid>(resourceID), EnvironmentID = new InArgument<Guid>(rootModel.Object.Environment.ID), UniqueID = Guid.NewGuid().ToString(), SimulationMode = SimulationMode.OnDemand };

            var modelItem = CreateModelItem(activity);

            //------------Execute Test---------------------------
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ResourceModel);

        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ServiceDesingerViewModel_RunWorkflowAsync")]
        public void ServiceDesingerViewModel_RunWorkflowAsync_Constructor_False()
        {

            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var resourceModel = CreateResourceModel(Guid.Empty, false);
            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(Guid.Empty);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            var activity = new DsfActivity { ResourceID = new InArgument<Guid>(resourceID), EnvironmentID = new InArgument<Guid>(Guid.Empty), UniqueID = Guid.NewGuid().ToString(), SimulationMode = SimulationMode.OnDemand };

            var modelItem = CreateModelItem(activity);
            //------------Execute Test---------------------------
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.RunWorkflowAsync);
            Assert.IsTrue(viewModel.OutputMappingEnabled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ServiceDesingerViewModel_RunWorkflowAsync")]
        public void ServiceDesingerViewModel_SetRunWorkflowAsync_True_OutputMappingEnabledFalse()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var resourceModel = CreateResourceModel(Guid.Empty, false);
            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(Guid.Empty);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            var activity = new DsfActivity { ResourceID = new InArgument<Guid>(resourceID), EnvironmentID = new InArgument<Guid>(Guid.Empty), UniqueID = Guid.NewGuid().ToString(), SimulationMode = SimulationMode.OnDemand };

            var modelItem = CreateModelItem(activity);
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object) { RunWorkflowAsync = true };
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.RunWorkflowAsync);
            Assert.IsFalse(viewModel.OutputMappingEnabled);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ServiceDesingerViewModel_IsAsyncVisible")]
        public void ServiceDesingerViewModel_IsAsyncVisible_WorkflowResource_True()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var resourceModel = CreateResourceModel(Guid.Empty, false);
            resourceModel.Setup(model => model.ServerResourceType).Returns("Workflow");
            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(Guid.Empty);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            var resourceType = resourceModel.Object.ServerResourceType;
            var activity = new DsfActivity { ResourceID = new InArgument<Guid>(resourceID), EnvironmentID = new InArgument<Guid>(Guid.Empty), UniqueID = Guid.NewGuid().ToString(), SimulationMode = SimulationMode.OnDemand, Type = new InArgument<string>(resourceType) };

            var modelItem = CreateModelItem(activity);
            //------------Execute Test---------------------------
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsAsyncVisible);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ServiceDesingerViewModel_IsAsyncVisible")]
        public void ServiceDesingerViewModel_IsAsyncVisible_NotWorkflowResource_False()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();

            var resourceModel = CreateResourceModel(Guid.Empty, false);
            resourceModel.Setup(model => model.ResourceType).Returns(Studio.Core.AppResources.Enums.ResourceType.Service);
            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);

            var rootModel = CreateResourceModel(Guid.Empty);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            var resourceType = resourceModel.Object.ResourceType.ToString();
            var activity = new DsfActivity { ResourceID = new InArgument<Guid>(resourceID), EnvironmentID = new InArgument<Guid>(Guid.Empty), UniqueID = Guid.NewGuid().ToString(), SimulationMode = SimulationMode.OnDemand, Type = new InArgument<string>(resourceType) };

            var modelItem = CreateModelItem(activity);
            //------------Execute Test---------------------------
            var viewModel = new ServiceDesignerViewModel(modelItem, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.IsAsyncVisible);
        }

        ///////////////////////////////////////////////
        // Static Helpers
        ///////////////////////////////////////////////

        #region CreateModelItem

        public static Mock<ModelItem> CreateModelItem(Guid uniqueID, Guid serviceID, Guid environmentID, params ModelProperty[] modelProperties)
        {
            const int OffSet = 4;
            var startIndex = 0;
            if (modelProperties == null)
            {
                modelProperties = new ModelProperty[OffSet];
            }
            else
            {
                startIndex = modelProperties.Length;
                Array.Resize(ref modelProperties, startIndex + OffSet);
            }

            modelProperties[startIndex++] = CreateModelProperty("UniqueID", uniqueID.ToString()).Object;
            modelProperties[startIndex++] = CreateModelProperty("ResourceID", serviceID).Object;
            modelProperties[startIndex++] = CreateModelProperty("EnvironmentID", new InArgument<Guid>(environmentID)).Object;
            modelProperties[startIndex] = CreateModelProperty("ServiceName", "TestResource").Object;

            var properties = new Mock<ModelPropertyCollection>();

            foreach (var modelProperty in modelProperties)
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
            resourceRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), true)).Returns(resourceRepositoryReturnsNull ? null : resourceModel.Object);
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
            environment.Setup(e => e.HasLoadedResources).Returns(true);
            environment.Setup(e => e.IsLocalHost).Returns(true);
            environment.Setup(e => e.IsLocalHostCheck()).Returns(true);
            var errors = new ObservableReadOnlyList<IErrorInfo>();
            if (resourceErrors != null)
            {
                foreach (var resourceError in resourceErrors)
                {
                    errors.Add(resourceError);
                }
            }

            var model = new Mock<IContextualResourceModel>();
            model.Setup(r => r.ResourceName).Returns("TestResource");
            model.Setup(r => r.ServerID).Returns(Guid.NewGuid());
            model.Setup(r => r.WorkflowXaml).Returns(new StringBuilder("<root/>"));
            model.Setup(m => m.Errors).Returns(errors);
            model.Setup(m => m.ID).Returns(resourceID);
            var environmentModel = environment.Object;
            model.Setup(m => m.Environment).Returns(environmentModel);
            model.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(errors);
            model.Setup(m => m.HasErrors).Returns(() => model.Object.Errors.Count > 0);
            model.SetupProperty(m => m.IsValid);
            model.Setup(m => m.RemoveError(It.IsAny<IErrorInfo>())).Callback((IErrorInfo error) => errors.Remove(error));

            resourceRepository = new Mock<IResourceRepository>();
            var mockEnvironmentRepository = new Mock<IEnvironmentRepository>();
            mockEnvironmentRepository.Setup(e => e.LookupEnvironments(It.IsAny<IEnvironmentModel>(), null)).Returns(new List<IEnvironmentModel> { environmentModel });
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(mockEnvironmentRepository.Object);
            // ReSharper restore ObjectCreationAsStatement
            environment.Setup(e => e.ResourceRepository).Returns(resourceRepository.Object);
            return model;
        }

        #endregion

        #region CreateServiceDesignerViewModel

        static ServiceDesignerViewModel CreateServiceDesignerViewModelWithEmptyResourceID(Guid instanceID, params IErrorInfo[] resourceErrors)
        {
            var rootModel = CreateResourceModel(Guid.NewGuid(), false, resourceErrors);
            rootModel.Setup(model => model.ResourceType).Returns(Studio.Core.AppResources.Enums.ResourceType.WorkflowService);
            var resourceModel = CreateResourceModel(Guid.Empty, false);
            resourceModel.Setup(model => model.ResourceType).Returns(Studio.Core.AppResources.Enums.ResourceType.WorkflowService);
            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");
            resourceModel.Setup(model => model.ResourceName).Returns("TestResource");

            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);
            var modelItem = CreateModelItem(instanceID, resourceModel.Object.ID, resourceModel.Object.Environment.ID, null);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(e => e.ActiveEnvironment).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            var worker = new Mock<IAsyncWorker>().Object;
            return new ServiceDesignerViewModel(modelItem.Object, rootModel.Object, envRepository.Object, new Mock<IEventAggregator>().Object, worker);
        }

        static ServiceDesignerViewModel CreateServiceDesignerViewModel(Guid instanceID, params IErrorInfo[] resourceErrors)
        {
            return CreateServiceDesignerViewModel(instanceID, null, resourceErrors);
        }

        static ServiceDesignerViewModel CreateServiceDesignerViewModel(Guid instanceID, ModelProperty[] modelProperties, params IErrorInfo[] resourceErrors)
        {
            return CreateServiceDesignerViewModel(instanceID, false, modelProperties, resourceErrors);
        }

        static ServiceDesignerViewModel CreateServiceDesignerViewModel(Guid instanceID, bool resourceRepositoryReturnsNull, ModelProperty[] modelProperties, params IErrorInfo[] resourceErrors)
        {
            return CreateServiceDesignerViewModel(instanceID, resourceRepositoryReturnsNull, new Mock<IEventAggregator>().Object, modelProperties, resourceErrors);
        }

        static ServiceDesignerViewModel CreateServiceDesignerViewModel(Guid instanceID, bool resourceRepositoryReturnsNull, IEventAggregator eventPublisher, ModelProperty[] modelProperties, params IErrorInfo[] resourceErrors)
        {
            var rootModel = CreateResourceModel(Guid.NewGuid(), resourceRepositoryReturnsNull, resourceErrors);
            rootModel.Setup(model => model.ResourceType).Returns(Studio.Core.AppResources.Enums.ResourceType.WorkflowService);
            var resourceModel = CreateResourceModel(Guid.NewGuid(), resourceRepositoryReturnsNull);
            resourceModel.Setup(model => model.ResourceType).Returns(Studio.Core.AppResources.Enums.ResourceType.WorkflowService);
            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");

            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);
            var modelItem = CreateModelItem(instanceID, resourceModel.Object.ID, resourceModel.Object.Environment.ID, modelProperties);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);
            var worker = new Mock<IAsyncWorker>().Object;
            return new ServiceDesignerViewModel(modelItem.Object, rootModel.Object, envRepository.Object, eventPublisher, worker);
        }

        static ServiceDesignerViewModel CreateServiceDesignerViewModel(Guid instanceID, bool resourceRepositoryReturnsNull, IEventAggregator eventPublisher, ModelProperty[] modelProperties, Mock<IContextualResourceModel> resourceModel, params IErrorInfo[] resourceErrors)
        {
            var rootModel = CreateResourceModel(Guid.NewGuid(), resourceRepositoryReturnsNull, resourceErrors);
            resourceModel.Setup(model => model.DataList).Returns("<DataList><n1/></DataList>");
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(resourceModel.Object);
            dataListViewModel.ScalarCollection.Add(new DataListItemModel("n1"));
            DataListSingleton.SetDataList(dataListViewModel);
            var modelItem = CreateModelItem(instanceID, resourceModel.Object.ID, resourceModel.Object.Environment.ID, modelProperties);

            var envRepository = new Mock<IEnvironmentRepository>();
            envRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(resourceModel.Object.Environment);
            envRepository.Setup(r => r.ActiveEnvironment).Returns(resourceModel.Object.Environment);

            return new ServiceDesignerViewModel(modelItem.Object, rootModel.Object, envRepository.Object, eventPublisher);
        }

        static Mock<IResourceRepository> SetupForSourceCheck(out Guid instanceID, out Mock<IEnvironmentModel> environment, out Mock<IContextualResourceModel> resourceModel, out Guid sourceID, bool mangleXaml = false)
        {
            Mock<IResourceRepository> mockRepo = new Mock<IResourceRepository>();
            instanceID = Guid.NewGuid();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(conn => conn.ServerEvents).Returns(new EventPublisher());

            var environmentID = Guid.NewGuid();
            environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.Connection).Returns(connection.Object);
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.IsConnected).Returns(true);

            const string src = @"1afe38e9-a6f5-403d-9e52-06dd7ae11198";
            string xaml = string.Format(@"<Action Name=""foobar"" Type=""InvokeWebService"" SourceID=""{0}"" SourceName=""dummy"" SourceMethod="""" RequestUrl="""" RequestMethod=""Post"" JsonPath=""""></Action>", src);

            if (mangleXaml)
            {
                xaml += "<foo./>.</";
            }

            resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(r => r.ResourceName).Returns("TestResource");
            resourceModel.Setup(r => r.ServerID).Returns(Guid.NewGuid());
            resourceModel.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(xaml));
            resourceModel.Setup(m => m.ID).Returns(instanceID);
            resourceModel.Setup(m => m.Environment).Returns(environment.Object);
            resourceModel.SetupProperty(m => m.IsValid);
            sourceID = new Guid(src);
            return mockRepo;
        }

        #endregion


        static ModelItem CreateModelItem(DsfActivity activity)
        {
            return ModelItemUtils.CreateModelItem(activity);
        }

        static ServiceDesignerViewModel GenerateServiceDesignerViewModel(string name, Mock<IContextualResourceModel> myModel = null, string type = null, string serviceURI = null)
        {

            var resourceID = Guid.NewGuid();
            var rootModel = myModel;

            if (myModel == null)
            {
                rootModel = new Mock<IContextualResourceModel>();
                rootModel.Setup(m => m.Errors.Count).Returns(0);
                rootModel.Setup(m => m.GetErrors(It.IsAny<Guid>())).Returns(new List<IErrorInfo>());
            }

            var activity = new DsfActivity { ResourceID = new InArgument<Guid>(resourceID), EnvironmentID = new InArgument<Guid>(Guid.Empty), UniqueID = Guid.NewGuid().ToString(), SimulationMode = SimulationMode.OnDemand, ServiceName = name };

            if (type != null)
            {
                activity.Type = type;
            }

            if (serviceURI != null)
            {
                activity.ServiceUri = serviceURI;
            }

            var modelItem = CreateModelItem(activity);

            // setup the resource repository
            var resourceRepository = new Mock<IResourceRepository>();

            resourceRepository.Setup(r => r.IsLoaded).Returns(true);
            resourceRepository.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), true)).Returns(rootModel.Object);

            // setup active environment
            var activeEnvironment = new Mock<IEnvironmentModel>();
            activeEnvironment.Setup(a => a.IsLocalHostCheck()).Returns(false);
            activeEnvironment.Setup(a => a.IsLocalHost).Returns(false);
            activeEnvironment.Setup(a => a.HasLoadedResources).Returns(true);
            activeEnvironment.Setup(a => a.IsConnected).Returns(true);
            activeEnvironment.Setup(a => a.ResourceRepository).Returns(resourceRepository.Object);

            // setup the rootModel to return the insane levels of nested junk
            rootModel.Setup(r => r.Environment).Returns(activeEnvironment.Object);

            // setup the environment repository
            var environmentRepository = new Mock<IEnvironmentRepository>();
            environmentRepository.Setup(e => e.ActiveEnvironment).Returns(activeEnvironment.Object);

            // ReSharper disable ObjectCreationAsStatement
            var model = new ServiceDesignerViewModel(modelItem, rootModel.Object, environmentRepository.Object, new Mock<IEventAggregator>().Object);
            // ReSharper restore ObjectCreationAsStatement
            return model;
        }

    }
}
