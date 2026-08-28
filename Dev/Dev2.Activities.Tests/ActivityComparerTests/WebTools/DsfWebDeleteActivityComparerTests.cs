using System;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Ouput;
using Dev2.Common.Interfaces.Core.Graph;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common;
using Dev2.Common.X6;
using Newtonsoft.Json.Linq;

namespace Dev2.Tests.Activities.ActivityComparerTests.WebTools
{
    [TestClass]
    public class DsfWebDeleteActivityComparerTests
    {
        /// <summary>
        /// F10: omitting the optional headers key left Headers null; ConfigureHttp then silently
        /// sent no headers instead of hard-failing, but null vs. empty should not be a distinction
        /// callers have to know about.
        /// </summary>
        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DsfWebDeleteActivity))]
        public void DsfWebDeleteActivity_FromX6Json_CellWithoutHeaders_DefaultsToEmptyNotNull()
        {
            //---------------Set up test pack-------------------
            var cell = new Cell
            {
                data = new Dictionary<string, object>
                {
                    { Constants.TYPE, Constants.WEBDELETEACTIVITY.ToLower() },
                    { Constants.WEBMETHOD_QUERYSTRING, "search?q=warewolf" }
                }
            };
            var activity = new DsfWebDeleteActivity();
            //---------------Execute Test ----------------------
            activity.FromX6Json(cell);
            //---------------Test Result -----------------------
            Assert.IsNotNull(activity.Headers);
            Assert.AreEqual(0, activity.Headers.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(DsfWebDeleteActivity))]
        public void DsfWebDeleteActivity_FromX6Json_HeadersSupplied_ArePreserved()
        {
            //---------------Set up test pack-------------------
            // Headers is a JArray on the wire (ToX6Json writes the live IList<INameValue>
            // directly, which only round-trips through an actual JSON pass — as it does in
            // production via X6WorkflowSaveModel serialization — so the fixture is built as the
            // wire shape TryGetHeaders actually reads).
            var cell = new Cell
            {
                data = new Dictionary<string, object>
                {
                    { Constants.TYPE, Constants.WEBDELETEACTIVITY.ToLower() },
                    { Constants.WEBMETHOD_QUERYSTRING, "search?q=warewolf" },
                    { Constants.WEBMETHOD_HEADERS, JArray.FromObject(new List<NameValue> { new NameValue("Accept", "application/json") }) }
                }
            };
            var restored = new DsfWebDeleteActivity();
            //---------------Execute Test ----------------------
            restored.FromX6Json(cell);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, restored.Headers.Count);
            Assert.AreEqual("Accept", restored.Headers[0].Name);
            Assert.AreEqual("application/json", restored.Headers[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameUniqueID_EmptyWebDeleteTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webputActivity = new DsfWebDeleteActivity() { UniqueID = uniqueId };
            var webputActivity1 = new DsfWebDeleteActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webputActivity);
            //---------------Execute Test ----------------------
            var @equals = webputActivity.Equals(webputActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentWebDeleteToolIds_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var uniqueId2 = Guid.NewGuid().ToString();
            var webputActivity = new DsfWebDeleteActivity() { UniqueID = uniqueId };
            var webputActivity1 = new DsfWebDeleteActivity() { UniqueID = uniqueId2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webputActivity);
            //---------------Execute Test ----------------------
            var @equals = webputActivity.Equals(webputActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameWebDeleteTool_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webputActivity = new DsfWebDeleteActivity();
            var webputActivity1 = webputActivity;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webputActivity);
            //---------------Execute Test ----------------------
            var @equals = webputActivity.Equals(webputActivity1);
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
            var webDelete = new DsfWebDeleteActivity() { UniqueID = uniqueId, DisplayName = "" };
            var webDelete1 = new DsfWebDeleteActivity() { UniqueID = uniqueId, DisplayName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDelete);
            //---------------Execute Test ----------------------
            var @equals = webDelete.Equals(webDelete1);
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
            var webDelete = new DsfWebDeleteActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var webDelete1 = new DsfWebDeleteActivity() { UniqueID = uniqueId, DisplayName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDelete);
            //---------------Execute Test ----------------------
            var @equals = webDelete.Equals(webDelete1);
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
            var webDelete = new DsfWebDeleteActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var webDelete1 = new DsfWebDeleteActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDelete);
            //---------------Execute Test ----------------------
            var @equals = webDelete.Equals(webDelete1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_QueryString_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webDelete = new DsfWebDeleteActivity() { UniqueID = uniqueId, QueryString = "A" };
            var webDelete1 = new DsfWebDeleteActivity() { UniqueID = uniqueId, QueryString = "A" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDelete);
            //---------------Execute Test ----------------------
            var @equals = webDelete.Equals(webDelete1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_QueryString_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webDelete = new DsfWebDeleteActivity() { UniqueID = uniqueId, QueryString = "A" };
            var webDelete1 = new DsfWebDeleteActivity() { UniqueID = uniqueId, QueryString = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDelete);
            //---------------Execute Test ----------------------
            var @equals = webDelete.Equals(webDelete1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_OutputDescription_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outDescr = new OutputDescription();
            var webDelete = new DsfWebDeleteActivity() { UniqueID = uniqueId, OutputDescription = outDescr };
            var webDelete1 = new DsfWebDeleteActivity() { UniqueID = uniqueId, OutputDescription = outDescr };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDelete);
            //---------------Execute Test ----------------------
            var @equals = webDelete.Equals(webDelete1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_OutputDescription_IsNOTEqual()
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
            var webDelete = new DsfWebDeleteActivity() { UniqueID = uniqueId, OutputDescription = outDescr };
            var webDelete1 = new DsfWebDeleteActivity() { UniqueID = uniqueId, OutputDescription = outDescr2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDelete);
            //---------------Execute Test ----------------------
            var @equals = webDelete.Equals(webDelete1);
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
            var headers = new List<INameValue>();
            var headers2 = new List<INameValue> { new NameValue("a", "x") };
            var webDelete = new DsfWebDeleteActivity() { UniqueID = uniqueId, Headers = headers };
            var webDelete1 = new DsfWebDeleteActivity() { UniqueID = uniqueId, Headers = headers2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDelete);
            //---------------Execute Test ----------------------
            var @equals = webDelete.Equals(webDelete1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Headers_DifferentIndexes_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue> { new NameValue("b", "y"), new NameValue("a", "x") };
            var headers2 = new List<INameValue> { new NameValue("a", "x"), new NameValue("b", "y") };
            var webDelete = new DsfWebDeleteActivity() { UniqueID = uniqueId, Headers = headers};
            var webDelete1 = new DsfWebDeleteActivity() { UniqueID = uniqueId, Headers = headers };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDelete);
            //---------------Execute Test ----------------------
            var @equals = webDelete.Equals(webDelete1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Headers_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue> { new NameValue("a", "x") };
            var webDelete = new DsfWebDeleteActivity() { UniqueID = uniqueId, Headers = headers};
            var webDelete1 = new DsfWebDeleteActivity() { UniqueID = uniqueId, Headers = headers };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDelete);
            //---------------Execute Test ----------------------
            var @equals = webDelete.Equals(webDelete1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
    }
}