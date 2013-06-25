using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.DataList.Contract;
using Dev2.Common;
using Dev2.Studio.Core.Wizards.CallBackHandlers;
using Dev2.Core.Tests.Utils;
using System.Activities.Presentation.Model;

namespace Dev2.Core.Tests.WizardTests.WizardCallbackHandlerTests
{
    /// <summary>
    /// Summary description for DsfMultiAssignActivityWizCallbackTests
    /// </summary>
    [TestClass]
    public class DsfMultiAssignActivityWizCallbackTests
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
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CompleteCallback_Expected_Properties_Changed()
        {
            DsfMultiAssignActivity multiAct = new DsfMultiAssignActivity();
            multiAct.FieldsCollection = new List<ActivityDTO>();
            string TestDataListShape = @"<DataList>
	<FieldsCollection>
		<FieldName/>
		<FieldValue/>
	</FieldsCollection>
</DataList>";
            string TestDataList = @"<DataList>
	<FieldsCollection>
		<FieldName>FieldName1</FieldName>
		<FieldValue>FieldValue1</FieldValue>
	</FieldsCollection>
	<FieldsCollection>
		<FieldName>FieldName2</FieldName>
		<FieldValue>FieldValue2</FieldValue>
	</FieldsCollection>
	<FieldsCollection>
		<FieldName>FieldName3</FieldName>
		<FieldValue>FieldValue3</FieldValue>
	</FieldsCollection>
</DataList>";
            ErrorResultTO errors;

            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            Func<IDataListCompiler> createCompiler = new Func<IDataListCompiler>(() => testCompiler);

            Guid dlID = testCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), TestDataList, TestDataListShape, out errors);

            ModelItem testItem = TestModelItemFactory.CreateModelItem(multiAct);

            DsfMultiAssignActivityWizCallback callbackHandler = new DsfMultiAssignActivityWizCallback() { Activity = testItem, DatalistID = dlID, CreateCompiler = createCompiler };

            callbackHandler.CompleteCallback();
            Assert.IsTrue((testItem.Properties["FieldsCollection"].ComputedValue as IList<ActivityDTO>).Count == 3);
        }

        [TestMethod]
        public void CancelCallback_Expected_Properties_Changed()
        {
            DsfMultiAssignActivity multiAct = new DsfMultiAssignActivity();
            multiAct.FieldsCollection = new List<ActivityDTO>();
            string TestDataListShape = @"<DataList>
	<FieldsCollection>
		<FieldName/>
		<FieldValue/>
	</FieldsCollection>
</DataList>";
            string TestDataList = @"<DataList>
	<FieldsCollection>
		<FieldName>FieldName1</FieldName>
		<FieldValue>FieldValue1</FieldValue>
	</FieldsCollection>
	<FieldsCollection>
		<FieldName>FieldName2</FieldName>
		<FieldValue>FieldValue2</FieldValue>
	</FieldsCollection>
	<FieldsCollection>
		<FieldName>FieldName3</FieldName>
		<FieldValue>FieldValue3</FieldValue>
	</FieldsCollection>
</DataList>";
            ErrorResultTO errors;

            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            Func<IDataListCompiler> createCompiler = new Func<IDataListCompiler>(() => testCompiler);

            Guid dlID = testCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), TestDataList, TestDataListShape, out errors);

            ModelItem testItem = TestModelItemFactory.CreateModelItem(multiAct);

            DsfMultiAssignActivityWizCallback callbackHandler = new DsfMultiAssignActivityWizCallback() { Activity = testItem, DatalistID = dlID, CreateCompiler = createCompiler };

            callbackHandler.CancelCallback();
            Assert.IsTrue((testItem.Properties["FieldsCollection"].ComputedValue as IList<ActivityDTO>).Count == 0);
        }
    }
}
