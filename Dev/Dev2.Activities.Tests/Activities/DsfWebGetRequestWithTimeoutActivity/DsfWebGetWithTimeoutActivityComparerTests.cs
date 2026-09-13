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
using Dev2.Common.X6;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.DsfWebGetRequestWithTimeoutActivityTests
{
    [TestClass]
    public class DsfWebGetWithTimeoutActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_X6RoundTrip_PreservesPostData()
        {
            //---------------Set up test pack-------------------
            var original = new DsfWebGetRequestWithTimeoutActivity
            {
                Url = "http://localhost/orders",
                Method = "POST",
                Headers = "Content-Type:application/json",
                PostData = "{\"orderId\":\"A-1001\"}",
                Result = "[[res]]"
            };
            var cell = new Cell();
            //---------------Execute Test ----------------------
            original.ToX6Json(cell);
            var restored = new DsfWebGetRequestWithTimeoutActivity();
            restored.FromX6Json(cell);
            //---------------Test Result -----------------------
            Assert.AreEqual("{\"orderId\":\"A-1001\"}", restored.PostData);
            Assert.AreEqual("POST", restored.Method);
            Assert.AreEqual("http://localhost/orders", restored.Url);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_FromX6Json_CellWithoutPostData_DefaultsToEmpty()
        {
            //---------------Set up test pack-------------------
            // A .bite authored before PostData existed has no webrequest_postdata key;
            // loading one must not produce a null body.
            var cell = new Cell
            {
                data = new System.Collections.Generic.Dictionary<string, object>
                {
                    { Constants.TYPE, Constants.DSFWEBGETREQUESTWITHTIMEOUTACTIVITY.ToLower() },
                    { Constants.WEBREQUEST_METHOD, "GET" },
                    { Constants.WEBREQUEST_URL, "http://localhost" }
                }
            };
            var activity = new DsfWebGetRequestWithTimeoutActivity();
            //---------------Execute Test ----------------------
            activity.FromX6Json(cell);
            //---------------Test Result -----------------------
            Assert.AreEqual(string.Empty, activity.PostData);
        }

        /// <summary>
        /// F11: TimeOutText was previously defaulted to a literal "100" independent of
        /// TimeoutSeconds, so a cell with webrequest_timeoutseconds but no webrequest_timeouttext
        /// (e.g. an older save with only the seconds field) persisted a contradiction — and
        /// execution re-derives its effective timeout from TimeOutText, not TimeoutSeconds, so the
        /// drift was live, not cosmetic.
        /// </summary>
        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_FromX6Json_TimeoutSecondsWithoutTimeoutText_DerivesText()
        {
            //---------------Set up test pack-------------------
            var cell = new Cell
            {
                data = new System.Collections.Generic.Dictionary<string, object>
                {
                    { Constants.TYPE, Constants.DSFWEBGETREQUESTWITHTIMEOUTACTIVITY.ToLower() },
                    { Constants.WEBREQUEST_METHOD, "GET" },
                    { Constants.WEBREQUEST_URL, "http://localhost" },
                    { Constants.WEBREQUEST_TIMEOUTSECONDS, 300 }
                }
            };
            var activity = new DsfWebGetRequestWithTimeoutActivity();
            //---------------Execute Test ----------------------
            activity.FromX6Json(cell);
            //---------------Test Result -----------------------
            Assert.AreEqual(300, activity.TimeoutSeconds);
            Assert.AreEqual("300", activity.TimeOutText);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_FromX6Json_BothTimeoutFieldsSupplied_PreservesExplicitText()
        {
            //---------------Set up test pack-------------------
            var cell = new Cell
            {
                data = new System.Collections.Generic.Dictionary<string, object>
                {
                    { Constants.TYPE, Constants.DSFWEBGETREQUESTWITHTIMEOUTACTIVITY.ToLower() },
                    { Constants.WEBREQUEST_METHOD, "GET" },
                    { Constants.WEBREQUEST_URL, "http://localhost" },
                    { Constants.WEBREQUEST_TIMEOUTSECONDS, 300 },
                    { Constants.WEBREQUEST_TIMEOUTTEXT, "five minutes" }
                }
            };
            var activity = new DsfWebGetRequestWithTimeoutActivity();
            //---------------Execute Test ----------------------
            activity.FromX6Json(cell);
            //---------------Test Result -----------------------
            Assert.AreEqual(300, activity.TimeoutSeconds);
            Assert.AreEqual("five minutes", activity.TimeOutText);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_FromX6Json_NeitherTimeoutFieldSupplied_DefaultsToOneHundred()
        {
            //---------------Set up test pack-------------------
            var cell = new Cell
            {
                data = new System.Collections.Generic.Dictionary<string, object>
                {
                    { Constants.TYPE, Constants.DSFWEBGETREQUESTWITHTIMEOUTACTIVITY.ToLower() },
                    { Constants.WEBREQUEST_METHOD, "GET" },
                    { Constants.WEBREQUEST_URL, "http://localhost" }
                }
            };
            var activity = new DsfWebGetRequestWithTimeoutActivity();
            //---------------Execute Test ----------------------
            activity.FromX6Json(cell);
            //---------------Test Result -----------------------
            Assert.AreEqual(100, activity.TimeoutSeconds);
            Assert.AreEqual("100", activity.TimeOutText);
        }

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
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = "http://localhsot", Headers = "Content-Type:json", PostData = "{\"a\":1}", TimeOutText = "10", Result = "[[res]]" };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(5, stateItems.Count());

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
                    Name = "PostData",
                    Type = StateVariable.StateType.Input,
                    Value ="{\"a\":1}"
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