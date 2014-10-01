
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.ViewModels.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.UtilsTests
{
    /// <summary>
    /// Summary description for NewWorkflowNamesTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
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
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string Data = "<DataList></DataList>";
            const string Shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            //------------Execute Test---------------------------
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), Data, Shape, out errors);
            var bdl = compiler.FetchBinaryDataList(dlID, out errors);

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
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string Data = "<DataList></DataList>";
            const string Shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            //------------Execute Test---------------------------
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), Data, Shape, out errors);
            var bdl = compiler.FetchBinaryDataList(dlID, out errors);

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
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string Data = "<DataList></DataList>";
            const string Shape = @"<DataList><scalar Description="""" IsEditable=""True"" ColumnIODirection=""Input"" ></scalar></DataList>";

            //------------Execute Test---------------------------
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), Data, Shape, out errors);
            var bdl = compiler.FetchBinaryDataList(dlID, out errors);

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
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string Data = "<DataList></DataList>";
            const string Shape = @"<DataList><scalar Description="""" IsEditable=""True"" ColumnIODirection=""Both"" ></scalar></DataList>";

            //------------Execute Test---------------------------
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), Data, Shape, out errors);
            var bdl = compiler.FetchBinaryDataList(dlID, out errors);

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
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string Data = "<DataList></DataList>";
            const string Shape = @"<DataList><scalar Description="""" IsEditable=""True"" ColumnIODirection=""None"" ></scalar></DataList>";

            //------------Execute Test---------------------------
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), Data, Shape, out errors);
            var bdl = compiler.FetchBinaryDataList(dlID, out errors);

            var result = converter.CreateListToBindTo(bdl);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }
    }
}
