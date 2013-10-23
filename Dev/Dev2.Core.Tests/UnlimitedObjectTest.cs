using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Unlimited.Framework;
using Match = System.Text.RegularExpressions.Match;

namespace Dev2.Tests
{
    /// <summary>
    ///     This is a test class for UnlimitedObjectTest and is intended
    ///     to contain all UnlimitedObjectTest Unit Tests
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class UnlimitedObjectTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //

        #endregion
        // ReSharper disable InconsistentNaming

        [TestMethod]
        public void TestSerialization()
        {
            //--------------------Setup ----------------------------------------
            var item = new Test { FirstName = "Sameer", LastName = "Chunilall" };
            //-------------------Execute----------------------------------------
            var data = new UnlimitedObject(item).XmlString;
            //--------------------Assert-----------------------------------------
            Assert.AreEqual("<Test>  <FirstName>Sameer</FirstName>  <LastName>Chunilall</LastName></Test>", data.Replace(Environment.NewLine, ""));
        }
        
        [TestMethod]
        public void Test_DeleteElementByTagName_RemovesElement()
        {
            const string del = @"
<Dev2ServiceInput3>
  <XmlData>
    <XmlData>
      <XmlData>
        <Dev2ServiceInput>
          <x>
            <CurrentWebPart>
              <WebPart>
                <WebPartServiceName>Label</WebPartServiceName>
                <ColumnIndex>0</ColumnIndex>
                <RowIndex>0</RowIndex>
                <Dev2XMLResult>
                  <sr>
                    <sr>
                      <xData>
                        <WebServerUrl>http://rsaklfsRTvrgendev:2234/services/LiteralControl.wiz/instances/79c5b234-65aa-4504-a99d-bfece8574fcf/bookmarks/576e3dc7-69ff-4700-8a6b-0168b7d08082</WebServerUrl>
                        <XmlData />
                        <Async />
                        <title />
                        <span>Value</span>
                        <input />
                        <Name>displayTextLabel</Name>
                        <Value>Display Text</Value>
                        <Submit>Submit</Submit>
                      </xData>
                    </sr>
                  </sr>
                </Dev2XMLResult>
              </WebPart>
            </CurrentWebPart>
          </x>
        </Dev2ServiceInput>
        <Error>No Value Set</Error>
      </XmlData>
    </XmlData>
  </XmlData>
  <Resumption>
    <ParentWorkflowInstanceId>70bd64d8-bb99-4c62-9ad6-8109d5800efa</ParentWorkflowInstanceId>
    <ParentServiceName></ParentServiceName>
  </Resumption>
  <Service>ReplacePartWithErrorMsg</Service>
  <DynamicServiceFrameworkMessage>
    <Error>Error in request - inbound message contains error tag</Error>
  </DynamicServiceFrameworkMessage>
</Dev2ServiceInput3>
";

            UnlimitedObject d = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(del);
            d.RemoveElementsByTagName("Dev2ServiceInput");
            var dev2ServiceInput = d.GetElement("Dev2ServiceInput") as UnlimitedObject;
            Assert.IsNotNull(dev2ServiceInput);
            Assert.IsFalse(dev2ServiceInput.xmlData.HasElements);
            Assert.IsTrue(dev2ServiceInput.xmlData.IsEmpty);
        }

        // ReSharper disable ObjectCreationAsStatement
        /// <summary>
        ///     A test for UnlimitedObject Constructor
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnlimitedObjectConstructorTest_EmptyString_ThrowsArgumentNullException()
        {
            var xml = string.Empty;
            new UnlimitedObject(xml);
        }

        /// <summary>
        ///     A test for UnlimitedObject Constructor
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnlimitedObjectConstructorTest_NullString_ThrowsArgumentNullException()
        {
            new UnlimitedObject((string)null);
        }

        /// <summary>
        ///     A test for UnlimitedObject Constructor
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnlimitedObjectConstructorTest_NullObject_ThrowsArgumentNullException()
        {
            new UnlimitedObject((object)null);
        }

        /// <summary>
        ///     A test for UnlimitedObject Constructor
        /// </summary>
        [TestMethod]
        public void UnlimitedObjectConstructorTest_EmptyConstructor_CreatesObject()
        {
            var target = new UnlimitedObject();
            Assert.IsNotNull(target);
            Assert.AreEqual(enObjectState.NEW,target.ObjectState);
            Assert.AreEqual("<XmlData />",target.xmlData.ToString());
        }

