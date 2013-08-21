//using Unlimited.Framework;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Xml.Linq;
//using System.Collections.Generic;
//using System.Dynamic;
//using System.Collections;
//using System.Linq;
//using System.Xml;
//using System.Reflection;
//using System.Diagnostics;
//using System.IO;
//using System.Text.RegularExpressions;

//namespace Unlimited.UnitTest.Framework
//{
    
    
//    /// <summary>
//    ///This is a test class for UnlimitedObjectTest and is intended
//    ///to contain all UnlimitedObjectTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class UnlimitedObjectTest {

 
//        private TestContext testContextInstance;

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
//        public TestContext TestContext {
//            get {
//                return testContextInstance;
//            }
//            set {
//                testContextInstance = value;
//            }
//        }

//        #region Additional test attributes
//        // 
//        //You can use the following additional attributes as you write your tests:
//        //
//        //Use ClassInitialize to run code before running the first test in the class
//        //[ClassInitialize()]
//        //public static void MyClassInitialize(TestContext testContext)
//        //{
//        //}
//        //
//        //Use ClassCleanup to run code after all tests in a class have run
//        //[ClassCleanup()]
//        //public static void MyClassCleanup()
//        //{
//        //}
//        //
//        //Use TestInitialize to run code before running each test
//        //[TestInitialize()]
//        //public void MyTestInitialize()
//        //{
//        //}
//        //
//        //Use TestCleanup to run code after each test has run
//        //[TestCleanup()]
//        //public void MyTestCleanup()
//        //{
//        //}
//        //
//        #endregion

//        internal class Test {
//            public string FirstName { get; set; }
//            public string LastName { get; set; }
//        }

//        [TestMethod]
//        public void Test_Serialization() {
//            var tests = new List<Test>();
//            tests.Add(new Test {FirstName = "Sameer", LastName = "Chunilall"});
//            tests.Add(new Test {FirstName = "Sameeras", LastName = "Chunilallasdf23"});

//            var genType = tests.GetType().GetGenericArguments();

//            var data = new UnlimitedObject(tests).XmlString;

//        }

//        [TestMethod()]
//        public void Set_Value_Performance_Test() {
//            var elementName = "RowsVar";
//            var value = "0";
//            Stopwatch st = new Stopwatch();

//            dynamic test = new UnlimitedObject();
//            test.abc = 1;
//            st.Start();
//            var rest = UnlimitedObject.Get("abc", new List<string> {test.XmlString });
//            st.Stop();


//            var res = st.ElapsedMilliseconds;
               


//        }

//        [TestMethod()]
//        public void Test_CData() {
//            UnlimitedObject test = new UnlimitedObject();
//            test.GetElement("script").SetValue("if(a>b){ t++;}", true);

//            string tmp = test.GetValue("script");

//           //dynamic tmp = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(xmlData);

//           //dynamic data = tmp.x;
            
//        }

//        [TestMethod()]
//        public void Test_Fragment() {
//            var test= new UnlimitedObject("companyLogo");


//            var r = test.RootName;

//            Stopwatch s = new Stopwatch();

//            s.Start();

//            XElement x = new XElement("Fragment", "<td>");
//            test.Add(x);

//            s.Stop();

//            var re = s.ElapsedMilliseconds;

//        }


//        [TestMethod()]
//        public void Test_GetXmlStringFromUnlimitedObject() {
//            string XmlData = "<ParentServiceName>asdfasd</ParentServiceName>";
//            string _parentServiceName = string.Empty;




//            if (!string.IsNullOrEmpty(XmlData)) {
//                Stopwatch s = new Stopwatch();



//                dynamic d = new UnlimitedObject();
//                d.FirstName = "sameer";

//                s.Start();
//                string test = d.GetValue("FirstName1412341234");
//                s.Stop();

//                var dd = s.ElapsedMilliseconds;

//                s.Reset();

//                s.Start();
//                dynamic dataObject = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(XmlData) as UnlimitedObject;
//                if (dataObject.ParentServiceName is string) {
//                    _parentServiceName = dataObject.ParentServiceName;
//                }
//                //var te = XElement.Parse(XmlData);
//                s.Stop();

//                var r = s.ElapsedMilliseconds;

//                s.Reset();


//                //s.Start();
//                //if (dataObject.ParentServiceName is string) {
//                //    _parentServiceName = dataObject.ParentServiceName;
//                //}
//                //s.Stop();

//                //r = s.ElapsedMilliseconds;
//            }            
//        }

//        [TestMethod()]
//        public void XPath_Attribute_Returns_Result() {
//            var a = @"<Regions>
//<Region Type=""text"" Name=""companyName"">
//    
//
//</Region>
//<Region Type=""image"" Name=""companyLogo"">
//
//
//</Region>
//<Region Type=""menu"" Name=""companyLogo"">
//
//</Region>
//</Regions>
//                      ";


