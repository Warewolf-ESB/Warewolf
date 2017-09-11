using System;
using Dev2.Common.ExtMethods;
using Dev2.Data.ServiceModel;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Dev2.Tests.Activities.ActivityComparerTests.XPath
{
    [TestClass]
    public class DsfXPathActivityComparerTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void UniqueIDEquals_EmptyXpathTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new DsfXPathActivity() { UniqueID = uniqueId };
            var xPathActivity1 = new DsfXPathActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void UniqueIDEquals_Given_DifferentXpathToolIds_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var uniqueId2 = Guid.NewGuid().ToString();
            var xPathActivity = new DsfXPathActivity() { UniqueID = uniqueId };
            var xPathActivity1 = new DsfXPathActivity() { UniqueID = uniqueId2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SamexPathTool_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new DsfXPathActivity();
            var xPathActivity1 = xPathActivity;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentxPathTools_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new DsfXPathActivity();
            var xPathActivity1 = new DsfXPathActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_SourceString_Value_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPath = new DsfXPathActivity() { UniqueID = uniqueId, SourceString = "" };
            var xPath1 = new DsfXPathActivity() { UniqueID = uniqueId, SourceString = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPath);
            //---------------Execute Test ----------------------
            var @equals = xPath.Equals(xPath1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_SourceString_Value_IsNOT_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPath = new DsfXPathActivity() { UniqueID = uniqueId, SourceString = "A" };
            var xPath1 = new DsfXPathActivity() { UniqueID = uniqueId, SourceString = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPath);
            //---------------Execute Test ----------------------
            var @equals = xPath.Equals(xPath1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_SourceString_Value_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPath = new DsfXPathActivity() { UniqueID = uniqueId, SourceString = "A" };
            var xPath1 = new DsfXPathActivity() { UniqueID = uniqueId, SourceString = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPath);
            //---------------Execute Test ----------------------
            var @equals = xPath.Equals(xPath1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Empty_ResultCollection_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var resultCol = new List<Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO>();
            var xPath = new DsfXPathActivity() { UniqueID = uniqueId, ResultsCollection = resultCol };
            var xPath1 = new DsfXPathActivity() { UniqueID = uniqueId, ResultsCollection = resultCol };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPath);
            //---------------Execute Test ----------------------
            var @equals = xPath.Equals(xPath1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_ResultCollection_With_Same_Items_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var resultCol = new List<Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO>();
            resultCol.Add
                (
                    new Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO{}
                );
            var xPath = new DsfXPathActivity() { UniqueID = uniqueId, ResultsCollection = resultCol };
            var xPath1 = new DsfXPathActivity() { UniqueID = uniqueId, ResultsCollection = resultCol };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPath);
            //---------------Execute Test ----------------------
            var @equals = xPath.Equals(xPath1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_ResultCollection_With_Empty_Items_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var resultCol = new List<Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO>();
            var xPath = new DsfXPathActivity() { UniqueID = uniqueId, ResultsCollection = resultCol };
            var xPath1 = new DsfXPathActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPath);
            //---------------Execute Test ----------------------
            var @equals = xPath.Equals(xPath1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_ResultCollection_With_Different_Items_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var resultCol = new List<Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO>();
            resultCol.Add
                (
                    new Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO
                    {
                        XPath = "Some xpath"
                    }
                );
            var resultCol2 = new List<Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO>();
            resultCol.Add
                (
                    new Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO { }
                );
            var xPath = new DsfXPathActivity() { UniqueID = uniqueId, ResultsCollection = resultCol };
            var xPath1 = new DsfXPathActivity() { UniqueID = uniqueId, ResultsCollection = resultCol2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPath);
            //---------------Execute Test ----------------------
            var @equals = xPath.Equals(xPath1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_ResultCollection_With_Same_Items_In_Different_Indexes_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var resultCol = new List<Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO>();
            resultCol.Add
                (
                    new Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO { }
                );
            resultCol.Add
                (
                    new Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO
                    {
                        XPath = "Some xpath"
                    }
                );
            var resultCol2 = new List<Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO>();
            resultCol.Add
                (
                    new Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO
                    {
                        XPath = "Some xpath"
                    }
                );
            resultCol.Add
                (
                    new Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO { }
                );
            var xPath = new DsfXPathActivity() { UniqueID = uniqueId, ResultsCollection = resultCol };
            var xPath1 = new DsfXPathActivity() { UniqueID = uniqueId, ResultsCollection = resultCol2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPath);
            //---------------Execute Test ----------------------
            var @equals = xPath.Equals(xPath1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}