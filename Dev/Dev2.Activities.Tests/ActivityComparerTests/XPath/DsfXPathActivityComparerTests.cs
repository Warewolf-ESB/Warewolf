using System;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

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
                    new Unlimited.Applications.BusinessDesignStudio.Activities.XPathDTO { }
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

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_EmptyXpathDtos_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO();
            var xPathActivity1 = new XPathDTO();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameXPath_XpathDtos_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { XPath = "" };
            var xPathActivity1 = new XPathDTO() { XPath = "" } ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentXPath_XpathDtos_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { XPath = "Some path" };
            var xPathActivity1 = new XPathDTO() { XPath = "Some other path" } ;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameOutputVariable_XpathDtos_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { OutputVariable = "" };
            var xPathActivity1 = new XPathDTO() { OutputVariable = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentOutputVariable_XpathDtos_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { OutputVariable = "some variable" };
            var xPathActivity1 = new XPathDTO() { OutputVariable = "some other variable" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameIndexNumber_XpathDtos_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { IndexNumber = 2 };
            var xPathActivity1 = new XPathDTO() { IndexNumber = 2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentIndexNumber_XpathDtos_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { IndexNumber = 0 };
            var xPathActivity1 = new XPathDTO() { IndexNumber = 2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameInserted_XpathDtos_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { Inserted = true };
            var xPathActivity1 = new XPathDTO() { Inserted = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentInserted_XpathDtos_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { Inserted = true };
            var xPathActivity1 = new XPathDTO() { Inserted = false };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameIsXpathVariableFocused_XpathDtos_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { IsXpathVariableFocused = true };
            var xPathActivity1 = new XPathDTO() { IsXpathVariableFocused = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentIsXpathVariableFocused_XpathDtos_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { IsXpathVariableFocused = true };
            var xPathActivity1 = new XPathDTO() { IsXpathVariableFocused = false };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameIsOutputVariableFocused_XpathDtos_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { IsOutputVariableFocused = true };
            var xPathActivity1 = new XPathDTO() { IsOutputVariableFocused = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentIsOutputVariableFocused_XpathDtos_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { IsOutputVariableFocused = true };
            var xPathActivity1 = new XPathDTO() { IsOutputVariableFocused = false };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameWatermarkTextVariable_XpathDtos_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { WatermarkTextVariable = "" };
            var xPathActivity1 = new XPathDTO() { WatermarkTextVariable = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentWatermarkTextVariable_XpathDtos_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var xPathActivity = new XPathDTO() { WatermarkTextVariable = "some varable" };
            var xPathActivity1 = new XPathDTO() { WatermarkTextVariable = "some other variable" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(xPathActivity);
            //---------------Execute Test ----------------------
            var @equals = xPathActivity.Equals(xPathActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}