
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
    /// Unit Tests related to the File webpart.
    /// These test will serve as a POST to the file webpart service and checking the HTML Fragment returned in the webserver XML data.
    /// </summary>
    [TestClass]
    public class FileUpLoadTest
    {
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();

        #region Pre and Post Test Activities

        public FileUpLoadTest()
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
            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.FileUpload_DataList, TestResource.Xpath_ElementName, "testFileUploadElementName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ErrorMsg, "testFileUploadErrorMessage");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "fileDisplayText", "testFileUploadDisplayText");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "1");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testFileUploadToolTip");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CustomStyle, "testFileUploadCustomStyle");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Fragment, "testFileUploadFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "requiredF", "on");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "InjectedLabel", "testFileUploadInjectedLabel");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "InjectStar", "testFileUploadInjectStar");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2FileTypes", "testFileUploadFileTypes");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptF", "testFileUploadCustomScript");
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Expected User Interaction Cases


        [TestMethod]
        public void File_ExpectedUserInteraction_AnyFileType_Expected_AcceptAttributeSetToAny()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2FileTypes", "any");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);

            #region Expecteds

            string expectedWebpart = @"<input type=""file"" name=""testFileUploadElementName"" id=""testFileUploadElementName"" accept=""any/*"" title=""testFileUploadToolTip"" tabindex=""1"" class=""requiredClass testFileUploadCustomStyle"" style=""height:4px;width:4px;"" />";
            string expectedOverLay = @"<input type=""text"" name=""testFileUploadElementNameFake"" id=""testFileUploadElementNameFake"" />";
            string expectedOverlayButton = @"<button type=""button"" id=""testFileUploadElementNameUploadBtn""> ... </button>";
            string expectedValidaton = @"<font color=""red"">*</font>";
            string expectedScript = @"<script>
$(""#testFileUploadElementName"").change(function(){ 
 var v = this.value;
 $(""#testFileUploadElementNameFake"").val(v);
});
 // hack for custom file upload control
 $(""#testFileUploadElementName"").hover(function(){ 
   $(""#testFileUploadElementNameFake"").addClass(""ui-state-hover""); 
   $(""#testFileUploadElementNameUploadBtn"").addClass(""ui-state-hover""); 
  },
  function(){ 
   $(""#testFileUploadElementNameFake"").removeClass(""ui-state-hover""); 
   $(""#testFileUploadElementNameUploadBtn"").removeClass(""ui-state-hover""); 
  }
 );

</script>";

            #endregion Expecteds
            List<string> expectedElements = new List<string> { expectedWebpart, expectedOverLay, expectedOverlayButton, expectedValidaton, expectedScript };

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            actual = "<myData>" + actual + "</myData>";
            List<string> actualElements = TestHelper.BreakHTMLElement(actual);
            CollectionAssert.AreEqual(expectedElements, actualElements);
        }

        [TestMethod]
        public void File_ExpectedUserInteraction_TextFileType_Expected_AcceptAttributeSetToText()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2FileTypes", "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"accept=""text/*""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void File_ExpectedUserInteraction_ImageFileType_Expected_AcceptAttributeSetToImage()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2FileTypes", "image");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"accept=""image/*""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void File_ExpectedUserInteraction_VideoFileType_Expected_AcceptAttributeSetToVideo()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2FileTypes", "video");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"accept=""video/*""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void File_ExpectedUserInteraction_ApplicationFileType_Expected_AcceptAttributeSetToApplication()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2FileTypes", "application");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"accept=""application/*""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void File_ExpectedUserInteraction_MissingName_Expected_WebpartErrorDisplayed()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.FileUpload_DataList, TestResource.Xpath_ElementName, "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"<span class=""internalError"">";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
            Assert.IsFalse(actual.Contains(@"<input type=""file"""));
        }

        [TestMethod()]
        public void FileUpLoad_ExpectedUserInteraction_ShowingAssociatedLabel_Expected_LabelDisplayed()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "showTextF", "on");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"testFileUploadDisplayText";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }


        #endregion

        #region Advanced Options - Tab Index

        [TestMethod]
        public void File_ExpectedUserInteraction_TabIndex_Expected_CustomTabIndexOnWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "5");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"tabindex=""5""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void File_ExpectedUserInteraction_TabIndexWithTextValue_Expected_TextasTabIndex()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_TabIndex, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"tabindex=""text""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Advanced Options - Custom styling

        [TestMethod()]
        public void File_AdvancedOptions_CustomStyling_Expected_ClassAttributeSetToSpecifiedStyle()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CustomStyle, "testStyle");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"class=""requiredClass testStyle""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Advanced Options - Altering Width and Height

        [TestMethod()]
        public void File_AdvancedOptions_AlteringWidth_Expected_WidthSpecifiedInWidthBlock()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "50px");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"style=""height:4px;width:50px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void File_AdvancedOptions_EnteringTextInWidth_Expected_WidthAttributeSetOnWebpartToText()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"style=""height:4px;width:text;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void File_AdvancedOptions_AlteringHeight_Expected_HeightSpecifiedInBlock()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "50px");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"style=""height:50px;width:4px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void File_AdvancedOptions_InputingTextInHeight_Expected_HeightAttributeSetOnWebpartToText()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"style=""height:text;width:4px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Advanced Options - Tooltip Text

        [TestMethod()]
        public void File_AdvancedSettings_TooltipText_Expected_TooltipAttributeWithTestingTextGeneratedWithWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "testToolTip");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"title=""testToolTip""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void File_AdvancedSettings_TooltipTextWithInteger_Expected_TooltipGeneratedWithIntegerValue()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ToolTip, "12");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
            string expected = @"title=""12""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Advanced Options - Requiredcheckbox Text

        // PBI 5376: Seems to not work, still get requiredClass in HTML

        [TestMethod()]
        public void File_AdvancedSettings_RequiredCheckBoxNotChecked_Expected_RequiredClassNotAppliedToWebpart()
        {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "requiredF", "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);

            string notExpected = @"requiredClass";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            Assert.IsFalse(actual.Contains(notExpected));
        }

        #endregion

        #region Advanced Options - Custom Scripting

        // PBI 5376 : Script Tags not generated for custom script
        // Test currently broken: BugId 6600

        //[TestMethod()]
        //public void File_AdvancedOptions_SimpleCustomScript_Expected_SimpleJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptF", "alert('hi');");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
        //    string expected = @"<script>alert('hi');</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    actual = actual.Replace("\n", "").Replace("\r", "");
        //    StringAssert.Contains(actual, expected);
        //}

        // PBI 5376: Same As above

        //[TestMethod()]
        //public void File_AdvancedOptions_ComplexCustomScript_Expected_ComplexJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptF", @"var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    actual = actual.Replace("\n", "").Replace("\r", "");

        //    StringAssert.Contains(actual, expected);
        //}

        //[TestMethod()]
        //public void File_AdvancedOptions_MalformedCustomScript_Expected_MalformedJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptF", @"var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open(', url, true);   xmlHttpRequest.onreadystatechangion){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.File, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open(', url, true);   xmlHttpRequest.onreadystatechangion){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    actual = actual.Replace("\n", "").Replace("\r", "");

        //    StringAssert.Contains(actual, expected);
        //}

        #endregion
    }
}