//            UnlimitedObject tmp = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(a);

//            var test = tmp.XPath("//Region/@Type");

            



//        }

//        [TestMethod()]
//        public void Formatting_Test() {

//            var a = new XElement("abc", @"
//                      <xData>
//                        <WebServerUrl>http://rsaklfsRTvrgendev:2234/services/LiteralControl.wiz/instances/79c5b234-65aa-4504-a99d-bfece8574fcf/bookmarks/576e3dc7-69ff-4700-8a6b-0168b7d08082</WebServerUrl>
//                        <XmlData />
//                        <Async />
//                        <title />
//                        <span>Value</span>
//                        <input />
//                        <Name>displayTextLabel</Name>
//                        <Value>Display Text</Value>
//                        <Submit>Submit</Submit>
//                      </xData>
//");

//            var test = new UnlimitedObject();
//            test.GetElement("abc").SetValue(@"<x><a>sameer</a></x>", true);
//            var tmp = test.XmlString;

//        }



//        [TestMethod()]
//        public void test_delete() {
//            string del = @"
//<Dev2ServiceInput3>
//  <XmlData>
//    <XmlData>
//      <XmlData>
//        <Dev2ServiceInput>
//          <x>
//            <CurrentWebPart>
//              <WebPart>
//                <WebPartServiceName>Label</WebPartServiceName>
//                <ColumnIndex>0</ColumnIndex>
//                <RowIndex>0</RowIndex>
//                <Dev2XMLResult>
//                  <sr>
//                    <sr>
//                      <xData>
//                        <WebServerUrl>http://rsaklfsRTvrgendev:2234/services/LiteralControl.wiz/instances/79c5b234-65aa-4504-a99d-bfece8574fcf/bookmarks/576e3dc7-69ff-4700-8a6b-0168b7d08082</WebServerUrl>
//                        <XmlData />
//                        <Async />
//                        <title />
//                        <span>Value</span>
//                        <input />
//                        <Name>displayTextLabel</Name>
//                        <Value>Display Text</Value>
//                        <Submit>Submit</Submit>
//                      </xData>
//                    </sr>
//                  </sr>
//                </Dev2XMLResult>
//              </WebPart>
//            </CurrentWebPart>
//          </x>
//        </Dev2ServiceInput>
//        <Error>No Value Set</Error>
//      </XmlData>
//    </XmlData>
//  </XmlData>
//  <Resumption>
//    <ParentWorkflowInstanceId>70bd64d8-bb99-4c62-9ad6-8109d5800efa</ParentWorkflowInstanceId>
//    <ParentServiceName></ParentServiceName>
//  </Resumption>
//  <Service>ReplacePartWithErrorMsg</Service>
//  <DynamicServiceFrameworkMessage>
//    <Error>Error in request - inbound message contains error tag</Error>
//  </DynamicServiceFrameworkMessage>
//</Dev2ServiceInput3>
//";

//            UnlimitedObject d = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(del);
//            d.RemoveElementsByTagName("Dev2ServiceInput");

//        }

//        [TestMethod()]
//        public void TextXml_XElement_Enumeration() {
//            string data = @"
//            <x>
//                <firstname></firstname>
//                <FormView>
//                    <html></html>
//                </FormView>
//            </x>
//
//";

//            var test = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(data);
//            var d = test.FormView;


//            test.FormView = "asdfasdfasdfasdf";

//        }

//        /// <summary>
//        ///A test for UnlimitedObject Constructor
//        ///</summary>
//        [TestMethod()]
//        public void UnlimitedObjectConstructorTest() {
//            string xml = string.Empty; // TODO: Initialize to an appropriate value
//            UnlimitedObject target = new UnlimitedObject(xml);
//            Assert.Inconclusive("TODO: Implement code to verify target");
//        }

//        /// <summary>
//        ///A test for UnlimitedObject Constructor
//        ///</summary>
//        [TestMethod()]
//        public void UnlimitedObjectConstructorTest1() {
//            object dataObject = null; // TODO: Initialize to an appropriate value
//            UnlimitedObject target = new UnlimitedObject(dataObject);
//            Assert.Inconclusive("TODO: Implement code to verify target");
//        }

//        /// <summary>
//        ///A test for UnlimitedObject Constructor
//        ///</summary>
//        [TestMethod()]
//        public void UnlimitedObjectConstructorTest2() {
//            UnlimitedObject target = new UnlimitedObject();
//            Assert.Inconclusive("TODO: Implement code to verify target");
//        }

//        /// <summary>
//        ///A test for UnlimitedObject Constructor
//        ///</summary>
//        [TestMethod()]
//        public void UnlimitedObjectConstructorTest3() {
//            XElement xml = null; // TODO: Initialize to an appropriate value
//            UnlimitedObject target = new UnlimitedObject(xml);
//            Assert.Inconclusive("TODO: Implement code to verify target");
//        }

