
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
using Dev2.Common.Interfaces.Core.Convertors.Case;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.ConverterTests.Case
{
    /// <summary>
    /// Summary description for CaseConverterTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CaseConverterTests
    {
        ICaseConverter converter = CaseConverterFactory.CreateCaseConverter();

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region All Upper Tests

        /// <summary>
        /// Tests that the Upper method converts all characters to upper case
        /// </summary>
        [TestMethod]
        public void AllUpper_Simple_String_Expected_All_UpperCase()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("string to make uppercase", "");
            IBinaryDataListItem result = converter.TryConvert("UPPER", item);

            Assert.AreEqual("STRING TO MAKE UPPERCASE", result.TheValue);
        }

        /// <summary>
        /// Tests that the Upper method converts all characters to upper case
        /// </summary>
        [TestMethod]
        public void AllUpper_StringContainingNumerics_Expected_All_UpperCase()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("2strinG 4To 8m9ake upp522erc7ase", "");
            IBinaryDataListItem result = converter.TryConvert("UPPER", item);

            Assert.AreEqual("2STRING 4TO 8M9AKE UPP522ERC7ASE", result.TheValue);
        }

        /// <summary>
        /// Tests that the Upper Method does not attempt to convert Special Characters
        /// </summary>
        [TestMethod]
        public void AllUpper_Complex_String_Expected_All_UpperCase()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("string.(to)@ 'make' u//ppercase", "");
            IBinaryDataListItem result = converter.TryConvert("UPPER", item);

            Assert.AreEqual("STRING.(TO)@ 'MAKE' U//PPERCASE", result.TheValue);
        }

        #endregion All Upper Tests

        #region All Lower Tests

        /// <summary>
        /// Tests that the Lower method converts all text to lower case 
        /// </summary>
        [TestMethod]
        public void AllLower_Simple_String_Expected_All_LowerCase()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("STRING TO MAKE LOWERCASE", "");
            IBinaryDataListItem result = converter.TryConvert("lower", item);

            Assert.AreEqual("string to make lowercase", result.TheValue);
        }

        /// <summary>
        /// Tests that numeric characters are not converted by the case convert when specifying lower case
        /// </summary>
        [TestMethod]
        public void AllLower_StringContainingNumerics_Expected_All_LowerCase()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("2STRING T4O M6AKE LO08WERCASE", "");
            IBinaryDataListItem result = converter.TryConvert("lower", item);

            Assert.AreEqual("2string t4o m6ake lo08wercase", result.TheValue);
        }

        /// <summary>
        /// Tests that the Lower Method does not attempt to convert Special Characters
        /// </summary>
        [TestMethod]
        public void AllLower_Complex_String_Expected_All_LowerCase()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("STRING.(TO)@ 'MAKE' L//OWERCASE", "");
            IBinaryDataListItem result = converter.TryConvert("lower", item);

            Assert.AreEqual("string.(to)@ 'make' l//owercase", result.TheValue);
        }

        #endregion All Lower Tests

        #region Upper First Tests

        /// <summary>
        /// Tests that the converter converts the first character to Upper Case
        /// </summary>
        [TestMethod]
        public void UpperFirst_Simple_String_Expected_All_FirstLetterUpperCase()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("make first letter uppercase", "");
            IBinaryDataListItem result = converter.TryConvert("Sentence", item);

            Assert.AreEqual("Make first letter uppercase", result.TheValue);
        }

        /// <summary>
        /// Tests that numerics are not converted during case convert
        /// </summary>
        [TestMethod]
        public void UpperFirst_StringContainingNumerics_Expected_All_FirstLetterUpperCase()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("5make1 1first 3letter up5percase", "");
            IBinaryDataListItem result = converter.TryConvert("Sentence", item);

            Assert.AreEqual("5make1 1first 3letter up5percase", result.TheValue);
        }

        /// <summary>
        /// Tests that Special Characters are not converted by the Case Converter
        /// </summary>
        [TestMethod]
        public void UpperFirst_Complex_String_Expected_All_FirstLetterUpperCase()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("make.(first)@ 'letter'//cap", "");
            IBinaryDataListItem result = converter.TryConvert("Sentence", item);

            Assert.AreEqual("Make.(first)@ 'letter'//cap", result.TheValue);
        }

        #endregion Upper First Tests

        #region Upper First All Tests

        /// <summary>
        /// Tests that the UpperFirst all method functions according to specification
        /// </summary>
        [TestMethod]
        public void TitleCase_Simple_String_Expected_All_FirstLetterUpperCaseOnAll()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("make first letter uppercase", "");
            IBinaryDataListItem result = converter.TryConvert("Title Case", item);

            Assert.AreEqual("Make First Letter Uppercase", result.TheValue);
        }

        /// <summary>
        /// Tests that the UpperFirst all method functions according to specification
        /// </summary>
        [TestMethod]
        public void TitleCase_Simple_String_All_Caps_Expected_All_FirstLetterUpperCaseOnAll()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("MAKE FIRST LETTER UPPERCASE", "");
            IBinaryDataListItem result = converter.TryConvert("Title Case", item);

            Assert.AreEqual("MAKE FIRST LETTER UPPERCASE", result.TheValue);
        }


        /// <summary>
        /// Tests that Numeric characters are not converted by the UpperFirstAll method
        /// </summary>
        [TestMethod]
        public void TitleCase_StringWithNumeric_Expected_All_FirstLetterUpperCaseOnAll()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("1make fi5rst le6tter up3percase", "");
            IBinaryDataListItem result = converter.TryConvert("Title Case", item);

            Assert.AreEqual("1make Fi5rst Le6tter Up3percase", result.TheValue);
        }


        /// <summary>
        /// Tests that Special Characters are not converted by the UpperFirstAll method
        /// </summary>
        [TestMethod]
        public void TitleCase_Complex_String_Expected_All_FirstLetterUpperCaseOnAll()
        {
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem("make.(first)@ 'letter'//cap", "");
            IBinaryDataListItem result = converter.TryConvert("Title Case", item);

            Assert.AreEqual("Make.(First)@ 'Letter'//Cap", result.TheValue);
        }



        #endregion Upper First All Tests
    }
}
