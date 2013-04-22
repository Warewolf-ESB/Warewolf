using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.DataList.Contract;
using Dev2.Core.Tests.Utils;
using Dev2.Common;
using Dev2.Studio.Core.Wizards.CallBackHandlers;
using System.Activities.Presentation.Model;

namespace Dev2.Core.Tests.WizardTests.WizardCallbackHandlerTests
{
    /// <summary>
    /// Summary description for DsfDataSplitActivityWizCallbackHandlerTests
    /// </summary>
    [TestClass]
    public class DsfDataSplitActivityWizCallbackHandlerTests
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
            DsfDataSplitActivity multiAct = new DsfDataSplitActivity();
            multiAct.ResultsCollection = new List<DataSplitDTO>();
            string TestDataListShape = @"<DataList>
	<SourceString/>
	<ReverseOrder/>
	<ResultsCollection>
		<SplitType/>
		<At/>
		<Include/>
		<Result/>
	</ResultsCollection>
</DataList>";
            string TestDataList = @"<DataList>
	<SourceString>[[Source]]</SourceString>
	<ReverseOrder>false</ReverseOrder>
	<ResultsCollection>
		<SplitType>Index</SplitType>
		<At>2</At>
		<Include>false</Include>
		<Result>[[res1]]</Result>
	</ResultsCollection>
	<ResultsCollection>
		<SplitType>Index</SplitType>
		<At>2</At>
		<Include>false</Include>
		<Result>[[res1]]</Result>
	</ResultsCollection>
	<ResultsCollection>
		<SplitType>Index</SplitType>
		<At>2</At>
		<Include>false</Include>
		<Result>[[res1]]</Result>
	</ResultsCollection>
</DataList>";
            ErrorResultTO errors;
            
            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            Func<IDataListCompiler> createCompiler = new Func<IDataListCompiler>(() => testCompiler);

            Guid dlID = testCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), TestDataList, TestDataListShape, out errors);

            ModelItem testItem = TestModelItemFactory.CreateModelItem(multiAct);

            DsfDataSplitActivityWizCallback callbackHandler = new DsfDataSplitActivityWizCallback() { Activity = testItem, DatalistID = dlID, CreateCompiler = createCompiler };

            callbackHandler.CompleteCallback();
            Assert.IsTrue((testItem.Properties["ResultsCollection"].ComputedValue as IList<DataSplitDTO>).Count == 3);
        }

        [TestMethod]
        public void CancelCallback_Expected_Properties_Changed()
        {
            DsfDataSplitActivity multiAct = new DsfDataSplitActivity();
            multiAct.ResultsCollection = new List<DataSplitDTO>();
            string TestDataListShape = @"<DataList>
	<SourceString/>
	<ReverseOrder/>
	<ResultsCollection>
		<SplitType/>
		<At/>
		<Include/>
		<Result/>
	</ResultsCollection>
</DataList>";
            string TestDataList = @"<DataList>
	<SourceString>[[Source]]</SourceString>
	<ReverseOrder>false</ReverseOrder>
	<ResultsCollection>
		<SplitType>Index</SplitType>
		<At>2</At>
		<Include>false</Include>
		<Result>[[res1]]</Result>
	</ResultsCollection>
	<ResultsCollection>
		<SplitType>Index</SplitType>
		<At>2</At>
		<Include>false</Include>
		<Result>[[res1]]</Result>
	</ResultsCollection>
	<ResultsCollection>
		<SplitType>Index</SplitType>
		<At>2</At>
		<Include>false</Include>
		<Result>[[res1]]</Result>
	</ResultsCollection>
</DataList>";
            ErrorResultTO errors;
            
            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            Func<IDataListCompiler> createCompiler = new Func<IDataListCompiler>(() => testCompiler);

            Guid dlID = testCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), TestDataList, TestDataListShape, out errors);

            ModelItem testItem = TestModelItemFactory.CreateModelItem(multiAct);

            DsfDataSplitActivityWizCallback callbackHandler = new DsfDataSplitActivityWizCallback() { Activity = testItem, DatalistID = dlID, CreateCompiler = createCompiler };

            callbackHandler.CancelCallback();
            Assert.IsTrue((testItem.Properties["ResultsCollection"].ComputedValue as IList<DataSplitDTO>).Count == 0);
        }
    }
}
