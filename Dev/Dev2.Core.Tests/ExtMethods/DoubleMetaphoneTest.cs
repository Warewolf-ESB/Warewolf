using System.Diagnostics.CodeAnalysis;
using Dev2.Common.ExtMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.ExtMethods
{
    /// <summary>
    /// Summary description for DoubleMetaphoneTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DoubleMetaphoneTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

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
        [Owner("Travis Frisinger")]
        [TestCategory("DoubleMetaphone_GenerateDoubleMetaphone")]
        public void DoubleMetaphone_GenerateDoubleMetaphone_NormalUsage_GoodResult()
        {
            //------------Setup for test--------------------------
            string variable = "foobar1";


            //------------Execute Test---------------------------
            var result = variable.GenerateDoubleMetaphone();

            //------------Assert Results-------------------------

            Assert.AreEqual("FPR", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DoubleMetaphone_GenerateDoubleMetaphone")]
        public void DoubleMetaphone_GenerateDoubleMetaphone_NormalUsage_GoodResult2()
        {
            //------------Setup for test--------------------------
            string variable = "foobor2";


            //------------Execute Test---------------------------
            var result = variable.GenerateDoubleMetaphone();

            //------------Assert Results-------------------------

            Assert.AreEqual("FPR", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DoubleMetaphone_GenerateDoubleMetaphone")]
        public void DoubleMetaphone_GenerateDoubleMetaphone_NormalUsage_GoodResult3()
        {
            //------------Setup for test--------------------------
            string variable = "f1a";
            string variable2 = "f1";


            //------------Execute Test---------------------------
            var result = variable.GenerateDoubleMetaphone();
            var result2 = variable2.GenerateDoubleMetaphone();

            //------------Assert Results-------------------------

            Assert.AreEqual("F", result);
            Assert.AreEqual("F", result2);
        }

    }
}
