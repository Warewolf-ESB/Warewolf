
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
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
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ScalarAndPositionIsInsideVariable_Scalar()
        {
            const string data = "[[var1]]";
            var region = data.RegionInPostion(3);
            Assert.AreEqual("[[var1]]", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ScalarAndPositionIsZero_EmptyString()
        {
            const string data = "[[var1]]";
            var region = data.RegionInPostion(0);
            Assert.AreEqual("", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ScalarAndPositionIsGreaterThanStringLength_EmptyString()
        {
            const string data = "[[var1]]";
            var region = data.RegionInPostion(100);
            Assert.AreEqual("", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_SomeTextAndScalarAndPositionIsInsideVariable_Scalar()
        {
            const string data = "Some text [[var1]]";
            var region = data.RegionInPostion(13);
            Assert.AreEqual("[[var1]]", region.Name);
            Assert.AreEqual(10, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_SomeTextAndScalarAndPositionIsZero_EmptyString()
        {
            const string data = "Some text [[var1]]";
            var region = data.RegionInPostion(0);
            Assert.AreEqual("", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        public void ProviderExtensions_RegionInPosition_SomeTextAndScalarAndPositionIsGreaterThanStringLength_EmptyString()
        {
            const string data = "Some text [[var1]]";
            var region = data.RegionInPostion(100);
            Assert.AreEqual("", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_TwoScalarsAndPositionIsInsideFirstVariable_Scalar()
        {
            const string data = "[[var1]] [[var2]]";
            var region = data.RegionInPostion(3);
            Assert.AreEqual("[[var1]]", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_TwoScalarsAndPositionIsInsideSecondVariable_Scalar()
        {
            const string data = "[[var1]] [[var2]]";
            var region = data.RegionInPostion(13);
            Assert.AreEqual("[[var2]]", region.Name);
            Assert.AreEqual(9, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ComplexVariableAndPositionIsTheMainVariable_EntireComplexVariable()
        {
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var region = data.RegionInPostion(2);
            Assert.AreEqual(data, region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ComplexVariableAndPositionIsTheFirstVariable_OnlyFirstVariableReturned()
        {
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var region = data.RegionInPostion(4);
            Assert.AreEqual("[[var1]]", region.Name);
            Assert.AreEqual(2, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ComplexVariableAndPositionIsTheSecondVariable_OnlySecondVariableReturned()
        {
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var region = data.RegionInPostion(13);
            Assert.AreEqual("[[var2]]", region.Name);
            Assert.AreEqual(10, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ComplexVariableAndPositionIsRecordset_OnlyRecordsetReturned()
        {
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var region = data.RegionInPostion(21);
            Assert.AreEqual("[[rec([[indexVar]]).fielA]]", region.Name);
            Assert.AreEqual(18, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_InputStringIsEmpty_NoRegion()
        {
            const string data = "";
            var region = data.RegionInPostion(21);
            Assert.AreEqual("", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ComplexVariableAndPositionIsRecordsetIndex_OnlyRecordsetIndexReturned()
        {
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var region = data.RegionInPostion(27);
            Assert.AreEqual("[[indexVar]]", region.Name);
            Assert.AreEqual(24, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_AllIndexesOf")]
        public void ProviderExtensions_AllIndexesOf_TextStartingWithVar_Indexes4And12()
        {
            var expected = new List<int> { 4, 12 };
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var actual = data.AllIndexesOf("var");
            CollectionAssert.AreEqual(actual.ToList(), expected);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_AllIndexesOf")]
        public void ProviderExtensions_AllIndexesOf_StringIsEmpty_Indexes0()
        {
            var expected = new List<int> { 0 };
            const string data = "";
            var actual = data.AllIndexesOf("var");
            CollectionAssert.AreEqual(actual.ToList(), expected);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_AllIndexesOf")]
        public void ProviderExtensions_AllIndexesOf_SearchStringIsEmpty_Indexes0()
        {
            var expected = new List<int> { 0 };
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var actual = data.AllIndexesOf("");
            CollectionAssert.AreEqual(actual.ToList(), expected);
        }
        
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
