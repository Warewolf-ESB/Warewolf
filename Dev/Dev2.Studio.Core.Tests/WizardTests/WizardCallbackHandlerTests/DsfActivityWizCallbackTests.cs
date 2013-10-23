using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Wizards;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Studio.Core.Wizards.CallBackHandlers;
using System.Activities.Presentation.Model;
using Dev2.DataList.Contract;
using Dev2.Core.Tests.Utils;
using Dev2.Common;
using System.Xml.Linq;

namespace Dev2.Core.Tests.WizardTests.WizardCallbackHandlerTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class DsfActivityWizCallbackTests
    {
        private TestContext testContextInstance;

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
            //testAct = null;//new ModelItem();
            //testAct.InputMapping = "";
            //testAct.OutputMapping = "";
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        //[TestMethod]
        //public void CompleteCallback_Expected_Properties_Changed()
        //{
        //    DsfActivity Act = new DsfActivity();
        //    Act.InputMapping = @"<Inputs><Input Name=""Host"" Source=""Host"" DefaultValue=""mail.bellevuenet.co.za""><Validator Type=""Required"" /></Input><Input Name=""Port"" Source=""Port"" DefaultValue=""25""><Validator Type=""Required"" /></Input><Input Name=""From"" Source=""From"" DefaultValue=""DynamicServiceFramework@theunlimited.co.za""><Validator Type=""Required"" /></Input><Input Name=""To"" Source=""To""><Validator Type=""Required"" /></Input><Input Name=""Subject"" Source=""Subject""><Validator Type=""Required"" /></Input><Input Name=""BodyType"" Source=""Bodytype"" DefaultValue=""html""><Validator Type=""Required"" /></Input><Input Name=""Body"" Source=""Body""><Validator Type=""Required"" /></Input><Input Name=""Attachment"" Source=""Attachment"" DefaultValue=""NONE""><Validator Type=""Required"" /></Input></Inputs>";
        //    Act.OutputMapping = @"<Outputs><Output Name=""FailureMessage"" MapsTo=""FailureMessage"" Value=""[[FailureMessage]]"" /><Output Name=""Message"" MapsTo=""Message"" Value=""[[Message]]"" /></Outputs>";
        //    string TestDataListShape = @"<DataList><Host_Input></Host_Input><Port_Input></Port_Input><From_Input></From_Input><To_Input></To_Input><Subject_Input></Subject_Input><BodyType_Input></BodyType_Input><Body_Input></Body_Input><Attachment_Input></Attachment_Input><FailureMessage_Output></FailureMessage_Output><Message_Output></Message_Output></DataList>";
        //    string TestDataList = @"<DataList><Host_Input>HostData</Host_Input><Port_Input>PortData</Port_Input><From_Input>FromData</From_Input><To_Input>ToData</To_Input><Subject_Input>SubjectData</Subject_Input><BodyType_Input>BodyTypeData</BodyType_Input><Body_Input>BodyData</Body_Input><Attachment_Input>AttachmentData</Attachment_Input><FailureMessage_Output>[[FailureMessageData]]</FailureMessage_Output><Message_Output>[[MessageData]]</Message_Output></DataList>";
        //    ErrorResultTO errors;

        //    IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
        //    Func<IDataListCompiler> createCompiler = new Func<IDataListCompiler>(() => testCompiler);

        //    Guid dlID = testCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), TestDataList, TestDataListShape, out errors);

        //    ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);

        //    DsfActivityWizCallback callbackHandler = new DsfActivityWizCallback() { Activity = testItem, DatalistID = dlID, CreateCompiler = createCompiler };
        //    string expected = @"<Inputs><Input Name=""Host"" Source=""HostData"" DefaultValue=""mail.bellevuenet.co.za""><Validator Type=""Required"" /></Input><Input Name=""Port"" Source=""PortData"" DefaultValue=""25""><Validator Type=""Required"" /></Input><Input Name=""From"" Source=""FromData"" DefaultValue=""DynamicServiceFramework@theunlimited.co.za""><Validator Type=""Required"" /></Input><Input Name=""To"" Source=""ToData""><Validator Type=""Required"" /></Input><Input Name=""Subject"" Source=""SubjectData""><Validator Type=""Required"" /></Input><Input Name=""BodyType"" Source=""BodyTypeData"" DefaultValue=""html""><Validator Type=""Required"" /></Input><Input Name=""Body"" Source=""BodyData""><Validator Type=""Required"" /></Input><Input Name=""Attachment"" Source=""AttachmentData"" DefaultValue=""NONE""><Validator Type=""Required"" /></Input></Inputs>";
        //    XElement.Parse(expected);

        //    XElement.DeepEquals(XElement.Parse(expected), XElement.Parse(testItem.Properties["InputMapping"].ComputedValue as string));

        //    callbackHandler.CompleteCallback();
        //    Assert.IsTrue(XElement.DeepEquals(XElement.Parse(expected), XElement.Parse(testItem.Properties["InputMapping"].ComputedValue as string)));
        //}

        [TestMethod]
        public void CancelCallback_Expected_Properties_Changed()
        {
            DsfActivity Act = new DsfActivity();
            string inputMapping = @"<Inputs><Input Name=""Host"" Source=""Host"" DefaultValue=""mail.bellevuenet.co.za""><Validator Type=""Required"" /></Input><Input Name=""Port"" Source=""Port"" DefaultValue=""25""><Validator Type=""Required"" /></Input><Input Name=""From"" Source=""From"" DefaultValue=""DynamicServiceFramework@theunlimited.co.za""><Validator Type=""Required"" /></Input><Input Name=""To"" Source=""To""><Validator Type=""Required"" /></Input><Input Name=""Subject"" Source=""Subject""><Validator Type=""Required"" /></Input><Input Name=""BodyType"" Source=""Bodytype"" DefaultValue=""html""><Validator Type=""Required"" /></Input><Input Name=""Body"" Source=""Body""><Validator Type=""Required"" /></Input><Input Name=""Attachment"" Source=""Attachment"" DefaultValue=""NONE""><Validator Type=""Required"" /></Input></Inputs>";
            Act.InputMapping = inputMapping;
            Act.OutputMapping = @"<Outputs><Output Name=""FailureMessage"" MapsTo=""FailureMessage"" Value=""[[FailureMessage]]"" /><Output Name=""Message"" MapsTo=""Message"" Value=""[[Message]]"" /></Outputs>";
            string TestDataListShape = @"<DataList><Host_Input></Host_Input><Port_Input></Port_Input><From_Input></From_Input><To_Input></To_Input><Subject_Input></Subject_Input><BodyType_Input></BodyType_Input><Body_Input></Body_Input><Attachment_Input></Attachment_Input><FailureMessage_Output></FailureMessage_Output><Message_Output></Message_Output></DataList>";
            string TestDataList = @"<DataList><Host_Input>HostData</Host_Input><Port_Input>PortData</Port_Input><From_Input>FromData</From_Input><To_Input>ToData</To_Input><Subject_Input>SubjectData</Subject_Input><BodyType_Input>BodyTypeData</BodyType_Input><Body_Input>BodyData</Body_Input><Attachment_Input>AttachmentData</Attachment_Input><FailureMessage_Output>[[FailureMessageData]]</FailureMessage_Output><Message_Output>[[MessageData]]</Message_Output></DataList>";
            ErrorResultTO errors;

            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            Func<IDataListCompiler> createCompiler = new Func<IDataListCompiler>(() => testCompiler);

            Guid dlID = testCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), TestDataList, TestDataListShape, out errors);

            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);

            DsfActivityWizCallback callbackHandler = new DsfActivityWizCallback() { Activity = testItem, DatalistID = dlID, CreateCompiler = createCompiler };

            callbackHandler.CancelCallback();
            Assert.IsTrue(XElement.DeepEquals(XElement.Parse(inputMapping), XElement.Parse(testItem.Properties["InputMapping"].ComputedValue as string)));
        }
    }
}
