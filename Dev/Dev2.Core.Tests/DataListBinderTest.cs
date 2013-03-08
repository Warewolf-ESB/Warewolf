//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Xml.Linq;
//using System.Linq;
//using System.IO;
//using System.Text.RegularExpressions;
//using System.Text;
//using Dev2;
//using System.Xml;
//using System.Xml.XPath;
//using System.Diagnostics;
//using Moq;
//using Dev2.DataList.Contract;
//using Unlimited.Framework;

//namespace ActivityUnitTests
//{
    
    
//     <summary>
//    This is a test class for ActivityHelperTest and is intended
//    to contain all ActivityHelperTest Unit Tests
//    </summary>
//    [TestClass()]
//    public class ActivityHelperTests {


//        private TestContext testContextInstance;
//        private string _testXMLData;
//        private List<string> _testAmbientDataList;
//        private string _testUIRenderingData;
//        private IDataListBinder binder = new DataListBinder();
//        private Mock<IEsbChannel> _dataChannel = new Mock<IEsbChannel>();

//         <summary>
//        Gets or sets the test context which provides
//        information about and functionality for the current test run.
//        </summary>
//        public TestContext TestContext {
//            get {
//                return testContextInstance;
//            }
//            set {
//                testContextInstance = value;
//            }
//        }

//        #region Additional test attributes
         
//        You can use the following additional attributes as you write your tests:
        
//        Use ClassInitialize to run code before running the first test in the class
//        [ClassInitialize()]
//        public static void MyClassInitialize(TestContext testContext)
//        {
//        }
        
//        Use ClassCleanup to run code after all tests in a class have run
//        [ClassCleanup()]
//        public static void MyClassCleanup()
//        {
//        }
        
//        Use TestInitialize to run code before running each test
//        [TestInitialize()]
//        public void InitializeTests() {
//            _testXMLData = @"
//<Xml>
//  <CountryID>129</CountryID>
//  <Description>South Africa</Description>
//
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
//<WebParts><WebPart><WebPartServiceName>Textbox</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>0</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <xData>
//        <WebServerUrl>http://localhost:1234/services/Textbox.wiz/instances/b6a607f2-07d8-4cf2-bfce-00ee3dd7ad44/bookmarks/7882570a-1c51-4eb4-a094-e04837e4f4b7</WebServerUrl>
//        <XmlData />
//        <Async />
//        <title />
//        <span>Search Term Tag Name</span>
//        <input />
//        <Name>Country</Name>
//        <ValidationClass />
//        <Autocomplete>yes</Autocomplete>
//        <Dev2RowDelimiter>Table</Dev2RowDelimiter>
//        <Dev2BoundServiceName>Get Countries</Dev2BoundServiceName>
//        <AutoCompleteTextProperty>Description</AutoCompleteTextProperty>
//        <RelatedFieldName />
//        <SearchTermTagName>Prefix</SearchTermTagName>
//        <Submit>Submit</Submit>
//      </xData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Textbox</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>0</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <xData>
//        <Service>Textbox.wiz</Service>
//        <XmlData />
//        <Async />
//        <title />
//        <span>Search Term Tag Name</span>
//        <input />
//        <Name>city</Name>
//        <ValidationClass />
//        <Autocomplete>yes</Autocomplete>
//        <Dev2RowDelimiter>Table</Dev2RowDelimiter>
//        <Dev2BoundServiceName>Get Cities</Dev2BoundServiceName>
//        <AutoCompleteTextProperty>City</AutoCompleteTextProperty>
//        <RelatedFieldName>Country</RelatedFieldName>
//        <SearchTermTagName>Prefix</SearchTermTagName>
//        <Submit>Submit</Submit>
//      </xData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart>
//</WebParts>
//<ColsVar>1</ColsVar>
//<RowsVar>0</RowsVar>
//<elementName>firstName</elementName>
//<firstName>sameer</firstName>
//<value><input type=""text"" id=""[[elementName]]"" name=""[[elementName]]"" value=""[[[[elementName]]]]"" /></value>
//    <Value2>
//      <span>labelspansameer</span>
//    </Value2>
//
//</Xml>
//
//";


