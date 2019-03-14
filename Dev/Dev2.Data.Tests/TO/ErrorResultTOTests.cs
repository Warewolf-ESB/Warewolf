/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Text;
using Dev2.Data.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Data.Tests.TO
{
    [TestClass]
    public class ErrorResultTOTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_AddError_CheckForDuplicates_True_AddSameError_ExpectLstToBeSame()
        {
            var resultTo = new ErrorResultTO();
            resultTo.AddError("some message", true);
            resultTo.AddError("some message", true);

            Assert.IsTrue(resultTo.HasErrors());
            Assert.AreEqual(1, resultTo.FetchErrors().Count);
            Assert.AreEqual("some message", resultTo.FetchErrors()[0]);
            Assert.AreEqual("<InnerError>some message</InnerError>", resultTo.MakeDataListReady());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_AddError_CheckForDuplicates_True_AddNewError_ExpectTrue()
        {
            var resultTo = new ErrorResultTO();
            resultTo.AddError("some message", true);
            resultTo.AddError("some message", true);
            resultTo.AddError("deferent message", true);

            Assert.IsTrue(resultTo.HasErrors());
            Assert.AreEqual(2, resultTo.FetchErrors().Count);
            Assert.AreEqual("some message", resultTo.FetchErrors()[0]);
            Assert.AreEqual("deferent message", resultTo.FetchErrors()[1]);
            Assert.AreEqual("<InnerError>some message</InnerError><InnerError>deferent message</InnerError>", resultTo.MakeDataListReady());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_AddError_NullMessage()
        {
            var resultTo = new ErrorResultTO();
            resultTo.AddError(null, true);

            Assert.IsFalse(resultTo.HasErrors());
            // Shouldn't this be passing?
            Assert.AreEqual(0, resultTo.FetchErrors().Count);
            Assert.AreEqual("", resultTo.MakeDataListReady());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_MakeErrorResultFromDataListStringWithMultipleErrorsExpectedCorrectErrorResultTO()
        {
            var makeErrorResultFromDataListString = ErrorResultTO.MakeErrorResultFromDataListString("<InnerError>First Error</InnerError><InnerError>Second Error</InnerError>");
            Assert.IsTrue(makeErrorResultFromDataListString.HasErrors());
            Assert.AreEqual(2, makeErrorResultFromDataListString.FetchErrors().Count);
            Assert.AreEqual("First Error", makeErrorResultFromDataListString.FetchErrors()[0]);
            Assert.AreEqual("Second Error", makeErrorResultFromDataListString.FetchErrors()[1]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_MakeErrorResultFromDataListString_WhenErrorStringNotValidXML_ShouldJustAddTheError()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var makeErrorResultFromDataListString = ErrorResultTO.MakeErrorResultFromDataListString("<InnerError>Could not insert <> into a field</InnerError>");
            //------------Assert Results-------------------------
            Assert.AreEqual(1, makeErrorResultFromDataListString.FetchErrors().Count);
            Assert.AreEqual("<Error><InnerError>Could not insert <> into a field</InnerError></Error>", makeErrorResultFromDataListString.FetchErrors()[0]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_MergeErrors_ShouldJustRemoveTheErrorInTheCollection()
        {
            var errorResultTo = new ErrorResultTO();

            Assert.AreEqual(0, errorResultTo.FetchErrors().Count);
            errorResultTo.AddError("SomeError");
            Assert.AreEqual(1, errorResultTo.FetchErrors().Count);

            var merge = new ErrorResultTO();
            merge.AddError("Error to merge");
            errorResultTo.MergeErrors(merge);

            Assert.AreEqual(2, errorResultTo.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_MergeErrors_EmptyOther()
        {
            var errorResultTo = new ErrorResultTO();

            Assert.AreEqual(0, errorResultTo.FetchErrors().Count);
            errorResultTo.AddError("SomeError");

            Assert.AreEqual(1, errorResultTo.FetchErrors().Count);

            var merge = new ErrorResultTO();
            errorResultTo.MergeErrors(merge);
            Assert.AreEqual(1, errorResultTo.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_MergeErrors_NullOtherDoesNotThrow()
        {
            var errorResultTo = new ErrorResultTO();

            Assert.AreEqual(0, errorResultTo.FetchErrors().Count);
            errorResultTo.AddError("SomeError");

            Assert.AreEqual(1, errorResultTo.FetchErrors().Count);

            errorResultTo.MergeErrors(null);
            Assert.AreEqual(1, errorResultTo.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_Remove_ShouldJustRemoveTheErrorInTheCollection()
        {
            var errorResultTo = new ErrorResultTO();

            Assert.AreEqual(0, errorResultTo.FetchErrors().Count);
            errorResultTo.AddError("SomeError");

            Assert.AreEqual(1, errorResultTo.FetchErrors().Count);

            errorResultTo.RemoveError("SomeError");
            Assert.AreEqual(0, errorResultTo.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_Clear_ShouldEmptyTheErrorCollection()
        {
            var errorResultTo = new ErrorResultTO();
            errorResultTo.AddError("SomeError");
            errorResultTo.AddError("AnotherError");

            Assert.AreEqual(2, errorResultTo.FetchErrors().Count);

            errorResultTo.ClearErrors();

            Assert.AreEqual(0, errorResultTo.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_MakeDisplayReady_ShouldReturnAllErrorsAsOne()
        {
            var result = new StringBuilder();
            result.AppendLine("SomeError");
            result.Append("AnotherError");
            var errorResultTo = new ErrorResultTO();

            Assert.AreEqual(0, errorResultTo.FetchErrors().Count);
            errorResultTo.AddError("SomeError");
            errorResultTo.AddError("AnotherError");

            Assert.AreEqual(2, errorResultTo.FetchErrors().Count);

            var makeDisplayReady = errorResultTo.MakeDisplayReady();
            Assert.AreEqual(result.ToString(), makeDisplayReady);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_MakeDataListReady_ShouldReturnAllErrorsAsOne()
        {
            var result = "<InnerError>SomeError</InnerError><InnerError>AnotherError</InnerError>";
            var errorResultTo = new ErrorResultTO();

            Assert.AreEqual(0, errorResultTo.FetchErrors().Count);
            errorResultTo.AddError("SomeError");
            errorResultTo.AddError("AnotherError");

            Assert.AreEqual(2, errorResultTo.FetchErrors().Count);

            var makeDisplayReady = errorResultTo.MakeDataListReady();
            Assert.AreEqual(result, makeDisplayReady);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_MakeDataListReady_AsXmlFalseShouldReturnAllErrorsAsOne()
        {
            var result = "\"errors\": [ \"SomeError\",\"AnotherError\"]";
            var errorResultTo = new ErrorResultTO();

            Assert.AreEqual(0, errorResultTo.FetchErrors().Count);
            errorResultTo.AddError("SomeError");
            errorResultTo.AddError("AnotherError");

            Assert.AreEqual(2, errorResultTo.FetchErrors().Count);

            var makeDisplayReady = errorResultTo.MakeDataListReady(false);
            Assert.AreEqual(result, makeDisplayReady);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ErrorResultTO))]
        public void ErrorResultTO_MakeDataListReady_CannotSetUnknownMember_RemapsErrorMessage()
        {
            var result = "\"errors\": [ \"SomeError\",\"Resource has unrecognized formatting, this Warewolf Server may be to outdated to read this resource.\",\"Another Error\"]";
            var errorResultTo = new ErrorResultTO();

            Assert.AreEqual(0, errorResultTo.FetchErrors().Count);
            errorResultTo.AddError("SomeError");
            errorResultTo.AddError("Cannot set unknown member");
            errorResultTo.AddError("Another Error");

            Assert.AreEqual(3, errorResultTo.FetchErrors().Count);

            var makeDisplayReady = errorResultTo.MakeDataListReady(false);
            Assert.AreEqual(result, makeDisplayReady);
        }
    }
}
