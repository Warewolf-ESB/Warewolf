/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dev2.Common.Interfaces;
using Dev2.Data.Exceptions;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Parsers;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Data.Util;
using Warewolf.Resource.Errors;

namespace Dev2.Data.Tests.Parsers
{
    [TestClass]
    public class Dev2DataLanguageParserTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_OnCreation_GivenNoErrors_ShouldConstructsCorreclty()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var newDev2DataLanguageParser = new Dev2DataLanguageParser();
            //---------------Test Result -----------------------
            Assert.IsNotNull(newDev2DataLanguageParser, "Cannot create new Dev2DataLanguageParser object.");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseExpressionIntoParts_GivenExpression_ShouldReturnParts()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var expressionIntoParts = parser.ParseExpressionIntoParts("[[x]]", new List<IDev2DataLanguageIntellisensePart>());
            //---------------Test Result -----------------------
            Assert.AreEqual(1, expressionIntoParts.Count);
            var error = expressionIntoParts.SingleOrDefault(result => !string.IsNullOrEmpty(result.Message));
            Assert.IsNotNull(error);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseExpressionIntoParts_GivenEmptyExpression_ShouldReturnEmptyParts()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var expressionIntoParts = parser.ParseExpressionIntoParts("", new List<IDev2DataLanguageIntellisensePart>());
            //---------------Test Result -----------------------
            Assert.AreEqual(0, expressionIntoParts.Count);
            var error = expressionIntoParts.SingleOrDefault(result => !string.IsNullOrEmpty(result.Message));
            Assert.IsNull(error);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseExpressionIntoParts_GivenExpressionInCache_ShouldReturnFromCache()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            var concurrentDictionary = new ConcurrentDictionary<string, IList<IIntellisenseResult>>();
            var tryAdd = concurrentDictionary.TryAdd("[[a]]", new List<IIntellisenseResult> { IntellisenseFactory.CreateErrorResult(1, 2, null, "", enIntellisenseErrorCode.FieldNotFound, false) });

