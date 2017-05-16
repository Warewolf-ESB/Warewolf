using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class DataListFactoryTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DataListFactory_CreateLanguageParser")]
        public void DataListFactory_CreateLanguageParser_IsNew_NewLanguageParser()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var dev2DataLanguageParser = DataListFactory.CreateLanguageParser();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dev2DataLanguageParser);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DataListFactory_CreateDefinition")]
        public void DataListFactory_CreateDefinition_IsNew_PassThrouh()
        {
            //------------Setup for test--------------------------
            var dev2DataLanguageParser = DataListFactory.CreateDefinition("a", "b", "c", "", false, "", false, "", false);

            //------------Execute Test---------------------------
            Assert.IsNotNull(dev2DataLanguageParser);
            //------------Assert Results-------------------------
            Assert.AreEqual("a", dev2DataLanguageParser.Name);
            Assert.AreEqual("b", dev2DataLanguageParser.MapsTo);
            Assert.AreEqual("c", dev2DataLanguageParser.Value);
            Assert.AreEqual("", dev2DataLanguageParser.RecordSetName);
            Assert.AreEqual(false, dev2DataLanguageParser.IsEvaluated);
            Assert.AreEqual("", dev2DataLanguageParser.DefaultValue);
            Assert.AreEqual(false, dev2DataLanguageParser.IsRequired);
            Assert.AreEqual("", dev2DataLanguageParser.RawValue);
            Assert.AreEqual(false, dev2DataLanguageParser.EmptyToNull);
            Assert.AreEqual(false, dev2DataLanguageParser.IsJsonArray);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DataListFactory_CreateDefinition")]
        public void DataListFactory_CreateDefinition_IsNewAndIsArray_PassThrouh()
        {
            //------------Setup for test--------------------------
            var dev2DataLanguageParser = DataListFactory.CreateDefinition("a", "b", "c", false, "", false, "", false, true);

            //------------Execute Test---------------------------
            Assert.IsNotNull(dev2DataLanguageParser);
            //------------Assert Results-------------------------
            Assert.AreEqual("a", dev2DataLanguageParser.Name);
            Assert.AreEqual("b", dev2DataLanguageParser.MapsTo);
            Assert.AreEqual("c", dev2DataLanguageParser.Value);
            Assert.AreEqual("", dev2DataLanguageParser.RecordSetName);
            Assert.AreEqual(false, dev2DataLanguageParser.IsEvaluated);
            Assert.AreEqual("", dev2DataLanguageParser.DefaultValue);
            Assert.AreEqual(false, dev2DataLanguageParser.IsRequired);
            Assert.AreEqual("", dev2DataLanguageParser.RawValue);
            Assert.AreEqual(false, dev2DataLanguageParser.EmptyToNull);
            Assert.AreEqual(true, dev2DataLanguageParser.IsJsonArray);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DataListFactory_CreateScalarList")]
        public void DataListFactory_CreateScalarList_IsNewAndIsArray_PassThrouh()
        {
            //------------Setup for test--------------------------
            var dev2DataLanguageParser = DataListFactory.CreateScalarList(new List<IDev2Definition>(), false);

            //------------Execute Test---------------------------
            Assert.IsNotNull(dev2DataLanguageParser);
            //------------Assert Results-------------------------
            var dev2Definitions = dev2DataLanguageParser.ToList();
            Assert.AreEqual(0, dev2Definitions.Count);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DataListFactory_CreateScalarList")]
        public void DataListFactory_CreateScalarList_IsNewAndIsArray_PassCount()
        {
            //------------Setup for test--------------------------
            var dev2DataLanguageParser = DataListFactory.CreateScalarList(new List<IDev2Definition>()
            {
                new Dev2Definition("","","",false,"",true,"")
            }, false);

            //------------Execute Test---------------------------
            Assert.IsNotNull(dev2DataLanguageParser);
            //------------Assert Results-------------------------
            var dev2Definitions = dev2DataLanguageParser.ToList();
            Assert.AreEqual(1, dev2Definitions.Count);
        }
    }
}
