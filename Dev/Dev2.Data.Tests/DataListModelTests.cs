/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using System.Xml;
using Dev2.Data.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class DataListModelTests
    {
        const string Shape = @"<DataList>
                                <Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" >
                                    <Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" />
                                    <Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" />
                                </Car>
                                <Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" />
                                <Person Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""Both"" >
                                    <Age Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Age>
                                    <Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name>
                                    <Schools Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" >
                                        <Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name>
                                        <Location Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Location>
                                    </Schools>
                                </Person>
                              </DataList>";

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DataListModel))]
        [ExpectedException(typeof(XmlException))]
        public void DataListModel_InvalidXml_ShouldThrow()
        {
            var dataListModel = new DataListModel();
            dataListModel.Create("<b", Shape);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DataListModel))]
        public void DataListModel_InvalidIODirection_ShouldThrow()
        {
            const string Shape = @"<DataList>
                                    <Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" >
                                        <Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""Input"" />
                                        <Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""Output"" />
                                    </Car>
                                   </DataList>";

            var dataListModel = new DataListModel();
            dataListModel.Create(@"<Data><Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" >
                                        <Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""Input"" />
                                        <Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""Output"" />
                                    </Car><Truck Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" >
                                        <Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""Input"" />
                                        <Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""Output"" />
                                    </Truck></Data>", Shape);


            Assert.AreEqual(1, dataListModel.RecordSets.Count);
            Assert.AreEqual(1, dataListModel.RecordSets[0].Columns.Count);
            Assert.AreEqual(1, dataListModel.RecordSets[0].Columns.Values.Count);
            var values = dataListModel.RecordSets[0].Columns.Values.First();
            Assert.AreEqual(2, values.Count);
            Assert.AreEqual(null, values[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Input, values[0].IODirection);
            Assert.AreEqual(false, values[0].IsEditable);
            Assert.AreEqual("Make", values[0].Name);
            Assert.AreEqual("", values[0].Value);

            Assert.AreEqual(null, values[1].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Output, values[1].IODirection);
            Assert.AreEqual(false, values[1].IsEditable);
            Assert.AreEqual("Model", values[1].Name);
            Assert.AreEqual("", values[1].Value);

            Assert.AreEqual(1, dataListModel.ShapeRecordSets.Count);
            Assert.AreEqual(1, dataListModel.ShapeRecordSets[0].Columns.Count);
            var col = dataListModel.ShapeRecordSets[0].Columns.First();
            values = col.Value;
            Assert.AreEqual(1, col.Key);
            Assert.AreEqual(2, values.Count);
            Assert.AreEqual("Make of vehicle", values[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Input, values[0].IODirection);
            Assert.AreEqual(true, values[0].IsEditable);
            Assert.AreEqual("Make", values[0].Name);
            Assert.AreEqual(null, values[0].Value);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DataListModel))]
        public void DataListModel_EmptyData()
        {
            var dataListModel = new DataListModel();
            dataListModel.Create("", Shape);

            // verify
            Assert.AreEqual(1, dataListModel.RecordSets.Count);
            Assert.AreEqual(1, dataListModel.RecordSets[0].Columns.Count);
            Assert.AreEqual(1, dataListModel.RecordSets[0].Columns.Values.Count);
            var values = dataListModel.RecordSets[0].Columns.Values.First();
            Assert.AreEqual(2, values.Count);
            Assert.AreEqual("Make of vehicle", values[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.None, values[0].IODirection);
            Assert.AreEqual(true, values[0].IsEditable);
            Assert.AreEqual("Make", values[0].Name);
            Assert.AreEqual(null, values[0].Value);

            Assert.AreEqual("Model of vehicle", values[1].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.None, values[1].IODirection);
            Assert.AreEqual(true, values[1].IsEditable);
            Assert.AreEqual("Model", values[1].Name);
            Assert.AreEqual(null, values[1].Value);

            Assert.AreEqual(1, dataListModel.ComplexObjects.Count);
            Assert.AreEqual("", dataListModel.ComplexObjects[0].Description);
            Assert.AreEqual("@Person", dataListModel.ComplexObjects[0].Name);
            Assert.AreEqual(null, dataListModel.ComplexObjects[0].Value);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Both, dataListModel.ComplexObjects[0].IODirection);
            Assert.AreEqual(false, dataListModel.ComplexObjects[0].IsArray);
            Assert.AreEqual(false, dataListModel.ComplexObjects[0].IsEditable);
            Assert.AreEqual(null, dataListModel.ComplexObjects[0].Parent);
            Assert.AreEqual(0, dataListModel.ComplexObjects[0].Children.Count);

            Assert.AreEqual(1, dataListModel.Scalars.Count);
            Assert.AreEqual("name of Country", dataListModel.Scalars[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Both, dataListModel.Scalars[0].IODirection);
            Assert.AreEqual(true, dataListModel.Scalars[0].IsEditable);
            Assert.AreEqual("Country", dataListModel.Scalars[0].Name);
            Assert.AreEqual(null, dataListModel.Scalars[0].Value);

            Assert.AreEqual(1, dataListModel.ShapeComplexObjects.Count);
            Assert.AreEqual(0, dataListModel.ShapeComplexObjects[0].Children.Count);
            Assert.AreEqual("", dataListModel.ShapeComplexObjects[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Both, dataListModel.ShapeComplexObjects[0].IODirection);
            Assert.AreEqual(false, dataListModel.ShapeComplexObjects[0].IsArray);
            Assert.AreEqual(false, dataListModel.ShapeComplexObjects[0].IsEditable);
            Assert.AreEqual("@Person", dataListModel.ShapeComplexObjects[0].Name);
            Assert.AreEqual(null, dataListModel.ShapeComplexObjects[0].Parent);
            Assert.AreEqual(null, dataListModel.ShapeComplexObjects[0].Value);

            Assert.AreEqual(1, dataListModel.ShapeRecordSets.Count);
            Assert.AreEqual(1, dataListModel.ShapeRecordSets[0].Columns.Count);
            var col = dataListModel.ShapeRecordSets[0].Columns.First();
            values = col.Value;
            Assert.AreEqual(1, col.Key);
            Assert.AreEqual(2, values.Count);
            Assert.AreEqual("Make of vehicle", values[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.None, values[0].IODirection);
            Assert.AreEqual(true, values[0].IsEditable);
            Assert.AreEqual("Make", values[0].Name);
            Assert.AreEqual(null, values[0].Value);

            Assert.AreEqual("Model of vehicle", values[1].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.None, values[1].IODirection);
            Assert.AreEqual(true, values[1].IsEditable);
            Assert.AreEqual("Model", values[1].Name);
            Assert.AreEqual(null, values[1].Value);

            Assert.AreEqual(1, dataListModel.ShapeScalars.Count);
            Assert.AreEqual("name of Country", dataListModel.ShapeScalars[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Both, dataListModel.ShapeScalars[0].IODirection);
            Assert.AreEqual(true, dataListModel.ShapeScalars[0].IsEditable);
            Assert.AreEqual("Country", dataListModel.ShapeScalars[0].Name);
            Assert.AreEqual(null, dataListModel.ShapeScalars[0].Value);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DataListModel))]
        public void DataListModel_DataXmlWithNoShape_Success()
        {
            var dataListModel = new DataListModel();
            dataListModel.Create(@"" +
                @"<Country Description="""" IsEditable=""True"" IsJson=""False"" IsArray=""False"" ColumnIODirection=""None"" />" +
                @"<Food Description="""" IsEditable=""True"" IsJson=""False"" IsArray=""False"" ColumnIODirection=""None"" />"
                , "<datalist></datalist>");

            Assert.AreEqual(0, dataListModel.Scalars.Count);
            Assert.AreEqual(0, dataListModel.ShapeScalars.Count);
            Assert.AreEqual(0, dataListModel.ComplexObjects.Count);
            Assert.AreEqual(0, dataListModel.ShapeComplexObjects.Count);
            Assert.AreEqual(0, dataListModel.RecordSets.Count);
            Assert.AreEqual(0, dataListModel.ShapeRecordSets.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DataListModel))]
        public void DataListModel_DataXmlWithSystemTag_GivenNoIODirection_NotAdded()
        {
            var dataListModel = new DataListModel();
            dataListModel.Create(@"" +
                @"<Other Description="""" IsEditable=""True"" IsJson=""False"" IsArray=""False"" ColumnIODirection=""Both"" />" +
                @"<Food Description="""" IsEditable=""True"" IsJson=""False"" IsArray=""False"" />"
                , "<datalist><WebServerUrl /><Food /></datalist>");

            Assert.AreEqual(1, dataListModel.Scalars.Count);
            Assert.AreEqual(1, dataListModel.ShapeScalars.Count);
            Assert.AreEqual(0, dataListModel.ComplexObjects.Count);
            Assert.AreEqual(0, dataListModel.ShapeComplexObjects.Count);
            Assert.AreEqual(0, dataListModel.RecordSets.Count);
            Assert.AreEqual(0, dataListModel.ShapeRecordSets.Count);


            Assert.AreEqual("", dataListModel.Scalars[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.None, dataListModel.Scalars[0].IODirection);
            Assert.AreEqual(true, dataListModel.Scalars[0].IsEditable);
            Assert.AreEqual("Food", dataListModel.Scalars[0].Name);
            Assert.AreEqual("", dataListModel.Scalars[0].Value);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DataListModel))]
        public void DataListModel_DataXmlWithSystemTag_GivenInputIODirection_NotAdded()
        {
            var dataListModel = new DataListModel();
            dataListModel.Create(@"" +
                @"<WebServerUrl Description="""" IsEditable=""True"" IsJson=""False"" IsArray=""False"" ColumnIODirection=""Input"" />" +
                @"<Food Description="""" IsEditable=""True"" IsJson=""False"" IsArray=""False"" ColumnIODirection=""None"" />"
                , "<datalist><WebServerUrl /><Food /></datalist>");

            Assert.AreEqual(1, dataListModel.Scalars.Count);
            Assert.AreEqual(1, dataListModel.ShapeScalars.Count);
            Assert.AreEqual(0, dataListModel.ComplexObjects.Count);
            Assert.AreEqual(0, dataListModel.ShapeComplexObjects.Count);
            Assert.AreEqual(0, dataListModel.RecordSets.Count);
            Assert.AreEqual(0, dataListModel.ShapeRecordSets.Count);


            Assert.AreEqual("", dataListModel.Scalars[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.None, dataListModel.Scalars[0].IODirection);
            Assert.AreEqual(true, dataListModel.Scalars[0].IsEditable);
            Assert.AreEqual("Food", dataListModel.Scalars[0].Name);
            Assert.AreEqual("", dataListModel.Scalars[0].Value);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DataListModel))]
        public void DataListModel_DataXmlWithSystemTag_GivenOutputIODirection_NotAdded()
        {
            var dataListModel = new DataListModel();
            dataListModel.Create(@"" +
                @"<WebServerUrl Description="""" IsEditable=""True"" IsJson=""False"" IsArray=""False"" ColumnIODirection=""Output"" />" +
                @"<Food Description="""" IsEditable=""True"" IsJson=""False"" IsArray=""False"" ColumnIODirection=""Output"" />"
                , "<datalist><WebServerUrl /><Food /></datalist>");

            Assert.AreEqual(1, dataListModel.Scalars.Count);
            Assert.AreEqual(1, dataListModel.ShapeScalars.Count);
            Assert.AreEqual(0, dataListModel.ComplexObjects.Count);
            Assert.AreEqual(0, dataListModel.ShapeComplexObjects.Count);
            Assert.AreEqual(0, dataListModel.RecordSets.Count);
            Assert.AreEqual(0, dataListModel.ShapeRecordSets.Count);


            Assert.AreEqual("", dataListModel.Scalars[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.None, dataListModel.Scalars[0].IODirection);
            Assert.AreEqual(true, dataListModel.Scalars[0].IsEditable);
            Assert.AreEqual("Food", dataListModel.Scalars[0].Name);
            Assert.AreEqual("", dataListModel.Scalars[0].Value);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DataListModel))]
        public void DataListModel_DataXmlWithNoRoot_Success()
        {
            var dataListModel = new DataListModel();
            dataListModel.Create(@""+
                @"<Country Description="""" IsEditable=""True"" IsJson=""False"" IsArray=""False"" ColumnIODirection=""None"" />"+
                @"<Food Description="""" IsEditable=""True"" IsJson=""False"" IsArray=""False"" ColumnIODirection=""None"" />"
                , "<datalist><Country /><Food /></datalist>");

            Assert.AreEqual(2, dataListModel.Scalars.Count);
            Assert.AreEqual("", dataListModel.Scalars[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.None, dataListModel.Scalars[0].IODirection);
            Assert.AreEqual(true, dataListModel.Scalars[0].IsEditable);
            Assert.AreEqual("Country", dataListModel.Scalars[0].Name);
            Assert.AreEqual("", dataListModel.Scalars[0].Value);

            Assert.AreEqual("", dataListModel.Scalars[1].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.None, dataListModel.Scalars[1].IODirection);
            Assert.AreEqual(true, dataListModel.Scalars[1].IsEditable);
            Assert.AreEqual("Food", dataListModel.Scalars[1].Name);
            Assert.AreEqual("", dataListModel.Scalars[1].Value);

            Assert.AreEqual(2, dataListModel.ShapeScalars.Count);

            Assert.AreEqual("", dataListModel.Scalars[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.None, dataListModel.Scalars[0].IODirection);
            Assert.AreEqual(true, dataListModel.Scalars[0].IsEditable);
            Assert.AreEqual("Country", dataListModel.Scalars[0].Name);
            Assert.AreEqual("", dataListModel.Scalars[0].Value);

            Assert.AreEqual("", dataListModel.Scalars[1].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.None, dataListModel.Scalars[1].IODirection);
            Assert.AreEqual(true, dataListModel.Scalars[1].IsEditable);
            Assert.AreEqual("Food", dataListModel.Scalars[1].Name);
            Assert.AreEqual("", dataListModel.Scalars[1].Value);

            Assert.AreEqual(0, dataListModel.ComplexObjects.Count);
            Assert.AreEqual(0, dataListModel.ShapeComplexObjects.Count);
            Assert.AreEqual(0, dataListModel.RecordSets.Count);
            Assert.AreEqual(0, dataListModel.ShapeRecordSets.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DataListModel))]
        public void DataListModel_Create_PayLoadWithComplexObjects_ShouldHaveComplexObjectItems()
        {
            //------------Setup for test--------------------------
            const string Data = "<DataList></DataList>";
            var dataListModel = new DataListModel();
            var converter = new DataListConversionUtils();
            //------------Execute Test---------------------------
            dataListModel.Create(Data, Shape);
            var result = converter.CreateListToBindTo(dataListModel);
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("", dataListModel.ComplexObjects[0].Description);
            Assert.AreEqual("Country", result[0].DisplayValue);
            Assert.AreEqual("@Person", result[1].DisplayValue);
            Assert.IsTrue(result[1].IsObject);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DataListModel))]
        public void DataListModel_Create_PayLoadWithComplexObjectsWithArrays_ShouldHaveComplexObjectItems()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList>
                                    <Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" >
                                        <Make Description=""Make of vehicle"" IsEditable=""True"" />
                                        <Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""SomeDirection"" />
                                    </Car>
                                    <Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" />
                                    <Person Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""Both"" >
                                        <Age Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Age>
                                        <Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name>
                                        <Schools Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""True"" ColumnIODirection=""None"" >
                                            <Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name>
                                            <Location Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Location>
                                        </Schools>
                                    </Person>
                                   </DataList>";
            const string Data = "<DataList></DataList>";
            var dataListModel = new DataListModel();
            var converter = new DataListConversionUtils();
            //------------Execute Test---------------------------
            dataListModel.Create(Data, Shape);
            var result = converter.CreateListToBindTo(dataListModel);
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("", dataListModel.ComplexObjects[0].Description);
            Assert.AreEqual("Country", result[0].DisplayValue);
            Assert.AreEqual("@Person", result[1].DisplayValue);
            Assert.IsTrue(result[1].IsObject);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DataListModel))]
        public void DataListModel_Create_PayLoadWithComplexObjectsArrayWithParentArray_ShouldHaveComplexObjectItems()
        {
            const string Shape = @"<DataList>
                                    <Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" >
                                        <Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""Input"" />
                                        <Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""Output"" />
                                    </Car>
                                    <Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" />
                                    <a Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""Both"" >
                                        <a Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""True"" ColumnIODirection=""None"" >
                                            <a Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""True"" ColumnIODirection=""None"" >
                                                <a1 Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></a1>
                                            </a>
                                        </a>
                                    </a>
                                   </DataList>";
            const string Data = "<DataList></DataList>";
            var dataListModel = new DataListModel();

            // test
            dataListModel.Create(Data, Shape);

            // verify
            Assert.AreEqual(1, dataListModel.RecordSets.Count);
            Assert.AreEqual(1, dataListModel.RecordSets[0].Columns.Count);
            Assert.AreEqual(1, dataListModel.RecordSets[0].Columns.Values.Count);
            var values = dataListModel.RecordSets[0].Columns.Values.First();
            Assert.AreEqual(2, values.Count);
            Assert.AreEqual("Make of vehicle", values[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Input, values[0].IODirection);
            Assert.AreEqual(true, values[0].IsEditable);
            Assert.AreEqual("Make", values[0].Name);
            Assert.AreEqual(null, values[0].Value);

            Assert.AreEqual("Model of vehicle", values[1].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Output, values[1].IODirection);
            Assert.AreEqual(true, values[1].IsEditable);
            Assert.AreEqual("Model", values[1].Name);
            Assert.AreEqual(null, values[1].Value);

            Assert.AreEqual(1, dataListModel.ComplexObjects.Count);
            Assert.AreEqual("", dataListModel.ComplexObjects[0].Description);
            Assert.AreEqual("@a", dataListModel.ComplexObjects[0].Name);
            Assert.AreEqual(null, dataListModel.ComplexObjects[0].Value);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Both, dataListModel.ComplexObjects[0].IODirection);
            Assert.AreEqual(false, dataListModel.ComplexObjects[0].IsArray);
            Assert.AreEqual(false, dataListModel.ComplexObjects[0].IsEditable);
            Assert.AreEqual(null, dataListModel.ComplexObjects[0].Parent);
            Assert.AreEqual(0, dataListModel.ComplexObjects[0].Children.Count);

            Assert.AreEqual(1, dataListModel.Scalars.Count);
            Assert.AreEqual("name of Country", dataListModel.Scalars[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Both, dataListModel.Scalars[0].IODirection);
            Assert.AreEqual(true, dataListModel.Scalars[0].IsEditable);
            Assert.AreEqual("Country", dataListModel.Scalars[0].Name);
            Assert.AreEqual(null, dataListModel.Scalars[0].Value);

            Assert.AreEqual(1, dataListModel.ShapeComplexObjects.Count);
            Assert.AreEqual(0, dataListModel.ShapeComplexObjects[0].Children.Count);
            Assert.AreEqual("", dataListModel.ShapeComplexObjects[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Both, dataListModel.ShapeComplexObjects[0].IODirection);
            Assert.AreEqual(false, dataListModel.ShapeComplexObjects[0].IsArray);
            Assert.AreEqual(false, dataListModel.ShapeComplexObjects[0].IsEditable);
            Assert.AreEqual("@a", dataListModel.ShapeComplexObjects[0].Name);
            Assert.AreEqual(null, dataListModel.ShapeComplexObjects[0].Parent);
            Assert.AreEqual(null, dataListModel.ShapeComplexObjects[0].Value);

            Assert.AreEqual(1, dataListModel.ShapeRecordSets.Count);
            Assert.AreEqual(1, dataListModel.ShapeRecordSets[0].Columns.Count);
            var col = dataListModel.ShapeRecordSets[0].Columns.First();
            values = col.Value;
            Assert.AreEqual(1, col.Key);
            Assert.AreEqual(2, values.Count);
            Assert.AreEqual("Make of vehicle", values[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Input, values[0].IODirection);
            Assert.AreEqual(true, values[0].IsEditable);
            Assert.AreEqual("Make", values[0].Name);
            Assert.AreEqual(null, values[0].Value);

            Assert.AreEqual("Model of vehicle", values[1].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Output, values[1].IODirection);
            Assert.AreEqual(true, values[1].IsEditable);
            Assert.AreEqual("Model", values[1].Name);
            Assert.AreEqual(null, values[1].Value);


            Assert.AreEqual(1, dataListModel.ShapeScalars.Count);
            Assert.AreEqual("name of Country", dataListModel.ShapeScalars[0].Description);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Both, dataListModel.ShapeScalars[0].IODirection);
            Assert.AreEqual(true, dataListModel.ShapeScalars[0].IsEditable);
            Assert.AreEqual("Country", dataListModel.ShapeScalars[0].Name);
            Assert.AreEqual(null, dataListModel.ShapeScalars[0].Value);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DataListModel))]
        public void DataListModel_Create_PayLoadWithComplexObjectsArray_ShouldHaveComplexObjectItems()
        {
            //------------Setup for test--------------------------
            const string Data = @"<DataList>
	                                <Food Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""Output"" >
		                                <FoodName Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" />
	                                </Food>
                                </DataList>";

            const string Shape = @"<DataList>
                                  <Food Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""Output"" >
                                    <FoodName Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" />
                                  </Food>
                                </DataList>";
            var dataListModel = new DataListModel();
            //------------Execute Test---------------------------
            dataListModel.Create(Data, Shape);


            Assert.AreEqual(0, dataListModel.RecordSets.Count);
            Assert.AreEqual(0, dataListModel.ShapeRecordSets.Count);
            Assert.AreEqual(0, dataListModel.Scalars.Count);
            Assert.AreEqual(0, dataListModel.ShapeScalars.Count);


            const string expectedValue = "{\r\n  \"Description\": \"\",\r\n  \"IsEditable\": \"True\",\r\n  \"IsJson\": \"True\",\r\n  \"IsArray\": \"False\",\r\n  \"ColumnIODirection\": \"Output\",\r\n  \"FoodName\": {\r\n    \"Description\": \"\",\r\n    \"IsEditable\": \"True\",\r\n    \"IsJson\": \"True\",\r\n    \"IsArray\": \"False\",\r\n    \"ColumnIODirection\": \"None\"\r\n  }\r\n}";

            Assert.AreEqual(1, dataListModel.ComplexObjects.Count);
            Assert.AreEqual("", dataListModel.ComplexObjects[0].Description);
            Assert.AreEqual("@Food", dataListModel.ComplexObjects[0].Name);
            Assert.AreEqual(expectedValue, dataListModel.ComplexObjects[0].Value);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Output, dataListModel.ComplexObjects[0].IODirection);
            Assert.AreEqual(false, dataListModel.ComplexObjects[0].IsArray);
            Assert.AreEqual(false, dataListModel.ComplexObjects[0].IsEditable);
            Assert.AreEqual(null, dataListModel.ComplexObjects[0].Parent);
            Assert.AreEqual(0, dataListModel.ComplexObjects[0].Children.Count);


            Assert.AreEqual(1, dataListModel.ShapeComplexObjects.Count);
            Assert.AreEqual("", dataListModel.ShapeComplexObjects[0].Description);
            Assert.AreEqual("@Food", dataListModel.ShapeComplexObjects[0].Name);
            Assert.AreEqual(expectedValue, dataListModel.ShapeComplexObjects[0].Value);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Output, dataListModel.ShapeComplexObjects[0].IODirection);
            Assert.AreEqual(false, dataListModel.ShapeComplexObjects[0].IsArray);
            Assert.AreEqual(false, dataListModel.ShapeComplexObjects[0].IsEditable);
            Assert.AreEqual(null, dataListModel.ShapeComplexObjects[0].Parent);
            Assert.AreEqual(0, dataListModel.ShapeComplexObjects[0].Children.Count);
        }


        [TestMethod]
        [Owner("Yogesh RajPurohit")]
        [TestCategory(nameof(DataListModel))]
        public void DataListModel_Create_PayLoadWithComplexObjectValue_ShouldMatchExpectedValue()
        {
            //------------Setup for test--------------------------
            const string Data = @"<DataList>
                                    <RequestPayload>
                                        <EmailAddress>Yogesh.rajpurohit@gmail.com</EmailAddress>
                                        <FirstName>Sune</FirstName>
                                        <DisplayNumber>TU00000</DisplayNumber>
                                        <MobilePhone>27832640</MobilePhone>
                                    </RequestPayload>
                                </DataList>";

            const string Shape = @"
                                <DataList>
                                    <RequestPayload Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""Input"">
                                    <EmailAddress Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None""></EmailAddress>    
                                    <FirstName Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None""></FirstName>
                                    <DisplayNumber Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None""></DisplayNumber>
                                    <MobilePhone Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None""></MobilePhone>                               
                                    </RequestPayload>
                                </DataList>";
            var dataListModel = new DataListModel();
            //------------Execute Test---------------------------
            dataListModel.Create(Data, Shape);


            Assert.AreEqual(0, dataListModel.RecordSets.Count);
            Assert.AreEqual(0, dataListModel.ShapeRecordSets.Count);
            Assert.AreEqual(0, dataListModel.Scalars.Count);
            Assert.AreEqual(0, dataListModel.ShapeScalars.Count);


            const string expectedValue = "{\r\n  \"EmailAddress\": \"Yogesh.rajpurohit@gmail.com\",\r\n  \"FirstName\": \"Sune\",\r\n  \"DisplayNumber\": \"TU00000\",\r\n  \"MobilePhone\": \"27832640\"\r\n}";

            Assert.AreEqual(1, dataListModel.ComplexObjects.Count);
            Assert.AreEqual("", dataListModel.ComplexObjects[0].Description);
            Assert.AreEqual("@RequestPayload", dataListModel.ComplexObjects[0].Name);
            Assert.AreEqual(expectedValue, dataListModel.ComplexObjects[0].Value);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Input, dataListModel.ComplexObjects[0].IODirection);
            Assert.AreEqual(false, dataListModel.ComplexObjects[0].IsArray);
            Assert.AreEqual(false, dataListModel.ComplexObjects[0].IsEditable);
            Assert.AreEqual(null, dataListModel.ComplexObjects[0].Parent);
            Assert.AreEqual(0, dataListModel.ComplexObjects[0].Children.Count);
        }

    }
}
