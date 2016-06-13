
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data;
using Dev2.ViewModels.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.UtilsTests
{
    /// <summary>
    /// Summary description for NewWorkflowNamesTests
    /// </summary>
    [TestClass]
    public class DataListConversionUtilTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListConversionUtilTest_CreateListToBindTo")]
        // ReSharper disable InconsistentNaming
        public void DataListConversionUtilTest_CreateListToBindTo_WhenColumnHasInputMapping_ExpectCollectionWithOneItem()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var converter = new DataListConversionUtils();

            const string Data = "<DataList></DataList>";
            const string Shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            //------------Execute Test---------------------------
            var bdl = new DataListModel();
            bdl.Create(Data, Shape);
            var result = converter.CreateListToBindTo(bdl);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("rec", result[0].Recordset);
            Assert.AreEqual("a", result[0].Field);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListConversionUtilTest_CreateListToBindTo")]
        // ReSharper disable InconsistentNaming
        public void DataListConversionUtilTest_CreateListToBindTo_WhenColumnHasBothMapping_ExpectCollectionWithOneItem()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var converter = new DataListConversionUtils();


            const string Data = "<DataList></DataList>";
            const string Shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            //------------Execute Test---------------------------
            var bdl = new DataListModel();
            bdl.Create(Data, Shape);
            var result = converter.CreateListToBindTo(bdl);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("rec", result[0].Recordset);
            Assert.AreEqual("a", result[0].Field);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListConversionUtilTest_CreateListToBindTo")]
        // ReSharper disable InconsistentNaming
        public void DataListConversionUtilTest_CreateListToBindTo_WhenScalarHasInputMapping_ExpectCollectionWithOneItem()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var converter = new DataListConversionUtils();

            const string Data = "<DataList></DataList>";
            const string Shape = @"<DataList><scalar Description="""" IsEditable=""True"" ColumnIODirection=""Input"" ></scalar></DataList>";

            //------------Execute Test---------------------------
            var bdl = new DataListModel();
            bdl.Create(Data, Shape);
            var result = converter.CreateListToBindTo(bdl);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("scalar", result[0].Field);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListConversionUtilTest_CreateListToBindTo")]
        // ReSharper disable InconsistentNaming
        public void DataListConversionUtilTest_CreateListToBindTo_WhenScalarHasBothMapping_ExpectCollectionWithOneItem()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var converter = new DataListConversionUtils();

            const string Data = "<DataList></DataList>";
            const string Shape = @"<DataList><scalar Description="""" IsEditable=""True"" ColumnIODirection=""Both"" ></scalar></DataList>";

            //------------Execute Test---------------------------
            var bdl = new DataListModel();
            bdl.Create(Data, Shape);
            var result = converter.CreateListToBindTo(bdl);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("scalar", result[0].Field);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListConversionUtilTest_CreateListToBindTo")]
        // ReSharper disable InconsistentNaming
        public void DataListConversionUtilTest_CreateListToBindTo_WhenScalarHasNoneMapping_ExpectCollectionWithNoItems()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var converter = new DataListConversionUtils();

            const string Data = "<DataList></DataList>";
            const string Shape = @"<DataList><scalar Description="""" IsEditable=""True"" ColumnIODirection=""None"" ></scalar></DataList>";

            //------------Execute Test---------------------------
            var bdl = new DataListModel();
            bdl.Create(Data, Shape);
            var result = converter.CreateListToBindTo(bdl);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListModel_Create")]
        public void DataListModel_Create_PayLoadWithComplexObjects_ShouldHaveComplexObjectItems()
        {
            //------------Setup for test--------------------------
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
            const string Data = "<DataList></DataList>";
            var dataListModel = new DataListModel();
            var converter = new DataListConversionUtils();
            //------------Execute Test---------------------------
            dataListModel.Create(Data,Shape);
            var result = converter.CreateListToBindTo(dataListModel);
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(5,result.Count);
            Assert.AreEqual("Country", result[0].DisplayValue);
            Assert.AreEqual("Person.Age",result[1].DisplayValue);
            Assert.IsTrue(result[1].IsObject);
            Assert.AreEqual("Person.Name",result[2].DisplayValue);
            Assert.AreEqual("Person.Schools.Name",result[3].DisplayValue);
            Assert.IsTrue(result[3].IsObject);
            Assert.AreEqual("Person.Schools.Location",result[4].DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListModel_Create")]
        public void DataListModel_Create_PayLoadWithComplexObjectsWithArrays_ShouldHaveComplexObjectItems()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList>
                                    <Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" >
                                        <Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" />
                                        <Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" />
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
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual("Country", result[0].DisplayValue);
            Assert.AreEqual("Person.Age", result[1].DisplayValue);
            Assert.IsTrue(result[1].IsObject);
            Assert.AreEqual("Person.Name", result[2].DisplayValue);
            Assert.AreEqual("Person.Schools(1).Name", result[3].DisplayValue);
            Assert.IsTrue(result[3].IsObject);
            Assert.AreEqual("Person.Schools(1).Location", result[4].DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListModel_Create")]
        public void DataListModel_Create_PayLoadWithComplexObjectsWithParentArray_ShouldHaveComplexObjectItems()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList>
                                    <Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" >
                                        <Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" />
                                        <Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" />
                                    </Car>
                                    <Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" />
                                    <Person Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""True"" ColumnIODirection=""Both"" >
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
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual("Country", result[0].DisplayValue);
            Assert.AreEqual("Person(1).Age", result[1].DisplayValue);
            Assert.IsTrue(result[1].IsObject);
            Assert.AreEqual("Person(1).Name", result[2].DisplayValue);
            Assert.AreEqual("Person(1).Schools(1).Name", result[3].DisplayValue);
            Assert.IsTrue(result[3].IsObject);
            Assert.AreEqual("Person(1).Schools(1).Location", result[4].DisplayValue);
        }
    }
}
