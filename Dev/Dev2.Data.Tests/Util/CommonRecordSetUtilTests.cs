#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Data.Tests.Util
{
    [TestClass]
    public class CommonRecordSetUtilTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ReplaceRecordBlankWithStar()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("[[rec(*)]]", instance.ReplaceRecordBlankWithStar("[[rec(*)]]"));
            Assert.AreEqual("[[rec(*)]]", instance.ReplaceRecordBlankWithStar("[[rec()]]"));
            Assert.AreEqual("[[rec]]", instance.ReplaceRecordBlankWithStar("[[rec]]"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ReplaceRecordsetBlankWithStar()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("[[rec(*).n]]", instance.ReplaceRecordsetBlankWithStar("[[rec().n]]"));
            Assert.AreEqual("[[rec()]]", instance.ReplaceRecordsetBlankWithStar("[[rec()]]"));
            Assert.AreEqual("[[rec(a)]]", instance.ReplaceRecordsetBlankWithStar("[[rec(a)]]"));
            Assert.AreEqual("[[rec]]", instance.ReplaceRecordsetBlankWithStar("[[rec]]"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ReplaceRecordsetBlankWithIndex()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("[[rec(*).a]]", instance.ReplaceRecordsetBlankWithIndex("[[rec(*).a]]", 2));
            Assert.AreEqual("[[rec(2).a]]", instance.ReplaceRecordsetBlankWithIndex("[[rec().a]]", 2));
            Assert.AreEqual("[[rec]]", instance.ReplaceRecordsetBlankWithIndex("[[rec]]", 2));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ReplaceObjectBlankWithIndex()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("[[rec(*)]]", instance.ReplaceObjectBlankWithIndex("[[rec(*)]]", 2));
            Assert.AreEqual("[[rec(2)]]", instance.ReplaceObjectBlankWithIndex("[[rec()]]", 2));
            Assert.AreEqual("[[rec]]", instance.ReplaceObjectBlankWithIndex("[[rec]]", 2));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_CreateRecordsetDisplayValue()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("rec(2).col1", instance.CreateRecordsetDisplayValue("rec", "col1", "2"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_RemoveRecordsetBracketsFromValue()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("[[rec(*)]]", instance.RemoveRecordsetBracketsFromValue("[[rec(*)]]"));
            Assert.AreEqual("rec(*)", instance.RemoveRecordsetBracketsFromValue("rec(*)"));
            Assert.AreEqual("rec", instance.RemoveRecordsetBracketsFromValue("rec"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_GetRecordsetIndexType()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual(Interfaces.Enums.enRecordsetIndexType.Blank, instance.GetRecordsetIndexType("[[rec()]]"));
            Assert.AreEqual(Interfaces.Enums.enRecordsetIndexType.Error, instance.GetRecordsetIndexType("[[rec(a)]]"));
            Assert.AreEqual(Interfaces.Enums.enRecordsetIndexType.Numeric, instance.GetRecordsetIndexType("[[rec(3)]]"));
            Assert.AreEqual(Interfaces.Enums.enRecordsetIndexType.Star, instance.GetRecordsetIndexType("[[rec(*)]]"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_IsStarIndex()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual(true, instance.IsStarIndex("[[rec(*)]]"));
            Assert.AreEqual(false, instance.IsStarIndex("[[rec()]]"));
            Assert.AreEqual(false, instance.IsStarIndex(""));
            Assert.AreEqual(false, instance.IsStarIndex(null));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ExtractIndexRegionFromRecordset()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("*", instance.ExtractIndexRegionFromRecordset("[[rec(*)]]"));
            Assert.AreEqual("a", instance.ExtractIndexRegionFromRecordset("[[rec(a)]]"));
            Assert.AreEqual("2", instance.ExtractIndexRegionFromRecordset("[[rec(2)]]"));
            Assert.AreEqual("2", instance.ExtractIndexRegionFromRecordset("[[rec(2"));
            Assert.AreEqual("", instance.ExtractIndexRegionFromRecordset("[[rec]]"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_MakeValueIntoHighLevelRecordset()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("rec(*)", instance.MakeValueIntoHighLevelRecordset("[[rec]]", true));
            Assert.AreEqual("rec()", instance.MakeValueIntoHighLevelRecordset("[[rec(]]", true));
            Assert.AreEqual("rec(*)", instance.MakeValueIntoHighLevelRecordset("[[rec)]]", true));
            Assert.AreEqual("rec()", instance.MakeValueIntoHighLevelRecordset("[[rec]]", false));
            Assert.AreEqual("rec()", instance.MakeValueIntoHighLevelRecordset("[[rec(]]", false));
            Assert.AreEqual("rec()", instance.MakeValueIntoHighLevelRecordset("[[rec)]]", false));
            Assert.AreEqual("rec(*)", instance.MakeValueIntoHighLevelRecordset("rec", true));
            Assert.AreEqual("rec()", instance.MakeValueIntoHighLevelRecordset("rec(", true));
            Assert.AreEqual("rec(*)", instance.MakeValueIntoHighLevelRecordset("rec)", true));
            Assert.AreEqual("rec()", instance.MakeValueIntoHighLevelRecordset("rec", false));
            Assert.AreEqual("rec()", instance.MakeValueIntoHighLevelRecordset("rec(", false));
            Assert.AreEqual("rec()", instance.MakeValueIntoHighLevelRecordset("rec)", false));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ExtractFieldNameOnlyFromValue()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("bab", instance.ExtractFieldNameOnlyFromValue("[[rec(*).bab]]"));
            Assert.AreEqual("ab", instance.ExtractFieldNameOnlyFromValue("rec(*).ab"));
            Assert.AreEqual("ab", instance.ExtractFieldNameOnlyFromValue("[[rec().ab]]"));
            Assert.AreEqual("ab", instance.ExtractFieldNameOnlyFromValue("[[rec().ab"));
            Assert.AreEqual("", instance.ExtractFieldNameOnlyFromValue("[[rec()."));
            Assert.AreEqual("", instance.ExtractFieldNameOnlyFromValue("[[rec()"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ExtractFieldNameFromValue()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("bab", instance.ExtractFieldNameFromValue("[[rec(*).bab]]"));
            Assert.AreEqual("ab", instance.ExtractFieldNameFromValue("rec(*).ab"));
            Assert.AreEqual("ab", instance.ExtractFieldNameFromValue("[[rec().ab]]"));
            Assert.AreEqual("ab", instance.ExtractFieldNameFromValue("[[rec().ab"));
            Assert.AreEqual("", instance.ExtractFieldNameFromValue("[[rec()."));
            Assert.AreEqual("", instance.ExtractFieldNameFromValue("[[rec()"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ExtractRecordsetNameFromValue()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("rec", instance.ExtractRecordsetNameFromValue("[[rec(*).bab]]"));
            Assert.AreEqual("rec", instance.ExtractRecordsetNameFromValue("rec(*).ab"));
            Assert.AreEqual("rec", instance.ExtractRecordsetNameFromValue("[[rec().ab]]"));
            Assert.AreEqual("rec", instance.ExtractRecordsetNameFromValue("[[rec().ab"));
            Assert.AreEqual("rec", instance.ExtractRecordsetNameFromValue("[[rec()."));
            Assert.AreEqual("rec", instance.ExtractRecordsetNameFromValue("[[rec()"));
            Assert.AreEqual("", instance.ExtractRecordsetNameFromValue("rec"));
            Assert.AreEqual("", instance.ExtractRecordsetNameFromValue("[[rec]]"));
            Assert.AreEqual("", instance.ExtractRecordsetNameFromValue(null));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_IsValueRecordsetWithFields()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual(false, instance.IsValueRecordsetWithFields(null));
            Assert.AreEqual(false, instance.IsValueRecordsetWithFields(""));
            Assert.AreEqual(false, instance.IsValueRecordsetWithFields("a"));
            Assert.AreEqual(false, instance.IsValueRecordsetWithFields("[[rec(*)]]"));
            Assert.AreEqual(false, instance.IsValueRecordsetWithFields("[[rec()]]"));
            Assert.AreEqual(true, instance.IsValueRecordsetWithFields("[[rec(*).a]]"));
            Assert.AreEqual(true, instance.IsValueRecordsetWithFields("[[rec(*).asdf]]"));
            Assert.AreEqual(true, instance.IsValueRecordsetWithFields("rec(*).a"));
            Assert.AreEqual(true, instance.IsValueRecordsetWithFields("rec(*).asdf"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_IsValueRecordset()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual(false, instance.IsValueRecordset(null));
            Assert.AreEqual(false, instance.IsValueRecordset(""));
            Assert.AreEqual(false, instance.IsValueRecordset("a"));
            Assert.AreEqual(true, instance.IsValueRecordset("[[rec(*)]]"));
            Assert.AreEqual(true, instance.IsValueRecordset("[[rec()]]"));
            Assert.AreEqual(true, instance.IsValueRecordset("[[rec(*).a]]"));
            Assert.AreEqual(true, instance.IsValueRecordset("[[rec(*).asdf]]"));
            Assert.AreEqual(true, instance.IsValueRecordset("rec(*).a"));
            Assert.AreEqual(true, instance.IsValueRecordset("rec(*).asdf"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ReplaceRecordsetIndexWithStar()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("[[rec(*)]]", instance.ReplaceRecordsetIndexWithStar("[[rec(*)]]"));
            Assert.AreEqual("[[rec(*)]]", instance.ReplaceRecordsetIndexWithStar("[[rec(2)]]"));
            Assert.AreEqual("[[rec(*).a]]", instance.ReplaceRecordsetIndexWithStar("[[rec(2).a]]"));
            Assert.AreEqual("rec(*).a", instance.ReplaceRecordsetIndexWithStar("rec(2).a"));
            Assert.AreEqual("[[rec(*)]]", instance.ReplaceRecordsetIndexWithStar("[[rec(a)]]"));
            Assert.AreEqual("[[rec()]]", instance.ReplaceRecordsetIndexWithStar("[[rec()]]"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ReplaceRecordsetIndexWithBlank()
        {
            var instance = new CommonRecordSetUtil();

            Assert.AreEqual("[[rec()]]", instance.ReplaceRecordsetIndexWithBlank("[[rec(*)]]"));
            Assert.AreEqual("[[rec()]]", instance.ReplaceRecordsetIndexWithBlank("[[rec(2)]]"));
            Assert.AreEqual("[[rec().a]]", instance.ReplaceRecordsetIndexWithBlank("[[rec(2).a]]"));
            Assert.AreEqual("rec().a", instance.ReplaceRecordsetIndexWithBlank("rec(2).a"));
            Assert.AreEqual("[[rec()]]", instance.ReplaceRecordsetIndexWithBlank("[[rec(a)]]"));
            Assert.AreEqual("[[rec()]]", instance.ReplaceRecordsetIndexWithBlank("[[rec()]]"));
            Assert.AreEqual("()", instance.ReplaceRecordsetIndexWithBlank("[[rec)(]]"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_RemoveRecordSetBraces()
        {
            var instance = new CommonRecordSetUtil();
            var boolV = false;
            Assert.AreEqual("rec", instance.RemoveRecordSetBraces("rec(*)", ref boolV));
            Assert.IsTrue(boolV);
            boolV = false;
            Assert.AreEqual("rec", instance.RemoveRecordSetBraces("rec()", ref boolV));
            Assert.IsTrue(boolV);
            boolV = false;
            Assert.AreEqual("a", instance.RemoveRecordSetBraces("a", ref boolV));
            Assert.IsFalse(boolV);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ProcessRecordSetFields()
        {
            var instance = new CommonRecordSetUtil();

            var payload = new Mock<IParseTO>().Object;
            bool addCompleteParts = false;
            var result = new List<IIntellisenseResult>();
            var mockChild = new Mock<IDev2DataLanguageIntellisensePart>();
            mockChild.Setup(o => o.Name).Returns("part1");
            mockChild.Setup(o => o.Description).Returns("mockchildintellip");
            var mockIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockIntellisensePart.Setup(o => o.Description).Returns("mockintellip1");
            mockIntellisensePart.Setup(o => o.Children).Returns(new List<IDev2DataLanguageIntellisensePart>
            {
                mockChild.Object,
            });
            var intellisensePart = mockIntellisensePart.Object;

            instance.ProcessRecordSetFields(payload, addCompleteParts, result, intellisensePart);

            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("mockintellip1 / Select a specific row or Close", result[0].Message);
            Assert.AreEqual("mockintellip1 / Takes all rows ", result[1].Message);
            Assert.AreEqual("mockintellip1 / Take last row", result[2].Message);
            Assert.AreEqual("mockintellip1 / Use the field of a Recordset", result[3].Message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ProcessRecordSetFields_AddCompleteParts()
        {
            var instance = new CommonRecordSetUtil();

            var payload = new Mock<IParseTO>().Object;
            bool addCompleteParts = true;
            var result = new List<IIntellisenseResult>();
            var mockChild = new Mock<IDev2DataLanguageIntellisensePart>();
            mockChild.Setup(o => o.Name).Returns("part1");
            mockChild.Setup(o => o.Description).Returns("mockchildintellip");
            var mockIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockIntellisensePart.Setup(o => o.Description).Returns("mockintellip1");
            mockIntellisensePart.Setup(o => o.Children).Returns(new List<IDev2DataLanguageIntellisensePart>
            {
                mockChild.Object,
            });
            var intellisensePart = mockIntellisensePart.Object;

            instance.ProcessRecordSetFields(payload, addCompleteParts, result, intellisensePart);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("mockintellip1 / Takes all rows ", result[0].Message);
            Assert.AreEqual("mockintellip1 / Take last row", result[1].Message);
            Assert.AreEqual("mockintellip1 / Use the field of a Recordset", result[2].Message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ProcessNonRecordsetFieldsWithParent()
        {
            var instance = new CommonRecordSetUtil();

            var mockPayload = new Mock<IParseTO>();
            var payload = mockPayload.Object;
            var result = new List<IIntellisenseResult>();

            var mockParent = new Mock<IParseTO>();
            mockParent.Setup(o => o.Payload).Returns("rec()");
            mockPayload.Setup(o => o.Parent).Returns(mockParent.Object);

            var mockIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockIntellisensePart.Setup(o => o.Name).Returns("mockintellip1Name");
            mockIntellisensePart.Setup(o => o.Description).Returns("mockintellip1Desc");
            var intellisensePart = mockIntellisensePart.Object;

            instance.ProcessNonRecordsetFields(payload, result, intellisensePart);

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual("mockintellip1Desc / Use row at this index", result[0].Message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ProcessNonRecordsetFieldsWithNonRecordsetParent()
        {
            var instance = new CommonRecordSetUtil();

            var mockPayload = new Mock<IParseTO>();
            var payload = mockPayload.Object;
            var result = new List<IIntellisenseResult>();

            var mockParent = new Mock<IParseTO>();
            mockParent.Setup(o => o.Payload).Returns("somename");
            mockPayload.Setup(o => o.Parent).Returns(mockParent.Object);

            var mockIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockIntellisensePart.Setup(o => o.Name).Returns("mockintellip1Name");
            mockIntellisensePart.Setup(o => o.Description).Returns("mockintellip1Desc");
            var intellisensePart = mockIntellisensePart.Object;

            instance.ProcessNonRecordsetFields(payload, result, intellisensePart);

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual("mockintellip1Desc", result[0].Message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ProcessNonRecordsetFields()
        {
            var instance = new CommonRecordSetUtil();

            var mockPayload = new Mock<IParseTO>();
            var payload = mockPayload.Object;
            var result = new List<IIntellisenseResult>();

            var mockIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockIntellisensePart.Setup(o => o.Name).Returns("mockintellip1Name");
            mockIntellisensePart.Setup(o => o.Description).Returns("mockintellip1Desc");
            var intellisensePart = mockIntellisensePart.Object;

            instance.ProcessNonRecordsetFields(payload, result, intellisensePart);

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual("mockintellip1Desc", result[0].Message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ProcessRecordSetMatch()
        {
            var instance = new CommonRecordSetUtil();

            var mockChild = new Mock<IParseTO>();
            mockChild.Setup(o => o.Payload).Returns("childpayload");

            var mockPayload = new Mock<IParseTO>();
            mockPayload.Setup(o => o.Payload).Returns("recset");
            mockPayload.Setup(o => o.IsLeaf).Returns(false);
            mockPayload.Setup(o => o.Child).Returns(mockChild.Object);
            var payload = mockPayload.Object;
            var result = new List<IIntellisenseResult>();

            var mockIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockIntellisensePart.Setup(o => o.Name).Returns("mockintellip1Name");
            mockIntellisensePart.Setup(o => o.Description).Returns("mockintellip1Desc");
            var mockChildIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockChildIntellisensePart.Setup(o => o.Name).Returns("childintellipartName");
            mockChildIntellisensePart.Setup(o => o.Description).Returns("childintellipartDesc");
            mockIntellisensePart.Setup(o => o.Children).Returns(new List<IDev2DataLanguageIntellisensePart>
            {
                mockChildIntellisensePart.Object,
            });
            var intellisensePart = mockIntellisensePart.Object;

            instance.ProcessRecordSetMatch(payload, result, "rawrec", "searchrec", intellisensePart);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("mockintellip1Desc / Select a specific row", result[0].Message);
            Assert.AreEqual("childintellipartDesc / Select a specific field at a specific row", result[1].Message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_ProcessRecordSetMatch_NoChildren()
        {
            var instance = new CommonRecordSetUtil();

            var mockChild = new Mock<IParseTO>();
            mockChild.Setup(o => o.Payload).Returns("childpayload");

            var mockPayload = new Mock<IParseTO>();
            mockPayload.Setup(o => o.Payload).Returns("recset");
            mockPayload.Setup(o => o.IsLeaf).Returns(true);
            mockPayload.Setup(o => o.Child).Returns(mockChild.Object);
            var payload = mockPayload.Object;
            var result = new List<IIntellisenseResult>();

            var mockIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockIntellisensePart.Setup(o => o.Name).Returns("mockintellip1Name");
            mockIntellisensePart.Setup(o => o.Description).Returns("mockintellip1Desc");
            var mockChildIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockChildIntellisensePart.Setup(o => o.Name).Returns("childintellipartName");
            mockChildIntellisensePart.Setup(o => o.Description).Returns("childintellipartDesc");
            mockIntellisensePart.Setup(o => o.Children).Returns(new List<IDev2DataLanguageIntellisensePart>
            {
                mockChildIntellisensePart.Object,
            });
            var intellisensePart = mockIntellisensePart.Object;

            instance.ProcessRecordSetMatch(payload, result, "rawrec", "searchrec", intellisensePart);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("mockintellip1Desc / Select a specific row", result[0].Message);
            Assert.AreEqual("childintellipartDesc / Select a specific field at a specific row", result[1].Message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_AddRecordSetIndex()
        {
            var instance = new CommonRecordSetUtil();

            var mockChild = new Mock<IParseTO>();
            mockChild.Setup(o => o.Payload).Returns("childpayload");

            var mockPayload = new Mock<IParseTO>();
            mockPayload.Setup(o => o.Payload).Returns("recset");
            mockPayload.Setup(o => o.IsLeaf).Returns(true);
            mockPayload.Setup(o => o.Child).Returns(mockChild.Object);
            var payload = mockPayload.Object;
            var result = new List<IIntellisenseResult>();

            var mockIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockIntellisensePart.Setup(o => o.Name).Returns("mockintellip1Name");
            mockIntellisensePart.Setup(o => o.Description).Returns("mockintellip1Desc");
            var mockChildIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockChildIntellisensePart.Setup(o => o.Name).Returns("childintellipartName");
            mockChildIntellisensePart.Setup(o => o.Description).Returns("childintellipartDesc");
            mockIntellisensePart.Setup(o => o.Children).Returns(new List<IDev2DataLanguageIntellisensePart>
            {
                mockChildIntellisensePart.Object,
            });
            var intellisensePart = mockIntellisensePart.Object;

            bool addCompleteParts = false;
            string[] parts = { "rec(1)" };
            bool emptyOk = false;

            Assert.AreEqual(true, instance.AddRecordSetIndex(payload, addCompleteParts, result, parts, intellisensePart, emptyOk));
            Assert.AreEqual(0, result.Count);

            addCompleteParts = true;
            Assert.AreEqual(false, instance.AddRecordSetIndex(payload, addCompleteParts, result, parts, intellisensePart, emptyOk));

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("mockintellip1Desc", result[0].Message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_RecordsetMatch()
        {
            var instance = new CommonRecordSetUtil();

            var mockChild = new Mock<IParseTO>();
            mockChild.Setup(o => o.Payload).Returns("childpayload");

            var mockPayload = new Mock<IParseTO>();
            mockPayload.Setup(o => o.HangingOpen).Returns(true);
            mockPayload.Setup(o => o.Payload).Returns("recset");
            mockPayload.Setup(o => o.IsLeaf).Returns(true);
            mockPayload.Setup(o => o.Child).Returns(mockChild.Object);
            var payload = mockPayload.Object;
            var result = new List<IIntellisenseResult>();

            var mockIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockIntellisensePart.Setup(o => o.Name).Returns("mockintellip1Name");
            mockIntellisensePart.Setup(o => o.Description).Returns("mockintellip1Desc");
            var mockChildIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockChildIntellisensePart.Setup(o => o.Name).Returns("childintellipartName");
            mockChildIntellisensePart.Setup(o => o.Description).Returns("childintellipartDesc");
            mockIntellisensePart.Setup(o => o.Children).Returns(new List<IDev2DataLanguageIntellisensePart>
            {
                mockChildIntellisensePart.Object,
            });
            var intellisensePart = mockIntellisensePart.Object;

            bool addCompleteParts = false;
            string[] parts = { "rec(1)" };
            bool emptyOk = false;

            Assert.AreEqual(false, instance.RecordsetMatch(payload, addCompleteParts, result, "rawsearch", "search", emptyOk, parts, intellisensePart));

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("mockintellip1Desc / Select a specific row", result[0].Message);
            Assert.AreEqual("childintellipartDesc / Select a specific field at a specific row", result[1].Message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_RecordsetMatch_NoHangingOpen()
        {
            var instance = new CommonRecordSetUtil();

            var mockChild = new Mock<IParseTO>();
            mockChild.Setup(o => o.Payload).Returns("childpayload");

            var mockPayload = new Mock<IParseTO>();
            mockPayload.Setup(o => o.HangingOpen).Returns(false);
            mockPayload.Setup(o => o.Payload).Returns("recset");
            mockPayload.Setup(o => o.IsLeaf).Returns(true);
            mockPayload.Setup(o => o.Child).Returns(mockChild.Object);
            var payload = mockPayload.Object;
            var result = new List<IIntellisenseResult>();

            var mockIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockIntellisensePart.Setup(o => o.Name).Returns("mockintellip1Name");
            mockIntellisensePart.Setup(o => o.Description).Returns("mockintellip1Desc");
            var mockChildIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockChildIntellisensePart.Setup(o => o.Name).Returns("childintellipartName");
            mockChildIntellisensePart.Setup(o => o.Description).Returns("childintellipartDesc");
            mockIntellisensePart.Setup(o => o.Children).Returns(new List<IDev2DataLanguageIntellisensePart>
            {
                mockChildIntellisensePart.Object,
            });
            var intellisensePart = mockIntellisensePart.Object;

            bool addCompleteParts = false;
            string[] parts = { "rec(1)" };
            bool emptyOk = false;

            Assert.AreEqual(true, instance.RecordsetMatch(payload, addCompleteParts, result, "rawsearch", "search", emptyOk, parts, intellisensePart));

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_OpenRecordsetItem()
        {
            var instance = new CommonRecordSetUtil();

            var mockChild = new Mock<IParseTO>();
            mockChild.Setup(o => o.Payload).Returns("childpayload");

            var mockPayload = new Mock<IParseTO>();
            mockPayload.Setup(o => o.HangingOpen).Returns(false);
            mockPayload.Setup(o => o.Payload).Returns("recset");
            mockPayload.Setup(o => o.IsLeaf).Returns(true);
            mockPayload.Setup(o => o.Child).Returns(mockChild.Object);
            var payload = mockPayload.Object;
            var result = new List<IIntellisenseResult>();

            var mockIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockIntellisensePart.Setup(o => o.Name).Returns("mockintellip1Name");
            mockIntellisensePart.Setup(o => o.Description).Returns("mockintellip1Desc");
            var mockChildIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockChildIntellisensePart.Setup(o => o.Name).Returns("childintellipartName");
            mockChildIntellisensePart.Setup(o => o.Description).Returns("childintellipartDesc");
            mockIntellisensePart.Setup(o => o.Children).Returns(new List<IDev2DataLanguageIntellisensePart>
            {
                mockChildIntellisensePart.Object,
            });
            var intellisensePart = mockIntellisensePart.Object;

            bool addCompleteParts = false;
            string[] parts = { "rec(1)" };
            bool emptyOk = false;

            instance.OpenRecordsetItem(payload, result, intellisensePart);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(" / Select a specific row", result[0].Message);
            Assert.AreEqual(" / Select a specific row", result[1].Message);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonRecordSetUtil))]
        public void CommonRecordSetUtil_OpenRecordsetItem_MalformedIndex()
        {
            var instance = new CommonRecordSetUtil();

            var mockChild = new Mock<IParseTO>();
            mockChild.Setup(o => o.Payload).Returns("childpayload()");

            var mockPayload = new Mock<IParseTO>();
            mockPayload.Setup(o => o.HangingOpen).Returns(false);
            mockPayload.Setup(o => o.Payload).Returns("recset(");
            mockPayload.Setup(o => o.IsLeaf).Returns(true);
            mockPayload.Setup(o => o.Child).Returns(mockChild.Object);
            var payload = mockPayload.Object;
            var result = new List<IIntellisenseResult>();

            var mockIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockIntellisensePart.Setup(o => o.Name).Returns("mockintellip1Name");
            mockIntellisensePart.Setup(o => o.Description).Returns("mockintellip1Desc");
            var mockChildIntellisensePart = new Mock<IDev2DataLanguageIntellisensePart>();
            mockChildIntellisensePart.Setup(o => o.Name).Returns("childintellipartName");
            mockChildIntellisensePart.Setup(o => o.Description).Returns("childintellipartDesc");
            mockIntellisensePart.Setup(o => o.Children).Returns(new List<IDev2DataLanguageIntellisensePart>
            {
                mockChildIntellisensePart.Object,
            });
            var intellisensePart = mockIntellisensePart.Object;

            bool addCompleteParts = false;
            string[] parts = { "rec(1)" };
            bool emptyOk = false;

            instance.OpenRecordsetItem(payload, result, intellisensePart);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("[[recset([[childpayload(]])]]", result[0].Option.DisplayValue);
            Assert.AreEqual("[[recset([[childpayload(]]).childintellipartName]]", result[1].Option.DisplayValue);
        }
    }
}
