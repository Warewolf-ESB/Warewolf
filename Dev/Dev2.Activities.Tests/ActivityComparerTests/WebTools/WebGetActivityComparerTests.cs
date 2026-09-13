/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Ouput;
using Dev2.Common.Interfaces.Core.Graph;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.X6;
using Newtonsoft.Json.Linq;

namespace Dev2.Tests.Activities.ActivityComparerTests.WebTools
{
    [TestClass]
    public class WebGetActivityComparerTests
    {
        /// <summary>
        /// F10: omitting the optional headers key left Headers null, and WebGetActivity's own
        /// ExecutionImpl hard-fails execution with HeadersAreNull when that happens — the schema
        /// documents headers as optional, so a caller-omitted field should not require a null
        /// check the activity itself doesn't need.
        /// </summary>
        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebGetActivity))]
        public void WebGetActivity_FromX6Json_CellWithoutHeaders_DefaultsToEmptyNotNull()
        {
            //---------------Set up test pack-------------------
            var cell = new Cell
            {
                data = new Dictionary<string, object>
                {
                    { Constants.TYPE, Constants.WEBGETACTIVITY.ToLower() },
                    { Constants.WEBMETHOD_QUERYSTRING, "search?q=warewolf" }
                }
            };
            var activity = new WebGetActivity();
            //---------------Execute Test ----------------------
            activity.FromX6Json(cell);
            //---------------Test Result -----------------------
            Assert.IsNotNull(activity.Headers);
            Assert.AreEqual(0, activity.Headers.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebGetActivity))]
        public void WebGetActivity_FromX6Json_HeadersSupplied_ArePreserved()
        {
            //---------------Set up test pack-------------------
            // Headers is a JArray on the wire (ToX6Json writes the live IList<INameValue>
            // directly, which only round-trips through an actual JSON pass — as it does in
            // production via X6WorkflowSaveModel serialization — so the fixture is built as the
            // wire shape TryGetHeaders actually reads, matching WEBMETHOD_HEADERS' real content).
            var cell = new Cell
            {
                data = new Dictionary<string, object>
                {
                    { Constants.TYPE, Constants.WEBGETACTIVITY.ToLower() },
                    { Constants.WEBMETHOD_QUERYSTRING, "search?q=warewolf" },
                    { Constants.WEBMETHOD_HEADERS, JArray.FromObject(new List<NameValue> { new NameValue("Accept", "application/json") }) }
                }
            };
            var restored = new WebGetActivity();
            //---------------Execute Test ----------------------
            restored.FromX6Json(cell);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, restored.Headers.Count);
            Assert.AreEqual("Accept", restored.Headers[0].Name);
            Assert.AreEqual("application/json", restored.Headers[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        public void WebGetActivity_Equals_Given_SameUniqueID_EmptyWebGetTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGetActivity = new WebGetActivity() { UniqueID = uniqueId };
            var webGetActivity1 = new WebGetActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGetActivity);
            //---------------Execute Test ----------------------
            var @equals = webGetActivity.Equals(webGetActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        public void WebGetActivity_Equals_Given_DifferentWebGetToolIds_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var uniqueId2 = Guid.NewGuid().ToString();
            var webGetActivity = new WebGetActivity() { UniqueID = uniqueId };
            var webGetActivity1 = new WebGetActivity() { UniqueID = uniqueId2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGetActivity);
            //---------------Execute Test ----------------------
            var @equals = webGetActivity.Equals(webGetActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        public void WebGetActivity_Equals_Given_SameWebGetTool_IsEqual()
        {
            //---------------Set up test pack-------------------
            var webGetActivity = new WebGetActivity();
            var webGetActivity1 = webGetActivity;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGetActivity);
            //---------------Execute Test ----------------------
            var @equals = webGetActivity.Equals(webGetActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        public void WebGetActivity_Equals_Given_Same_DisplayName_Value_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new WebGetActivity() { UniqueID = uniqueId, DisplayName = "" };
            var webGet1 = new WebGetActivity() { UniqueID = uniqueId, DisplayName = "" };
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
        public void WebGetActivity_Equals_Given_Different_DisplayName_Value_IsNOT_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new WebGetActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var webGet1 = new WebGetActivity() { UniqueID = uniqueId, DisplayName = "" };
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
        public void WebGetActivity_Equals_Given_Same_DisplayName_Value_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new WebGetActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var webGet1 = new WebGetActivity() { UniqueID = uniqueId, DisplayName = "a" };
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
        public void WebGetActivity_Equals_Given_Same_QueryString_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new WebGetActivity() { UniqueID = uniqueId, QueryString = "A" };
            var webGet1 = new WebGetActivity() { UniqueID = uniqueId, QueryString = "A" };
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
        public void WebGetActivity_Equals_Given_Different_QueryString_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new WebGetActivity() { UniqueID = uniqueId, QueryString = "A" };
            var webGet1 = new WebGetActivity() { UniqueID = uniqueId, QueryString = "B" };
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
        public void WebGetActivity_Equals_Given_Same_OutputDescription_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outDescr = new OutputDescription();
            var webGet = new WebGetActivity() { UniqueID = uniqueId, OutputDescription = outDescr };
            var webGet1 = new WebGetActivity() { UniqueID = uniqueId, OutputDescription = outDescr };
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
        public void WebGetActivity_Equals_Given_Different_OutputDescription_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outDescr = new OutputDescription()
            {
                Format = OutputFormats.Unknown
            };
            var outDescr2 = new OutputDescription()
            {
                Format = OutputFormats.ShapedXML
            };
            var webGet = new WebGetActivity() { UniqueID = uniqueId, OutputDescription = outDescr };
            var webGet1 = new WebGetActivity() { UniqueID = uniqueId, OutputDescription = outDescr2 };
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
        public void WebGetActivity_Equals_Given_Different_Headers_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue>();
            var headers2 = new List<INameValue> { new NameValue("a", "x") };
            var webGet = new WebGetActivity() { UniqueID = uniqueId, Headers = headers };
            var webGet1 = new WebGetActivity() { UniqueID = uniqueId, Headers = headers2 };
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
        public void WebGetActivity_Equals_Given_Same_Headers_DifferentIndexes_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue> { new NameValue("b", "y"), new NameValue("a", "x") };
            var webGet = new WebGetActivity() { UniqueID = uniqueId, Headers = headers };
            var webGet1 = new WebGetActivity() { UniqueID = uniqueId, Headers = headers };
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
        public void WebGetActivity_Equals_Given_Same_Headers_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue> { new NameValue("a", "x") };
            var webGet = new WebGetActivity() { UniqueID = uniqueId, Headers = headers };
            var webGet1 = new WebGetActivity() { UniqueID = uniqueId, Headers = headers };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
    }
}