//        /// <summary>
//        ///A test for AddResponse
//        ///</summary>
//        [TestMethod()]
//        public void AddResponseTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            UnlimitedObject response = null; // TODO: Initialize to an appropriate value
//            target.AddResponse(response);
//            Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        }

//        /// <summary>
//        ///A test for ElementExists
//        ///</summary>
//        [TestMethod()]
//        public void ElementExistsTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            string elementName = string.Empty; // TODO: Initialize to an appropriate value
//            bool expected = false; // TODO: Initialize to an appropriate value
//            bool actual;
//            actual = target.ElementExists(elementName);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for GenerateServiceRequest
//        ///</summary>
//        [TestMethod()]
//        public void GenerateServiceRequestTest() {
//            string serviceName = "resumeTest";
//            object dataTags = null; // TODO: Initialize to an appropriate value
//            List<string> dataSources = new List<string>() { @"<XmlData>
//  <XmlData>
//    <InputData>
//      <Dev2BoundServiceName>resumetest</Dev2BoundServiceName>
//      <Dev2RowDelimiter>Pet</Dev2RowDelimiter>
//      <Dev2BoundFields>
//        <Dev2Field>id</Dev2Field>
//        <Dev2Field>type</Dev2Field>
//      </Dev2BoundFields>
//      <callback>__embeddedFn__</callback>
//    </InputData>
//  </XmlData>
//</XmlData>", 
//@"<Resumption><ParentWorkflowInstanceId>5b230ddc-ff62-4f31-9e7d-09c1a9178c74</ParentWorkflowInstanceId><ParentServiceName>JSON Binder</ParentServiceName></Resumption>" };
//            var actual = UnlimitedObject.GenerateServiceRequest(serviceName, dataTags, dataSources);

//        }

//        /// <summary>
//        ///A test for GenerateServiceRequest
//        ///</summary>
//        [TestMethod()]
//        public void GenerateServiceRequestTest1() {
//            string serviceName = string.Empty; // TODO: Initialize to an appropriate value
//            string dataTags = string.Empty; // TODO: Initialize to an appropriate value
//            string workflowInputData = string.Empty; // TODO: Initialize to an appropriate value
//            string lastResult = string.Empty; // TODO: Initialize to an appropriate value
//            string resultPipeLine = string.Empty; // TODO: Initialize to an appropriate value
//            string expected = string.Empty; // TODO: Initialize to an appropriate value
//            string actual;
//            actual = UnlimitedObject.GenerateServiceRequest(serviceName, dataTags, workflowInputData, lastResult, resultPipeLine);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for GetCsvAsUnlimitedObject
//        ///</summary>
//        [TestMethod()]
//        public void GetCsvAsUnlimitedObjectTest() {
//            string csv = string.Empty; // TODO: Initialize to an appropriate value
//            object expected = null; // TODO: Initialize to an appropriate value
//            object actual;
//            actual = UnlimitedObject.GetCsvAsUnlimitedObject(csv);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for GetDataValueFromUnlimitedObject
//        ///</summary>
//        [TestMethod()]
//        public void GetDataValueFromUnlimitedObjectTest() {
//            string tagName = string.Empty; // TODO: Initialize to an appropriate value
//            object unlimitedObjectSource = null; // TODO: Initialize to an appropriate value
//            string expected = string.Empty; // TODO: Initialize to an appropriate value
//            string actual;
//            actual = UnlimitedObject.GetDataValueFromUnlimitedObject(tagName, unlimitedObjectSource);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for GetDataValueFromUnlimitedObject
//        ///</summary>
//        [TestMethod()]
//        public void GetDataValueFromUnlimitedObjectTest1() {
//            string tagName = string.Empty; // TODO: Initialize to an appropriate value
//            List<object> searchUnlimitedObjects = null; // TODO: Initialize to an appropriate value
//            string expected = string.Empty; // TODO: Initialize to an appropriate value
//            string actual;
//            actual = UnlimitedObject.GetDataValueFromUnlimitedObject(tagName, searchUnlimitedObjects);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for GetDynamicMemberNames
//        ///</summary>
//        [TestMethod()]
//        public void GetDynamicMemberNamesTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            IEnumerable<string> expected = null; // TODO: Initialize to an appropriate value
//            IEnumerable<string> actual;
//            actual = target.GetDynamicMemberNames();
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for GetElement
//        ///</summary>
//        [TestMethod()]
//        public void GetElementTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            string Name = string.Empty; // TODO: Initialize to an appropriate value
//            object expected = null; // TODO: Initialize to an appropriate value
//            object actual;
//            actual = target.GetElement(Name);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for GetStringXmlDataAsUnlimitedObject
//        ///</summary>
//        [TestMethod()]
//        public void GetStringXmlDataAsUnlimitedObjectTest() {
//            string xmlData = string.Empty; // TODO: Initialize to an appropriate value
//            object expected = null; // TODO: Initialize to an appropriate value
//            object actual;
//            actual = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(xmlData);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for GetTagName
//        ///</summary>
//        [TestMethod()]
//        public void GetTagNameTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            string expected = string.Empty; // TODO: Initialize to an appropriate value
//            string actual;
//            actual = target.GetTagName();
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for GetValue
//        ///</summary>
//        [TestMethod()]
//        public void GetValueTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            string Name = string.Empty; // TODO: Initialize to an appropriate value
//            string expected = string.Empty; // TODO: Initialize to an appropriate value
//            string actual;
//            actual = target.GetValue(Name);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for GetXmlDataFromObject
//        ///</summary>
//        [TestMethod()]
//        public void GetXmlDataFromObjectTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            object dataObject = null; // TODO: Initialize to an appropriate value
//            string expected = string.Empty; // TODO: Initialize to an appropriate value
//            string actual;
//            actual = target.GetXmlDataFromObject(dataObject);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for IsDescendantOf
//        ///</summary>
//        [TestMethod()]
//        public void IsDescendantOfTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            string elementName = string.Empty; // TODO: Initialize to an appropriate value
//            bool expected = false; // TODO: Initialize to an appropriate value
//            bool actual;
//            actual = target.IsDescendantOf(elementName);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for Load
//        ///</summary>
//        [TestMethod()]
//        public void LoadTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            string XmlData = string.Empty; // TODO: Initialize to an appropriate value
//            target.Load(XmlData);
//            Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        }

