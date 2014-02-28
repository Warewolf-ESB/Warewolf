using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Utils;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.UtilsTests.ActivityDesignerUtilsTests
{
    /// <summary>
    /// Summary description for ForeachActivityDesignerUtilsTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ForeachActivityDesignerUtilsTests
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
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void DraggingANormalActivityIntoAForeachExpectedIsDropableTrue()
        {
            ModelItem modelItem = ModelItemUtils.CreateModelItem(new DsfMultiAssignActivity());

            ForeachActivityDesignerUtils foreachActivityDesignerUtils = new ForeachActivityDesignerUtils();
            bool isDropable = foreachActivityDesignerUtils.ForeachDropPointOnDragEnter(modelItem);

            Assert.IsTrue(isDropable);
        }

        [TestMethod]
        public void DraggingADecisionActivityIntoAForeachExpectedIsDropableFalse()
        {
            ModelItem modelItem = ModelItemUtils.CreateModelItem(new FlowDecision());

            ForeachActivityDesignerUtils foreachActivityDesignerUtils = new ForeachActivityDesignerUtils();
            bool isDropable = foreachActivityDesignerUtils.ForeachDropPointOnDragEnter(modelItem);

            Assert.IsFalse(isDropable);
        }

        [TestMethod]
        public void DraggingASwitchActivityIntoAForeachExpectedIsDropableFalse()
        {
            ModelItem modelItem = ModelItemUtils.CreateModelItem(new FlowSwitch<string>());

            ForeachActivityDesignerUtils foreachActivityDesignerUtils = new ForeachActivityDesignerUtils();
            bool isDropable = foreachActivityDesignerUtils.ForeachDropPointOnDragEnter(modelItem);

            Assert.IsFalse(isDropable);
        }
    }
}
