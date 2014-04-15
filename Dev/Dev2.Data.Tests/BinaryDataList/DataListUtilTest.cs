using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common;
using Dev2.Common.StringTokenizer.Interfaces;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Data.Tests.BinaryDataList
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataListUtilTest
    {

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
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_IsXML")]
        public void DataListUtil_IsXML_CheckIsXML_ExpectTrue()
        {
            //------------Setup for test--------------------------
            const string defs = @"<Outputs><Output Name=""TableCountry"" MapsTo=""TableCountry"" Value=""[[string_NewDataSet().TableCountry]]"" Recordset=""string_NewDataSet"" /><Output Name=""TableCity"" MapsTo=""[[City]]"" Value=""[[City]]"" Recordset=""string_NewDataSet"" /></Outputs>";

            //------------Execute Test---------------------------
            bool isFragment;
            bool isHTML;
            var result = DataListUtil.IsXml(defs, out isFragment, out isHTML);

            //------------Assert Results-------------------------

            Assert.IsTrue(result);
            Assert.IsFalse(isFragment);
            Assert.IsFalse(isHTML);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_IsXML")]
        public void DataListUtil_IsXML_CheckIsFragment_ExpectTrue()
        {
            //------------Setup for test--------------------------
            const string defs = @"<x><a></a></x><x></x><x></x>";

            //------------Execute Test---------------------------
            bool isFragment;
            bool isHTML;
            var result = DataListUtil.IsXml(defs, out isFragment, out isHTML);

            //------------Assert Results-------------------------

            Assert.IsTrue(result);
            Assert.IsTrue(isFragment);
            Assert.IsFalse(isHTML);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_IsXML")]
        public void DataListUtil_IsXML_CheckIsHTML_ExpectTrue()
        {
            //------------Setup for test--------------------------
            const string defs = @"<html></html>";

            //------------Execute Test---------------------------
            bool isFragment;
            bool isHTML;
            var result = DataListUtil.IsXml(defs, out isFragment, out isHTML);

            //------------Assert Results-------------------------

            Assert.IsFalse(result);
            Assert.IsFalse(isFragment);
            Assert.IsTrue(isHTML);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_IsXML")]
        public void DataListUtil_IsXML_CheckIsHTMLWhenDocTypePresent_ExpectTrue()
        {
            //------------Setup for test--------------------------
            const string defs = @"<!DOCTYPE html><html></html>";

            //------------Execute Test---------------------------
            bool isFragment;
            bool isHTML;
            var result = DataListUtil.IsXml(defs, out isFragment, out isHTML);

            //------------Assert Results-------------------------

            Assert.IsFalse(result);
            Assert.IsFalse(isFragment);
            Assert.IsTrue(isHTML);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_ShapeDefsToDataList")]
        public void DataListUtil_ShapeDefinitionsToDataList_EnsureNameValueDoesNotFlipFlot_ExpectValidXML()
        {
            //------------Setup for test--------------------------
            const string defs = @"<Outputs><Output Name=""TableCountry"" MapsTo=""TableCountry"" Value=""[[string_NewDataSet().TableCountry]]"" Recordset=""string_NewDataSet"" /><Output Name=""TableCity"" MapsTo=""[[City]]"" Value=""[[City]]"" Recordset=""string_NewDataSet"" /></Outputs>";

            //------------Execute Test---------------------------
            ErrorResultTO invokeErrors;
            var result = DataListUtil.ShapeDefinitionsToDataList(defs, enDev2ArgumentType.Output, out invokeErrors);

            //------------Assert Results-------------------------
            const string expected = @"<ADL>
<string_NewDataSet>
	<TableCountry></TableCountry>
	<TableCity></TableCity>
</string_NewDataSet>

</ADL>";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_ShapeDefsToDataList")]
        public void DataListUtil_ShapeDefinitionsToDataList_WhenOutputsContainsRecordsetsBothSides_ExpectValidXML()
        {
            //------------Setup for test--------------------------
            const string defs = @"<Outputs><Output Name=""CountryID"" MapsTo=""CountryID"" Value=""[[dbo_spGetCountries().CountryID]]"" Recordset=""dbo_spGetCountries"" /><Output Name=""Description"" MapsTo=""Description"" Value=""[[dbo_spGetCountries().Description]]"" Recordset=""dbo_spGetCountries"" /></Outputs>";

            //------------Execute Test---------------------------
            ErrorResultTO invokeErrors;
            var result = DataListUtil.ShapeDefinitionsToDataList(defs, enDev2ArgumentType.Output, out invokeErrors);

            //------------Assert Results-------------------------
            const string expected = @"<ADL>
<dbo_spGetCountries>
	<CountryID></CountryID>
	<Description></Description>
</dbo_spGetCountries>

</ADL>";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_ShapeDefsToDataList")]
        public void DataListUtil_ShapeDefinitionsToDataList_WhenInputContainsStaticData_ExpectValidXML1()
        {
            //------------Setup for test--------------------------
            const string defs = @"<Inputs><Input Name=""r1"" Source=""[[InjectValue]]"" DefaultValue=""5""><Validator Type=""Required"" /></Input></Inputs>";

            //------------Execute Test---------------------------
            ErrorResultTO invokeErrors;
            var result = DataListUtil.ShapeDefinitionsToDataList(defs, enDev2ArgumentType.Input, out invokeErrors);

            //------------Assert Results-------------------------
            const string expected = @"<ADL>
<r1></r1>

</ADL>";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_ShapeDefsToDataList")]
        public void DataListUtil_ShapeDefinitionsToDataList_WhenInputContainsStaticData_ExpectValidXML()
        {
            //------------Setup for test--------------------------
            const string defs = @"<Inputs><Input Name=""isUpper"" Source=""0""><Validator Type=""Required"" /></Input></Inputs>";

            //------------Execute Test---------------------------
            ErrorResultTO invokeErrors;
            var result = DataListUtil.ShapeDefinitionsToDataList(defs, enDev2ArgumentType.Input, out invokeErrors);

            //------------Assert Results-------------------------
            const string expected = @"<ADL>
<isUpper></isUpper>

</ADL>";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_ShapeDefsToDataList")]
        public void DataListUtil_ShapeDefinitionsToDataList_WhenOutputContainsRecordset_ExpectValidXML()
        {
            //------------Setup for test--------------------------
            const string defs = @"<Outputs><Output Name=""result"" MapsTo=""result"" Value=""[[result]]"" Recordset=""dbo_NullReturnsZZZ_NotNullReturnsAAA"" /></Outputs>";

            //------------Execute Test---------------------------
            ErrorResultTO invokeErrors;
            var result = DataListUtil.ShapeDefinitionsToDataList(defs, enDev2ArgumentType.Output, out invokeErrors);

            //------------Assert Results-------------------------
            const string expected = @"<ADL>
<dbo_NullReturnsZZZ_NotNullReturnsAAA>
	<result></result>
</dbo_NullReturnsZZZ_NotNullReturnsAAA>

</ADL>";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_ShapeDefsToDataList")]
        public void DataListUtil_ShapeDefinitionsToDataList_WhenInputContainsScalar_ExpectValidXML()
        {
            //------------Setup for test--------------------------
            const string defs = @"<Inputs><Input Name=""Prefix"" Source=""[[foobar]]"" /></Inputs>";

            //------------Execute Test---------------------------
            ErrorResultTO invokeErrors;
            var result = DataListUtil.ShapeDefinitionsToDataList(defs, enDev2ArgumentType.Input, out invokeErrors);

            //------------Assert Results-------------------------
            const string expected = @"<ADL>
<Prefix></Prefix>

</ADL>";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_ShapeDefsToDataList")]
        public void DataListUtil_ShapeDefinitionsToDataList_WhenInputContainsRecordsetWithStar_ExpectValidXML()
        {
            //------------Setup for test--------------------------
            const string defs = @"<Inputs><Input Name=""Prefix"" Source=""[[prefix(*).val]]"" /></Inputs>";

            //------------Execute Test---------------------------
            ErrorResultTO invokeErrors;
            var result = DataListUtil.ShapeDefinitionsToDataList(defs, enDev2ArgumentType.Input, out invokeErrors);

            //------------Assert Results-------------------------
            const string expected = @"<ADL>
<Prefix></Prefix>
<prefix>
	<val></val>
</prefix>

</ADL>";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_ShapeDefsToDataList")]
        public void DataListUtil_ShapeDefinitionsToDataList_WhenOutputContainsDifferentRecordset_ExpectTwoRecordsets()
        {
            //------------Setup for test--------------------------
            const string defs = @"<Outputs><Output Name=""MapLocationID"" MapsTo=""[[MapLocationID]]"" Value=""[[dbo_proc_GetAllMapLocations(*).MapLocationID]]"" Recordset=""dbo_proc_GetAllMapLocations"" /><Output Name=""StreetAddress"" MapsTo=""[[StreetAddress]]"" Value=""[[dbo_proc_GetAllMapLocations2(*).StreetAddress]]"" Recordset=""dbo_proc_GetAllMapLocations"" /><Output Name=""Latitude"" MapsTo=""[[Latitude]]"" Value=""[[dbo_proc_GetAllMapLocations(*).Latitude]]"" Recordset=""dbo_proc_GetAllMapLocations"" /><Output Name=""Longitude"" MapsTo=""[[Longitude]]"" Value=""[[dbo_proc_GetAllMapLocations(*).Longitude]]"" Recordset=""dbo_proc_GetAllMapLocations"" /></Outputs>";

            //------------Execute Test---------------------------
            ErrorResultTO invokeErrors;
            var result = DataListUtil.ShapeDefinitionsToDataList(defs, enDev2ArgumentType.Output, out invokeErrors);

            //------------Assert Results-------------------------
            const string expected = @"<ADL>
<dbo_proc_GetAllMapLocations>
	<MapLocationID></MapLocationID>
	<Latitude></Latitude>
	<Longitude></Longitude>
</dbo_proc_GetAllMapLocations>
<dbo_proc_GetAllMapLocations2>
	<StreetAddress></StreetAddress>
</dbo_proc_GetAllMapLocations2>

</ADL>";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListUtil_ShapeDefsToDataList")]
        public void DataListUtil_ShapeDefinitionsToDataList_WhenOutputContainsOneRecordset_ExpectOneRecordset()
        {
            //------------Setup for test--------------------------
            const string defs = @"<Outputs><Output Name=""MapLocationID"" MapsTo=""[[MapLocationID]]"" Value=""[[dbo_proc_GetAllMapLocations(*).MapLocationID]]"" Recordset=""dbo_proc_GetAllMapLocations"" /><Output Name=""StreetAddress"" MapsTo=""[[StreetAddress]]"" Value=""[[dbo_proc_GetAllMapLocations(*).StreetAddress]]"" Recordset=""dbo_proc_GetAllMapLocations"" /><Output Name=""Latitude"" MapsTo=""[[Latitude]]"" Value=""[[dbo_proc_GetAllMapLocations(*).Latitude]]"" Recordset=""dbo_proc_GetAllMapLocations"" /><Output Name=""Longitude"" MapsTo=""[[Longitude]]"" Value=""[[dbo_proc_GetAllMapLocations(*).Longitude]]"" Recordset=""dbo_proc_GetAllMapLocations"" /></Outputs>";

            //------------Execute Test---------------------------
            ErrorResultTO invokeErrors;
            var result = DataListUtil.ShapeDefinitionsToDataList(defs, enDev2ArgumentType.Output, out invokeErrors);

            //------------Assert Results-------------------------
            const string expected = @"<ADL>
<dbo_proc_GetAllMapLocations>
	<MapLocationID></MapLocationID>
	<StreetAddress></StreetAddress>
	<Latitude></Latitude>
	<Longitude></Longitude>
</dbo_proc_GetAllMapLocations>

</ADL>";
            Assert.AreEqual(expected, result);
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
            DataListUtil.UpsertTokens(target, tokenizer.Object, tokenPrefix: "rs(*).", tokenSuffix: "a", removeEmptyEntries: false);

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
            DataListUtil.UpsertTokens(target, tokenizer.Object, tokenPrefix: "rs(*).", tokenSuffix: "a");

            //------------Assert Results-------------------------
            const int ExpectedCount = TokenCount - 2;

            Assert.AreEqual(ExpectedCount, target.Count);

            for(var i = 0; i < ExpectedCount; i++)
            {
                Assert.AreEqual(string.Format("[[rs(*).f{0}a]]", i + 1), target[i].Key);
            }
        }
    }
}
