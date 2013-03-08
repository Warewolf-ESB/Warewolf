// Travis.Frisinger - Not relavent any longer
//using Unlimited.Applications.BusinessDesignStudio.Activities;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Xml.Linq;
//using System.Linq;
//using System.IO;
//using System.Text.RegularExpressions;
//using System.Text;
//using Unlimited.Framework;
//using System.Xml;
//using System.Xml.XPath;

//namespace Dev2.Integration.Tests.Activity.Tests
//{
//    /// <summary>
//    ///This is a test class for binderTest and is intended
//    ///to contain all binderTest Unit Tests
//    ///fd
//    ///</summary>
//    [TestClass()]
//    public partial class binderTest
//    {


//        private TestContext testContextInstance;
//        private static string _testXMLData;
//        private static string _xpathExpressionData;
//        private static List<string> _testAmbientDataList;

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
//        public TestContext TestContext
//        {
//            get
//            {
//                return testContextInstance;
//            }
//            set
//            {
//                testContextInstance = value;
//            }
//        }

//        #region Additional test attributes
//        // 
//        //You can use the following additional attributes as you write your tests:
//        //
//        //Use ClassInitialize to run code before running the first test in the class
//        [ClassInitialize()]
//        public static void MyClassInitialize(TestContext testContext)
//        {
//            _testXMLData = @"
//<Xml>
//  <CountryID>129</CountryID>
//  <Description>South Africa</Description>
//  <AutoServiceName>Get Countries</AutoServiceName>
//  <Prefix>so</Prefix>
//  <callback>jsonp123123</callback>
//  <rowdelimiter>Table</rowdelimiter>
//    <boundfield>Description</boundfield>
//<test>test2</test>
//<test2>Prefix</test2>
//  <autofields>
//    <field>CountryID</field>
//  </autofields>
//</Xml>
//";

//            _testAmbientDataList = new List<string>() { _testXMLData };

//            _xpathExpressionData = @"<sum>
//                             <first>10</first>
//                             <second>20</second>
//                            </sum>";
//        }
//        //
//        //Use ClassCleanup to run code after all tests in a class have run
//        //[ClassCleanup()]
//        //public static void MyClassCleanup()
//        //{
//        //}
//        //
//        //Use TestInitialize to run code before running each test
//        [TestInitialize()]
//        public void InitializeTests()
//        {
//            binder = new DataListBinder();
//        }
//        //
//        //Use TestCleanup to run code after each test has run
//        //[TestCleanup()]
//        //public void MyTestCleanup()
//        //{
//        //}
//        //
//        #endregion

//        #region DataRegion Tests
//        [TestMethod()]
//        public void DataRegion_Tag_Only_Works()
//        {
//            string transform = "[[rowdelimiter]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual("Table", result);
//        }

//        [TestMethod()]
//        public void DataRegion_Side_By_Side()
//        {
//            string transform = "\"[[boundfield]]\":\"[[[[boundfield]]]]\"";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual("\"Description\":\"South Africa\"", result);
//        }

//        [TestMethod()]
//        public void DataRegion_Tag_Only_Invalid_Tokens_Returns_Unparsed_DataRegion()
//        {
//            string transform = "[[rowdelimiter?!@#$?!@%ASDF]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual(transform, result);
//        }

//        [TestMethod()]
//        public void DataRegion_Tag_Only_With_Embedded_Extra_Close_Delimiter_Returns_Data_Concatenated_To_Close_Delimiter()
//        {
//            string transform = "[[rowdelimiter]]]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual("Table]]", result);
//        }

//        //[TestMethod()]
//        //public void DataRegion_Tag_Only_With_Embedded_Open_Delimiter_Returns_Syntax_Error()
//        //{
//        //    string transform = "[[rowdelimiter[[]]";
//        //    string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//        //    Assert.AreEqual(string.Empty, result);
//        //}

//        [TestMethod()]
//        public void DataRegion_Nested_Tag_Works()
//        {
//            string transform = "[[[[field]]]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual("129", result);
//        }

//        [TestMethod()]
//        public void DataRegion_Nested_Tag_With_Invalid_Tokens_Returns_Valid_Result_Concatenated_With_Invalid_Tokens()
//        {
//            string transform = "[[[[CountryID]]!@#$ASDF]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual("[[129!@#$ASDF]]", result);
//        }

//        public void DataRegion_Nested_Field_Return()
//        {
//            string transform = "[[autofields]]";

//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            result.Normalize();
//            Assert.AreEqual(@"<autofields>\r\n
//                              <field>CountryID</field>\r\n
//                            </autofields>", result);
//        }

//        [TestMethod()]
//        public void DataRegion_Duplicated_NestedTag_Test()
//        {
//            string newTestXMLData = @"<Xml>
//                                <lineset1>
//                                    <string>text</string>
//                                    <testing>class1</testing>
//                                </lineset1>
//                                <lineset2>
//                                <string>text1</string>
//                                <testing>class2</testing>
//                                </lineset2>
//                                <autofields>
//                                    <field>string</field>
//                                    <field>testing</field>
//                                    <field>string1</field>
//                                </autofields>
//                            </Xml>";