//            _testUIRenderingData = @"
//<WebPage><WebPageServiceName>Textarea_Wizard</WebPageServiceName><WebParts><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>0</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>0</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>0</RowIndex></WebPart><WebPart><WebPartServiceName>Label</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>2</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Label_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Label_Wizard/instances/b0221c44-b20a-4e7a-aa38-16fd8a57bfea/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementNameLabel>textareaWizardDisplayTextLabel</Dev2elementNameLabel>
//          <Dev2displayTextLabel>Display Text</Dev2displayTextLabel>
//          <requiredValueArray />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Textbox</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>2</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Textbox_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Textbox_Wizard/instances/e1a108b0-6d32-40ff-9a4d-9e2f305a4df9/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementName>Dev2displayTextTextarea</Dev2elementName>
//          <Dev2displayText />
//          <allowEdit>yes</allowEdit>
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>HtmlWidget</WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>2</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <XmlData>
//          <Service>LiteralControl.wiz</Service>
//          <WebServerUrl>http://localhost:1234/services/LiteralControl.wiz/instances/5adae46b-b6a8-403e-a4f7-6597e9b94cdf/bookmarks/a024153c-a383-4a8a-b1e4-8e7637e6fe04</WebServerUrl>
//          <XmlData>
//            <Name />
//            <Value>
//              <span>
//                <script>
//	var isChecked = ""[[showText]]"";
//	if(isChecked.length &gt; 0){
//		document.write('<input type=""checkbox"" name=""showText"" id=""showText"" checked=""true"" /> Show');
//	}else{
//		document.write('<input type=""checkbox"" name=""showText"" id=""showText"" /> Show');
//	}
//  </script>
//              </span>
//            </Value>
//            <cssClass />
//            <Submit>Submit</Submit>
//          </XmlData>
//          <Async />
//        </XmlData>
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Label</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>3</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Label_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Label_Wizard/instances/5bb9675b-05ac-423b-9a73-6000128580c9/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementNameLabel>textareaWizardAllowEditLabel</Dev2elementNameLabel>
//          <Dev2displayTextLabel>Allow User Editing</Dev2displayTextLabel>
//          <requiredValueArray />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>HtmlWidget</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>3</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <XmlData>
//          <WebServerUrl>http://localhost:1234/services/LiteralControl.wiz/instances/80c84c45-26d0-4406-9f51-ad15c1c53760/bookmarks/52280466-8dd0-418b-bfea-6bca045c09ca</WebServerUrl>
//          <XmlData>
//            <Name>allowEditingRegion</Name>
//            <Value>
//              <span>
//                <script>
//  var edit = ""[[allowEdit]]"";
//  
//  if(edit == ""yes""){
//	document.write('<input type=""radio"" name=""allowEdit"" id=""allowEditY"" value=""yes"" checked=""true"" /> Yes ');
//	document.write('<input type=""radio"" name=""allowEdit"" id=""allowEditN"" value=""no"" /> No ');
//  }else{
//	document.write('<input type=""radio"" name=""allowEdit"" id=""allowEditY"" value=""yes"" /> Yes ');
//	document.write('<input type=""radio"" name=""allowEdit"" id=""allowEditN"" value=""no"" checked=""true"" /> No ');
//  }
//  </script>
//              </span>
//            </Value>
//            <cssClass />
//            <Submit>Submit</Submit>
//          </XmlData>
//          <Async />
//        </XmlData>
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Tile 9</WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>3</RowIndex></WebPart><WebPart><WebPartServiceName>HtmlWidget</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>4</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <XmlData>
//          <Service>LiteralControl.wiz</Service>
//          <WebServerUrl>http://localhost:1234/services/LiteralControl.wiz/instances/77a15733-2134-4ef6-b639-3ced64db458b/bookmarks/2efd00da-98d0-40dc-874d-0b0ccdf40ba6</WebServerUrl>
//          <XmlData>
//            <Name>requiredCheckBoxRegion</Name>
//            <Value>
//              <span>
//                <script>
//	var r = ""[[required]]"";
//	if(r.length &gt; 0){
//		document.write('<input type=""checkbox"" name=""required"" id=""required"" value=""on"" checked=""true"" /> Required');
//	}else{
//		document.write('<input type=""checkbox"" name=""required"" id=""required"" value=""on"" /> Required');
//	}
//  </script>
//              </span>
//            </Value>
//            <cssClass />
//            <Submit>Submit</Submit>
//          </XmlData>
//          <Async />
//        </XmlData>
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Tile 11</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>4</RowIndex></WebPart><WebPart><WebPartServiceName>Tile 12</WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>4</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>0</RowIndex></WebPart><WebPart><WebPartServiceName>HelpRegion</WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>2</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>HelpRegion.wiz</Service>
//        <WebServerUrl>http://localhost:1234/services/HelpRegion.wiz/instances/f7dc0e58-1566-4625-bd22-7e92802c4da3/bookmarks/5e92371d-e67f-4bad-bd88-6547ef589b82</WebServerUrl>
//        <XmlData>
//          <helpText>help text for display text</helpText>
//          <Submit>Done</Submit>
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>HelpRegion</WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>3</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>HelpRegion.wiz</Service>
//        <WebServerUrl>http://localhost:1234/services/HelpRegion.wiz/instances/bb61c4aa-8d00-4793-aa45-24745d972c41/bookmarks/8725deab-359d-4fba-acdf-58cf8d17fde9</WebServerUrl>
//        <XmlData>
//          <helpText>help text for allow user editing</helpText>
//          <Submit>Done</Submit>
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Tile 16</WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>4</RowIndex></WebPart><WebPart><WebPartServiceName>Button</WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>0</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Button_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Button_Wizard/instances/1b000df9-d238-4db5-87c6-a3209a632414/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <displayTextButton>Tell Me More</displayTextButton>
//          <btnType>custom</btnType>
//          <customButtonCode>toggleHelp();</customButtonCode>
//          <requiredValueArray />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Tile 18</WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>2</RowIndex></WebPart><WebPart><WebPartServiceName>Tile 19</WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>3</RowIndex></WebPart><WebPart><WebPartServiceName>Tile 20</WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>4</RowIndex></WebPart><WebPart><WebPartServiceName>Button</WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>0</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Button_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Button_Wizard/instances/8094c2c0-d4e2-4784-9e72-20e53cfe00f6/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <displayTextButton>Advanced</displayTextButton>
//          <btnType>custom</btnType>
//          <customButtonCode>toggleAdvanced();</customButtonCode>
//          <requiredValueArray />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Tile 22</WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>2</RowIndex></WebPart><WebPart><WebPartServiceName>Tile 23</WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>3</RowIndex></WebPart><WebPart><WebPartServiceName>Tile 24</WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>4</RowIndex></WebPart><WebPart><WebPartServiceName>Tile 25</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>13</RowIndex></WebPart><WebPart><WebPartServiceName>Tile 26</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>13</RowIndex></WebPart><WebPart><WebPartServiceName>Tile 27</WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>13</RowIndex></WebPart><WebPart><WebPartServiceName>Tile 28</WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>13</RowIndex></WebPart><WebPart><WebPartServiceName>Button</WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>13</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Button_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Button_Wizard/instances/d5ae3a7a-22d6-4c8b-b076-6b5409662957/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <displayTextButton>Cancel</displayTextButton>
//          <btnType>custom</btnType>
//          <customButtonCode>exitWizard();</customButtonCode>
//          <requiredValueArray />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Button</WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>13</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Button_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Button_Wizard/instances/1031734c-87f2-4cd7-be08-119161ea225f/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <displayTextButton>Done</displayTextButton>
//          <btnType>submit</btnType>
//          <requiredValueArray />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Label</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>1</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Label_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Label_Wizard/instances/3e347ce9-dd2a-44ac-aac1-83f17cd6d1ac/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementNameLabel>textareaWizardNameLabel</Dev2elementNameLabel>
//          <Dev2displayTextLabel>Name</Dev2displayTextLabel>
//          <requiredValueArray />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Textbox</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>1</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Textbox_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Textbox_Wizard/instances/c7fc3a31-77ae-42d1-bd9d-8a962288d3b8/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementName>Dev2elementNameTextarea</Dev2elementName>
//          <Dev2displayText />
//          <showText>on</showText>
//          <allowEdit>yes</allowEdit>
//          <required>on</required>
//          <Dev2tabIndex />
//          <Dev2validator>a</Dev2validator>
//          <Dev2toolTip />
//          <Dev2customStyle />
//          <Dev2width />
//          <Dev2height />
//          <Dev2maxChars />
//          <Dev2autoComplete />
//          <Dev2customScript></Dev2customScript>
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>NameRegion</WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>1</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>NameRegion_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/NameRegion_Wizard/instances/c9536970-582f-4f83-92ac-ba7f9016916d/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <namedRegionBoundElement>Dev2elementNameTextarea</namedRegionBoundElement>
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>HelpRegion</WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>1</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>HelpRegion.wiz</Service>
//        <WebServerUrl>http://localhost:1234/services/HelpRegion.wiz/instances/51f8d7c1-6041-4847-99e5-78db22d8adaf/bookmarks/21e71b0f-5aed-48c0-8bdd-5f1989f84a6a</WebServerUrl>
//        <XmlData>
//          <helpText>help text for name</helpText>
//          <Submit>Done</Submit>
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>1</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>1</RowIndex></WebPart><WebPart><WebPartServiceName>Label</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>5</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Label_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Label_Wizard/instances/295f5a8f-8dae-4786-9df1-b6bbd2301d41/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementNameLabel>tabIndexLabel</Dev2elementNameLabel>
//          <Dev2displayTextLabel>Tab Index</Dev2displayTextLabel>
//          <Dev2tabIndex />
//          <Dev2customStyle />
//          <Dev2width />
//          <Dev2width />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Textbox</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>5</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Textbox_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Textbox_Wizard/instances/4baa3965-81a1-4a4e-aa6e-1fb8d90f5aed/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementName>Dev2tabIndex</Dev2elementName>
//          <Dev2displayText />
//          <allowEdit>yes</allowEdit>
//          <Dev2tabIndex />
//          <Dev2validator>a</Dev2validator>
//          <Dev2toolTip />
//          <Dev2customStyle />
//          <Dev2width />
//          <Dev2height />
//          <Dev2maxChars />
//          <Dev2autoComplete />
//          <Dev2customScript></Dev2customScript>
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>5</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>5</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>5</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>5</RowIndex></WebPart><WebPart><WebPartServiceName>Label</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>10</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Label_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Label_Wizard/instances/cbcf6fa2-b96d-485c-aa14-68ca5281d207/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementNameLabel>heightLabel</Dev2elementNameLabel>
//          <Dev2displayTextLabel>Height</Dev2displayTextLabel>
//          <Dev2tabIndex />
//          <Dev2customStyle />
//          <Dev2width />
//          <Dev2width />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Textbox</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>10</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Textbox_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Textbox_Wizard/instances/3458d87c-9bcd-445c-96eb-3cf0e2834f51/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementName>Dev2height</Dev2elementName>
//          <Dev2displayText />
//          <allowEdit>yes</allowEdit>
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>10</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>10</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>10</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>10</RowIndex></WebPart><WebPart><WebPartServiceName>Label</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>9</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Label_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Label_Wizard/instances/66aee700-66e1-48dd-8e3a-fd61e9d8d4f3/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementNameLabel>widthLabel</Dev2elementNameLabel>
//          <Dev2displayTextLabel>Width</Dev2displayTextLabel>
//          <Dev2tabIndex />
//          <Dev2customStyle />
//          <Dev2width />
//          <Dev2width />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Textbox</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>9</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Textbox_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Textbox_Wizard/instances/44181acd-1b12-4bb0-9aef-4e8e804c1b63/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementName>Dev2width</Dev2elementName>
//          <Dev2displayText />
//          <allowEdit>yes</allowEdit>
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>9</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>9</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>9</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>9</RowIndex></WebPart><WebPart><WebPartServiceName>Label</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>6</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Label_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Label_Wizard/instances/e2e63569-2a1a-4f98-94fb-8a0a485258a1/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementNameLabel>validationLabel</Dev2elementNameLabel>
//          <Dev2displayTextLabel>Validation</Dev2displayTextLabel>
//          <Dev2tabIndex />
//          <Dev2customStyle />
//          <Dev2width />
//          <Dev2width />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Drop Down List</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>6</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>DropDownList_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/DropDownList_Wizard/instances/605060de-8c48-48c9-aab9-0313c53413e0/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementNameDropDownList>Dev2validation</Dev2elementNameDropDownList>
//          <Dev2displayTextDropDownList />
//          <allowEdit>yes</allowEdit>
//          <fromService>no</fromService>
//          <listItem>a</listItem>
//          <listItem>b</listItem>
//          <Dev2tabIndex />
//          <Dev2toolTip />
//          <Dev2customStyle />
//          <Dev2width />
//          <Dev2height />
//          <Dev2customScript></Dev2customScript>
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>6</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>6</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>6</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>6</RowIndex></WebPart><WebPart><WebPartServiceName>Label</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>7</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Label_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Label_Wizard/instances/778abe74-cbec-4d9f-be6d-d9011ba2660b/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementNameLabel>tooltipLabel</Dev2elementNameLabel>
//          <Dev2displayTextLabel>Tooltip Text</Dev2displayTextLabel>
//          <Dev2tabIndex />
//          <Dev2customStyle />
//          <Dev2width />
//          <Dev2width />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Textbox</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>7</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Textbox_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Textbox_Wizard/instances/f8ca06c1-bf9a-4e59-967f-aca1d650e66f/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementName>Dev2toolTip</Dev2elementName>
//          <Dev2displayText />
//          <allowEdit>yes</allowEdit>
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>7</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>7</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>7</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>7</RowIndex></WebPart><WebPart><WebPartServiceName>Label</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>8</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Label_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Label_Wizard/instances/671cbce8-4ef4-4bfe-9a21-02c7090fc6d2/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementNameLabel>customStyleLabel</Dev2elementNameLabel>
//          <Dev2displayTextLabel>Custom  Styling</Dev2displayTextLabel>
//          <Dev2tabIndex />
//          <Dev2customStyle />
//          <Dev2width />
//          <Dev2width />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>Textbox</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>8</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Textbox_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Textbox_Wizard/instances/d8abef1b-7ec9-47e7-ba35-466ce089cdc1/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementName>Dev2customStyle</Dev2elementName>
//          <Dev2displayText />
//          <allowEdit>yes</allowEdit>
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>8</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>8</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>8</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>8</RowIndex></WebPart><WebPart><WebPartServiceName>Label</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>12</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Label_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Label_Wizard/instances/5d751237-e957-4d1d-bacf-26f627a1148d/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementNameLabel>customScriptLabel</Dev2elementNameLabel>
//          <Dev2displayTextLabel>Custom Script</Dev2displayTextLabel>
//          <Dev2tabIndex />
//          <Dev2customStyle />
//          <Dev2width />
//          <Dev2width />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>HtmlWidget</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>12</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <XmlData>
//          <Service>LiteralControl.wiz</Service>
//          <WebServerUrl>http://localhost:1234/services/LiteralControl.wiz/instances/8251a5ee-cf6c-4e2f-a628-722c1b9f3b00/bookmarks/e44478df-b5f4-4c9b-8364-bd4b09a602e1</WebServerUrl>
//          <XmlData>
//            <Name />
//            <Value>
//              <span>
//                <textarea id=""Dev2customScript"" name=""Dev2customScript"" cols=""30"" rows=""7"">[[Dev2customScript]]
//</textarea>
//              </span>
//            </Value>
//            <cssClass />
//            <Submit>Submit</Submit>
//          </XmlData>
//          <Async />
//        </XmlData>
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>12</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>12</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>12</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>12</RowIndex></WebPart><WebPart><WebPartServiceName>Label</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>11</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <Service>Label_Wizard</Service>
//        <WebServerUrl>http://localhost:1234/services/Label_Wizard/instances/17fa7494-fcac-48cd-9860-ff0d012763b2/bookmarks/dsfResumption</WebServerUrl>
//        <XmlData>
//          <Dev2elementNameLabel>wrapTextLabel</Dev2elementNameLabel>
//          <Dev2displayTextLabel>Wrap Text</Dev2displayTextLabel>
//          <Dev2tabIndex />
//          <Dev2customStyle />
//          <Dev2width />
//          <Dev2width />
//        </XmlData>
//        <Async />
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName>HtmlWidget</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>11</RowIndex><Dev2XMLResult>
//  <sr>
//    <sr>
//      <XmlData>
//        <XmlData>
//          <WebServerUrl>http://localhost:1234/services/LiteralControl.wiz/instances/f86f75c4-c1da-4853-bb2c-3aa3f2fe5845/bookmarks/485b2d7e-1757-478d-98dc-3e1d96422b6a</WebServerUrl>
//          <XmlData>
//            <Name />
//            <Value>
//              <span>
//                <script>
//  var edit = ""[[wrapText]]"";
//  
//  if(edit == ""yes""){
//	document.write('<input type=""radio"" name=""wrapText"" id=""wrapY"" value=""yes"" checked=""true"" /> Yes ');
//	document.write('<input type=""radio"" name=""wrapText"" id=""wrapN"" value=""no"" /> No ');
//  }else{
//	document.write('<input type=""radio"" name=""wrapText"" id=""wrapY"" value=""yes"" /> Yes ');
//        document.write('<input type=""radio"" name=""wrapText"" id=""wrapN"" value=""no"" checked=""true"" /> No ');
//}
//</script>
//              </span>
//            </Value>
//            <cssClass />
//            <Submit>Submit</Submit>
//          </XmlData>
//          <Async />
//        </XmlData>
//      </XmlData>
//    </sr>
//  </sr>
//</Dev2XMLResult></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>2</ColumnIndex><RowIndex>11</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>3</ColumnIndex><RowIndex>11</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>4</ColumnIndex><RowIndex>11</RowIndex></WebPart><WebPart><WebPartServiceName></WebPartServiceName><ColumnIndex>5</ColumnIndex><RowIndex>11</RowIndex></WebPart><Rows>14</Rows><Cols>6</Cols></WebParts></WebPage>
//";