//        /// <summary>
//        ///A test for RemoveElementByTagName
//        ///</summary>
//        [TestMethod()]
//        public void RemoveElementByTagNameTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            string tagName = string.Empty; // TODO: Initialize to an appropriate value
//            target.RemoveElementByTagName(tagName);
//            Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        }

//        /// <summary>
//        ///A test for ReplaceTagName
//        ///</summary>
//        [TestMethod()]
//        public void ReplaceTagNameTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            string sourceTagName = string.Empty; // TODO: Initialize to an appropriate value
//            string targetTagName = string.Empty; // TODO: Initialize to an appropriate value
//            string expected = string.Empty; // TODO: Initialize to an appropriate value
//            string actual;
//            actual = target.ReplaceTagName(sourceTagName, targetTagName);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for SetValue
//        ///</summary>
//        [TestMethod()]
//        public void SetValueTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            string value = string.Empty; // TODO: Initialize to an appropriate value
//            bool expected = false; // TODO: Initialize to an appropriate value
//            bool actual;
//            actual = target.SetValue(value);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for TryGetIndex
//        ///</summary>
//        [TestMethod()]
//        public void TryGetIndexTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            GetIndexBinder binder = null; // TODO: Initialize to an appropriate value
//            object[] indexes = null; // TODO: Initialize to an appropriate value
//            object tmp = null; // TODO: Initialize to an appropriate value
//            object resultExpected = null; // TODO: Initialize to an appropriate value
//            bool expected = false; // TODO: Initialize to an appropriate value
//            bool actual;
//            actual = target.TryGetIndex(binder, indexes, out tmp);
//            Assert.AreEqual(resultExpected, tmp);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for TryGetMember
//        ///</summary>
//        [TestMethod()]
//        public void TryGetMemberTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            GetMemberBinder binder = null; // TODO: Initialize to an appropriate value
//            object tmp = null; // TODO: Initialize to an appropriate value
//            object resultExpected = null; // TODO: Initialize to an appropriate value
//            bool expected = false; // TODO: Initialize to an appropriate value
//            bool actual;
//            actual = target.TryGetMember(binder, out tmp);
//            Assert.AreEqual(resultExpected, tmp);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for TrySetMember
//        ///</summary>
//        [TestMethod()]
//        public void TrySetMemberTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            SetMemberBinder binder = null; // TODO: Initialize to an appropriate value
//            object value = null; // TODO: Initialize to an appropriate value
//            bool expected = false; // TODO: Initialize to an appropriate value
//            bool actual;
//            actual = target.TrySetMember(binder, value);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }


//        [TestMethod()]
//        public void Delineate__Valid_XML_Fragment_Returns_ListOFXElement() {
//            string xmlData = @"<x>a</x><x>b</x><x>d</x>";

//            dynamic t = new UnlimitedObject();

//            t.SetValue(xmlData);

//            dynamic tmp = UnlimitedObject.DelineateXMLString(xmlData);

//            Assert.IsInstanceOfType(tmp, typeof(List<XElement>));
//        }


//        [TestMethod()]
//        public void Delineate__InValid_XML_Fragment_Returns_String() {
//            string xmlData = @"<x>a</x>
//<x>b</x>
//<x>c</x>
//<x>d</x>asdfasd
//";
//            dynamic tmp = UnlimitedObject.DelineateXMLString(xmlData);

