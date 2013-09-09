using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.MEF;
using Dev2.Integration.Tests.Enums;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.BPM.Unit.Test {
    /// <summary>
    /// Summary description for LabelTest
    /// </summary>
    [TestClass]
    public class LabelTest {
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();

        #region Pre and Post Test Activities

        public LabelTest() {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        string WebserverUrl = ServerSettings.WebserverURI;
        //private static ProcessInvoker _process;
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

        #endregion

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //

        //// Initialize requirements before test start

        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //    string processName = "Unlimited.Applications.DynamicServicesHost.exe";
        //    FileIO myfile = new FileIO();
        //    string file = myfile.ReturnPathOfLatest(processName);
        //    string mydirectory = myfile.ReturnLatestDirectory();
        //    myfile.CopyRequiredFiles();
        //    System.Threading.Thread.Sleep(1000);
        //    _process = new ProcessInvoker(file, processName);
        //    _process.InvokeProcess();
        //    System.Threading.Thread.Sleep(5000);
        //}

        //// Kill the Webserver for the next test run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //    _process.KillWebserver();
        //    System.Threading.Thread.Sleep(1000);
        //}
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize() {

            tempXmlString = string.Empty;

            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.Label_DataList, TestResource.Xpath_DisplayText, "testLabelDisplayText");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ErrorMsg, "testLabelErrorMessage");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Fragment, "testLabelFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ElementName, "testLabelElementName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CssClass, "testLabelCssClass");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "testLabelTabIndex");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customStyleLabel", "testLabelCustomStyle");


        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Expected User Interaction

        [TestMethod]
        public void Label_ExpectedUserInteraction_PositiveCase_Expected_LabelGenerated() {
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Label, tempXmlString);
            string expected = @"<label id=""testLabelElementName""  class=""testLabelCustomStyle"" tabindex=""testLabelTabIndex"" style=""height:4px;width:4px;"">testLabelDisplayText</label>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void Label_ExpectedUserInteraction_LabelWithoutNameField_WebpartDispayedWithoutIdAttributeSet() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ElementName, "");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Label, tempXmlString);
            string expected = @"<label id=""""  class=""testLabelCustomStyle"" tabindex=""testLabelTabIndex"" style=""height:4px;width:4px;"">testLabelDisplayText</label>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);


            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Custom Styling

        [TestMethod()]
        public void Label_AdvancedOptions_CustomStyling_Expected_ClassAttributeSetToSpecifiedStyle() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CustomStyle, "testCustomStyle");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Label, tempXmlString);
            string expected = @"class=""testLabelCustomStyle""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Advanced Options - Width and Height

        [TestMethod()]
        public void Label_AdvancedOptions_AdjustWidth_Expected_WidthSpecifiedInWidthBlock() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "20");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Label, tempXmlString);
            string expected = @"style=""height:4px;width:20px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void Label_AdvancedOptions_AdjustWidthWithText_Expected_WidthAttributeSetOnWebpartToText() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Label, tempXmlString);
            string expected = @"style=""height:4px;width:text;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void Label_AdvancedOptions_AdjustHeigh_Expected_HeightSpecifiedInBlock() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "20");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Label, tempXmlString);
            string expected = @"style=""height:20px;width:4px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void Label_AdvancedOptions_AdjustHeighWithText_Expected_HeightAttributeSetOnWebpartToText() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Label, tempXmlString);
            string expected = @"style=""height:text;width:4px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Advanced options - Tab Index

        [TestMethod()]
        public void Label_AdvancedOptions_TabIndex_Test()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "10");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Label, tempXmlString);
            string expected = @"tabindex=""10""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }


        #endregion
    }
}