//            _testAmbientDataList = new List<string> { newTestXMLData };

//            string transform = "[[string]]";

//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            result.Normalize();
//            _testAmbientDataList = new List<string> { _testXMLData };
//            Assert.AreEqual("<XmlData>\r\n  <string>text</string>\r\n  <string>text1</string>\r\n</XmlData>", result);
//        }

//        [TestMethod()]
//        public void DataRegion_Duplicate_Tags_Seperate_Data_List_Test()
//        {
//            _testAmbientDataList.Add(_testXMLData);
//            string transform = "[[autofields]]";
//            string ExpectedReturn = null;

//            ExpectedReturn = "<XmlData>";
//            foreach (string ambientdata in _testAmbientDataList)
//                ExpectedReturn += XPath_Test(ambientdata, "//" + TestingXMLParser(transform, ambientdata), "OuterXmlInclusive");
//            ExpectedReturn += "</XmlData>";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);

//            // This is duplicated, maybe think about using Lambda's here
//            result = result.Replace("\r\n", "");
//            result = result.Replace(" ", "");
//            ExpectedReturn = ExpectedReturn.Replace("\r\n", "");
//            ExpectedReturn = ExpectedReturn.Replace(" ", "");
//            _testAmbientDataList.Remove(_testXMLData);
//            Assert.AreEqual(ExpectedReturn, result);
//        }

//        [TestMethod()]
//        public void DataRegion_XPathExpression_Test()
//        {
//            string xpathTagToEvaluate = "//first";
//            string transform = "[[xpath(" + xpathTagToEvaluate + ")]]";
//            _testAmbientDataList.Add(_xpathExpressionData);

//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            string xpathData = XPath_Test(_xpathExpressionData, xpathTagToEvaluate, "OuterXmlInclusive");
//            string xpath1 = xpathData.ToString();
//            Assert.AreEqual(xpathData, result);
//        }


//        // XPath Expression including Tag data
//        //[TestMethod()]
//        //public void DataRegion_XpathExpression_With_Tag_Test()
//        //{
//        //    string xpathTagToEvaluate = "//first";
//        //    string xpathTagInclusive = "[[test2]]";
//        //    string transform = xpathTagInclusive + "[[xpath(" + xpathTagToEvaluate + ")]]";

//        //    _testAmbientDataList.Add(_xpathExpressionData);
//        //    string XMLTagData = TestingXMLParser(xpathTagInclusive, _testAmbientDataList[0]);
//        //    XMLTagData = XPath_Test(_testAmbientDataList[0], "//" + XMLTagData, "InnerXMLInclusive");
//        //    XMLTagData += XPath_Test(_testAmbientDataList[1], xpathTagToEvaluate, "OuterXmlInclusive");

//        //    string result = ActivityHelper.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//        //    Assert.AreEqual(XMLTagData, result);
//        //}

//        // Jscript - Functionality of JScript Eval
//        [TestMethod()]
//        public void DataRegion_JScript_Function_Evaluation_Test()
//        {
//            //string jscript = ("{{var test = '[[Prefix]]'; var options = \"toolbar=0,status=0,menubar=0,scrollbars=0\"; var newwindow = window.open(\"\",testing.toString(), options); newwindow.document.writeln(testing); newwindow.document.close();}}");
//            //string jscript = (@"{{var test = '[[Prefix]]', return test;}}");
//            string jscript = @"{{var howManyIterations = 11; 
//                                 var sum = new Array(howManyIterations);
//                                 var theSum = 0; 
//                                 sum[0] = 0;
//                                 for(var counter = 1; counter < howManyIterations; counter++) {
//                                    theSum += counter;
//                                    sum[counter] = theSum;
//                                 }
//                                 }}";
//            string transform = jscript;
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual("55", result);
//        }

//        [TestMethod()]
//        public void DataRegion_JScript_Simple_DataTags_Test()
//        {
//            string jscript = "{{var sidebysidetags ='[[Prefix]] :' + '[[rowdelimiter]]'}}";
//            string expected = "so :Table";
//            string transform = jscript;

//            expected = XPath_Test(_testAmbientDataList[0], "//Prefix", "InnerXMLInclusive");
//            expected += String.Format(" :{0}", XPath_Test(_testAmbientDataList[0], "//rowdelimiter", "InnerXMLInclusive"));

//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);

//            Assert.AreEqual(expected, result);
//        }

//        [TestMethod()]
//        public void DataRegion_JScript_NestedTags_SingleNode_Test()
//        {
//            string jscript = @"{{var country; var countryId = '[[field]]'; var i;
//                                 for (i = 0; i <= countryId.lenght;i++) { country = '[[[[boundfield]]]]' }; }}";
//            string actual = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, jscript);
//        }


