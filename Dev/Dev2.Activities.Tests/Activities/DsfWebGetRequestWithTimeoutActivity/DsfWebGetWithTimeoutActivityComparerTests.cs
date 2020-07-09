/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using Dev2.Activities;
using Dev2.Common.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.DsfWebGetRequestWithTimeoutActivityTests
{
    [TestClass]
    public class DsfWebGetWithTimeoutActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_SameUniqueID_EmptyWebGetRequestTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGetRequestActivity = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId };
            var webGetRequestActivity1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGetRequestActivity);
            //---------------Execute Test ----------------------
            var @equals = webGetRequestActivity.Equals(webGetRequestActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_DifferentWebGetRequestToolIds_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var uniqueId2 = Guid.NewGuid().ToString();
            var webGetRequestActivity = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId };
            var webGetRequestActivity1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGetRequestActivity);
            //---------------Execute Test ----------------------
            var @equals = webGetRequestActivity.Equals(webGetRequestActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Same_DisplayName_Value_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Same_Result_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Result = "A" };
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Different_Result_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Result = "A" };
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Result = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Same_Url_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Url = "" };
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Url = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Same_Url_Different_Casing_IsNOtEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Url = "A" };
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Url = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Different_Url_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Url = "A" };
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Url = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Different_Headers_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Headers = "A" };
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Headers = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Same_Headers_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Headers = "A"};
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Headers = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Same_Headers_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Headers = ""};
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Headers = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Different_Method_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Method = "A" };
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Method = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Same_Method_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Method = "A"};
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Method = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Same_Method_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Method = ""};
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, Method = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Same_TimeOutText_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, TimeOutText = ""};
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, TimeOutText = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Same_TimeOutText_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, TimeOutText = "A"};
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, TimeOutText = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Different_TimeOutText_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, TimeOutText = "A"};
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, TimeOutText = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Different_TimeoutSeconds_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, TimeoutSeconds = 1};
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, TimeoutSeconds = 2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Given_Same_TimeoutSeconds_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, TimeoutSeconds = 0};
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, TimeoutSeconds = 0 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = "http://localhsot", Headers = "Content-Type:json", TimeOutText = "10", Result = "[[res]]" };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(4, stateItems.Count());

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
                    Name = "TimeOutText",
                    Type = StateVariable.StateType.Input,
                    Value ="10"
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