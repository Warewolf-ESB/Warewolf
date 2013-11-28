using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebpartConfiguration.Test;
//using Dev2ApplicationServer.Unit.Tests;
using Dev2.Integration.Tests.Helpers;
using Dev2.Integration.Tests.MEF;

namespace Dev2.Integration.Tests.BPM.Unit.Test
{

    // PBI 5376 : Do not work due to issue with databinding occuring - Seems For Each related.
    // Unlimited.Applications/BusinessDesignStudio.Activities - DsfForEachActivity.cs

    /// <summary>
    /// Summary description for DataGridTest
    /// </summary>
    [TestClass]
    public class DataGridTest
    {
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();
        public DataGridTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        private TestContext testContextInstance;
        string WebserverURI = ServerSettings.WebserverURI;
        string DataGrid = "Data Grid";


        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            tempXmlString = string.Empty;
            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.DataGrid_DataList, "Dev2BoundServiceName", "ServiceToBindFrom");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2RowDelimiter", "regions");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2RowsPerGrid", "5");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2GridMappingJson", @"({ cols: [ { Dev2ColumnHeader: ""ID"",Dev2Field: ""id"",Dev2GridWidth: ""250"",Dev2ColumnAlignment: ""Left"" }, { Dev2ColumnHeader: ""NameOf"",Dev2Field: ""name"",Dev2GridWidth: ""550"",Dev2ColumnAlignment: ""Left"" } ]})");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ElementName, "Test Data Grid");
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region  Expected User - Service Binding

        // PBI 6278 : Issue caused in ForEach Tool
        /*
        [TestMethod()]
        public void DataGrid_AdvancedOptions_ServiceBinding_Expected_DataReturnedFromServiceAndDisplayedInGrid()
        {
            // PBI 5376: Issue caused in ForEach Tool
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DataGrid, tempXmlString);
            string expected = TestResource.DataGrid_ServiceBindingResult;

            string ReponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ReponseData);

            Assert.AreEqual(expected, actual);

        }
         * */
        #endregion
        

        #region  Expected User - Testing With Missing Values

        [TestMethod()]
        public void DataGrid_AdvancedOptions_MissingServiceName_Expected_SpanGeneratedWithWebpartError()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2BoundServiceName", "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DataGrid, tempXmlString);
            string expected = @"<span class=""internalError"">Webpart Render Error : No Service Name</span>";

            string ReponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ReponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);

        }
        

        [TestMethod()]
        public void DataGridTest_AdvancedOptions_MissingFieldDelimeter_Expected_SpanGeneratedWithWebpartError()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2RowDelimiter", "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DataGrid, tempXmlString);
            string expected = @"<span class=""internalError"">Webpart Render Error : No Row Delimeter</span>";

            string ReponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ReponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region EditingService
        // PBI 6278: Issue caused in ForEach Tool
        /*
        [TestMethod]
        public void DataGrid_Editing_Service_Renders_Toolbar_Expected_ToolbarforEditingServiceDisplayed()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2elementGridEditingService", "ServiceToBindFrom");

            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DataGrid, tempXmlString);
            string expected = TestResource.Data_Grid_Editing_Service_Result;

            string ReponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ReponseData);

            Assert.AreEqual(expected, actual);
        }
         * */
        #endregion

        #region Grid Width Tests

        // PBI 6278: Issue caused in ForEach Tool
        /*
        [TestMethod]
        public void DataGridFixed_Width_Expected_IsRendered_As_Such()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2GridWidth", "850");

            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DataGrid, tempXmlString);
            string expected = TestResource.Data_Grid_Fixed_Width_Result;

            string ReponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ReponseData);

            Assert.AreEqual(expected, actual);
        }
        */

        // PBI 6278: Issue caused in ForEach Tool
        /*
        [TestMethod]
        public void DataGrid_All_Rows_Iteration_Expected_Renders_As_Such()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2RowsPerGrid", "All");

            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DataGrid, tempXmlString);
            string expected = TestResource.Data_Grid_All_Rows_Result;

            string ReponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ReponseData);

            Assert.AreEqual(expected, actual);
        }
        */
        #endregion GridWidth

    }
}
