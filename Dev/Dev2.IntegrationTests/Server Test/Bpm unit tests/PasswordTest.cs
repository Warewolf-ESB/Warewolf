
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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
using WebpartConfiguration.Test.Test_Utils;
//using Dev2ApplicationServer.Unit.Tests;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Dev2.Integration.Tests.Enums;
using Dev2.Integration.Tests.MEF;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.BPM.Unit.Test
{
    /// <summary>
    /// Summary description for PasswordTest
    /// </summary>
    [TestClass]
    public class PasswordTest
    {
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();

        #region Pre and Post Test Activities

        public PasswordTest()
        {
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
        public void MyTestInitialize()
        {
            tempXmlString = string.Empty;

            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.Password_DataList, TestResource.Xpath_DisplayText, "testPasswordDisplayText");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ElementName, "testPasswordElementName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ErrorMsg, "testPasswordErrorMessage");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "showTextP", "on");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "requiredP", "true");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationP", "true");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2minCharactersTextbox", "2");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2maxCharactersTextbox", "10");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customStylePassword", "testPasswordCustomStyle");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "1");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testPasswordToolTip");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Fragment, "testPasswordFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CssClass, "testPasswordCssClass");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "InjectedLabel", "testPasswordInjectedLabel");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "InjectStar", "testPasswordInjectStar");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2validationErrMsgP", "testPasswordValidationErrMsg");

        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Expected User Interactions

        [TestMethod()]
        public void Password_Positve_Expected_WebpartCorrectlyGenerated()
        {
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""1"" class=""testPasswordCustomStyle"" style=""height:4px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Password_NoNameFieldSpecified_Expected_SpanContainingError()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ElementName, "");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""internalError"">Webpart Render Error : No Name Assigned</span>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        // PBI 6278 : Error not appearing in span
        /*
        [TestMethod()]
        public void Password_NoDisplayTextFieldSpecified_Expected_SpanContainingError()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.Password_DataList, TestResource.Xpath_DisplayText, "");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""internalError"">Webpart Render Error : No Name Assigned</span>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }
        */
        #endregion

        #region Advanced Options - Custom Styling

        [TestMethod()]
        public void Password_AdvancedOptions_CustomStyling_Expected_ClassAttributeSetToSpecifiedStyle()
        {
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""1"" class=""testPasswordCustomStyle"" style=""height:4px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Tooltip Text

        [TestMethod()]
        public void Password_AdvancedOptions_TooltipText_Expected_CustomTabIndexOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testTooltip");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testTooltip"" tabindex=""1"" class=""testPasswordCustomStyle"" style=""height:4px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Password_AdvancedOptions_TooltipTextWithIntegerValue_Expected_()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "12");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""12"" tabindex=""1"" class=""testPasswordCustomStyle"" style=""height:4px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Tab Index

        [TestMethod()]
        public void PasswordTest_AdvancedOptions_TabIndex_Expected_CustomTabIndexOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "12");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""12"" class=""testPasswordCustomStyle"" style=""height:4px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void PasswordTest_AdvancedOptions_TabIndexTextValue_Expected_CustomTabIndexWithTextOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""text"" class=""testPasswordCustomStyle"" style=""height:4px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Altering Width and Height

        [TestMethod()]
        public void Password_AdvanceOptions_AlterHeight_Expected_HeightSpecifiedInHeightBlock()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "20");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""1"" class=""testPasswordCustomStyle"" style=""height:20px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Password_AdvanceOptions_AlterHeightText_Expected_HeightAttributeSetOnWebpartToText()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""1"" class=""testPasswordCustomStyle"" style=""height:text;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Password_AdvancedOptions_AlterWidth_Expected_WidthSpecifiedInWidthBlock()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "20");

            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""1"" class=""testPasswordCustomStyle"" style=""height:4px;width:20px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Password_AdvancedOptions_AlterWidthText_Expected_WidthAttributeSetOnWebpartToText()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "text");

            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""1"" class=""testPasswordCustomStyle"" style=""height:4px;width:text;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Custom Scripting

        // PBI 5376 : Script Tags not generated for custom script
        // Test currently broken: BugId 6600
        //[TestMethod()]
        //public void Password_AdvancedOptions_SimpleCustomScript_Expected_SimpleJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptP", "alert('Chasdigal wants a beer frig!!!!');");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
        //    string expected = "<script>alert('Chasdigal wants a beer frig!!!!');</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
        //    actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", ""); ;

        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376: Same As above

        //[TestMethod()]
        //public void Password_AdvancedOptions_ComplexCustomScript_Expected_ComplexJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptP", @"var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
        //    actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376 : Same as above

        //[TestMethod()]
        //public void Password_AdvancedOptions_MalformedCustomScript_Expected_MalformedJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptP", @"   var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GE url, true);   xmlHttpRequest.onreadystatecion(){      document.getElementById('lblTeerHTML=xmlHttpRe.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GE url, true);   xmlHttpRequest.onreadystatecion(){      document.getElementById('lblTeerHTML=xmlHttpRe.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
        //    actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

        //    StringAssert.Contains(actual, expected);
        //}

        #endregion

        #region Password Validation Tests
        [TestMethod]
        public void Password_UpperCaseLowerCaseAndNumbers_Expected_ClassInjectedWithUpperCaseLowerCaseAndNumberValidation()
        {

            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationP", "upperCaseLowerCaseAndNumbersValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""upperCaseLowerCaseAndNumbersValidation testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""1"" class=""upperCaseLowerCaseAndNumbersValidation testPasswordCustomStyle"" style=""height:4px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void Password_UpperCaseAndLowerCase_Expected_ClassInjectedWithUpperCaseAndLowerCaseValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationP", "upperCaseAndLowerCaseValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""upperCaseAndLowerCaseValidation testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""1"" class=""upperCaseAndLowerCaseValidation testPasswordCustomStyle"" style=""height:4px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void Password_LettersAndNumbersValidation_Expected_ClassInjectedWithLetterAndNumberValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationP", "lettersAndNumbersValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""lettersAndNumbersValidation testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""1"" class=""lettersAndNumbersValidation testPasswordCustomStyle"" style=""height:4px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }



        [TestMethod]
        public void WholeNumbersOnlyValidation_Expected_ClassInjectedWithWholeNumberValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationP", "numbersOnlyValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""numbersOnlyValidation testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""1"" class=""numbersOnlyValidation testPasswordCustomStyle"" style=""height:4px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void LettersOnlyValidatio_Expected_ClassInjectedWithLetterOnlyValidationn()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationP", "charactersOnlyValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""charactersOnlyValidation testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""1"" class=""charactersOnlyValidation testPasswordCustomStyle"" style=""height:4px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void MinMaxCharactersPasswordValidationTest_Expected_InputInjectedWithMinMaxCharactervalues()
        {

            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2minCharactersTextbox", "8");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2maxCharactersTextbox", "10");
            string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
            string expected = @"<span class=""testPasswordCustomStyle""><input type=""password"" name=""testPasswordElementName"" id=""testPasswordElementName"" title=""testPasswordToolTip"" tabindex=""1"" class=""testPasswordCustomStyle"" style=""height:4px;width:4px;""/> </span><input type=""hidden"" id=""Dev2MinChars"" value=""8"" /><input type=""hidden"" id=""Dev2MaxChars"" value=""10"" /><input type=""hidden"" id=""testPasswordElementNameErrMsg"" value=""testPasswordValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region Miscallaneous



        #endregion Miscallaneous
    }
}
