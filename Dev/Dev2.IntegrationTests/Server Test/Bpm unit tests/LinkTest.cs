
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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.MEF;
using Dev2.Integration.Tests.Enums;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.BPM.Unit.Test {
    /// <summary>
    /// Summary description for LinkTest
    /// </summary>
    [TestClass]
    public class LinkTest {
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();
        #region Pre and Post Test Activities

        public LinkTest() {
            //
            // TODO: Add constructor logic here
            //
        }
        private string WebserverURL = ServerSettings.WebserverURI;
        private TestContext testContextInstance;
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

            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.Link_DataList, TestResource.Xpath_DisplayText, "testLinkDisplayText");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ErrorMsg, "testLinkErrorMessage");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Fragment, "testLinkFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CssClass, "testLinkCssClass");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "testLinkTabIndex");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testLinkToolTip");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customStylelink", "testLinkCustomStyle");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "testLinkUri");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "customScripting", "testLinkCustomScripting");
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Expected User Interaction Cases

        [TestMethod()]
        public void Link_ExpectedUserInteraction_LinkToExternalSite_Expected_LinkRefPointingToExternalSite() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "http://www.google.com");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""http://www.google.com"" title=""testLinkToolTip"" tabindex=""testLinkTabIndex"" class=""testLinkCustomStyle"" style=""height:4px;width:4px;"" >testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #region Expected User Interaction Cases with wrong address

        [TestMethod()]
        public void Link_ExpectedUserInteraction_LinkEmpty_Expected_LinkCreatedWithoutReference() {
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""testLinkUri"" title=""testLinkToolTip"" tabindex=""testLinkTabIndex"" class=""testLinkCustomStyle"" style=""height:4px;width:4px;"" >testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }
        #endregion

        [TestMethod()]
        public void Link_ExpecteduserInteraction_LinkToAnotherWorkflow_Expected_LinkCreatedWithRefToAnotherWorkflow() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "http://localhost:2234/services/Button_Wizard");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""http://localhost:2234/services/Button_Wizard"" title=""testLinkToolTip"" tabindex=""testLinkTabIndex"" class=""testLinkCustomStyle"" style=""height:4px;width:4px;"" >testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Link_ExpectedUserInteraction_LinkWithoutProtocol_LinkCreatedWithoutProtocol() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "localhost:2234/services/Button_Wizard");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""localhost:2234/services/Button_Wizard"" title=""testLinkToolTip"" tabindex=""testLinkTabIndex"" class=""testLinkCustomStyle"" style=""height:4px;width:4px;"" >testLinkDisplayText</a>testLinkCustomScripting";
            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Link_ExpectedUserInteraction_LinkToAnFTPSite_LinkCreatedToFTPSite() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "ftp://ftp.secureftp-test.com");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""ftp://ftp.secureftp-test.com"" title=""testLinkToolTip"" tabindex=""testLinkTabIndex"" class=""testLinkCustomStyle"" style=""height:4px;width:4px;"" >testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        public void Link_ExpectedUserInteraction_InputSpecialCharactersInDisplayText_Test() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "www.google.com/audi/cars/");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""www.google.com/audi/cars/"" title=""testLinkToolTip"" tabindex=""testLinkTabIndex"" class=""testLinkCustomStyle"" style=""height:4px;width:4px;"" >testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region Advanced Options - Open In New Tab

        [TestMethod()]
        public void Link_AdvancedOptions_OpenInNewTab_Expected_TargetAttributeSetToBlank() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2LinkInTab", "yes");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "http://www.google.co.za");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""http://www.google.co.za"" title=""testLinkToolTip"" tabindex=""testLinkTabIndex"" class=""testLinkCustomStyle"" style=""height:4px;width:4px;"" target=""_blank"">testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Custom Styling

        [TestMethod()]
        public void Link_AdvancedOptions_CustomStyling_ApplyingCustomStyle_Expected_ClassAttributeSetToSpecifiedStyle() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "http://www.google.com");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""http://www.google.com"" title=""testLinkToolTip"" tabindex=""testLinkTabIndex"" class=""testLinkCustomStyle"" style=""height:4px;width:4px;"" >testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Tab Index

        [TestMethod()]
        public void Link_AdvancedOptions_TabIndex_Expected_CustomTabIndexOnWebpart() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "2");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "http://www.google.com");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""http://www.google.com"" title=""testLinkToolTip"" tabindex=""2"" class=""testLinkCustomStyle"" style=""height:4px;width:4px;"" >testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Link_AdvancedOptions_TabIndexWithText_Expected_CustomTabIndexWithTextOnWebpart() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "text");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "http://www.google.com");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""http://www.google.com"" title=""testLinkToolTip"" tabindex=""text"" class=""testLinkCustomStyle"" style=""height:4px;width:4px;"" >testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Tooltip Text

        [TestMethod()]
        public void Link_AdvancedOptions_TooltipText_Expected_CustomTabIndexOnWebpart() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testingToolTip");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "http://www.google.com");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""http://www.google.com"" title=""testingToolTip"" tabindex=""testLinkTabIndex"" class=""testLinkCustomStyle"" style=""height:4px;width:4px;"" >testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void Link_AdvancedOptions_TooltipTextIntegerValue_Expected_TextasTabIndex() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "12");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "http://www.google.com");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""http://www.google.com"" title=""12"" tabindex=""testLinkTabIndex"" class=""testLinkCustomStyle"" style=""height:4px;width:4px;"" >testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);

        }

        #endregion

        #region Advanced Options - Width and Height

        [TestMethod()]
        public void Link_AdvancedOptions_AlterWidth_Test() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "20");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "http://www.google.com");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""http://www.google.com"" title=""testLinkToolTip"" tabindex=""testLinkTabIndex"" class=""testLinkCustomStyle"" style=""height:4px;width:20px;"" >testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Link_AdvancedOptions_AlterHeight_Test() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "20");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2uri", "http://www.google.com");
            string PostData = String.Format("{0}{1}?{2}", WebserverURL, WebpartType.Link, tempXmlString);
            string expected = @"<a href=""http://www.google.com"" title=""testLinkToolTip"" tabindex=""testLinkTabIndex"" class=""testLinkCustomStyle"" style=""height:20px;width:4px;"" >testLinkDisplayText</a>testLinkCustomScripting";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

    }
}
