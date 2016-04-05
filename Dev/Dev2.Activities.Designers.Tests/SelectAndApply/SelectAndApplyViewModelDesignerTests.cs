using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Security.Permissions;
using System.Windows;
using Castle.DynamicProxy.Generators;
using Dev2.Activities.Designers2.SelectAndApply;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Core.Tests.Environments;
using Dev2.Models;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.SelectAndApply
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SelectAndApplyViewModelDesignerTests
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            AttributesToAvoidReplicating.Add(typeof(UIPermissionAttribute));
        }
        private ModelItem CreateModelItem(object activity)
        {
            return ModelItemUtils.CreateModelItem(activity);
        }

        private SelectAndApplyDesignerViewModel CreateViewModel(ModelItem modelItem)
        {
            return new SelectAndApplyDesignerViewModel(modelItem);
        }

        static void SetupEnvironmentRepo(Guid environmentId)
        {
            var mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(environmentId);
            Mock<IResourceRepository> mockResRepo = new Mock<IResourceRepository>();
            mockResRepo.Setup(d => d.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(new TestDataWithContexResourceModel().DataContext);
            mockEnvironment.Setup(c => c.ResourceRepository).Returns(mockResRepo.Object);
            GetEnvironmentRepository(mockEnvironment);
        }

        private static void GetEnvironmentRepository(Mock<IEnvironmentModel> mockEnvironment)
        {
            var repo = new TestLoadEnvironmentRespository(mockEnvironment.Object) { IsLoaded = true };
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(repo);
            // ReSharper restore ObjectCreationAsStatement
            repo.ActiveEnvironment = mockEnvironment.Object;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenModelItem_ShouldHaveViewModelInstance()
        {
            //---------------Set up test pack-------------------
            var modelItem = CreateModelItem(new DsfSelectAndApplyActivity());
            var vm = CreateViewModel(modelItem);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(vm);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenModelItemWithApplyActivity_ShouldSetApplyActivity()
        {
            //---------------Set up test pack-------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var vm = CreateViewModel(modelItem);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(vm.ApplyActivity);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ApplyActivity_GivenIsNewWithNoActivity_ShouldHaveNoApplyActivity()
        {
            //---------------Set up test pack-------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var vm = CreateViewModel(modelItem);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNull(vm.ApplyActivity);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ApplyActivity_GivenNumberFormat_ShouldSetModelItemApplyActivity()
        {
            //---------------Set up test pack-------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var vm = CreateViewModel(modelItem);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm);
            //---------------Execute Test ----------------------
            var dsfNumberFormatActivity = new DsfNumberFormatActivity();
            vm.ApplyActivity = dsfNumberFormatActivity;
            //---------------Test Result -----------------------
            var modelProperty = modelItem.Properties["ApplyActivity"];
            if(modelProperty == null || modelProperty.Value == null)
            {
                Assert.Fail("Property Does not exist");
            }
            Assert.AreEqual(dsfNumberFormatActivity, (modelProperty.Value.GetCurrentValue()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DataSource_GivenActivityIsNew_ShouldHaveNoDataSource()
        {
            //---------------Set up test pack-------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var vm = CreateViewModel(modelItem);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(vm.ApplyActivity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(string.IsNullOrEmpty(vm.DataSource));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DataSource_GivenIsSet_ShouldSetModelItemProperty()
        {
            //---------------Set up test pack-------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var vm = CreateViewModel(modelItem);
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(vm.DataSource));
            //---------------Execute Test ----------------------
            const string dataSource = "[[Parents]]";
            vm.DataSource = dataSource;
            //---------------Test Result -----------------------
            var modelProperty = modelItem.Properties["DataSource"];
            if(modelProperty == null || modelProperty.Value == null)
            {
                Assert.Fail("Property Does not exist");
            }
            Assert.AreEqual(dataSource, modelProperty.Value.ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Alias_GivenActivityIsNew_ShouldHaveNoDataSource()
        {
            //---------------Set up test pack-------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var vm = CreateViewModel(modelItem);
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(vm.DataSource));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(string.IsNullOrEmpty(vm.Alias));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Alias_GivenActivityIsSet_ShouldSetModelItemProperty()
        {
            //---------------Set up test pack-------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var vm = CreateViewModel(modelItem);
            //---------------Assert Precondition----------------
            Assert.IsTrue(string.IsNullOrEmpty(vm.Alias));
            //---------------Execute Test ----------------------
            const string alias = "[[Parents]]";
            vm.Alias = alias;
            //---------------Test Result -----------------------
            var modelProperty = modelItem.Properties["Alias"];
            if(modelProperty == null || modelProperty.Value == null)
            {
                Assert.Fail("Property Does not exist");
            }
            Assert.AreEqual(alias, modelProperty.Value.ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetModelItemForServiceTypes_GivenDataObjectNull_ShouldAddeNothingToDataObject()
        {
            //------------Setup for test--------------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var selectAndApplyDesignerViewModel = CreateViewModel(modelItem);
            //------------Execute Test---------------------------
            var modelItemForServiceTypes = selectAndApplyDesignerViewModel.SetModelItemForServiceTypes(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(modelItemForServiceTypes);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyDesignerViewModel_SetModelItemForServiceTypes")]
        public void SelectAndApplyDesignerViewModel_SetModelItemForServiceTypes_Data_NothingAddedToDataObject()
        {
            //------------Setup for test--------------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var selectAndApplyDesignerViewModel = CreateViewModel(modelItem);
            var dataObject = new DataObject("Some Wrong Format", new object());
            //------------Execute Test---------------------------
            selectAndApplyDesignerViewModel.SetModelItemForServiceTypes(dataObject);
            //------------Assert Results-------------------------
            bool dataPresent = dataObject.GetDataPresent(DragDropHelper.ModelItemDataFormat);
            Assert.IsFalse(dataPresent);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyDesignerViewModel_SetModelItemForServiceTypes")]
        public void SelectAndApplyDesignerViewModel_SetModelItemForServiceTypes_DataNotHaveDataContext_NothingAddedToDataObject()
        {
            //------------Setup for test--------------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var selectAndApplyDesignerViewModel = CreateViewModel(modelItem);
            var dataObject = new DataObject(GlobalConstants.ExplorerItemModelFormat, new TestData());
            //------------Execute Test---------------------------
            selectAndApplyDesignerViewModel.SetModelItemForServiceTypes(dataObject);
            //------------Assert Results-------------------------
            bool dataPresent = dataObject.GetDataPresent(DragDropHelper.ModelItemDataFormat);
            Assert.IsFalse(dataPresent);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyDesignerViewModel_SetModelItemForServiceTypes")]
        public void SelectAndApplyDesignerViewModel_SetModelItemForServiceTypes_DataHaveDataContextNotResourceModel_NothingAddedToDataObject()
        {
            //------------Setup for test--------------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var selectAndApplyDesignerViewModel = CreateViewModel(modelItem);
            var dataObject = new DataObject(GlobalConstants.ExplorerItemModelFormat, new TestDataWithContext());
            //------------Execute Test---------------------------
            selectAndApplyDesignerViewModel.SetModelItemForServiceTypes(dataObject);
            //------------Assert Results-------------------------
            bool dataPresent = dataObject.GetDataPresent(DragDropHelper.ModelItemDataFormat);
            Assert.IsFalse(dataPresent);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyDesignerViewModel_SetModelItemForServiceTypes")]
        public void SelectAndApplyDesignerViewModel_SetModelItemForServiceTypes_DataHaveDataContextResourceModel_NothingAddedToDataObject()
        {
            //------------Setup for test--------------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            SetupEnvironmentRepo(Guid.Empty);
            var selectAndApplyDesignerViewModel = CreateViewModel(modelItem);
            var dataObject = new DataObject(GlobalConstants.ExplorerItemModelFormat, new ExplorerItemModel { DisplayName = "MyDBService", ResourceType = ResourceType.DbService, EnvironmentId = Guid.Empty });
            //------------Execute Test---------------------------
            bool isSet = selectAndApplyDesignerViewModel.SetModelItemForServiceTypes(dataObject);
            //------------Assert Results-------------------------
            Assert.IsTrue(isSet);
            Assert.IsNotNull(dsfSelectAndApplyActivity.ApplyActivity);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyDesignerViewModel_DoDrop")]
        public void SelectAndApplyDesignerViewModel_DoDrop_WhenNoFormats_ActivityNotAdded()
        {
            //------------Setup for test--------------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var selectAndApplyDesignerViewModel = CreateViewModel(modelItem);
            var dataObjectMock = new Mock<IDataObject>();
            dataObjectMock.Setup(o => o.GetFormats()).Returns(new string[] { });
            //------------Execute Test---------------------------
            bool doDrop = selectAndApplyDesignerViewModel.DoDrop(dataObjectMock.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(doDrop);
            Assert.IsNotNull(dsfSelectAndApplyActivity.ApplyActivity);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyDesignerViewModel_DoDrop")]
        public void SelectAndApplyDesignerViewModel_DoDrop_WhenFormatDoesNotContainModelItemFormat_ActivityNotAdded()
        {
            //------------Setup for test--------------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var selectAndApplyDesignerViewModel = CreateViewModel(modelItem);
            var dataObjectMock = new Mock<IDataObject>();
            dataObjectMock.Setup(o => o.GetFormats()).Returns(new[] { "Something Else" });
            dataObjectMock.Setup(o => o.GetData("Something Else")).Returns(ModelItemUtils.CreateModelItem(new DsfGatherSystemInformationActivity()));
            //------------Execute Test---------------------------
            bool doDrop = selectAndApplyDesignerViewModel.DoDrop(dataObjectMock.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(doDrop);
            Assert.IsNotNull(dsfSelectAndApplyActivity.ApplyActivity);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyDesignerViewModel_DoDrop")]
        public void SelectAndApplyDesignerViewModel_DoDrop_WhenModelItemsFormatHasMultipleItems_ActivitiesAdded()
        {
            //------------Setup for test--------------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var selectAndApplyDesignerViewModel = CreateViewModel(modelItem);
            var dataObjectMock = new Mock<IDataObject>();
            dataObjectMock.Setup(o => o.GetFormats()).Returns(new[] { "ModelItemsFormat" });
            dataObjectMock.Setup(o => o.GetData("ModelItemsFormat")).Returns(new List<ModelItem> { ModelItemUtils.CreateModelItem(new DsfGatherSystemInformationActivity()), ModelItemUtils.CreateModelItem(new DsfGatherSystemInformationActivity()) });
            //------------Execute Test---------------------------
            bool doDrop = selectAndApplyDesignerViewModel.DoDrop(dataObjectMock.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(doDrop);
            Assert.IsNotNull(dsfSelectAndApplyActivity.ApplyActivity);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SelectAndApplyDesignerViewModel_DoDrop")]
        public void SelectAndApplyDesignerViewModel_DoDrop_WhenModelItemsFormatHasSingleItem_ActivitiesAdded()
        {
            //------------Setup for test--------------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var selectAndApplyDesignerViewModel = CreateViewModel(modelItem);
            var dataObjectMock = new Mock<IDataObject>();
            dataObjectMock.Setup(o => o.GetFormats()).Returns(new[] { "ModelItemsFormat" });
            dataObjectMock.Setup(o => o.GetData("ModelItemsFormat")).Returns(new List<ModelItem> { ModelItemUtils.CreateModelItem(new DsfGatherSystemInformationActivity()) });
            //------------Execute Test---------------------------
            bool doDrop = selectAndApplyDesignerViewModel.DoDrop(dataObjectMock.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(doDrop);
            Assert.IsNotNull(dsfSelectAndApplyActivity.ApplyActivity);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetModelItemForServiceTypes_GivenRaisesRuntimeBinder_ShouldCatchException()
        {
            //---------------Set up test pack-------------------
            var dsfSelectAndApplyActivity = new DsfSelectAndApplyActivity { ApplyActivity = new DsfSelectAndApplyActivity() };
            ModelItem modelItem = CreateModelItem(dsfSelectAndApplyActivity);
            var selectAndApplyDesignerViewModel = new Mock<SelectAndApplyDesignerViewModel>(modelItem);
            selectAndApplyDesignerViewModel.Setup(model => model.DoDrop(It.IsAny<IDataObject>())).Throws(new RuntimeBinderException());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(selectAndApplyDesignerViewModel.Object);
            //---------------Execute Test ----------------------
            try
            {
                selectAndApplyDesignerViewModel.Object.DoDrop(It.IsAny<IDataObject>());
            }
            catch(Exception ex)
            {
                var isRunTimeBinderException = ex is RuntimeBinderException;
                Assert.IsTrue(isRunTimeBinderException);
            }
            //---------------Test Result -----------------------
        }
    }

    public class TestDataWithContext
    {
        public object DataContext { get; set; }
    }

    public class TestData
    {
    }

    public class TestDataWithContexResourceModel
    {
        public ResourceModel DataContext
        {
            get
            {
                var mockEnvironmentModel = new Mock<IEnvironmentModel>();
                mockEnvironmentModel.Setup(model => model.ID).Returns(Guid.NewGuid());
                mockEnvironmentModel.Setup(model => model.Name).Returns("testEnv");
                var resourceModel = new ResourceModel(mockEnvironmentModel.Object)
                {
                    ResourceType = Studio.Core.AppResources.Enums.ResourceType.Service,
                    ServerResourceType = "DbService",
                    ResourceName = "MyDBService",
                    IconPath = "IconPath"
                };
                return resourceModel;
            }
        }
    }
}