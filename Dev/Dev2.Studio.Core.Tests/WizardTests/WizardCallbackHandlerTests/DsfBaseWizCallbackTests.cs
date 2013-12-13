using System;
using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.Core.Tests.Utils;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Wizards.CallBackHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.WizardTests.WizardCallbackHandlerTests
{
    /// <summary>
    /// Summary description for DsfBaseWizCallbackTests
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class DsfBaseWizCallbackTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

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
        public void MyTestInitialize() 
        {           
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CompleteCallback_Expected_Properties_Changed()
        {
            DsfCalculateActivity calcAct = new DsfCalculateActivity();
            calcAct.Expression = "PreExpressionData";
            calcAct.Result = "[[ResultData]]";
            string TestDataListShape = @"<DataList>
	<Expression></Expression>
	<Result></Result>
</DataList>";
            string TestDataList = @"<DataList>
	<Expression>PostExpressionData</Expression>
	<Result>PostResultData</Result>
</DataList>";
            ErrorResultTO errors;

            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            Func<IDataListCompiler> createCompiler = new Func<IDataListCompiler>(() => testCompiler);

            Guid dlID = testCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), TestDataList, TestDataListShape, out errors);                   

            ModelItem testItem = TestModelItemFactory.CreateModelItem(calcAct);

            DsfBaseWizCallback<DsfCalculateActivity> callbackHandler = new DsfCalculateActivityWizCallback() { Activity = testItem, DatalistID = dlID, CreateCompiler = createCompiler };
                
            callbackHandler.CompleteCallback();
            Assert.IsTrue((testItem.Properties["Expression"].ComputedValue as string) == "PostExpressionData");
        }

        [TestMethod]
        public void CancelCallback_Expected_Properties_No_Change()
        {
            DsfCalculateActivity calcAct = new DsfCalculateActivity();
            calcAct.Expression = "PreExpressionData";
            calcAct.Result = "[[ResultData]]";
            string TestDataListShape = @"<DataList>
	<Expression></Expression>
	<Result></Result>
</DataList>";
            string TestDataList = @"<DataList>
	<Expression>PostExpressionData</Expression>
	<Result>PostResultData</Result>
</DataList>";
            ErrorResultTO errors;

            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            Func<IDataListCompiler> createCompiler = new Func<IDataListCompiler>(() => testCompiler);

            Guid dlID = testCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), TestDataList, TestDataListShape, out errors);

            ModelItem testItem = TestModelItemFactory.CreateModelItem(calcAct);

            DsfBaseWizCallback<DsfCalculateActivity> callbackHandler = new DsfCalculateActivityWizCallback() { Activity = testItem, DatalistID = dlID, CreateCompiler = createCompiler };

            callbackHandler.CancelCallback();
            Assert.IsTrue((testItem.Properties["Expression"].ComputedValue as string) == "PreExpressionData");
        }
    }
}
