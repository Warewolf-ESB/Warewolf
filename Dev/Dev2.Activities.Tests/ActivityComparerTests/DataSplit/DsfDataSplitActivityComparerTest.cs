using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.DataMerge
{
    [TestClass]
    public class DsfDataSplitActivityComparerTest
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentUniqueIds_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId };
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId };
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
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId };
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_SourceString_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId, SourceString = "a" };
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId, SourceString = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_SourceString_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId, SourceString = "A" };
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId, SourceString = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_DisplayName_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId, DisplayName = "a" };
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
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_SkipBlankRows_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId, SkipBlankRows = true};
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId, SkipBlankRows = true};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_SkipBlankRows_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId, SkipBlankRows = true };
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId, SkipBlankRows = false };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_ReverseOrder_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId, ReverseOrder = true};
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId, ReverseOrder = true};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_ReverseOrder_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId, ReverseOrder = true };
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId, ReverseOrder = false };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_ResultsCollection_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var splitColl = new List<DataSplitDTO>();
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId, ResultsCollection = splitColl };
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId, ResultsCollection = splitColl };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_ResultsCollection_Different_Indexes_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var splitColl = new List<DataSplitDTO>
            {
                 new DataSplitDTO
                {
                    OutputVariable = "[[VariableA]]"
                    ,
                    At = "'"

                },
                new DataSplitDTO
                {
                    OutputVariable = "[[VariableB]]"
                    ,
                    At = "@"
                }

            };
            var splitColl2 = new List<DataSplitDTO>()
            {
                 new DataSplitDTO
                {
                    OutputVariable = "[[VariableB]]"
                    ,
                    At = "@"

                }
                 ,
                new DataSplitDTO
                {
                    OutputVariable = "[[VariableA]]"
                    ,
                    At = "'"                   
                }
            };
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId, ResultsCollection = splitColl };
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId, ResultsCollection = splitColl2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_ResultsCollection_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var splitColl = new List<DataSplitDTO>
            {
                 new DataSplitDTO
                {
                    OutputVariable = "[[VariableA]]"
                    ,
                    At = "'"
                }
            };
            var splitColl2 = new List<DataSplitDTO>()
            {
                new DataSplitDTO
                {
                    OutputVariable = "[[VariableA]]"
                    ,
                    At = "'"
                }
            };
            var activity1 = new DsfDataSplitActivity() { UniqueID = uniqueId, ResultsCollection = splitColl };
            var activity = new DsfDataSplitActivity() { UniqueID = uniqueId, ResultsCollection = splitColl2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}