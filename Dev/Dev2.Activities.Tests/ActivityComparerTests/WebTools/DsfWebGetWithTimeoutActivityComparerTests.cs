using System;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.WebTools
{
    [TestClass]
    public class DsfWebGetWithTimeoutActivityComparerTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameUniqueID_EmptyWebGetRequestTools_AreEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentWebGetRequestToolIds_AreNotEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameWebGetRequestTool_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGetRequestActivity = new DsfWebGetRequestWithTimeoutActivity();
            var webGetRequestActivity1 = webGetRequestActivity;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGetRequestActivity);
            //---------------Execute Test ----------------------
            var @equals = webGetRequestActivity.Equals(webGetRequestActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_DisplayName_Value_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, DisplayName = "" };
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, DisplayName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_DisplayName_Value_IsNOT_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webGet = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var webGet1 = new DsfWebGetRequestWithTimeoutActivity() { UniqueID = uniqueId, DisplayName = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webGet);
            //---------------Execute Test ----------------------
            var @equals = webGet.Equals(webGet1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_DisplayName_Value_Different_Casing_IsNotEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Result_IsEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_Result_IsNOTEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Url_IsEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Url_Different_Casing_IsNOtEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_Url_IsNOTEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_Headers_IsNOTEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Headers_Different_Casing_IsNotEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Headers_IsEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_Method_IsNOTEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Method_Different_Casing_IsNotEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Method_IsEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_TimeOutText_IsEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_TimeOutText_Different_Casing_IsNotEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_TimeOutText_IsNotEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_TimeoutSeconds_IsNotEqual()
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_TimeoutSeconds_IsEqual()
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
    }
}