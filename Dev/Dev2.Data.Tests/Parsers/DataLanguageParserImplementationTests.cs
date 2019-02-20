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

            //var mockIntellisenseResult = new Mock<IIntellisenseResult>()
            //var results = new List<IIntellisenseResult>()
            //var results1 = new Mock<IList<IIntellisenseResult>>()

            var mockParserHelper = new Mock<IParserHelper>();
            //mockParserHelper.Setup(parserHelper => parserHelper.ValidateName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IIntellisenseResult>>(), out It.IsAny<List<IIntellisenseResult>>()))
            //    .Callback<IIntellisenseResult>(value => ref mockIntellisenseResult.Object)
            //    .Returns(true)

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
    }
}
