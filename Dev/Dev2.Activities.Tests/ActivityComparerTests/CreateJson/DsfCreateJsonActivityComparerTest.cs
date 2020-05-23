using Dev2.Common.State;
using Dev2.Communication;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.CreateJson
{
    [TestClass]
    public class DsfCreateJsonActivityComparerTest
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentUniqueIds_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfCreateJsonActivity() { UniqueID = uniqueId };
            var activity1 = new DsfCreateJsonActivity() { UniqueID = uniqueId };
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
            var activity = new DsfCreateJsonActivity() { UniqueID = uniqueId };
            var activity1 = new DsfCreateJsonActivity() { UniqueID = uniqueId };
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
            var activity1 = new DsfCreateJsonActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity = new DsfCreateJsonActivity() { UniqueID = uniqueId, DisplayName = "a" };
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
            var activity1 = new DsfCreateJsonActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity = new DsfCreateJsonActivity() { UniqueID = uniqueId, DisplayName = "ass" };
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
        public void Equals_Given_Same_JsonString_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfCreateJsonActivity() { UniqueID = uniqueId, JsonString = "a" };
            var activity = new DsfCreateJsonActivity() { UniqueID = uniqueId, JsonString = "a" };
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
        public void Equals_Given_Different_JsonString_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfCreateJsonActivity() { UniqueID = uniqueId, JsonString = "A" };
            var activity = new DsfCreateJsonActivity() { UniqueID = uniqueId, JsonString = "ass" };
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
        public void Equals_Given_Same_JsonMappings_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var mappings = new List<JsonMappingTo>();
            var activity1 = new DsfCreateJsonActivity() { UniqueID = uniqueId, JsonMappings = mappings };
            var activity = new DsfCreateJsonActivity() { UniqueID = uniqueId, JsonMappings = mappings };
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
        public void Equals_Given_Same_JsonMappings_DifferentIndexes_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var mappings = new List<JsonMappingTo>
            {
                new JsonMappingTo
                {
                    DestinationName = "Some Name"
                },
                new JsonMappingTo
                {
                    DestinationName = "Some Other Name"
                }
            };
            var mappings2 = new List<JsonMappingTo>
            {
                new JsonMappingTo
                {
                    DestinationName = "Some Other Name"
                },
                new JsonMappingTo
                {
                    DestinationName = "Some Name"
                }
            };
            var activity1 = new DsfCreateJsonActivity() { UniqueID = uniqueId, JsonMappings = mappings };
            var activity = new DsfCreateJsonActivity() { UniqueID = uniqueId, JsonMappings= mappings2 };
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
        public void Equals_Given_Different_JsonMappings_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var mappings = new List<JsonMappingTo>
            {
                new JsonMappingTo
                {
                    DestinationName = "Some Name"
                }
            };
            var mappings2 = new List<JsonMappingTo>
            {
                new JsonMappingTo
                {
                    DestinationName = "Some Other Name"
                }
            };
            var activity1 = new DsfCreateJsonActivity() { UniqueID = uniqueId, JsonMappings = mappings };
            var activity = new DsfCreateJsonActivity() { UniqueID = uniqueId, JsonMappings= mappings2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfCreateJsonActivity_GetState")]
        public void DsfCreateJsonActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var mappings = new List<JsonMappingTo>
            {
                new JsonMappingTo
                {
                    DestinationName = "Some Name"
                }
            };

            //------------Setup for test--------------------------
            var activity = new DsfCreateJsonActivity() { UniqueID = uniqueId, JsonMappings = mappings, JsonString = "{ Json String }" };
            //------------Execute Test---------------------------
            var stateItems = activity.GetState();
            Assert.AreEqual(2, stateItems.Count());

            var serializer = new Dev2JsonSerializer();
            var mappingItems = serializer.Serialize(activity.JsonMappings);

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name="JsonMappings",
                    Type = StateVariable.StateType.Input,
                    Value = mappingItems
                },
                new StateVariable
                {
                    Name="JsonString",
                    Type = StateVariable.StateType.Output,
                    Value = "{ Json String }"
                }
            };

            var iter = activity.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }
    }
}