        /// <summary>
        ///     A test for UnlimitedObject Constructor
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnlimitedObjectConstructorTest_NullXElement_ThrowsArgumentException()
        {
            new UnlimitedObject((XElement)null);
        }
        // ReSharper restore ObjectCreationAsStatement
        /// <summary>
        ///     A test for ElementExists
        /// </summary>
        [TestMethod]
        public void ElementExistsTest_WhenHasElement_ReturnsTrue()
        {
            const string xml = @"
<Dev2ServiceInput3>
  <XmlData>
    <XmlData>
      <XmlData>
        <Dev2ServiceInput>
          <x>
            <CurrentWebPart>
              <WebPart>
                <WebPartServiceName>Label</WebPartServiceName>
                <ColumnIndex>0</ColumnIndex>
                <RowIndex>0</RowIndex>
                <Dev2XMLResult>
                  <sr>
                    <sr>
                      <xData>
                        <WebServerUrl>http://rsaklfsRTvrgendev:2234/services/LiteralControl.wiz/instances/79c5b234-65aa-4504-a99d-bfece8574fcf/bookmarks/576e3dc7-69ff-4700-8a6b-0168b7d08082</WebServerUrl>
                        <XmlData />
                        <Async />
                        <title />
                        <span>Value</span>
                        <input />
                        <Name>displayTextLabel</Name>
                        <Value>Display Text</Value>
                        <Submit>Submit</Submit>
                      </xData>
                    </sr>
                  </sr>
                </Dev2XMLResult>
              </WebPart>
            </CurrentWebPart>
          </x>
        </Dev2ServiceInput>
        <Error>No Value Set</Error>
      </XmlData>
    </XmlData>
  </XmlData>
  <Resumption>
    <ParentWorkflowInstanceId>70bd64d8-bb99-4c62-9ad6-8109d5800efa</ParentWorkflowInstanceId>
    <ParentServiceName></ParentServiceName>
  </Resumption>
  <Service>ReplacePartWithErrorMsg</Service>
  <DynamicServiceFrameworkMessage>
    <Error>Error in request - inbound message contains error tag</Error>
  </DynamicServiceFrameworkMessage>
</Dev2ServiceInput3>
";
            UnlimitedObject target = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(xml);
            const string elementName = "Dev2ServiceInput";
            var actual = target.ElementExists(elementName);
            Assert.IsTrue(actual);
        }
        
        /// <summary>
        ///     A test for ElementExists
        /// </summary>
        [TestMethod]
        public void ElementExistsTest_WhenElementDoesNotExist_ReturnsFalse()
        {
            const string xml = @"
<Dev2ServiceInput3>
  <XmlData>
    <XmlData>
      <XmlData>
        <Dev2ServiceInput>
          <x>
            <CurrentWebPart>
              <WebPart>
                <WebPartServiceName>Label</WebPartServiceName>
                <ColumnIndex>0</ColumnIndex>
                <RowIndex>0</RowIndex>
                <Dev2XMLResult>
                  <sr>
                    <sr>
                      <xData>
                        <WebServerUrl>http://rsaklfsRTvrgendev:2234/services/LiteralControl.wiz/instances/79c5b234-65aa-4504-a99d-bfece8574fcf/bookmarks/576e3dc7-69ff-4700-8a6b-0168b7d08082</WebServerUrl>
                        <XmlData />
                        <Async />
                        <title />
                        <span>Value</span>
                        <input />
                        <Name>displayTextLabel</Name>
                        <Value>Display Text</Value>
                        <Submit>Submit</Submit>
                      </xData>
                    </sr>
                  </sr>
                </Dev2XMLResult>
              </WebPart>
            </CurrentWebPart>
          </x>
        </Dev2ServiceInput>
        <Error>No Value Set</Error>
      </XmlData>
    </XmlData>
  </XmlData>
  <Resumption>
    <ParentWorkflowInstanceId>70bd64d8-bb99-4c62-9ad6-8109d5800efa</ParentWorkflowInstanceId>
    <ParentServiceName></ParentServiceName>
  </Resumption>
  <Service>ReplacePartWithErrorMsg</Service>
  <DynamicServiceFrameworkMessage>
    <Error>Error in request - inbound message contains error tag</Error>
  </DynamicServiceFrameworkMessage>
</Dev2ServiceInput3>
";
            UnlimitedObject target = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(xml);
            const string elementName = "Hello";
            var actual = target.ElementExists(elementName);
            Assert.IsFalse(actual);
        }

