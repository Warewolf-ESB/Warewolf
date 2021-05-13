/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Parsers;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Text;

namespace Dev2.Data.Tests.Parsers
{
    [TestClass]
    public class DataLanguageParserImplementationTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_PartsGeneration_NoResult()
        {
            const string payload = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";
            var parts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            const bool isFromIntellisense = false;
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();

            var mockParserHelper = new Mock<IParserHelper>();
            var mockDev2DataLanguageParser = new Mock<IDev2DataLanguageParser>();

            var dataLanguageParserImplementation = new DataLanguageParserImplementation(mockParserHelper.Object, mockDev2DataLanguageParser.Object);

            var result = dataLanguageParserImplementation.PartsGeneration(payload, parts, addCompleteParts, isFromIntellisense, additionalParts);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_PartsGeneration_CdataStart()
        {
            const string text = @"<![CDATA[[[]]]]>";
            var parts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            const bool isFromIntellisense = false;
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();

            var mockParserHelper = new Mock<IParserHelper>();
            var mockDev2DataLanguageParser = new Mock<IDev2DataLanguageParser>();

            var dataLanguageParserImplementation = new DataLanguageParserImplementation(mockParserHelper.Object, mockDev2DataLanguageParser.Object);

            var result = dataLanguageParserImplementation.PartsGeneration(text, parts, addCompleteParts, isFromIntellisense, additionalParts);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result[0].StartIndex);
            Assert.AreEqual(4, result[0].EndIndex);
            Assert.AreEqual(enIntellisenseErrorCode.SyntaxError, result[0].ErrorCode);
            Assert.AreEqual("Variable [[]] is missing a name", result[0].Message);
            Assert.IsNull(result[0].Option);
            Assert.AreEqual(enIntellisenseResultType.Error, result[0].Type);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_PartsGeneration_OpeningSquareBrackets()
        {
            const string textNoBrackets = @"Some text[[";
            var parts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            const bool isFromIntellisense = false;
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.StartIndex).Returns(0);
            var parseToList = new List<IParseTO>
            {
                mockParseTo.Object
            };

            var mockParserHelper = new Mock<IParserHelper>();
            var mockDev2DataLanguageParser = new Mock<IDev2DataLanguageParser>();
            mockDev2DataLanguageParser.Setup(dataLanguageParser => dataLanguageParser.MakeParts(textNoBrackets, addCompleteParts)).Returns(parseToList);

            var dataLanguageParserImplementation = new DataLanguageParserImplementation(mockParserHelper.Object, mockDev2DataLanguageParser.Object);

            var result = dataLanguageParserImplementation.PartsGeneration(textNoBrackets, parts, addCompleteParts, isFromIntellisense, additionalParts);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result[0].StartIndex);
            Assert.AreEqual(4, result[0].EndIndex);
            Assert.AreEqual(enIntellisenseErrorCode.SyntaxError, result[0].ErrorCode);
            Assert.AreEqual("Variable [[]] is missing a name", result[0].Message);
            Assert.IsNull(result[0].Option);
            Assert.AreEqual(enIntellisenseResultType.Error, result[0].Type);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_PartsGeneration_OpeningSquareBrackets_HangingOpen()
        {
            const string payload = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";
            const string textNoBrackets = @"Some text[[";
            var parts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            const bool isFromIntellisense = false;
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();

            var mockChildParseTo = new Mock<IParseTO>();
            mockChildParseTo.Setup(parseTo => parseTo.StartIndex).Returns(0);
            mockChildParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockChildParseTo.Setup(parseTo => parseTo.Payload).Returns(payload);

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.StartIndex).Returns(0);
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockParseTo.Setup(parseTo => parseTo.Child).Returns(mockChildParseTo.Object);
            mockParseTo.Setup(parseTo => parseTo.Payload).Returns(payload);
            var parseToList = new List<IParseTO>
            {
                mockParseTo.Object
            };

            var mockParserHelper = new Mock<IParserHelper>();
            var mockDev2DataLanguageParser = new Mock<IDev2DataLanguageParser>();
            mockDev2DataLanguageParser.Setup(dataLanguageParser => dataLanguageParser.MakeParts(textNoBrackets, addCompleteParts)).Returns(parseToList);

            var dataLanguageParserImplementation = new DataLanguageParserImplementation(mockParserHelper.Object, mockDev2DataLanguageParser.Object);

            var result = dataLanguageParserImplementation.PartsGeneration(textNoBrackets, parts, addCompleteParts, isFromIntellisense, additionalParts);

