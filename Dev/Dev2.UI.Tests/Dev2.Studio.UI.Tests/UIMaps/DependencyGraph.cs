
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.CodedUI.Tests;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps.DependencyGraphClasses
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Input;
    using System.CodeDom.Compiler;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    
    
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