        /// <summary>
        ///     A test for GenerateServiceRequest
        /// </summary>
        [TestMethod]
        public void GenerateServiceRequest_NoParentRequest()
        {
            const string serviceName = "resumeTest";
            var dataSources = new List<string>
            {
                @"<XmlData>
  <XmlData>
    <InputData>
      <Dev2BoundServiceName>resumetest</Dev2BoundServiceName>
      <Dev2RowDelimiter>Pet</Dev2RowDelimiter>
      <Dev2BoundFields>
        <Dev2Field>id</Dev2Field>
        <Dev2Field>type</Dev2Field>
      </Dev2BoundFields>
      <callback>__embeddedFn__</callback>
    </InputData>
  </XmlData>
</XmlData>",
                @"<Resumption><ParentWorkflowInstanceId>5b230ddc-ff62-4f31-9e7d-09c1a9178c74</ParentWorkflowInstanceId><ParentServiceName>JSON Binder</ParentServiceName></Resumption>"
            };
            
            const string expected = "<Dev2ServiceInput>\r\n  <XmlData>\r\n    <XmlData>\r\n      <InputData>\r\n        <Dev2BoundServiceName>resumetest</Dev2BoundServiceName>\r\n        <Dev2RowDelimiter>Pet</Dev2RowDelimiter>\r\n        <Dev2BoundFields>\r\n          <Dev2Field>id</Dev2Field>\r\n          <Dev2Field>type</Dev2Field>\r\n        </Dev2BoundFields>\r\n        <callback>__embeddedFn__</callback>\r\n      </InputData>\r\n    </XmlData>\r\n  </XmlData>\r\n  <Resumption>\r\n    <ParentWorkflowInstanceId>5b230ddc-ff62-4f31-9e7d-09c1a9178c74</ParentWorkflowInstanceId>\r\n    <ParentServiceName>JSON Binder</ParentServiceName>\r\n  </Resumption>\r\n  <Service>resumeTest</Service>\r\n</Dev2ServiceInput>";
            var actual = new UnlimitedObject().GenerateServiceRequest(serviceName, null, dataSources, null);
            Assert.AreEqual(expected,actual);
        }


        /// <summary>
        ///     A test for GetStringXmlDataAsUnlimitedObject
        /// </summary>
        [TestMethod]
        public void GetStringXmlDataAsUnlimitedObjectTest()
        {
            const string xml = @"
<Dev2ServiceInput3>
  <XmlData>
    <XmlData>
      <XmlData>
        <Dev2ServiceInput>
          <x>
            <CurrentWebPart>
              <WebPart>
                <WebPartServiceName>Label</WebPartServiceName>
                <ColumnIndex>0</ColumnIndex>
                <RowIndex>0</RowIndex>
                <Dev2XMLResult>
                  <sr>
                    <sr>
                      <xData>
                        <WebServerUrl>http://rsaklfsRTvrgendev:2234/services/LiteralControl.wiz/instances/79c5b234-65aa-4504-a99d-bfece8574fcf/bookmarks/576e3dc7-69ff-4700-8a6b-0168b7d08082</WebServerUrl>
                        <XmlData />
                        <Async />
                        <title />
                        <span>Value</span>
                        <input />
                        <Name>displayTextLabel</Name>
                        <Value>Display Text</Value>
                        <Submit>Submit</Submit>
                      </xData>
                    </sr>
                  </sr>
                </Dev2XMLResult>
              </WebPart>
            </CurrentWebPart>
          </x>
        </Dev2ServiceInput>
        <Error>No Value Set</Error>
      </XmlData>
    </XmlData>
  </XmlData>
  <Resumption>
    <ParentWorkflowInstanceId>70bd64d8-bb99-4c62-9ad6-8109d5800efa</ParentWorkflowInstanceId>
    <ParentServiceName></ParentServiceName>
  </Resumption>
  <Service>ReplacePartWithErrorMsg</Service>
  <DynamicServiceFrameworkMessage>
    <Error>Error in request - inbound message contains error tag</Error>
  </DynamicServiceFrameworkMessage>
</Dev2ServiceInput3>
";
            UnlimitedObject actual = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(xml);
            Assert.IsNotNull(actual);
            Assert.AreEqual("Dev2ServiceInput3", actual.RootName);
        }

