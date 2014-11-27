
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
    /// Summary description for RadioButtonTesthj
    /// </summary>
    [TestClass]
    public class RadioButtonTest {

        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();

        #region Pre And Post Test Activities

        public RadioButtonTest() {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        string WebserverURI = ServerSettings.WebserverURI;
        private const string RadioButton = "Radio Button";
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

            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.RadioButton_DataList, TestResource.Xpath_ElementName, "testRadioButtonElementName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ErrorMsg, "testRadioButtonErrorMessage");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "1");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testRadioButtonToolTip");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditRB", "true");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "requiredRB", "true");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customStyleradioButton", "testRadioButtonCustomStyle");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Fragment, "testRadioButtonFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CssClass, "testRadioButtonCssClass");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptRB", "testRadioButtonCustomScript");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "alignmentRB", "testRadioButtonAlignment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "InjectStar", "testRadioButtonInjectStar");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2FromServiceRB", "no");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "DevCustomDataServiceRB", "testRadioButtonCustomDataService");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataRowDelimiterRB", "testRadioButtonCustomDataRowDelimiter");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataRowDisplayRB", "testRadioButtonCustomDataRowDisplay");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataOptionFieldRB", "testRadioButtonCustomDataOptionField");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2RadioButtonStaticOptions", "<itemCollection><item>test1</item><item>test2</item></itemCollection>");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "BindingData", "testRadioButtonBindingData");
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Expected User Interaction Cases

        [TestMethod]
        public void RadioButtonTest_ExpectedUserInteraction_VerticalAlignment_Expected_RadioButtonsCreatedWithLineBreaks() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "alignmentRB", "v");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
            string expected = @"<span class=""requiredClass testRadioButtonCustomStyle""><span><input type=""radio"" value=""test1"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled"" />test1 <br/></span><span><input type=""radio"" value=""test2"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled"" />test2 <br/></span></span>testRadioButtonCustomScript";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        public void RadioButtton_AllowUserEditingTrue_Expected_NoReadOnlyAttributeSetonWebpart() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditRB", "yes");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
            string expected = @"<span class=""requiredClass testRadioButtonCustomStyle""><span><input type=""radio"" value=""test1"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" />test1 </span><span><input type=""radio"" value=""test2"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" />test2 </span></span>testRadioButtonCustomScript";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "").Replace("\"  />", "\" />");

            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region Advanced Options - Tooltip Text

        [TestMethod()]
        public void RadioButton_AdvancedSettings_TooltipText_Expected_CustomTooltipOnWebpart() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
            string expected = @"<span class=""requiredClass testRadioButtonCustomStyle""><span><input type=""radio"" value=""test1"" name=""testRadioButtonElementName"" tabindex=""1"" title=""text"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled"" />test1 </span><span><input type=""radio"" value=""test2"" name=""testRadioButtonElementName"" tabindex=""1"" title=""text"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled"" />test2 </span></span>testRadioButtonCustomScript";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RadioButton_AdvancedSettings_TooltipTextWithInteger_Expected_TooltipWithIntegerOnWebpart() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "12");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
            string expected = @"<span class=""requiredClass testRadioButtonCustomStyle""><span><input type=""radio"" value=""test1"" name=""testRadioButtonElementName"" tabindex=""1"" title=""12"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled"" />test1 </span><span><input type=""radio"" value=""test2"" name=""testRadioButtonElementName"" tabindex=""1"" title=""12"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled"" />test2 </span></span>testRadioButtonCustomScript";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }


        #endregion

        #region Advanced Options - Tab Index

        [TestMethod()]
        public void RadioButton_AdvancedOptions_TabIndex_Expected_CustomTabIndexOnWebpart() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "2");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
            string expected = @"<span class=""requiredClass testRadioButtonCustomStyle""><span><input type=""radio"" value=""test1"" name=""testRadioButtonElementName"" tabindex=""2"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled"" />test1 </span><span><input type=""radio"" value=""test2"" name=""testRadioButtonElementName"" tabindex=""2"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled"" />test2 </span></span>testRadioButtonCustomScript";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RadioButton_AdvancedOptions_TabIndexText_Expected_CustomTabIndexWithTextValueOnWebpart() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
            string expected = @"<span class=""requiredClass testRadioButtonCustomStyle""><span><input type=""radio"" value=""test1"" name=""testRadioButtonElementName"" tabindex=""text"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled"" />test1 </span><span><input type=""radio"" value=""test2"" name=""testRadioButtonElementName"" tabindex=""text"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled"" />test2 </span></span>testRadioButtonCustomScript";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Altering Width and Height

        [TestMethod()]
        public void RadioButton_AdvancedOptions_AlteringWidth_Expected_WidthSpecifiedInWidthBlock() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "20");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
            string expected = @"<span class=""requiredClass testRadioButtonCustomStyle""><span><input type=""radio"" value=""test1"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:20px;"" readonly=""true"" disabled=""disabled"" />test1 </span><span><input type=""radio"" value=""test2"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:20px;"" readonly=""true"" disabled=""disabled"" />test2 </span></span>testRadioButtonCustomScript";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RadioButton_AdvancedOptions_AlteringWidthText_Expected_WidthAttributeSetOnWebpartToText() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
            string expected = @"<span class=""requiredClass testRadioButtonCustomStyle""><span><input type=""radio"" value=""test1"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:text;"" readonly=""true"" disabled=""disabled"" />test1 </span><span><input type=""radio"" value=""test2"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:text;"" readonly=""true"" disabled=""disabled"" />test2 </span></span>testRadioButtonCustomScript";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RadioButton_AdvancedOptions_AlteringHeight_Expected_WidthSpecifiedInHeightBlock() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "20");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
            string expected = @"<span class=""requiredClass testRadioButtonCustomStyle""><span><input type=""radio"" value=""test1"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:20px;width:4px;"" readonly=""true"" disabled=""disabled"" />test1 </span><span><input type=""radio"" value=""test2"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:20px;width:4px;"" readonly=""true"" disabled=""disabled"" />test2 </span></span>testRadioButtonCustomScript";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RadioButton_AdvancedOptions_AlteringHeightText_Expected_HeightAttributeSetOnWebpartToText() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
            string expected = @"<span class=""requiredClass testRadioButtonCustomStyle""><span><input type=""radio"" value=""test1"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:text;width:4px;"" readonly=""true"" disabled=""disabled"" />test1 </span><span><input type=""radio"" value=""test2"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:text;width:4px;"" readonly=""true"" disabled=""disabled"" />test2 </span></span>testRadioButtonCustomScript";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Custom styling

        [TestMethod()]
        public void RadioButton_AdvancedOptions_CustomStyling_Expected_ClassAttributeSetToSpecifiedStyle() {
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
            string expected = @"<span class=""requiredClass testRadioButtonCustomStyle""><span><input type=""radio"" value=""test1"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled"" />test1 </span><span><input type=""radio"" value=""test2"" name=""testRadioButtonElementName"" tabindex=""1"" title=""testRadioButtonToolTip"" class=""requiredClass testRadioButtonCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled"" />test2 </span></span>testRadioButtonCustomScript";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
            actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Options - Custom Scripting

        // PBI 5376 : Script Tags not generated for custom script
        // Test currently broken: BugId 6600
        //[TestMethod()]
        //public void RadioButton_AdvancedOptions_SimpleCustomScript_Expected_SimpleJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptRB", "alert('i');");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
        //    string expected = @"<script>alert('i');</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
        //    actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376: Same As above

        //[TestMethod()]
        //public void TextArea_AdvancedOptions_CustomScript_complexScript_Expected_ComplexJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptRB", @"var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
        //    actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376: Same As above

        //[TestMethod()]
        //public void TextArea_AdvancedOptions_CustomScript_MalformedScript_Expected_MalformedJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptRB", @"   var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open(', url, true);   xmlHttpRequest.onreadystatechange = function(      doculementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, RadioButton, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open(', url, true);   xmlHttpRequest.onreadystatechange = function(      doculementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    expected = TestHelper.RemoveWhiteSpaceBetweenTags(expected);
        //    actual = TestHelper.RemoveWhiteSpaceBetweenTags(actual).Replace("\r", "").Replace("\n", "");

        //    StringAssert.Contains(actual, expected);
        //}

        #endregion

    }
}
