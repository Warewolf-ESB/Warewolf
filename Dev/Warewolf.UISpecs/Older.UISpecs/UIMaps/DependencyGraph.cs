using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps.DependencyGraphClasses
{
    using System.CodeDom.Compiler;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class DependencyGraph
    {
        public void TestRecursiveDependencies()
        {
            #region Variable Declarations
            WpfText uIRecursiveDependencyTText = this.UIBusinessDesignStudioWindow.UIItemCustom.UIMyScrollViewerPane.UIRecursiveDependencyTText;
            #endregion

            // Verify that the 'AutomationId' property of 'RecursiveDependencyTest' label equals '[DependencyGraph_RecursiveDependencyTest_IsCircular_True]'
            Assert.AreEqual("[DependencyGraph_RecursiveDependencyTest_IsCircular_True]", uIRecursiveDependencyTText.AutomationId);
        }

        public void findControlByAutomationId()
        {
            //WpfWindow studioBase = new WpfWindow();
            //studioBase.SearchProperties[WpfWindow.PropertyNames.Name] = TestBase.GetStudioWindowName();
            //studioBase.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            //studioBase.WindowTitles.Add(TestBase.GetStudioWindowName());
            //studioBase.Find();
            ////UITestControlCollection 
            //WpfText dependancyItem = new WpfText(studioBase);
            //dependancyItem.SearchProperties[WpfText.PropertyNames.AutomationId] = "[DependencyGraph_IntegrationTestReporting_IsCircular_False]";
            //dependancyItem.Find();
        }
        
    }

    /// <summary>
    /// Parameters to be passed into 'TestAssert'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "11.0.50727.1")]
    public class TestAssertExpectedValues
    {

        #region Fields
        /// <summary>
        /// Verify that the 'AutomationId' property of 'RecursiveDependencyTest' label equals '[DependencyGraph_RecursiveDependencyTest_IsCircular_True]'
        /// </summary>
        public string UIRecursiveDependencyTTextAutomationId = "[DependencyGraph_RecursiveDependencyTest_IsCircular_True]";
        #endregion
    }
}
