
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
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Data.Binary_Objects;
using Dev2.Data.DataListCache;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.DynamicServices.Test;
using Dev2.Server.DataList;
using Dev2.Server.DataList.Translators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.BinaryDataList
{
    /// <summary>
    /// Summary description for ServerTest
    /// </summary>
    [TestClass]    
    public class ServerTest
    {
        private static readonly IDataListServer Dls = new DataListServer(DataListPersistenceProviderFactory.CreateMemoryProvider());
        private static readonly DataListFormat XmlFormat = DataListFormat.CreateFormat(GlobalConstants._XML);
        private static readonly DataListFormat XmlFormatWithoutSystemTags = DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags);
        private static readonly DataListFormat XmlFormatInputsOnly = DataListFormat.CreateFormat(GlobalConstants._XML_Inputs_Only);

        readonly StringBuilder _dataListWellformed = "<DataList><scalar1/><rs1><f1/><f2/></rs1><scalar2/></DataList>".ToStringBuilder();
        const string DataListWellformedData = "<DataList><scalar1>s1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><scalar2/></DataList>";
        readonly StringBuilder _dataListWellformedMult = "<DataList><scalar1/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>".ToStringBuilder();
        readonly StringBuilder _dataListMalformed = "<DataList><scalar1/><rs1><f1/><f2/><f3/><scalar2/></DataList>".ToStringBuilder();
        readonly StringBuilder _dataListWellformedDescAttributes = "<DataList><scalar1 Description=\"Test scalar description\"/><rs1 Description=\"Test recordset desciption\"><f1 Description=\"Test field1 desciption\"/></rs1><scalar2/></DataList>".ToStringBuilder();
        const string DataListWellformedDataWithDesc = "<DataList><scalar1 Description=\"Test scalar description\"/>s1</scalar1><rs1 Description=\"Test recordset desciption\"><f1 Description=\"Test field1 desciption\">f1.1</f1></rs1><rs1 Description=\"Test recordset desciption\"><f1 Description=\"Test field1 desciption\">f1.2</f1></rs1><scalar2/></DataList>";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            // boot strap the server
            DataListTranslatorFactory dltf = new DataListTranslatorFactory();
            Dls.AddTranslator(dltf.FetchTranslator(DataListFormat.CreateFormat(GlobalConstants._BINARY)));
            Dls.AddTranslator(dltf.FetchTranslator(DataListFormat.CreateFormat(GlobalConstants._XML)));
            Dls.AddTranslator(dltf.FetchTranslator(DataListFormat.CreateFormat(GlobalConstants._JSON)));
            Dls.AddTranslator(dltf.FetchTranslator(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags)));
            Dls.AddTranslator(dltf.FetchTranslator(DataListFormat.CreateFormat(GlobalConstants._XML_Inputs_Only)));

        }
        #endregion

        #region Positive Test

        // ReSharper disable InconsistentNaming

        [TestMethod]
        public void Fetch_TranslationTypes_ExpectAddTypes()
        {

            IList<DataListFormat> formats = Dls.FetchTranslatorTypes();

            Assert.AreEqual(5, formats.Count);
            Assert.AreEqual(GlobalConstants._BINARY, formats[0].FormatName);
            Assert.AreEqual(GlobalConstants._XML, formats[1].FormatName);
            Assert.AreEqual(GlobalConstants._JSON, formats[2].FormatName);
            Assert.AreEqual(GlobalConstants._XML_Without_SystemTags, formats[3].FormatName);
            Assert.AreEqual(GlobalConstants._XML_Inputs_Only, formats[4].FormatName);
        }

        #endregion

        #region XMLTest

        #region SerializeToXML Tests

        [TestMethod]
        public void SerializeToXML_ValidXML_Expect_Sucess()
        {
            ErrorResultTO errors;
            string error;
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormat);
            byte[] data = (TestHelper.ConvertStringToByteArray(DataListWellformedData));
            IBinaryDataList obj = xmlConverter.ConvertTo(data, _dataListWellformed, out errors);

            IBinaryDataListEntry entry;
            if(obj.TryGetEntry("rs1", out entry, out error))
            {
                IList<IBinaryDataListItem> cols = entry.FetchRecordAt(1, out error);
                int systemTagCount = Enum.GetValues(typeof(enSystemTag)).Length;

                var res1 = obj.FetchAllEntries().Count;
                var res2 = obj.FetchScalarEntries().Count;
                var res3 = obj.FetchRecordsetEntries().Count;
                var res4 = cols.Count;

                Assert.AreEqual(3 + systemTagCount, res1);
                Assert.AreEqual(2 + systemTagCount, res2);
                Assert.AreEqual(1, res3);
                Assert.AreEqual(2, res4);
            }
            else
            {
                Assert.Fail("Error");
            }
        }

        [TestMethod]
        public void SerializeToXML_ValidXMLMultRecordsets_Expect_Sucess()
        {
            ErrorResultTO errors;
            string error;
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormat);
            byte[] data = (TestHelper.ConvertStringToByteArray(DataListWellformedData));
            IBinaryDataList obj = xmlConverter.ConvertTo(data, _dataListWellformedMult, out errors);

            IBinaryDataListEntry entry;

            if(obj.TryGetEntry("rs1", out entry, out error))
            {
                IList<IBinaryDataListItem> cols = entry.FetchRecordAt(1, out error);
                int systemTagCount = Enum.GetValues(typeof(enSystemTag)).Length;


                var res1 = obj.FetchAllEntries().Count;
                var res2 = obj.FetchScalarEntries().Count;
                var res3 = obj.FetchRecordsetEntries().Count;
                var res4 = cols.Count;

                Assert.AreEqual(4 + systemTagCount, res1);
                Assert.AreEqual(2 + systemTagCount, res2);
                Assert.AreEqual(2, res3);
                Assert.AreEqual(2, res4);
            }
            else
            {
                Assert.Fail("Error");
            }
        }

        [TestMethod]
        public void SerializeToXML_ValidXMLWithDescriptions_Expect_Sucess()
        {
            string error;
            ErrorResultTO errors;
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormat);
            byte[] data = (TestHelper.ConvertStringToByteArray(DataListWellformedData));
            IBinaryDataList obj = xmlConverter.ConvertTo(data, _dataListWellformedDescAttributes, out errors);

            IBinaryDataListEntry entry;

            if(obj.TryGetEntry("rs1", out entry, out error))
            {
                IList<IBinaryDataListItem> cols = entry.FetchRecordAt(1, out error);
                int systemTagCount = Enum.GetValues(typeof(enSystemTag)).Length;

                var res1 = obj.FetchAllEntries().Count;
                var res2 = obj.FetchScalarEntries().Count;
                var res3 = obj.FetchRecordsetEntries().Count;
                var res4 = cols.Count;

                Assert.AreEqual(3 + systemTagCount, res1);
                Assert.AreEqual(2 + systemTagCount, res2);
                Assert.AreEqual(1, res3);
                Assert.AreEqual(1, res4);

            }
            else
            {
                Assert.Fail("Error");
            }
        }

        [TestMethod]
        public void SerializeToXML_ValidXMLWithDescriptions_Return_Decriptions_Expect_Sucess()
        {
            ErrorResultTO errors;
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormat);
            byte[] data = (TestHelper.ConvertStringToByteArray(DataListWellformedDataWithDesc));
            IBinaryDataList obj = xmlConverter.ConvertTo(data, _dataListWellformedDescAttributes, out errors);

            IList<IBinaryDataListEntry> scalars = obj.FetchScalarEntries();
            IList<IBinaryDataListEntry> recordsets = obj.FetchRecordsetEntries();

            var res1 = scalars[0].Description;
            var res2 = recordsets[0].Columns[0].ColumnDescription;

            Assert.AreEqual("Test scalar description", res1);
            Assert.AreEqual("Test field1 desciption", res2);
        }

        [TestMethod]
        public void SerializeToXML_ValidXML_BlankXML_Expect_Blank_Values_Returned()
        {
            string error;
            ErrorResultTO errors;
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormat);
            byte[] data = (TestHelper.ConvertStringToByteArray(""));
            IBinaryDataList obj = xmlConverter.ConvertTo(data, _dataListWellformedDescAttributes, out errors);

            IList<IBinaryDataListEntry> scalars = obj.FetchScalarEntries();
            IList<IBinaryDataListEntry> recordsets = obj.FetchRecordsetEntries();
            IBinaryDataListItem item = scalars[0].FetchScalar();
            IList<IBinaryDataListItem> items = recordsets[0].FetchRecordAt(1, out error);

            Assert.IsTrue(item != null && items != null);

        }

        [TestMethod]
        public void SerializeToXML_InValidXML_Expect_Error()
        {
            ErrorResultTO errors;
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormat);
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListMalformed.ToString()));
            xmlConverter.ConvertTo(data, _dataListMalformed, out errors);

            // convert fails, hence no datalist cleanup ;)

            Assert.AreEqual("The 'rs1' start tag on line 1 position 22 does not match the end tag of 'DataList'. Line 1, position 53.", errors.FetchErrors()[0]);
        }

        #endregion

        #region DeserializeToXMLFromBinary Tests
        [TestMethod]
        public void DeSerializeToXMLFromBinary_ValidXML_Expect_Sucess()
        {
            IBinaryDataList dl = Dev2BinaryDataListFactory.CreateDataList();
            List<Dev2Column> cols = new List<Dev2Column>
                {
                    DataListFactory.CreateDev2Column("f1", ""),
                    DataListFactory.CreateDev2Column("f2", ""),
                    DataListFactory.CreateDev2Column("f3", "")
                };
            string error;
            ErrorResultTO errors;


            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormat);
            dl.TryCreateRecordsetTemplate("rs1", "", cols, true, out error);
            dl.TryCreateScalarTemplate(string.Empty, "scalar1", "", true, out error);
            dl.TryCreateScalarValue("scalar1Value", "scalar1", out error);
            dl.TryCreateRecordsetValue("rec1.f1.vale", "f1", "rs1", 1, out error);
            dl.TryCreateRecordsetValue("rec1.f2.vale", "f2", "rs1", 1, out error);
            dl.TryCreateRecordsetValue("rec1.f3.vale", "f3", "rs1", 1, out error);
            dl.TryCreateRecordsetValue("rec2.f1.vale", "f1", "rs1", 2, out error);
            dl.TryCreateRecordsetValue("rec2.f2.vale", "f2", "rs1", 2, out error);
            dl.TryCreateRecordsetValue("rec2.f3.vale", "f3", "rs1", 2, out error);

            DataListTranslatedPayloadTO tmp = xmlConverter.ConvertFrom(dl, out errors);

            string result = tmp.FetchAsString().ToString();

            Assert.AreEqual("<DataList><rs1><f1>rec1.f1.vale</f1><f2>rec1.f2.vale</f2><f3>rec1.f3.vale</f3></rs1><rs1><f1>rec2.f1.vale</f1><f2>rec2.f2.vale</f2><f3>rec2.f3.vale</f3></rs1><scalar1>scalar1Value</scalar1></DataList>", result);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListTranslator_XMLNoInputs")]
        public void DeSerializeToXMLInputsOnlyFromBinary_ValidXML_Expect_Sucess()
        {
            IBinaryDataList dl = Dev2BinaryDataListFactory.CreateDataList();
            List<Dev2Column> cols1 = new List<Dev2Column>
                {
                    DataListFactory.CreateDev2Column("f1", ""),
                };
            List<Dev2Column> cols2 = new List<Dev2Column>
                {
                    DataListFactory.CreateDev2Column("f2", ""),
                };
            string error;
            ErrorResultTO errors;


            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormatInputsOnly);
            dl.TryCreateRecordsetTemplate("rs1", "", cols1, true, out error);
            dl.TryCreateRecordsetTemplate("rs2", "", cols2, true, true, enDev2ColumnArgumentDirection.Input, out error);
            dl.TryCreateScalarTemplate("", "scalar1", "", true, true, enDev2ColumnArgumentDirection.Input, out error);
            dl.TryCreateScalarTemplate("", "scalar2", "", true, true, enDev2ColumnArgumentDirection.Output, out error);
            dl.TryCreateScalarValue("scalar1Value", "scalar1", out error);
            dl.TryCreateScalarValue("scalar2Value", "scalar2", out error);
            dl.TryCreateRecordsetValue("rec1.f1.value", "f1", "rs1", 1, out error);
            dl.TryCreateRecordsetValue("rec1.f2.value", "f2", "rs2", 1, out error);
            dl.TryCreateRecordsetValue("rec2.f1.value", "f1", "rs1", 2, out error);
            dl.TryCreateRecordsetValue("rec2.f2.value", "f2", "rs2", 2, out error);

            DataListTranslatedPayloadTO tmp = xmlConverter.ConvertFrom(dl, out errors);

            string result = tmp.FetchAsString().ToString();

            Assert.AreEqual("<DataList><scalar1>scalar1Value</scalar1></DataList>", result);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListTranslator_XMLNoInputs")]
        public void ConvertTo_XMLInputsOnlyFromBinary_ValidXML_Expect_Sucess()
        {

            ErrorResultTO errors;
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormatInputsOnly);
            const string payload = "<DataList><rs2><f2>rec1.f2.value</f2></rs2><rs2><f2>rec2.f2.value</f2></rs2><scalar1>scalar1Value</scalar1></DataList>";
            const string shape = "<ADL><rs2><f2/></rs2><scalar1/></ADL>";
            var data = TestHelper.ConvertStringToByteArray(payload);

            var tmp = xmlConverter.ConvertTo(data, shape.ToStringBuilder(), out errors);
            var recordsets = tmp.FetchRecordsetEntries();
            var scalars = tmp.FetchScalarEntries();

            string error;
            Assert.AreEqual(1, recordsets.Count);
            var rowItems = recordsets[0].FetchRowAt(1, out error);
            Assert.AreEqual("rec1.f2.value", rowItems[0].TheValue);
            Assert.AreEqual(1, scalars.Count);
            Assert.AreEqual("scalar1Value", scalars[0].FetchScalar().TheValue);
        }

        [TestMethod]
        public void DeSerializeToXMLFromBinary_InvertedIndexInsert_ValidXML_Expect_Sucess()
        {
            IBinaryDataList dl = Dev2BinaryDataListFactory.CreateDataList();
            string error;
            List<Dev2Column> cols = new List<Dev2Column>();
            ErrorResultTO errors;
            cols.Add(DataListFactory.CreateDev2Column("f1", ""));
            cols.Add(DataListFactory.CreateDev2Column("f2", ""));
            cols.Add(DataListFactory.CreateDev2Column("f3", ""));
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormat);


            dl.TryCreateRecordsetTemplate("rs1", "", cols, true, out error);
            dl.TryCreateScalarTemplate(string.Empty, "scalar1", "", true, out error);
            dl.TryCreateScalarValue("scalar1Value", "scalar1", out error);
            dl.TryCreateRecordsetValue("rec1.f2.vale", "f2", "rs1", 1, out error);
            dl.TryCreateRecordsetValue("rec1.f1.vale", "f1", "rs1", 1, out error);
            dl.TryCreateRecordsetValue("rec1.f3.vale", "f3", "rs1", 1, out error);
            dl.TryCreateRecordsetValue("rec2.f1.vale", "f1", "rs1", 2, out error);
            dl.TryCreateRecordsetValue("rec2.f2.vale", "f2", "rs1", 2, out error);
            dl.TryCreateRecordsetValue("rec2.f3.vale", "f3", "rs1", 2, out error);

            DataListTranslatedPayloadTO tmp = xmlConverter.ConvertFrom(dl, out errors);

            string result = tmp.FetchAsString().ToString();

            Assert.AreEqual("<DataList><rs1><f1>rec1.f1.vale</f1><f2>rec1.f2.vale</f2><f3>rec1.f3.vale</f3></rs1><rs1><f1>rec2.f1.vale</f1><f2>rec2.f2.vale</f2><f3>rec2.f3.vale</f3></rs1><scalar1>scalar1Value</scalar1></DataList>", result);

        }

        [TestMethod]
        public void DeserializeToXMLFromBinaryDataListWhereDataListContainsInvalidXMLCharactersExpectedInvalidCharactersAreEscaped()
        {
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormat);
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            string error;
            dl1.TryCreateScalarTemplate(string.Empty, "cake", "", false, true, enDev2ColumnArgumentDirection.Both, out error);
            dl1.TryCreateScalarValue("Travis Is \"Cool\"&>'nstuff'<", "cake", out error);

            ErrorResultTO errors;
            var payload = xmlConverter.ConvertFrom(dl1, out errors);

            string actual = payload.FetchAsString().ToString();
            const string expected = "Travis Is \"Cool\"&amp;>'nstuff'<";

            StringAssert.Contains(actual, expected, "Not all XML special characters are escaped i.e \"'><&");

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XmlTranslator_ConvertFrom")]
        public void XmlTranslator_ConvertFrom_WhenXMLSpecialCharsPresent_ExpectEscapedChars()
        {
            //------------Setup for test--------------------------
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormatWithoutSystemTags);
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            string error;
            dl1.TryCreateScalarTemplate(string.Empty, "cake", "", false, true, enDev2ColumnArgumentDirection.Both, out error);
            dl1.TryCreateScalarValue("Travis Is \"Cool\"&>'nstuff'<", "cake", out error);

            //------------Execute Test---------------------------
            ErrorResultTO errors;
            var payload = xmlConverter.ConvertFrom(dl1, out errors);

            //------------Assert Results-------------------------
            string actual = payload.FetchAsString().ToString();
            const string expected = "Travis Is &quot;Cool&quot;&amp;&gt;&apos;nstuff&apos;&lt;";

            StringAssert.Contains(actual, expected, "Not all XML special characters are escaped i.e \"'><&");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XmlTranslator_ConvertFrom")]
        public void XmlTranslator_ConvertAndFilter_WhenXMLSpecialCharsPresent_ExpectEscapedChars()
        {
            //------------Setup for test--------------------------
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormatWithoutSystemTags);
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            string error;
            dl1.TryCreateScalarTemplate(string.Empty, "cake", "", false, true, enDev2ColumnArgumentDirection.Both, out error);
            dl1.TryCreateScalarValue("Travis Is \"Cool\"&>'nstuff'<", "cake", out error);

            //------------Execute Test---------------------------
            ErrorResultTO errors;
            var actual = xmlConverter.ConvertAndFilter(dl1, "<root><cake/></root>".ToStringBuilder(), out errors);

            //------------Assert Results-------------------------
            const string expected = "Travis Is &quot;Cool&quot;&amp;&gt;&apos;nstuff&apos;&lt;";

            StringAssert.Contains(actual.ToString(), expected, "Not all XML special characters are escaped i.e \"'><&");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XmlTranslator_ConvertFrom")]
        public void XmlTranslator_ConvertAndFilter_WhenRecordset_ExpectRowAnnotations()
        {
            //------------Setup for test--------------------------
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormatWithoutSystemTags);
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            string error;
            var cols = new List<Dev2Column> { DataListFactory.CreateDev2Column("val", string.Empty) };
            dl1.TryCreateRecordsetTemplate("rs", string.Empty, cols, true, out error);

            dl1.TryCreateRecordsetValue("1", "val", "rs", 1, out error);

            //------------Execute Test---------------------------
            ErrorResultTO errors;
            var actual = xmlConverter.ConvertAndFilter(dl1, "<root><rs><val/></rs></root>".ToStringBuilder(), out errors);
            actual = actual.Replace("\r", "").Replace("\n", "");

            //------------Assert Results-------------------------
            const string expected = "<rs index=\"1\"><val>1</val></rs>";

            StringAssert.Contains(actual.ToString(), expected, "rowID attribute not present");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XmlTranslator_ConvertFrom")]
        public void XmlTranslator_ConvertAndFilter_WhenRecordsetWithGaps_ExpectRowAnnotations()
        {
            //------------Setup for test--------------------------
            IDataListTranslator xmlConverter = Dls.GetTranslator(XmlFormatWithoutSystemTags);
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            string error;
            var cols = new List<Dev2Column> { DataListFactory.CreateDev2Column("val", string.Empty) };
            dl1.TryCreateRecordsetTemplate("rs", string.Empty, cols, true, out error);

            dl1.TryCreateRecordsetValue("1", "val", "rs", 1, out error);
            dl1.TryCreateRecordsetValue("3", "val", "rs", 3, out error);

            //------------Execute Test---------------------------
            ErrorResultTO errors;
            var actual = xmlConverter.ConvertAndFilter(dl1, "<root><rs><val/></rs></root>".ToStringBuilder(), out errors);
            actual = actual.Replace("\r", "").Replace("\n", "");

            //------------Assert Results-------------------------
            const string expected = "<rs index=\"1\"><val>1</val></rs><rs index=\"3\"><val>3</val></rs>";

            StringAssert.Contains(actual.ToString(), expected, "rowID attribute not present");
        }

        #endregion

        #endregion

        #region ConvertTo

        // BUG 9626 - 2013.06.11 - TWR: added
        [TestMethod]
        public void DataListXMLTranslatorConvertToWithSingleRecordsetExpectedEvaluatesCorrectly()
        {
            const string payload = "<ADL>\r\n  <rec>\r\n    <Name>berg</Name>\r\n    <Height>100</Height>\r\n  </rec>\r\n</ADL>";
            const string targetShape = "<ADL>\r\n<rec>\r\n\t<Name></Name>\r\n\t<Height></Height>\r\n</rec>\r\n\r\n</ADL>";

            var data = (TestHelper.ConvertStringToByteArray(payload));

            ErrorResultTO errors;
            var xmlConverter = Dls.GetTranslator(XmlFormat);
            var bdl = xmlConverter.ConvertTo(data, targetShape.ToStringBuilder(), out errors);

            var recordsets = bdl.FetchRecordsetEntries();

            string error;
            var items = recordsets[0].FetchRecordAt(1, out error);

            Assert.AreEqual(2, items.Count);

            foreach(var item in items)
            {
                switch(item.FieldName)
                {
                    case "Name":
                        Assert.AreEqual("berg", item.TheValue);
                        break;
                    case "Height":
                        Assert.AreEqual("100", item.TheValue);
                        break;
                }
            }
        }

        [TestMethod]
        [TestCategory("DataListXMLTranslatorWithOutSystemTags_ConvertTo")]
        [Description("Translator must convert payload that has no children i.e. where Payload.DocumentElement.Children[0] == '#text' as a scalar entry.")]
        [Owner("Trevor Williams-Ros")]
        public void DataListXMLTranslatorWithOutSystemTags_UnitTest_ConvertToPayloadHasNoChildNodes_EvaluatesCorrectly()
        {
            const string expectedValue = "74272317-2264-4564-3988-700350008298";
            const string targetShape = "<ADL><NumericGUID></NumericGUID></ADL>";
            var payload = string.Format("<DataList><NumericGUID>{0}</NumericGUID></DataList>", expectedValue);

            var data = (TestHelper.ConvertStringToByteArray(payload));

            ErrorResultTO errors;
            var xmlConverter = Dls.GetTranslator(XmlFormatWithoutSystemTags);
            var bdl = xmlConverter.ConvertTo(data, targetShape.ToStringBuilder(), out errors);

            var scalars = bdl.FetchScalarEntries();
            Assert.AreEqual(1, scalars.Count, "ConvertTo did not convert the payload value to a scalar.");

            var scalar = scalars[0].FetchScalar();

            var res = scalar.TheValue;

            Assert.AreEqual(expectedValue, res, "ConvertTo did not convert the payload value to the expected value.");
        }

        #endregion

        // ReSharper restore InconsistentNaming
    }
}