            Assert.IsTrue(tryAdd);
            var fieldInfo = typeof(Dev2DataLanguageParser).GetField("_expressionCache", BindingFlags.Static | BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(parser, concurrentDictionary);
            }
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var expressionIntoParts = parser.ParseExpressionIntoParts("[[a]]", new List<IDev2DataLanguageIntellisensePart>());
            //---------------Test Result -----------------------
            Assert.AreEqual(1, expressionIntoParts.Count);
            var error = expressionIntoParts.SingleOrDefault(result => !string.IsNullOrEmpty(result.Message));
            Assert.IsNull(error);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ForIntellisense_GivenValidArgs_ShouldExecutesCorreclty()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var expressionIntoParts = parser.ParseDataLanguageForIntellisense("[[a]]", datalist);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, expressionIntoParts.Count);
            Assert.AreEqual(enIntellisenseErrorCode.None, expressionIntoParts[0].ErrorCode);
            Assert.AreEqual("", expressionIntoParts[0].Message);
            Assert.AreEqual("[[var]]", expressionIntoParts[0].Option.DisplayValue);
            Assert.AreEqual("var", expressionIntoParts[0].Option.Field);
            Assert.AreEqual("", expressionIntoParts[0].Option.Recordset);
            Assert.AreEqual("", expressionIntoParts[0].Option.RecordsetIndex);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ForIntellisense_GivenInvalidDatalist_ShouldSwallowException()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            var datalist = "Invalid Datalist!!!";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var expressionIntoParts = parser.ParseDataLanguageForIntellisense("some value", datalist);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, expressionIntoParts.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ForIntellisense_GivenEmpty_ShouldExecutesCorreclty()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var expressionIntoParts = parser.ParseDataLanguageForIntellisense("", datalist);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, expressionIntoParts.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ForIntellisense_GivenRecordSetsFilter_ShouldExecutesCorreclty()
        {
            var parser = new Dev2DataLanguageParser();
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);

            var obj = new object();
            lock (obj)
            {
                var expressionIntoParts = parser.ParseDataLanguageForIntellisense("[[a]]", datalist, false, new IntellisenseFilterOpsTO { FilterType = enIntellisensePartType.RecordsetsOnly });
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ForIntellisense_GivenFuncThrowsException_ShouldCatchAndLogException()
        {
            var parser = new Dev2DataLanguageParser();
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);
            var mock = new Mock<IIntellisenseFilterOpsTO>();
            mock.SetupGet(to => to.FilterType).Throws(new Exception("Error"));
            try
            {
                parser.ParseDataLanguageForIntellisense("[[a]]", datalist, false, mock.Object);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Error", ex.Message);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseForActivityDataItems_GivenPayLoad_ShouldReturnCorreclty()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            const string datalist = "[[a]]";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var forActivityDataItems = parser.ParseForActivityDataItems(datalist);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, forActivityDataItems.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseForActivityDataItems_GivenPayLoadWithTwoVariables_ShouldReturnCorreclty()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            const string datalist = "[[a]] and [[b]]";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var forActivityDataItems = parser.ParseForActivityDataItems(datalist);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, forActivityDataItems.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseForActivityDataItems_GivenEmptyPayLoad_ShouldReturnCorreclty()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var forActivityDataItems = parser.ParseForActivityDataItems("");
            //---------------Test Result -----------------------
            Assert.AreEqual(0, forActivityDataItems.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_CheckValidIndex_GivenNegetiveIndex_ShouldThrow_InvalidCharacterError()
        {
            var parser = new ParserHelperUtil();
            try
            {
                parser.CheckValidIndex(new ParseTO(), "a", 1, 2);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Recordset index (a) contains invalid character(s)", ex.Message);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_CheckValidIndex_GivenNegetiveIndex_ShouldThrow_TooHighError()
        {
            var parser = new ParserHelperUtil();
            try
            {
                parser.CheckValidIndex(new ParseTO(), "-1", 1, 2);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Recordset index [ -1 ] is not greater than zero", ex.Message);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_CheckValidIndex_ValidIndex_ShouldMarkAsValid()
        {
            var parser = new ParserHelperUtil();
            var valid = parser.CheckValidIndex(new ParseTO(), "1", 1, 2);
            Assert.IsTrue(valid, "Valid recordset index marked as invalid.");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_CheckCurrentIndex_GivenInvalid_ShouldThrow_InvalidCharException()
        {
            var parser = new ParserHelperUtil();
            try
            {
                parser.CheckCurrentIndex(new ParseTO(), 0, "rec(a).name", "a".Length);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Recordset index (ec(a).nam) contains invalid character(s)", ex.Message);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_CheckCurrentIndex_GivenInvalid_ShouldThrow_LessThanZeroException()
        {
            var parser = new ParserHelperUtil();
            try
            {
                parser.CheckCurrentIndex(new ParseTO(), 3, "rec(-1)", "rec(-1)".Length);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Recordset index -1 is not greater than zero", ex.Message);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_CheckCurrentIndex_GivenPositiveIndex_ShouldMarkAsValid()
        {
            var parser = new ParserHelperUtil();
            var invoke = parser.CheckCurrentIndex(new ParseTO(), 3, "rec(1)", "rec(1)".Length);
            Assert.IsTrue(invoke, "Positive recordset index marked as invalid.");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseWithCData()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            const string text = @"<![CDATA[[[]]]]>";
            var parts = parser.ParseExpressionIntoParts(text, new List<IDev2DataLanguageIntellisensePart>());
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual(parts[0].Type, enIntellisenseResultType.Error);
            Assert.AreEqual(parts[0].ErrorCode, enIntellisenseErrorCode.SyntaxError);
            Assert.AreEqual(parts[0].Message, ErrorResource.VariableIsMissing);

            const string textNoBrackets = @"Some text[[";
            parts = parser.ParseExpressionIntoParts(textNoBrackets, new List<IDev2DataLanguageIntellisensePart>());
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual(parts[0].Type, enIntellisenseResultType.Error);
            Assert.AreEqual(parts[0].ErrorCode, enIntellisenseErrorCode.SyntaxError);
            Assert.AreEqual(parts[0].Message, ErrorResource.InvalidCloseRegion);

            const string textWithAuto = @"[[varName]]";
            parts = parser.ParseExpressionIntoParts(textWithAuto, new List<IDev2DataLanguageIntellisensePart>());
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual(enIntellisenseResultType.Error, parts[0].Type);
            Assert.AreEqual(enIntellisenseErrorCode.ScalarNotFound, parts[0].ErrorCode);
            Assert.AreEqual(" [[varName]] does not exist in your variable list", parts[0].Message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_ErrorOnUnclosed()
        {
            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName([[varName)]]", null, true, null, true);
            Assert.AreEqual(enIntellisenseErrorCode.SyntaxError, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Error, result[0].Type);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_EmptyHangingOpen()
        {
            const string dataList = @"<doc><recName Description=""RecName Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[", dataList, false, null, false);
            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual("RecName Description / Select this variable", result[0].Message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_EmptyClosed()
        {
            const string dataList = @"<doc><recName Description=""RecName Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[[[]]]]", dataList, false, null, false);
            Assert.AreEqual(enIntellisenseErrorCode.SyntaxError, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Error, result[0].Type);
            Assert.AreEqual("Variable [[]] is missing a name", result[0].Message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_OneMatch()
        {
            const string dataList = @"<doc><recName Description=""RecName Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recNam]]", dataList, false, null, true);
            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual("RecName Description", result[0].Message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_NoFieldMatchInValidRecordset()
        {
            const string dataList = @"<doc><recName Description=""RecName Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName().field]]", dataList, false, null, true);

            // Note: studio does not allow recordsets with no fields
            Assert.AreEqual(enIntellisenseErrorCode.NeitherRecordsetNorFieldFound, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Error, result[0].Type);
            Assert.AreEqual("[[recName()]] does not exist in your variable list", result[0].Message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_OneMatchWithParent()
        {
            const string dataList = @"<doc><recName Description=""RecName Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName([[recName([[index).field]]", dataList, false, null, true);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual("RecName Description", result[0].Message);
            Assert.AreEqual("[[recName]]", result[0].Option.DisplayValue);
            Assert.AreEqual("recName", result[0].Option.Field);
            Assert.AreEqual("", result[0].Option.Recordset);
            Assert.AreEqual("", result[0].Option.RecordsetIndex);

            Assert.AreEqual(enIntellisenseErrorCode.SyntaxError, result[1].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Error, result[1].Type);
            Assert.AreEqual("Variable name [[index).field]] contains invalid character(s). Only use alphanumeric _ and - ", result[1].Message);
            Assert.AreEqual("[[index).field()]]", result[1].Option.DisplayValue);
            Assert.AreEqual("", result[1].Option.Field);
            Assert.AreEqual("index).field", result[1].Option.Recordset);
            Assert.AreEqual("", result[1].Option.RecordsetIndex);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_OneMatchNoParent()
        {
            const string dataList = @"<doc><recName Description=""RecName Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName([[index).field]]", dataList, false, null, true);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual("RecName Description", result[0].Message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_OneMatchNoParent2()
        {
            const string dataList = @"<doc><recName Description=""RecName Description""></recName></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName([[recName).field]]", dataList, false, null, true);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual("RecName Description", result[0].Message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_TwoMatchingFields()
        {
            const string dataList = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName1", dataList, false, null, true);
            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual("RecName1 Description", result[0].Message);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[1].Type);
            Assert.AreEqual("field1 Desc", result[1].Message);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[2].Type);
            Assert.AreEqual("field1 Desc", result[2].Message);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[3].Type);
            Assert.AreEqual("field2 Desc", result[3].Message);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[4].Type);
            Assert.AreEqual("field2 Desc", result[4].Message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_TwoMatches()
        {
            const string dataList = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName1.field", dataList, false, null, true);
            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual("field1 Desc", result[0].Message);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[2].Type);
            Assert.AreEqual("field2 Desc", result[2].Message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_NoFieldMatches()
        {
            const string dataList = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName1.Name", dataList, false, null, true);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_BlankDescriptions()
        {
            const string dataList = @"<doc><recName1 Description=""""><field1 Description="""" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName1.field", dataList, false, null, true);
            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual(" Input: Use last row, Result: Append new record", result[0].Message);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[2].Type);
            Assert.AreEqual("field2 Desc", result[2].Message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseExpressionIntoParts()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            const string nullText = null;

            var emptyList = parser.MakeParts(nullText, true);
            Assert.AreEqual(0, emptyList.Count);

            var parts = parser.ParseForActivityDataItems(nullText);
            Assert.AreEqual(0, parts.Count);

            const string unclosed = "[[varName";
            parts = parser.ParseForActivityDataItems(unclosed);
            Assert.AreEqual("varName", parts[0]);

            Assert.ThrowsException<Dev2DataLanguageParseError>(() => parser.MakeParts(unclosed, true));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_TwoMatchingFields_AddCompleteParts()
        {
            const string dataList = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName1", dataList, false);
            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual("RecName1 Description", result[0].Message);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2DataLanguageParser))]
        public void Dev2DataLanguageParser_ValidateName()
        {
            //---------------Set up test pack-------------------
            const string dataList = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName1.field", dataList, false, null, true);
            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual("field1 Desc", result[0].Message);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[2].Type);
            Assert.AreEqual("field2 Desc", result[2].Message);

            var name = parser.ValidateName("field1", "Scalar");

            Assert.IsNull(name);

            name = parser.ValidateName("[[recName1]]", "Recordset");

            Assert.IsNotNull(name);
        }
    }
}
