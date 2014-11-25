
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Diagnostics.CodeAnalysis;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.TO
{
    /// <summary>
    /// Summary description for ErrorResultTOTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ErrorResultTOTests
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
        public void ErrorResultsTOMakeErrorResultFromDataListStringWithMultipleErrorsExpectedCorrectErrorResultTO()
        {
            ErrorResultTO makeErrorResultFromDataListString = ErrorResultTO.MakeErrorResultFromDataListString("<InnerError>First Error</InnerError><InnerError>Second Error</InnerError>");
            Assert.IsTrue(makeErrorResultFromDataListString.HasErrors());
            Assert.AreEqual(2, makeErrorResultFromDataListString.FetchErrors().Count);
            Assert.AreEqual("First Error", makeErrorResultFromDataListString.FetchErrors()[0]);
            Assert.AreEqual("Second Error", makeErrorResultFromDataListString.FetchErrors()[1]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ErrorResultTO_MakeErrorResultFromDataListString")]
        public void ErrorResultTO_MakeErrorResultFromDataListString_WhenErrorStringNotValidXML_ShouldJustAddTheError()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            ErrorResultTO makeErrorResultFromDataListString = ErrorResultTO.MakeErrorResultFromDataListString("<InnerError>Could not insert <> into a field</InnerError>");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, makeErrorResultFromDataListString.FetchErrors().Count);
            Assert.AreEqual("<Error><InnerError>Could not insert <> into a field</InnerError></Error>", makeErrorResultFromDataListString.FetchErrors()[0]);
        }
    }
}
