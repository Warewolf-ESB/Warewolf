using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Dev2.Data.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Wmhelp.XPath2;

namespace Dev2.Data.Tests
{
    [TestClass][ExcludeFromCodeCoverage]
    public class XPathParserTests
    {
        TestContext _testContextInstance;
        readonly XPathParser _xPathParser = new XPathParser();

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
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
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        const string XMLDocument = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>" +
                                   "<!DOCTYPE dotfuscator SYSTEM \"http://www.preemptive.com/dotfuscator/dtd/dotfuscator_v2.3.dtd\">" +
                                   "<dotfuscator version=\"2.3\">\n" +
                                   "<excludelist>" + "<namespace name=\"Unlimited.Applications.BusinessDesignStudio.Activities\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.Behaviors\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.WindowManagers\" />" + "<namespace name=\"Dev2.Studio.ActivityDesigners\" />" + "<namespace name=\"Dev2.Studio.Views.Workflow\" />" + "<type name=\"Dev2.Activities.DsfExecuteCommandLineActivity\" />" + "<type name=\"Dev2.Activities.DsfForEachItem\" />" + "<type name=\"Dev2.Activities.DsfGatherSystemInformationActivity\" />" + "<type name=\"Dev2.Activities.DsfRandomActivity\" />" + "<type name=\"Dev2.DynamicServices.DsfDataObject\" excludetype=\"false\">" + "<method name=\"ExtractInMergeDataFromRequest\" signature=\"void(object)\" />" + "<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />" + "</type>" + "<type name=\"Dev2.Runtime.Hosting.DynamicObjectHelper\" excludetype=\"false\">" + "<method name=\"SetID\" signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\" />" + "</type>" + "<type name=\"Dev2.CommandLineParameters\">" + "<method name=\"&lt;GetUsage&gt;b__0\" signature=\"void(CommandLine.Text.HelpText)\" />" + "<method name=\"GetUsage\" signature=\"string()\" />" + "<field name=\"&lt;Install&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;IntegrationTestMode&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StartService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StopService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;Uninstall&gt;k__BackingField\" signature=\"bool\" />" + "<propertymember name=\"Install\" />" + "<propertymember name=\"IntegrationTestMode\" />" + "<propertymember name=\"StartService\" />" + "<propertymember name=\"StopService\" />" + "<propertymember name=\"Uninstall\" />" + "</type>" + "<type name=\"Dev2.WebServer\" excludetype=\"false\">" + "<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />" + "</type>" + "</excludelist>" +
                                   "</dotfuscator>";

        const string XMLData = "<excludelist>" + "<namespace name=\"Unlimited.Applications.BusinessDesignStudio.Activities\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.Behaviors\" />" + "<namespace name=\"Dev2.Studio.Core.AppResources.WindowManagers\" />" + "<namespace name=\"Dev2.Studio.ActivityDesigners\" />" + "<namespace name=\"Dev2.Studio.Views.Workflow\" />" + "<type name=\"Dev2.Activities.DsfExecuteCommandLineActivity\" />" + "<type name=\"Dev2.Activities.DsfForEachItem\" />" + "<type name=\"Dev2.Activities.DsfGatherSystemInformationActivity\" />" + "<type name=\"Dev2.Activities.DsfRandomActivity\" />" + "<type name=\"Dev2.DynamicServices.DsfDataObject\" excludetype=\"false\">" + "<method name=\"ExtractInMergeDataFromRequest\" signature=\"void(object)\" />" + "<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />" + "</type>" + "<type name=\"Dev2.Runtime.Hosting.DynamicObjectHelper\" excludetype=\"false\">" + "<method name=\"SetID\" signature=\"void(Dev2.DynamicServices.IDynamicServiceObject, object)\" />" + "</type>" + "<type name=\"Dev2.CommandLineParameters\">" + "<method name=\"&lt;GetUsage&gt;b__0\" signature=\"void(CommandLine.Text.HelpText)\" />" + "<method name=\"GetUsage\" signature=\"string()\" />" + "<field name=\"&lt;Install&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;IntegrationTestMode&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StartService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;StopService&gt;k__BackingField\" signature=\"bool\" />" + "<field name=\"&lt;Uninstall&gt;k__BackingField\" signature=\"bool\" />" + "<propertymember name=\"Install\" />" + "<propertymember name=\"IntegrationTestMode\" />" + "<propertymember name=\"StartService\" />" + "<propertymember name=\"StopService\" />" + "<propertymember name=\"Uninstall\" />" + "</type>" + "<type name=\"Dev2.WebServer\" excludetype=\"false\">" + "<method name=\"CreateForm\" signature=\"Unlimited.Applications.WebServer.Responses.CommunicationResponseWriter(object, string, string)\" />" + "</type>" + "</excludelist>";

