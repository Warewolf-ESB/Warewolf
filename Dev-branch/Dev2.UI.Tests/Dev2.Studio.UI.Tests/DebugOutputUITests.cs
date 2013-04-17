using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
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
        private TestContext testContextInstance;

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void DebugOutputUITestInitiliaze()
        {
            // Set default browser to IE for tests
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\shell\\Associations\\UrlAssociations\\http\\UserChoice", true);
            if (regkey != null)
            {
                string browser = regkey.GetValue("Progid").ToString();
                if (browser != "IE.HTTP")
                {
                    regkey.SetValue("Progid", "IE.HTTP");
                }
            }


            // Set focus to the Studio (So your Coded UI Test doesn't start doing stuff on your actual screen)
            WpfWindow theWindow = new WpfWindow();
            theWindow.WindowTitles.Add(UITestUtils.GetStudioWindowName());
            theWindow.Find();
            theWindow.SetFocus();

            bool toCheck = true;

            // On the test box, all test initialisations should always run
            if (UITestUtils.GetStudioWindowName().Contains("IntegrationTester"))
            {
                toCheck = true;
            }
            // ReSharper restore ReplaceWithSingleAssignment.False

            // Useful when creating / debugging tests
            if (toCheck)
            {
                try
                {
                    // Make sure no instances of IE are running (For Bug Test crashes)
                    ExternalUIMap.CloseAllInstancesOfIE();
                }
                catch
                {
                    throw new Exception("Error - Cannot close all instances of IE!");
                }

                // Make sure the Server has started
                try
                {
                    //Process[] processList = System.Diagnostics.Process.GetProcesses();
                    List<Process> findDev2Servers = Process.GetProcesses().Where(p => p.ProcessName.StartsWith("Dev2.Server")).ToList();
                    int serverCounter = findDev2Servers.Count();
                    if (!(serverCounter > 0 && serverCounter < 3))
                    {
                        Assert.Fail("Tests cannot run - The Server is not running!");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Tests cannot run - The Server is not running!"))
                    {
                        throw new Exception("Tests cannot run - The Server is not running!");
                    }
                    else
                    {
                        throw new Exception("Error - Unable to check if the Server has started - " + ex.InnerException);
                    }
                }

                try
                {
                    // Make sure the Studio has started
                    var findDev2Studios = Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName.StartsWith("Dev2.Studio"));
                    int studioCounter = findDev2Studios.Count();
                    if (studioCounter != 1)
                    {
                        Assert.Fail("Tests cannot run - The Studio is not running!");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error - Unable to check if the Studio has started!", ex.InnerException);
                }

                try
                {
                    // Make sure no tabs are open
                    int openTabs = TabManagerUIMap.GetTabCount();
                    while (openTabs != 0)
                    {
                        string theTab = TabManagerUIMap.GetActiveTabName();
                        {
                            // Click in the middle of the screen and wait, incase a side menu is open (Which covers the tabs "X")
                            UITestControl zeTab = TabManagerUIMap.FindTabByName(theTab);
                            Mouse.Click(new Point(zeTab.BoundingRectangle.X + 500, zeTab.BoundingRectangle.Y + 500));
                            Thread.Sleep(2500);
                            Mouse.Click(new Point(zeTab.BoundingRectangle.X + 500, zeTab.BoundingRectangle.Y + 500));
                            Thread.Sleep(2500);
                        }
                        TabManagerUIMap.CloseTab(theTab);
                        SendKeys.SendWait("n");

                        SendKeys.SendWait("{DELETE}");     // 
                        SendKeys.SendWait("{BACKSPACE}");  // Incase it was actually typed
                        //

                        openTabs = TabManagerUIMap.GetTabCount();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error - Unable to close tabs! Reason: " + ex.Message);
                }
            }

        }

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

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
        public void DebugOutput_Given_RefreshOnBrowser_Expected_DebugOutputWindowNotUpdated()
        {
            // Create a new workflow

            string workflowToCreate = "DebugOutputOnRefreshBrowser";
            CreateWorkflow(workflowToCreate);

            UITestControl control = TabManagerUIMap.FindTabByName(workflowToCreate);
            if (control != null)
            {
                // Drag an assign onto the Design Surface and configure the control
                DockManagerUIMap.ClickOpenTabPage("Toolbox");
                ToolboxUIMap.DragControlToWorkflowDesigner("Assign", WorkflowDesignerUIMap.GetPointUnderStartNode(control));
                WorkflowDesignerUIMap.SetStartNode(control, "Assign");
                WorkflowDesignerUIMap.AssignControl_EnterData(control, "Assign", "[[test]]", "test");
                // Update the datalist
                DockManagerUIMap.ClickOpenTabPage("Variables");
                VariablesUIMap.UpdateDataList();
                //Debug the workflow.
                RibbonUIMap.ClickRibbonMenuItem("Home", "Save");
                RibbonUIMap.ClickRibbonMenuItem("Home", "Debug");
                Thread.Sleep(1000);
                DebugUIMap.ExecuteDebug();
                // Check the output tab for the debug data
                DockManagerUIMap.ClickOpenTabPage("Output");
                UITestControlCollection ctrl = DebugOutputUIMap.GetOutputWindow();
                UITestControlCollection initialOutputs = DebugOutputUIMap.GetStepInOutputWindow(ctrl[1], "Assign (1)");
                // View in Browser then refresh
                RibbonUIMap.ClickRibbonMenuItem("Home", "View in Browser");
                Thread.Sleep(1000);
                ExternalUIMap.SendIERefresh();
                // Close Internet Explorer
                ExternalUIMap.CloseAllInstancesOfIE();
                Thread.Sleep(1000);
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
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", workflowToCreate);
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

        #endregion UI Maps

        #region Private Test Methods

        private void CreateWorkflow(string workflowName)
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");

            while (!WorkflowWizardUIMap.IsWindowOpen())
            {
                Thread.Sleep(1000);
            }
            Thread.Sleep(2000);

            WorkflowWizardUIMap.EnterWorkflowName(workflowName);
            WorkflowWizardUIMap.EnterWorkflowCategory("CodedUITestCategory");
            WorkflowWizardUIMap.DoneButtonClick();
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