//            Assert.IsInstanceOfType(tmp, typeof(string));        
//        }



//        [TestMethod]
//        public void TestXPath_JSON() {
//              string xpathJSON = @"
//<data><x>
//{""CountryID"": ""61"", ""Description"":""Hungary""},
//</x><x>
//{""CountryID"": ""62"", ""Description"":""Iceland""},
//</x><x>
//{""CountryID"": ""63"", ""Description"":""India""},
//</x>
//</data>
//";

//              UnlimitedObject test = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(xpathJSON);

//              var abc = test.XPath("//x/text()");
        
//        }

//        [TestMethod()]
//        public void Test_Html_Parsing() {
//            string html = @"&lt;html&gt;\n  &lt;head&gt;\n    &lt;link href=\""http://rsaklfsvrgendev:1234/css/plainwebsite.css\"" rel=\""stylesheet\"" type=\""text/css\"" /&gt;\n  &lt;/head&gt;\n  &lt;script src=\""/scripts/jquery-1.6.2.min.js\""&gt;&lt;/script&gt;&lt;script src=\""/scripts/jquery.validationEngine-en.js\""&gt;&lt;/script&gt;&lt;script src=\""/scripts/jquery.validationEngine.js\""&gt;&lt;/script&gt;&lt;link rel=\""stylesheet\"" href=\""/css/validationEngine.jquery.css\"" type=\""text/css\""/&gt;&lt;script&gt;$(document).ready(function(){$(\""#Dev2Form\"").validationEngine('attach', {promptPosition : \""centerRight\"", scroll: false});});&lt;/script&gt;\n  &lt;body&gt;&lt;form id=\""Dev2Form\"" action=\""/services/Capture SMS Account/instances/4b9ee938-b4a6-4a59-8f5e-25f508491248/bookmarks/cd0cb8ad-bc4d-4cc3-934a-6dc8ec1ae2e1\"" method=\""post\""&gt;\n    &lt;h2&gt;\n      Widgets Incorporated\n    &lt;/h2&gt;\n    &lt;div style=\""float:left;width:250px;\""&gt;\n      &lt;ul&gt;\n        &lt;li&gt;\n          &lt;a href=\""#\""&gt;Leave application&lt
//;/a&gt;\n        &lt;/li&gt;\n        &lt;li&gt;\n          &lt;a href=\""#\""&gt;Currency Converter&lt;/a&gt;\n        &lt;/li&gt;\n        &lt;li&gt;\n          &lt;a href=\""#\""&gt;Lead Capture&lt;/a&gt;\n        &lt;/li&gt;\n        &lt;li&gt;\n          &lt;a href=\""#\""&gt;Record Note&lt;/a&gt;\n        &lt;/li&gt;\n        &lt;li&gt;\n          &lt;a href=\""/services/Search for SMS Customer\""&gt;Search for Company&lt;/a&gt;\n        &lt;/li&gt;\n      &lt;/ul&gt;\n    &lt;/div&gt;\n    &lt;div style=\""float:left;width:450px;margin-left:10px;border-left:1px solid #e2e2e2;padding:10px;\""&gt;\n      &lt;div&gt;\n  &lt;div style=\""padding-bottom:4px;\""&gt;&lt;span name=\""Label\""&gt;&lt;STRONG&gt;&lt;FONT size=6&gt;Account&amp;nbsp;Information&lt;/FONT&gt;&lt;/STRONG&gt;&lt;/span&gt;&lt;/div&gt;\n&lt;/div&gt;&lt;div&gt;\n  &lt;div style=\""padding-bottom:4px;\""&gt;&lt;span name=\""ErrorLabel\"" style=\""font-weight:bold;color:red;\""&gt;&lt;/span&gt;&lt;/div&gt;\n&lt;/div&gt;&lt;table&gt;\n  &lt;tr&gt;\n    &lt;td st
//yle=\""padding:4px;border-right:1px solid #e2e2e2;width:120px;\""&gt;Account Name&lt;/td&gt;\n    &lt;td style=\""padding:4px;\""&gt;&lt;input type=\""text\"" name=\""PageAccountName\"" id=\""PageAccountName\"" class=\""\"" value=\""\"" /&gt;&lt;/td&gt;\n  &lt;/tr&gt;\n&lt;/table&gt;&lt;table&gt;\n  &lt;tr&gt;\n    &lt;td style=\""padding:4px;border-right:1px solid #e2e2e2;width:120px;\""&gt;&lt;/td&gt;\n    &lt;td style=\""padding:4px;\""&gt;&lt;input type=\""submit\"" name=\""SubmitCapture\"" value=\""Save New Account\"" /&gt;&lt;/td&gt;\n  &lt;/tr&gt;\n&lt;/table&gt;&lt;div&gt;\n  &lt;div&gt;&lt;u&gt;Current Accounts&lt;/u&gt;&lt;/div&gt;\n  &lt;div style=\""padding-bottom:4px;\""&gt;&lt;span name=\""CurrentAccounts\""&gt;&lt;/span&gt;&lt;/div&gt;\n&lt;/div&gt;&lt;table&gt;\n  &lt;tr&gt;\n    &lt;td style=\""padding:4px;border-right:1px solid #e2e2e2;width:120px;\""&gt;&lt;/td&gt;\n    &lt;td style=\""padding:4px;\""&gt;&lt;TABLE class=\""data_table\""&gt;&lt;TableOutput&gt;\n  &lt;XmlData&gt;\n    &lt;TR&gt;\n      &lt;TH&gt;Account&lt;/TH
//&gt;\n      &lt;TH&gt;SMS's Left&lt;/TH&gt;\n      &lt;TH&gt;Reference ID&lt;/TH&gt;\n    &lt;/TR&gt;\n    &lt;TR&gt;\n      &lt;TD&gt;Test&lt;/TD&gt;\n      &lt;TD&gt;0&lt;/TD&gt;\n      &lt;TD&gt;ead1c1fa-d808-44c7-ac98-e2e2b11169bc&lt;/TD&gt;\n    &lt;/TR&gt;\n  &lt;/XmlData&gt;\n&lt;/TableOutput&gt;&lt;/TABLE&gt;&lt;/td&gt;\n  &lt;/tr&gt;\n&lt;/table&gt;\n    &lt;/div&gt;\n  &lt;/form&gt;&lt;/body&gt;\n&lt;/html&gt;";