        [TestMethod]
        public void ExecutePathWhereGivenXMLDocumentExpectXMLReturned()
        {
            //------------Setup for test--------------------------
            const string XPath = "//type/method";
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(XMLDocument, XPath);
            //------------Assert Results-------------------------
            var data = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(6,data.Count());
            Assert.AreEqual("<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />",data[1]);
        } 
        
        [TestMethod]
        public void ExecutePathWhereGivenXMLDocumentExpectXMLReturnedTestXML()
        {
            //------------Setup for test--------------------------
            const string XPath = "//x/a/text()";
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath("<x><a>1</a></x>", XPath);
            //------------Assert Results-------------------------
            var data = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(1,data.Count());
            Assert.AreEqual("1",data[0]);
        }
        
        [TestMethod]
        public void ExecutePathWhereGivenXMLDataExpectXMLReturned()
        {
            //------------Setup for test--------------------------
            const string XPath = "//type/method";
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(XMLData, XPath);
            //------------Assert Results-------------------------
            var data = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(6,data.Count());
            Assert.AreEqual("<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />",data[1]);
        }

        [TestMethod]
        public void ExecutePathWhereRootNodeInXPathGivenXMLDataExpectXMLReturned()
        {
            //------------Setup for test--------------------------
            const string XPath = "/excludelist/type/method";
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(XMLData, XPath);
            //------------Assert Results-------------------------
            var data = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(6,data.Count());
            Assert.AreEqual("<method name=\"ExtractOutMergeDataFromRequest\" signature=\"void(object)\" />",data[1]);
        }

        [TestMethod]
        public void ExecutePathWhereAttrXPathGivenXMLDataExpectXMLReturned()
        {
            //------------Setup for test--------------------------
            const string XPath = "//type/method/@name";
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(XMLData, XPath);
            //------------Assert Results-------------------------
            var data = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(6, data.Count());
            Assert.AreEqual("name=\"ExtractOutMergeDataFromRequest\"", data[1]);
        }
        
        [TestMethod]
        public void ExecutePathWhereOrXPathGivenXMLDataExpectXMLReturned()
        {
            //------------Setup for test--------------------------
            const string XPath = "//type/method/@name|//type/method/@signature";
            //------------Execute Test---------------------------
            IEnumerable<string> returnData = _xPathParser.ExecuteXPath(XMLData, XPath);
            //------------Assert Results-------------------------
            var data = returnData as IList<string> ?? returnData.ToList();
            Assert.AreEqual(12, data.Count());
            Assert.AreEqual("name=\"ExtractInMergeDataFromRequest\"", data[0]);
            Assert.AreEqual("signature=\"void(object)\"", data[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteXPathWhereNullXmlDataExpectExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            _xPathParser.ExecuteXPath(null, "//");
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteXPathWhereEmptyXmlDataExpectExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            _xPathParser.ExecuteXPath("", "//");
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteXPathWhereNullPathExpectExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            _xPathParser.ExecuteXPath("//", null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteXPathWhereEmptyPathExpectExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            _xPathParser.ExecuteXPath("//", "");
            //------------Assert Results-------------------------
        }
    }
}
