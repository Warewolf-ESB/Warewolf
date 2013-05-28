using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Dev2.Interfaces;
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
    /// Summary description for DsfCaseConvertActivityWizCallbackHandlerTests
    /// </summary>
    [TestClass]
    public class DsfCaseConvertActivityWizCallbackHandlerTests
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
            DsfCaseConvertActivity baseAct = new DsfCaseConvertActivity();            
            string TestDataListShape = @"<DataList>
	<ConvertCollection>
		<StringToConvert/>
		<ConvertType/>
		<Result/>		
	</ConvertCollection>
</DataList>";
            string TestDataList = @"<DataList>
	<ConvertCollection>
		<StringToConvert>[[expression1]]</StringToConvert>
		<ConvertType>Title Case</ConvertType>
		<Result>[[res1]]</Result>
	</ConvertCollection>
	<ConvertCollection>
		<StringToConvert>[[expression2]]</StringToConvert>
		<ConvertType>Title Case</ConvertType>
		<Result>[[res2]]</Result>
	</ConvertCollection>
	<ConvertCollection>
		<StringToConvert>[[expression3]]</StringToConvert>
		<ConvertType>Title Case</ConvertType>
		<Result>[[res3]]</Result>
	</ConvertCollection>
</DataList>";
            ErrorResultTO errors;

            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            Func<IDataListCompiler> createCompiler = new Func<IDataListCompiler>(() => testCompiler);

            Guid dlID = testCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), TestDataList, TestDataListShape, out errors);

            ModelItem testItem = TestModelItemFactory.CreateModelItem(baseAct);

            DsfCaseConvertActivityWizCallback callbackHandler = new DsfCaseConvertActivityWizCallback() { Activity = testItem, DatalistID = dlID, CreateCompiler = createCompiler };

            callbackHandler.CompleteCallback();
            Assert.IsTrue((testItem.Properties["ConvertCollection"].ComputedValue as IList<ICaseConvertTO>).Count == 3);
        }

        [TestMethod]
        public void CancelCallback_Expected_Properties_Changed()
        {
            DsfCaseConvertActivity baseAct = new DsfCaseConvertActivity();
            string TestDataListShape = @"<DataList>
	<ConvertCollection>
		<StringToConvert/>
		<ConvertType/>
		<Result/>		
	</ConvertCollection>
</DataList>";
            string TestDataList = @"<DataList>
	<ConvertCollection>
		<StringToConvert>[[expression1]]</StringToConvert>
		<ConvertType>Title Case</ConvertType>
		<Result>[[res1]]</Result>
	</ConvertCollection>
	<ConvertCollection>
		<StringToConvert>[[expression2]]</StringToConvert>
		<ConvertType>Title Case</ConvertType>
		<Result>[[res2]]</Result>
	</ConvertCollection>
	<ConvertCollection>
		<StringToConvert>[[expression3]]</StringToConvert>
		<ConvertType>Title Case</ConvertType>
		<Result>[[res3]]</Result>
	</ConvertCollection>
</DataList>";
            ErrorResultTO errors;

            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            Func<IDataListCompiler> createCompiler = new Func<IDataListCompiler>(() => testCompiler);

            Guid dlID = testCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), TestDataList, TestDataListShape, out errors);

            ModelItem testItem = TestModelItemFactory.CreateModelItem(baseAct);

            DsfCaseConvertActivityWizCallback callbackHandler = new DsfCaseConvertActivityWizCallback() { Activity = testItem, DatalistID = dlID, CreateCompiler = createCompiler };

            callbackHandler.CancelCallback();
            Assert.IsTrue((testItem.Properties["ConvertCollection"].ComputedValue as IList<ICaseConvertTO>).Count == 0);
        }
    }
}