//            XElement.Parse(html);

//        }

//        [TestMethod()]
//        public void TestJson() {
//            string json = @"
//jsonp1320652154835([
//{""CountryID"":""121"",""Description"":""Saint Kitts and Nevis""},
//])
//";

//            json = json.Replace("\r\n",string.Empty).Replace(",])", "])");


//        }

//        [TestMethod()]
//        public void RegExRegionFinder() {
//            string region = @"
//<Xml>
//  <CountryID>129</CountryID>
//  <AutoServiceName>Get Countries</AutoServiceName>
//  <Prefix>so</Prefix>
//  <callback>jsonp123123</callback>
//  <rowdelimiter>Table</rowdelimiter>
//  <autofields>
//    <field>CountryID</field>
//    <field>Description</field>
//  </autofields>
//</Xml>
//
//";
//            var data = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(region);

//            string transform = "[[xpath('//[[field]]')]]";

//            string start = @"\[\[";
//            string end = @"\]\]";

//            Regex regex = new Regex("(?<=" + start + ").*(?=" + end + ")");
//            MatchCollection matchcoll = regex.Matches(transform);
//            foreach (Match match in matchcoll) {
                
//            }




//        }


//        /// <summary>
//        ///A test for XPath
//        ///</summary>
//        [TestMethod()]
//        public void XPathTest() {

//            dynamic people = new UnlimitedObject("People");

//            dynamic person = new UnlimitedObject("Person");
//            person.FirstName = "Sameer";
//            person.Surname = "Chunilall";
//            person.City = "Durban";
//            person.Salary = 10;

//            people.Add(person);

//            person = new UnlimitedObject("Person");
//            person.FirstName = "Chris";
//            person.Surname = "Rowe";
//            person.City = "Hillcrest";
//            person.Salary = 20;

//            people.Add(person);


//            person = new UnlimitedObject("Person");
//            person.FirstName = @"<input type=""Text"" />";
//            person.Surname = "Buchan";
//            person.City = "Hillcrest";
//            person.Salary = 30;

//            people.Add(person);

//            string tmp = people.XPath(@"//Person/FirstName/text()").XmlString;

//        }

//        /// <summary>
//        ///A test for HasError
//        ///</summary>
//        [TestMethod()]
//        public void HasErrorTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            bool actual;
//            actual = target.HasError;
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for InnerXmlString
//        ///</summary>
//        [TestMethod()]
//        public void InnerXmlStringTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            string actual;
//            actual = target.InnerXmlString;
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for IsMultipleRequests
//        ///</summary>
//        [TestMethod()]
//        public void IsMultipleRequestsTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            bool actual;
//            actual = target.IsMultipleRequests;
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for ObjectState
//        ///</summary>
//        [TestMethod()]
//        public void ObjectStateTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            enObjectState expected = new enObjectState(); // TODO: Initialize to an appropriate value
//            enObjectState actual;
//            target.ObjectState = expected;
//            actual = target.ObjectState;
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for Parent
//        ///</summary>
//        [TestMethod()]
//        public void ParentTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            UnlimitedObject expected = null; // TODO: Initialize to an appropriate value
//            UnlimitedObject actual;
//            target.Parent = expected;
//            actual = target.Parent;
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for Requests
//        ///</summary>
//        [TestMethod()]
//        public void RequestsTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            IEnumerable<UnlimitedObject> actual;
//            actual = target.Requests;
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for XmlString
//        ///</summary>
//        [TestMethod()]
//        public void XmlStringTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            string actual;
//            actual = target.XmlString;
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for xmlData
//        ///</summary>
//        [TestMethod()]
//        public void xmlDataTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value
//            XElement expected = null; // TODO: Initialize to an appropriate value
//            XElement actual;
//            target.xmlData = expected;
//            actual = target.xmlData;
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        /// <summary>
//        ///A test for ElementOrAttributeExists
//        ///</summary>
//        [TestMethod()]
//        public void ElementOrAttributeExistsTest() {
//            UnlimitedObject target = new UnlimitedObject(); // TODO: Initialize to an appropriate value

