using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Utils
{
    /// <summary>
    /// Summary description for WorkflowDesignerUtilsTest
    /// </summary>
    [TestClass]
    public class WorkflowDesignerUtilsTest
    {
        public WorkflowDesignerUtilsTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CanFormatDsfActivityFieldHandleSpecialCharsWithNoException()
        {
           WorkflowDesignerUtils wdu = new WorkflowDesignerUtils();

           IList<string> result = wdu.FormatDsfActivityField(TestResourceStringsTest.SpecialChars);

           Assert.AreEqual(0, result.Count, "Strange behaviors parsing special chars, I got results when I should not?!");
        }

        [TestMethod]
        public void CanFormatDsfActivityFieldHandleNormalParse()
        {
            WorkflowDesignerUtils wdu = new WorkflowDesignerUtils();

            IList<string> result = wdu.FormatDsfActivityField("[[MoIsNotUber]]");

            Assert.AreEqual(1, result.Count, "Strange behaviors parsing normal regions, I was expecting 1 result");
        }

        //2013.06.10: Ashley Lewis for bug 9306 - Format DsfActivity handles mismatched region braces better
        [TestMethod]
        public void CanFormatDsfActivityFieldWithMissmatchedRegionBracesExpectedNotParsed()
        {
            WorkflowDesignerUtils wdu = new WorkflowDesignerUtils();

            IList<string> result = wdu.FormatDsfActivityField("[[MoIsNotUber([[invalid).field]]");

            Assert.AreEqual(0, result.Count, "Format DsfActivity returned results when the region braces where missmatched");
        }
    }
}
