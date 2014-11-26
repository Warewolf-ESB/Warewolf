
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
    /// Summary description for ImageTest
    /// </summary>
    [TestClass]
    public class ImageTest {
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();
        public ImageTest() {
            //
            // TODO: Add constructor logic here
            //
        }

        #region Pre And Post Test Activities

        private TestContext testContextInstance;
        string WebserverURI = ServerSettings.WebserverURI;

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

            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.Image_DataList, TestResource.Xpath_Width, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "4");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ErrorMsg, "testImageErrorMessage");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ElementName, "testImageElementName");
            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.Image_DataList, TestResource.Xpath_Fragment, "testImageFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CssClass, "testImageCssClass");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptImg", "testImageCustomScript");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customStyleImg", "testImageCustomStyle");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2ImageURI", "testImageURI");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2ImageDesc", "testImageDesc");

        }
        
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Expected User Interaction

        [TestMethod]
        public void ImageTest_ExpectedUserInteraction_NoNameSpecified_Expected_WebpartGeneratedWithoutId() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_ElementName, "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Image, tempXmlString);
            string expected = @"<img src=""testImageURI"" alt=""testImageDesc"" id="""" class=""testImageCustomStyle""  />testImageCustomScript";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            actual = actual.Replace("\r", "").Replace("\n", "");
            Assert.AreEqual(expected, actual);
        }

        #endregion 

        #region Advanced Options - Custom styling

        [TestMethod()]
        public void ImageTest_AdvancedOptions_CustomStyling_Expected_ClassAttributeSetToSpecifiedStyle() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_CustomStyle, "testStyle");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Image, tempXmlString);
            string expected = @"class=""testStyle""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Advanced Options - Altering Width and Height

        [TestMethod()]
        public void ImageTest_AdvancedOptions_AlteringHeight_Expected_HeightSpecifiedInBlock() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "60px");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Image, tempXmlString);
            string expected = @"style=""height:60px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void ImageTest_AdvancedOptions_AlteringHeightTextValue_Expected_WidthAttributeSetOnWebpartToText() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Height, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Image, tempXmlString);
            string expected = @"style=""height:text;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void ImageTest_AdvancedOptions_AlteringWidth_Expected_WidthSpecifiedInWidthBlock() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "60px");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Image, tempXmlString);
            string expected = @"style=""width:60px;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod()]
        public void ImageTest_AdvancedOptions_AlteringWidth_Expected_HeightAttributeSetOnWebpartToText() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_Width, "text");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Image, tempXmlString);
            string expected = @"style=""width:text;""";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);

            StringAssert.Contains(actual, expected);
        }

        #endregion

        #region Advanced Options - Custom Scripting


        // PBI 5376 : Scripts not being wrapped in script regions

        // Test currently broken: BugId 6600

        //[TestMethod()]
        //public void ImageTest_AdvancedOptions_SimpleCustomScript_Expected_SimpleJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptImg", "alert('hi');");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Image, tempXmlString);
        //    string expected = @"<script>alert('hi');</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376 : Same as above

        //[TestMethod()]
        //public void ImageTest_AdvancedOptions_ComplexCustomScript_Expected_ComplexJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptImg", @"var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Image, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = function(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    actual = actual.Replace("\n", "").Replace("\r", "");

        //    StringAssert.Contains(actual, expected);
        //}

        //// PBI 5376 : Same as above

        //[TestMethod()]
        //public void ImageTest_AdvancedOptions_MalformedCustomScript_Expected_MalformedJavascriptInjected() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2customScriptImg", @"var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = functio(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Image, tempXmlString);
        //    string expected = @"<script>var xmlHttpRequest = new XMLHttpRequest();   var url = 'http://localhost:2234/services/sashen_workflow';   xmlHttpRequest.open('GET', url, true);   xmlHttpRequest.onreadystatechange = functio(){      document.getElementById('lblTest').innerHTML=xmlHttpRequest.responseText;   }   xmlHttpRequest.send(null);</script>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);
        //    actual = actual.Replace("\n", "").Replace("\r", "");

        //    StringAssert.Contains(actual, expected);
        //}

        #endregion

        #region WTF

        //[TestMethod()]
        //public void ImageTest_ExpectedUserInteractionResetButton_Test()
        //{
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, "Dev2elementNameButton=ButtonClicked&displayTextButton=Reset&btnType=reset&Dev2tabIndexButton=&Dev2toolTipButton=&Dev2customStyleButton=&Dev2widthButton=&Dev2heightButton=&done=");
        //    string expected = @"";

        //    string ReponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ReponseData);

        //    Assert.AreEqual(expected, actual);
        //}

//        [TestMethod()]
//        public void Button_ExpectedUserInteraction_NoDisplayText_Test()
//        {
//            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, "Dev2elementNameButton=&displayTextButton=Reset&btnType=reset&Dev2tabIndexButton=&Dev2toolTipButton=&Dev2customStyleButton=&Dev2widthButton=&Dev2heightButton=&done=");
//            string expected = @"<button type=""reset"" name=""ButtonClicked"" id=""ButtonClicked"" value=""Reset""   class='' >Reset</button>
//
//<script>
//  $('#ButtonClicked').click(function(){
//      
//   });
//</script>";

//            string ResponseData = TestHelper.PostDataToWebserver(PostData);
//            string actual = TestHelper.ReturnFragment(ResponseData);

//            Assert.AreEqual(expected, actual);
//        }

        #endregion
        
    

        }
}