            const string expectedPayload = @" [[<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>]] contains a space, this is an invalid character for a variable name";
            const string expectedDisplayValue = @"[[<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>]]";
            const string expectedField = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result[0].StartIndex);
            Assert.AreEqual(0, result[0].EndIndex);
            Assert.AreEqual(enIntellisenseErrorCode.SyntaxError, result[0].ErrorCode);
            Assert.AreEqual(expectedPayload, result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual(expectedDisplayValue, result[0].Option.DisplayValue);
            Assert.AreEqual(expectedField, result[0].Option.Field);
            Assert.AreEqual(enIntellisenseResultType.Error, result[0].Type);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_PartsGeneration_OpeningSquareBrackets_AddFieldResult()
        {
            const string payload = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";
            const string textNoBrackets = @"Some text[[";
            var parts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            const bool isFromIntellisense = false;
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();

            var mockChildParseTo = new Mock<IParseTO>();
            mockChildParseTo.Setup(parseTo => parseTo.StartIndex).Returns(0);
            mockChildParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockChildParseTo.Setup(parseTo => parseTo.Payload).Returns(payload);

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.StartIndex).Returns(0);
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockParseTo.Setup(parseTo => parseTo.Child).Returns(mockChildParseTo.Object);
            mockParseTo.Setup(parseTo => parseTo.Payload).Returns(payload);
            var parseToList = new List<IParseTO>
            {
                mockParseTo.Object
            };

            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.AddFieldResult(It.IsAny<IParseTO>(), It.IsAny<List<IIntellisenseResult>>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>())).Returns(true);

            var mockDev2DataLanguageParser = new Mock<IDev2DataLanguageParser>();
            mockDev2DataLanguageParser.Setup(dataLanguageParser => dataLanguageParser.MakeParts(textNoBrackets, addCompleteParts)).Returns(parseToList);

            var dataLanguageParserImplementation = new DataLanguageParserImplementation(mockParserHelper.Object, mockDev2DataLanguageParser.Object);

            var result = dataLanguageParserImplementation.PartsGeneration(textNoBrackets, parts, addCompleteParts, isFromIntellisense, additionalParts);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_PartsGeneration_OpeningSquareBrackets_ProcessRegion()
        {
            const string payload = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>." +
                @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";
            const string textNoBrackets = @"Some text[[, ";
            var parts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            const bool isFromIntellisense = false;
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();

            var mockChildParseTo = new Mock<IParseTO>();
            mockChildParseTo.Setup(parseTo => parseTo.StartIndex).Returns(0);
            mockChildParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockChildParseTo.Setup(parseTo => parseTo.Payload).Returns(payload);

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.StartIndex).Returns(0);
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockParseTo.Setup(parseTo => parseTo.Child).Returns(mockChildParseTo.Object);
            mockParseTo.Setup(parseTo => parseTo.Payload).Returns(payload);
            var parseToList = new List<IParseTO>
            {
                mockParseTo.Object
            };

            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny))
                .Returns(true);

            var mockDev2DataLanguageParser = new Mock<IDev2DataLanguageParser>();
            mockDev2DataLanguageParser.Setup(dataLanguageParser => dataLanguageParser.MakeParts(textNoBrackets, addCompleteParts)).Returns(parseToList);

            var dataLanguageParserImplementation = new DataLanguageParserImplementation(mockParserHelper.Object, mockDev2DataLanguageParser.Object);

            var result = dataLanguageParserImplementation.PartsGeneration(textNoBrackets, parts, addCompleteParts, isFromIntellisense, additionalParts);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_ProcessForChild_ExpectsFalse()
        {
            var mockParseTo = new Mock<IParseTO>();

            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            var result = new List<IIntellisenseResult>();
            const string search = "";

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");

            var emptyOk = DataLanguageParserImplementation.ProcessForChild(mockParseTo.Object, refParts, result, search, mockDataIntellisensePart.Object);

            Assert.IsFalse(emptyOk);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_ProcessForChild_ExpectsTrue()
        {
            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.IsLeaf).Returns(true);

            List<IDev2DataLanguageIntellisensePart> refParts1 = null;

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Children).Returns(refParts1);

            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            refParts.Add(mockDataIntellisensePart.Object);

            var result = new List<IIntellisenseResult>();
            const string search = "";

            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");

            var emptyOk = DataLanguageParserImplementation.ProcessForChild(mockParseTo.Object, refParts, result, search, mockDataIntellisensePart.Object);

            Assert.IsTrue(emptyOk);

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(" / Select a specific row", result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual(" / Select a specific row", result[0].Option.Description);
            Assert.AreEqual("[[]]", result[0].Option.DisplayValue);
            Assert.AreEqual("", result[0].Option.Field);
            Assert.IsTrue(result[0].Option.HasRecordsetIndex);
            Assert.AreEqual("", result[0].Option.Recordset);
            Assert.AreEqual("[[field2]]", result[0].Option.RecordsetIndex);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[1].ErrorCode);
            Assert.AreEqual(" / Reference all rows in the Recordset ", result[1].Message);
            Assert.IsNotNull(result[1].Option);
            Assert.AreEqual(" / Reference all rows in the Recordset ", result[1].Option.Description);
            Assert.AreEqual("[[]]", result[1].Option.DisplayValue);
            Assert.AreEqual("", result[1].Option.Field);
            Assert.IsTrue(result[1].Option.HasRecordsetIndex);
            Assert.AreEqual("", result[1].Option.Recordset);
            Assert.AreEqual("*", result[1].Option.RecordsetIndex);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_MatchFieldVariables_ValidateName_ExpectsTrue()
        {
            const string payload = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";

            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny))
                .Returns(true);

            var mockParseTo = new Mock<IParseTO>();
            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            var result = new List<IIntellisenseResult>();
            var parts = new string[] { payload };
            const bool isRs = false;
            const string rawSearch = "";
            const string search = "";
            const bool emptyOk = false;

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchFieldVariables(mockParseTo.Object, refParts, addCompleteParts, result, parts, isRs, rawSearch, search, emptyOk);

            Assert.AreEqual(0, result.Count);
            mockParserHelper.Verify(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_MatchFieldVariables_ValidateName_ExpectsFalse()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny))
                .Returns(false);
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);

            const string payload = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>." +
                @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";

            var mockParseTo = new Mock<IParseTO>();
            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            var result = new List<IIntellisenseResult>();
            var parts = payload.Split('.');
            const bool isRs = false;
            const string rawSearch = "";
            const string search = "";
            const bool emptyOk = false;

            const string expectedMessage = @" [[<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>]] contains a space, this is an invalid character for a variable name";
            const string expectedDisplayValue = @"[[<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>.<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>]]";
            const string expectedField = @".<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";
            const string expectedRecordset = @"<doc><recName1 Description=""RecName1 Description""><field1 Description=""field1 Desc"" /><field2 Description=""field2 Desc"" /></recName1><recName2 Description=""RecName2 Description"" /></doc>";

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchFieldVariables(mockParseTo.Object, refParts, addCompleteParts, result, parts, isRs, rawSearch, search, emptyOk);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(enIntellisenseErrorCode.SyntaxError, result[0].ErrorCode);
            Assert.AreEqual(expectedMessage, result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual(expectedDisplayValue, result[0].Option.DisplayValue);
            Assert.AreEqual(expectedField, result[0].Option.Field);
            Assert.AreEqual(expectedRecordset, result[0].Option.Recordset);
            mockParserHelper.Verify(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny), Times.Once());
            mockParserHelper.Verify(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_MatchFieldVariables_IncorrectRecordset()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny))
                .Returns(false);
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);

            const string payload = "recName1.field1";

            var mockParseTo = new Mock<IParseTO>();
            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            var result = new List<IIntellisenseResult>();
            var parts = payload.Split('.');
            const bool isRs = false;
            const string rawSearch = "";
            const string search = "";
            const bool emptyOk = false;

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchFieldVariables(mockParseTo.Object, refParts, addCompleteParts, result, parts, isRs, rawSearch, search, emptyOk);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(enIntellisenseErrorCode.NeitherRecordsetNorFieldFound, result[0].ErrorCode);
            Assert.AreEqual("[[recName1]] does not exist in your variable list", result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual("[[recName1().field1]]", result[0].Option.DisplayValue);
            Assert.AreEqual("field1", result[0].Option.Field);
            Assert.AreEqual("recName1", result[0].Option.Recordset);
            mockParserHelper.Verify(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny), Times.Once());
            mockParserHelper.Verify(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_MatchFieldVariables_IsRecordset_ValidateName_ExpectsTrue()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny))
                .Returns(true);
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);

            const string payload = "recName1().field1";

            var mockParseTo = new Mock<IParseTO>();
            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            var result = new List<IIntellisenseResult>();
            var parts = payload.Split('.');
            const bool isRs = true;
            const string rawSearch = "recName1()";
            const string search = "";
            const bool emptyOk = false;

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchFieldVariables(mockParseTo.Object, refParts, addCompleteParts, result, parts, isRs, rawSearch, search, emptyOk);

            Assert.AreEqual(0, result.Count);
            mockParserHelper.Verify(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_MatchFieldVariables_IsRecordset_ValidateName_ExpectsFalse()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny))
                .Returns(false);
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);

            const string payload = "recName1().field1";

            var mockParseTo = new Mock<IParseTO>();
            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            var result = new List<IIntellisenseResult>();
            var parts = payload.Split('.');
            const bool isRs = true;
            const string rawSearch = "recName1()";
            const string search = "";
            const bool emptyOk = false;

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchFieldVariables(mockParseTo.Object, refParts, addCompleteParts, result, parts, isRs, rawSearch, search, emptyOk);

            mockParserHelper.Verify(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny), Times.Once());
            mockParserHelper.Verify(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>()), Times.Once);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(enIntellisenseErrorCode.NeitherRecordsetNorFieldFound, result[0].ErrorCode);
            Assert.AreEqual("[[recName1()]] does not exist in your variable list", result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual("[[recName1().field1]]", result[0].Option.DisplayValue);
            Assert.AreEqual("field1", result[0].Option.Field);
            Assert.AreEqual("recName1", result[0].Option.Recordset);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_MatchFieldVariables_IsRecordset_ValidateName_HasRecordset()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny))
                .Returns(false);
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);
            mockParserHelper.Setup(parserHelper => parserHelper.ProcessFieldsForRecordSet(It.IsAny<IParseTO>(), It.IsAny<bool>(), It.IsAny<IList<IIntellisenseResult>>(), It.IsAny<string[]>(), out It.Ref<string>.IsAny, out It.Ref<bool>.IsAny, It.IsAny<string>(), It.IsAny<IDev2DataLanguageIntellisensePart>(), It.IsAny<string>()))
                .Returns(true);

            const string payload = "recName1().field1";

            var mockParseTo = new Mock<IParseTO>();


            var mockDataIntellisensePartChild = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePartChild.Setup(dlip => dlip.Name).Returns("childField2");
            var refParts1 = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePartChild.Object
            };

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");
            mockDataIntellisensePart.Setup(dlip => dlip.Children).Returns(refParts1);

            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            refParts.Add(mockDataIntellisensePart.Object);

            const bool addCompleteParts = false;
            var result = new List<IIntellisenseResult>();
            var parts = payload.Split('.');
            const bool isRs = true;
            const string rawSearch = "recName1()";
            const string search = "field2";
            const bool emptyOk = false;

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchFieldVariables(mockParseTo.Object, refParts, addCompleteParts, result, parts, isRs, rawSearch, search, emptyOk);

            mockParserHelper.Verify(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny), Times.Once());
            mockParserHelper.Verify(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>()), Times.Once);
            mockParserHelper.Verify(parserHelper => parserHelper.ProcessFieldsForRecordSet(It.IsAny<IParseTO>(), It.IsAny<bool>(), It.IsAny<IList<IIntellisenseResult>>(), It.IsAny<string[]>(), out It.Ref<string>.IsAny, out It.Ref<bool>.IsAny, It.IsAny<string>(), It.IsAny<IDev2DataLanguageIntellisensePart>(), It.IsAny<string>()), Times.Once);

            Assert.AreEqual(0, result.Count);
        }

        class CallbackTestClass
        {
            public void ProcessFieldsForRecordSetCallback(IParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, out string search, out bool emptyOk, string display, IDev2DataLanguageIntellisensePart recordsetPart, string partName)
            {
                search = "field2";
                emptyOk = false;
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_MatchFieldVariables_EmptyList()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny))
                .Returns(false);
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);

            var asdf = new CallbackTestClass();
            var outSearch = "field2";
            var outEmptyOk = false;

            mockParserHelper.Setup(parserHelper => parserHelper.ProcessFieldsForRecordSet(It.IsAny<IParseTO>(), It.IsAny<bool>(), It.IsAny<IList<IIntellisenseResult>>(), It.IsAny<string[]>(), out outSearch, out outEmptyOk, It.IsAny<string>(), It.IsAny<IDev2DataLanguageIntellisensePart>(), It.IsAny<string>()))
                .Callback(() =>
                {
                    asdf.ProcessFieldsForRecordSetCallback(It.IsAny<IParseTO>(), It.IsAny<bool>(), It.IsAny<IList<IIntellisenseResult>>(), It.IsAny<string[]>(), out outSearch, out outEmptyOk, "", It.IsAny<IDev2DataLanguageIntellisensePart>(), It.IsAny<string>());
                    outSearch = "field2";
                })
                .Returns(false);

            const string payload = "a.b";

            var mockParseTo = new Mock<IParseTO>();

            var mockDataIntellisensePartChild = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePartChild.Setup(dlip => dlip.Name).Returns("childField2");
            var refParts1 = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePartChild.Object
            };

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");
            mockDataIntellisensePart.Setup(dlip => dlip.Children).Returns(refParts1);

            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            refParts.Add(mockDataIntellisensePart.Object);

            const bool addCompleteParts = false;
            var result = new List<IIntellisenseResult>();
            var parts = payload.Split('.');
            const bool isRs = true;
            const string rawSearch = "recName1()";
            const string search = "field2";
            const bool emptyOk = false;

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchFieldVariables(mockParseTo.Object, refParts, addCompleteParts, result, parts, isRs, rawSearch, search, emptyOk);

            mockParserHelper.Verify(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.Ref<IList<IIntellisenseResult>>.IsAny), Times.Once());
            mockParserHelper.Verify(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>()), Times.Once);
            mockParserHelper.Verify(parserHelper => parserHelper.ProcessFieldsForRecordSet(It.IsAny<IParseTO>(), It.IsAny<bool>(), It.IsAny<IList<IIntellisenseResult>>(), It.IsAny<string[]>(), out It.Ref<string>.IsAny, out It.Ref<bool>.IsAny, It.IsAny<string>(), It.IsAny<IDev2DataLanguageIntellisensePart>(), It.IsAny<string>()), Times.Once);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(enIntellisenseErrorCode.FieldNotFound, result[0].ErrorCode);
            Assert.AreEqual("Recordset Field [ field2 ] does not exist for [ a ]", result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual("[[a().field2]]", result[0].Option.DisplayValue);
            Assert.AreEqual("field2", result[0].Option.Field);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_Scalar_MatchNonFieldVariables()
        {
            var mockIntellisenseResult = new Mock<IIntellisenseResult>();
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>())).Returns(mockIntellisenseResult.Object);

            const string payload = "recName1().field1";

            var mockParseTo = new Mock<IParseTO>();
            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            var tmp = new StringBuilder();
            var result = new List<IIntellisenseResult>();
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool isRs = false;
            const string rawSearch = "field";
            const string search = "";
            const bool emptyOk = false;
            var parts = payload.Split('.');

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchNonFieldVariables(mockParseTo.Object, refParts, addCompleteParts, tmp, result, additionalParts, isRs, rawSearch, search, emptyOk, parts);

            mockParserHelper.Verify(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(mockIntellisenseResult.Object, result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_Recordset_MatchNonFieldVariables()
        {
            var mockIntellisenseResult = new Mock<IIntellisenseResult>();
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>())).Returns(mockIntellisenseResult.Object);

            const string payload = "recName1().field1";

            var mockParseTo = new Mock<IParseTO>();
            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool addCompleteParts = false;
            var tmp = new StringBuilder();
            var result = new List<IIntellisenseResult>();
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool isRs = true;
            const string rawSearch = "recName1()";
            const string search = "";
            const bool emptyOk = false;
            var parts = payload.Split('.');

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchNonFieldVariables(mockParseTo.Object, refParts, addCompleteParts, tmp, result, additionalParts, isRs, rawSearch, search, emptyOk, parts);

            mockParserHelper.Verify(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(mockIntellisenseResult.Object, result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_Recordset_LoopRefParts()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);

            const string payload = "recName1().field1";

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);

            var mockDev2DataLanguageIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDev2DataLanguageIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");

            var refParts = new List<IDev2DataLanguageIntellisensePart>();
            refParts.Add(mockDev2DataLanguageIntellisensePart.Object);

            const bool addCompleteParts = false;
            var tmp = new StringBuilder();
            var result = new List<IIntellisenseResult>();
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool isRs = true;
            const string rawSearch = "recName1()";
            const string search = "";
            const bool emptyOk = false;
            var parts = payload.Split('.');

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchNonFieldVariables(mockParseTo.Object, refParts, addCompleteParts, tmp, result, additionalParts, isRs, rawSearch, search, emptyOk, parts);

            mockParserHelper.Verify(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>()), Times.Once());
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.IsNull(result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual("[[field2]]", result[0].Option.DisplayValue);
            Assert.AreEqual("field2", result[0].Option.Field);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_Recordset_LoopRefParts_WithChildren()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);

            const string payload = "recName1().field1";

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);

            var mockDataIntellisensePartChild = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePartChild.Setup(dlip => dlip.Name).Returns("childField2");
            var refParts1 = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePartChild.Object
            };

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");
            mockDataIntellisensePart.Setup(dlip => dlip.Children).Returns(refParts1);

            var refParts = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePart.Object
            };

            const bool addCompleteParts = false;
            var tmp = new StringBuilder();
            var result = new List<IIntellisenseResult>();
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool isRs = true;
            const string rawSearch = "recName1()";
            const string search = "";
            const bool emptyOk = false;
            var parts = payload.Split('.');

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchNonFieldVariables(mockParseTo.Object, refParts, addCompleteParts, tmp, result, additionalParts, isRs, rawSearch, search, emptyOk, parts);

            mockParserHelper.Verify(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>()), Times.Once());
            Assert.AreEqual(4, result.Count);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(" / Select a specific row or Close", result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual(" / Select a specific row or Close", result[0].Option.Description);
            Assert.AreEqual("[[field2(", result[0].Option.DisplayValue);
            Assert.AreEqual("field2(", result[0].Option.Field);
            Assert.AreEqual("", result[0].Option.Recordset);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[1].ErrorCode);
            Assert.AreEqual(" / Takes all rows ", result[1].Message);
            Assert.IsNotNull(result[1].Option);
            Assert.AreEqual(" / Takes all rows ", result[1].Option.Description);
            Assert.AreEqual("[[field2(*)]]", result[1].Option.DisplayValue);
            Assert.AreEqual("", result[1].Option.Field);
            Assert.AreEqual("field2", result[1].Option.Recordset);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[2].ErrorCode);
            Assert.AreEqual(" / Take last row", result[2].Message);
            Assert.IsNotNull(result[2].Option);
            Assert.AreEqual(" / Take last row", result[2].Option.Description);
            Assert.AreEqual("[[field2()]]", result[2].Option.DisplayValue);
            Assert.AreEqual("", result[2].Option.Field);
            Assert.AreEqual("field2", result[2].Option.Recordset);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[3].ErrorCode);
            Assert.AreEqual(" / Use the field of a Recordset", result[3].Message);
            Assert.IsNotNull(result[3].Option);
            Assert.AreEqual(" / Use the field of a Recordset", result[3].Option.Description);
            Assert.AreEqual("[[field2().childField2]]", result[3].Option.DisplayValue);
            Assert.AreEqual("childField2", result[3].Option.Field);
            Assert.AreEqual("field2", result[3].Option.Recordset);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_Recordset_LoopRefParts_WithChildren_MatchSearch()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);

            const string payload = "recName1().field1";

            var mockParseToChild = new Mock<IParseTO>();
            mockParseToChild.Setup(parseTo => parseTo.Payload).Returns("");

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockParseTo.Setup(parseTo => parseTo.Child).Returns(mockParseToChild.Object);

            var mockDataIntellisensePartChild = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePartChild.Setup(dlip => dlip.Name).Returns("childField2");
            var refParts1 = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePartChild.Object
            };

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");
            mockDataIntellisensePart.Setup(dlip => dlip.Children).Returns(refParts1);

            var refParts = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePart.Object
            };

            const bool addCompleteParts = false;
            var tmp = new StringBuilder();
            var result = new List<IIntellisenseResult>();
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool isRs = true;
            const string rawSearch = "recName1()";
            const string search = "field2";
            const bool emptyOk = false;
            var parts = payload.Split('.');

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchNonFieldVariables(mockParseTo.Object, refParts, addCompleteParts, tmp, result, additionalParts, isRs, rawSearch, search, emptyOk, parts);

            mockParserHelper.Verify(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>()), Times.Once());
            Assert.AreEqual(3, result.Count);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(" / Select a specific row", result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual(" / Select a specific row", result[0].Option.Description);
            Assert.AreEqual("[[field2([[]])]]", result[0].Option.DisplayValue);
            Assert.AreEqual("", result[0].Option.Field);
            Assert.IsTrue(result[0].Option.HasRecordsetIndex);
            Assert.AreEqual("field2", result[0].Option.Recordset);
            Assert.AreEqual("[[]]", result[0].Option.RecordsetIndex);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[1].ErrorCode);
            Assert.AreEqual(" / Select a specific field at a specific row", result[1].Message);
            Assert.IsNotNull(result[1].Option);
            Assert.AreEqual(" / Select a specific field at a specific row", result[1].Option.Description);
            Assert.AreEqual("[[field2([[]]).childField2]]", result[1].Option.DisplayValue);
            Assert.AreEqual("childField2", result[1].Option.Field);
            Assert.IsTrue(result[1].Option.HasRecordsetIndex);
            Assert.AreEqual("field2", result[1].Option.Recordset);
            Assert.AreEqual("[[]]", result[1].Option.RecordsetIndex);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[2].ErrorCode);
            Assert.AreEqual(" /  Select this recordset field field", result[2].Message);
            Assert.IsNotNull(result[2].Option);
            Assert.AreEqual(" /  Select this recordset field field", result[2].Option.Description);
            Assert.AreEqual("[[field2().childField2]]", result[2].Option.DisplayValue);
            Assert.AreEqual("childField2", result[2].Option.Field);
            Assert.IsFalse(result[2].Option.HasRecordsetIndex);
            Assert.AreEqual("field2", result[2].Option.Recordset);
            Assert.AreEqual("", result[2].Option.RecordsetIndex);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_Recordset_LoopRefParts_WithChildren_MatchSearch_IsRsFalse()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);

            const string payload = "recName1().field1";

            var mockParseToChild = new Mock<IParseTO>();
            mockParseToChild.Setup(parseTo => parseTo.Payload).Returns("");

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockParseTo.Setup(parseTo => parseTo.Child).Returns(mockParseToChild.Object);

            var mockDataIntellisensePartChild = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePartChild.Setup(dlip => dlip.Name).Returns("childField2");
            var refParts1 = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePartChild.Object
            };

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");
            mockDataIntellisensePart.Setup(dlip => dlip.Children).Returns(refParts1);

            var refParts = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePart.Object
            };

            const bool addCompleteParts = false;
            var tmp = new StringBuilder();
            var result = new List<IIntellisenseResult>();
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool isRs = false;
            const string rawSearch = "recName1()";
            const string search = "field2";
            const bool emptyOk = false;
            var parts = payload.Split('.');

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchNonFieldVariables(mockParseTo.Object, refParts, addCompleteParts, tmp, result, additionalParts, isRs, rawSearch, search, emptyOk, parts);

            mockParserHelper.Verify(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>()), Times.Once());
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.IsNull(result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.IsNull(result[0].Option.Description);
            Assert.AreEqual("[[field2()]]", result[0].Option.DisplayValue);
            Assert.AreEqual("", result[0].Option.Field);
            Assert.IsFalse(result[0].Option.HasRecordsetIndex);
            Assert.AreEqual("field2", result[0].Option.Recordset);
            Assert.AreEqual("", result[0].Option.RecordsetIndex);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[1].ErrorCode);
            Assert.AreEqual(" / Use a field of the Recordset", result[1].Message);
            Assert.IsNotNull(result[1].Option);
            Assert.AreEqual(" / Use a field of the Recordset", result[1].Option.Description);
            Assert.AreEqual("[[field2().childField2]]", result[1].Option.DisplayValue);
            Assert.AreEqual("childField2", result[1].Option.Field);
            Assert.IsFalse(result[1].Option.HasRecordsetIndex);
            Assert.AreEqual("field2", result[1].Option.Recordset);
            Assert.AreEqual("", result[1].Option.RecordsetIndex);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_Recordset_LoopRefParts_NoChildren_MatchSearch_IsRsFalse()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);

            const string payload = "recName1().field1";

            var mockParseToChild = new Mock<IParseTO>();
            mockParseToChild.Setup(parseTo => parseTo.Payload).Returns("");

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockParseTo.Setup(parseTo => parseTo.Child).Returns(mockParseToChild.Object);

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");

            var refParts = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePart.Object
            };

            const bool addCompleteParts = true;
            var tmp = new StringBuilder();
            var result = new List<IIntellisenseResult>();
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool isRs = false;
            const string rawSearch = "recName1()";
            const string search = "field2";
            const bool emptyOk = false;
            var parts = payload.Split('.');

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchNonFieldVariables(mockParseTo.Object, refParts, addCompleteParts, tmp, result, additionalParts, isRs, rawSearch, search, emptyOk, parts);

            mockParserHelper.Verify(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>()), Times.Once());
            Assert.AreEqual(1, result.Count);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.IsNull(result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual("", result[0].Option.Description);
            Assert.AreEqual("[[field2]]", result[0].Option.DisplayValue);
            Assert.AreEqual("field2", result[0].Option.Field);
            Assert.IsFalse(result[0].Option.HasRecordsetIndex);
            Assert.AreEqual("", result[0].Option.Recordset);
            Assert.AreEqual("", result[0].Option.RecordsetIndex);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_Recordset_LoopRefParts_WithParent_MatchSearch_IsRsFalse()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);

            const string payload = "recName1().field1";

            var mockParseToChild = new Mock<IParseTO>();
            mockParseToChild.Setup(parseTo => parseTo.Payload).Returns("ChildPayload");

            var mockParseToParent = new Mock<IParseTO>();
            mockParseToParent.Setup(parseTo => parseTo.Payload).Returns("ParentPayload");

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockParseTo.Setup(parseTo => parseTo.Child).Returns(mockParseToChild.Object);
            mockParseTo.Setup(parseTo => parseTo.Parent).Returns(mockParseTo.Object);
            mockParseTo.Setup(parseTo => parseTo.Payload).Returns("Payload()");

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");

            var refParts = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePart.Object
            };

            const bool addCompleteParts = true;
            var tmp = new StringBuilder();
            var result = new List<IIntellisenseResult>();
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool isRs = false;
            const string rawSearch = "recName1()";
            const string search = "field2";
            const bool emptyOk = false;
            var parts = payload.Split('.');

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchNonFieldVariables(mockParseTo.Object, refParts, addCompleteParts, tmp, result, additionalParts, isRs, rawSearch, search, emptyOk, parts);

            mockParserHelper.Verify(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>()), Times.Once());
            Assert.AreEqual(1, result.Count);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(" / Select a specific row ", result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual("", result[0].Option.Description);
            Assert.AreEqual("[[field2]]", result[0].Option.DisplayValue);
            Assert.AreEqual("field2", result[0].Option.Field);
            Assert.IsFalse(result[0].Option.HasRecordsetIndex);
            Assert.AreEqual("", result[0].Option.Recordset);
            Assert.AreEqual("", result[0].Option.RecordsetIndex);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Match_FinalEvaluation()
        {
            var mockParserHelper = new Mock<IParserHelper>();
            mockParserHelper.Setup(parserHelper => parserHelper.IsValidIndex(It.IsAny<IParseTO>())).Returns(true);

            const string payload = "recName1().field1";

            var mockParseToChild = new Mock<IParseTO>();
            mockParseToChild.Setup(parseTo => parseTo.Payload).Returns("ChildPayload");

            var mockParseToParent = new Mock<IParseTO>();
            mockParseToParent.Setup(parseTo => parseTo.Payload).Returns("ParentPayload");

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockParseTo.Setup(parseTo => parseTo.Child).Returns(mockParseToChild.Object);
            mockParseTo.Setup(parseTo => parseTo.Parent).Returns(mockParseTo.Object);
            mockParseTo.Setup(parseTo => parseTo.Payload).Returns("Payload()");

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");

            var refParts = new List<IDev2DataLanguageIntellisensePart>();

            const bool addCompleteParts = true;
            var tmp = new StringBuilder();
            tmp.AppendFormat(payload);

            var result = new List<IIntellisenseResult>();
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();
            const bool isRs = true;
            const string rawSearch = "recName1()";
            const string search = "field2";
            const bool emptyOk = false;
            var parts = payload.Split('.');

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);

            match.MatchNonFieldVariables(mockParseTo.Object, refParts, addCompleteParts, tmp, result, additionalParts, isRs, rawSearch, search, emptyOk, parts);

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual(enIntellisenseErrorCode.RecordsetNotFound, result[0].ErrorCode);
            Assert.AreEqual(" [[recName1()]] does not exist in your variable list", result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual("", result[0].Option.Description);
            Assert.AreEqual("[[recName1()]]", result[0].Option.DisplayValue);
            Assert.AreEqual("", result[0].Option.Field);
            Assert.IsFalse(result[0].Option.HasRecordsetIndex);
            Assert.AreEqual("recName1()", result[0].Option.Recordset);
            Assert.AreEqual("", result[0].Option.RecordsetIndex);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Extract_ExtractIntellisenseOptions_ProcessRegion()
        {
            var mockParserHelper = new Mock<IParserHelper>();

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);
            var extract = new DataLanguageParserImplementation.Extract(mockParserHelper.Object, match);

            var mockParseTo = new Mock<IParseTO>();

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");

            var refParts = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePart.Object
            };

            const string payload = "recName1().field1.field2";

            const bool addCompleteParts = true;
            var tmp = new StringBuilder();
            tmp.AppendFormat(payload);
            var result = new List<IIntellisenseResult>();
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();

            extract.ProcessRegion(mockParseTo.Object, refParts, addCompleteParts, tmp, result, additionalParts);

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual(enIntellisenseErrorCode.SyntaxError, result[0].ErrorCode);
            Assert.AreEqual("Invalid Notation - Extra dots detected", result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual("", result[0].Option.Description);
            Assert.AreEqual("[[recName1().field1]]", result[0].Option.DisplayValue);
            Assert.AreEqual("field1", result[0].Option.Field);
            Assert.IsFalse(result[0].Option.HasRecordsetIndex);
            Assert.AreEqual("recName1()", result[0].Option.Recordset);
            Assert.AreEqual("", result[0].Option.RecordsetIndex);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Extract_ExtractIntellisenseOptions_ProcessForOnlyOpenRegion()
        {
            var mockParserHelper = new Mock<IParserHelper>();

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);
            var extract = new DataLanguageParserImplementation.Extract(mockParserHelper.Object, match);

            var mockParseToChild = new Mock<IParseTO>();
            mockParseToChild.Setup(parseTo => parseTo.Payload).Returns("ChildPayload");

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockParseTo.Setup(parseTo => parseTo.IsRecordSet).Returns(false);
            mockParseTo.Setup(parseTo => parseTo.Child).Returns(mockParseToChild.Object);
            mockParseTo.Setup(parseTo => parseTo.Parent).Returns(mockParseTo.Object);
            mockParseTo.Setup(parseTo => parseTo.Payload).Returns("");

            var mockDataIntellisensePartChild = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePartChild.Setup(dlip => dlip.Name).Returns("childField2");
            var refParts1 = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePartChild.Object
            };

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");
            mockDataIntellisensePart.Setup(dlip => dlip.Children).Returns(refParts1);

            var refParts = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePart.Object
            };

            const bool addCompleteParts = true;
            var additionalParts = new List<IDev2DataLanguageIntellisensePart>();

            var result = extract.ExtractIntellisenseOptions(mockParseTo.Object, refParts, addCompleteParts, additionalParts);

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual(" / Select this record set", result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual(" / Select this record set", result[0].Option.Description);
            Assert.AreEqual("[[field2()]]", result[0].Option.DisplayValue);
            Assert.AreEqual("", result[0].Option.Field);
            Assert.IsFalse(result[0].Option.HasRecordsetIndex);
            Assert.AreEqual("field2", result[0].Option.Recordset);
            Assert.AreEqual("", result[0].Option.RecordsetIndex);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[1].ErrorCode);
            Assert.AreEqual("\r\n", result[1].Message);
            Assert.IsNotNull(result[1].Option);
            Assert.AreEqual(" / Select this record set field", result[1].Option.Description);
            Assert.AreEqual("[[field2().childField2]]", result[1].Option.DisplayValue);
            Assert.AreEqual("childField2", result[1].Option.Field);
            Assert.IsFalse(result[1].Option.HasRecordsetIndex);
            Assert.AreEqual("field2", result[1].Option.Recordset);
            Assert.AreEqual("", result[1].Option.RecordsetIndex);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Extract_ExtractActualIntellisenseOptions_CreateResultsGeneric_HasIndex()
        {
            var mockParserHelper = new Mock<IParserHelper>();

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);
            var extract = new DataLanguageParserImplementation.Extract(mockParserHelper.Object, match);

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockParseTo.Setup(parseTo => parseTo.Payload).Returns("Payload()");

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");

            var refParts = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePart.Object
            };

            const bool addCompleteParts = true;
            var result = new List<IIntellisenseResult>();
            const string payload = "recName1()";
            var parts = payload.Split('.');
            const string search = "recName1()";

            DataLanguageParserImplementation.Extract.ExtractActualIntellisenseOptions(mockParseTo.Object, refParts, addCompleteParts, result, parts, search);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DataLanguageParserImplementation))]
        public void DataLanguageParserImplementation_Extract_ExtractActualIntellisenseOptions_CreateResultsGeneric_AddIndex()
        {
            var mockParserHelper = new Mock<IParserHelper>();

            var match = new DataLanguageParserImplementation.Match(mockParserHelper.Object);
            var extract = new DataLanguageParserImplementation.Extract(mockParserHelper.Object, match);

            var mockParseTo = new Mock<IParseTO>();
            mockParseTo.Setup(parseTo => parseTo.HangingOpen).Returns(true);
            mockParseTo.Setup(parseTo => parseTo.Payload).Returns("Payload(");

            var mockDataIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockDataIntellisensePart.Setup(dlip => dlip.Name).Returns("field2");

            var refParts = new List<IDev2DataLanguageIntellisensePart>
            {
                mockDataIntellisensePart.Object
            };

            const bool addCompleteParts = true;
            var result = new List<IIntellisenseResult>();
            const string payload = "recName1()";
            var parts = payload.Split('.');
            const string search = "recName1";

            DataLanguageParserImplementation.Extract.ExtractActualIntellisenseOptions(mockParseTo.Object, refParts, addCompleteParts, result, parts, search);

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual(enIntellisenseErrorCode.None, result[0].ErrorCode);
            Assert.AreEqual("", result[0].Message);
            Assert.IsNotNull(result[0].Option);
            Assert.AreEqual("", result[0].Option.Description);
            Assert.AreEqual("[[recName1([[field2]])]]", result[0].Option.DisplayValue);
            Assert.AreEqual("", result[0].Option.Field);
            Assert.IsTrue(result[0].Option.HasRecordsetIndex);
            Assert.AreEqual("recName1", result[0].Option.Recordset);
            Assert.AreEqual("[[field2]]", result[0].Option.RecordsetIndex);
        }
    }
}
