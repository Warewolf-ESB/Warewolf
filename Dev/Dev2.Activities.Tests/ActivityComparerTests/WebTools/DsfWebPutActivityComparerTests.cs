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
    public class DsfWebPutActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameUniqueID_EmptyWebPutTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webputActivity = new DsfWebPutActivity() { UniqueID = uniqueId };
            var webputActivity1 = new DsfWebPutActivity() { UniqueID = uniqueId };
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
        public void Equals_Given_DifferentWebPutToolIds_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var uniqueId2 = Guid.NewGuid().ToString();
            var webputActivity = new DsfWebPutActivity() { UniqueID = uniqueId };
            var webputActivity1 = new DsfWebPutActivity() { UniqueID = uniqueId2 };
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
        public void Equals_Given_SameWebPutTool_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webputActivity = new DsfWebPutActivity();
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
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, DisplayName = "" };
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, DisplayName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
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
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, DisplayName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
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
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_PutData_Value_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, PutData = "A" };
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, PutData = "A" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_PutData_Different_Casing_Value_IsNOtEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, PutData = "A" };
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, PutData = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_PutData_Value_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, PutData = "A" };
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, PutData = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
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
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, QueryString = "A" };
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, QueryString = "A" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
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
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, QueryString = "A" };
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, QueryString = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
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
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, OutputDescription = outDescr };
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, OutputDescription = outDescr };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
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
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, OutputDescription = outDescr };
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, OutputDescription = outDescr2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
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
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, Headers = headers };
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, Headers = headers2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
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
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, Headers = headers};
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, Headers = headers };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
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
            var webPut = new DsfWebPutActivity() { UniqueID = uniqueId, Headers = headers};
            var webPut1 = new DsfWebPutActivity() { UniqueID = uniqueId, Headers = headers };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
    }
}