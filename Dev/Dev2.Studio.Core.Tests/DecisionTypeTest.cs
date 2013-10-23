using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.Models;

namespace Dev2.Core.Tests {
    /// <summary>
    /// Summary description for DecisionTypeTest
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class DecisionTypeTest {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
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

        /// <summary>
        /// Tests that all decisionTypes are returned foreach operator type
        /// </summary>
        [TestMethod]
        public void ConstructorTest_Expected_ListOfAllOperatorTypesCreated() {
            IntegerDecisionType decisionType = new IntegerDecisionType();
            List<OperatorType> operators = OperatorTypeList(decisionType);
            Assert.AreEqual(decisionType.OperatorTypes.Count, operators.Count);
        }

        private List<OperatorType> OperatorTypeList(DecisionType decisionType) {
            List<OperatorType> operatorTypes = new List<OperatorType> { 
                        new OperatorType("t.Eq", "Is Equal To",  "{0}({1},{3}{2}{3})", decisionType),
                        new OperatorType("t.NtEq", "Is Not Equal To",  "{0}({1},{3}{2}{3})",decisionType),
                        new OperatorType("t.LsTh", "Is Less Than", "{0}({1},{3}{2}{3})",decisionType),
                        new OperatorType("t.GrTh", "Is Greater Than", "{0}({1},{3}{2}{3})",decisionType),
                        new OperatorType("t.GrThEq", "Is Greater Than Or Equal To","{0}({1},{3}{2}{3})",decisionType),
                        new OperatorType("t.LsThEq","Is Less Than Or Equal To", "{0}({1},{3}{2}{3})",decisionType),
                        new BetweenOperatorType("t.Btw","Is Between", "{0}({1},{3}{2}{3},{3}{4}{3})",decisionType)};

            return operatorTypes;
        }
    }
}