//            _testAmbientDataList = new List<string>() { _testXMLData };

//        }
        
//        Use TestCleanup to run code after each test has run
//        [TestCleanup()]
//        public void MyTestCleanup()
//        {
//        }
        
//        #endregion

//        [TestMethod()]
//        public void DataRegion_Self_Bind() {
//            string XmlData = "<ParentServiceName>asdfasd</ParentServiceName>";
//            string _parentServiceName = string.Empty;


//            if (!string.IsNullOrEmpty(XmlData)) {
//                Stopwatch s = new Stopwatch();

//                s.Start();
//                dynamic v = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(XmlData);
//                var te = XElement.Parse(XmlData);
//                s.Stop();

//                var r = s.ElapsedMilliseconds;

//                s.Reset();


//                s.Start();
//                if (dataObject.ParentServiceName is string) {
//                    _parentServiceName = dataObject.ParentServiceName;
//                }
//                s.Stop();

//                r = s.ElapsedMilliseconds;
//            }


//        }


//        [TestMethod()]
//        public void DataRegion_With_Data_Binding_Expression_Evaluates() {
//            string transform = "[[value]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform, "", true);
//            Assert.AreEqual(@"<value>
//  <input type=""text"" id=""firstName"" name=""firstName"" value=""sameer"" />
//</value>", result);
//        }

        
//        [TestMethod()]
//        public void DataRegion_With_IsolatedSquareBracket_Reads_As_Literal() {
//            string transform = "[[xpath('//WebPart[RowIndex=[[RowsVar]] and ColumnIndex=[[ColsVar]]]')]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);

