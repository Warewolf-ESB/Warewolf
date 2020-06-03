using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Dev2.Tests.Activities.ActivityComparerTests.SysInfo
{
    [TestClass]
    public class DsfGatherSystemInformationActivityComparerTest
    {
#pragma warning disable S3776,S1541,S134,CC0075,S1066,S1067
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentUniqueIds_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId };
            var activity1 = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId };
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
            var activity = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId };
            var activity1 = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId };
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
        public void Equals_Given_Same_DisplayName_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_DisplayName_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Text_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var systemInformationCollection = new List<GatherSystemInformationTO>();
            var activity1 = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId, SystemInformationCollection = systemInformationCollection };
            var activity = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId, SystemInformationCollection = systemInformationCollection };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_SystemInformationCollection_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var systemInformationCollection = new List<GatherSystemInformationTO>
            {
                new GatherSystemInformationTO
                {
                    EnTypeOfSystemInformation = Data.Interfaces.Enums.enTypeOfSystemInformationToGather.UserName
                }
            };

            var systemInformationCollection2 = new List<GatherSystemInformationTO>
            {
                new GatherSystemInformationTO
                {
                    EnTypeOfSystemInformation = Data.Interfaces.Enums.enTypeOfSystemInformationToGather.ComputerName
                }
            };
            var activity1 = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId, SystemInformationCollection = systemInformationCollection };
            var activity = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId, SystemInformationCollection = systemInformationCollection2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_SystemInformationCollection_Different_Indexes_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var systemInformationCollection = new List<GatherSystemInformationTO>
            {
                new GatherSystemInformationTO
                {
                    EnTypeOfSystemInformation = Data.Interfaces.Enums.enTypeOfSystemInformationToGather.UserName
                }
                ,
                new GatherSystemInformationTO
                {
                    EnTypeOfSystemInformation = Data.Interfaces.Enums.enTypeOfSystemInformationToGather.ComputerName
                }
            };

            var systemInformationCollection2 = new List<GatherSystemInformationTO>
            {
                new GatherSystemInformationTO
                {
                    EnTypeOfSystemInformation = Data.Interfaces.Enums.enTypeOfSystemInformationToGather.ComputerName
                }
                ,
                new GatherSystemInformationTO
                {
                    EnTypeOfSystemInformation = Data.Interfaces.Enums.enTypeOfSystemInformationToGather.UserName
                }
            };
            var activity1 = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId, SystemInformationCollection = systemInformationCollection };
            var activity = new DsfGatherSystemInformationActivity() { UniqueID = uniqueId, SystemInformationCollection = systemInformationCollection2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
#pragma warning restore S3776, S1541, S134, CC0075, S1066, S1067
    }
}
