
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

namespace Dev2.Integration.Tests.BPM.Unit.Test
{

    /// <summary>
    /// Unit Tests related to the Checkbox webpart.
    /// These test will serve as a POST to the checkbox webpart service and checking the HTML Fragment returned in the webserver XML data.
    /// </summary>
    [TestClass]
    public class CheckBoxTest
    {
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();

        #region Pre and Post Test Activities

        public CheckBoxTest()
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

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {

            tempXmlString = string.Empty;
            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.CheckBox_DataList, "Dev2elementNameCheckbox", "testCheckBoxElementName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ErrorMsg, "testCheckBoxErrorMessage");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "1");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testCheckBoxToolTip");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CustomStyle, "testCheckBoxCustomStyle");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "4");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2fromServiceCB", "no");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "DevCustomDataServiceCB", "testCheckBoxCustomDataService");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataRowDelimiterCB", "testCheckBoxCustomDataRowDelimiter");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataRowDisplayCB", "testCheckBoxCustomDataRowDisplay");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataOptionFieldCB", "testCheckBoxCustomDataOptionField");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Fragment, "testCheckBoxFragment");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "CheckBoxOptions", "testCheckBoxCheckBoxOptions");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CheckboxStaticOptions", "<itemCollection><item>test1</item><item>test2</item></itemCollection>");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptCheckbox", "<script>testCheckBoxCustomScript</script>");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "BindingData", "testCheckBoxBindingData");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditCB", "no");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "alignmentCB", "v");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "requiredCB", "no"); 
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Expected User Interaction Cases

        [TestMethod]
        public void Checkbox_ExpectedUserInteraction_VerticalAlignment_Expected_LineBreaksBetweenCheckboxes()
        {
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);

            string expected = @"<br/>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            StringAssert.Contains(actual, expected, string.Format("Actual: {0}\r\n does not contain vertical alignment requirements", actual));
        }

        [TestMethod()]
        public void Checkbox_ExpectedUserInteraction_HorizontalAlignment_ExpectedNoLineBreaksBetweenCheckboxes()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "alignmentCB", "h");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
            //string expected = @"<![CDATA[<div><span class=""testCheckBoxCustomStyle""><input type=""checkbox"" value=""test1"" name=""testCheckBoxElementName"" id=""testCheckBoxElementName"" tabindex=""1"" title=""testCheckBoxToolTip"" readonly=""true"" disabled=""disabled"" />test1 </span><span class=""testCheckBoxCustomStyle""><input type=""checkbox"" value=""test2"" name=""testCheckBoxElementName"" id=""testCheckBoxElementName"" tabindex=""1"" title=""testCheckBoxToolTip"" readonly=""true"" disabled=""disabled"" />test2 </span><script>testCheckBoxCustomScript</script></div>]]>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.AreEqual(1, actuals.Where(c => c.Contains("<input type=\"checkbox\" value=\"test1\"")).ToList().Count);

        }
   
        [TestMethod()]
        public void Checkbox_ExpectedUserInteraction_NoNameSpecified_Expected_WebpartErrorDisplayed()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ElementName, "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
            string expected = @"<![CDATA[<span class=""internalError"">Webpart Render Error : No Name Set</span>]]>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.AreEqual(1, actuals.Where(c => c.Contains("<span class=\"internalError\">")).ToList().Count);
        }


        [TestMethod()]
        public void Checkbox_ExpectedUserInteraction_AllowEditing_Expected_NoReadOnlyAttributeOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditCB", "yes");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
            string notExpected = @"readonly=""true""";
            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            Assert.IsFalse(actual.Contains(notExpected), string.Format("Actual: {0}\r\n has attribute{1}", actual, notExpected));
        }


        [TestMethod()]
        public void Checkbox_ExpectedUserInteraction_NoOptions_NoWebpartGenerated()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CheckboxStaticOptions", "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
            string expected = @"<input";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            Assert.IsFalse(actual.Contains(expected), string.Format("Actual: {0}\r\nrendered inputs when not given inputs to render", actual));
        }


        #endregion

        #region Advanced Options - Custom styling

        [TestMethod()]
        public void Checkbox_AdvancedOptions_CustomStyling_Expected_ClassAttributeSetToSpecifiedStyle()
        {
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
            string expected = @"class=""testCheckBoxCustomStyle""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            StringAssert.Contains(actual, expected);

        }

        #endregion

        #region Advanced Options - Altering Width and Height

        [TestMethod()]
        public void Checkbox_AdvancedOptions_AlteringWidth_Expected_HeightSpecifiedInHeightBlock()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "20");

            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
            string expected = @"style=""width:20px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void Checkbox_AdvancedOptions_AlteringHeight_Test_Expected_WidthSpecifiedInBlock()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "20");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
            string expected = @"style=""height:20px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Advanced Options - Adding CheckBox

        [TestMethod()]
        public void Checkbox_AdvancedOptions_AddingCheckBoxWithNoValue_Expected_CheckboxGeneratedWithoutAValueForNameOrValue()
        {

            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CheckboxStaticOptions", "<itemCollection><item>test1</item><item></item><item>test3</item></itemCollection>");

            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
            string expected = @"<![CDATA[<div><span class=""testCheckBoxCustomStyle""><input type=""checkbox"" value=""test1"" name=""testCheckBoxElementName"" id=""testCheckBoxElementName"" tabindex=""1"" title=""testCheckBoxToolTip"" readonly=""true"" disabled=""disabled"" />test1 <br /></span><span class=""testCheckBoxCustomStyle""><input type=""checkbox"" value="""" name=""testCheckBoxElementName"" id=""testCheckBoxElementName"" tabindex=""1"" title=""testCheckBoxToolTip"" readonly=""true"" disabled=""disabled"" checked=""yes"" /><br /></span><span class=""testCheckBoxCustomStyle""><input type=""checkbox"" value=""test3"" name=""testCheckBoxElementName"" id=""testCheckBoxElementName"" tabindex=""1"" title=""testCheckBoxToolTip"" readonly=""true"" disabled=""disabled"" />test3 <br /></span><script>testCheckBoxCustomScript</script></div>]]>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.IsTrue(actuals.Where(c => c.Contains("input")).ToList().Count == 3);
        }

        #endregion

        #region Advanced Options - Tab Index

        [TestMethod()]
        public void Checkbox_AdvancedOptions_TabIndex_Expected_CustomTabIndexOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "2");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
            string expected = @"tabindex=""2""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.IsTrue(actuals.Where(c => c.Contains(expected)).ToList().Count == 2, string.Format("Actual: {0} actual \r\n does not contain 2 entries for tabindex=2", actual));
        }

        [TestMethod()]
        public void Checkbox_AdvancedOptions_TabIndexAsString_Expected_TextasTabIndex()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
            string expected = @"tabindex=""text""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);
            Assert.IsTrue(actuals.Where(c => c.Contains(expected)).ToList().Count == 2, string.Format("Actual: {0} actual \r\n does not contain 2 entries for {1}", actual, expected));
        }

        #endregion

        #region Advanced Options - Tooltip Text

        [TestMethod()]
        public void Checkbox_AdvancedSettings_TooltipText_Expected_TooltipAttributeWithTestingTextGeneratedWithWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testingTooltip");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);

            Assert.AreEqual(2, actuals.Where(c => c.Contains(@"title=""testingTooltip""")).ToList().Count);
        }

        #endregion

        #region Advanced Options - Custom Scripting

        // PBI 5376 : Not wrapping in script tags
        // Test currently broken: BugId 6600
        //[TestMethod()]
        //public void Checkbox_AdvancedOptions_SimpleCustomScript_Expected_CustomSimpleScriptInjectedWithWebpart() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptCheckbox", "<script>a;</script>");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
        //    string expected = @"<div><span class=""testCheckBoxCustomStyle""><input type=""checkbox"" value=""test1"" name=""testCheckBoxElementName"" id=""testCheckBoxElementName"" tabindex=""1"" title=""testCheckBoxToolTip"" readonly=""true"" disabled=""disabled"" />test1 <br /></span><span class=""testCheckBoxCustomStyle""><input type=""checkbox"" value=""test2"" name=""testCheckBoxElementName"" id=""testCheckBoxElementName"" tabindex=""1"" title=""testCheckBoxToolTip"" readonly=""true"" disabled=""disabled"" />test2 <br /></span><script>alert('hi');</script></div>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    List<string> actuals = TestHelper.BreakHTMLElement(expected);
        //    Assert.AreEqual(1, actuals.Where(c => c.Contains("<script>alert('hi');</script>")).ToList().Count);
        //}

        //// PBI 5376: Same Issue as above

        //[TestMethod()]
        //public void CheckBox_AdvancedOptions_ComplexCustomScript_Expected_CustomComplexScriptInjectedWithWebpart() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptCheckbox", @"var xmlHttpRequest = new XMLHttpRequest(); var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById      ('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById      ('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);<script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    actual = actual.Replace("\n", "").Replace("\r", "");

        //    StringAssert.Contains(actual, expected);
        //}


        //// PBI 5376: Same issue as above

        //[TestMethod()]
        //public void Checkbox_AdvancedOptions_MalformedCustomScript_Expected_MalformedScriptInjectedWithWebpart() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptCheckbox", @"var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';  xmlHttpRequest.open('GET', url, true);   xmlHttponreadystatechange = func{      document.getElementById('lblTe.innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttponreadystatechange = func{      document.getElementById('lblTe.innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    actual = actual.Replace("\n", "").Replace("\r", "");

        //    Assert.AreEqual(expected, actual);
        //}

        #endregion

        #region Advanced Options - Service Binding


        // PBI 6278: Issue caused in ForEach Tool
        /*
        // PBI 5376: Issue caused in ForEach Tool
        [TestMethod()]
        public void Checkbox_AdvancedOptions_ServiceBinding_Test() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2fromServiceCB", "yes");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "DevCustomDataServiceCB", "ServiceToBindFrom");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataRowDelimiterCB", "regions");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataRowDisplayCB", "name");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataOptionFieldCB", "id");
            // Dev2WebpartType
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2WebpartType", "checkbox");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.CheckBox, tempXmlString);

            string expected = @"region";
            string ReponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ReponseData);
            List<string> actuals = TestHelper.BreakHTMLElement(actual);

            Assert.IsTrue(actuals.Where(c => c.Contains(expected)).ToList().Count == 10, string.Format("Actual: {0} actual \r\n does not contain expected: {0}\r\n", actual, expected));


        }
        */
        #endregion
    }
}