//        }

//        [TestMethod()]
//        public void DataRegion_Tag_Only_Works() {
//            string transform = "[[rowdelimiter]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual("Table", result);
//        }

//        [TestMethod()]
//        public void DataRegion_Side_By_Side() {
//            string transform = "\"[[boundfield]]\":\"[[[[boundfield]]]]\"";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual("\"Description\":\"South Africa\"", result);
//        }

//        [TestMethod()]
//        public void DataRegion_Tag_Only_Invalid_Tokens_Returns_Unparsed_DataRegion() {
//            string transform = "[[rowdelimiter?!@#$?!@%ASDF]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual(transform, result);
//        }

//        [TestMethod()]
//        public void DataRegion_Tag_Only_With_Embedded_Extra_Close_Delimiter_Returns_Data_Concatenated_To_Close_Delimiter() {
//            string transform = "[[rowdelimiter]]]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual("Table]]", result);
//        }

//        [TestMethod()]
//        public void DataRegion_Tag_Only_With_Embedded_Open_Delimiter_Returns_Syntax_Error() {
//            string transform = "[[rowdelimiter[[]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual("[[rowdelimiter", result);
//        }

//        [TestMethod()]
//        public void DataRegion_Nested_Tag_Works() {
//            string transform = "[[[[field]]]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual("129", result);
//        }

