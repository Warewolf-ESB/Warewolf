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
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenNoErrors_ShouldConstructsCorreclty()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var newDev2DataLanguageParser = new Dev2DataLanguageParser();
            //---------------Test Result -----------------------
            Assert.IsNotNull(newDev2DataLanguageParser, "Cannot create new Dev2DataLanguageParser object.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ParseExpressionIntoParts_GivenExpression_ShouldReturnParts()
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
        [Owner("Nkosinathi Sangweni")]
        public void ParseExpressionIntoParts_GivenEmptyExpression_ShouldReturnEmptyParts()
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
        [Owner("Nkosinathi Sangweni")]
        public void ParseExpressionIntoParts_GivenExpressionInCache_ShouldReturnFromCache()
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
        [Owner("Nkosinathi Sangweni")]
        public void ParseDataLanguageForIntellisense_GivenValidArgs_ShouldExecutesCorreclty()
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
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ParseDataLanguageForIntellisense_GivenEmpty_ShouldExecutesCorreclty()
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
        [Owner("Nkosinathi Sangweni")]
        public void ParseDataLanguageForIntellisense_GivenRecordSetsFilter_ShouldExecutesCorreclty()
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
        [Owner("Nkosinathi Sangweni")]
        public void ParseDataLanguageForIntellisense_GivenFuncThrowsException_ShouldCatchAndLogException()
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
        [Owner("Nkosinathi Sangweni")]
        public void ParseForActivityDataItems_GivenPayLoad_ShouldReturnCorreclty()
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
        [Owner("Nkosinathi Sangweni")]
        public void ParseForActivityDataItems_GivenPayLoadWithTwoVariables_ShouldReturnCorreclty()
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
        [Owner("Nkosinathi Sangweni")]
        public void ParseForActivityDataItems_GivenEmptyPayLoad_ShouldReturnCorreclty()
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
        [Owner("Nkosinathi Sangweni")]
        public void CheckValidIndex_GivenNegetiveIndex_ShouldThrow_InvalidCharacterError()
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
        [Owner("Nkosinathi Sangweni")]
        public void CheckValidIndex_GivenNegetiveIndex_ShouldThrow_TooHighError()
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
        [Owner("Nkosinathi Sangweni")]
        public void CheckValidIndex_ValidIndex_ShouldMarkAsValid()
        {
            var parser = new ParserHelperUtil();
            var valid = parser.CheckValidIndex(new ParseTO(), "1", 1, 2);
            Assert.IsTrue(valid, "Valid recordset index marked as invalid.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CheckCurrentIndex_GivenInvalid_ShouldThrow_InvalidCharException()
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
        [Owner("Nkosinathi Sangweni")]
        public void CheckCurrentIndex_GivenInvalid_ShouldThrow_LessThanZeroException()
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
        [Owner("Nkosinathi Sangweni")]
        public void CheckCurrentIndex_GivenPositiveIndex_ShouldMarkAsValid()
        {
            var parser = new ParserHelperUtil();
            var invoke = parser.CheckCurrentIndex(new ParseTO(), 3, "rec(1)", "rec(1)".Length);
            Assert.IsTrue(invoke, "Positive recordset index marked as invalid.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsValidIndex_GivenInvalidIndex_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            var privateObject = new PrivateObject(parser);
            var parseTO = new ParseTO { Payload = "rec(-1)", EndIndex = "rec(-1)".Length, StartIndex = 3 };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                //---------------Test Result -----------------------
                privateObject.Invoke("IsValidIndex", parseTO);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Recordset index -1 is not greater than zero", ex.InnerException.Message);
                try
                {
                    parseTO = new ParseTO { Payload = "rec(-1", EndIndex = "rec(-1".Length, StartIndex = 3 };
                    //---------------Test Result -----------------------
                    privateObject.Invoke("IsValidIndex", parseTO);
                }
                catch (Exception ex1)
                {
                    Assert.AreEqual("Recordset index [ -1 ] is not greater than zero", ex1.InnerException.Message);
                    try
                    {
                        parseTO = new ParseTO { Payload = "rec(1", EndIndex = "rec(1".Length, StartIndex = 3 };
                        //---------------Test Result -----------------------
                        privateObject.Invoke("IsValidIndex", parseTO);
                    }
                    catch (Exception ex2)
                    {
                        Assert.AreEqual("Recordset [ rec(1 ] does not contain a matching ')'", ex2.InnerException.Message);
                    }
                }
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddErrorToResults_GivenErrors_ShouldAddErrors()
        {
            //---------------Set up test pack-------------------
            //AddErrorToResults(bool isRs, string part, Dev2DataLanguageParseError e, bool isOpen)
            var parser = new Dev2DataLanguageParser();
            var privateObject = new PrivateObject(parser);
            var error = new Dev2DataLanguageParseError("Error", 1, 5, enIntellisenseErrorCode.SyntaxError);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            if (privateObject.Invoke("AddErrorToResults", true, "rec().Name", error, false) is IntellisenseResult invoke)
            {
                Assert.AreEqual(5, invoke.EndIndex);
                Assert.AreEqual(enIntellisenseErrorCode.SyntaxError, invoke.ErrorCode);
                Assert.AreEqual("Error", invoke.Message);
                Assert.AreEqual("[[rec()]]", invoke.Option.DisplayValue);
                Assert.AreEqual("rec", invoke.Option.Recordset);
                Assert.AreEqual("Error", invoke.Type.ToString());
            }

            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddErrorToResults_GivenValidationErrors_ShouldAddErrors()
        {
            //---------------Set up test pack-------------------
            //AddErrorToResults(bool isRs, string part, Dev2DataLanguageParseError e, bool isOpen)
            var parser = new Dev2DataLanguageParser();
            var privateObject = new PrivateObject(parser);
            var error = new Dev2DataLanguageParseError("Error", 1, 5, enIntellisenseErrorCode.SyntaxError);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            if (privateObject.Invoke("AddErrorToResults", false, "rec().Name", error, false) is IntellisenseResult invoke)
            {
                //---------------Test Result -----------------------
                Assert.AreEqual(5, invoke.EndIndex);
                Assert.AreEqual(enIntellisenseErrorCode.SyntaxError, invoke.ErrorCode);
                Assert.AreEqual("Error", invoke.Message);
                Assert.AreEqual("[[rec().Name]]", invoke.Option.DisplayValue);
                Assert.AreEqual("", invoke.Option.Recordset);
                Assert.AreEqual("Error", invoke.Type.ToString());
            }

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ProcessForOnlyOpenRegion_GivenValidationArgs_ShouldProcessCorrectly()
        {
            //---------------Set up test pack-------------------
            //ProcessForOnlyOpenRegion(ParseTO payload, IEnumerable<IDev2DataLanguageIntellisensePart> refParts, IList<IIntellisenseResult> result)
            var parser = new Dev2DataLanguageParser();
            var privateObject = new PrivateObject(parser);
            var refParts = new List<IDev2DataLanguageIntellisensePart> { new Dev2DataLanguageIntellisensePart("Rec().Name", "rec", null) };
            var result = new List<IIntellisenseResult>();
            var parseTO = new ParseTO
            {
                Parent = new ParseTO { Payload = "rec().Name" }
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            bool invoke;
            try
            {
                privateObject.Invoke("ProcessForOnlyOpenRegion", parseTO, refParts, result);
                invoke = true;
            }
            catch (Exception)
            {
                invoke = false;
            }
            //---------------Test Result -----------------------
            Assert.IsTrue(invoke);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ProcessForOnlyOpenRegion_GivenValidationArgsAndHasChildrenAndISRecordSet_ShouldProcessCorrectly()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            var privateObject = new PrivateObject(parser);
            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            var children = new List<IDev2DataLanguageIntellisensePart> { new Dev2DataLanguageIntellisensePart("name", "name", new List<IDev2DataLanguageIntellisensePart>()) };
            refParts.Add(new Dev2DataLanguageIntellisensePart("Rec().Name", "rec", children));
            var result = new List<IIntellisenseResult>();
            var parseTO = new ParseTO
            {
                Parent = new ParseTO { Payload = "Name" },

            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            bool invoke;
            try
            {
                privateObject.Invoke("ProcessForOnlyOpenRegion", parseTO, refParts, result);
                invoke = true;
            }
            catch (Exception)
            {
                invoke = false;
            }
            //---------------Test Result -----------------------
            Assert.IsTrue(invoke, "Cannot invoke new Dev2DataLanguageParser PrivateObject.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ProcessRecordSetFields_GivenInvalidArgs_ShouldThrowValidException()
        {
            //---------------Set up test pack-------------------
            //ProcessRecordSetFields(ParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
            var parser = new Dev2DataLanguageParser();
            var parseTO = new ParseTO
            {
                Parent = new ParseTO { Payload = "Name" },

            };
            var languageIntellisensePart = new Dev2DataLanguageIntellisensePart("Name", "Desc", new List<IDev2DataLanguageIntellisensePart>());
            var results = new List<IIntellisenseResult>();
            var privateObject = new PrivateObject(parser);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                privateObject.Invoke("ProcessRecordSetFields", parseTO, true, results, languageIntellisensePart);

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ProcessRecordSetFields_GivenAddCompletePartsFalse_ShouldThrowValidException()
        {
            //---------------Set up test pack-------------------
            //ProcessRecordSetFields(ParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
            var parser = new Dev2DataLanguageParser();
            var parseTO = new ParseTO
            {
                Parent = new ParseTO { Payload = "Name" },

            };
            var languageIntellisensePart = new Dev2DataLanguageIntellisensePart("Name", "Desc", new List<IDev2DataLanguageIntellisensePart>());
            var results = new List<IIntellisenseResult>();
            var privateObject = new PrivateObject(parser);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                privateObject.Invoke("ProcessRecordSetFields", parseTO, false, results, languageIntellisensePart);

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExtractActualIntellisenseOptions_GivenValidArgs_ShouldExecuteCorrectly()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            var parseTO = new ParseTO
            {
                Parent = new ParseTO { Payload = "Name" },

            };

            var parseTORecSet = new ParseTO
            {
                Parent = new ParseTO { Payload = "rec().Name" },
                Payload = "rec().Name"
            };
            var intellisenseParts = new List<IDev2DataLanguageIntellisensePart>();
            var privateObject = new PrivateObject(parser);
            //ExtractActualIntellisenseOptions(ParseTO payload, IEnumerable<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts)
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                privateObject.Invoke("ExtractActualIntellisenseOptions", parseTO, intellisenseParts, true);
                privateObject.Invoke("ExtractActualIntellisenseOptions", parseTORecSet, intellisenseParts, false);
            }
            catch (Exception e)
            {
                //---------------Test Result -----------------------
                Assert.Fail(e.Message);
            }

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RecordsetMatch_GivenValidArgs_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            //RecordsetMatch(IList<IIntellisenseResult> result, string rawSearch, string search)
            var parser = new Dev2DataLanguageParser();
            var intellisenseResults = new List<IIntellisenseResult> { IntellisenseFactory.CreateErrorResult(1, 2, new Mock<IDataListVerifyPart>().Object, "msg", enIntellisenseErrorCode.None, false) };
            var privateObject = new PrivateObject(parser);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                privateObject.Invoke("RecordsetMatch", intellisenseResults, "rawSearch", "seach");
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void Dev2DataLanguageParser_ParseWithCData()
        {
            //---------------Set up test pack-------------------
            //RecordsetMatch(IList<IIntellisenseResult> result, string rawSearch, string search)
            var parser = new Dev2DataLanguageParser();
            const string text = @"<![CDATA[[[]]]]>";
            var parts = parser.ParseExpressionIntoParts(text, new List<IDev2DataLanguageIntellisensePart>());
            Assert.AreEqual(parts.Count, 1);
            Assert.AreEqual(parts[0].Type, enIntellisenseResultType.Error);
            Assert.AreEqual(parts[0].ErrorCode, enIntellisenseErrorCode.SyntaxError);
            Assert.AreEqual(parts[0].Message, ErrorResource.VariableIsMissing);

            const string textNoBrackets = @"Some text[[";
            parts = parser.ParseExpressionIntoParts(textNoBrackets, new List<IDev2DataLanguageIntellisensePart>());
            Assert.AreEqual(parts.Count, 1);
            Assert.AreEqual(parts[0].Type, enIntellisenseResultType.Error);
            Assert.AreEqual(parts[0].ErrorCode, enIntellisenseErrorCode.SyntaxError);
            Assert.AreEqual(parts[0].Message, ErrorResource.InvalidCloseRegion);

            const string textWithAuto = @"[[varName]]";
            parts = parser.ParseExpressionIntoParts(textWithAuto, new List<IDev2DataLanguageIntellisensePart>());
            Assert.AreEqual(parts.Count, 1);
            Assert.AreEqual(enIntellisenseResultType.Error, parts[0].Type);
            Assert.AreEqual(enIntellisenseErrorCode.ScalarNotFound, parts[0].ErrorCode);
            Assert.AreEqual(" [[varName]] does not exist in your variable list", parts[0].Message);

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_ErrorOnUnclosed()
        {
            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName([[varName)]]", null, true, null, true);
            Assert.AreEqual(enIntellisenseErrorCode.SyntaxError, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Error, result[0].Type);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
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
        [Owner("Rory McGuire")]
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
        [Owner("Rory McGuire")]
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
        [Owner("Rory McGuire")]
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
        [Owner("Rory McGuire")]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_OneMatchWithParent()
        {
            const string dataList = @"<doc><recName Description=""RecName Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName([[recName([[index).field]]", dataList, false, null, true);

            // TODO: surely this should show an error indicating that the field was not found in the recordset?
            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual("RecName Description", result[0].Message);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_OneMatchNoParent()
        {
            const string dataList = @"<doc><recName Description=""RecName Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName([[index).field]]", dataList, false, null, true);

            // TODO: surely this should show an error indicating that the field was not found in the recordset?
            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual("RecName Description", result[0].Message);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_OneMatchNoParent2()
        {
            const string dataList = @"<doc><recName Description=""RecName Description""></recName></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName([[recName).field]]", dataList, false, null, true);

            // TODO: surely this should show an error indicating that the field was not found in the recordset?
            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type);
            Assert.AreEqual("RecName Description", result[0].Message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
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
        [Owner("Rory McGuire")]
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
        [Owner("Rory McGuire")]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_NoFieldMatches()
        {
            const string dataList = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";

            var parser = new Dev2DataLanguageParser();
            var result = parser.ParseDataLanguageForIntellisense("[[recName1.Name", dataList, false, null, true);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
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
        [Owner("Rory McGuire")]
        public void Dev2DataLanguageParser_ParseExpressionIntoParts()
        {
            //---------------Set up test pack-------------------
            //RecordsetMatch(IList<IIntellisenseResult> result, string rawSearch, string search)
            var parser = new Dev2DataLanguageParser();
            const string nullText = null;

            var emptyList = parser.MakeParts(nullText, true);
            Assert.AreEqual(emptyList.Count, 0);

            var parts = parser.ParseForActivityDataItems(nullText);
            Assert.AreEqual(parts.Count, 0);

            const string unclosed = "[[varName";
            parts = parser.ParseForActivityDataItems(unclosed);
            Assert.AreEqual(parts[0], "varName");

            Assert.ThrowsException<Dev2DataLanguageParseError>(() => parser.MakeParts(unclosed, true));
        }
    }
}
