/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Text;
using Dev2.Data.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Dev2.Data.Tests.TO
{
    /// <summary>
    /// Summary description for ErrorResultTOTests
    /// </summary>
    [TestClass]
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

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void ErrorResultTO_MergeErrors_ShouldJustRemoveTheErrorInTheCollection()
        {
            ErrorResultTO errorResultTo = new ErrorResultTO();
            Assert.IsNotNull(errorResultTo);
            var prObj = new PrivateObject(errorResultTo);
            var errors = prObj.GetField("_errorList") as IList<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
            errorResultTo.AddError("SomeError");
            errors = prObj.GetField("_errorList") as IList<string>;
            if (errors != null) Assert.AreEqual(1, errors.Count);
            var merge = new ErrorResultTO();
            merge.AddError("Error to merge");
            errorResultTo.MergeErrors(merge);
            if (errors != null) Assert.AreEqual(2, errors.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void ErrorResultTO_Remove_ShouldJustRemoveTheErrorInTheCollection()
        {
            ErrorResultTO errorResultTo = new ErrorResultTO();
            Assert.IsNotNull(errorResultTo);
            var prObj = new PrivateObject(errorResultTo);
            var errors = prObj.GetField("_errorList") as IList<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
            errorResultTo.AddError("SomeError");
            errors = prObj.GetField("_errorList") as IList<string>;
            if (errors != null) Assert.AreEqual(1, errors.Count);
            errorResultTo.RemoveError("SomeError");
            if (errors != null) Assert.AreEqual(0, errors.Count);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void ErrorResultTO_Clear_ShouldEmptyTheErrorCollection()
        {
            var errorResultTo = new ErrorResultTO();
            Assert.IsNotNull(errorResultTo);
            var prObj = new PrivateObject(errorResultTo);
            var errors = prObj.GetField("_errorList") as IList<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
            errorResultTo.AddError("SomeError");
            errorResultTo.AddError("AnotherError");
            errors = prObj.GetField("_errorList") as IList<string>;
            if (errors != null) Assert.AreEqual(2, errors.Count);
            errorResultTo.ClearErrors();
            if (errors != null) Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void ErrorResultTO_MakeDisplayReady_ShouldReturnAllErrorsAsOne()
        {
            var result = new StringBuilder();
            result.AppendLine("SomeError");
            result.Append("AnotherError");
            var errorResultTo = new ErrorResultTO();
            Assert.IsNotNull(errorResultTo);
            var prObj = new PrivateObject(errorResultTo);
            var errors = prObj.GetField("_errorList") as IList<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
            errorResultTo.AddError("SomeError");
            errorResultTo.AddError("AnotherError");
            errors = prObj.GetField("_errorList") as IList<string>;
            if (errors != null) Assert.AreEqual(2, errors.Count);
            var makeDisplayReady = errorResultTo.MakeDisplayReady();            
            Assert.AreEqual(result.ToString(), makeDisplayReady);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void ErrorResultTO_MakeDataListReady_ShouldReturnAllErrorsAsOne()
        {
            var result = "<InnerError>SomeError</InnerError><InnerError>AnotherError</InnerError>";
            var errorResultTo = new ErrorResultTO();
            Assert.IsNotNull(errorResultTo);
            var prObj = new PrivateObject(errorResultTo);
            var errors = prObj.GetField("_errorList") as IList<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
            errorResultTo.AddError("SomeError");
            errorResultTo.AddError("AnotherError");
            errors = prObj.GetField("_errorList") as IList<string>;
            if (errors != null) Assert.AreEqual(2, errors.Count);
            var makeDisplayReady = errorResultTo.MakeDataListReady();            
            Assert.AreEqual(result, makeDisplayReady);
        }
    }
}
