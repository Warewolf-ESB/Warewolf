using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;
using Dev2.Integration.Tests.Enums;
using Dev2.Integration.Tests.MEF;

namespace Dev2.Integration.Tests.BPM.Unit.Test
{
    /// <summary>
    /// Summary description for HtmlWidget
    /// </summary>
    [TestClass]
    public class HtmlWidget
    {
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();

        public HtmlWidget()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        string WebserverURI = ServerSettings.WebserverURI;

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
        public void MyTestInitialize() {
           
            tempXmlString = string.Empty;

            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.HTMLWidget_DataList, TestResource.Xpath_Fragment, "testHTMLWidgetFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CssClass, "testHTMLWidgetCssClass");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "DataValue", "testHTMLWidgetDataValue");           
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void HTMLWidget_ExpectedUserInteraction_PositiveCreation_Expected_SpecifiedHTMLObjectCreated() {

            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.HtmlWidget, tempXmlString);
            string expected = @"<div class=""testHTMLWidgetCssClass""> testHTMLWidgetDataValue </div>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            //Remove line carriages
            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);

        }
    }
}
