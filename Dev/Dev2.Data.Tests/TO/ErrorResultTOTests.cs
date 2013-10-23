using System;
using System.Text;
using System.Collections.Generic;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;

namespace Dev2.Data.Tests.TO
{
    /// <summary>
    /// Summary description for ErrorResultTOTests
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class ErrorResultTOTests
    {
        public ErrorResultTOTests()
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
        public void ErrorResultsTOMakeErrorResultFromDataListStringWithMultipleErrorsExpectedCorrectErrorResultTO()
        {
            ErrorResultTO makeErrorResultFromDataListString = ErrorResultTO.MakeErrorResultFromDataListString("<InnerError>First Error</InnerError><InnerError>Second Error</InnerError>");
            Assert.IsTrue(makeErrorResultFromDataListString.HasErrors());
            Assert.IsTrue(makeErrorResultFromDataListString.FetchErrors().Count == 2);
            Assert.IsTrue(makeErrorResultFromDataListString.FetchErrors()[0] == "First Error");
            Assert.IsTrue(makeErrorResultFromDataListString.FetchErrors()[1] == "Second Error");
        }
    }
}