//            target.Load(@"<x><source value=""test""><data1><data name=""test""/></data1></source></x>");

//            Assert.IsFalse(target.ElementExists("name"));
//            Assert.IsTrue(target.ElementOrAttributeExists("name"));


//        }

//        [TestMethod]
//        public void TestModelBinding() {
//            string returnValue = string.Empty;
//            XmlDocument xdoc = new XmlDocument();



//                XmlElement xelem = xdoc.CreateElement("Error");
//                xelem.InnerText = "ASDFASD";

//                xdoc.AppendChild(xelem);

            


            

//        }

//        [TestMethod()]
//        public void TestXml() {
            
//        }


//        [TestMethod()]
//        public void Parse() {
//            string xmlData = @"<XmlData>
//                                  <Score>&lt;PolicyNo&gt;CR100100&lt;/PolicyNo&gt;
//                                &lt;DebitRunID&gt;25&lt;/DebitRunID&gt;
//                                &lt;LUCCollectionProfile&gt;SunPayMon&lt;/LUCCollectionProfile&gt;
//                                &lt;SuccessScore&gt;1&lt;/SuccessScore&gt;
//                                &lt;FailScore&gt;0&lt;/FailScore&gt;
//
//
//                                </Score>
//                                  <Score>&lt;PolicyNo&gt;CR100100&lt;/PolicyNo&gt;
//                                &lt;DebitRunID&gt;25&lt;/DebitRunID&gt;
//                                &lt;LUCCollectionProfile&gt;SatPayMon&lt;/LUCCollectionProfile&gt;
//                                &lt;SuccessScore&gt;1&lt;/SuccessScore&gt;
//                                &lt;FailScore&gt;0&lt;/FailScore&gt;
//
//
//                                </Score>
//                                </XmlData>";

//            XElement data = XElement.Parse(xmlData);

//        }

//        /// <summary>
//        /// Recursively reflects over an object and returns an xml representation of its members
//        /// </summary>
//        /// <param name="dataObject">The object that we want to return and xml representation of</param>
//        /// <returns>string containing XML representation of the object</returns>
//        public object XmlDataToObject(XElement data, Type targetType) {

//            if (targetType == null)
//                throw new ArgumentException("Target Type cannot be null");

//            var xData = new UnlimitedObject(data);

//            //Set the root node of the xml document that we are building
//            //to the type name of the object.
//            //e.g if the object is being passed in is a SqlException type then the
//            //root tag will be SqlException
//            dynamic xmlData = new UnlimitedObject(data);
//            object objectInstance = Activator.CreateInstance(targetType);
            
//            return XmlDataToObjectInstance(xData, objectInstance);
//        }

//        public object XmlDataToObjectInstance(UnlimitedObject data, object objectInstance) {

//            //Iterate through every property in the object 
//            //and process it into an xml representation.
//            //If the property if of a type that implements 
//            //ICollection in its inheritance/implementation
//            //hierarchy e.g string[] or List<object> then 
//            //process each item in the collection recursively 
//            //by calling this method again to process the complex type
//            objectInstance.GetType().GetProperties().ToList().ForEach(e => {
//                try {
//                    //Get property itself - not that we are not looking to the property
//                    //type, but the property itself
//                    object propValue = e.GetValue(objectInstance, null);
//                    string propName = e.Name;

//                    //Process 
//                    if (propValue is ICollection) {
//                        foreach (object item in (propValue as IEnumerable)) {
//                            XmlDataToObjectInstance(data, item);
//                        }
//                    }
//                    else {

                        

//                        e.SetValue(objectInstance, data.GetValue(propName), null);
//                    }
//                }
//                catch (Exception ex) {
//                    throw;
//                }
//            });


//            return objectInstance;

//        }

//        [TestMethod]
//        public void TestWebService() {
//            Uri testWebService = new Uri("http://rsaklfchrisrowe:2815/utility.asmx");
//            Unlimited.Framework.DynamicServices.WebServiceInvoker ws = new Unlimited.Framework.DynamicServices.WebServiceInvoker(testWebService);
//            var output = ws.InvokeMethod<string>("Utility", "GetWeather", new object[] { "Durban"});


