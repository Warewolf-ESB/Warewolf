using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using DirectoryUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QueueBuildTest
{
    /// <summary>
    /// Summary description for DirectoryTest
    /// </summary>
    [TestClass]
    public class DirectoryTest
    {
        public DirectoryTest()
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
        public void CanCopyContentsWithoutPattern()
        {
            var src = @"F:\foo";
            var dst = @"F:\bar";

            Directory.CreateDirectory(dst);

            int res = DirectoryCommands.Main(new string[] {src, dst});

            Directory.Delete(dst, true);

            Assert.AreEqual(0, res, "Failed to copy directory contents ;(");  

        }

        [TestMethod]
        public void CanCopyContentsWithPattern()
        {
            var src = @"F:\foo";
            var dst = @"F:\bar";

            Directory.CreateDirectory(dst);

            int res = DirectoryCommands.Main(new string[] { src, dst, "*.dll" });

            Directory.Delete(dst, true);

            Assert.AreEqual(0, res, "Failed to copy directory contents ;(");

        }
    }
}