//        [TestMethod()]
//        public void DataRegion_Nested_Tag_With_Invalid_Tokens_Returns_Valid_Result_Concatenated_With_Invalid_Tokens() {
//            string transform = "[[[[field]]!@#$ASDF]]";
//            string result = binder.TextAndJScriptRegionEvaluator(_testAmbientDataList, transform);
//            Assert.AreEqual("[[CountryID!@#$ASDF]]", result);
//        }


//        [TestMethod()]
//        public void UI_Rendering_Perf_Text() {
//            var test = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(_testUIRenderingData);
//            test.RowsVar = 10;
//            test.ColsVar = 3;

//            string transform = "[[xpath('//WebPart[RowIndex=[[RowsVar]] and ColumnIndex=[[ColsVar]]]')]]";

//            Stopwatch s = new Stopwatch();

//            s.Start();
//            string result = binder.TextAndJScriptRegionEvaluator(new List<string> { test.XmlString }, transform);
//            s.Stop();

//            var res = s.ElapsedMilliseconds;
//            s.Reset();


//            s.Start();
//            dynamic xpathResult = test.XPath("//WebPart[RowIndex=1 and ColumnIndex=1]");
//            s.Stop();
//            s.Reset();

//            res = s.ElapsedMilliseconds;

//        }


//        [TestMethod]
//        public void RecordSetExtraction(){
//             AllRows() - Good
//             RowNumber(x) - Good
//             FirstRows(x) - Good
//             LastRows(x) -  
//            string testPart = "[[cars(1).reg]]";
//            string DL = "<ADL><cars><reg>abc</reg></cars><cars><reg>def</reg></cars><cars><reg>ghi</reg></cars><cars><reg>xyz</reg></cars></ADL>";
//            DataListBinder dlb = new DataListBinder();
//            List<string> fakeDL = new List<string>(){ DL };
//            string result = dlb.TextAndJScriptRegionEvaluator(fakeDL, testPart);

