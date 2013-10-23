using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.DataList.Contract;
using Dev2.Common;
using Dev2.Core.Tests.Utils;
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Wizards.CallBackHandlers;
using Unlimited.Framework;

namespace Dev2.Core.Tests.WizardTests.WizardCallbackHandlerTests
{
    /// <summary>
    /// Summary description for DsfBaseConvertActivityWizCallbackHandlerTests
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class DsfBaseConvertActivityWizCallbackHandlerTests
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
            DsfBaseConvertActivity baseAct = new DsfBaseConvertActivity();                       
            string TestDataListShape = @"<DataList>
	<ConvertCollection>
		<FromExpression/>
		<FromType/>
		<ToType/>
		<Result/>
	</ConvertCollection>
</DataList>";
            string TestDataList = @"<DataList>
	<ConvertCollection>
		<FromExpression>[[expression1]]</FromExpression>
		<FromType>Base64</FromType>
		<ToType>Hex</ToType>
		<Result>[[res1]]</Result>
	</ConvertCollection>
	<ConvertCollection>
		<FromExpression>[[expression2]]</FromExpression>
		<FromType>Base64</FromType>
		<ToType>Hex</ToType>
		<Result>[[res2]]</Result>
	</ConvertCollection>
	<ConvertCollection>
		<FromExpression>[[expression3]]</FromExpression>
		<FromType>Base64</FromType>
		<ToType>Hex</ToType>
		<Result>[[res3]]</Result>
	</ConvertCollection>
</DataList>";
            ErrorResultTO errors;

            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            Func<IDataListCompiler> createCompiler = new Func<IDataListCompiler>(() => testCompiler);
            
            Guid dlID = testCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), TestDataList, TestDataListShape, out errors);

            ModelItem testItem = TestModelItemFactory.CreateModelItem(baseAct);

            DsfBaseConvertActivityWizCallback callbackHandler = new DsfBaseConvertActivityWizCallback() { Activity = testItem, DatalistID = dlID, CreateCompiler = createCompiler };

            callbackHandler.CompleteCallback();
            Assert.IsTrue((testItem.Properties["ConvertCollection"].ComputedValue as IList<BaseConvertTO>).Count == 3);
        }

        [TestMethod]
        public void CancelCallback_Expected_Properties_Changed()
        {
            DsfBaseConvertActivity baseAct = new DsfBaseConvertActivity();
            string TestDataListShape = @"<DataList>
	<ConvertCollection>
		<FromExpression/>
		<FromType/>
		<ToType/>
		<Result/>
	</ConvertCollection>
</DataList>";
            string TestDataList = @"<DataList>
	<ConvertCollection>
		<FromExpression>[[expression1]]</FromExpression>
		<FromType>Base64</FromType>
		<ToType>Hex</ToType>
		<Result>[[res1]]</Result>
	</ConvertCollection>
	<ConvertCollection>
		<FromExpression>[[expression2]]</FromExpression>
		<FromType>Base64</FromType>
		<ToType>Hex</ToType>
		<Result>[[res2]]</Result>
	</ConvertCollection>
	<ConvertCollection>
		<FromExpression>[[expression3]]</FromExpression>
		<FromType>Base64</FromType>
		<ToType>Hex</ToType>
		<Result>[[res3]]</Result>
	</ConvertCollection>
</DataList>";
            ErrorResultTO errors;

            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            Func<IDataListCompiler> createCompiler = new Func<IDataListCompiler>(() => testCompiler);

            Guid dlID = testCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), TestDataList, TestDataListShape, out errors);

            ModelItem testItem = TestModelItemFactory.CreateModelItem(baseAct);

            DsfBaseConvertActivityWizCallback callbackHandler = new DsfBaseConvertActivityWizCallback() { Activity = testItem, DatalistID = dlID, CreateCompiler = createCompiler };

            callbackHandler.CancelCallback();
            Assert.IsTrue((testItem.Properties["ConvertCollection"].ComputedValue as IList<BaseConvertTO>).Count == 0);
        }
    }
}
