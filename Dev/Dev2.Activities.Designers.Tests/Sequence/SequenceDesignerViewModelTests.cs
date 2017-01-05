/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Windows;
using Castle.DynamicProxy.Generators;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Sequence;
using Dev2.Common;
using Dev2.Common.Interfaces.Help;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
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
            AttributesToAvoidReplicating.Add(typeof(UIPermissionAttribute));
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
            sequenceDesignerViewModel.Validate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(sequenceDesignerViewModel);
            Assert.IsInstanceOfType(sequenceDesignerViewModel, typeof(ActivityDesignerViewModel));
            Assert.AreEqual("Created Sequence", sequenceDesignerViewModel.ModelItem.GetProperty("DisplayName"));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SequenceDesignerViewModel_Handle")]
        public void SequenceDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new SequenceDesignerViewModel(CreateModelItem());
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
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
            Assert.AreEqual(0, sequenceDesignerViewModel.TitleBarToggles.Count);
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
        [TestCategory("SequenceDesignerViewModel_SetSmallViewItem")]
        public void SequenceDesignerViewModel_SetSmallViewItem_WhenModelItemDsfPluginActivity_ActivityNotAdded()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var dsfFindRecordsMultipleCriteriaActivity = new DsfFindRecordsMultipleCriteriaActivity();
            dsfSequenceActivity.Activities.Add(dsfFindRecordsMultipleCriteriaActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            ModelItem modelItem = ModelItemUtils.CreateModelItem(new DsfPluginActivity());
            //------------Execute Test---------------------------
            sequenceDesignerViewModel.SmallViewItem = modelItem;
            //------------Assert Results-------------------------
            Assert.AreEqual(3, dsfSequenceActivity.Activities.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_SetSmallViewItem")]
        public void SequenceDesignerViewModel_SetSmallViewItem_WhenModelItemDsfWebserviceActivity_ActivityNotAdded()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var dsfFindRecordsMultipleCriteriaActivity = new DsfFindRecordsMultipleCriteriaActivity();
            dsfSequenceActivity.Activities.Add(dsfFindRecordsMultipleCriteriaActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            ModelItem modelItem = ModelItemUtils.CreateModelItem(new DsfWebserviceActivity());
            //------------Execute Test---------------------------
            sequenceDesignerViewModel.SmallViewItem = modelItem;
            //------------Assert Results-------------------------
            Assert.AreEqual(3, dsfSequenceActivity.Activities.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SequenceDesignerViewModel_SetSmallViewItem")]
        public void SequenceDesignerViewModel_SetSmallViewItem_WhenModelItemDsfDbServiceActivity_ActivityNotAdded()
        {
            //------------Setup for test--------------------------
            var dsfSequenceActivity = new DsfSequenceActivity();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            dsfSequenceActivity.Activities.Add(dsfMultiAssignActivity);
            var dsfFindRecordsMultipleCriteriaActivity = new DsfFindRecordsMultipleCriteriaActivity();
            dsfSequenceActivity.Activities.Add(dsfFindRecordsMultipleCriteriaActivity);
            var sequenceDesignerViewModel = new SequenceDesignerViewModel(CreateModelItem(dsfSequenceActivity));
            ModelItem modelItem = ModelItemUtils.CreateModelItem(new DsfDatabaseActivity());
            //------------Execute Test---------------------------
            sequenceDesignerViewModel.SmallViewItem = modelItem;
            //------------Assert Results-------------------------
            Assert.AreEqual(3, dsfSequenceActivity.Activities.Count);
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
        [TestCategory("SequenceDesignerViewModel_DoDrop")]
        public void SequenceDesignerViewModel_DoDrop_WhenModelItemsFormatHasSingleItem_ActivitiesAdded()
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
            dataObjectMock.Setup(o => o.GetData("ModelItemsFormat")).Returns(new List<ModelItem> { ModelItemUtils.CreateModelItem(new DsfGatherSystemInformationActivity()) });
            //------------Execute Test---------------------------
            bool doDrop = sequenceDesignerViewModel.DoDrop(dataObjectMock.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(doDrop);
            Assert.AreEqual(3, dsfSequenceActivity.Activities.Count);
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

    public class TestDataWithContext
    {
        public object DataContext { get; set; }
    }

    public class TestData
    {
    }
}
