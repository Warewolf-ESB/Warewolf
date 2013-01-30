using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.MEF;
using Dev2.Integration.Tests.Enums;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.BPM.Unit.Test
{
    /// <summary>
    /// Summary description for TextArea
    /// </summary>
    [TestClass]
    public class TextAreaTest
    {
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();

        #region Pre and Post Test Activities

        public TextAreaTest()
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

        #endregion

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

            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.TextArea_DataList, TestResource.Xpath_ElementName, "testTextAreaElementName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ErrorMsg, "testTextAreaErrorMessage");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "InjectedLabel", "testTextAreaInjectLabel");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_DisplayText, "testTextAreaDisplayText");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "showTextTA", "true");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Fragment, "testTextAreaFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CssClass, "testTextAreaCssClass");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "InjectStar", "testTextAreaInjectStar");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "1");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testTextAreaToolTip");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "requiredTA", "yes");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customStyleTextarea", "testTextAreaCustomStyle");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptTA", "testTextAreaCustomScript");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditTA", "no");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2validationErrMsgTA", "testTextAreaValidationErrMsg");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTA", "testTextAreaValidation");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "DataValue", "testTextAreaDataValue");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2AllowResizeTA", "no");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2InjectedData", "testTextAreaInjectedData");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "menuID", "testTextAreaMenuID");
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Expected User Interaction

        [TestMethod()]
        public void TextArea_ExpectedUserInteraction_NoNameSpecified_Expected_SpanWithWebpartErrorRendered()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ElementName, "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""internalError"">Webpart Render Error : Name not provided</span>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = TestHelper.CleanUp(actual);
            expected = TestHelper.CleanUp(expected);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_ExpectedUserInteraction_Validation_None_Expected_requiredClassCreatedWithoutValidationType()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTA", "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""    class=""requiredClass testTextAreaCustomStyle"" style=""height:4px;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_ExpectedUserInteraction_Validation_Email_Expected_RequiredClassCreatedWithEmailValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTA", "emailValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle emailValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""    class=""requiredClass testTextAreaCustomStyle emailValidation"" style=""height:4px;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_ExpectedUserInteraction_Validation_LettersOnly_Expected_RequiredClassCreatedWithLettersValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTA", "lettersValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle lettersValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""    class=""requiredClass testTextAreaCustomStyle lettersValidation"" style=""height:4px;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_ExpectedUserInteraction_Validation_WholeNumbers_Expected_RequiredClassCreatedWithWholeValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTA", "wholeValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle wholeValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""    class=""requiredClass testTextAreaCustomStyle wholeValidation"" style=""height:4px;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_ExpectedUserInteraction_Validation_DecimalNumbers_Expected_RequiredClassCreatedWithDecimalValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTA", "decimalValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle decimalValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""    class=""requiredClass testTextAreaCustomStyle decimalValidation"" style=""height:4px;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_ExpectedUserInteraction_Validation_LettersAndNumbers_Expected_RequiredClassCreatedWithLettersAndNumbersValidation()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "validationTA", "letterNumbersValidation");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle letterNumbersValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""    class=""requiredClass testTextAreaCustomStyle letterNumbersValidation"" style=""height:4px;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_AllowUserEditingTrue_Expected_ReadonlyAttributeNotRendered()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditTA", "yes");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle testTextAreaValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""    class=""requiredClass testTextAreaCustomStyle testTextAreaValidation"" style=""height:4px;width:4px;"" wrap='off' > </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_AllowResize_Test()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2AllowResizeTA", "yes");

            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);

            string expected1 = @" var baseW = elm.width();    var wrapOn = """";    elm.addClass(""noscroll"");    hiddenElm.addClass(""hiddendiv"");    /* only set width if in wrap mode */";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\n", "").Replace("\r", "");


            Assert.IsTrue(actual.Contains(expected1));


        }

        #endregion

        #region Advanced Options - Altering Width and Height

        [TestMethod()]
        public void TextArea_AdvancedOptions_AlteringWidth_Expected_WidthSpecifiedInWidthBlock()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "20");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle testTextAreaValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""    class=""requiredClass testTextAreaCustomStyle testTextAreaValidation"" style=""height:4px;width:20px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_AdvancedOptions_AlteringWidthText_Expected_WidthAttributeSetOnWebpartToText()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle testTextAreaValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""    class=""requiredClass testTextAreaCustomStyle testTextAreaValidation"" style=""height:4px;width:text;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_AdvancedOptions_AlteringHeight_Expected_WidthSpecifiedInHeightBlock()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "20");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle testTextAreaValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""    class=""requiredClass testTextAreaCustomStyle testTextAreaValidation"" style=""height:20px;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_AdvancedOptions_AlteringHeightText_Expected_WidthAttributeSetOnWebpartToText()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle testTextAreaValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""    class=""requiredClass testTextAreaCustomStyle testTextAreaValidation"" style=""height:text;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Tooltip Text

        [TestMethod()]
        public void TextArea_AdvancedSettings_TooltipText_Expected_CustomTooltipOnWebpart()
        {
            // Dev2toolTipTextarea
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2toolTipTextarea", "test tooltip");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle testTextAreaValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""  title=""test tooltip""  class=""requiredClass testTextAreaCustomStyle testTextAreaValidation"" style=""height:4px;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_AdvancedSettings_TooltipTextInteger_Expected_TooltipWithIntegerOnWebpart()
        {
            // Dev2toolTipTextarea
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2toolTipTextarea", "12");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle testTextAreaValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""  title=""12""  class=""requiredClass testTextAreaCustomStyle testTextAreaValidation"" style=""height:4px;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced options - Tab Index

        [TestMethod()]
        public void TextArea_AdvancedOptions_TabIndex_Expected_CustomTabIndexOnWebpart()
        {
            // Dev2tabIndexTextarea
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2tabIndexTextarea", "1");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle testTextAreaValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""   tabindex=""1"" class=""requiredClass testTextAreaCustomStyle testTextAreaValidation"" style=""height:4px;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TextArea_AdvancedOptions_TabIndexText_Expected_CustomTabIndexWithTextOnWebpart()
        {
            // Dev2tabIndexTextarea
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2tabIndexTextarea", "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle testTextAreaValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""   tabindex=""text"" class=""requiredClass testTextAreaCustomStyle testTextAreaValidation"" style=""height:4px;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Custom styling

        [TestMethod()]
        public void TextArea_AdvancedOptions_CustomStyling_Expected_ClassAttributeSetToSpecifiedStyle()
        {
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
            string expected = @"<span class=""requiredClass testTextAreaCustomStyle testTextAreaValidation""> <textarea rows=""10"" cols=""25"" name=""testTextAreaElementName"" id=""testTextAreaElementName""    class=""requiredClass testTextAreaCustomStyle testTextAreaValidation"" style=""height:4px;width:4px;"" wrap='off' readonly=""true"" disabled=""disabled""> </textarea> </span>testTextAreaCustomScript<input type=""hidden"" id=""testTextAreaElementNameErrMsg"" value=""testTextAreaValidationErrMsg"" />";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            actual = actual.Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Custom Scripting

        // PBI 5376 : Scripts are not wrapped in script tags
        // Test currently broken: BugId 6600
        //[TestMethod()]
        //public void TextArea_AdvancedOptions_SimpleCustomScript_Expected_SimpleJavascriptInjected()
        //{
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptTA", "alert('i');");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
        //    string expected = @"<script>alert('i');</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    actual = actual.Replace("\r", "").Replace("\n", "");

        //    StringAssert.Contains(actual, expected);
        //}

        //[TestMethod()]
        //public void TextArea_AdvancedOptions_CustomScript_complexScript_Expected_ComplexJavascriptInjected()
        //{
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptTA", @"   var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script?";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    actual = actual.Replace("\r", "").Replace("\n", "");

        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376 : Scripts are not wrapped in script tags
        //[TestMethod()]
        //public void TextArea_AdvancedOptions_CustomScript_MalformedScript_Expected_ComplexJavascriptInjected()
        //{
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptTA", @"   var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open(', url, true);   xmlHttpRequest.onreadystatechange = function(){ document.getElementById('lblTest'nnerHTML=xmlHttpRequesponseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open(', url, true);   xmlHttpRequest.onreadystatechange = functio      document.getElementById('lblTest'nnerHTML=xmlHttpRequesponseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    actual = actual.Replace("\r", "").Replace("\n", "");

        //    StringAssert.Contains(actual, expected);
        //}

        #endregion

        #region Miscallaneous



        #endregion Miscallaneous
    }
}
