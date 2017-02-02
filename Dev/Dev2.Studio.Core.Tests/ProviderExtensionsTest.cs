/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Intellisense.Provider;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class ProviderExtensionsTest
    {
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_EmptyString_ExpectEmpty()
        {
            const string inputText = "";
            const int caretPosition = 13;
            const string expectedResult = "";
            FindTextTestHelper(caretPosition, inputText, expectedResult);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_InvalidPositionNegative_ExpectEmpty()
        {

            FindTextTestHelper(-1, "bob", "");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_InvalidPosition_ExpectValue()
        {

            FindTextTestHelper(29, "bob", "bob");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_ClosedBraceAfterValue_ExpectValue()
        {

            FindTextTestHelper(9, "[[rec.(pp)]]", "pp");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_ClosedBrace_ExpectValue()
        {

            FindTextTestHelper(9, ")", "");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_ClosedSquareBraceAfterValue_ExpectValue()
        {

            FindTextTestHelper(5, "[[bob]]", "[[bob");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_ClosedSquareBracePartialWord_ExpectValue()
        {

            FindTextTestHelper(4, "[[bob]]", "[[bo");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_UnbalancedSquareBracePartialWord_ExpectValue()
        {

            FindTextTestHelper(4, "[[bob", "[[bo");
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_Spaces_ExpectValue()
        {

            FindTextTestHelper(16, "[[rec.( dora bob )  ", "bob");
            FindTextTestHelper(12, "[[rec.( dora bob )  ", "dora");
            FindTextTestHelper(10, "[[rec  .( dora bob )  ", "");
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_ClosedSquareBraceEmpty_ExpectEmpty()
        {

            FindTextTestHelper(4, "[[ ]]", "");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_ClosedRoundBrace_ExpectEmpty()
        {

            FindTextTestHelper(2, "( )", "");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_OpenRoundBrace_ExpectCorrectRegions()
        {

            FindTextTestHelper(2, "( ", "");
            FindTextTestHelper(1, "a( ", "a");
            FindTextTestHelper(1, "(a ", "");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_MatchingRoundBrace_ExpectCorrectRegions()
        {

            FindTextTestHelper(2, "( )", "");
            FindTextTestHelper(2, "(a) ", "a");
            FindTextTestHelper(2, "()a ()", "");
            FindTextTestHelper(7, "()([abc ()", "[abc");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_SpecialCharacters_ExpectCorrectRegions()
        {

            FindTextTestHelper(4, "(a++)", "");
            FindTextTestHelper(2, "(a+*)", "a");
            FindTextTestHelper(3, "(a%^)", "");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_NullThrowsException()
        {

           ((IntellisenseProviderContext)null).FindTextToSearch();
         
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_UnbalalancedBoundary_ExpectValue()
        {
            FindTextTestHelper(2, "[ba )", "[b");
            FindTextTestHelper(8, "dave [ba)", "[ba");
            FindTextTestHelper(8, "dave (ba]", "ba");
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_InputTextIsNull_EmptyString()
        {
            FindTextTestHelper(2, null, "");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_FindTextToSearch")]
        public void ProviderExtensions_FindTextToSearch_InputTextIsEmpty_EmptyString()
        {
            FindTextTestHelper(2, "", "");
        }

        private static void FindTextTestHelper(int caretPosition, string inputText, string expectedResult)
        {
            var context = new IntellisenseProviderContext
                {
                    CaretPosition = caretPosition,
                    InputText = inputText,
                    DesiredResultSet = IntellisenseDesiredResultSet.Default
                };

            var search = context.FindTextToSearch();
            Assert.AreEqual(expectedResult, search);
        }
    }
}
