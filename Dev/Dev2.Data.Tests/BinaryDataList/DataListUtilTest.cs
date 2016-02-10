
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Data.Tests.BinaryDataList
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class DataListUtilTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_IsJson")]
        public void DataListUtil_IsJson_WhenValidJsonString_ExpectTrue()
        {//------------Setup for test--------------------------
            const string startingData = "{ \"message\" : \"Howzit, Samantha\"}";
            //------------Execute Test---------------------------
            bool result = DataListUtil.IsJson(startingData);
            //------------Assert Results-------------------------
            Assert.IsTrue(result, "Else Valid JSON not detected as such");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_IsJson")]
        public void DataListUtil_IsJson_WhenValidJsonStringWithExtraWhitespace_ExpectTrue()
        {//------------Setup for test--------------------------
            const string startingData = " { \"message\" : \"Howzit, Samantha\"} ";
            //------------Execute Test---------------------------
            bool result = DataListUtil.IsJson(startingData);
            //------------Assert Results-------------------------
            Assert.IsTrue(result, "Else Valid JSON not detected as such");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_IsJson")]
        public void DataListUtil_IsJson_WhenInvalidJsonString_ExpectFalse()
        {//------------Setup for test--------------------------
            const string startingData = "<\"message\" : \"Howzit, Samantha\">";
            //------------Execute Test---------------------------
            bool result = DataListUtil.IsJson(startingData);
            //------------Assert Results-------------------------
            Assert.IsFalse(result, "Invalid JSON not detected as such");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataListUtil_AdjustForEncodingIssues")]
        public void DataListUtil_AdjustForEncodingIssues_WithStringLengthOf3_SameDataAsPassedIn()
        {
            //------------Setup for test--------------------------
            const string startingData = "A A";
            //------------Execute Test---------------------------
            string result = DataListUtil.AdjustForEncodingIssues(startingData);
            //------------Assert Results-------------------------
            Assert.AreEqual(startingData, result, "The data has changed when there was no encoding issues.");
        }

        [TestMethod]
        [Owner("eon Rajindrapersadh")]
        [TestCategory("DataListUtil_AdjustForEncodingIssues")]
        public void DataListUtil_AdjustForEncodingIssues_BOMRemoved()
        {
            //------------Setup for test--------------------------
            const char c = (char)65279;
            string startingData = c + "<A></A>";
            Assert.IsFalse(startingData.StartsWith("<",StringComparison.OrdinalIgnoreCase));
            //------------Execute Test---------------------------
            string result = DataListUtil.AdjustForEncodingIssues(startingData);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.StartsWith("<"));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListUtil_AdjustForEncodingIssues")]
        public void DataListUtil_GetRecordsetIndexTypeRaw_StarNotationDoesNotParseInt()
        {
            //------------Setup for test--------------------------
            const string startingData = "**";
            //------------Execute Test---------------------------
            Assert.AreEqual(enRecordsetIndexType.Error, DataListUtil.GetRecordsetIndexTypeRaw(startingData));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListUtil_AdjustForEncodingIssues")]
        public void DataListUtil_GetRecordsetIndexTypeRaw_IntParsesInt()
        {
            //------------Setup for test--------------------------
            const string startingData = "1";
            //------------Execute Test---------------------------
            Assert.AreEqual(enRecordsetIndexType.Numeric, DataListUtil.GetRecordsetIndexTypeRaw(startingData));
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_IsSystemTag")]
        public void DataListUtil_IsSystemTag_WhenNoPrefix_ExpectSystemTagDetected()
        {
            //------------Setup for test--------------------------
            const string tag = "ManagmentServicePayload";

            //------------Execute Test---------------------------
            var result = DataListUtil.IsSystemTag(tag);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_IsSystemTag")]
        public void DataListUtil_IsSystemTag_WhenDev2SystemPrefix_ExpectSystemTagDetected()
        {
            //------------Setup for test--------------------------
            const string tag = GlobalConstants.SystemTagNamespaceSearch + "ManagmentServicePayload";

            //------------Execute Test---------------------------
            var result = DataListUtil.IsSystemTag(tag);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }





        
        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure star is replaced with an index")]
        [TestCategory("DataListUtil,UnitTest")]
        public void DataListUtil_UnitTest_ReplaceStarWithFixedIndex()
        {
            const string exp = "[[rs(*).val]]";
            var result = DataListUtil.ReplaceStarWithFixedIndex(exp, 1);

            Assert.AreEqual("[[rs(1).val]]", result, "Did not replace index in recordset");
        }

        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure star is not replaced with an invalid index")]
        [TestCategory("DataListUtil,UnitTest")]
        public void DataListUtil_UnitTest_NotReplaceStarWithInvalidIndex()
        {
            const string exp = "[[rs(*).val]]";
            var result = DataListUtil.ReplaceStarWithFixedIndex(exp, -1);

            Assert.AreEqual("[[rs(*).val]]", result, "Replaced with invalid index in recordset");
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DataListUtil_UpsertTokens_NullTarget_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_NullTokenizer_ClearsTarget()
        {
            //------------Setup for test--------------------------
            var target = new Collection<ObservablePair<string, string>>
            {
                new ObservablePair<string, string>("key1", "value1"), 
                new ObservablePair<string, string>("key2", "value2")
            };

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, null);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_AddsToTarget()
        {
            //------------Setup for test--------------------------
            const int TokenCount = 5;
            var tokenNumber = 0;

            var tokenizer = new Mock<IDev2Tokenizer>();
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber < TokenCount);
            tokenizer.Setup(t => t.NextToken()).Returns(() => string.Format("[[Var{0}]]", tokenNumber++));

            var target = new Collection<ObservablePair<string, string>>();

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(TokenCount, target.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_AddsDuplicatesToTarget()
        {
            //------------Setup for test--------------------------
            const int DuplicateCount = 2;
            const int TokenCount = 3;
            var tokenNumber = 0;
            var iterCount = 0;
            var tokenizer = new Mock<IDev2Tokenizer>();
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber < TokenCount);
            tokenizer.Setup(t => t.NextToken()).Returns(() =>
            {
                var result = string.Format("[[Var{0}]]", tokenNumber);
                if(++iterCount % DuplicateCount == 0)
                {
                    tokenNumber++;
                }
                return result;
            });

            var target = new Collection<ObservablePair<string, string>>();

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(TokenCount * DuplicateCount, target.Count);
            var groups = target.GroupBy(g => g.Key).ToList();
            Assert.AreEqual(3, groups.Count);
            foreach(var grp in groups)
            {
                var enumerator = grp.GetEnumerator();
                var duplicateCount = 0;
                while(enumerator.MoveNext())
                {
                    duplicateCount++;
                }
                Assert.AreEqual(DuplicateCount, duplicateCount);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_ClearsTargetFirst()
        {
            //------------Setup for test--------------------------
            const int TokenCount1 = 5;
            var tokenNumber1 = 0;

            var tokenizer = new Mock<IDev2Tokenizer>();
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber1 < TokenCount1);
            tokenizer.Setup(t => t.NextToken()).Returns(() => string.Format("[[Var{0}]]", tokenNumber1++));

            var target = new Collection<ObservablePair<string, string>>();

            DataListUtil.UpsertTokens(target, tokenizer.Object);

            // Create a second tokenizer that will return 2 vars with the same name as the first tokenizer and 1 new one
            const int ExpectedCount = 3;
            const int TokenCount2 = 6;
            var tokenNumber2 = 3;
            var tokenizer2 = new Mock<IDev2Tokenizer>();
            tokenizer2.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber2 < TokenCount2);
            tokenizer2.Setup(t => t.NextToken()).Returns(() => string.Format("[[Var{0}]]", tokenNumber2++));


            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer2.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedCount, target.Count);

            // Expect only vars from second tokenizer
            var keys = target.Select(p => p.Key);
            var i = 3;
            foreach(var key in keys)
            {
                var expected = string.Format("[[Var{0}]]", i++);
                Assert.AreEqual(expected, key);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_StripsWhiteSpaces()
        {
            //------------Setup for test--------------------------
            const int TokenCount1 = 5;
            var tokenNumber1 = 0;

            var tokenizer = new Mock<IDev2Tokenizer>();
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber1 < TokenCount1);
            tokenizer.Setup(t => t.NextToken()).Returns(() => string.Format("[[Var {0}]]", tokenNumber1++));

            var target = new Collection<ObservablePair<string, string>>();

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(5, target.Count);

            // Expect only vars from second tokenizer
            var keys = target.Select(p => p.Key);
            var i = 0;
            foreach(var key in keys)
            {
                var expected = string.Format("[[Var{0}]]", i++);
                Assert.AreEqual(expected, key);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_AddsPrefixToKey()
        {
            //------------Setup for test--------------------------
            const int TokenCount = 5;
            var tokenNumber = 0;

            var tokenizer = new Mock<IDev2Tokenizer>();
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber < TokenCount);
            tokenizer.Setup(t => t.NextToken()).Returns(() => string.Format("[[Var{0}]]", tokenNumber++));

            var target = new Collection<ObservablePair<string, string>>();

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer.Object, "prefix");

            //------------Assert Results-------------------------
            Assert.AreEqual(TokenCount, target.Count);

            var keys = target.Select(p => p.Key);
            var i = 0;
            foreach(var key in keys)
            {
                var expected = string.Format("[[prefixVar{0}]]", i++);
                Assert.AreEqual(expected, key);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_AddsSuffixToKey()
        {
            //------------Setup for test--------------------------
            const int TokenCount = 5;
            var tokenNumber = 0;

            var tokenizer = new Mock<IDev2Tokenizer>();
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber < TokenCount);
            tokenizer.Setup(t => t.NextToken()).Returns(() => string.Format("[[Var{0}]]", tokenNumber++));

            var target = new Collection<ObservablePair<string, string>>();

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer.Object, "prefix", "suffix");

            //------------Assert Results-------------------------
            Assert.AreEqual(TokenCount, target.Count);

            var keys = target.Select(p => p.Key);
            var i = 0;
            foreach(var key in keys)
            {
                var expected = string.Format("[[prefixVar{0}suffix]]", i++);
                Assert.AreEqual(expected, key);
            }
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_RemoveEmptyEntriesIsFalse_AddsEmptyEntriesToTarget()
        {
            //------------Setup for test--------------------------
            const int TokenCount = 5;
            var tokenNumber = 0;

            var tokens = new List<string> { "f1", "", "f3", "f4", "" };

            var tokenizer = new Mock<IDev2Tokenizer>();
            // ReSharper disable ImplicitlyCapturedClosure
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber < TokenCount);
            // ReSharper restore ImplicitlyCapturedClosure
            tokenizer.Setup(t => t.NextToken()).Returns(() => tokens[tokenNumber++]);

            var target = new Collection<ObservablePair<string, string>>();

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer.Object, "rs(*).", "a", false);

            //------------Assert Results-------------------------
            Assert.AreEqual(TokenCount, target.Count);

            for(var i = 0; i < TokenCount; i++)
            {
                if(i == 1 || i == 4)
                {
                    Assert.AreEqual(string.Empty, target[i].Key);
                }
                else
                {
                    Assert.AreEqual(string.Format("[[rs(*).{0}a]]", tokens[i]), target[i].Key);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataListUtil_UpsertTokens")]
        public void DataListUtil_UpsertTokens_ValidTokenizer_RemoveEmptyEntriesIsTrue_RemovesEmptyEntriesFromTarget()
        {
            //------------Setup for test--------------------------
            const int TokenCount = 5;
            var tokenNumber = 0;

            var tokens = new List<string> { "f1", "", "f2", "f3", "" };

            var tokenizer = new Mock<IDev2Tokenizer>();
            // ReSharper disable ImplicitlyCapturedClosure
            tokenizer.Setup(t => t.HasMoreOps()).Returns(() => tokenNumber < TokenCount);
            // ReSharper restore ImplicitlyCapturedClosure
            tokenizer.Setup(t => t.NextToken()).Returns(() => tokens[tokenNumber++]);

            var target = new Collection<ObservablePair<string, string>>();

            //------------Execute Test---------------------------
            DataListUtil.UpsertTokens(target, tokenizer.Object, "rs(*).", "a");

            //------------Assert Results-------------------------
            const int ExpectedCount = TokenCount - 2;

            Assert.AreEqual(ExpectedCount, target.Count);

            for(var i = 0; i < ExpectedCount; i++)
            {
                Assert.AreEqual(string.Format("[[rs(*).f{0}a]]", i + 1), target[i].Key);
            }
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListUtil_EndsWithClosingTags")]
        public void DataListUtil_EndsWithClosingTags_VariableHasNoClosingTags_False()
        {
            //------------Execute Test---------------------------
            var isClosed = DataListUtil.EndsWithClosingTags("[[var");
            //------------Assert Results-------------------------
            Assert.IsFalse(isClosed);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListUtil_EndsWithClosingTags")]
        public void DataListUtil_EndsWithClosingTags_VariableIsEmpty_False()
        {
            //------------Execute Test---------------------------
            var isClosed = DataListUtil.EndsWithClosingTags("");
            //------------Assert Results-------------------------
            Assert.IsFalse(isClosed);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListUtil_EndsWithClosingTags")]
        public void DataListUtil_EndsWithClosingTags_VariableHasClosingTags_True()
        {
            //------------Execute Test---------------------------
            var isClosed = DataListUtil.EndsWithClosingTags("[[var]]");
            //------------Assert Results-------------------------
            Assert.IsTrue(isClosed);
        }





        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListUtil_IndexOfClosingTags")]
        public void DataListUtil_IndexOfClosingTags_VariableHasNoClosingTags_NegativeOne()
        {
            //------------Execute Test---------------------------
            var index = DataListUtil.IndexOfClosingTags("[[var");
            //------------Assert Results-------------------------
            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListUtil_IndexOfClosingTags")]
        public void DataListUtil_IndexOfClosingTags_VariableIsEmpty_NegativeOne()
        {
            //------------Execute Test---------------------------
            var index = DataListUtil.IndexOfClosingTags("");
            //------------Assert Results-------------------------
            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListUtil_IndexOfClosingTags")]
        public void DataListUtil_IndexOfClosingTags_VariableHasClosingTags_Index()
        {
            //------------Execute Test---------------------------
            var index = DataListUtil.IndexOfClosingTags("[[var]]");
            //------------Assert Results-------------------------
            Assert.AreEqual(5, index);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListUtil_IsRecordsetOpeningBrace")]
        public void DataListUtil_IsRecordsetOpeningBrace_InputDoesnotStartsRecordsetOpeningBrace_False()
        {
            //------------Execute Test---------------------------
            var isClosed = DataListUtil.IsRecordsetOpeningBrace("[[rec([[var");
            //------------Assert Results-------------------------
            Assert.IsFalse(isClosed);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListUtil_IsRecordsetOpeningBrace")]
        public void DataListUtil_IsRecordsetOpeningBrace_VariableIsEmpty_False()
        {
            //------------Execute Test---------------------------
            var isOpeningBrace = DataListUtil.IsRecordsetOpeningBrace("");
            //------------Assert Results-------------------------
            Assert.IsFalse(isOpeningBrace);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataListUtil_IsRecordsetOpeningBrace")]
        public void DataListUtil_IsRecordsetOpeningBrace_InputStartsRecordsetOpeningBrace_True()
        {
            //------------Execute Test---------------------------
            var isOpeningBrace = DataListUtil.IsRecordsetOpeningBrace("([[var");
            //------------Assert Results-------------------------
            Assert.IsTrue(isOpeningBrace);
        }

    }
}
