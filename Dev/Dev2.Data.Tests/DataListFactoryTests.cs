using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Factories;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class DataListFactoryTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_Construct()
        {
            var dlf = new DataListFactoryImplementation();
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_CreateOutputParser()
        {
            var dlf = new DataListFactoryImplementation();
            var parser = dlf.CreateOutputParser();
            var outputs = parser.Parse("<Outputs><Output Name =\"scalar1\" MapsTo=\"[[scalar1]]\" Value=\"[[scalar1]]\" DefaultValue=\"1234\" /></Outputs>");

            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("1234", outputs[0].DefaultValue);
            Assert.AreEqual("scalar1", outputs[0].MapsTo);
            Assert.AreEqual("scalar1", outputs[0].Value);
            Assert.AreEqual(false, outputs[0].EmptyToNull);
            Assert.AreEqual(true, outputs[0].IsEvaluated);
            Assert.AreEqual(false, outputs[0].IsRequired);
            Assert.AreEqual(false, outputs[0].IsObject);
            Assert.AreEqual("[[scalar1]]", outputs[0].RawValue);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_CreateScalarList()
        {
            t(true);
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_CreateScalarList2()
        {
            t(false);
        }
        void t(bool a)
        {
            var dlf = new DataListFactoryImplementation();
            var parser = dlf.CreateOutputParser();
            var outputs = parser.Parse("<Outputs><Output Name =\"scalar1\" MapsTo=\"[[scalar1]]\" Value=\"[[scalar1]]\" DefaultValue=\"1234\" /></Outputs>");
            var scalars = dlf.CreateScalarList(outputs).ToArray();

            Assert.AreEqual(1, scalars.Length);
            Assert.AreEqual("1234", scalars[0].DefaultValue);
            Assert.AreEqual("scalar1", scalars[0].MapsTo);
            Assert.AreEqual("scalar1", scalars[0].Value);
            Assert.AreEqual(false, scalars[0].EmptyToNull);
            Assert.AreEqual(true, scalars[0].IsEvaluated);
            Assert.AreEqual(false, scalars[0].IsRequired);
            Assert.AreEqual(false, scalars[0].IsObject);
            Assert.AreEqual("[[scalar1]]", scalars[0].RawValue);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_CreateScalarList_WithOnlyRecordsetData()
        {
            var dlf = new DataListFactoryImplementation();
            var parser = dlf.CreateOutputParser();
            var outputs = parser.Parse("<Outputs><Output Name=\"name\" MapsTo=\"[[name]]\" Value=\"[[person(*).name]]\" Recordset=\"person\" DefaultValue=\"bob1\" /></Outputs>");
            var scalars = dlf.CreateScalarList(outputs).ToArray();

            Assert.AreEqual(0, scalars.Length);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_CreateRecordSetCollection()
        {
            var dlf = new DataListFactoryImplementation();
            var parser = dlf.CreateOutputParser();
            var outputs = parser.Parse("<Outputs><Output Name=\"name\" MapsTo=\"[[name]]\" Value=\"[[person(*).name]]\" Recordset=\"person\" DefaultValue=\"bob1\" /></Outputs>");
            var collection = dlf.CreateRecordSetCollection(outputs, true);

            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("person", outputs[0].RecordSetName);
            Assert.AreEqual("name", outputs[0].MapsTo);
            Assert.AreEqual("person(*).name", outputs[0].Value);
            Assert.AreEqual(false, outputs[0].EmptyToNull);
            Assert.AreEqual(true, outputs[0].IsEvaluated);
            Assert.AreEqual(false, outputs[0].IsRequired);
            Assert.AreEqual(true, outputs[0].IsRecordSet);
            Assert.AreEqual(false, outputs[0].IsObject);
            Assert.AreEqual("[[person(*).name]]", outputs[0].RawValue);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_CreateObjectList()
        {
            var dlf = new DataListFactoryImplementation();
            var parser = dlf.CreateOutputParser();
            var outputs = parser.Parse("<Outputs><Output Name=\"@obj.a\" MapsTo=\"[[a]]\" Value=\"[[@obj.a]]\" IsObject=\"True\" DefaultValue=\"1\" /></Outputs>");
            var collection = dlf.CreateObjectList(outputs);

            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("@obj.a", outputs[0].Name);
            Assert.AreEqual("a", outputs[0].MapsTo);
            Assert.AreEqual("@obj.a", outputs[0].Value);
            Assert.AreEqual(false, outputs[0].EmptyToNull);
            Assert.AreEqual(true, outputs[0].IsEvaluated);
            Assert.AreEqual(false, outputs[0].IsRequired);
            Assert.AreEqual(false, outputs[0].IsRecordSet);
            Assert.AreEqual(true, outputs[0].IsObject);
            Assert.AreEqual("[[@obj.a]]", outputs[0].RawValue);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_Instance_Singleton()
        {
            var dlfs = new ConcurrentBag<IDataListFactory>();
            var threads = new List<Thread>();
            for (var i=0; i<100; i++)
            {
                var t = new Thread(() => {
                    var instance = DataListFactory.Instance;
                    dlfs.Add(instance);
                });
                threads.Add(t);
            }
            foreach (var t in threads)
            {
                t.Start();
            }
            foreach (var t in threads)
            {
                t.Join();
            }

            Assert.AreEqual(1, dlfs.Distinct().Count());
            Assert.AreEqual(DataListFactory.Instance, dlfs.Distinct().First());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_Static_CreateOutputParser()
        {
            Assert.IsNotNull(DataListFactory.CreateOutputParser());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_Static_CreateInputParser()
        {
            Assert.IsNotNull(DataListFactory.CreateInputParser());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_Static_GenerateIntellisensePartsFromDataList()
        {
            var filterTo = new IntellisenseFilterOpsTO();
            var dataList = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";

            var result = DataListFactory.GenerateIntellisensePartsFromDataList(dataList, filterTo);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("", result[0].Description);
            Assert.AreEqual("scalar1", result[0].Name);
            Assert.IsNull(result[0].Children);

            Assert.AreEqual("", result[1].Description);
            Assert.AreEqual("rs", result[1].Name);
            Assert.AreEqual(2, result[1].Children.Count);

            Assert.AreEqual("", result[1].Children[0].Description);
            Assert.AreEqual("f1", result[1].Children[0].Name);
            Assert.IsNull(result[1].Children[0].Children);

            Assert.AreEqual("", result[1].Children[1].Description);
            Assert.AreEqual("f2", result[1].Children[1].Name);
            Assert.IsNull(result[1].Children[1].Children);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_Static_CreateIntellisensePart()
        {
            var part = DataListFactory.CreateIntellisensePart("name", "desc");
            Assert.IsNotNull(part);
            Assert.AreEqual("name", part.Name);
            Assert.AreEqual("desc", part.Description);
            Assert.IsNull(part.Children);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_Static_CreateIntellisensePartWithChildren()
        {
            var children = new List<IDev2DataLanguageIntellisensePart> {
                new Dev2DataLanguageIntellisensePart("child1", "child1 desc", null),
                new Dev2DataLanguageIntellisensePart("child2", "child2 desc", null),
            };
            var part = DataListFactory.CreateIntellisensePart("name", "desc", children);
            Assert.IsNotNull(part);
            Assert.AreEqual("name", part.Name);
            Assert.AreEqual("desc", part.Description);
            Assert.AreEqual(2, part.Children.Count);

            Assert.AreEqual("child1", part.Children[0].Name);
            Assert.AreEqual("child1 desc", part.Children[0].Description);
            Assert.IsNull(part.Children[0].Children);
            Assert.AreEqual("child2", part.Children[1].Name);
            Assert.AreEqual("child2 desc", part.Children[1].Description);
            Assert.IsNull(part.Children[1].Children);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_Static_CreateOutputTO()
        {
            var to = DataListFactory.CreateOutputTO("desc");
            Assert.IsNotNull(to);
            Assert.AreEqual("desc", to.OutPutDescription);
            Assert.AreEqual(0, to.OutputStrings.Count);
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_Static_CreateOutputTO_OutList()
        {
            var to = DataListFactory.CreateOutputTO("desc", new List<string> { "string1", "string2" });
            Assert.IsNotNull(to);
            Assert.AreEqual("desc", to.OutPutDescription);
            Assert.AreEqual(2, to.OutputStrings.Count);
            Assert.AreEqual("string1", to.OutputStrings[0]);
            Assert.AreEqual("string2", to.OutputStrings[1]);
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_Static_CreateOutputTO_OutString()
        {
            var to = DataListFactory.CreateOutputTO("desc", "string1");
            Assert.IsNotNull(to);
            Assert.AreEqual("desc", to.OutPutDescription);
            Assert.AreEqual(1, to.OutputStrings.Count);
            Assert.AreEqual("string1", to.OutputStrings[0]);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        public void DataListFactory_Static_CreateDev2Column()
        {
            var col = DataListFactory.CreateDev2Column("name", "desc", true, Interfaces.Enums.enDev2ColumnArgumentDirection.Both);
            Assert.AreEqual("name", col.ColumnName);
            Assert.AreEqual("desc", col.ColumnDescription);
            Assert.AreEqual(Interfaces.Enums.enDev2ColumnArgumentDirection.Both, col.ColumnIODirection);
            Assert.AreEqual(true, col.IsEditable);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DataListFactory_CreateLanguageParser")]
        public void DataListFactory_Static_CreateLanguageParser_IsNew_NewLanguageParser()
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
        public void DataListFactory_Static_CreateDefinition_IsNew_PassThrouh()
        {
            //------------Setup for test--------------------------
            var dev2DataLanguageParser = DataListFactory.CreateDefinition_Recordset("a", "b", "c", "", false, "", false, "", false);

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
        public void DataListFactory_Static_CreateDefinition_IsNewAndIsArray_PassThrouh()
        {
            //------------Setup for test--------------------------
            var dev2DataLanguageParser = DataListFactory.CreateDefinition_JsonArray("a", "b", "c", false, "", false, "", false, true);

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
    }
}
