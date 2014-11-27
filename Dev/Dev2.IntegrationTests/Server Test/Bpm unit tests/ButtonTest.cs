
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
using System.Xml;
using Dev2.Integration.Tests.Helpers;
using Dev2.Integration.Tests.MEF;
using Dev2.Integration.Tests.Enums;

namespace Dev2.Integration.Tests.BPM.Unit.Test
{
    /// <summary>
    /// All test cases for the different rendering options of the button webpart.
    /// </summary>
    [TestClass]
    public class ButtonTest
    {
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();
        #region Pre And Post Test Activities

        public ButtonTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        string WebserverURI = ServerSettings.WebserverURI;
        //private static ProcessInvoker _process;
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
        //    System.Threading.Thread.Sleep(5000);
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
        public void MyTestInitialize()
        {
            tempXmlString = string.Empty;
            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.Button_DataList, TestResource.Xpath_DisplayText, "testButtonName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ErrorMsg, "testButtonErrorMessage");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testButtonToolTip");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "1");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CustomStyle, "testButtonCustomStyle");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Fragment, "testButtonFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "btnType", "Submit");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ElementName, "testButtonElementName");
        }

        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Expected User Interaction

        [TestMethod]
        public void Button_ExpectedUserInteraction_NoNameTest_Expected_ButtonCreatedWithoutText()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ElementName, "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.AreEqual(1, actuals.Where(c => c.Contains(@"<button type=""Submit"" name=""ButtonClicked""")).ToList().Count);

        }

        [TestMethod]
        public void Button_ExpectedUserInteractionSubmitButton_Expected_ButtonTypeSubmitGenerated()
        {
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);


            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.AreEqual(1, actuals.Where(c => c.Contains(@"<button type=""Submit""")).ToList().Count);
        }

        [TestMethod()]
        public void Button_ExpectedUserInteractionResetButton_Expected_ButtonTypeResetGenerated()
        {

            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "btnType", "Reset");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);
            string expected = @"<button type=""Reset"" name=""testButtonElementName"" id=""testButtonElementName"" value=""testButtonName"" title=""testButtonToolTip"" tabindex=""1"" class=""testButtonCustomStyle"" style=""height:4px;width:4px;"">testButtonName</button>]]>";

            string ReponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ReponseData);

            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.AreEqual(1, actuals.Where(c => c.Contains(@"<button type=""Reset""")).ToList().Count);
        }

        [TestMethod()]
        public void Button_ExpectedUserInteractionTextintabIndex_Expected_ButtonCreatedTabIndexNotEqualToOne()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "2");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);


            string ReponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ReponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.AreEqual(1, actuals.Where(c => c.Contains(@"tabindex=""2""")).ToList().Count);
        }

        [TestMethod()]
        public void Button_ExpectedUserInteraction_NoDisplayText_Expected_SpanGeneratedwithWebpartErrorMessage()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.Button_DataList, TestResource.Xpath_DisplayText, "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.AreEqual(1, actuals.Where(c => c.Contains(@"internalError"">") && c.Contains("Webpart Render Error : No Display Text Assigned")).ToList().Count);
        }

        #endregion

        #region Advanced Options - Custom Javascript

        [TestMethod()]
        public void Button_AdvancedOptions_SimpleJavaScript_Expected_JavascriptRegionGeneratedWithSimpleJavascriptCode()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "buttonScriptRegionuseRegion", @"alert('Chasdigal wants a beer frig!!!!');");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);
            string expected = @"<script>  $(""#testButtonElementName"").click(function(){      alert('Chasdigal wants a beer frig!!!!');   });</script>";
            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            actual = actual.Replace("\n", "").Replace("\r", "");

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void Button_AdvancedOptions_ComplexJavaScript_Expected_JavascriptRegionGeneratedWithComplexJavascriptCode()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "buttonScriptRegionuseRegion", @"var xmlHttpRequest = new XMLHttpRequest();   var url = ""http://localhost:2234/services/sashen_workflow"";   xmlHttpRequest.open(""GET"", url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById(""lblTest"").innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);
            string expected = @"<script>  $(""#testButtonElementName"").click(function(){      var xmlHttpRequest = new XMLHttpRequest();   var url = ""http://localhost:2234/services/sashen_workflow"";   xmlHttpRequest.open(""GET"", url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById(""lblTest"").innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);   });</script>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            actual = actual.Replace("\n", "").Replace("\r", "");

            StringAssert.Contains(actual, expected);
        }


        [TestMethod()]
        public void Button_AdvancedOptions_MalformedJavascript_Expected_JavascriptRegionGeneratedWithMalformedJavascriptCode()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "buttonScriptRegionuseRegion", @"var xmlHttpRequest = new XMLHttpRequest();   var url = ""http://localhost:2234/services/sashen_workflow"";   xmlHttpRequpen""GET"", url, true);   xmlHttpRequest.onreadystatechange = function){      document.getElementById(""lblTest"").innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);
            string expected = @"<script>  $(""#testButtonElementName"").click(function(){      var xmlHttpRequest = new XMLHttpRequest();   var url = ""http://localhost:2234/services/sashen_workflow"";   xmlHttpRequpen""GET"", url, true);   xmlHttpRequest.onreadystatechange = function){      document.getElementById(""lblTest"").innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);   });</script>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            actual = actual.Replace("\n", "").Replace("\r", "");
            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Advanced Options - Altering Width and Height

        [TestMethod()]
        public void Button_AdvancedOptions_UsingWordsInHeightBlock_Expected_ButtonGeneratedWithCharactersInHeightBlock()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);
            string expected = @"style=""height:text;width:4px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            StringAssert.Contains(actual, expected);
        }

        //Sashen - 28-11-2012 - This test seems to cause the a timeout exception, investigate please
        [TestMethod()]
        public void Button_AdvancedOptions_AlteringHeight_Expected_ButtonGeneratedWithHeightSpecifiedInHeightBlock()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "100px");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);
            string expected = @"style=""height:100px;width:4px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.AreEqual(1, actuals.Where(c => c.Contains(expected)).ToList().Count);
        }

        [TestMethod()]
        public void Button_AdvancedOptions_AlteringWidth_Expected_ButtonGeneratedWithWidthSpecifiedInBlock()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "100px");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);
            string expected = @"<button type=""Submit"" name=""testButtonElementName"" id=""testButtonElementName"" value=""testButtonName"" title=""testButtonToolTip"" tabindex=""1"" class=""testButtonCustomStyle"" style=""height:4px;width:100px;"">testButtonName</button>]]>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.AreEqual(1, actuals.Where(c => c.Contains(@"style=""height:4px;width:100px;""")).ToList().Count);
        }

        [TestMethod()]
        public void Button_AdvancedOptions_UsingWordsInWidthBlock_ButtonGeneratedWithCharactersInWidthBlock()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);
            string expected = @"<button type=""Submit"" name=""testButtonElementName"" id=""testButtonElementName"" value=""testButtonName"" title=""testButtonToolTip"" tabindex=""1"" class=""testButtonCustomStyle"" style=""height:4px;width:text;"">testButtonName</button>]]>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.AreEqual(1, actuals.Where(c => c.Contains(@"width:text;")).ToList().Count);
        }

        #endregion

        #region Advanced Options - Custom Styling

        [TestMethod()]
        public void Button_AdvancedOptions_CustomStyling_Expected_ButtonGeneratedWithClassFieldPopulated()
        {
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);
            //string expected = @"<button type=""Submit"" name=""testButtonElementName"" id=""testButtonElementName"" value=""testButtonName"" title=""testButtonToolTip"" tabindex=""1"" class=""testButtonCustomStyle"" style=""height:4px;width:4px;"">testButtonName</button>]]>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.AreEqual(1, actuals.Where(c => c.Contains(@"class=""testButtonCustomStyle""")).ToList().Count);
        }

        #endregion

        #region Advanced Options - Tab Index

        [TestMethod()] // WHATS THE POINT OF NOT CHANGING THE TAB INDEX!!!!! -- Rectified
        public void Button_AdvancedOptions_TabIndex_Expected_ButtonGeneratedWithCustomTabIndex()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "4");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);


            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);

            Assert.AreEqual(1, actuals.Where(c => c.Contains(@"tabindex=""4""")).ToList().Count);
        }

        #endregion

        #region Advanced Options - Tooltip Text

        [TestMethod()]
        public void Button_AdvancedOptions_TooltipText_ButtonGeneratedWithTestingTooltipText()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "NewTestButtonToolTip");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);
            string expected = @"<button type=""Submit"" name=""testButtonElementName"" id=""testButtonElementName"" value=""testButtonName"" title=""testButtonToolTip"" tabindex=""1"" class=""testButtonCustomStyle"" style=""height:4px;width:4px;"">testButtonName</button>]]>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            List<string> actuals = TestHelper.BreakHTMLElement(actual);

            Assert.AreEqual(1, actuals.Where(c => c.Contains(@"title=""NewTestButtonToolTip""")).ToList().Count);
        }

        #endregion
    }
}