        /// <summary>
        ///     A test for GetValue
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetValueTest_WithEmptyString_ThrowsException()
        {
            var target = new UnlimitedObject();
            var Name = string.Empty;
            var actual = target.GetValue(Name);
            Assert.IsNull(actual);
        }        
        
        /// <summary>
        ///     A test for GetValue
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetValueTest_WithNull_ThrowsException()
        {
            var target = new UnlimitedObject();
            var actual = target.GetValue(null);
            Assert.IsNull(actual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObject_GetValue_HasAttribute_ReturnsAttributeValue()
        {
            //------------Setup for test--------------------------
            var actual = GetUnlimitedObject();
            //------------Execute Test---------------------------
            var value = actual.GetValue("ID");
            //------------Assert Results-------------------------
            Assert.AreEqual("TestID",value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObject_GetValue_FavoursAttributeOverNode_ReturnsAttributeValue()
        {
            //------------Setup for test--------------------------
            var actual = GetUnlimitedObject();
            //------------Execute Test---------------------------
            var value = actual.GetValue("ID");
            //------------Assert Results-------------------------
            Assert.AreEqual("TestID",value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObject_GetValue_HasNodeInDataListAndXMLData_ReturnsNonDataListNode()
        {
            //------------Setup for test--------------------------
            var actual = GetUnlimitedObject();
            //------------Execute Test---------------------------
            var value = actual.GetValue("DisplayName");
            //------------Assert Results-------------------------
            Assert.AreEqual("TesstValues", value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObject_GetValue_DataListRequested_ReturnsDataListNode()
        {
            //------------Setup for test--------------------------
            var actual = GetUnlimitedObject();
            //------------Execute Test---------------------------
            var value = actual.GetValue("DataList");
            //------------Assert Results-------------------------
            Assert.AreEqual("<DataList>\r\n  <ID Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <ServerID Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <ResourceType Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <Version Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <IsValid Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <ErrorMessages Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <DisplayName Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <DataList Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n</DataList>", value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObject_GetValue_GreaterThanOneElement_ReturnsXmlString()
        {
            //------------Setup for test--------------------------
            var actual = GetUnlimitedObject();
            //------------Execute Test---------------------------
            var value = actual.GetValue("Test");
            //------------Assert Results-------------------------
            Assert.AreEqual("<XmlData>\r\n  <Test>Tesst</Test>\r\n  <Test World=\"Hi\">Tesst1</Test>\r\n</XmlData>", value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObject_GetValue_ElementNotFound_ReturnsEmptyString()
        {
            //------------Setup for test--------------------------
            var actual = GetUnlimitedObject();
            //------------Execute Test---------------------------
            var value = actual.GetValue("Hello");
            //------------Assert Results-------------------------
            Assert.AreEqual("", value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObject_GetValue_ElementHasAttribute_ReturnsXmlString()
        {
            //------------Setup for test--------------------------
            var actual = GetUnlimitedObject();
            //------------Execute Test---------------------------
            var value = actual.GetValue("World");
            //------------Assert Results-------------------------
            Assert.AreEqual("Hi", value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObject_DynamicPropery_ReturnsNonDataListValues()
        {
            //------------Setup for test--------------------------
            dynamic actual = GetUnlimitedObject();
            //------------Execute Test---------------------------
            var value = actual.DisplayName;
            //------------Assert Results-------------------------
            Assert.AreEqual("TesstValues", value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObject_DynamicPropery_FavoursAttribute_ReturnsNonDataListValues()
        {
            //------------Setup for test--------------------------
            dynamic actual = GetUnlimitedObject();
            //------------Execute Test---------------------------
            var value = actual.ID;
            //------------Assert Results-------------------------
            Assert.AreEqual("TestID", value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObject_DynamicPropery_DataList_ReturnsDataListNode()
        {
            //------------Setup for test--------------------------
            dynamic actual = GetUnlimitedObject();
            //------------Execute Test---------------------------
            var value = actual.DataList as UnlimitedObject;
            //------------Assert Results-------------------------
            Assert.IsNotNull(value);
            Assert.AreEqual("<DataList>\r\n  <ID Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <ServerID Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <ResourceType Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <Version Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <IsValid Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <ErrorMessages Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <DisplayName Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n  <DataList Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />\r\n</DataList>", value.XmlString);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObjectService_GetValue_HasNodeInDataListAndXMLData_Inputs_ReturnsNonDataListNode()
        {
            //------------Setup for test--------------------------
            var actual = GetUnlimitedObjectForService();
            //------------Execute Test---------------------------
            var value = actual.GetValue("Inputs");
            //------------Assert Results-------------------------
            Assert.AreEqual("<Inputs>\r\n  <Input Name=\"CityName\" Source=\"CityName\" EmptyToNull=\"false\" DefaultValue=\"Nice\" />\r\n  <Input Name=\"CountryName\" Source=\"CountryName\" EmptyToNull=\"false\" DefaultValue=\"France\" />\r\n</Inputs>", value);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObjectService_GetValue_HasNodeInDataListAndXMLData_Input_ReturnsNonDataListNode()
        {
            //------------Setup for test--------------------------
            var actual = GetUnlimitedObjectForService();
            //------------Execute Test---------------------------
            var value = actual.GetValue("Input");
            //------------Assert Results-------------------------
            Assert.AreEqual("<XmlData>\r\n  <Input Name=\"CityName\" Source=\"CityName\" EmptyToNull=\"false\" DefaultValue=\"Nice\" />\r\n  <Input Name=\"CountryName\" Source=\"CountryName\" EmptyToNull=\"false\" DefaultValue=\"France\" />\r\n</XmlData>", value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObjectService_GetValue_HasNodeInDataListAndXMLData_Outputs_ReturnsNonDataListNode()
        {
            //------------Setup for test--------------------------
            var actual = GetUnlimitedObjectForService();
            //------------Execute Test---------------------------
            var value = actual.GetValue("Outputs");
            //------------Assert Results-------------------------
            Assert.AreEqual("<Outputs>\r\n  <Output Name=\"Location\" MapsTo=\"Location\" Value=\"[[Location]]\" Recordset=\"\" />\r\n  <Output Name=\"Time\" MapsTo=\"Time\" Value=\"[[Time]]\" Recordset=\"\" />\r\n</Outputs>", value);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("UnlimitedObject_GetValue")]
        public void UnlimitedObjectService_GetValue_HasNodeInDataListAndXMLData_Output_ReturnsNonDataListNode()
        {
            //------------Setup for test--------------------------
            var actual = GetUnlimitedObjectForService();
            //------------Execute Test---------------------------
            var value = actual.GetValue("Output");
            //------------Assert Results-------------------------
            Assert.AreEqual("<XmlData>\r\n  <Output Name=\"Location\" MapsTo=\"Location\" Value=\"[[Location]]\" Recordset=\"\" />\r\n  <Output Name=\"Time\" MapsTo=\"Time\" Value=\"[[Time]]\" Recordset=\"\" />\r\n</XmlData>", value);
        }

        static UnlimitedObject GetUnlimitedObject()
        {
            const string xml = @"
        <Dev2ServiceInput ID=""TestID"">
            <DisplayName>TesstValues</DisplayName>
            <Test>Tesst</Test>
            <Test World=""Hi"">Tesst1</Test>
            <DataList>
		       <ID Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
		       <ServerID Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
		       <ResourceType Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
		       <Version Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
		       <IsValid Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
		       <ErrorMessages Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
	    	   <Name Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    		   <DisplayName Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
		       <DataList Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
	        </DataList>
        </Dev2ServiceInput>
";
            UnlimitedObject actual = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(xml);
            return actual;
        }
        
        static UnlimitedObject GetUnlimitedObjectForService()
        {
            const string xml = @"
        <Dev2ServiceInput ID=""TestID"">
            <DisplayName>TesstValues</DisplayName>
            <Test>Tesst</Test>
            <Test World=""Hi"">Tesst1</Test>
           <Inputs>
				<Input Name=""CityName"" Source=""CityName"" EmptyToNull=""false"" DefaultValue=""Nice"" />
				<Input Name=""CountryName"" Source=""CountryName"" EmptyToNull=""false"" DefaultValue=""France"" />
			</Inputs>
			<Outputs>
				<Output Name=""Location"" MapsTo=""Location"" Value=""[[Location]]"" Recordset="""" />
				<Output Name=""Time"" MapsTo=""Time"" Value=""[[Time]]"" Recordset="""" />
			</Outputs>
            <DataList>
		       <Inputs Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
		       <Input Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
		       <Output Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
		       <Outputs Description="""" IsEditable=""True"" ColumnIODirection=""None"" />		       
	        </DataList>
        </Dev2ServiceInput>
";
            UnlimitedObject actual = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(xml);
            return actual;
        }
        
        internal class Test
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}