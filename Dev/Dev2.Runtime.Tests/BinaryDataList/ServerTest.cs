using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.Server.DataList;
using Dev2.Server.DataList.Translators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Dev2.DynamicServices.Test {
    /// <summary>
    /// Summary description for ServerTest
    /// </summary>
    [TestClass]
    public class ServerTest {

        private static IDataListServer dls = new DataListServer(DataListPersistenceProviderFactory.CreateMemoryProvider());
        private static DataListFormat xmlFormat = DataListFormat.CreateFormat(GlobalConstants._XML);
        private static DataListFormat binFormat = DataListFormat.CreateFormat(GlobalConstants._BINARY);


        private static readonly string _dataListWellformed = "<DataList><scalar1/><rs1><f1/><f2/></rs1><scalar2/></DataList>";
        private static readonly string _dataListWellformedData = "<DataList><scalar1>s1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><scalar2/></DataList>";
        private static readonly string _dataListWellformedDataWithIllegalCharacters = "<DataList><scalar1>&s1</scalar1><rs1><f1>&f1.1</f1></rs1><rs1><f1>&f1.2</f1></rs1><scalar2/></DataList>";
        private static readonly string _dataListWellformedMult = "<DataList><scalar1/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>";
        //private static readonly string _dataListWellformedMultData = "<DataList><scalar1>s1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2/></DataList>";
        private static readonly string _dataListMalformed = "<DataList><scalar1/><rs1><f1/><f2/><f3/><scalar2/></DataList>";
        //private static readonly string _dataListMalformedData = "<DataList><scalar1/>abc<rs1><f1/><f2/><f3/><scalar2/></DataList>";
        private static readonly string _dataListWellformedDescAttributes = "<DataList><scalar1 Description=\"Test scalar description\"/><rs1 Description=\"Test recordset desciption\"><f1 Description=\"Test field1 desciption\"/></rs1><scalar2/></DataList>";
        private static readonly string _dataListWellformedDataWithDesc = "<DataList><scalar1 Description=\"Test scalar description\"/>s1</scalar1><rs1 Description=\"Test recordset desciption\"><f1 Description=\"Test field1 desciption\">f1.1</f1></rs1><rs1 Description=\"Test recordset desciption\"><f1 Description=\"Test field1 desciption\">f1.2</f1></rs1><scalar2/></DataList>";
        
        
        private TestContext testContextInstance;
        //static Process _redisProcess;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext) {
            // boot strap the server
//            var pathToRedis = Path.Combine(testContext.DeploymentDirectory, "redis-server.exe");
//            if (_redisProcess == null) _redisProcess = Process.Start(pathToRedis);
            dls.AddTranslator(DataListTranslatorFactory.FetchBinaryTranslator());
            dls.AddTranslator(DataListTranslatorFactory.FetchXmlTranslator());
            dls.AddTranslator(DataListTranslatorFactory.FetchJSONTranslator());


        }
        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void BaseActivityUnitTestCleanup()
        {
            //if(_redisProcess != null)
            //{
            //    _redisProcess.Kill();
            //}
        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        
        #endregion



        #region Positive Test

        [TestMethod]
        public void Fetch_TranslationTypes_ExpectThreeTypes() {

            IList<DataListFormat> formats =  dls.FetchTranslatorTypes();

            Assert.IsTrue(formats.Count == 3);
            Assert.AreEqual(GlobalConstants._BINARY,formats[0].FormatName);
            Assert.AreEqual(GlobalConstants._XML, formats[1].FormatName);
            Assert.AreEqual(GlobalConstants._JSON, formats[2].FormatName);
        }

        #endregion

        #region XMLTest

        #region SerializeToXML Tests

        [TestMethod]
        public void SerializeToXML_ValidXML_Expect_Sucess() {
            ErrorResultTO errors = new ErrorResultTO();
            string error;
            IDataListTranslator xmlConverter = dls.GetTranslator(xmlFormat);
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            IBinaryDataList obj = xmlConverter.ConvertTo(data, _dataListWellformed, out errors);
            //enSystemTag
            
            using(obj)
            {
                IBinaryDataListEntry entry = null;
                if(obj.TryGetEntry("rs1", out entry, out error))
                {
                    IList<IBinaryDataListItem> cols = entry.FetchRecordAt(1, out error);
                    int systemTagCount = Enum.GetValues(typeof(enSystemTag)).Length;
                    Assert.IsTrue(obj.FetchAllEntries().Count == 3 + systemTagCount && obj.FetchScalarEntries().Count == 2 + systemTagCount && obj.FetchRecordsetEntries().Count == 1 && cols.Count == 2);
                }
                else
                {
                    Assert.Fail("Error");
                }
            }
        }

        [TestMethod]
        public void SerializeToXML_ValidXMLMultRecordsets_Expect_Sucess() {
            ErrorResultTO errors = new ErrorResultTO();
            string error = string.Empty;
            IDataListTranslator xmlConverter = dls.GetTranslator(xmlFormat);
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            IBinaryDataList obj = xmlConverter.ConvertTo(data, _dataListWellformedMult, out errors);

            IBinaryDataListEntry entry = null;

            if (obj.TryGetEntry("rs1", out entry, out error)) {
                IList<IBinaryDataListItem> cols = entry.FetchRecordAt(1, out error);
                int systemTagCount = Enum.GetValues(typeof(enSystemTag)).Length;
                Assert.IsTrue(obj.FetchAllEntries().Count == 4 + systemTagCount && obj.FetchScalarEntries().Count == 2 + systemTagCount && obj.FetchRecordsetEntries().Count == 2 && cols.Count == 2);
            } else {
                Assert.Fail("Error");
            }
        }

        [TestMethod]
        public void SerializeToXML_ValidXMLWithDescriptions_Expect_Sucess() {
            string error = string.Empty;
            ErrorResultTO errors = new ErrorResultTO();
            IDataListTranslator xmlConverter = dls.GetTranslator(xmlFormat);
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            IBinaryDataList obj = xmlConverter.ConvertTo(data, _dataListWellformedDescAttributes, out errors);

            IBinaryDataListEntry entry = null;

            if (obj.TryGetEntry("rs1", out entry, out error)) {
                IList<IBinaryDataListItem> cols = entry.FetchRecordAt(1, out error);
                int systemTagCount = Enum.GetValues(typeof(enSystemTag)).Length;
                Assert.IsTrue(obj.FetchAllEntries().Count == 3 + systemTagCount && obj.FetchScalarEntries().Count == 2 + systemTagCount && obj.FetchRecordsetEntries().Count == 1 && cols.Count == 1);
            } else {
                Assert.Fail("Error");
            }
        }

        [TestMethod]
        public void SerializeToXML_ValidXMLWithDescriptions_Return_Decriptions_Expect_Sucess() {
            string error = string.Empty;
            ErrorResultTO errors = new ErrorResultTO();
            IDataListTranslator xmlConverter = dls.GetTranslator(xmlFormat);
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedDataWithDesc));
            IBinaryDataList obj = xmlConverter.ConvertTo(data, _dataListWellformedDescAttributes, out errors);

            IList<IBinaryDataListEntry> scalars = obj.FetchScalarEntries();
            IList<IBinaryDataListEntry> recordsets = obj.FetchRecordsetEntries();

            Assert.IsTrue(scalars[0].Description == "Test scalar description" && recordsets[0].Columns[0].ColumnDescription == "Test field1 desciption");
        }

        [TestMethod]
        public void SerializeToXML_ValidXML_BlankXML_Expect_Blank_Values_Returned() {
            string error = string.Empty;
            ErrorResultTO errors = new ErrorResultTO();
            IDataListTranslator xmlConverter = dls.GetTranslator(xmlFormat);
            byte[] data = (TestHelper.ConvertStringToByteArray(""));
            IBinaryDataList obj = xmlConverter.ConvertTo(data, _dataListWellformedDescAttributes, out errors);
            using(obj)
            {
                IList<IBinaryDataListEntry> scalars = obj.FetchScalarEntries();
                IList<IBinaryDataListEntry> recordsets = obj.FetchRecordsetEntries();
                IBinaryDataListItem item = scalars[0].FetchScalar();
                IList<IBinaryDataListItem> items = recordsets[0].FetchRecordAt(1, out error);
                Assert.IsTrue(item != null && items != null);
            }
        }

        [TestMethod]
        public void SerializeToXML_InValidXML_Expect_Error() {
            string error = string.Empty;
            ErrorResultTO errors = new ErrorResultTO();
            IDataListTranslator xmlConverter = dls.GetTranslator(xmlFormat);
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListMalformed));
            IBinaryDataList obj = xmlConverter.ConvertTo(data, _dataListMalformed, out errors);

            Assert.AreEqual("The 'rs1' start tag on line 1 position 22 does not match the end tag of 'DataList'. Line 1, position 53.", errors.FetchErrors()[0]);
        }

        #endregion

        #region DeserializeToXMLFromBinary Tests
        [TestMethod]
        public void DeSerializeToXMLFromBinary_ValidXML_Expect_Sucess() {
            IBinaryDataList dl = Dev2BinaryDataListFactory.CreateDataList();
            List<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(DataListFactory.CreateDev2Column("f1", ""));
            cols.Add(DataListFactory.CreateDev2Column("f2", ""));
            cols.Add(DataListFactory.CreateDev2Column("f3", ""));
            string error = string.Empty;
            ErrorResultTO errors = new ErrorResultTO();


                IDataListTranslator xmlConverter = dls.GetTranslator(xmlFormat);
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

                string result = tmp.FetchAsString();

                Assert.AreEqual("<DataList><rs1><f1>rec1.f1.vale</f1><f2>rec1.f2.vale</f2><f3>rec1.f3.vale</f3></rs1><rs1><f1>rec2.f1.vale</f1><f2>rec2.f2.vale</f2><f3>rec2.f3.vale</f3></rs1><scalar1>scalar1Value</scalar1></DataList>", result);
            
        }

        [TestMethod]
        public void DeSerializeToXMLFromBinary_InvertedIndexInsert_ValidXML_Expect_Sucess() {
            IBinaryDataList dl = Dev2BinaryDataListFactory.CreateDataList();
            string error = string.Empty;
            List<Dev2Column> cols = new List<Dev2Column>();
            ErrorResultTO errors = new ErrorResultTO();
            cols.Add(DataListFactory.CreateDev2Column("f1", ""));
            cols.Add(DataListFactory.CreateDev2Column("f2", ""));
            cols.Add(DataListFactory.CreateDev2Column("f3", ""));
            IDataListTranslator xmlConverter = dls.GetTranslator(xmlFormat);


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

                string result = tmp.FetchAsString();

                Assert.AreEqual("<DataList><rs1><f1>rec1.f1.vale</f1><f2>rec1.f2.vale</f2><f3>rec1.f3.vale</f3></rs1><rs1><f1>rec2.f1.vale</f1><f2>rec2.f2.vale</f2><f3>rec2.f3.vale</f3></rs1><scalar1>scalar1Value</scalar1></DataList>", result);
            
        }

        [TestMethod]
        public void DeserializeToXMLFromBinaryDataListWhereDataListContainsInvalidXMLCharactersExpectedInvalidCharactersAreEscaped()
        {
            IDataListTranslator xmlConverter = dls.GetTranslator(xmlFormat);
            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            using (dl1)
            {
                string error;
                dl1.TryCreateScalarTemplate(string.Empty, "cake", "", false, true, enDev2ColumnArgumentDirection.Both, out error);
                dl1.TryCreateScalarValue("Travis Is \"Cool\"&>'nstuff'<", "cake", out error);

                ErrorResultTO errors;
                var payload = xmlConverter.ConvertFrom(dl1, out errors);

                string actual = payload.FetchAsString();
                string expected = "Travis Is \"Cool\"&amp;>'nstuff'<";

                StringAssert.Contains(actual, expected, "Not all XML special characters are escaped i.e \"'><&");

                //Assert.Inconclusive("& only in use");
            }
        }
        #endregion

        #endregion

    }
}