//            //CurrencyConvertor c = new CurrencyConvertor();
//            //var t = c.ConversionRate(Currency.AED, Currency.AFA);
            


//        }

//        [TestMethod]
//        public void WebService_Enum_Params() {

            
//            var methodName = "ConversionRate";
//            var args = new object[] { "ZAR", "USD" };

//            var assmbly = Assembly.LoadFile(@"C:\Development\Dev Source\Unlimited.UnitTest.Framework\bin\Debug\test.dll");
            
//            object obj =  assmbly.CreateInstance("CurrencyConvertor");

//            Type type = obj.GetType();

//            List<object> typedArgs = new List<object>();


//            var argsCount = 0;
//            type.GetMethod(methodName).GetParameters().ToList().ForEach(par => {
//                var paramType = par.ParameterType;
//                if (paramType.IsEnum) {
//                    typedArgs.Add(Enum.Parse(paramType, args[argsCount].ToString()));
//                    argsCount++;
//                }

//                if (paramType.IsClass) {
//                    var newparam = Activator.CreateInstance(paramType);
//                    var propertyCount = newparam.GetType().GetProperties().Count();
//                    var argCount = args.Count();

//                    if (propertyCount == argCount) {

//                    }
//                    else {
//                        throw new Exception("Parameters provided in the service definition do not match any web method");
//                    }
//                }
//            });

//            //int counter =1 ;
//            //types.ToList().ForEach(c => {


//            //    if (c.IsEnum) {
//            //        Enum.Parse(c.ReflectedType, args[counter])

//            //    }

//            //    var d = Activator.CreateInstance(c);


//            //});
            
//            var res = type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, obj, typedArgs.ToArray());


//        }


//        [TestMethod()]
//        public void GetMember_PerfTest() {
//            dynamic tmp = null;
//            bool returnVal = false;
//            string name = "S";

//            XElement xmlData = XElement.Parse("<XmlData><S>asdfasdf</S></XmlData>");

//            //Check if there is a match in the xmldocument either node or attribute with the name of the property the developer is requesting
//            Stopwatch s = new Stopwatch();

//            s.Start();

//            var match = xmlData.DescendantsAndSelf(name);

//            //s.Stop();

//            //var r = s.ElapsedMilliseconds;

//            //s.Reset();

//            if (match.Count() > 0) {
//                //If there is a single match that has children or attributes then return a generic list containing a single UnlimitedObject
//                //We do this to enable iteration.
//                if (match.Count() == 1) {
//                    if (match.First().Descendants().Count() > 0 || match.First().HasAttributes) {
//                        //If the match has attributes return a generic list as this makes
//                        //iterations simple using a foreach
//                        if (match.First().HasAttributes) {
//                            tmp = new List<UnlimitedObject> { new UnlimitedObject(match.FirstOrDefault()) };
//                        }
//                        //If the match does not have attributes we 
//                        //can safely assume that the application
//                        //is creating an xml document and does not require
//                        //a generic list of unlimited objects
//                        //If we didn't do this then the notation of 
//                        //XmlRequest.AccountInfo.AccountNumber will fail 
//                        //as AccountInfo will return a generic list and 
//                        //assigning Accountnumber to generic list will 
//                        //throw an exception.
//                        else {
//                            tmp = new UnlimitedObject(match.FirstOrDefault());

//                        }
//                    }
//                    else {
//                        //There is only 1 element match - we should return the value of that match
//                        //s.Start();
//                        tmp = match.FirstOrDefault().Value;
//                        //s.Stop();

//                        //r = s.ElapsedMilliseconds;
//                        //s.Reset();
//                    }
//                    returnVal = true;
//                }


//                //s.Start();
//                //There are multiple element matches - we should create a new UnlimitedDynamic object per match
//                if (match.Count() > 1) {
//                    List<UnlimitedObject> matches = new List<UnlimitedObject>();
//                    foreach (XElement subelement in match) {
//                        matches.Add(new UnlimitedObject(subelement));
//                    }
//                    tmp = matches;
//                    returnVal = true;
//                }
//                //s.Stop();
//                //r = s.ElapsedMilliseconds;
//                //s.Reset();

//            }
//            else {
//                if (xmlData.Attribute(XName.Get(name)) != null) {
//                    tmp = xmlData.Attribute(XName.Get(name)).Value;
//                    returnVal = true;
//                }
//                else {

//                    //There is no matching element in the xml document - we create it.
//                    XElement newelement = new XElement(name);
//                    xmlData.Add(newelement);
//                    tmp = new UnlimitedObject(newelement);
//                    returnVal = true;
//                }
//            }

//            s.Stop();

//            var d = s.ElapsedMilliseconds;

            
//        }

//    }

//    public class test {
//        public string Name { get; set; }
//        public string Surname { get; set; }

//    }

//}