//            Console.WriteLine("Result : " + result);
            
//        }

//        /*
//         * Data List Testing : 
//         * 
//            * Recordset.AllRows()
//            * Recordset.RowNumber(x)
//            * Recordset.RowNumber(x).FieldName
//            * Recordset.First(x)
//            * Recordset.First(x).FieldName
//            * Recordset.Last(x)
//            * Recordset.Last(x).FieldName
//            * 
//            * 
//            */ 


//        #region Lexical Scanner Test Code
//         <summary>
//         This method was built to test a lexical scanner in order to process
//         a data region. We are using Regex as a basis to do this.
//         </summary>
//        [TestMethod()]
//        public void ParseDataRegionTokens() {
//            string assignData = "sertwertgdsfgsdg[[CountryID]][[xpath('//[[[[test]]]]')]]";

//            StringReader s = new StringReader(assignData);

//            int intChar;
//            char convertedChar;
//            char prvChar = ' ';
//            char nxtChar = ' ';
//            List<DataRegion> regions = new List<DataRegion>();
//            StringBuilder regionData = new StringBuilder();
//            DataRegion lastDataRegion = null;

//            while (s.Peek() != -1) {


//                var lastRegionList = regions.Where(c => c.IsOpen);
//                if (lastRegionList.Count() > 0) {
//                    lastDataRegion = lastRegionList.Last();
//                }
//                else {
//                    lastDataRegion = null;
//                }
                
