using System;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Ouput;
using Dev2.Common.Interfaces.Core.Graph;
using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Tests.Activities.ActivityComparerTests.WebTools
{
    [TestClass]
    public class DsfWebPostActivityComparerTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameUniqueID_EmptyWebPostTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new DsfWebPostActivity() { UniqueID = uniqueId };
            var webPostActivity1 = new DsfWebPostActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var @equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentWebPostToolIds_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var uniqueId2 = Guid.NewGuid().ToString();
            var webPostActivity = new DsfWebPostActivity() { UniqueID = uniqueId };
            var webPostActivity1 = new DsfWebPostActivity() { UniqueID = uniqueId2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var @equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameWebPostTool_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new DsfWebPostActivity();
            var webPostActivity1 = webPostActivity;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var @equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_DisplayName_Value_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPut = new DsfWebPostActivity() { UniqueID = uniqueId, DisplayName = "" };
            var webPut1 = new DsfWebPostActivity() { UniqueID = uniqueId, DisplayName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_DisplayName_Value_IsNOT_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPut = new DsfWebPostActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var webPut1 = new DsfWebPostActivity() { UniqueID = uniqueId, DisplayName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_DisplayName_Value_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPut = new DsfWebPostActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var webPut1 = new DsfWebPostActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_QueryString_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPut = new DsfWebPostActivity() { UniqueID = uniqueId, QueryString = "A" };
            var webPut1 = new DsfWebPostActivity() { UniqueID = uniqueId, QueryString = "A" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_QueryString_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPut = new DsfWebPostActivity() { UniqueID = uniqueId, QueryString = "A" };
            var webPut1 = new DsfWebPostActivity() { UniqueID = uniqueId, QueryString = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_OutputDescription_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outDescr = new OutputDescription();
            var webPut = new DsfWebPostActivity() { UniqueID = uniqueId, OutputDescription = outDescr };
            var webPut1 = new DsfWebPostActivity() { UniqueID = uniqueId, OutputDescription = outDescr };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
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
            var webPut = new DsfWebPostActivity() { UniqueID = uniqueId, OutputDescription = outDescr };
            var webPut1 = new DsfWebPostActivity() { UniqueID = uniqueId, OutputDescription = outDescr2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_Headers_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue>();
            var headers2 = new List<INameValue> { new NameValue("a", "x") };
            var webPut = new DsfWebPostActivity() { UniqueID = uniqueId, Headers = headers };
            var webPut1 = new DsfWebPostActivity() { UniqueID = uniqueId, Headers = headers2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Headers_DifferentIndexes_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue> { new NameValue("b", "y"), new NameValue("a", "x") };
            var headers2 = new List<INameValue> { new NameValue("a", "x"), new NameValue("b", "y") };
            var webPut = new DsfWebPostActivity() { UniqueID = uniqueId, Headers = headers};
            var webPut1 = new DsfWebPostActivity() { UniqueID = uniqueId, Headers = headers };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Headers_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue> { new NameValue("a", "x") };
            var webPut = new DsfWebPostActivity() { UniqueID = uniqueId, Headers = headers};
            var webPut1 = new DsfWebPostActivity() { UniqueID = uniqueId, Headers = headers };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPut);
            //---------------Execute Test ----------------------
            var @equals = webPut.Equals(webPut1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
    }
}