
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
    /// Summary description for DropDownListTest
    /// </summary>
    [TestClass]
    public class DropDownListTest
    {
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();

        public DropDownListTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        protected string WebserverURI = ServerSettings.WebserverURI;
         string DropDownList = "Drop Down List";
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
            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.DropDownList_DataList, TestResource.Xpath_ElementName, "testDropDownListElementName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ErrorMsg, "testDropDownListErrorMessage");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_DisplayText, "testDropDownListDisplayText");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "1");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testDropDownListToolTip");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CustomStyle, "testDropDownListCustomStyle");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Fragment, "testDropDownListFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditDD", "yes");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "showTextDD", "true");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "requiredDD", "true");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2fromServiceDD", "no");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "DevCustomDataServiceDD", "testDropDownListCustomDataService");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataRowDelimiterDD", "testDropDownListCustomDataRowDelimiter");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataRowDisplayDD", "testDropDownListCustomDataRowDisplay");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataOptionFieldDD", "testDropDownListCustomDataOptionField");
            //tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptDD", "testDropDownListCustomScript");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2DropDownStaticOptions", "<itemCollection><item>test1</item><item>test2</item></itemCollection>");
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Expected User Interaction

        [TestMethod()]
        public void DropDownList_ExpectedUserInteraction_Expected_CorrectlyFormedWebpart() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditDD", "yes");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"<option value=""test1"" >test1</option><option value=""test2"" >test2</option>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected, "Actual:{0}\r\n did not render with {1}", actual, expected);
        }

        [TestMethod()]
        public void DropDownList_ExpectedUserInteraction_DissallowEditing_Expected_ReadonlyAttributeSetToTrue() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditDD", "no");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"readonly=""true"" disabled=""disabled""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected, "Actual:{0}\r\n did not render with {1}", actual, expected);
        }

        [TestMethod()]
        public void DropDownList_ExpectedUserInteraction_ShowingAssociatedLabel_Expected_LabelDisplayed() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "showTextDD", "on");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2displayTextDropDownList", "testDropDownListInjectedLabel");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"testDropDownListInjectedLabel";


            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            StringAssert.Contains(actual, expected, "Actual:{0}\r\n did not render with {1}", actual, expected);
        }

        #endregion

        // PBI 6278 : Service Binding Throwing a ForEach Error

        #region Advanced Options - Creating from service
        /*
        [TestMethod()]
        public void DropDownList_AdvancedOptions_WithallinfoforSevice_Test() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2fromServiceDD", "yes");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "DevCustomDataServiceDD", "ServiceToBindFrom");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataRowDelimiterDD", "regions");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataRowDisplayDD", "name");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataOptionFieldDD", "id");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptDD", "testDropDownListCustomScript");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"<![CDATA[ <select name=""testDropDownListElementName"" id=""testDropDownListElementName"" title=""testDropDownListToolTip"" tabindex=""1"" class=""requiredClass testDropDownListCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""><option value=""0"">region0</option><option value=""1"">region1</option><option value=""2"">region2</option><option value=""3"">region3</option><option value=""4"">region4</option><option value=""5"">region5</option><option value=""6"">region6</option><option value=""7"">region7</option><option value=""8"">region8</option><option value=""9"">region9</option><option value=""10"">region10</option></select>testDropDownListCustomScript]]>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            Assert.AreEqual(expected, actual);
        }
        */
        // PBI 6278 : Service Binding Throwing a ForEach Error
        /*
        [TestMethod()]
        public void DropDownList_AdvancedOptions_WithallinfoforSevice_Two_Invoke() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2fromServiceDD", "yes");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "DevCustomDataServiceDD", "ServiceToBindFrom");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataRowDelimiterDD", "regions");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataRowDisplayDD", "name");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2CustomDataOptionFieldDD", "id");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptDD", "testDropDownListCustomScript");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"<![CDATA[ <select name=""testDropDownListElementName"" id=""testDropDownListElementName"" title=""testDropDownListToolTip"" tabindex=""1"" class=""requiredClass testDropDownListCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""><option value=""0"">region0</option><option value=""1"">region1</option><option value=""2"">region2</option><option value=""3"">region3</option><option value=""4"">region4</option><option value=""5"">region5</option><option value=""6"">region6</option><option value=""7"">region7</option><option value=""8"">region8</option><option value=""9"">region9</option><option value=""10"">region10</option></select>testDropDownListCustomScript]]>";

            string actual = string.Empty;

            //
             // 01.08.2012
             // In the epic battle between activities and Dev2 this test came into existence
             // It runs twice to ensure inner activity does not mutate its IO mapping defs
             // 
             //
            for (int i = 0; i < 2; i++) {
                string ResponseData = TestHelper.PostDataToWebserver(PostData);
                actual = TestHelper.ReturnFragment(ResponseData);  
            }

            Assert.AreEqual(expected, actual);
        }
*/

        #endregion

        #region Advanced Options - Adding ListItem

        [TestMethod()]
        public void DropDownList_AdvancedOptions_AddingListItemWithNoValue_Test_Expected_DropDownItemCreatedWithoutValue() {
            //tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.DropDownList_DataList, TestResource.Xpath_ElementName, "testDropDownListElementName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2DropDownStaticOptions", "<itemCollection><item>testItem1</item><item></item><item>testItem2</item></itemCollection>");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            //string expected = @"<![CDATA[<span class=""requiredClass testDropDownListCustomStyle""> <select name=""testDropDownListElementName"" id=""testDropDownListElementName"" title=""testDropDownListToolTip"" tabindex=""1"" class=""requiredClass testDropDownListCustomStyle"" style=""height:4px;width:4px;"" readonly=""true"" disabled=""disabled""><option value=""test1"">test1</option><option value="""" selected=""true""></option><option value=""test2"">test2</option></select></span>]]>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            Assert.IsTrue(actualElements.Count == 3 && actualElements[1].Contains(@"option value="""" selected=""true"""));
            
        }

        #endregion

        #region Advanced Options - Custom Script

        // PBI 5376 : Script Tags not generated for custom script
        // Test currently broken: BugId 6600

        //[TestMethod()]
        //public void DropDownList_AdvancedOptions_SimpleJavaScript_Expected_SimpleJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptDD", "alert('Chasdigal wants a beer frig!!!!');");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
        //    string expected = @"<script>alert('Chasdigal wants a beer frig!!!!');</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376 : Same as above
        // Test currently broken: BugId 6600

        //[TestMethod()]
        //public void DropDownList_AdvancedOptions_ComplexJavaScript_Expected_ComplexJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptDD", @"var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    actual = actual.Replace("\n", "").Replace("\r", "");
        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376 : Same As Above
        // Test currently broken: BugId 6600

        //[TestMethod()]
        //public void DropDownList_AdvancedOptions_MalformedJavaScript_Expected_MalformedJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptDD", @"var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open(', url, true);   xmlHttpRequest.onreadystatechange = function){      document.getElementById('lblTest').innerHTML=xmlHttpRequesponseText   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
        //    string expected = @"<script>ar xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open(', url, true);   xmlHttpRequest.onreadystatechange = function){      document.getElementById('lblTest').innerHTML=xmlHttpRequesponseText   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    actual = actual.Replace("\n", "").Replace("\r", "");
        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376 : Same as above
        //[TestMethod()]
        //public void DropDownList_AdvancedOptions_NormalTextInCustomScript_ExpectedNormalTextInjectedWithWebpart() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptDD", "testDropDownListCustomScript");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
        //    string expected = @"<script>testDropDownListCustomScript</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    actual = actual.Replace("\n", "").Replace("\r", "");
        //    StringAssert.Contains(actual, expected);
        //}

        #endregion

        #region Advanced Options - Tab Index

        [TestMethod()]
        public void DropDownList_AdvancedOptions_TabIndex_Expected_CustomTabIndexOnWebpart() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "50");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"tabindex=""50""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            StringAssert.Contains(actual, expected);
            Assert.IsFalse(actualElements.Where(c => c.Contains(expected)).ToList().Count > 0);
        }

        #endregion

        #region Advanced Options - Tooltip Text

        [TestMethod()]
        public void DropDownList_AdvancedOptions_TooltipText_Expected_TooltipAttributeWithTestingTextGeneratedWithWebpart() {
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"title=""testDropDownListToolTip""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            StringAssert.Contains(actual, expected);
            Assert.IsFalse(actualElements.Where(c => c.Contains(expected)).ToList().Count > 0);
        }

        [TestMethod()]
        public void DropDownList_AdvancedOptions_TooltipTextWithIntegerValue_Expected_TooltipGeneratedWithIntegerValue() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "12");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"title=""12""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }


        #endregion

        #region Advanced Options - Custom styling

        [TestMethod()]
        public void DropDownList_AdvancedOptions_CustomStyling_Expected_ClassAttributedUpdatedOnWebpart() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CustomStyle, "testingCss");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"class=""requiredClass testingCss""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Advanced Options - Altering Width and Height

        [TestMethod()]
        public void DropDownList_AdvancedOptions_AlteringWidth_Expected_HeightAttributeSetOnWebpartToCustomwidth() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "150");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"style=""height:4px;width:150px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void DropDownList_AdvancedOptions_AlteringWidthTextInput_Expected_WidthAttributeSetOnWebpartToText() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"style=""height:4px;width:text;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void DropDownList_AdvancedOptions_AlteringHeight_Expected_HeightAttributeSetOnWebpartToCustomHeight() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "150");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"style=""height:150px;width:4px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void DropDownList_AdvancedOptions_AlteringHeightWithText_Expected_HeightAttributeSetOnWebpartToText(){
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
            string expected = @"style=""height:text;width:4px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Miscallaneous


        #endregion Miscallaneous

        #region Private Test Methods

        private string ReturnDropDown(string dropDownElementName) {
            string dropDownData = @"<span class=""requiredClass testDropDownListCustomStyle"">
 
<select name=""testDropDownListElementName"" id=""testDropDownListElementName"" title=""testDropDownListToolTip"" tabindex=""1"" class=""requiredClass testDropDownListCustomStyle"" style=""height:4px;width:4px;"" >

<option value=""test1"" >test1</option><option value=""test2"" >test2</option>
</select>




</span>";
            dropDownData = dropDownData.Replace("testDropDownListElementName", dropDownElementName);

            return dropDownData;
        }

        #endregion Private Test Methods

    }
}