//                char ch = (char)s.Peek();


                
//                if (char.IsWhiteSpace(ch)) {
//                    s.Read();
//                }

//                if (ch == '[') {
//                    s.Read();

//                    ch = (char)s.Peek();

//                    if (ch == '[') {
//                        Region Starting
//                        DataRegion d = new DataRegion { IsOpen = true };
//                        if (lastDataRegion != null) {
//                            d.Parent = lastDataRegion;
//                            d.Parent.Child = d;
//                            lastDataRegion.RegionData.Append('$');
//                        }

//                        regions.Add(d);
//                        d.RegionData.Append(new char[] { ch, ch });
//                        s.Read();
//                    }

//                }
//                else {
//                    if (ch == ']') {
//                        s.Read();

//                        ch = (char)s.Peek();

//                        if (ch == ']') {
//                            Region Closing
//                            lastDataRegion.RegionData.Append(new char[] { ch, ch });

//                            lastDataRegion.IsOpen = false;

//                            string regionString = lastDataRegion.RegionData.ToString();

//                            s.Read();
//                        }
//                        else {
//                            if (lastDataRegion != null) {
//                                lastDataRegion.RegionData.Append(ch);

//                            }
//                            else {
//                               This is just literal text
//                            }
//                            s.Read();
//                        }

