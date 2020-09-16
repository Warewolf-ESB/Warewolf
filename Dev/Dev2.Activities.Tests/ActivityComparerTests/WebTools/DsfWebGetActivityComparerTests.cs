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
    public class DsfWebGetActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameUniqueID_EmptyWebGetTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGetActivity = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId };
            var webGetActivity1 = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGetActivity);
            //---------------Execute Test ----------------------
            var @equals = webGetActivity.Equals(webGetActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentWebGetToolIds_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var uniqueId2 = Guid.NewGuid().ToString();
            var webGetActivity = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId };
            var webGetActivity1 = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGetActivity);
            //---------------Execute Test ----------------------
            var @equals = webGetActivity.Equals(webGetActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameWebGetTool_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGetActivity = new DsfWebGetActivityWithBase64();
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_DisplayName_Value_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, DisplayName = "" };
            var webGet1 = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, DisplayName = "" };
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
            var webGet = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, DisplayName = "A" };
            var webGet1 = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, DisplayName = "" };
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
            var webGet = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, DisplayName = "A" };
            var webGet1 = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, DisplayName = "a" };
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
        public void Equals_Given_Same_QueryString_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, QueryString = "A" };
            var webGet1 = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, QueryString = "A" };
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
        public void Equals_Given_Different_QueryString_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, QueryString = "A" };
            var webGet1 = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, QueryString = "B" };
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
        public void Equals_Given_Same_OutputDescription_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outDescr = new OutputDescription();
            var webGet = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, OutputDescription = outDescr };
            var webGet1 = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, OutputDescription = outDescr };
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
            var webGet = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, OutputDescription = outDescr };
            var webGet1 = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, OutputDescription = outDescr2 };
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
            var headers = new List<INameValue>();
            var headers2 = new List<INameValue> { new NameValue("a", "x") };
            var webGet = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, Headers = headers };
            var webGet1 = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, Headers = headers2 };
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
        public void Equals_Given_Same_Headers_DifferentIndexes_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue> { new NameValue("b", "y"), new NameValue("a", "x") };
            var headers2 = new List<INameValue> { new NameValue("a", "x"), new NameValue("b", "y") };
            var webGet = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, Headers = headers};
            var webGet1 = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, Headers = headers };
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
        public void Equals_Given_Same_Headers_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue> { new NameValue("a", "x") };
            var webGet = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, Headers = headers};
            var webGet1 = new DsfWebGetActivityWithBase64() { UniqueID = uniqueId, Headers = headers };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
    }
}