//        /// <summary>
//        ///A test for FindDataObjectByTagName
//        ///</summary>
//        [TestMethod()]
//        public void FindDataObjectByTagNameTest_MultipleTagsContainSameName_ExpectedListofTagsContainingsReturnedContainingTagName()
//        {
//            List<string> dataList = new List<string>(){ 
//                "<xml><a val=\"test\"></a><a val=\"test1\"></a></xml>",
//                "<xml><a val=\"test1\"></a></xml>",
//                "<xml><a val=\"test2\"></a></xml>"
//            };
//            string tagName = "a"; // TODO: Initialize to an appropriate value
//            IList<UnlimitedObject> actual;
//            actual = binder.FindDataObjectByTagName(dataList, tagName);
//            Assert.IsTrue(actual.Count == 4);
//        }

//        #endregion DataRegion Tests

//        #region XPath Expression Code

//        private string XPath_Test(string data, string tags, string XMLDataType)
//        {
//            XmlDocument xdoc = new XmlDocument();
//            xdoc.LoadXml(data);
//            XPathNavigator docnav = xdoc.CreateNavigator();
//            XPathNodeIterator iterator = docnav.Select(tags);
//            string xpathdata = null;

//            while (iterator.MoveNext())
//            {
//                if (XMLDataType == "OuterXmlInclusive")
//                    xpathdata = iterator.Current.OuterXml.ToString();
//                else if (XMLDataType == "InnerXMLInclusive")
//                    xpathdata = iterator.Current.InnerXml.ToString();
//            }

//            return xpathdata;
//        }

//        #endregion

//        #region JScript C# class implementation
//        /* This method creates a jscript code DOM from string source to generate javascript library
//           Currently under construction as the implementation of jscript requires a little client side code
//         */
//        [TestMethod()]
//        public void JScriptEval()
//        {
//            string DataSource = @"
//            <d>
//                <test>
//                    <CompanyName>0</CompanyName>
//                </test>
//                <IPAddress>2</IPAddress>
//                <DsfProxyHttpContextUserHostAddress>sdfa</DsfProxyHttpContextUserHostAddress>
//                <ServiceName>asdfasdf</ServiceName>
//                <test>
//                    <CompanyName>1</CompanyName>
//                </test>
//                <test>
//                    <CompanyName>2</CompanyName>
//                </test>
//
//            </d>
//
//";
//            string Transform = @"
//<Result>
//{{
//	function validate(id){
//		//var t = new RegExp('(?<Year>[0-9][0-9])(?<Month>([0][1-9])|([1][0-2]))(?<Day>([0-2][0-9])|([3][0-1]))(?<Gender>[0-9])(?<Series>[0-9]{3})(?<Citizenship>[0-9])(?<Uniform>[0-9])(?<Control>[0-9])');
//		//return t.test(id);
//        if(1>2)return true;else return false;
//	}
//
//
//	validate('8004215168083');
//}}
//</Result>
//            ";


//            Transform = @"
//{{
//
//function Test() {
//return '[[IPAddress]]' == '[[DsfProxyHttpContextUserHostAddress]]'
//                && '[[ServiceName]]' == '[[DsfProxyHttpContextRequestedService]]';
//
//        
//}
//Test();
//
//}}
//
//";

//            Transform = @"
//{{
//       function getDate() 
//       {
//	var dt = new Date();
//	var m = (dt.getMonth() + 1) ;	
//	if (10 > m)
//	{
//	      m = ""0"" + m;	    
//	}		
//	
//	var d = dt.getDate();
//	if (10 > d)
//	{
//	      d = ""0"" + d;
//	}
//	
//	var s = dt.getFullYear() + m + d + ""_QLINK_Fails_dataclean.csv"";
//    	
//	return (s);
//       }
//       getDate();
//}}
//";

//            Transform = @"
//{{
//       function GetEndDate() 
//       {
//	var dt = new Date();
//	var m = (dt.getMonth() + 1) ;	
//	var y = dt.getFullYear();	
//	
//	if(m == 12)
//	{
//	      m = ""01"";
//	      y++;
//	}
//	else
//	{
//	      m++;
//                      if (10 > m)
//	      {
//	             m = ""0"" + m;	    
//	      }
//	}
//				
//	var s = y + ""-"" + m + ""-15 23:59:59.000"";
//	return (s);
//       }
//       GetEndDate();
//}}
//
//";

//            Transform = @"[[xpath('//test')]]";

//            string test = binder.TextAndJScriptRegionEvaluator(new List<string>() { DataSource }, Transform);

//            #region test
//            string expected = "<test><CompanyName>0</CompanyName></test><test><CompanyName>1</CompanyName></test><test><CompanyName>2</CompanyName></test>";
//            Assert.AreEqual(expected, test);
//            #endregion

//        }

//        #endregion

//        #region XML Data Parser

//        private string TestingXMLParser(string XMLTag, string XMLData)
//        {
//            string XMLTagdata = null;

//            StringReader sr = new StringReader(XMLTag);

//            while (sr.Peek() != -1)
//            {
//                if ((char)sr.Peek() == '[' || (char)sr.Peek() == ']')
//                {
//                    sr.Read();
//                }
//                else
//                {
//                    XMLTagdata += (char)sr.Peek();
//                    sr.Read();
//                }
//            }

//            return XMLTagdata;
//        }

//        #endregion
//    }

//}
