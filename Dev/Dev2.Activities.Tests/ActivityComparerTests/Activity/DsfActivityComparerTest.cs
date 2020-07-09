using Dev2.Common.Interfaces.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;

namespace Dev2.Tests.Activities.ActivityComparerTests.Activity
{
    [TestClass]
    public class DsfActivityComparerTest
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentUniqueIds_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfActivity { UniqueID = uniqueId };
            var activity1 = new DsfActivity { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_EmptyActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfActivity { UniqueID = uniqueId };
            var activity1 = new DsfActivity { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentInputs_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var inputs = new List<Common.Interfaces.DB.IServiceInput>();
            inputs.Add(new ServiceInput("Input1", "[[InputValue1]]"));
            var inputs2 = new List<Common.Interfaces.DB.IServiceInput>();
            var activity = new DsfActivity { UniqueID = uniqueId, Inputs = inputs };
            var activity1 = new DsfActivity { UniqueID = uniqueId, Inputs = inputs2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameInputsDifferentIndexes_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var inputs = new List<Common.Interfaces.DB.IServiceInput>
            {
                new ServiceInput("Input2", "[[InputValue2]]"),
                new ServiceInput("Input1", "[[InputValue1]]")
            };
            var inputs2 = new List<Common.Interfaces.DB.IServiceInput>
            {
                new ServiceInput("Input1", "[[InputValue1]]"),
                new ServiceInput("Input2", "[[InputValue2]]")
            };
            var activity = new DsfActivity { UniqueID = uniqueId, Inputs = inputs };
            var activity1 = new DsfActivity { UniqueID = uniqueId, Inputs = inputs2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameInputs_ActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var inputs = new List<Common.Interfaces.DB.IServiceInput>();
            var activity = new DsfActivity { UniqueID = uniqueId, Inputs = inputs };
            var activity1 = new DsfActivity { UniqueID = uniqueId, Inputs = inputs };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentOutputs_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outputs = new List<Common.Interfaces.DB.IServiceOutputMapping>();
            var outputs2 = new List<Common.Interfaces.DB.IServiceOutputMapping>();
            outputs2.Add(new ServiceOutputMapping("a", "b", "c"));
            var activity = new DsfActivity { UniqueID = uniqueId, Outputs = outputs };
            var activity1 = new DsfActivity { UniqueID = uniqueId, Outputs = outputs2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameOutputs_DifferentIndexes_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outputs = new List<Common.Interfaces.DB.IServiceOutputMapping>
            {
                new ServiceOutputMapping("d", "e", "f"),
                new ServiceOutputMapping("a", "b", "c")
            };
            var outputs2 = new List<Common.Interfaces.DB.IServiceOutputMapping>
            {
                new ServiceOutputMapping("a", "b", "c"),
                new ServiceOutputMapping("d", "e", "f")
            };
            var activity = new DsfActivity { UniqueID = uniqueId, Outputs = outputs };
            var activity1 = new DsfActivity { UniqueID = uniqueId, Outputs = outputs2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameOutputs_ActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outputs = new List<Common.Interfaces.DB.IServiceOutputMapping>();
            var activity = new DsfActivity { UniqueID = uniqueId, Outputs = outputs };
            var activity1 = new DsfActivity { UniqueID = uniqueId, Outputs = outputs };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentServiceUri_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfActivity { UniqueID = uniqueId, ServiceUri = "some url" };
            var activity1 = new DsfActivity { UniqueID = uniqueId, ServiceUri = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameServiceUri_ActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfActivity { UniqueID = uniqueId, ServiceUri = "" };
            var activity1 = new DsfActivity { UniqueID = uniqueId, ServiceUri = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentRunWorkflowAsync_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfActivity { UniqueID = uniqueId, RunWorkflowAsync = true };
            var activity1 = new DsfActivity { UniqueID = uniqueId, RunWorkflowAsync = false };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameRunWorkflowAsync_ActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfActivity { UniqueID = uniqueId, RunWorkflowAsync = false };
            var activity1 = new DsfActivity { UniqueID = uniqueId, RunWorkflowAsync = false };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentIsObject_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfActivity { UniqueID = uniqueId, IsObject = true };
            var activity1 = new DsfActivity { UniqueID = uniqueId, IsObject = false };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameIsObject_ActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfActivity { UniqueID = uniqueId, IsObject = false };
            var activity1 = new DsfActivity { UniqueID = uniqueId, IsObject = false };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameCategory_ActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfActivity { UniqueID = uniqueId, Category = "" };
            var activity1 = new DsfActivity { UniqueID = uniqueId, Category = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameCategory_Different_Casing_ActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfActivity { UniqueID = uniqueId, Category = "A" };
            var activity1 = new DsfActivity { UniqueID = uniqueId, Category = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentCategory_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfActivity { UniqueID = uniqueId, Category = "A" };
            var activity1 = new DsfActivity { UniqueID = uniqueId, Category = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Empty_ActivityDto_AreEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO();
            var activity1 = new ActivityDTO();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameFieldName_ActivityDto_AreEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { FieldName = "" };
            var activity1 = new ActivityDTO { FieldName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentFieldName_ActivityDto_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { FieldName = "Field A" };
            var activity1 = new ActivityDTO { FieldName = "Field B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameFieldValue_ActivityDto_AreEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { FieldValue = "" };
            var activity1 = new ActivityDTO { FieldValue = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentFieldValue_ActivityDto_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { FieldValue = "some field value" };
            var activity1 = new ActivityDTO { FieldValue = "some other field value" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameIndexNumber_ActivityDto_AreEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { IndexNumber = 0 };
            var activity1 = new ActivityDTO { IndexNumber = 0 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameIsFieldNameFocused_ActivityDto_AreEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { IsFieldNameFocused = false };
            var activity1 = new ActivityDTO { IsFieldNameFocused = false };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentIsFieldNameFocused_ActivityDto_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { IsFieldNameFocused = false };
            var activity1 = new ActivityDTO { IsFieldNameFocused = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameErrorMessage_ActivityDto_AreEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { ErrorMessage = "" };
            var activity1 = new ActivityDTO { ErrorMessage = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentErrorMessage_ActivityDto_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { ErrorMessage = "some error" };
            var activity1 = new ActivityDTO { ErrorMessage = "some other error" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameWatermarkTextVariable_ActivityDto_AreEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { WatermarkTextVariable = "" };
            var activity1 = new ActivityDTO { WatermarkTextVariable = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentWatermarkTextVariable_ActivityDto_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { WatermarkTextVariable = "Some variable" };
            var activity1 = new ActivityDTO { WatermarkTextVariable = "some other variable" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameWatermarkTextValue_ActivityDto_AreEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { WatermarkTextValue = "" };
            var activity1 = new ActivityDTO { WatermarkTextValue = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentWatermarkTextValue_ActivityDto_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { WatermarkTextValue = "some text value" };
            var activity1 = new ActivityDTO { WatermarkTextValue = "some other text value" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameInserted_ActivityDto_AreEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { Inserted = false };
            var activity1 = new ActivityDTO { Inserted = false };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DisfferentInserted_ActivityDto_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new ActivityDTO { Inserted = false };
            var activity1 = new ActivityDTO { Inserted = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentOutList_ActivityDto_AreNOTEqual()
        {
            //---------------Set up test pack-------------------
            var outlist = new List<string> { "some item" };
            var outlist2 = new List<string> { "some item1" };
            var activity = new ActivityDTO { OutList = outlist };
            var activity1 = new ActivityDTO { OutList = outlist2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameOutList_DifferentIndexes_ActivityDto_AreNOTEqual()
        {
            //---------------Set up test pack-------------------
            var outlist = new List<string> { "some item2", "some item1" };
            var outlist2 = new List<string> { "some item1", "some item2" };
            var activity = new ActivityDTO { OutList = outlist };
            var activity1 = new ActivityDTO { OutList = outlist2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameOutList_ActivityDto_AreEqual()
        {
            //---------------Set up test pack-------------------
            var outlist = new List<string> { "some item" };
            var activity = new ActivityDTO { OutList = outlist };
            var activity1 = new ActivityDTO { OutList = outlist };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        public void SavedSource_Null_Object_Is_NotEqual()
        {
            //---------------Set up test pack-------------------
            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition
            {
                Id = Guid.NewGuid(),
                Path = "A"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(wcfServiceSourceDefinition.Equals(null), "Equals operator can't compare to null.");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SavedSource_Itself_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition
            {
                Id = Guid.NewGuid(),
                Path = "A"
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(wcfServiceSourceDefinition.Equals(wcfServiceSourceDefinition), "Equals operator can't compare to itself.");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SavedSource_DifferentType_Is_NotEqual()
        {
            //---------------Set up test pack-------------------
            var wcfServiceSourceDefinition = new WcfServiceSourceDefinition { EndpointUrl = "bravo" };
            object differentObject = new ActivityDTO
            {
                IndexNumber = 0,
                FieldName = "A"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(wcfServiceSourceDefinition.Equals(differentObject), "Equals operator can't compare to differently typed object.");
        }

        [TestMethod]
        [Timeout(60000)]
        public void EqualsOperator_WithEqualObjects_AreEqual()
        {
            var firstWcfServiceSourceDefinition = new WcfServiceSourceDefinition { EndpointUrl = "bravo" };
            var secondWcfServiceSourceDefinition = new WcfServiceSourceDefinition { EndpointUrl = "bravo" };
            Assert.IsTrue(firstWcfServiceSourceDefinition == secondWcfServiceSourceDefinition, "Equals operator doesnt work.");
        }

        [TestMethod]
        [Timeout(60000)]
        public void NotEqualsOperator_WithNotEqualObjects_AreNotEqual()
        {
            var firstWcfServiceSourceDefinition = new WcfServiceSourceDefinition { EndpointUrl = "bravo" };
            var secondWcfServiceSourceDefinition = new WcfServiceSourceDefinition { EndpointUrl = "charlie" };
            Assert.IsTrue(firstWcfServiceSourceDefinition != secondWcfServiceSourceDefinition, "Not equals operator doesnt work.");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfActivity_GetState")]
        public void DsfActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var act = new DsfActivity();
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, stateItems.Count());

        }
    }
}