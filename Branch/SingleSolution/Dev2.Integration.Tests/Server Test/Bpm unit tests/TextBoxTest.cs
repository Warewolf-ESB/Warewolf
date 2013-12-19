using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.MEF;
using Dev2.Integration.Tests.Enums;
using Dev2.Integration.Tests.Helpers;
using System.Text.RegularExpressions;

namespace Dev2.Integration.Tests.BPM.Unit.Test
{


    #region Pre and Post Test Activities
    /// <summary>
    /// Summary description for TextBoxTest
    /// </summary>
    [TestClass]
    public class TextBoxTest
    {
        DataListValueInjector dataListValueInjector = new DataListValueInjector();
        string tempXmlString = string.Empty;
        public TextBoxTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        private string WebserverURI = ServerSettings.WebserverURI;
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

        //// Initialize requirements before test start

        //        [ClassInitialize()]
        //        public static void MyClassInitialize(TestContext testContext)
        //        {
        //            string processName = "Unlimited.Applications.DynamicServicesHost.exe";
        //            FileIO myfile = new FileIO();
        //            string file = myfile.ReturnPathOfLatest(processName);
        //            string mydirectory = myfile.ReturnLatestDirectory();
        //            myfile.CopyRequiredFiles();
        //            System.Threading.Thread.Sleep(1000);
        //            _process = new ProcessInvoker(file, processName);
        //            _process.InvokeProcess();
        //            System.Threading.Thread.Sleep(5000);
        //        }

