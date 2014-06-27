using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Sequence;
using Dev2.Common;
using Dev2.Core.Tests.Environments;
using Dev2.Models;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
namespace Dev2.Activities.Designers.Tests.Sequence
{
    [TestClass]
    public class SequenceDesignerViewModelTests
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            Castle.DynamicProxy.Generators.AttributesToAvoidReplicating.Add(typeof(System.Security.Permissions.UIPermissionAttribute));
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SequenceDesignerViewModel_Constructor")]
        public void SequenceDesignerViewModel_Constructor_SetSmallViewItem_AlwaysReturnsNull()
        {
            //------------Setup for test--------------------------
            var sequenceActivity = new DsfSequenceActivity();
            var modelItem = ModelItemUtils.CreateModelItem(sequenceActivity);
            //------------Execute Test---------------------------
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(modelItem) { SmallViewItem = "test" };
            //------------Assert Results-------------------------
            Assert.IsNull(sequenceDesignerViewModel.SmallViewItem, "This item should always be null");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_Constructor")]
        public void SequenceDesignerViewModel_Constructor_Constructed_IsInstanceOfActivityDesignerViewModel()
        {
            //------------Setup for test--------------------------
            var sequenceActivity = new DsfSequenceActivity { DisplayName = "Created Sequence" };
            var modelItem = ModelItemUtils.CreateModelItem(sequenceActivity);
            //------------Execute Test---------------------------
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(modelItem);
            //------------Assert Results-------------------------
            Assert.IsNotNull(sequenceDesignerViewModel);
            Assert.IsInstanceOfType(sequenceDesignerViewModel, typeof(ActivityDesignerViewModel));
            Assert.AreEqual("Created Sequence", sequenceDesignerViewModel.ModelItem.GetProperty("DisplayName"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_Constructor")]
        public void SequenceDesignerViewModel_Constructor_Constructed_HasHelpLargeViewToogle()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem();
            //------------Execute Test---------------------------
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(modelItem);
            //------------Assert Results-------------------------
            Assert.IsNotNull(sequenceDesignerViewModel);
            Assert.IsTrue(sequenceDesignerViewModel.HasLargeView);
            Assert.AreEqual(1, sequenceDesignerViewModel.TitleBarToggles.Count);
            StringAssert.Contains(sequenceDesignerViewModel.TitleBarToggles[0].ExpandToolTip, "Help");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_ActivityNames")]
        public void SequenceDesignerViewModel_ActivityNames_WhenNewSequence_HasEmptyList()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            //------------Execute Test---------------------------
            var activityNames = sequenceDesignerViewModel.ActivityNames;
            //------------Assert Results-------------------------
            Assert.AreEqual(0, activityNames.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_ActivityNames")]
        public void SequenceDesignerViewModel_ActivityNames_WhenLessThan4_HasAllNames()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var dsfFindRecordsMultipleCriteriaActivity = new DsfFindRecordsMultipleCriteriaActivity();
            dsfSequenceActivity.Activities.Add(dsfFindRecordsMultipleCriteriaActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            //------------Execute Test---------------------------
            var activityNames = sequenceDesignerViewModel.ActivityNames;
            //------------Assert Results-------------------------
            Assert.AreEqual(2, activityNames.Count);
            Assert.AreEqual(dsfMultiAssignActivity.DisplayName, activityNames[0]);
            Assert.AreEqual(dsfFindRecordsMultipleCriteriaActivity.DisplayName, activityNames[1]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_ActivityNames")]
        public void SequenceDesignerViewModel_ActivityNames_When4_HasAllNames()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var dsfFindRecordsMultipleCriteriaActivity = new DsfFindRecordsMultipleCriteriaActivity();
            dsfSequenceActivity.Activities.Add(dsfFindRecordsMultipleCriteriaActivity);
            var dsfGatherSystemInformationActivity = new DsfGatherSystemInformationActivity();
            dsfSequenceActivity.Activities.Add(dsfGatherSystemInformationActivity);
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            dsfSequenceActivity.Activities.Add(dsfBaseConvertActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            //------------Execute Test---------------------------
            var activityNames = sequenceDesignerViewModel.ActivityNames;
            //------------Assert Results-------------------------
            Assert.AreEqual(4, activityNames.Count);
            Assert.AreEqual(dsfMultiAssignActivity.DisplayName, activityNames[0]);
            Assert.AreEqual(dsfFindRecordsMultipleCriteriaActivity.DisplayName, activityNames[1]);
            Assert.AreEqual(dsfGatherSystemInformationActivity.DisplayName, activityNames[2]);
            Assert.AreEqual(dsfBaseConvertActivity.DisplayName, activityNames[3]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_ActivityNames")]
        public void SequenceDesignerViewModel_ActivityNames_WhenMoreThan4_Has4NamesLastItemEllipsis()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var dsfFindRecordsMultipleCriteriaActivity = new DsfFindRecordsMultipleCriteriaActivity();
            dsfSequenceActivity.Activities.Add(dsfFindRecordsMultipleCriteriaActivity);
            var dsfGatherSystemInformationActivity = new DsfGatherSystemInformationActivity();
            dsfSequenceActivity.Activities.Add(dsfGatherSystemInformationActivity);
            var dsfBaseConvertActivity = new DsfBaseConvertActivity();
            dsfSequenceActivity.Activities.Add(dsfBaseConvertActivity);
            var dsfCaseConvertActivity = new DsfCaseConvertActivity();
            dsfSequenceActivity.Activities.Add(dsfCaseConvertActivity);
            var dsfCalculateActivity = new DsfCalculateActivity();
            dsfSequenceActivity.Activities.Add(dsfCalculateActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            //------------Execute Test---------------------------
            var activityNames = sequenceDesignerViewModel.ActivityNames;
            //------------Assert Results-------------------------
            Assert.AreEqual(5, activityNames.Count);
            Assert.AreEqual(dsfMultiAssignActivity.DisplayName, activityNames[0]);
            Assert.AreEqual(dsfFindRecordsMultipleCriteriaActivity.DisplayName, activityNames[1]);
            Assert.AreEqual(dsfGatherSystemInformationActivity.DisplayName, activityNames[2]);
            Assert.AreEqual(dsfBaseConvertActivity.DisplayName, activityNames[3]);
            Assert.AreEqual("...", activityNames[4]);

            CollectionAssert.DoesNotContain(activityNames, dsfCaseConvertActivity.DisplayName);
            CollectionAssert.DoesNotContain(activityNames, dsfCalculateActivity.DisplayName);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_SetSmallViewItem")]
        public void SequenceDesignerViewModel_SetSmallViewItem_WhenValidModelItem_ActivityAdded()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var dsfFindRecordsMultipleCriteriaActivity = new DsfFindRecordsMultipleCriteriaActivity();
            dsfSequenceActivity.Activities.Add(dsfFindRecordsMultipleCriteriaActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            ModelItem modelItem = ModelItemUtils.CreateModelItem(new DsfGatherSystemInformationActivity());
            //------------Execute Test---------------------------
            sequenceDesignerViewModel.SmallViewItem = modelItem;
            //------------Assert Results-------------------------
            Assert.AreEqual(3, dsfSequenceActivity.Activities.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_SetSmallViewItem")]
        public void SequenceDesignerViewModel_SetSmallViewItem_WhenModelItemSequence_ActivityNotAdded()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var dsfFindRecordsMultipleCriteriaActivity = new DsfFindRecordsMultipleCriteriaActivity();
            dsfSequenceActivity.Activities.Add(dsfFindRecordsMultipleCriteriaActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            ModelItem modelItem = ModelItemUtils.CreateModelItem(new System.Activities.Statements.Sequence());
            //------------Execute Test---------------------------
            sequenceDesignerViewModel.SmallViewItem = modelItem;
            //------------Assert Results-------------------------
            Assert.AreEqual(2, dsfSequenceActivity.Activities.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_SetSmallViewItem")]
        public void SequenceDesignerViewModel_SetSmallViewItem_WhenModelItemDsfActivity_ActivityNotAdded()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var dsfFindRecordsMultipleCriteriaActivity = new DsfFindRecordsMultipleCriteriaActivity();
            dsfSequenceActivity.Activities.Add(dsfFindRecordsMultipleCriteriaActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            ModelItem modelItem = ModelItemUtils.CreateModelItem(new DsfActivity());
            //------------Execute Test---------------------------
            sequenceDesignerViewModel.SmallViewItem = modelItem;
            //------------Assert Results-------------------------
            Assert.AreEqual(2, dsfSequenceActivity.Activities.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_DoDrop")]
        public void SequenceDesignerViewModel_DoDrop_WhenNoFormats_ActivityNotAdded()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var dsfFindRecordsMultipleCriteriaActivity = new DsfFindRecordsMultipleCriteriaActivity();
            dsfSequenceActivity.Activities.Add(dsfFindRecordsMultipleCriteriaActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            var dataObjectMock = new Mock<IDataObject>();
            dataObjectMock.Setup(o => o.GetFormats()).Returns(new string[] { });
            //------------Execute Test---------------------------
            bool doDrop = sequenceDesignerViewModel.DoDrop(dataObjectMock.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(doDrop);
            Assert.AreEqual(2, dsfSequenceActivity.Activities.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_DoDrop")]
        public void SequenceDesignerViewModel_DoDrop_WhenFormatDoesNotContainModelItemFormat_ActivityNotAdded()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var dsfFindRecordsMultipleCriteriaActivity = new DsfFindRecordsMultipleCriteriaActivity();
            dsfSequenceActivity.Activities.Add(dsfFindRecordsMultipleCriteriaActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            var dataObjectMock = new Mock<IDataObject>();
            dataObjectMock.Setup(o => o.GetFormats()).Returns(new[] { "Something Else" });
            dataObjectMock.Setup(o => o.GetData("Something Else")).Returns(ModelItemUtils.CreateModelItem(new DsfGatherSystemInformationActivity()));
            //------------Execute Test---------------------------
            bool doDrop = sequenceDesignerViewModel.DoDrop(dataObjectMock.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(doDrop);
            Assert.AreEqual(2, dsfSequenceActivity.Activities.Count);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SequenceDesignerViewModel_DoDrop")]
        public void SequenceDesignerViewModel_DoDrop_WhenModelItemsFormatHasMultipleItems_ActivitiesAdded()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var dsfFindRecordsMultipleCriteriaActivity = new DsfFindRecordsMultipleCriteriaActivity();
            dsfSequenceActivity.Activities.Add(dsfFindRecordsMultipleCriteriaActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            var dataObjectMock = new Mock<IDataObject>();
            dataObjectMock.Setup(o => o.GetFormats()).Returns(new[] { "ModelItemsFormat" });
            dataObjectMock.Setup(o => o.GetData("ModelItemsFormat")).Returns(new List<ModelItem> { ModelItemUtils.CreateModelItem(new DsfGatherSystemInformationActivity()), ModelItemUtils.CreateModelItem(new DsfGatherSystemInformationActivity()) });
            //------------Execute Test---------------------------
            bool doDrop = sequenceDesignerViewModel.DoDrop(dataObjectMock.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(doDrop);
            Assert.AreEqual(4, dsfSequenceActivity.Activities.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_SetModelItemForServiceTypes")]
        public void SequenceDesignerViewModel_SetModelItemForServiceTypes_DataObjectNull_NothingAddedToDataObject()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            //------------Execute Test---------------------------
            var modelItemForServiceTypes = sequenceDesignerViewModel.SetModelItemForServiceTypes(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(modelItemForServiceTypes);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_SetModelItemForServiceTypes")]
        public void SequenceDesignerViewModel_SetModelItemForServiceTypes_Data_NothingAddedToDataObject()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            var dataObject = new DataObject("Some Wrong Format", new object());
            //------------Execute Test---------------------------
            sequenceDesignerViewModel.SetModelItemForServiceTypes(dataObject);
            //------------Assert Results-------------------------
            bool dataPresent = dataObject.GetDataPresent(DragDropHelper.ModelItemDataFormat);
            Assert.IsFalse(dataPresent);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_SetModelItemForServiceTypes")]
        public void SequenceDesignerViewModel_SetModelItemForServiceTypes_DataNotHaveDataContext_NothingAddedToDataObject()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            var dataObject = new DataObject(GlobalConstants.ExplorerItemModelFormat, new TestData());
            //------------Execute Test---------------------------
            sequenceDesignerViewModel.SetModelItemForServiceTypes(dataObject);
            //------------Assert Results-------------------------
            bool dataPresent = dataObject.GetDataPresent(DragDropHelper.ModelItemDataFormat);
            Assert.IsFalse(dataPresent);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_SetModelItemForServiceTypes")]
        public void SequenceDesignerViewModel_SetModelItemForServiceTypes_DataHaveDataContextNotResourceModel_NothingAddedToDataObject()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            var dataObject = new DataObject(GlobalConstants.ExplorerItemModelFormat, new TestDataWithContext());
            //------------Execute Test---------------------------
            sequenceDesignerViewModel.SetModelItemForServiceTypes(dataObject);
            //------------Assert Results-------------------------
            bool dataPresent = dataObject.GetDataPresent(DragDropHelper.ModelItemDataFormat);
            Assert.IsFalse(dataPresent);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_SetModelItemForServiceTypes")]
        public void SequenceDesignerViewModel_SetModelItemForServiceTypes_DataHaveDataContextResourceModel_NothingAddedToDataObject()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            SetupEnvironmentRepo(Guid.Empty);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            var dataObject = new DataObject(GlobalConstants.ExplorerItemModelFormat, new ExplorerItemModel { DisplayName = "MyDBService", ResourceType = Data.ServiceModel.ResourceType.DbService, EnvironmentId = Guid.Empty });
            //------------Execute Test---------------------------
            bool added = sequenceDesignerViewModel.SetModelItemForServiceTypes(dataObject);
            //------------Assert Results-------------------------
            Assert.IsTrue(added);
            Assert.AreEqual(2, dsfSequenceActivity.Activities.Count);

        }

        static void SetupEnvironmentRepo(Guid environmentId)
        {
            var mockResourceRepository = new Mock<IResourceRepository>();
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(environmentId);
            Mock<IResourceRepository> mockResRepo = new Mock<IResourceRepository>();
            mockResRepo.Setup(d => d.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(new TestDataWithContexResourceModel().DataContext);
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

        static ModelItem CreateModelItem()
        {
            var sequenceActivity = new DsfSequenceActivity { DisplayName = "Created Sequence" };
            var modelItem = CreateModelItem(sequenceActivity);
            return modelItem;
        }

        static ModelItem CreateModelItem(DsfSequenceActivity sequenceActivity)
        {
            var modelItem = ModelItemUtils.CreateModelItem(sequenceActivity);
            return modelItem;
        }
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
                var resourceModel = new ResourceModel(mockEnvironmentModel.Object);
                resourceModel.ResourceType = ResourceType.Service;
                resourceModel.ServerResourceType = "DbService";
                resourceModel.ResourceName = "MyDBService";
                resourceModel.IconPath = "IconPath";
                return resourceModel;
            }
        }
    }

    public class TestDataWithContext
    {
        public object DataContext { get; set; }
    }

    public class TestData
    {
    }
}
