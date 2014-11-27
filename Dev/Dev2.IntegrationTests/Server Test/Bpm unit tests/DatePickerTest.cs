
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
using WebpartConfiguration.Test.Test_Utils;
//using Dev2ApplicationServer.Unit.Tests;
using Dev2.Integration.Tests.MEF;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.BPM.Unit.Test
{
    /// <summary>
    /// Summary description for DatePickerTest
    /// </summary>
    [TestClass]
    public class DatePickerTest
    {
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();
        #region Pre and Post Test Activities

        public DatePickerTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        string WebserverURI = ServerSettings.WebserverURI;
        string DatePicker = "Date Picker";
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
            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.DatePicker_DataList, TestResource.Xpath_ElementName, "testDatePickerElementName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ErrorMsg, "testDatePickerErrorMessage");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_DisplayText, "testDatePickerDisplayText");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "1");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testDatePickerToolTip");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CustomStyle, "testDatePickerCustomStyle");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Fragment, "testDatePickerFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2DateFormat", "dd/mm/yyyy");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptDP", "testDatePickerCustomScript");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditDP", "yes");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "requiredDP", "false");


        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Expected User Interaction

        [TestMethod()]
        public void DatePicker_ExpectedUserInteraction_CorrectlyFormed_Expected_WebpartGenerated()
        {
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);

            string expectedInput = ReturnDatePickerElement("testDatePickerElementName");
            string expectedScript = ReturnDatePickerScript("testDatePickerElementName");
            List<string> expectedDatePickerRenderData = new List<string> { expectedInput, expectedScript };
            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            CollectionAssert.AreEqual(expectedDatePickerRenderData, actualElements);
        }

        [TestMethod()]
        public void DatePicker_ExpectedUserInteraction_NoName_Expected_SpanWithWebpartError()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.DatePicker_DataList, TestResource.Xpath_ElementName, "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
            string expected = @"<span class=""internalError""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actualElements = TestHelper.BreakHTMLElement(actual);

            StringAssert.Contains(actual, expected, string.Format("Actual: {0}\r\n did not render with {1}", actual, expected));
        }

        [TestMethod()]
        public void DatePicker_ExpectedUserInteraction_DissallowEditing_WebpartReadonlyAttributeTrue()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "allowEditDP", "no");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
            string expected = @"readonly=""true"" disabled=""disabled""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected, "Actual: {0}\r\n did not render with requirement {1}", actual, expected);
        }


        [TestMethod()]
        public void DatePicker_ExpectedUserInteraction_ShowingAssociatedLabel_Expected_WebpartRendersWithAssociatedLabel()
        {
            // showTextDP
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "showTextDP", "on");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "InjectedLabel", "testDatePickerInjectedLabel");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
            string expected = @"testDatePickerDisplayText<input type=""text""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected, "Actual: {0}\r\n did not render with {1} required for association label", actual, expected);
        }

        #endregion

        #region Advanced Options - Custom Script

        // PBI 5376 : custom scripts are not wrapped in script tags
        // Test currently broken: BugId 6600

        //[TestMethod()]
        //public void DatePicker_AdvancedOptions_SimpleJavaScript_Expected_WebpartWithSimpleJavascript() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptDP", "alert('Chasdigal wants a beer frig!!!!');");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker,tempXmlString);
        //    string expected = @"<![CDATA[<span><input type=""text"" name=""testDatePickerElementName"" id=""testDatePickerElementName"" value=""dd/mm/yyyy"" title=""testDatePickerToolTip"" tabindex=""1"" class=""requiredClass testDatePickerCustomStyle"" style=""height:4px;width:4px;"" /><script> /* make region date picker */ $(""#testDatePickerElementName"").datepicker({ dateFormat: 'dd/mm/yy'}); /* clear formating when user clicks */ $(""#testDatePickerElementName"").click(function(){   this.value = """"; }); /* do validation for the date field here */ $(""#testDatePickerElementName"").mouseup(function(){      var pDate = validateDate(this.value, ""dd/mm/yyyy"");  if(pDate == false){   alert(""Invalid date entered. Please use [ dd/mm/yyyy ] format if manually capturing! "");   this.value="""";  } }); $(document).ready(function(){  $(""#testDatePickerElementName"").val('dd/mm/yyyy'); });</script>alert('Chasdigal wants a beer frig!!!!');</span>]]>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    List<string> actualElements = TestHelper.BreakHTMLElement(actual);

        //    Assert.AreEqual(expected, actual);
        //}

        //// PBI 5376 : Script tags not present when script region is present
        //// This also causes an issue with webpart rendering

        //[TestMethod()]
        //public void DatePicker_AdvancedOptions_ComplexJavaScript_Expected_WebpartWithComplexJavascript() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptDP", @"var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker,tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    actual = actual.Replace("\n", "").Replace("\r", "");
        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376: Same As above
        //[TestMethod()]
        //public void DatePicker_AdvancedOptions_MalformedJavaScript_Expected_WebpartWithMalformedJavascript() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptDP", @"var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open(', url, true);   xmlHttpRequesteadystatechange  function){      document.getElementById('lblTestnerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open(', url, true);   xmlHttpRequesteadystatechange  function){      document.getElementById('lblTestnerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    actual = actual.Replace("\n", "").Replace("\r", "");

        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376: Same As above
        //[TestMethod()]
        //public void DatePicker_AdvancedOptions_NormalTextInCustomScript_Test() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptDP", "testCustomScript");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker,tempXmlString);
        //    string expected = @"<script>testCustomScript</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    StringAssert.Contains(actual, expected, "Actual: {0}\r\n does not contain script {1}", actual, expected);
        //}

        #endregion

        #region Advanced Options - Tab index

        [TestMethod()]
        public void DatePicker_AdvancedOptions_TabIndex_Expected_CustomTabIndexOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "2");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
            string expected = @"tabindex=""2""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            Assert.IsTrue(actualElements.Where(c => c.Contains(expected)).ToList().Count == 1, "Actual: {0} \r\n does not contain {1}", actual, expected);

        }


        [TestMethod()]
        public void DatePicker_AdvancedOptions_TextInTabIndex_Expected_TabIndexWithTextOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
            string expected = @"tabindex=""text"" ";
            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            Assert.IsTrue(actualElements.Where(c => c.Contains(expected)).ToList().Count == 1, "Actual: {0} \r\n does not contain {1}", actual, expected);
        }

        #endregion

        #region Advanced Options - Tooltip Text

        [TestMethod()]
        public void DatePicker_AdvancedOptions_TooltipText_Expected_TooltipAttributeWithTestingTextGeneratedWithWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testTooltipText");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
            string expected = @"title=""testTooltipText""";
            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            Assert.IsTrue(actualElements.Where(c => c.Contains(expected)).ToList().Count == 1, "Actual: {0} \r\n does not contain {1}", actual, expected);
        }

        [TestMethod()]
        public void DatePicker_AdvancedOptions_TooltipTextWithIntegers_Expected_TooltipGeneratedWithIntegerValue()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "12");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
            string expected = @"title=""12""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            Assert.IsTrue(actualElements.Where(c => c.Contains(expected)).ToList().Count == 1, "Actual: {0} \r\n does not contain {1}", actual, expected);
        }

        #endregion

        #region Advanced Options - Custom Styling

        // PBI 5376 : CSS Class not being updated.

        [TestMethod()]
        public void DatePicker_AdvancedOptions_CustomStyling_Expected_ClassAttributedUpdatedOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CssClass, "test");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
            string expected = @"class=""requiredClass testDatePickerCustomStyle""";
            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            Assert.IsTrue(actualElements.Where(c => c.Contains(expected)).ToList().Count == 1, "Actual: {0} \r\n does not contain {1}", actual, expected);
        }

        #endregion

        #region Advanced Options - Width and Height

        [TestMethod()]
        public void DatePicker_AdvancedOptions_AlterHeight_Expected_HeightAttributeSetOnWebpartToCustomHeight()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "2234px");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
            string expected = @"style=""height:2234px;width:4px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            Assert.IsTrue(actualElements.Where(c => c.Contains(expected)).ToList().Count == 1, "Actual: {0} \r\n does not contain {1}", actual, expected);
        }

        [TestMethod()]
        public void DatePicker_AdvancedOptions_AlterHeightWithText_Expected_HeightAttributeSetOnWebpartToText()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
            string expected = @"style=""height:text;width:4px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            Assert.IsTrue(actualElements.Where(c => c.Contains(expected)).ToList().Count == 1, "Actual: {0} \r\n does not contain {1}", actual, expected);
        }

        [TestMethod()]
        public void DatePicker_AdvancedOptions_AlteringWidth_Expected_WidthAttributeSetOnWebpartToCustomHeight()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "2234px");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
            string expected = @"style=""height:4px;width:2234px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            Assert.IsTrue(actualElements.Where(c => c.Contains(expected)).ToList().Count == 1, "Actual: {0} \r\n does not contain {1}", actual, expected);
        }

        #endregion

        #region Miscallaeneous

        private string ReturnDatePickerScript(string datePickerName)
        {
            string expectedScript = @"<script>
 /* make region date picker */
 $(""#testDatePickerElementName"").datepicker({ dateFormat: 'dd/mm/yy'});
 /* clear formating when user clicks */
 $(""#testDatePickerElementName"").click(function(){ 
  this.value = """";
 });
 /* do validation for the date field here */
 $(""#testDatePickerElementName"").mouseup(function(){
  
  
  var pDate = validateDate(this.value, ""dd/mm/yyyy"");
  if(pDate == false){
   alert(""Invalid date entered. Please use [ dd/mm/yyyy ] format if manually capturing! "");
   this.value="""";
  }
 });
 $(document).ready(function(){
  $(""#testDatePickerElementName"").val('dd/mm/yyyy');
 });

</script>";
            expectedScript = expectedScript.Replace("testDatePickerElementName", datePickerName);

            return expectedScript;
        }

        private string ReturnDatePickerElement(string elementName)
        {
            string expectedInput = @"<input type=""text"" name=""testDatePickerElementName"" id=""testDatePickerElementName"" value=""dd/mm/yyyy"" title=""testDatePickerToolTip"" tabindex=""1"" class=""requiredClass testDatePickerCustomStyle"" style=""height:4px;width:4px;"" />";
            expectedInput = expectedInput.Replace("testDatePickerElementName", elementName);

            return expectedInput;
        }


        #endregion

    }
}