        //// Kill the Webserver for the next test run
        //        [ClassCleanup()]
        //        public static void MyClassCleanup() {
        //            _process.KillWebserver();
        //            System.Threading.Thread.Sleep(1000);
        //        }

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {

            tempXmlString = string.Empty;

            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.TextBox_DataList, TestResource.Xpath_ElementName, "testTextBoxElementName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ErrorMsg, "testTextBoxErrorMessage");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_DisplayText, "testTextBoxDisplayText");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Fragment, "testTextBoxFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CssClass, "testTextBoxCssClass");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "1");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testTextAreaToolTip");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "requiredTB", "true");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customStyleTextbox", "testTextBoxCustomStyle");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptTB", "testTextBoxCustomScript");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2validationErrMsgTB", "testTextBoxValidationErrMsg");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTB", "testTextBoxValidation");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditTB", "true");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2maxCharsTB", "30");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Inject", "testTextBoxInject");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "autoTB", "testTextBoxAuto");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2DisplayNameTB", "testTextBoxDisplayName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2RelFieldNameTB", "testTextBoxRelFieldName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2RowDelimTB", "testTextBoxRowDelim");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2autoComplete", "testTextBoxAutoComplete");
        }

        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Expected User Interaction

        [TestMethod()]
        public void TextBox_ExpectedUserInput_Expected_CorrectlyCreatedWebpart()
        {
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle testTextBoxValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTextAreaToolTip"" tabindex=""1"" class=""requiredClass testTextBoxCustomStyle testTextBoxValidation"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextBox_InputXMLAsDisplayText_Expected_NoDisplayText()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "showTextTB", "on");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_DisplayText, @"<test>testing</test>");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle testTextBoxValidation""><test>testing</test> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTextAreaToolTip"" tabindex=""1"" class=""requiredClass testTextBoxCustomStyle testTextBoxValidation"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void TextBox_TextBoxWithoutName_Expected_WebpartRenderErrorReturned()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ElementName, "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""internalError"">Webpart Render Error : Name not provided</span>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void TextBox_TextBoxWithoutTextToDisplay_Expected_NoLabelSet()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_DisplayText, "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle testTextBoxValidation"">
<input type=""text"" name=""testTextBoxElementName""";



            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual);

            StringAssert.Contains(actual, expected);

        }

        [TestMethod()]
        public void TextBox_AllowUserEditingFalse_Expected_ReadonlyAttributeSetToTrue()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditTB", "false");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"readonly=""true"" disabled=""disabled""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);


            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Advanced Options - Custom Script
        // PBI 5376 : Scripts are not wrapped in script tags
        // Test currently broken: BugId 6600

        //[TestMethod()]
        //public void TextBox_AdvancedOptions_SimpleCustomScript_Expected_SimpleJavascriptInjected()
        //{
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptTB", "alert('Chasdigal wants a beer frig!!!!');");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
        //    string expected = @"<script>alert('Chasdigal wants a beer frig!!!!');</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    actual = actual.Replace("\r", "").Replace("\n", "");

        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376 : Scripts are not wrapped in script tags
        //[TestMethod()]
        //public void TextBox_AdvancedOptions_CustomScript_complexScript_Expected_ComplexJavascriptInjected()
        //{
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptTB", @"   var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    actual = actual.Replace("\r", "").Replace("\n", "");

        //    StringAssert.Contains(actual, expected);
        //}

        //[TestMethod()]
        //public void TextBox_AdvancedOptions_CustomScript_MalformedScript_Expected_MalformedJavascriptInjected()
        //{
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptTB", @"   var xmlHttpRequest = new XMLHttpRequest();   var url = /localhost:2234/services/sashen_workflow';   xmlET', url, true);   xmlHt function(){      document.getElementById('lblTinnerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
        //    string expected = @"<script>   var xmlHttpRequest = new XMLHttpRequest();   var url = /localhost:2234/services/sashen_workflow';   xmlET', url, true);   xmlHt function(){      document.getElementById('lblTinnerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    actual = actual.Replace("\r", "").Replace("\n", "");

        //    StringAssert.Contains(actual, expected);
        //}

        #endregion

        #region Advanced options - Validation

        [TestMethod()]
        public void TextBox_AdvancedOptions_EmailValidation_Expected_ClassInjectedWithEmailValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTB", "emailValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle emailValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTextAreaToolTip"" tabindex=""1"" class=""requiredClass testTextBoxCustomStyle emailValidation"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextBox_AdvancedOptions_WholeNumberValidation_Expected_ClassInjectedWithWholeNumberValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTB", "wholeValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle wholeValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTextAreaToolTip"" tabindex=""1"" class=""requiredClass testTextBoxCustomStyle wholeValidation"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextBox_AdvancedOptions_DecimalNumberValidation_Expected_ClassInjectedWithDecimalValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTB", "decimalValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle decimalValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTextAreaToolTip"" tabindex=""1"" class=""requiredClass testTextBoxCustomStyle decimalValidation"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextBox_AdvancedOptions_LettersOnlyValidation_Expected_ClassInjectedWithLettersValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTB", "lettersValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle lettersValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTextAreaToolTip"" tabindex=""1"" class=""requiredClass testTextBoxCustomStyle lettersValidation"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        public void TextBox_AdvancedOptions_LettersAndNumbersValidation_Expected_ClassInjectedWithLetterAndNumberValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTB", "letterNumbersValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle letterNumbersValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTextAreaToolTip"" tabindex=""1"" class=""requiredClass testTextBoxCustomStyle letterNumbersValidation"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region Advanced Options - Tab Index

        [TestMethod()]
        public void TextBox_AdvancedOptions_TabIndex_Expected_CustomTabIndexOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "2");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle testTextBoxValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTextAreaToolTip"" tabindex=""2"" class=""requiredClass testTextBoxCustomStyle testTextBoxValidation"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextBox_AdvancedOptions_TabIndexText_Expected_CustomTabIndexWithTextOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle testTextBoxValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTextAreaToolTip"" tabindex=""text"" class=""requiredClass testTextBoxCustomStyle testTextBoxValidation"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Tooltip Text

        [TestMethod()]
        public void TextBox_AdvancedOptions_TooltipText_Expected_CustomTooltipOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testTooltip");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle testTextBoxValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTooltip"" tabindex=""1"" class=""requiredClass testTextBoxCustomStyle testTextBoxValidation"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextBox_AdvancedOptions_TooltipTextInteger_Expected_TooltipWithIntegerOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "12");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle testTextBoxValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""12"" tabindex=""1"" class=""requiredClass testTextBoxCustomStyle testTextBoxValidation"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Custom Styling

        [TestMethod()]
        public void TextBox_AdvancedOptions_CustomStyling_Expected_ClassAttributeSetToSpecifiedStyle()
        {
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle testTextBoxValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTextAreaToolTip"" tabindex=""1"" class=""requiredClass testTextBoxCustomStyle testTextBoxValidation"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Width And Height

        [TestMethod()]
        public void TextBox_AdvancedOptions_WidthAndHeight_Expected_WidthAndHeightSpecifiedInWidthAndHeightBlocks()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "20");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "20");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle testTextBoxValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTextAreaToolTip"" tabindex=""1"" class=""requiredClass testTextBoxCustomStyle testTextBoxValidation"" style=""height:20px;width:20px;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextBox_AdvancedOptions_WidthAndHeightText_Expected_WidthAndHeightAttributeSetOnWebpartToText()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "text");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
            string expected = @"<span class=""requiredClass testTextBoxCustomStyle testTextBoxValidation""> <input type=""text"" name=""testTextBoxElementName"" id=""testTextBoxElementName"" value="""" maxlength=""30"" title=""testTextAreaToolTip"" tabindex=""1"" class=""requiredClass testTextBoxCustomStyle testTextBoxValidation"" style=""height:text;width:text;"" readonly=""true"" disabled=""disabled""/> </span>testTextBoxCustomScripttestTextBoxInject<input type=""hidden"" id=""testTextBoxElementNameErrMsg"" value=""testTextBoxValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Miscallaneous



        #endregion Miscallaneous
    }
}
