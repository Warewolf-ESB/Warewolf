
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using Dev2.CodedUI.Tests;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.OutputUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExternalUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.Win32;
using System.Diagnostics;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using System.Threading;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.ServiceDetailsUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.VariablesUIMapClasses;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class DebugOutputUITests
    {
        public DebugOutputUITests()
        {
        }

        #region Additional test attributes

                /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext myTestContext
        {
            get
            {
                return _myTestContextInstance;
            }
            set
            {
                _myTestContextInstance = value;
            }
        }

        private TestContext _myTestContextInstance;

        #endregion

        #region Debug Output Tests

        //Sashen : 14-02-2012 : BUG 8793 : This test ensures that the browser refreshes do not affect the Debug Output window.
        //                                 Previously, the debug window would contain a new entry for debug info for every single
        //                                 browser refresh, which seemed extremely disconnected and was a little weird.
        //                                 The test itself will create a new workflow, Drag n assign onto the workflow.
        //                                 Setup the assign with a little test data, saves the workflow, then runs through debug
        //                                 ensures that the debug information exists, then executes in a browser, and refreshes
        //                                 the page displayed by the browser, and checks that the debug output window does not contain
        //                                 the result of what came from the server when executing the workflow.
        [TestMethod]
        [Ignore]
        // Faulty DebugInput window
        public void DebugOutputWithRefreshOnBrowserExpectedDebugOutputWindowNotUpdated()
        {
            // Create a new workflow
        
            CreateWorkflow();

            UITestControl control = TabManagerUIMap.FindTabByName("Unsaved 1");
            if (control != null)
            {
                // Drag an assign onto the Design Surface and configure the control
                DockManagerUIMap.ClickOpenTabPage("Toolbox");
                ToolboxUIMap.DragControlToWorkflowDesigner("Assign", WorkflowDesignerUIMap.GetPointUnderStartNode(control));
                WorkflowDesignerUIMap.SetStartNode(control, "Assign");
                WorkflowDesignerUIMap.AssignControl_EnterData(control, "Assign", "[[test]]", "test");             
                //Debug the workflow.                
                RibbonUIMap.ClickRibbonMenuItem("Home", "Debug");               
                DebugUIMap.ExecuteDebug();
                // Check the output tab for the debug data
                DockManagerUIMap.ClickOpenTabPage("Output");
                var ctrl = DebugOutputUIMap.GetOutputWindow();               
                // View in Browser then refresh
                RibbonUIMap.ClickRibbonMenuItem("Home", "View in Browser");
                Thread.Sleep(1000);
                ExternalUIMap.SendIERefresh();
                // Close Internet Explorer
                ExternalUIMap.CloseAllInstancesOfIE();                
                // Check that the Output window only contains the Compiler message for successful service compilation
                // As it always does on View in Browser
                DockManagerUIMap.ClickOpenTabPage("Output");
                UITestControlCollection actualOutputs = DebugOutputUIMap.GetOutputWindow();

                Assert.AreEqual(1, actualOutputs.Count);
            }
            else
            {
                Assert.Fail("Unable to create workflow to test Debug Output on Browser Refresh");
            }
            // All good - Cleanup time!
            new TestBase().DoCleanup("Unsaved 1",true);             
        }

        //2013.05.07: Ashley Lewis - Bug 7904
        [TestMethod]
        [Ignore]
        // Unstable
        public void DebugOutputWithLargeDataExpectedDebugOutputWindowUpdatedWithin5Seconds()
        {
            //Open LargeFileTesting workflow
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("large");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "TESTS", "LargeFileTesting");
            ExplorerUIMap.ClearExplorerSearchText();

            //Click debug
            SendKeys.SendWait("{F5}");
            Thread.Sleep(2000);
            SendKeys.SendWait("{F5}");

            //Assert the debug output window is responsive after 5 seconds
            // Get data split step
            DockManagerUIMap.ClickOpenTabPage("Output");
            DebugOutputUIMap.ClearSearch();
            Mouse.Click();
            Keyboard.SendKeys("Data Split");
            var getSteps = DebugOutputUIMap.GetOutputWindow();
            Assert.IsTrue(getSteps.Count == 1, "Debug output window took too long to respond");
            var dataSplitDebugOutput = getSteps[0];
            Assert.IsTrue(dataSplitDebugOutput.Exists, "Debug output window took too long to respond");

            //Assert recset node expands and the more data link within that recset node is visible
            var expander = dataSplitDebugOutput.GetChildren().Last(c => c.ControlType.Name == "Expander");
            Mouse.Move(expander, new Point(0, 0));
            Mouse.Move(new Point(Mouse.Location.X + 5, Mouse.Location.Y + 5));
            Mouse.Click();
            Mouse.Move(new Point(Mouse.Location.X - 25, Mouse.Location.Y - 25));
            Mouse.Click();
            for (var i = 0; i < 4; i++)
            {
                Mouse.MoveScrollWheel(-1);
            }
            // Get more data link
            var moreLink = expander.GetChildren()[1].GetChildren().Last(c => c.Name == "...");
            var moreLinkClickablePoint = new Point();
            Assert.IsTrue(moreLink.TryGetClickablePoint(out moreLinkClickablePoint), "Recordset did not expand properly");
            new TestBase().DoCleanup("LargeFileTesting");
        }

        static void CloseError()
        {
            var errMsg = new UIErrorWindow();
            if(errMsg.Exists)
            {
                Mouse.DoubleClick(errMsg, new Point(errMsg.Width - 15, 5));
                Mouse.Click();
            }
        }

        #endregion Debug Output Tests

        #region UI Maps

        #region Debug Output UI Map

        private OutputUIMap DebugOutputUIMap
        {
            get
            {
                if (_debugOutputUIMap == null)
                {
                    _debugOutputUIMap = new OutputUIMap();
                }

                return _debugOutputUIMap;
            }
        }

        private OutputUIMap _debugOutputUIMap;


        #endregion Debug Output UI Map

        #region Debug UI Map

        private DebugUIMap DebugUIMap
        {
            get
            {
                if (_debugUIMap == null)
                {
                    _debugUIMap = new DebugUIMap();
                }

                return _debugUIMap;
            }
        }

        private DebugUIMap _debugUIMap;

        #endregion Debug UI Map

        #region Browser UI Map

        private ExternalUIMap ExternalUIMap
        {
            get
            {
                if (_externalUIMap == null)
                {
                    _externalUIMap = new ExternalUIMap();
                }

                return _externalUIMap;
            }
        }

        private ExternalUIMap _externalUIMap;

        #endregion Browser UI Map

        #region Tab Manager UI Map

        private TabManagerUIMap TabManagerUIMap
        {
            get
            {
                if (_tabManagerUIMap == null)
                {
                    _tabManagerUIMap = new TabManagerUIMap();
                }

                return _tabManagerUIMap;
            }
        }


        private TabManagerUIMap _tabManagerUIMap;
        #endregion Tab Manager UI Map

        #region Explorer UI Map

        private ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_explorerUIMap == null)
                {
                    _explorerUIMap = new ExplorerUIMap();
                }

                return _explorerUIMap;
            }
        }

        private ExplorerUIMap _explorerUIMap;

        #endregion Explorer UI Map

        #region Ribbon UI Map

        private RibbonUIMap RibbonUIMap
        {
            get
            {
                if (_ribbonUIMap == null)
                {
                    _ribbonUIMap = new RibbonUIMap();
                }

                return _ribbonUIMap;
            }
        }

        private RibbonUIMap _ribbonUIMap;

        #endregion Ribbon UI Map

        #region Service Details UI Map

        private ServiceDetailsUIMap ServiceDetailsUIMap
        {
            get
            {
                if (_serviceDetailsUIMap == null)
                {
                    _serviceDetailsUIMap = new ServiceDetailsUIMap();
                }

                return _serviceDetailsUIMap;
            }
        }

        private ServiceDetailsUIMap _serviceDetailsUIMap;

        #endregion Service Details UI Map

        #region WorkflowWizard UI Map

        private WorkflowWizardUIMap WorkflowWizardUIMap
        {
            get
            {
                if (_workflowWizardUIMap == null)
                {
                    _workflowWizardUIMap = new WorkflowWizardUIMap();
                }

                return _workflowWizardUIMap;
            }
        }

        private WorkflowWizardUIMap _workflowWizardUIMap;

        #endregion Workflow Wizard UI Map

        #region WorkflowDesigner UI Map

        private WorkflowDesignerUIMap WorkflowDesignerUIMap
        {
            get
            {
                if (_workflowDesignerUIMap == null)
                {
                    _workflowDesignerUIMap = new WorkflowDesignerUIMap();
                }

                return _workflowDesignerUIMap;
            }
        }

        private WorkflowDesignerUIMap _workflowDesignerUIMap;

        #endregion WorkflowDesigner UI Map


        #region DataList UI Map

        private VariablesUIMap VariablesUIMap
        {
            get
            {
                if (_variablesUIMap == null)
                {
                    _variablesUIMap = new VariablesUIMap();
                }

                return _variablesUIMap;
            }
        }

        private VariablesUIMap _variablesUIMap;

        #endregion DataList UI Map

        #region DockManager UI Map

        private DocManagerUIMap DockManagerUIMap
        {
            get
            {
                if (_dockManagerUIMap == null)
                {
                    _dockManagerUIMap = new DocManagerUIMap();
                }

                return _dockManagerUIMap;
            }
        }

        private DocManagerUIMap _dockManagerUIMap;

        #endregion DockManager UI Map

        #region Toolbox UI Map

        private ToolboxUIMap ToolboxUIMap
        {
            get
            {
                if (_toolboxManagerUIMap == null)
                {
                    _toolboxManagerUIMap = new ToolboxUIMap();
                }

                return _toolboxManagerUIMap;
            }
        }

        private ToolboxUIMap _toolboxManagerUIMap;

        #endregion Toolbox UI Map

        #region UIBusinessDesignStudioWindow

        public class UiBusinessDesignStudioWindow : WpfWindow
        {

            public UiBusinessDesignStudioWindow()
            {
                SearchProperties[UITestControl.PropertyNames.Name] = TestBase.GetStudioWindowName();
                SearchProperties.Add(new PropertyExpression(UITestControl.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
                WindowTitles.Add(TestBase.GetStudioWindowName());
            }

            public WpfTabList UiRibbonTabList
            {
                get
                {
                    if ((_mUiRibbonTabList == null))
                    {
                        _mUiRibbonTabList = new WpfTabList(this);
                        _mUiRibbonTabList.SearchProperties[WpfControl.PropertyNames.AutomationId] = "ribbon";
                        _mUiRibbonTabList.WindowTitles.Add(TestBase.GetStudioWindowName());
                    }
                    return this._mUiRibbonTabList;
                }
            }

            #region Fields
            private WpfTabList _mUiRibbonTabList;
            #endregion
        }

        #endregion

        #region UIErrorWindow

		public class UIErrorWindow : WpfWindow
        {

            public UIErrorWindow()
            {
                #region Search Criteria
                SearchProperties[UITestControl.PropertyNames.Name] = "Error";
                SearchProperties.Add(new PropertyExpression(UITestControl.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
                WindowTitles.Add("Error");
                #endregion
            }
        } 

	    #endregion

        #endregion UI Maps

        #region Private Test Methods

        private void CreateWorkflow()
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
        }

        private void DoCleanup(string server, string serviceType, string category, string workflowName)
        {
            // Test complete - Delete itself
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();

            // Delete the workflow
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickDeleteProject(server, serviceType, category, workflowName);
        }

        #endregion Private Test Methods
    }
}
