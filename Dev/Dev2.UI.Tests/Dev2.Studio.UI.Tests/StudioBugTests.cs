using System.Diagnostics;
using Dev2.CodedUI.Tests;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DeployViewUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExternalUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.PluginServiceWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.VariablesUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WebpageServiceWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DependencyGraphClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    //[Ignore]
    public class StudioBugTests
    {
        private readonly DecisionWizardUIMap _decisionWizardUiMap = new DecisionWizardUIMap();

        public void CreateWorkflow()
        {
            Keyboard.SendKeys(DocManagerUIMap.UIBusinessDesignStudioWindow, "{CTRL}W");
            //RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
        }

        public void DoCleanup(string workflowName, bool clickNo = false)
        {
            try
            {
                // Test complete - Delete itself  
                if (clickNo)
                {
                    TabManagerUIMap.CloseTab_Click_No(workflowName);
                }
                else
                {
                    TabManagerUIMap.CloseTab(workflowName);
                }
            }
            catch (Exception e)
            {
                // Log it so the UI Test still passes...
                Trace.WriteLine(e.Message);
            }

        }

        // Bug 6180
        [TestMethod]
        [Ignore]
        // Deploy Rework
        public void MakeSureDeployedItemsAreNotFiltered()
        {
            // Jurie has apparently fixed this, but just hasn't checked it in :D
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickDeployProject("localhost", "WORKFLOWS", "SYSTEM", "Base64ToString");
            //this.UIMap.DeployOptionClick - To fix
            // Wait for it to open!
            Thread.Sleep(5000);
            UITestControl theTab = TabManagerUIMap.FindTabByName("Deploy Resources");
            TabManagerUIMap.Click(theTab);
            DeployViewUIMap.EnterTextInSourceServerFilterBox(theTab, "ldnslgnsdg"); // Random text
            if (!DeployViewUIMap.DoesSourceServerHaveDeployItems(theTab))
            {
                Assert.Inconclusive("The deployed item has been removed with the filter - It should not be (Jurie should have fixed this....)");
            }
        }

        // Bug 6501
        [TestMethod]
        public void DeleteFirstDatagridRow_Expected_RowIsNotDeleted()
        {

            // Create the workflow
            //CreateWorkflow();

            // Get some design surface
            UITestControl theTab = TabManagerUIMap.FindTabByName("Unsaved 1");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("BaseConvert");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);


            // Enter some data
            UITestControl baseConversion = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "BaseConvert");
            Point p = new Point(baseConversion.BoundingRectangle.X + 40, baseConversion.BoundingRectangle.Y + 40);
            Mouse.Click(p);
            SendKeys.SendWait("someText");

            // Click the index
            p = new Point(baseConversion.BoundingRectangle.X + 20, baseConversion.BoundingRectangle.Y + 40);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(100);
            SendKeys.SendWait("{UP}");
            Thread.Sleep(100);
            SendKeys.SendWait("{UP}");
            Thread.Sleep(100);
            SendKeys.SendWait("{RIGHT}");
            Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(100);

            // Try type some data
            p = new Point(baseConversion.BoundingRectangle.X + 40, baseConversion.BoundingRectangle.Y + 40);
            Mouse.Click(p);
            SendKeys.SendWait("newText");
            SendKeys.SendWait("{END}"); // Shift Home - Highlights the item
            SendKeys.SendWait("+{HOME}"); // Shift Home - Highlights the item
            // Just to make sure it wasn't already copied before the test
            Clipboard.SetText("someRandomText");
            SendKeys.SendWait("^c"); // Copy command
            string clipboardText = Clipboard.GetText();
            if (clipboardText != "newText")
            {
                Assert.Fail("Error - The Item was not deleted!");
            }

            // Cleanup! \o/
            // All good - Cleanup time!
            new TestBase().DoCleanup("Unsaved 1", true); 
        }

        // Bug 6617
        [TestMethod]
        [Ignore]
        // Ashley work - Removed this menu option
        public void OpeningDependancyWindowTwiceKeepsItOpen()
        {
            // The workflow so we have a second tab
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "SYSTEM", "Base64ToString");
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Open the Dependancy Window twice
            for (int openCount = 0; openCount < 2; openCount++)
            {
                DocManagerUIMap.ClickOpenTabPage("Explorer");
                ExplorerUIMap.RightClickShowProjectDependancies("localhost", "WORKFLOWS", "SYSTEM", "Base64ToString");
            }

            string activeTab = TabManagerUIMap.GetActiveTabName();
            if (activeTab == "Base64ToString")
            {
                Assert.Fail("Opening the Dependency View twice should keep the UI on the same tab");
            }
        }
       
        // Bug 8408
        [TestMethod]
        [Ignore] // Silly test that does nothing really
        public void SortToolAndBaseConvertDropDownListsMatch()
        {
            // Create the workflow
            CreateWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.FindTabByName("Unsaved 1");

            // Get a reference point to start dragging
            Point thePoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);

            // Drag the controls on
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner("SortRecords", thePoint);

            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner("FindRecords", new Point(thePoint.X, thePoint.Y + 150));

            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner("BaseConvert", new Point(thePoint.X, thePoint.Y + 250));

            int sortControlHeight = WorkflowDesignerUIMap.Sort_GetDDLHeight(theTab, "SortRecords");
            int findRecordsHeight = WorkflowDesignerUIMap.FindRecords_GetDDLHeight(theTab, "Find Record Index");
            int baseConvertHeight = WorkflowDesignerUIMap.BaseConvert_GetDDLHeight(theTab, "Base Conversion");

            Assert.AreEqual(sortControlHeight, findRecordsHeight, "The height of the DDL's on the Sort Control and Find Record control are different!");
            Assert.AreNotEqual(sortControlHeight, baseConvertHeight, "The height of the DDL's on the Sort Control and Base Convert control are the same!");

            // Cleanup
            new TestBase().DoCleanup("Unsaved 1");
        }

        // Bug 8816
        [TestMethod]
        [Ignore]
        // Deploy Rework
        public void IsDeployButtonEnabledWithNothingToDeploy_Expected_DeployButtonIsDisabled()
        {
            // Click the Deploy button in the Ribbon
            RibbonUIMap.ClickRibbonMenuItem("Home", "Deploy");
            Thread.Sleep(3000);

            UITestControl deployTab = TabManagerUIMap.FindTabByName("Deploy Resources");

            // Make sure the Deploy button is disabled
            Assert.IsTrue(!DeployViewUIMap.IsDeployButtonEnabled(deployTab));

            // Connect to a Destination Server
            DeployViewUIMap.ChooseDestinationServer(deployTab, "localhost");

            // Make sure its still disabled, as nothing has been chosen to deploy
            Assert.IsTrue(!DeployViewUIMap.IsDeployButtonEnabled(deployTab), "As we have not chosen anything to deploy, the Deploy button should still be disabled!");
        }

        // Bug 8819
        [TestMethod]
        [Ignore]
        // Deploy Rework
        public void EnterFilterOnDestinationServer_Expected_DeployedItemsStillVisible()
        {
            // Choose to deploy one of our own items
            //ExplorerUIMap.DoRefresh();
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickDeployProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Set ourself as the destination server
            UITestControl deployTab = TabManagerUIMap.FindTabByName("Deploy Resources");
            DeployViewUIMap.ChooseDestinationServer(deployTab, "localhost");

            // Make sure the Destination server has items
            Assert.IsTrue(DeployViewUIMap.DoesDestinationServerHaveItems(deployTab));

            // Enter a filter in the destination server
            DeployViewUIMap.EnterTextInDestinationServerFilterBox(deployTab, "zzzzzzzzz");

            // And make sure it still has items
            Assert.IsTrue(DeployViewUIMap.DoesDestinationServerHaveItems(deployTab), "After a filter was applied, the destination Server lost all its items!");
        }

        //2013.05.29: Ashley Lewis for bug 9455 - Dont allow copy paste workflow xaml to another workflow
        [TestMethod]
        public void CopyWorkFlowWithContextMenuCopyAndPasteToAnotherWorkflowExpectedNothingCopied()
        {
            Clipboard.SetText(" ");
           
            Keyboard.SendKeys("{CTRL}W");
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            WorkflowDesignerUIMap.CopyWorkflowXamlWithContextMenu(theTab);
            Assert.IsTrue(string.IsNullOrWhiteSpace(Clipboard.GetText()), "Able to copy workflow Xaml using context menu");
            Keyboard.SendKeys("{CTRL}W");
            theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            var startButton = WorkflowDesignerUIMap.FindStartNode(theTab);
            Mouse.Click(new Point(startButton.BoundingRectangle.X - 5, startButton.BoundingRectangle.Y - 5));
            SendKeys.SendWait("^V");
            Assert.IsFalse(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "Unsaved 1(FlowchartDesigner)"));
            DoCleanup("Unsaved 1", true);

        }

        //2013.06.06: Ashley Lewis for 9448 - Dsf Activity Title - shows up as "DSFActivity" After a service has been dragged onto a workflow.
        [TestMethod]
        public void AddSecondServiceToWorkFlowExpectedDisplayTitleNotDsfActivity()
        {
            Keyboard.SendKeys("{CTRL}W");
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl startButton = WorkflowDesignerUIMap.FindStartNode(theTab);
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("email service");
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", new Point(startButton.BoundingRectangle.X + 50, startButton.BoundingRectangle.Y + 110));
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", new Point(startButton.BoundingRectangle.X + 50, startButton.BoundingRectangle.Y + 210));
            Assert.IsFalse(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "DsfActivity(DsfActivityDesigner)"), "Dropped services display title was 'DsfActivity' rather than the name of the service");
            DoCleanup("Unsaved 1", true);
        }

        

        //2013.06.22: Ashley Lewis for bug 9717 - copy paste multiple decisions
        [TestMethod]
        public void CopyDecisionsWithContextMenuAndPasteExpectedNoWizardsDisplayed()
        {
            //Initialize
            Clipboard.SetText(" ");
            Keyboard.SendKeys(DatabaseServiceWizardUIMap.UIBusinessDesignStudioWindow, "^w");
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl startButton = WorkflowDesignerUIMap.FindStartNode(theTab);
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            var decision = ToolboxUIMap.FindControl("Decision");
            //Drag on two decisions
            ToolboxUIMap.DragControlToWorkflowDesigner(decision, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            Thread.Sleep(1000);
            Keyboard.SendKeys("{TAB}{ENTER}");
            var newPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            newPoint.Y = newPoint.Y + 200;
            ToolboxUIMap.DragControlToWorkflowDesigner(decision, newPoint);
            Thread.Sleep(1000);
            Keyboard.SendKeys("{TAB}{ENTER}");
            //Rubberband select them
            var startDragPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            startDragPoint.X = startDragPoint.X - 100;
            startDragPoint.Y = startDragPoint.Y - 100;
            Mouse.Move(startDragPoint);
            newPoint.X = newPoint.X + 100;
            newPoint.Y = newPoint.Y + 100;
            Mouse.StartDragging();
            Mouse.StopDragging(newPoint);
            startDragPoint.X = startDragPoint.X + 110;
            startDragPoint.Y = startDragPoint.Y + 110;
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, startDragPoint);
            Keyboard.SendKeys("{DOWN}{DOWN}{ENTER}");
            var designSurface = WorkflowDesignerUIMap.GetFlowchartDesigner(theTab);
            Keyboard.SendKeys(designSurface, "^v");
            UITestControl uIItemImage = DatabaseServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            Assert.AreEqual("System Menu Bar", uIItemImage.FriendlyName);
            DoCleanup("Unsaved 1", true);
        }

        //2013.06.06: Ashley Lewis for 9599 - Default docking window layout and reset
        [TestMethod]
        [Ignore]//this needs ui pane mappings: pin pane button and rezise pane
        public void ResetLayOutWithDebugOutputExpandedAndExplorerPanePinnedExpectedReset()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.PinPane();
        }

        //2013.06.28: Ashley Lewis for bug 9802 - Foreach drill down test
        [TestMethod]
        public void DragAMultiAssignIntoAndOutOfAForEachExpectedNoDrillDown()
        {
            // Create the workflow
            CreateWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            Point requiredPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            requiredPoint.Offset(20, 50);

            // Drag a ForEach onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl tcForEach = ToolboxUIMap.FindToolboxItemByAutomationId("ForEach"); // ForEach
            ToolboxUIMap.DragControlToWorkflowDesigner(tcForEach, workflowPoint1);

            // Get a multiassign, and drag it onto the "Drop Activity Here" part of the ForEach box
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, new Point(workflowPoint1.X + 25, workflowPoint1.Y + 25));

            // Wait for the ForEach thing to do its things that that thing needs to do
            Thread.Sleep(3000);

            theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Assert.IsTrue(theStartButton.Exists, "Dropping a multiassign onto a foreach drilled down");
            DoCleanup("Unsaved 1",true);
        }

        private int GetInstanceUnderParent(UITestControl control)
        {
            UITestControl parent = control.GetParent();
            UITestControlCollection col = parent.GetChildren();
            int index = 1;

            foreach (UITestControl child in col)
            {
                if (child.Equals(control))
                {
                    break;
                }

                if (child.ControlType == control.ControlType)
                {
                    index++;
                }
            }
            return index;
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        #endregion

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }
        private TestContext _testContextInstance;

        #region UI Maps

        #region Ribbon UI Map

        public RibbonUIMap RibbonUIMap
        {
            get
            {
                if (_ribbonMap == null)
                {
                    _ribbonMap = new RibbonUIMap();
                }

                return _ribbonMap;
            }
        }

        private RibbonUIMap _ribbonMap;

        #endregion

        #region DocManager UI Map

        public DocManagerUIMap DocManagerUIMap
        {
            get
            {
                if ((_docManagerMap == null))
                {
                    _docManagerMap = new DocManagerUIMap();
                }

                return _docManagerMap;
            }
        }

        private DocManagerUIMap _docManagerMap;

        #endregion

        #region Toolbox UI Map

        public ToolboxUIMap ToolboxUIMap
        {
            get
            {
                if ((_toolboxUIMap == null))
                {
                    _toolboxUIMap = new ToolboxUIMap();
                }

                return _toolboxUIMap;
            }
        }

        private ToolboxUIMap _toolboxUIMap;

        #endregion

        #region Explorer UI Map

        public ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if ((_explorerUIMap == null))
                {
                    _explorerUIMap = new ExplorerUIMap();
                }

                return _explorerUIMap;
            }
        }

        private ExplorerUIMap _explorerUIMap;

        #endregion

        #region DependencyGraph UI Map

        public DependencyGraph DependencyGraphUIMap
        {
            get
            {
                if ((DependencyGraphUIMap == null))
                {
                    DependencyGraphUIMap = new DependencyGraph();
                }

                return DependencyGraphUIMap;
            }
            set { throw new NotImplementedException(); }
        }

        private DependencyGraph _dependencyGraphUIMap;

        #endregion

        #region DeployView UI Map

        public DeployViewUIMap DeployViewUIMap
        {
            get
            {
                if ((_deployViewUIMap == null))
                {
                    _deployViewUIMap = new DeployViewUIMap();
                }

                return _deployViewUIMap;
            }
        }

        private DeployViewUIMap _deployViewUIMap;

        #endregion

        #region TabManager UI Map

        public TabManagerUIMap TabManagerUIMap
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

        #endregion TabManager UI Map

        #region WorkflowDesigner UI Map

        public WorkflowDesignerUIMap WorkflowDesignerUIMap
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

        #region WorkflowWizard UI Map

        public WorkflowWizardUIMap WorkflowWizardUIMap
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

        #endregion WorkflowWizard UI Map

        #region Database Wizard UI Map

        public DatabaseServiceWizardUIMap DatabaseServiceWizardUIMap
        {
            get
            {
                if (_databaseServiceWizardUIMap == null)
                {
                    _databaseServiceWizardUIMap = new DatabaseServiceWizardUIMap();
                }

                return _databaseServiceWizardUIMap;
            }
        }

        private DatabaseServiceWizardUIMap _databaseServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region Plugin Wizard UI Map

        public PluginServiceWizardUIMap PluginServiceWizardUIMap
        {
            get
            {
                if (_pluginServiceWizardUIMap == null)
                {
                    _pluginServiceWizardUIMap = new PluginServiceWizardUIMap();
                }

                return _pluginServiceWizardUIMap;
            }
        }

        private PluginServiceWizardUIMap _pluginServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region Webpage Wizard UI Map

        public WebpageServiceWizardUIMap WebpageServiceWizardUIMap
        {
            get
            {
                if (_webpageServiceWizardUIMap == null)
                {
                    _webpageServiceWizardUIMap = new WebpageServiceWizardUIMap();
                }

                return _webpageServiceWizardUIMap;
            }
        }

        private WebpageServiceWizardUIMap _webpageServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region External UI Map

        public ExternalUIMap ExternalUIMap
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

        #endregion External Window UI Map

        #region Variables UI Map

        public VariablesUIMap VariablesUIMap
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

        #endregion Connect Window UI Map

        #endregion UI Maps
    }
}