//                    }
//                    else {
//                        This data needs to be appended to the last open region
//                        if (lastDataRegion != null) {
//                            lastDataRegion.RegionData.Append(ch);
//                        }
//                        else {
//                            This is just literal text
//                        }
//                        s.Read();
//                    }
//                }
//            }

//            regions.ForEach(c => {
//                GetInnermostChild(c, _testXMLData);
//            });


//            Get all root level items;

//            var roots = regions.Where(c => c.IsRootLevel);
//            if (roots.Count() > 0) {
//                roots
//                    .OrderByDescending(a=> a.RootLevelToken.Length)
//                    .ToList()
//                    .ForEach(d => assignData = assignData.Replace(d.RootLevelToken, d.Value));
//            }
//        }



//        public void GetInnermostChild(DataRegion region, string dataSource) {
//            if (region.IsTokenGenerated) {
//                return;
//            }

//            var current = region;

//            if (!current.HasChild && !current.HasParent) {
//                current.RootLevelToken = current.RegionData.ToString();
//                current.Value = binder.RegionParser(dataSource, current.RootLevelToken);
//                current.IsTokenGenerated = true;
//                current.IsRootLevel = true;
//                return;
//            }

//            while (true) {
//                if (current.Child == null) break;
//                current = current.Child;
//            }

//            current is now the child at the bottom of the tree
//            if (!current.HasChild) {
//                current.RootLevelToken = current.RegionData.ToString();
//                current.Value = binder.RegionParser(dataSource, current.RootLevelToken);
//                current.IsTokenGenerated = true;
//            }
            
//            while (current.Parent != null && !region.IsTokenGenerated) {
//                string parentData = current.Parent.RegionData.ToString();
//                string myData = current.RootLevelToken;
//                string myDataValue = current.Value;

//                current.Parent.RootLevelToken = parentData.Replace("$", myData);
//                current.Parent.Value = parentData.Replace("$", myDataValue);
//                current.Parent.IsTokenGenerated = true;
//                current.Parent.Value = binder.RegionParser(dataSource, current.Parent.Value);

//                current = current.Parent;
//                if (current.Parent == null) {
//                    current.IsRootLevel = true;
//                }

//            }


//        }



//        #endregion

//        #region XPath Expression Code

//        private string XPath_Test(string data, string tags)
//        {
//            XmlDocument xdoc = new XmlDocument();
//            xdoc.LoadXml(data);
//            XPathNavigator docnav = xdoc.CreateNavigator();
//            XPathNodeIterator iterator = docnav.Select(tags);
//            string xpathExpression = "sum(" + tags + ")";
//            double total = (double)docnav.Evaluate(xpathExpression);
//            string xpathdata = null;

//            while (iterator.MoveNext())
//            {
//                xpathdata = iterator.Current.OuterXml.ToString();
//            }

//            return xpathdata;
//        }

//        #endregion
//    }
//}
