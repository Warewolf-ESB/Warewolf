using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class MergeWorkflowViewModelTests
    {
        [TestMethod]
        public void Initialize_GivenIsNewNoEmptyConflicts_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var currentChanges = new List<ModelItem>();
            var differenceChanges = new List<ModelItem>();
            var appAdaptor = new Mock<IApplicationAdaptor>();

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentChanges, differenceChanges, appAdaptor.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(mergeWorkflowViewModel.CurrentConflictViewModel);
            Assert.IsNotNull(mergeWorkflowViewModel.DifferenceConflictViewModel);
        }

        [TestMethod]
        public void Initialize_GivenHasConflicts_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var dsfMultiAssignActivity = new DsfMultiAssignActivity();
            var dsfMultiAssignActivity1 = new DsfMultiAssignActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dsfMultiAssignActivity);
            var modelItem1 = ModelItemUtils.CreateModelItem(dsfMultiAssignActivity1);
            var appAdaptor = new Mock<IApplicationAdaptor>();
            var currentChanges = new List<ModelItem>()
            {
                modelItem
            };
            var differenceChanges = new List<ModelItem>()
            {
                modelItem1
            };

            appAdaptor.Setup(adaptor => adaptor.TryFindResource(It.IsAny<object>())).Returns(new object());
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentChanges, differenceChanges, appAdaptor.Object);
            //---------------Assert Precondition----------------
            Assert.AreNotSame(dsfMultiAssignActivity, dsfMultiAssignActivity1);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(mergeWorkflowViewModel.CurrentConflictViewModel);
            Assert.IsNotNull(mergeWorkflowViewModel.DifferenceConflictViewModel);
            //---------------Test Result -----------------------
            //var mergeToolModels = mergeWorkflowViewModel.CurrentConflictViewModel.MergeConflicts;
            //var differenceViewModel = mergeWorkflowViewModel.DifferenceConflictViewModel.MergeConflicts;
            //Assert.AreEqual(1, mergeToolModels.Count);
            //Assert.AreEqual(1, differenceViewModel.Count);
            appAdaptor.Verify(adaptor => adaptor.TryFindResource("Data-Assign"), Times.Exactly(2));
        }


        [TestMethod]
        public void Initialize_GivenHasConflictsAndForEach_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var assignId = Guid.NewGuid();
            var foreachId = Guid.NewGuid();
            var dsfMultiAssignActivity = new DsfMultiAssignActivity()
            {
                UniqueID = assignId.ToString(),
                FieldsCollection = new List<ActivityDTO>()
                {
                    new ActivityDTO("a","a",1),
                    new ActivityDTO("a","a",2)
                }
            };
            var dsfMultiAssignActivity1 = new DsfMultiAssignActivity()
            {
                UniqueID = assignId.ToString(),
                FieldsCollection = new List<ActivityDTO>()
                {
                    new ActivityDTO("a","b",1),
                    new ActivityDTO("a","a",2)
                }
            };
            var dsfForEachActivity = new DsfForEachActivity()
            {
                UniqueID = foreachId.ToString(),
                DataFunc = new ActivityFunc<string, bool>()
                {
                    Handler = new DsfDateTimeActivity()
                }
            };
            var dsfForEachActivity1 = new DsfForEachActivity()
            {
                UniqueID = foreachId.ToString(),
                DataFunc = new ActivityFunc<string, bool>()
                {
                    Handler = new DsfDateTimeActivity()
                }
            };
            var assignOne = ModelItemUtils.CreateModelItem(dsfMultiAssignActivity);
            var assign2 = ModelItemUtils.CreateModelItem(dsfMultiAssignActivity1);
            var forEach = ModelItemUtils.CreateModelItem(dsfForEachActivity);
            var forEach1 = ModelItemUtils.CreateModelItem(dsfForEachActivity1);
            var appAdaptor = new Mock<IApplicationAdaptor>();
            var currentChanges = new List<ModelItem>()
            {
                assignOne,forEach
            };
            var differenceChanges = new List<ModelItem>()
            {
                assign2,forEach1
            };

            appAdaptor.Setup(adaptor => adaptor.TryFindResource(It.IsAny<object>())).Returns(new object());
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentChanges, differenceChanges, appAdaptor.Object);
            //---------------Assert Precondition----------------
            Assert.AreNotSame(dsfMultiAssignActivity, dsfMultiAssignActivity1);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(mergeWorkflowViewModel.CurrentConflictViewModel);
            Assert.IsNotNull(mergeWorkflowViewModel.DifferenceConflictViewModel);
            //---------------Test Result -----------------------
            //var mergeToolModels = mergeWorkflowViewModel.CurrentConflictViewModel.MergeConflicts;
            //var differenceViewModel = mergeWorkflowViewModel.DifferenceConflictViewModel.MergeConflicts;
            //Assert.AreEqual(1, mergeToolModels.Count);
            //Assert.AreEqual(1, differenceViewModel.Count);
            appAdaptor.Verify(adaptor => adaptor.TryFindResource("Data-Assign"), Times.Exactly(2));
        }
    }
}
