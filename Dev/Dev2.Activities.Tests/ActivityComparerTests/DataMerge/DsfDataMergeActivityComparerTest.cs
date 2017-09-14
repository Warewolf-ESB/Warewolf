using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.DataMerge
{
    [TestClass]
    public class DsfDataMergeActivityComparerTest
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentUniqueIds_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfDataMergeActivity() { UniqueID = uniqueId };
            var activity1 = new DsfDataMergeActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_EmptyActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfDataMergeActivity() { UniqueID = uniqueId };
            var activity1 = new DsfDataMergeActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_DisplayName_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDataMergeActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity = new DsfDataMergeActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_DisplayName_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDataMergeActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity = new DsfDataMergeActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_Result_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDataMergeActivity() { UniqueID = uniqueId, Result = "a" };
            var activity = new DsfDataMergeActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_Result_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDataMergeActivity() { UniqueID = uniqueId, Result = "A" };
            var activity = new DsfDataMergeActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_MergeCollection_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var mergeColl = new List<DataMergeDTO>();
            var activity1 = new DsfDataMergeActivity() { UniqueID = uniqueId, MergeCollection = mergeColl };
            var activity = new DsfDataMergeActivity() { UniqueID = uniqueId, MergeCollection = mergeColl };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_MergeCollection_Different_Indexes_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var mergeColl = new List<DataMergeDTO>
            {
                 new DataMergeDTO
                {
                    InputVariable = "[[VariableA]]"
                    ,
                    At = "'"
                    ,
                    MergeType = "CHAR"
                    ,
                    Alignment = "RIGHT"

                },
                new DataMergeDTO
                {
                    InputVariable = "[[VariableB]]"
                    ,
                    At = "@"
                    ,
                    MergeType = "CHAR"
                    ,
                    Alignment = "LEFT"

                }

            };
            var mergeColl2 = new List<DataMergeDTO>()
            {
                 new DataMergeDTO
                {
                    InputVariable = "[[VariableB]]"
                    ,
                    At = "@"
                    ,
                    MergeType = "CHAR"
                    ,
                    Alignment = "LEFT"

                }
                 ,
                new DataMergeDTO
                {
                    InputVariable = "[[VariableA]]"
                    ,
                    At = "'"
                    ,
                    MergeType = "CHAR"
                    ,
                    Alignment = "LEFT"
                    
                }
            };
            var activity1 = new DsfDataMergeActivity() { UniqueID = uniqueId, MergeCollection = mergeColl };
            var activity = new DsfDataMergeActivity() { UniqueID = uniqueId, MergeCollection = mergeColl2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_MergeCollection_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var mergeColl = new List<DataMergeDTO>
            {
                 new DataMergeDTO
                {
                    InputVariable = "[[VariableA]]"
                    ,
                    At = "'"
                    ,
                    MergeType = "CHAR"
                    ,
                    Alignment = "RIGHT"

                }
            };
            var mergeColl2 = new List<DataMergeDTO>()
            {
                new DataMergeDTO
                {
                    InputVariable = "[[VariableA]]"
                    ,
                    At = "'"
                    ,
                    MergeType = "CHAR"
                    ,
                    Alignment = "LEFT"
                    
                }
            };
            var activity1 = new DsfDataMergeActivity() { UniqueID = uniqueId, MergeCollection = mergeColl };
            var activity = new DsfDataMergeActivity() { UniqueID = uniqueId, MergeCollection = mergeColl2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}