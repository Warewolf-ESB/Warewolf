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
            var tryAdd = concurrentDictionary.TryAdd("[[a]]", new List<IIntellisenseResult>() { IntellisenseFactory.CreateErrorResult(1, 2, null, "", enIntellisenseErrorCode.FieldNotFound, false) });

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
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            object obj = new object();
            lock (obj)
            {
                var expressionIntoParts = parser.ParseDataLanguageForIntellisense("[[a]]", datalist, false, new IntellisenseFilterOpsTO() { FilterType = enIntellisensePartType.RecordsetsOnly });
                //---------------Test Result -----------------------
            }

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ParseDataLanguageForIntellisense_GivenFuncThrowsException_ShouldCatchAndLogException()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);
            var mock = new Mock<IIntellisenseFilterOpsTO>();
            mock.SetupGet(to => to.FilterType).Throws(new Exception("Error"));
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {

                parser.ParseDataLanguageForIntellisense("[[a]]", datalist, false, mock.Object);
                //---------------Test Result -----------------------
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
        public void CheckValidIndex_GivenNwegetiveIndex_ShouldThrowError()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            PrivateObject privateObject = new PrivateObject(parser);
            //CheckValidIndex(ParseTO to, string part, int start, int end)
            var parseTO = new ParseTO() { };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                privateObject.Invoke("CheckValidIndex", parseTO, "a", 1, 2);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Recordset index (a) contains invalid character(s)", ex.InnerException.Message);
                try
                {
                    privateObject.Invoke("CheckValidIndex", parseTO, "-1", 1, 2);
                }
                catch (Exception ex1)
                {
                    Assert.AreEqual("Recordset index [ -1 ] is not greater than zero", ex1.InnerException.Message);
                    try
                    {
                        //---------------Test Result -----------------------
                        var valid = privateObject.Invoke("CheckValidIndex", parseTO, "1", 1, 2);
                        Assert.IsTrue(bool.Parse(valid.ToString()));
                    }
                    catch (Exception ex2)
                    {
                        Assert.Fail(ex2.Message);
                    }

                }
            }


        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CheckCurrentIndex_GivenInvalid_ShouldThrowValidException()
        {
            //---------------Set up test pack-------------------
            var parser = new Dev2DataLanguageParser();
            PrivateObject privateObject = new PrivateObject(parser);
            //  CheckCurrentIndex(ParseTO to, int start, string raw, int end)
            var parseTO = new ParseTO() { };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                privateObject.Invoke("CheckCurrentIndex", parseTO, 0, "rec(a).name", "a".Length);
            }
            catch (Exception ex)
            {
                //---------------Test Result -----------------------
                Assert.AreEqual("Recordset index (ec(a).nam) contains invalid character(s)", ex.InnerException.Message);
                try
                {
                    privateObject.Invoke("CheckCurrentIndex", parseTO, 3, "rec(-1)", "rec(-1)".Length);
                }
                catch (Exception ex1)
                {
                    Assert.AreEqual("Recordset index -1 is not greater than zero", ex1.InnerException.Message);
                    var invoke = privateObject.Invoke("CheckCurrentIndex", parseTO, 3, "rec(1)", "rec(1)".Length);
                    Assert.IsTrue(bool.Parse(invoke.ToString()));
                }
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsValidIndex_GivenInvalidIndex_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            //IsValidIndex(ParseTO to)
            var parser = new Dev2DataLanguageParser();
            PrivateObject privateObject = new PrivateObject(parser);
            //  CheckCurrentIndex(ParseTO to, int start, string raw, int end)
            var parseTO = new ParseTO() { Payload = "rec(-1)", EndIndex = "rec(-1)".Length, StartIndex = 3 };
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
                    parseTO = new ParseTO() { Payload = "rec(-1", EndIndex = "rec(-1".Length, StartIndex = 3 };
                    //---------------Test Result -----------------------
                    privateObject.Invoke("IsValidIndex", parseTO);
                }
                catch (Exception ex1)
                {
                    Assert.AreEqual("Recordset index [ -1 ] is not greater than zero", ex1.InnerException.Message);
                    try
                    {
                        parseTO = new ParseTO() { Payload = "rec(1", EndIndex = "rec(1".Length, StartIndex = 3 };
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
            PrivateObject privateObject = new PrivateObject(parser);
            Dev2DataLanguageParseError error = new Dev2DataLanguageParseError("Error", 1, 5, enIntellisenseErrorCode.SyntaxError);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var invoke = privateObject.Invoke("AddErrorToResults", true, "rec().Name", error, false) as IntellisenseResult;
            if (invoke != null)
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
            PrivateObject privateObject = new PrivateObject(parser);
            Dev2DataLanguageParseError error = new Dev2DataLanguageParseError("Error", 1, 5, enIntellisenseErrorCode.SyntaxError);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var invoke = privateObject.Invoke("AddErrorToResults", false, "rec().Name", error, false) as IntellisenseResult;
            if (invoke != null)
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
            var parseTO = new ParseTO()
            {
                Parent = new ParseTO() { Payload = "rec().Name" }
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
            Assert.IsTrue(invoke);//, "Cannot invoke new Dev2DataLanguageParser PrivateObject.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ProcessForOnlyOpenRegion_GivenValidationArgsAndHasChildrenAndISRecordSet_ShouldProcessCorrectly()
        {
            //---------------Set up test pack-------------------
            //ProcessForOnlyOpenRegion(ParseTO payload, IEnumerable<IDev2DataLanguageIntellisensePart> refParts, IList<IIntellisenseResult> result)
            var parser = new Dev2DataLanguageParser();
            var privateObject = new PrivateObject(parser);
            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            var children = new List<IDev2DataLanguageIntellisensePart> { new Dev2DataLanguageIntellisensePart("name", "name", new List<IDev2DataLanguageIntellisensePart>()) };
            refParts.Add(new Dev2DataLanguageIntellisensePart("Rec().Name", "rec", children));
            var result = new List<IIntellisenseResult>();
            var parseTO = new ParseTO()
            {
                Parent = new ParseTO() { Payload = "Name" },

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
            var parseTO = new ParseTO()
            {
                Parent = new ParseTO() { Payload = "Name" },

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
            var parseTO = new ParseTO()
            {
                Parent = new ParseTO() { Payload = "Name" },

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
            var parseTO = new ParseTO()
            {
                Parent = new ParseTO() { Payload = "Name" },

            };

            var parseTORecSet = new ParseTO()
            {
                Parent = new ParseTO() { Payload = "rec().Name" },
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
            catch(Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

    }
}
