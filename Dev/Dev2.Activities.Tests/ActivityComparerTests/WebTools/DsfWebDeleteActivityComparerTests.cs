using System;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Ouput;
using Dev2.Common.Interfaces.Core.Graph;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common;

namespace Dev2.Tests.Activities.ActivityComparerTests.WebTools
{
    [TestClass]
    public class DsfWebDeleteActivityComparerTests
    {
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