using System;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.WebService
{
    [TestClass]
    public class DsfWebserviceActivityComparerTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameUniqueID_EmptyWebServiceTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webServiceActivity = new DsfWebserviceActivity() { UniqueID = uniqueId };
            var WebServiceActivity1 = new DsfWebserviceActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webServiceActivity);
            //---------------Execute Test ----------------------
            var @equals = webServiceActivity.Equals(WebServiceActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentWebServiceToolIds_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var uniqueId2 = Guid.NewGuid().ToString();
            var WebServiceActivity = new DsfWebserviceActivity() { UniqueID = uniqueId };
            var WebServiceActivity1 = new DsfWebserviceActivity() { UniqueID = uniqueId2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(WebServiceActivity);
            //---------------Execute Test ----------------------
            var @equals = WebServiceActivity.Equals(WebServiceActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameWebServiceTool_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var WebServiceActivity = new DsfWebserviceActivity();
            var WebServiceActivity1 = WebServiceActivity;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(WebServiceActivity);
            //---------------Execute Test ----------------------
            var @equals = WebServiceActivity.Equals(WebServiceActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_DisplayName_Value_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var WebService = new DsfWebserviceActivity() { UniqueID = uniqueId, DisplayName = "" };
            var WebService1 = new DsfWebserviceActivity() { UniqueID = uniqueId, DisplayName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(WebService);
            //---------------Execute Test ----------------------
            var @equals = WebService.Equals(WebService1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_DisplayName_Value_IsNOT_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var WebService = new DsfWebserviceActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var WebService1 = new DsfWebserviceActivity() { UniqueID = uniqueId, DisplayName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(WebService);
            //---------------Execute Test ----------------------
            var @equals = WebService.Equals(WebService1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_DisplayName_Value_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var WebService = new DsfWebserviceActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var WebService1 = new DsfWebserviceActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(WebService);
            //---------------Execute Test ----------------------
            var @equals = WebService.Equals(WebService1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}