using System;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Ouput;
using Dev2.Common.Interfaces.Core.Graph;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using System.Linq;
using Dev2.Common.State;

namespace Dev2.Tests.Activities.ActivityComparerTests.WebTools
{
    [TestClass]
    public class DsfWebGetRequestActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameUniqueID_EmptyWebGetRequestTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGetRequestActivity = new DsfWebGetRequestActivity() { UniqueID = uniqueId };
            var webGetRequestActivity1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGetRequestActivity);
            //---------------Execute Test ----------------------
            var @equals = webGetRequestActivity.Equals(webGetRequestActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentWebGetRequestToolIds_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var uniqueId2 = Guid.NewGuid().ToString();
            var webGetRequestActivity = new DsfWebGetRequestActivity() { UniqueID = uniqueId };
            var webGetRequestActivity1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGetRequestActivity);
            //---------------Execute Test ----------------------
            var @equals = webGetRequestActivity.Equals(webGetRequestActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameWebGetRequestTool_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGetRequestActivity = new DsfWebGetRequestActivity();
            var webGetRequestActivity1 = webGetRequestActivity;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGetRequestActivity);
            //---------------Execute Test ----------------------
            var @equals = webGetRequestActivity.Equals(webGetRequestActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_DisplayName_Value_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, DisplayName = "" };
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, DisplayName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_DisplayName_Value_IsNOT_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, DisplayName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_DisplayName_Value_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Result_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Result = "A" };
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_Result_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Result = "A" };
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Result = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Url_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Url = "" };
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Url = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Url_Different_Casing_IsNOtEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Url = "A" };
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Url = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_Url_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Url = "A" };
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Url = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_Headers_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Headers = "A" };
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Headers = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Headers_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Headers = "A"};
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Headers = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Headers_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Headers = ""};
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Headers = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_Method_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Method = "A" };
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Method = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Method_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Method = "A"};
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Method = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Method_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Method = ""};
            var webGet1 = new DsfWebGetRequestActivity() { UniqueID = uniqueId, Method = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebGetRequestActivity_GetState")]
        public void DsfWebGetRequestActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var act = new DsfWebGetRequestActivity { Url = "http://localhsot", Headers="Content-Type:json", Result="[[res]]" };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(3, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "Url",
                    Type = StateVariable.StateType.Input,
                    Value ="http://localhsot"
                },
                new StateVariable
                {
                    Name = "Headers",
                    Type = StateVariable.StateType.Input,
                    Value ="Content-Type:json"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "[[res]]"
                }
            };

            var iter = act.GetState().Select(
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