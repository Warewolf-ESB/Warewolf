using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Dev2.Studio.UI.Tests.UIMaps.DependencyGraphClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DatabaseServiceUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ConnectViewUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DeployViewUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.PluginServiceWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WebpageServiceWizardUIMapClasses;
using System.Diagnostics;
using System.Linq;
using System.IO;
using Dev2.CodedUI.Tests.UIMaps.VariablesUIMapClasses;
using System.Management;
using Microsoft.Win32;
using Dev2.CodedUI.Tests.UIMaps.ExternalUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.ServiceDetailsUIMapClasses;
using System.Threading;
using System.Security.Principal;
using Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.FeedbackUIMapClasses;


namespace Dev2.CodedUI.Tests
{

    /// Test Cases TO DO: 5 (v), 7 (vii), 8 (viii), 11 (xi) (xii - Resumption does not work)
    /// // vi done due to the ability to access items on the Workflow (WorkflowDesignerUIMap.cs)
    /// // xi limited by the inability to connect to other servers (Sashen + Server down at time of testing)
    /// <summary>
    /// Summary description for TestBase
    /// </summary>
    [CodedUITest]
    public class TestBase
    {
        // To bring up the generator utility, click in a method, and press
        // Cntrl+\, Cntrl+C

        public TestBase()
        {

        }

        // These run at the start of every test to make sure everything is sane
        [TestInitialize]
        public void CheckStartIsValid()
        {
            try
            {
                // Set default browser to IE for tests
                RegistryKey regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\shell\\Associations\\UrlAssociations\\http\\UserChoice", true);
                string browser = regkey.GetValue("Progid").ToString();
                if (browser != "IE.HTTP")
                {
                    regkey.SetValue("Progid", "IE.HTTP");
                }
            }
            catch (Exception ex)
            {
                // Some PC's don't have this value - We have to assume they only have IE :(
            }

            try
            {
                // Make sure no instances of IE are running (For Bug Test crashes)
                ExternalUIMap.CloseAllInstancesOfIE();
            }
            catch
            {
                throw new Exception("Error - Cannot close all instances of IE!");
            }

            bool toCheck = false;   // Disable this if you don't want the pre-test validation to occur.
            // Useful when creating / debugging tests
            if (toCheck)
            {
                // Make sure the Server has started
                try
                {
                    //Process[] processList = System.Diagnostics.Process.GetProcesses();
                    List<Process> findDev2Servers = System.Diagnostics.Process.GetProcesses().Where(p => p.ProcessName.Equals("Dev2.Server")).ToList();
                    int serverCounter = findDev2Servers.Count();
                    if (serverCounter != 1)
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
                        throw new Exception("Error - Unable to check if the Server has started!", ex.InnerException);
                    }
                }

                try
                {
                    // Make sure the Studio has started
                    var findDev2Studios = System.Diagnostics.Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName.StartsWith("Dev2.Studio"));
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
                            System.Threading.Thread.Sleep(2500);
                            Mouse.Click(new Point(zeTab.BoundingRectangle.X + 500, zeTab.BoundingRectangle.Y + 500));
                            System.Threading.Thread.Sleep(2500);
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
                    throw new Exception("Error - Unable to close tabs!", ex.InnerException);
                }
            }
        }
        // Comment
        [TestMethod]
        public void ThisMethodIsForTestingRandomTestFragments()
        {
            CreateCustomWorkflow("Bobby", "SomeCatHereYay");
            //DocManagerUIMap.ClickOpenTabPage("Variables");
            //string varName = VariablesUIMap.GetVariableName(0);
            //int j = 0;
            //CreateCustomWorkflow("Bobby", "SomeCatHereYay");
            //DocManagerUIMap.ClickOpenTabPage("Explorer");
            //ToolboxUIMap.clickToolboxItem("Calculate");
            //DocManagerUIMap.ClickOpenTabPage("Explorer");
            //ExplorerUIMap.RightClickDeleteProject("localhost", "WORKFLOWS", "DEMO", "DemoBootStrapAndRename");
            //this.DocManagerUIMap.ClickOpenTabPage("Explorer");
            //this.ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "DEMO", "DemoBootStrapAndRename");
            //UITestControl theTab = TabManagerUIMap.FindTabByName("5782Point2");
            //WorkflowDesignerUIMap.Adorner_ClickWizard(theTab, "CalculateTaxReturns");
            //double j = double.MaxValue;
            //UITestControl theTab = TabManagerUIMap.FindTabByName("CollapseTest");
            //UITestControl testFlowControl = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "TestFlow");


            //UITestControl theTab = TabManagerUIMap.FindTabByName("CollapseTest");
            //WorkflowDesignerUIMap.ClickExpandAll(theTab);

            /*
            //// Memory Leak Test
            while (true)
            {
                RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
                Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
                string windowTitle = WorkflowWizardUIMap.GetWorkflowWizardName();
                Assert.AreEqual("Workflow Service Details", windowTitle);
                bool isClosed = this.WorkflowWizardUIMap.CloseWizard();
                if (!isClosed)
                {
                    Assert.Fail("Unable to close wizard window");
                }
            }
            */
            //this.DocManagerUIMap.ClickOpenTabPage("Explorer");
            //this.ExplorerUIMap.DoRefresh();

            //UITestControl commentControl = ToolboxUIMap.FindToolboxItemByAutomationID("Comment");
            //DocManagerUIMap.ClickOpenTabPage("Toolbox");
            //this.UIMap.TabManagerIsNotNull();
            //TabManagerUIMap.CloseTab("Deploy Resources");
            //TabManagerUIMap.CloseTab("Deploy Resources");

            //UITestControl requiredTab = TabManagerUIMap.FindTabByName("PBI 6527");


            //DeployViewUIMap.doSomething(requiredTab);
            //this.UIMap.ClickLocalHost();

            //this.UIMap.sourceServerNotNull();


            //this.UIMap.TabPageExists();


            //UITestControl requiredTab = TabManagerUIMap.FindTabByName("PBI 6527");
            //UITestControl commentBox = WorkflowDesignerUIMap.FindControlByAutomationID(requiredTab, "Comment");
            //this.UIMap.splurtExists();
            //this.UIMap.Cake1Exists();
            //this.UIMap.Cake2Exists();

            //this.UIMap.RecordedMethod5();

            //this.UIMap.RecordedMethod4();

            //this.UIMap.RecordedMethod3();

            //this.UIMap.RecordedMethod2();
            //this.UIMap.RecordedMethod1();

            //this.UIMap.ContentPaneExists();
            //this.UIMap.MoveToWorkflowDesigner();

            //this.UIMap.WorkflowDesignerWindowExists(); // This does not work... ?
            //this.UIMap.UserControl_1Exists();
            //this.UIMap.scrollViewerExists();


            //this.UIMap.ActivityTypeDesignerExists();
            //this.UIMap.WorkflowItemPresenterExists();

        }


        #region Bugs

        // All bugs have been moved to BugTests.cs

        #endregion

        #region Feedback Tests

        [TestMethod]
        public void ClickHelpFeedback_Expected_FeedbackWindowOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Help", "Feedback");
            if (!FeedbackUIMap.DoesRecordedFeedbackWindowExist())
            {
                Assert.Fail("Error - Clicking the Feedback button does not create the Feedback Window");
            }

            SendKeys.SendWait("Y");
            Thread.Sleep(500);
            SendKeys.SendWait("{ENTER}");

            // Wait for the init, then click around a bit
            Thread.Sleep(2500);
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            Thread.Sleep(500);
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            Thread.Sleep(500);

            // Click stop, then make sure the Feedback window has appeared.
            FeedbackUIMap.ClickStartStopRecordingButton();
            Thread.Sleep(500);
            if (!FeedbackUIMap.DoesFeedbackWindowExist())
            {
                Assert.Fail("The Feedback window did not appear after the recording has been stopped.");
            }

            // Click Open default email
            FeedbackUIMap.FeedbackWindow_ClickOpenDefaultEmail();

            Thread.Sleep(2500);
            bool hasOutlookOpened = ExternalUIMap.Outlook_HasOpened();
            if (!hasOutlookOpened)
            {
                Assert.Fail("Outlook did not open when ClickOpenDefaultEmail was clicked!");
            }
            else
            {
                KillAllInstancesOf("OUTLOOK");
            }
        }

        #endregion Feedback Tests

        #region Add Server Tests

        // Bugs 8394 and related need to be fixed before this is done

        #endregion Add Server Tests

        #region Dev2ServiceDetails Wizard Tests

        // 1 Test for Standard Workflow Creation
        [TestMethod]
        public void StandardWorkflowCreation_Expected_WorkflowCreated()
        {
            // Create the Workflow
            CreateCustomWorkflow("StandardWorkflowCreation", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("StandardWorkflowCreation");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");

            // Click the start button - This shows the workflow has been created, and the Designer Surface has loaded
            Mouse.Click(theStartButton, new Point(10, 10));

            // Do Cleanup
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "StandardWorkflowCreation");
        }

        // 1 Test for Standard Workflow Properties (It should populate)
        [TestMethod]
        public void WorkflowProperties_Expected_PropertiesWindowPopulates()
        {
            // Create the Workflow
            //CreateCustomWorkflow("StandardWorkflowProperties", "CODEDUITESTCATEGORY");
            UITestControl theTab = TabManagerUIMap.FindTabByName("StandardWorkflowProperties");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");

            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickProperties("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "StandardWorkflowProperties");

            int attempts = 0;
            while (attempts < 2 && !ServiceDetailsUIMap.ServiceDetailsWindowExists())
            {
                System.Threading.Thread.Sleep(1000);
                attempts++;
            }
            System.Threading.Thread.Sleep(2500); // It's loaded - Let it initialise
            int j = 0;

            // Get a value here verifying it loaded correctly
        }

        // 1 Test for Database Service Creation
        [TestMethod]
        public void DatabaseCreation_Expected_DatabaseCreated()
        {
            // The Database Creation Wizard cannot pass the second step...
        }

        // 1 Test for Database Properties
        [TestMethod]
        public void DatabaseProperties_Expected_PropertiesWindowPopulates()
        {
            // Database property Window is missing the Category populate
        }

        // 1 Test for Plugin Service Creation
        [TestMethod]
        public void PluginServiceCreation_Expected_PluginServiceCreated()
        {
            // Created plugins end off as Workflows
        }

        // 1 Test for Plugin Properties (It should populate)
        [TestMethod]
        public void PluginProperties_Expects_PropertiesWindowPopulates()
        {
            // Plugin properties window gives Namespace Error
        }

        // 1 Test for Database Source Creation

        // 1 Test for Database Source Properties (It should populate)
        // 1 Test for Plugin Source Creation
        // 1 Test for Plugin Source Properties (It should populate)

        #endregion

        #region Test Case Backlog

        // Backlog 5378.1
        /*
        - MVR
            1. Drop a workflow activity onto a workflow (which must have input mapping).
            2. Modify the name of the workflow.
            3. Click on the Input Mapping tab.
            The tab must have exactly the same inputs.
         */
        [TestMethod]
        public void InputMappingTabMaintainsMappingWithNameChange()
        {
            CreateCustomWorkflow("5378Point1", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("5378Point1");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");

            // Get a point underneath the start button for the workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 150);

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Count the input mappings
            WorkflowDesignerUIMap.Adorner_ClickMapping(theTab, "CalculateTaxReturns");
            int adornerInputsBefore = WorkflowDesignerUIMap.Adorner_CountInputMappings(theTab, "CalculateTaxReturns");

            // And delete it
            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "CalculateTaxReturns");
            Mouse.Click(controlOnWorkflow, new Point(5, 5));
            SendKeys.SendWait("{DELETE}");

            // Drag it on again
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Change the name
            controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "CalculateTaxReturns");
            Mouse.Click(new Point(controlOnWorkflow.BoundingRectangle.Left + 100, controlOnWorkflow.BoundingRectangle.Top + 5));
            SendKeys.SendWait("Test123");

            // And re-count the input mappings
            WorkflowDesignerUIMap.Adorner_ClickMapping(theTab, "CalculateTaxReturns");
            int adornerInputsAfter = WorkflowDesignerUIMap.Adorner_CountInputMappings(theTab, "CalculateTaxReturns");
            if (adornerInputsBefore != adornerInputsAfter)
            {
                Assert.Fail("The adorner input count changed after a name change!");
            }

            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5378Point1");
        }

        // Backlog 5385.1
        /*
            - MG
            1.Open the studio and open a workflow.
            2.Copy the following text ""test@"" and paste it into the last blank textbox in the datalist tab.
            Essure that there is not exception thrown and that when moving off the textbox that the text box is highlighted in red and shows the error message for the invalid chars.
         */
        [TestMethod]
        public void ValidDatalistSearchTest()
        {

            // Create the workflow
            CreateCustomWorkflow("5385Point1", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("5385Point1");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");

            // Open the Variables tab, and enter the invalid value
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            SendKeys.SendWait("test@");

            // Click below to fire the validity check
            VariablesUIMap.ClickVariableName(1);

            // The box should be invalid, and have the tooltext saying as much.
            bool isValid = VariablesUIMap.CheckVariableIsValid(0);

            if (isValid)
            {
                Assert.Fail("The DataList accepted the invalid variable name.");
            }

            // Clean Up! \o/
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5385Point1");

        }

        // Backlog 5559.(1,2,3,4,5,6,7) - To Do
        /*
            5559.1
            - TF
            Replace NativationViewModel - Single enviroment : Replace NavigationViewModelTest :: RefreshMenuCommand

            5559.2
            - TF
            Replace NavigationItemViewModelTest.cs :: ConnectCommand

            5559.3
            - TF
            Replace NavigationItemViewModelTest.cs :: DisconnectCommand

            5559.4
            - TF
            Replace NavigationItemViewModelTest.cs :: RunCommand

            5559.5
            - TF
            Replace NavigationItemViewModelTest.cs :: HelpCommand

            5559.6
            - TF
            Replace NavigationItemViewModelTest.cs :: showDependencies

            5559.7
            - TF
            Replace ExplorerViewModelTest.cs :: Connect_ConnectCommand_ExpectAdd
          
            8,9,10,11 are being exempt for now (RE Sashen)
        // ^ - Are tests that need refactoring, and not actually Coded UI Tests
        */
        [TestMethod]
        public void RightClickMenuTests_Expected_NoErrors()
        {
            // Create a Workflow
            //CreateCustomWorkflow("RightClickMenuTests", "CodedUITestCategory");

            // 5559.1 - Refresh

            // Refresh to make it appear

            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();

            /*
            
            These two tests are no longer valid by specification!
            
            // 5559.3 - Disconnect
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.Server_RightClick_Disconnect("localhost");
            if (IsLocalServerConnected())
            {
                Assert.Fail("The server did not disconnect :(");
            }
            
             * // 5559.2 - Connect
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.Server_RightClick_Connect("localhost");
            if (!IsLocalServerConnected())
            {
                Assert.Fail("The server did not reconnect :(");
            }
            */

            // 5559.4 - Run -- ?

            // 5559.5 - Help
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickHelp("localhost", "WORKFLOWS", "JSON", "JSON Binder");

            // Make sure the help tab opened
            string tabName = TabManagerUIMap.GetTabNameAtPosition(0);

            if (tabName != "JSON Binder*Help")
            {
                Assert.Fail("The Right Click -> Help menu did not open the help tab!");
            }

            // Click in the middle of the screen to hide the "Explorer" Tab
            Mouse.Click(new Point(Screen.PrimaryScreen.WorkingArea.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2));
            System.Threading.Thread.Sleep(500); // Give it time to close
            TabManagerUIMap.CloseTab(tabName);

            // 5559.6 - ShowDependencies
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickShowProjectDependancies("localhost", "WORKFLOWS", "JSON", "JSON Binder");
            tabName = TabManagerUIMap.GetTabNameAtPosition(0);
            if (tabName != "JSON Binder*Dependencies")
            {
                Assert.Fail("The Right Click -> Show Dependencies menu did not open the dependencies tab!");
            }

            // Click in the middle of the screen to hide the "Explorer" Tab
            Mouse.Click(new Point(Screen.PrimaryScreen.WorkingArea.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2));
            System.Threading.Thread.Sleep(500); // Give it time to close
            TabManagerUIMap.CloseTab(tabName);

            // 5559.7 - Connect_ConnectCommand_ExpectAdd - Removed until new UI bit is mapped!
            /*
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();
            UITestControl theServer = ExplorerUIMap.GetServer("localhost");
            Mouse.Click(theServer, new Point(10, 10));
            SendKeys.SendWait("{LEFT}");
            ConnectToServer("tfsbld", "http://rsaklfsvrtfsbld:77/dsf");

            // And count! :D
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            int serversBefore = ExplorerUIMap.CountServers();
            ExplorerUIMap.ClickServerInServerDDL("tfsbld");
            int serversAfter = ExplorerUIMap.CountServers();
            int tryCount = 0;
            while ((serversBefore == serversAfter) && (tryCount < 25)) // If it's not connected within 25 seconds, it failed!
            {
                tryCount++;
                System.Threading.Thread.Sleep(1000);
                serversAfter = ExplorerUIMap.CountServers();
            }
            Assert.AreEqual(serversBefore + 1, serversAfter, "Connecting to a new server did not add it to the explorer list.");
            ExplorerUIMap.Server_RightClick_Delete("tfsbld");
            */

        }

        // 8,9,10,11 are being exempt for now (RE Sashen)
        // ^ - Are tests that need refactoring, and not actually Coded UI Tests

        // Backlog 5615.1
        /*
            - Matt
            Calc tooltip support for how data is to be entered when using a function
         */
        [TestMethod]
        public void TypeInCalcBox_Expected_TooltipAppears()
        {
            // Create the Workflow for the test
            CreateCustomWorkflow("5615Point1", "CodedUITestCategory");

            // For later
            UITestControl theTab = TabManagerUIMap.FindTabByName("5615Point1");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag a Calculate control on
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl calculateControl = ToolboxUIMap.FindToolboxItemByAutomationID("Calculate");
            ToolboxUIMap.DragControlToWorkflowDesigner(calculateControl, workflowPoint1);

            Mouse.Click();
            SendKeys.SendWait("sum{(}");

            // Find the control
            UITestControl calculateOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Calculate");

            // Find the fxBox - This seemed resilient to filter properties for some odd reason...
            WpfEdit fxBox = new WpfEdit(calculateOnWorkflow);
            //fxBox.FilterProperties.Add("AutomationId", "UI__fxtxt_AutoID");
            //fxBox.Find();

            UITestControlCollection boxCollection = fxBox.FindMatchingControls();
            int boxCounter = boxCollection.Count;
            WpfEdit realfxBox = new WpfEdit();
            foreach (WpfEdit theBox in boxCollection)
            {
                string autoID = theBox.AutomationId;
                if (autoID == "UI__fxtxt_AutoID")
                {
                    realfxBox = theBox;
                }
            }

            string helpText = realfxBox.GetProperty("Helptext").ToString();

            if (!helpText.Contains("sum(number{0}, number{N})") || (!helpText.Contains("Sums all the numbers given as arguments and returns the sum.")))
            {
                Assert.Fail("The tooltip for the Sum box does not appear.");
            }

            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5615Point1");
        }

        // Backlog 5679.1
        /*
            - Mo
            Auto bracketing in the MutiAssign left hand side intellisense box
        */
        [TestMethod]
        public void TypeTextInMutiAssignClickAway_Expected_BracketsAutoFilled()
        {
            // Create the Workflow
            CreateCustomWorkflow("5679Point1");

            // Get some data
            UITestControl theTab = TabManagerUIMap.FindTabByName("5679Point1");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag a Multi Assign on
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl asssignControlInToolbox = ToolboxUIMap.FindToolboxItemByAutomationID("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(asssignControlInToolbox, workflowPoint1);

            // Add some text
            UITestControl assignControlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Assign");
            WorkflowDesignerUIMap.AssignControl_ClickFirstTextbox(theTab, "Assign");
            SendKeys.SendWait("someVal");

            // Click away
            Mouse.Click(new Point(workflowPoint1.X + 50, workflowPoint1.Y + 50));

            // Get the value
            string text = WorkflowDesignerUIMap.AssignControl_GetVariableName(theTab, "Assign", 0);
            StringAssert.Contains(text, "[[someVal]]");

            // Clean up :D
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5679Point1");
        }

        // Backlog 5725.1
        /*
            - Mo
            Clicking through on the dependency viewer
        */
        [TestMethod]
        public void DependancyGraph_OnDoubleClickWorkFlow_Expected_NewTab()
        {
            var studioBugTests = new StudioBugTests();

            // Created by Ashley \o/
            studioBugTests.DependancyGraph_OnDoubleClickWorkFlow_Expected_NewTab();
        }

        // Backlog 5735.1
        /*
            - Brendon
            Drag source being registered as the item the mouse was over when dragging started instead of the item the mouse was over when the mouse button was pressed
         */
        [TestMethod]
        public void DragWorkflowWhilstCreating_Expected_WorkflowRenders()
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");

            // Get the Window
            WpfWindow theWindow = new WpfWindow();
            theWindow.SearchProperties[WpfWindow.PropertyNames.Name] = "Workflow Service Details";
            theWindow.SearchProperties[WpfWindow.PropertyNames.AutomationId] = "WebPropertyEditor";
            theWindow.Find();

            // Got the bottom right corner for some dragging fun!
            Point p = new Point(theWindow.BoundingRectangle.Left + theWindow.BoundingRectangle.Width - 5, theWindow.BoundingRectangle.Top + theWindow.BoundingRectangle.Height - 5);
            Point toDragTo = new Point(p.X - 100, p.Y - 100);
            Mouse.Click(p);
            for (int j = 0; j < 25; j++)
            {
                Mouse.StartDragging();
                Mouse.StopDragging(-50, -50);
                Mouse.StartDragging();
                Mouse.StopDragging(p);
                Point clickPoint = new Point();
                if (!theWindow.TryGetClickablePoint(out clickPoint))
                {
                    Assert.Inconclusive("It died");
                    break;
                }
            }
        }

        // Backlog 5772.1
        /*
            - Mo
            Studio hangs on add and remove missing/unused datalist items
        */
        [TestMethod]
        public void AddLargeAmountsOfDataListItems_Expected_NoHanging()
        {
            // Create the workflow
            CreateCustomWorkflow("5772Point1", "CodedUITestCategory");

            // Get some variables
            UITestControl theTab = TabManagerUIMap.FindTabByName("5772Point1");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationID("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            // Add the data!
            WorkflowDesignerUIMap.AssignControl_ClickFirstTextbox(theTab, "Assign");
            for (int j = 0; j < 100; j++)
            {
                // Sleeps are due to the delay when adding a lot of items
                SendKeys.SendWait("[[theVar" + j.ToString() + "]]");
                Thread.Sleep(50);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(50);
                SendKeys.SendWait(j.ToString());
                Thread.Sleep(50);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(50);
            }

            // And map!
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.UpdateDataList();

            // All good - Cleanup time!
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5772Point1");
        }

        // Backlog 5782.1 (TWR Version)
        /*
            - TWR
            1. Open a workflow
            2. Add a new variable
            3. Click View in Browser
            4. Check that the new variable has been added to the DataList (without any dialog popups)
         */
        [TestMethod]
        public void ViewInBrowser_Expected_NewlyCreatedVariableAddedToDataList()
        {
            // Create the workflow
            CreateCustomWorkflow("5782Point1", "CodedUITestCategory");

            // Add the variable
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            SendKeys.SendWait("testVar");

            // Click "View in Browser"
            RibbonUIMap.ClickRibbonMenuItem("Home", "View in Browser");

            // Give the slow IE time to open ;D
            System.Threading.Thread.Sleep(2500);

            // Check if the IE Body contains the data list item
            string IEText = ExternalUIMap.GetIEBodyText();
            if (!IEText.Contains("<testVar />"))
            {
                Assert.Fail("The variable was not added to the DataList :(");
            }

            // Close the browser
            ExternalUIMap.CloseAllInstancesOfIE();

            // And do cleanup
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5782Point1");
        }

        // Backlog 5782.1 (Mo Version)
        /*
            - Mo
            Open a workflow with a recordset in its datalist and hit F5 or debug, then set the focus to the recordsets first row and hit shift+enter and observe that a new row is added.
         */
        [TestMethod]
        public void DebugDataTyped_Expected_NewRowIsAdded()
        {
            // Create the Workflow
            CreateCustomWorkflow("5782Point1Mo");

            // Vars
            UITestControl theTab = TabManagerUIMap.FindTabByName("5782Point1Mo");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag an Assign onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theItem = ToolboxUIMap.FindControl("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theItem, workflowPoint1);

            // Fill some data
            WorkflowDesignerUIMap.AssignControl_ClickFirstTextbox(theTab, "Assign");
            SendKeys.SendWait("[[recSet{(}{)}.Name]]");
            Thread.Sleep(100); // Wait bug if you type too fast
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(100);
            SendKeys.SendWait("myName");

            // Map it
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.UpdateDataList();

            RibbonUIMap.ClickRibbonMenuItem("Home", "Debug");
            if (DebugUIMap.CountRows() != 1)
            {
                DebugUIMap.CloseDebugWindow();
                Assert.Fail("There are no rows!");
            }
            DebugUIMap.ClickItem(0);
            SendKeys.SendWait("test123");
            int newCount = DebugUIMap.CountRows();
            DebugUIMap.CloseDebugWindow();
            if (newCount != 2)
            {
                Assert.Fail("There was no added row!");
            }

            // Cleanup
            //DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5782Point2Mo");

        }

        // Backlog 5782.2 (TWR Version)
        /*
            - TWR
            1. Open a workflow
            2. Make a change
            3. Click View in Browser
            4. Check that the change has been persisted to the workflow file in the workspace folder: \{ServerRoot}\Workspaces\{Workspace GUID}\Services\
            5. Check that the change has NOT been persisted to the workflow file in the server folder: \{ServerRoot}\Services\
         */
        [TestMethod]
        public void ViewInBrowserIsClicked_Expected_OnlyRelevantWorkflowFilesSaved()
        {
            // 1. Create the workflow
            CreateCustomWorkflow("5782Point2TWR", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("5782Point2TWR");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            System.Threading.Thread.Sleep(1000);

            // Get the data for later comparison
            string folder = GetServerEXEFolder();
            string workspaceID = GetWorkspaceID();
            string path1 = folder + @"Workspaces\" + workspaceID + @"\Services\5782Point2TWR.xml";
            string path2 = folder + @"Services\5782Point2TWR.xml";
            StreamReader sr1 = new StreamReader(path1);
            StreamReader sr2 = new StreamReader(path2);
            string fileData1 = sr1.ReadToEnd();
            string fileData2 = sr2.ReadToEnd();
            sr1.Close();
            sr2.Close();

            // 2. Make a change
            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // 3. Click View in Browser
            RibbonUIMap.ClickRibbonMenuItem("Home", "View in Browser");

            // Re-read the data to check for changes
            sr1 = new StreamReader(path1);
            sr2 = new StreamReader(path2);
            string newFileData1 = sr1.ReadToEnd();
            string newFileData2 = sr2.ReadToEnd();
            sr1.Close();
            sr2.Close();

            // Make sure that the data WAS altered in the first file
            Assert.AreEqual(fileData1, newFileData1, "Error - The data is equal in case 1");

            // Make sure that the data WAS NOT altered in the second file
            Assert.AreEqual(fileData2, newFileData2, "Error - The data is not equal in case 2");

            // Close the browser
            ExternalUIMap.CloseAllInstancesOfIE();

            // Finally - Clean Up
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5782Point2TWR");
        }

        // Backlog 5782.2 (Mo Version) + 5782.3 (Mo Version)
        // Note: Bug 8018 and Bug 8363 can sometimes cause this test to fail

        /*
            - Mo
            Open a workflow with a recordset in its datalist and hit F5 or debug, then set the focus to the recordsets first row and hit shift+enter and observe that a new row is added.
         */
        [TestMethod]
        public void DebugRowAdded_Expected_NewRowIsAdded()
        {
            // Create the Workflow
            CreateCustomWorkflow("5782Point2Mo");

            // Vars
            UITestControl theTab = TabManagerUIMap.FindTabByName("5782Point2Mo");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag an Assign onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theItem = ToolboxUIMap.FindControl("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theItem, workflowPoint1);

            // Fill some data
            WorkflowDesignerUIMap.AssignControl_ClickFirstTextbox(theTab, "Assign");
            SendKeys.SendWait("[[recSet{(}{)}.Name]]");
            Thread.Sleep(100); // Wait bug if you type too fast
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(100);
            SendKeys.SendWait("myName");

            // Map it
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.UpdateDataList();

            RibbonUIMap.ClickRibbonMenuItem("Home", "Debug");
            int tries = 0;
            while (!DebugUIMap.DebugWindowExists() && tries < 5)
            {
                Thread.Sleep(1000);
            }
            if (!DebugUIMap.DebugWindowExists())
            {
                Assert.Fail("The Debug Window did not open!");
            }

            if (DebugUIMap.CountRows() != 1)
            {
                DebugUIMap.CloseDebugWindow();
                Assert.Fail("There are no rows!");
            }
            DebugUIMap.ClickItem(0);
            SendKeys.SendWait("+{ENTER}");
            int newCountAdded = DebugUIMap.CountRows();

            SendKeys.SendWait("+{DELETE}");
            int newCountDeleted = DebugUIMap.CountRows();

            DebugUIMap.CloseDebugWindow();
            if (newCountAdded != 2)
            {
                Assert.Fail("There was no added row!");
            }
            if (newCountDeleted != 1)
            {
                Assert.Fail("The row was not deleted!");
            }

            // Cleanup
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5782Point2Mo");

        }

        // Backlog 5782.3 (TWR Version) 
        /*
            - TWR
            1. Open a workflow
            2. Make a change
            3. Click Save
            4. Check that the change has been persisted to the workflow file in the workspace folder: \{ServerRoot}\Workspaces\{Workspace GUID}\Services\
            5. Check that the change has been persisted to the workflow file in the server folder: \{ServerRoot}\Services\
         */
        [TestMethod]
        public void ClickingSave_Expected_WorkflowFilesAreUpdated()
        {
            // 1. Create the workflow
            CreateCustomWorkflow("5782Point3", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("5782Point3");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            System.Threading.Thread.Sleep(1000);

            // Get the data for later comparison
            string folder = GetServerEXEFolder();
            string workspaceID = GetWorkspaceID();
            string path1 = folder + @"Workspaces\" + workspaceID + @"\Services\5782Point3.xml";
            string path2 = folder + @"Services\5782Point3.xml";
            StreamReader sr1 = new StreamReader(path1);
            StreamReader sr2 = new StreamReader(path2);
            string fileData1 = sr1.ReadToEnd();
            string fileData2 = sr2.ReadToEnd();
            sr1.Close();
            sr2.Close();

            // 2. Make a change
            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            System.Threading.Thread.Sleep(1000);

            // Re-read the data
            sr1 = new StreamReader(path1);
            sr2 = new StreamReader(path2);
            string newFileData1 = sr1.ReadToEnd();
            string newFileData2 = sr2.ReadToEnd();
            sr1.Close();
            sr2.Close();

            // And make sure that the data WAS altered
            Assert.AreNotEqual(fileData1, newFileData1, "Error - The data is equal in case 1");
            Assert.AreNotEqual(fileData2, newFileData2, "Error - The data is equal in case 2");

            // Finally - Clean Up
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5782Point3");
        }

        // Backlog 5782.4 (TWR Version) 
        /*
            - TWR
            1. Open a workflow
            2. Make a change
            3. Close the tab
            4. Check that a Save Changes dialog pops up
         */
        [TestMethod]
        public void CloseTabWithUnsavedChanges_Expected_SaveChangesDialogAppears()
        {
            // 1. Create the workflow
            CreateCustomWorkflow("5782Point4", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("5782Point4");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            System.Threading.Thread.Sleep(1000);

            // 2. Make a change
            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Close the tab and click No (If you can click No, it means the box popped up :p)
            string tabName = TabManagerUIMap.GetTabNameAtPosition(0);
            TabManagerUIMap.CloseTab_Click_No(tabName);

            // All good - Clean up time!
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5782Point4");

        }

        // Backlog 5782.4 (Mo Version) + 5782.5 (Mo Version) - To Do (Blocked by lack of AutomationID for XML Text Box - Mo has been notified)
        /*
            - MG
            Open a workflow with recordsets and scalars in its datalist and hit F5 or debug
            Enter data into all the fields and click on the XML tab then enter more data into the XML and switch back to the inputs tab and notice that it has updated.
         */
        [TestMethod]
        public void DebugTabUpdatesWhenXMLIsModified()
        {
            /*
            // Create the workflow
            //CreateCustomWorkflow("5782Point4Mo", "CodedUITestCategory");

            // Set some variabes
            UITestControl theTab = TabManagerUIMap.FindTabByName("5782Point4Mo");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 150);

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Toolbox");

            // Get an assign Tool
            UITestControl assignTool = ToolboxUIMap.FindControl("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(assignTool, workflowPoint1);

            // Enter two record sets and 2 scalars
            WorkflowDesignerUIMap.AssignControl_ClickFirstTextbox(theTab, "Assign");
            SendKeys.SendWait("[[recSet{(}{)}.Name]]");
            Thread.Sleep(250);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(250);
            SendKeys.SendWait("Michael");
            Thread.Sleep(250);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(250);
            SendKeys.SendWait("[[recSet{(}{)}.Surname]]");
            Thread.Sleep(250);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(250);
            SendKeys.SendWait("Cullen");
            Thread.Sleep(250);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(250);
            SendKeys.SendWait("[[recSet2{(}{)}.SomeVal]]");
            Thread.Sleep(250);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(250);
            SendKeys.SendWait("SomeData");
            Thread.Sleep(250);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(250);
            SendKeys.SendWait("[[scalarOne]]");
            Thread.Sleep(250);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(250);
            SendKeys.SendWait("SOData");
            Thread.Sleep(250);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(250);
            SendKeys.SendWait("[[scalarTwo]]");
            Thread.Sleep(250);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(250);
            SendKeys.SendWait("STData");

            // Update the DataList
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.UpdateDataList();
            
            // Open the Debug Menu, and enter some values
            RibbonUIMap.ClickRibbonMenuItem("Home", "Debug");
            DebugUIMap.ClickItem(0);
            SendKeys.SendWait("soValue");
            DebugUIMap.ClickItem(1);
            SendKeys.SendWait("stValue");
            DebugUIMap.ClickItem(2);
            SendKeys.SendWait("rsoName");
            DebugUIMap.ClickItem(3);
            SendKeys.SendWait("rsoSurname");

            // 6, because some rows should have been auto-added
            DebugUIMap.ClickItem(6);
            SendKeys.SendWait("rstValue");

            // Change to the XML tab, and make sure everything's OK
            DebugUIMap.ClickXMLTab();

            // Rest of test blocked by lack of Automation ID
             */
        }

        // Backlog 5782.5 (TWR Version) 
        /*
            - TWR
            1. Open a workflow
            2. Make a change
            3. Close the tab
            4. Click Yes in the Save Changes dialog
            5. Check that the change has been persisted to the workflow file in the workspace folder: \{ServerRoot}\Workspaces\{Workspace GUID}\Services\
            6. Check that the change has been persisted to the workflow file in the server folder: \{ServerRoot}\Services\
         */
        [TestMethod]
        public void CloseNonSavedWorkflowClickYes_Expected_ChangesPersist()
        {
            // 1. Create the workflow
            CreateCustomWorkflow("5782Point5", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("5782Point5");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            System.Threading.Thread.Sleep(1000);

            // Get the data for later comparison
            string folder = GetServerEXEFolder();
            string workspaceID = GetWorkspaceID();
            string path1 = folder + @"Workspaces\" + workspaceID + @"\Services\5782Point5.xml";
            string path2 = folder + @"Services\5782Point5.xml";
            StreamReader sr1 = new StreamReader(path1);
            StreamReader sr2 = new StreamReader(path2);
            string fileData1 = sr1.ReadToEnd();
            string fileData2 = sr2.ReadToEnd();
            sr1.Close();
            sr2.Close();

            // 2. Make a change
            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            string tabName = TabManagerUIMap.GetTabNameAtPosition(0);
            // 3. Close the tab, and click Yes
            TabManagerUIMap.CloseTab_Click_Yes("5782Point5");

            // Check that no tabs remain open
            int tabCount = TabManagerUIMap.GetTabCount();
            Assert.AreEqual(tabCount, 0, "Error - Clicking No kept the tab open.");

            // Re-read the data
            sr1 = new StreamReader(path1);
            sr2 = new StreamReader(path2);
            string newFileData1 = sr1.ReadToEnd();
            string newFileData2 = sr2.ReadToEnd();
            sr1.Close();
            sr2.Close();

            // And make sure that the data WAS altered
            Assert.AreNotEqual(fileData1, newFileData1, "Error - The data is equal in case 1");
            Assert.AreNotEqual(fileData2, newFileData2, "Error - The data is equal in case 2");

            // Finally - Clean Up
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5782Point5");
        }

        // Backlog 5782.6 (TWR Version) 
        /*
            - TWR
            1. Open a workflow
            2. Make a change
            3. Close the tab
            4. Click No in the Save Changes dialog
            5. Check that the change has NOT been persisted to the workflow file in the workspace folder: \{ServerRoot}\Workspaces\{Workspace GUID}\Services\
            6. Check that the change has NOT been persisted to the workflow file in the server folder: \{ServerRoot}\Services\
         */
        [TestMethod]
        public void CloseNonSavedWorkflowClickNo_Expected_ChangesDoNotPersist()
        {
            // 1. Create the workflow
            CreateCustomWorkflow("5782Point6", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("5782Point6");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            System.Threading.Thread.Sleep(1000);

            // Get the data for later comparison
            string folder = GetServerEXEFolder();
            string workspaceID = GetWorkspaceID();
            string path1 = folder + @"Workspaces\" + workspaceID + @"\Services\5782Point6.xml";
            string path2 = folder + @"Services\5782Point6.xml";
            StreamReader sr1 = new StreamReader(path1);
            StreamReader sr2 = new StreamReader(path2);
            string fileData1 = sr1.ReadToEnd();
            string fileData2 = sr2.ReadToEnd();
            sr1.Close();
            sr2.Close();

            // 2. Make a change
            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            string tabName = TabManagerUIMap.GetTabNameAtPosition(0);
            // 3. Close the tab, and click No
            TabManagerUIMap.CloseTab_Click_No("5782Point6");

            // Check that no tabs remain open
            int tabCount = TabManagerUIMap.GetTabCount();
            Assert.AreEqual(tabCount, 0, "Error - Clicking No kept the tab open.");

            // Re-read the data
            sr1 = new StreamReader(path1);
            sr2 = new StreamReader(path2);
            string newFileData1 = sr1.ReadToEnd();
            string newFileData2 = sr2.ReadToEnd();
            sr1.Close();
            sr2.Close();

            // And make sure that nothing was altered
            Assert.AreEqual(fileData1, newFileData1, "The data is not equal in case 1");
            Assert.AreEqual(fileData2, newFileData2, "The data is not equal in case 2");

            // Finally - Clean Up
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5782Point6");
        }

        // Backlog 5782.7 (TWR Version) 
        /*
            - TWR
            1. Open a workflow
            2. Make a change
            3. Close the tab
            4. Click Cancel in the Save Changes dialog
            5. Check that the dialog closes
            6. Check that the workflow tab is open
            7. Check that the change has NOT been persisted to the workflow file in the workspace folder: \{ServerRoot}\Workspaces\{Workspace GUID}\Services\
            8. Check that the change has NOT been persisted to the workflow file in the server folder: \{ServerRoot}\Services\
         */
        [TestMethod]
        public void CloseNonSavedWorkflowClickCancel_Expected_ChangesDoNotPersist()
        {
            // 1. Create the workflow
            CreateCustomWorkflow("5782Point7", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("5782Point7");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            System.Threading.Thread.Sleep(1000);

            // Get the data for later comparison
            string folder = GetServerEXEFolder();
            string workspaceID = GetWorkspaceID();
            string path1 = folder + @"Workspaces\" + workspaceID + @"\";
            string path2 = folder + @"Services\5782Point7.xml";

            // Pre 

            path1 = path1 + @"\Services\5782Point7.xml";
            StreamReader sr1 = new StreamReader(path1);
            StreamReader sr2 = new StreamReader(path2);
            string fileData1 = sr1.ReadToEnd();
            string fileData2 = sr2.ReadToEnd();
            sr1.Close();
            sr2.Close();

            // 2. Make a change
            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            string tabName = TabManagerUIMap.GetTabNameAtPosition(0);
            // 3. Close the tab, and click Cancel
            TabManagerUIMap.CloseTab_Click_Cancel("5782Point7");

            // Check that the workflow tab is still open
            string newTabName = TabManagerUIMap.GetTabNameAtPosition(0);
            if (tabName != newTabName)
            {
                Assert.Fail("Clicking Cancel closed the tab :(");
            }

            // Re-read the data
            sr1 = new StreamReader(path1);
            sr2 = new StreamReader(path2);
            string newFileData1 = sr1.ReadToEnd();
            string newFileData2 = sr2.ReadToEnd();
            sr1.Close();
            sr2.Close();

            // And make sure that nothing was altered
            Assert.AreEqual(fileData1, newFileData1, "The data is not equal in case 1");
            Assert.AreEqual(fileData2, newFileData2, "The data is not equal in case 2");

            // Finally - Clean Up
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5782Point7");
        }

        // Backlog 5782.8 (TWR Version) AND 6166.1 (Duplicates apart from a simple additional tab check)
        /*
            5782.8
            - TWR
            1. Open 3 workflows
            2. Close the studio
            3. Open the studio
            4. Check that the 3 workflows and the start page tabs are open
            5. Check that the workflow tabs are in the same order
        
            6166.1
            - MG
            1.Open the studio and open three different workflows.
            2. Drag the first tab to be the second tab.
            3. Close the studio.
            4. Open the studio.
            The tab must be in the same order as when the studio was closed.
         */
        [TestMethod]
        public void CloseAndReopenStudio_Expected_TabOrderRemainsConstant()
        {
            // Open 3 different workflows
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "JSON", "JSON Binder");
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "JSON", "JSON Binder Clean");
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "JSON", "JSON Binder Raw");

            // Drag the first tab to the second position
            // Due to our ... Odd UI Design, this is accomplished by dragging it onto the third tab
            TabManagerUIMap.DragTabToTab("JSON Binder Raw", "JSON Binder");
            string fileName = GetStudioEXELocation();

            // Close the Studio (Don't kill it)
            WpfWindow theStudio = new WpfWindow();
            theStudio.SearchProperties[WpfWindow.PropertyNames.Name] = TestBase.GetStudioWindowName();
            theStudio.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            theStudio.WindowTitles.Add(TestBase.GetStudioWindowName());
            theStudio.Find();
            Point closeButtonPoint = new Point(theStudio.BoundingRectangle.Left + theStudio.BoundingRectangle.Width - 10, theStudio.BoundingRectangle.Top + 10);
            Mouse.Click(closeButtonPoint);
            System.Threading.Thread.Sleep(2000); // Give it time to die

            // Restart the studio!
            System.Diagnostics.Process.Start(fileName);

            // Get the tab names
            string tab1 = TabManagerUIMap.GetTabNameAtPosition(0);
            string tab2 = TabManagerUIMap.GetTabNameAtPosition(1);
            string tab3 = TabManagerUIMap.GetTabNameAtPosition(2);
            string tab4 = TabManagerUIMap.GetTabNameAtPosition(3);

            // Expected position - JSON Binder Clean, JSON Binder Raw, JSON Binder
            StringAssert.Contains(tab1, "JSON Binder Clean");
            StringAssert.Contains(tab2, "JSON Binder Raw");
            StringAssert.Contains(tab3, "JSON Binder");

            // 5782.8 requires a check to make sure the Start Page has also opened
            StringAssert.Contains(tab4, "Start Page");
        }

        // Backlog 5792.1 AND 5792.2
        /*
            5792.1
            - Brendon
            When dragging a workflow from the explorer into a foreach or sequence it would come in as a blank DsfActivity
            
            5792.2
            - Brendon
            When dragging a workflow or multi assign out of a for each and connecting it, it would indicate an error
         */
        [TestMethod]
        public void DragAWorkflowIntoAndOutOfAForEach_Expected_NoErrors()
        {
            CreateCustomWorkflow("5792Point2", "CodedUITestCategory");

            System.Threading.Thread.Sleep(1000);
            UITestControl theTab = TabManagerUIMap.FindTabByName("5792Point2");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");

            // Get a point underneath the start button for each workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag a ForEach onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl tcForEach = ToolboxUIMap.FindToolboxItemByAutomationID("ForEach");
            ToolboxUIMap.DragControlToWorkflowDesigner(tcForEach, workflowPoint1);

            // Get a sample workflow, and drag it onto the "Drop Activity Here" part of the ForEach box
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, new Point(workflowPoint1.X + 25, workflowPoint1.Y + 25));

            // Wait for the ForEach thing to do its init-y thing
            System.Threading.Thread.Sleep(1500);

            // And click below the tab to get us back to the normal screen
            Mouse.Move(new Point(theTab.BoundingRectangle.X + 50, theTab.BoundingRectangle.Y + 50));
            Mouse.Click();

            // Now - Onto Part 2!

            // 5792.2

            // Get the location of the ForEach box
            UITestControl forEachControl = workflowDesignerUIMap.FindControlByAutomationID(theTab, "ForEach");

            // Move the mouse to the contained CalculateTaxReturns box
            Mouse.Move(new Point(forEachControl.BoundingRectangle.X + 25, forEachControl.BoundingRectangle.Y + 75));

            // Click it
            Mouse.Click();

            // And drag it down
            Mouse.StartDragging();
            Mouse.StopDragging(new Point(workflowPoint1.X, workflowPoint1.Y + 100));

            // Now get its position
            UITestControl calcTaxReturnsControl = workflowDesignerUIMap.FindControlByAutomationID(theTab, "CalculateTaxReturns");
            try
            {
                Point p = calcTaxReturnsControl.GetClickablePoint();
            }
            catch
            {
                Assert.Fail("No clickable point is acheivable, so it was not dragged out :(");
            }


        }

        // Backlog 6664.1
        /*
            - Brendon
            1. Open the studio.
            2. Open a workflow with 2 or more workflows or worker services on it.
            3. Click the expand all button. Observe all items are expanded.
            4. Click the restore button. Observe all items are collapsed.
            5. Click the expand all button. Observe all items are expanded.
            6. Click the collapse all button. Observe all items are collapsed.
         */
        [TestMethod]
        public void AdornerMultipleCollapseExpand_Expected_NoErrors()
        {
            CreateCustomWorkflow("CollapseTest", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("CollapseTest");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");

            // Get a point underneath the start button for each workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);
            Point workflowPoint2 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 400);

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get sample workflows
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");
            UITestControl testLogic = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestLogic");

            // Drag it onto each point
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);
            ExplorerUIMap.DragControlToWorkflowDesigner(testLogic, workflowPoint2);

            // Due to a unique technicality, we have to click the "Start" node
            UITestControl startNode = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Mouse.Click(startNode, new Point(5, 5));

            // Click the expand all button
            WorkflowDesignerUIMap.ClickExpandAll(theTab);

            // Observe all items are expanded.
            if (!WorkflowDesignerUIMap.IsAdornerVisible(theTab, "TestFlow") || !WorkflowDesignerUIMap.IsAdornerVisible(theTab, "TestLogic"))
            {
                Assert.Fail("One of the adorners is not visible!");
            }

            // Click the "Restore" button
            WorkflowDesignerUIMap.ClickRestore(theTab);

            // Observe all items are collapsed.
            if (WorkflowDesignerUIMap.IsAdornerVisible(theTab, "TestFlow") || WorkflowDesignerUIMap.IsAdornerVisible(theTab, "TestLogic"))
            {
                Assert.Fail("One of the adorners is visible!");
            }

            // Click the expand all button.
            WorkflowDesignerUIMap.ClickExpandAll(theTab);

            // Observe all items are expanded.
            if (!WorkflowDesignerUIMap.IsAdornerVisible(theTab, "TestFlow") || !WorkflowDesignerUIMap.IsAdornerVisible(theTab, "TestLogic"))
            {
                Assert.Fail("One of the adorners is not visible!");
            }

            // Click the collapse all button.
            WorkflowDesignerUIMap.ClickCollapseAll(theTab);

            // Observe all items are collapsed.
            if (WorkflowDesignerUIMap.IsAdornerVisible(theTab, "TestFlow") || WorkflowDesignerUIMap.IsAdornerVisible(theTab, "TestLogic"))
            {
                Assert.Fail("One of the adorners is visible!");
            }

            // Test complete - Delete itself
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "CollapseTest");
        }

        // Backlog 6601 - To Do
        /*
            - Brendon
            6601.1 - Given the user has clicked on Help -> Feedback (Destination) then the start feedback button is highlighted to the user.
            6601.2 - Given that the user starts feedback a feedback session, if Windows Steps App is present then recording feedback session is started, otherwise an email feedback session is started.
            6601.3 - Given that the user has finished the feedback session then a feedback email will be generated as per part 1 of this PBI with the relevant files attached to it.
         */
        [TestMethod]
        public void Feedback_Expected_Tests()
        {

        }

        // Backlog 6664.2 - To Do
        /*
            - Brendon
            1. Open the studio.
            2. Open a workflow another workflow on it.
            3. Select the workflow, observe the adorners show.
            4. Click the show mapping button, observe the input output adorner shows.
               If there are 12 rows then all 12 should show, if there are more than 12 only 10 should show.
         */
        [TestMethod]
        public void ClickShowMapping_Expected_InputOutputAdornersAreDisplayed()
        {
            CreateCustomWorkflow("6664Point2", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("6664Point2");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");

            // Get a point underneath the start button for the workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Click it
            UITestControl controlOnWorkflow = workflowDesignerUIMap.FindControlByAutomationID(theTab, "TestFlow");
            Mouse.Click(controlOnWorkflow, new Point(5, 5));
        }

        // Backlog 6664.3
        /*
            - Brendon
            1. Open the studio.
            2. Open a workflow another workflow (the workflow on the design surface must have a wizard) on it.
            3. Select the workflow, observe the adorners show.
            4. Click the wizzard button, observe the wizard overlay shows.
         */
        [TestMethod]
        public void OpenAdornerWizardOverlay_Expected_AdornerWizardOverlayShows()
        {
            CreateCustomWorkflow("6664Point3", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("6664Point3");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");

            // Get a point underneath the start button for the workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);


            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Click it
            UITestControl controlOnWorkflow = workflowDesignerUIMap.FindControlByAutomationID(theTab, "CalculateTaxReturns");
            Mouse.Click(controlOnWorkflow, new Point(5, 5));

            // Make sure the adorner is visible
            if (!WorkflowDesignerUIMap.IsAdornerVisible(theTab, "CalculateTaxReturns"))
            {
                Assert.Fail("The adorner is not visible!");
            }

            WorkflowDesignerUIMap.Adorner_ClickWizard(theTab, "CalculateTaxReturns");
            string theTitle = WorkflowDesignerUIMap.GetWizardTitle(theTab);
            StringAssert.Contains(theTitle, "Setup: CalculateTaxReturns", "Error - The Wizard did not appear!");

            // Test complete - Delete itself
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "6664Point3");

        }

        // Backlog 6664.4
        /*
            - Brendon
            1. Open the studio.
            2. Open a workflow another workflow on it.
            3. Select the workflow, observe the adorners show.
            4. Resize the input outout mapping, observe the resize working correctly.
         */
        [TestMethod]
        public void ResizeAdornerMappings_Expected_AdornerMappingIsResized()
        {

            CreateCustomWorkflow("6664Point4", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("6664Point4");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");

            // Get a point underneath the start button for the workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 100);

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Click it
            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "CalculateTaxReturns");
            Mouse.Click(controlOnWorkflow, new Point(5, 5));
            WorkflowDesignerUIMap.Adorner_ClickMapping(theTab, "CalculateTaxReturns");
            controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "CalculateTaxReturns");
            UITestControlCollection controlCollection = controlOnWorkflow.GetChildren();

            Point initialResizerPoint = new Point();
            Point newResizerPoint = new Point();
            // Validate the assumption that the last child is the resizer
            if (controlCollection[controlCollection.Count - 1].ControlType.ToString() == "Indicator")
            {
                UITestControl theResizer = controlCollection[controlCollection.Count - 1];
                initialResizerPoint.X = theResizer.BoundingRectangle.X + 5;
                initialResizerPoint.Y = theResizer.BoundingRectangle.Y + 5;
            }

            // Drag
            Mouse.Click(initialResizerPoint);
            Mouse.StartDragging();

            // Y - 50 since it starts at the lowest point
            Mouse.StopDragging(new Point(initialResizerPoint.X + 50, initialResizerPoint.Y - 50));

            // Check position to see it dragged
            if (controlCollection[controlCollection.Count - 1].ControlType.ToString() == "Indicator")
            {
                UITestControl theResizer = controlCollection[controlCollection.Count - 1];
                newResizerPoint.X = theResizer.BoundingRectangle.X + 5;
                newResizerPoint.Y = theResizer.BoundingRectangle.Y + 5;
            }

            if (!(newResizerPoint.X > initialResizerPoint.X) || !(newResizerPoint.Y < initialResizerPoint.Y))
            {
                Assert.Fail("The control was not resized properly.");
            }

            // Test complete - Delete itself
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "6664Point4");
        }

        // Backlog 8074
        /*
            - Brendon
            Please add a test for dragging a workflow onto another workflow.
         */
        [TestMethod]
        public void DragWorkflowOntoAnotherWorkflow_Expected_DraggedWorkflowHasAClickablePoint()
        {
            // Create the Workflow
            CreateCustomWorkflow("8074");

            // Get a point for later
            UITestControl theTab = TabManagerUIMap.FindTabByName("8074");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Open the explorer, and get another Workflo
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns", workflowPoint1);

            // See if it's there
            UITestControl theWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "CalculateTaxReturns");
            Point p = new Point();
            if (!theWorkflow.TryGetClickablePoint(out p))
            {
                Assert.Fail("Error - Could not get a clickable point after dragging a workflow onto another workflow!");
            }

            // Test has passed - Deleted the newly created Workflow
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "8074");
        }

        // Backlog 8111
        /*
            - Brendon
            1. Delete %LOCALAPPDATA%\Dev2\UserInterfaceLayouts\Default.xml.
            2. Open the studio.
            3. Close the Studio.
            4. Check %LOCALAPPDATA%\Dev2\UserInterfaceLayouts\Default.xml exists.
         */
        [TestMethod]
        public void DeleteDefaultFileRestartStudio_Expected_FileExistsOnRecreate()
        {
            // A test case with an identity crisis! :D
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string defaultFile = localAppData + @"\Dev2\UserInterfaceLayouts\Default.xml";
            if (!File.Exists(defaultFile))
            {
                Assert.Fail("The file does not exist in the first place!");
            }

            string studioPath = GetStudioEXELocation();
            CloseTheStudio();
            File.Delete(defaultFile);
            System.Threading.Thread.Sleep(100); // Time to delete
            if (File.Exists(defaultFile))
            {
                Assert.Fail("The file could not be deleted!");
            }
            System.Diagnostics.Process.Start(studioPath);

            // Wait for it to open
            Thread.Sleep(5000);

            // Aaaand re-close it, since the file should have now been created!
            CloseTheStudio();

            if (!File.Exists(defaultFile))
            {
                Assert.Fail("The file was not recreated!");
            }

            // Test over - Re-open the Studio D:
            System.Diagnostics.Process.Start(studioPath);

            // Wait for it to open
            Thread.Sleep(5000);
        }

        // Bug 7854
        /*
            - Massimo
            1. Drag a multi-assign onto the workflow designer
            2. Have a datalist with variables
            3. In the multi-assign enter [[
            4. Now pick one of the entries from the drop-down, this must populate the input box with the entry that was selected
         */
        [TestMethod]
        public void ClickingIntellisenseItemFillsField_Expected_IntellisenseItemAppearsInField()
        {
            // In the StudioBugTests file
        }

        #endregion

        #region Connecting to Servers

        //i. Can I connect to a server - TODO (External Server ?)
        [TestMethod]
        public void CreateServerConnection_Expected_ExplorerContainsNewServer()
        {
            // Commented out since you can no longer DC local server
            #region Setup

            /* 
             * Note From Michael: Deleting the environment file doesn't help, as the server remains in the Explorer
             * 
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appDataFolder = appDataFolder.Replace(@"\Roaming", @"\Local");
            appDataFolder += @"\Dev2\Environments\";
            List<string> environmentList = Directory.GetFiles(appDataFolder).ToList();
            foreach (string file in environmentList)
            {
                StreamReader sr1 = new StreamReader(file);
                string theLine = sr1.ReadLine();
                theLine = sr1.ReadLine();
                theLine = sr1.ReadLine(); // Get the third line
                sr1.Close();
                if (theLine.Contains("<DsfAddress>http://127.0.0.1:77/tfs</DsfAddress>"))
                {
                    File.Delete(file);
                }
            }
             */

            // Here we will have to remove all environment related files incase there is a server name localhost already exists

            #endregion Setup
            /*
            UITestControl theServer = this.ExplorerUIMap.GetServer("localhost");
            if (theServer == null) //If it exists, connecting must work. If it doesn't - Test is fine
            {
                ConnectToServer("localhost", "http://localhost:77/dsf");
                Assert.IsNotNull(this.ExplorerUIMap.GetServer("localhost"));
            }
             */
        }

        // i - Can I connect to the server and do I see all the server resources?
        [TestMethod]
        public void ConnectToServer_ServerAlreadyInExplorer()
        {
            // Commented out since you can no longer DC local server
            // Disconnect if we're already connected to a server
            /*
            this.DocManagerUIMap.ClickOpenTabPage("Explorer");
            if (IsLocalServerConnected())
            {
                this.UIMap.Environment_Explorer_Server_RightClick_Disconnect();
            }
            this.UIMap.Environment_Explorer_Server_RightClick_Connect();

            // Make sure we're now connected
            Assert.IsTrue(IsLocalServerConnected());

            // Make sure all the resource context-menus have been loaded
            Assert.IsNotNull(this.ExplorerUIMap.GetServiceType("localhost", "WORKFLOWS"));
            Assert.IsNotNull(this.ExplorerUIMap.GetServer("localhost"));
            */
        }

        //x.	Can I connect to another server?
        [TestMethod]
        public void ConnectToAnotherServer_Expected_ServerAndResourcesLoadedInRespectiveResourceTree()
        {
            // Ask Sashen why I can no longer connect to this server?

            /*
            #region Setup

            // Remove folders where server specified exists in environment

            #endregion Setup

            ConnectToServer("tfsbld", "http://rsaklfsvrtfsbld:77/dsf");
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            UITestControl service = this.ExplorerUIMap.GetService("tfsbld", "WORKFLOW SERVICE", "SYSTEM", "BaseWizard");
            Assert.IsNotNull(service);
             */
        }

        #endregion Connecting To Server

        #region Launching Wizards

        // ii - All new buttons launch their relevant wizard
        // Remaing - Plugin/Webpage/Website

        // ii.0 - Workflow Wizard
        [TestMethod]
        public void AllButtonsLaunchRelevantWizards_WorkflowWizard_Expected_WorkflowWizardLaunched()
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            string windowTitle = WorkflowWizardUIMap.GetWorkflowWizardName();
            Assert.AreEqual("Workflow Service Details", windowTitle);
            bool isClosed = this.WorkflowWizardUIMap.CloseWizard();
            if (!isClosed)
            {
                Assert.Fail("Unable to close wizard window");
            }
        }

        // ii.1 - Database Service = database service wizard.
        [TestMethod]
        public void AllButtonsLaunchRelevantWizards_DatabaseWizard_Expected_DatabaseWizardLaunched()
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Database Service");
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            string windowTitle = DatabaseServiceWizardUIMap.GetWorkflowWizardName();
            Assert.AreEqual("Database Service Details", windowTitle);
            bool isClosed = this.DatabaseServiceWizardUIMap.CloseWizard();
            if (!isClosed)
            {
                Assert.Fail("Unable to close wizard window");
            }
        }

        // ii.2 - Plugin Service = plugin service wizard
        [TestMethod]
        public void AllButtonsLaunchRelevantWizards_PluginWizard_Expected_PluginWizardLaunched()
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Plugin Service");
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            string windowTitle = PluginServiceWizardUIMap.GetWorkflowWizardName();
            Assert.AreEqual("Plugin Service Details", windowTitle);
            bool isClosed = PluginServiceWizardUIMap.CloseWizard();
            if (!isClosed)
            {
                Assert.Fail("Unable to close wizard window");
            }
        }

        // ii.3 - Webpage = Webpage wizard - Commented out since it's a web-based bug
        [TestMethod]
        public void AllButtonsLaunchRelevantWizards_WebpageWizard_Expected_WebpageWizardLaunched()
        {
            /*
            RibbonUIMap.ClickRibbonMenuItem("Home", "Webpage");
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            string windowTitle = WebpageServiceWizardUIMap.GetWorkflowWizardName();
            Assert.AreEqual("Webpage Service Details", windowTitle);
            bool isClosed = this.WebpageServiceWizardUIMap.CloseWizard();
            if (!isClosed)
            {
                Assert.Fail("Unable to close wizard window");
            }
             */
        }

        // ii.4 - Website = Website Wizard
        [TestMethod]
        public void AllButtonsLaunchRelevantWizards_WebsiteWizard_Expected_WebsiteWizardLaunched()
        {

        }
        #endregion Launching Wizards

        #region Deploy Tab

        /*
        [TestMethod]
        public void DeployTab()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            //this.UIMap.somestuff();
            //this.UIMap.someMethodForDeployTab();
        }
         */

        #endregion Deploy Tab

        #region Resource Management

        // iii - Double-clicking a service opens the service in a new tab.
        // Sashen : 29-11-2012 Would be nice if we could use the workflow designer here
        // Done
        [TestMethod]
        public void DoubleClickingAService_Expected_OpenInNewTab()
        {

            int tabCount = TabManagerUIMap.GetTabCount();

            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "DEMO", "DemoDeleteResource");

            int newTabCount = TabManagerUIMap.GetTabCount();

            Assert.AreEqual(tabCount + 1, newTabCount);
        }

        // iv - Can I create a workflow and does it render correctly?
        // Done - Still requires WorkflowDesigner to check rendering.
        [TestMethod]
        public void CreateWorkflow()
        {
            #region Workflow Input Parameters

            string workflowName = "CodedUITestWorkflow";

            #endregion Workflow Input Parameters
            this.RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");

            CreateCustomWorkflow("CodedUITestWorkflow");

            // Sashen - 29-11-2012 - Please swap this out to check the WorkflowDesigner for the Workflow name
            UITestControl createdTab = this.TabManagerUIMap.FindTabByName(workflowName);

            Assert.IsNotNull(createdTab);

            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(createdTab, "Start");

            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "CodedUITestWorkflow");
        }

        public void CreateCustomWorkflow(string workflowName)
        {
            CreateCustomWorkflow(workflowName, "CodedUITestCategory");
        }

        public void CreateCustomWorkflow(string workflowName, string workflowCategory)
        {
            #region Workflow Input Parameters

            #endregion Workflow Input Parameters
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");

            // Wait due to the way browser hosting occurs, the controls have to be found by their relative
            // position to the window
            while (!WorkflowWizardUIMap.IsWindowOpen())
            {
                System.Threading.Thread.Sleep(1000);
            }
            System.Threading.Thread.Sleep(2000);
            //this.UIMap.DoesThisExist();
            this.WorkflowWizardUIMap.EnterWorkflowName(workflowName);
            this.WorkflowWizardUIMap.EnterWorkflowCategory(workflowCategory);


            this.WorkflowWizardUIMap.DoneButtonClick();

            // Sashen - 29-11-2012 - Please swap this out to check the WorkflowDesigner for the Workflow name
            UITestControl createdTab = this.TabManagerUIMap.FindTabByName(workflowName);

            Assert.IsNotNull(createdTab);
            //this.UIMap.Environment_Workflow_StartButtonExists();
        }

        #endregion Resource Management

        #region Tests Requiring Designer access

        // vi - Can I drop a tool onto the designer?
        [TestMethod]
        public void CanAToolBeDroppedOntoTheDesigner()
        {
            // Create the Workflow
            CreateCustomWorkflow("CanAToolBeDroppedOntoTheDesigner", "CodedUITestCategory");

            // Get the tab
            UITestControl theTab = TabManagerUIMap.FindTabByName("CanAToolBeDroppedOntoTheDesigner");

            // And click it to make sure it's focused
            TabManagerUIMap.Click(theTab);

            // Wait a bit for user noticability
            System.Threading.Thread.Sleep(500);

            // Get the location of the Start button
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");

            // And click it for UI responsiveness :P
            WorkflowDesignerUIMap.ClickControl(theStartButton);

            // Make sure no comment box exists on the Workflow Designer
            if (WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "Comment"))
            {
                Assert.Fail("A Comment tool already exists on the Workflow Designer - Please delete it before running the test!");
            }

            // Get a point underneath the start button
            Point p = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Toolbox");

            // Get the comment box
            UITestControl commentControl = ToolboxUIMap.FindToolboxItemByAutomationID("Comment");

            // And drag it onto the point
            ToolboxUIMap.DragControlToWorkflowDesigner(commentControl, p);

            // Check if it exists on the designer
            Assert.IsTrue(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "Comment"));

            // Cleanup
            // Refresh the list
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();

            // Delete the workflow
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickDeleteProject("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "CanAToolBeDroppedOntoTheDesigner");

            // Re-refresh the list to clean up
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();

        }

        //vii.	Can I drop a workflow service onto the design surface?
        // TODO!
        [TestMethod]
        public void CanAWorkflowServiceBeDroppedOntoTheDesigner()
        {
            /*
            // Get the tab
            UITestControl theTab = TabManagerUIMap.FindTabByName("someNameHere");

            // And click it to make sure it's focused
            TabManagerUIMap.Click(theTab);

            // Wait a bit for user noticability
            System.Threading.Thread.Sleep(500);

            // Get the location of the Start button
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");

            // And click it for UI responsiveness :P
            WorkflowDesignerUIMap.ClickControl(theStartButton);

            // Make sure no comment box exists on the Workflow Designer
            if (WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "Comment"))
            {
                Assert.Fail("A Comment tool already exists on the Workflow Designer - Please delete it before running the test!");
            }

            // Get a point underneath the start button
            Point p = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Toolbox");

            // Get the comment box
            UITestControl commentControl = ToolboxUIMap.FindToolboxItemByAutomationID("Comment");

            // And drag it onto the point
            ToolboxUIMap.DragControlToWorkflowDesigner(commentControl, p);

            // Check if it exists on the designer
            Assert.IsTrue(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "Comment"));
             */
        }

        //viii.	Can I drop a worker service onto the design surface?
        [TestMethod]
        public void CanAWorkerServiceBeDroppedOntoTheDesigner()
        {
            //this.UIMap.Environment_Workflow_Flowchart_Click(); // See iv bug
        }

        // ix. Does clicking the Workflow button launches the workflow wizard?
        [TestMethod]
        public void DoesClickingTheWorkflowButtonLaunchTheWorkflowWizard()
        {

            // Launch the wizard
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");

            // The close will only work if you were able to open it in the first place :P
            WorkflowWizardUIMap.CloseWizard();
        }

        // x. Can I connect to another server?

        // xi. Can I deploy to my server? (AKA: Can I deploy?)
        [TestMethod]
        public void CanIDeploy()
        {
            // Open the Explorer tab
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Naviate to the Workflow, and Right click it
            ExplorerUIMap.RightClickDeployProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Wait for the Deploy tab to load!
            System.Threading.Thread.Sleep(5000);

            // Make sure the correct tab is highlighted
            UITestControl theTab = TabManagerUIMap.FindTabByName("Deploy Resources");
            TabManagerUIMap.Click(theTab);

            // Choose the required servers
            DeployViewUIMap.SelectServers(theTab, "localhost", "localhost");

            // Make sure the deploy count is correct
            Assert.IsTrue(DeployViewUIMap.DoSourceAndDestinationCountsMatch(theTab));

            // Click the "Deploy" button
            DeployViewUIMap.ClickDeploy(theTab); // This currently just mouses over the Deploy Button, since I had no servers to test against
        }

        #endregion Tests Requiring Designer access

        #region Additional test attributes


        /*
        public void Environment_DocManager_Click(string tabName)
        {
            #region Variable Declarations
            //WpfTabPage uIExplorerTabPage = UIMap.UIBusinessDesignStudioWindow.UIDockManagerCustom.UIPART_UnpinnedTabAreaTabList.UIExplorerTabPage;
            #endregion

            // Click 'Explorer' tab
            //Mouse.Click(uIExplorerTabPage, new Point(4, 26));
        }

        public class UIPART_UnpinnedTabAreaTabList : WpfTabList
        {

            public UIPART_UnpinnedTabAreaTabList(UITestControl searchLimitContainer) :
                base(searchLimitContainer)
            {
                #region Search Criteria
                this.SearchProperties[WpfTabList.PropertyNames.AutomationId] = "PART_UnpinnedTabAreaLeft";
                this.WindowTitles.Add(TestBase.GetStudioWindowName());
                #endregion
            }
            private WpfTabPage mUIExplorerTabPage;

            public WpfTabPage UIExplorerTabPage(string tabName)
            {
                mUIExplorerTabPage = new WpfTabPage(this);
                #region Search Criteria
                this.mUIExplorerTabPage.SearchProperties[WpfTabPage.PropertyNames.Name] = tabName;
                this.mUIExplorerTabPage.WindowTitles.Add("Business Design Studio (DEV2\\Michael.Cullen)");
                #endregion
                return this.mUIExplorerTabPage;
            }
        }*/

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

        #region Additional test methods

        private string GetWorkspaceID()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string workspaceFile = localAppData + @"\Dev2\UserInterfaceLayouts\WorkspaceItems.xml";
            StreamReader sr1 = new StreamReader(workspaceFile);
            string fileData = sr1.ReadToEnd();
            sr1.Close();
            string workspaceID = fileData.Remove(0, fileData.IndexOf("WorkspaceID=") + 13);
            workspaceID = workspaceID.Substring(0, workspaceID.IndexOf("\""));
            return workspaceID;
        }

        private string GetServerEXEFolder()
        {
            var findDev2Studios = System.Diagnostics.Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName.StartsWith("Dev2.Server"));
            if (findDev2Studios.Count() != 1)
            {
                throw new Exception("Error - Cannot find location if more than 1 studio is open :(");
            }
            else
            {
                foreach (Process p in findDev2Studios)
                {
                    int processID = p.Id;
                    string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processID.ToString();
                    using (var searcher = new ManagementObjectSearcher(wmiQueryString))
                    {
                        using (var results = searcher.Get())
                        {
                            ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                            if (mo != null)
                            {
                                string fileLocation = (string)mo["ExecutablePath"];
                                string folder = fileLocation.Substring(0, fileLocation.LastIndexOf(@"\") + 1);
                                return folder;
                            }
                        }
                    }
                    return null;
                }
            }
            return null;
        }

        public string GetStudioEXELocation()
        {
            var findDev2Studios = System.Diagnostics.Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName.StartsWith("Dev2.Studio"));
            if (findDev2Studios.Count() != 1)
            {
                throw new Exception("Error - Cannot find location if more than 1 studio is open :(");
            }
            else
            {
                foreach (Process p in findDev2Studios)
                {
                    string fileName = p.MainModule.FileName;
                    // Fix the file name
                    fileName = fileName.Replace(".vshost.exe", ".exe");
                    return fileName;
                }
            }
            return null;
        }

        public void KillAllInstancesOf(string instanceName)
        {
            List<Process> processInstances = System.Diagnostics.Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName.StartsWith(instanceName)).ToList();
            foreach (Process p in processInstances)
            {
                p.Kill();
            }
        }

        public void DoCleanup(string server, string serviceType, string category, string workflowName)
        {
            // Test complete - Delete itself
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();

            // Delete the workflow
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickDeleteProject(server, serviceType, category, workflowName);

            // Re-refresh the list to clean up
            // Thank to Jurie, this bug has been fixed! \o/ \o/ \o/
            //DocManagerUIMap.ClickOpenTabPage("Explorer");
            //ExplorerUIMap.DoRefresh();
        }

        private bool IsLocalServerConnected()
        {
            try
            {
                this.ExplorerUIMap.GetServer("localhost");
                Point p = this.ExplorerUIMap.GetServiceType("localhost", "WORKFLOWS").GetClickablePoint();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsAnotherServerConnected()
        {
            try
            {

                //this.UIMap.Environment_Explorer_AnotherServer_IsConnected();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetStudioWindowName()
        {
            return "Business Design Studio (DEV2\\" + Environment.UserName + ")";
        }

        private void ConnectToServer(string serverName, string address)
        {
            this.DocManagerUIMap.ClickOpenTabPage("Explorer");
            //this.ExplorerUIMap.ConnectBtnClick();
            this.ConnectViewUIMap.InputServerName(serverName);
            this.ConnectViewUIMap.InputServerAddress(address);
            this.ConnectViewUIMap.ClickConnectButton();
        }

        private void CloseTheStudio()
        {
            // Close the Studio (Don't kill it)
            WpfWindow theStudio = new WpfWindow();
            theStudio.SearchProperties[WpfWindow.PropertyNames.Name] = TestBase.GetStudioWindowName();
            theStudio.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            theStudio.WindowTitles.Add(TestBase.GetStudioWindowName());
            theStudio.Find();
            Point closeButtonPoint = new Point(theStudio.BoundingRectangle.Left + theStudio.BoundingRectangle.Width - 10, theStudio.BoundingRectangle.Top + 10);
            Mouse.Click(closeButtonPoint);
            System.Threading.Thread.Sleep(2000); // Give it time to die
        }

        #endregion

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        ///
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

        #region UI Maps

        #region Base UI Map

        public UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        public DocManagerUIMap DocManagerUIMap
        {
            get
            {
                if ((this.docManagerMap == null))
                {
                    this.docManagerMap = new DocManagerUIMap();
                }

                return this.docManagerMap;
            }
        }

        public ToolboxUIMap ToolboxUIMap
        {
            get
            {
                if ((this.toolboxUIMap == null))
                {
                    this.toolboxUIMap = new ToolboxUIMap();
                }

                return this.toolboxUIMap;
            }
        }

        public ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if ((this.explorerUIMap == null))
                {
                    this.explorerUIMap = new ExplorerUIMap();
                }

                return this.explorerUIMap;
            }
        }

        public DeployViewUIMap DeployViewUIMap
        {
            get
            {
                if ((this.deployViewUIMap == null))
                {
                    this.deployViewUIMap = new DeployViewUIMap();
                }

                return this.deployViewUIMap;
            }
        }

        private ExplorerUIMap explorerUIMap;
        private ToolboxUIMap toolboxUIMap;
        private DocManagerUIMap docManagerMap;
        private UIMap map;
        private DeployViewUIMap deployViewUIMap;

        #endregion Base UI Map

        #region Connect Window UI Map

        public ConnectViewUIMap ConnectViewUIMap
        {
            get
            {
                if (connectViewUIMap == null)
                {
                    connectViewUIMap = new ConnectViewUIMap();
                }
                return connectViewUIMap;
            }
        }

        private ConnectViewUIMap connectViewUIMap;

        #endregion Connect Window UI Map
        
        #region Debug UI Map

        public DebugUIMap DebugUIMap
        {
            get
            {
                if (debugUIMap == null)
                {
                    debugUIMap = new DebugUIMap();
                }
                return debugUIMap;
            }
        }

        private DebugUIMap debugUIMap;

        #endregion External UI Map

        #region DependencyGraph UI Map

        public DependencyGraph DependencyGraphUIMap
        {
            get
            {
                if (DependencyGraphUIMap == null)
                {
                    DependencyGraphUIMap = new DependencyGraph();
                }

                return DependencyGraphUIMap;
            }
            set { throw new NotImplementedException(); }
        }

        private DependencyGraph DependencyGraph;

        #endregion WorkflowDesigner UI Map

        #region WorkflowWizard UI Map

        public WorkflowWizardUIMap WorkflowWizardUIMap
        {
            get
            {
                if (workflowWizardUIMap == null)
                {
                    workflowWizardUIMap = new WorkflowWizardUIMap();
                }

                return workflowWizardUIMap;
            }
        }

        private WorkflowWizardUIMap workflowWizardUIMap;

        #endregion WorkflowWizard UI Map

        #region Database Wizard UI Map

        public DatabaseServiceWizardUIMap DatabaseServiceWizardUIMap
        {
            get
            {
                if (databaseServiceWizardUIMap == null)
                {
                    databaseServiceWizardUIMap = new DatabaseServiceWizardUIMap();
                }

                return databaseServiceWizardUIMap;
            }
        }

        private DatabaseServiceWizardUIMap databaseServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region Feedback UI Map

        public FeedbackUIMap FeedbackUIMap
        {
            get
            {
                if (_feedbackUIMap == null)
                {
                    _feedbackUIMap = new FeedbackUIMap();
                }

                return _feedbackUIMap;
            }
        }

        private FeedbackUIMap _feedbackUIMap;

        #endregion Feedback UI Map

        #region Plugin Wizard UI Map

        public PluginServiceWizardUIMap PluginServiceWizardUIMap
        {
            get
            {
                if (pluginServiceWizardUIMap == null)
                {
                    pluginServiceWizardUIMap = new PluginServiceWizardUIMap();
                }

                return pluginServiceWizardUIMap;
            }
        }

        private PluginServiceWizardUIMap pluginServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region Ribbon UI Map

        public RibbonUIMap RibbonUIMap
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

        #region TabManager UI Map


        public TabManagerUIMap TabManagerUIMap
        {
            get
            {
                if (tabManagerUIMap == null)
                {
                    tabManagerUIMap = new TabManagerUIMap();
                }

                return tabManagerUIMap;
            }
        }





        private TabManagerUIMap tabManagerUIMap;

        #endregion TabManager UI Map

        #region Variables UI Map

        public VariablesUIMap VariablesUIMap
        {
            get
            {
                if (variablesUIMap == null)
                {
                    variablesUIMap = new VariablesUIMap();
                }
                return variablesUIMap;
            }
        }

        private VariablesUIMap variablesUIMap;

        #endregion Connect Window UI Map

        #region Service Details UI Map

        public ServiceDetailsUIMap ServiceDetailsUIMap
        {
            get
            {
                if (serviceDetailsUIMap == null)
                {
                    serviceDetailsUIMap = new ServiceDetailsUIMap();
                }
                return serviceDetailsUIMap;
            }
        }

        private ServiceDetailsUIMap serviceDetailsUIMap;

        #endregion

        #region External UI Map

        public ExternalUIMap ExternalUIMap
        {
            get
            {
                if (externalUIMap == null)
                {
                    externalUIMap = new ExternalUIMap();
                }
                return externalUIMap;
            }
        }

        private ExternalUIMap externalUIMap;

        #endregion External UI Map
        
        #region Webpage Wizard UI Map

        public WebpageServiceWizardUIMap WebpageServiceWizardUIMap
        {
            get
            {
                if (webpageServiceWizardUIMap == null)
                {
                    webpageServiceWizardUIMap = new WebpageServiceWizardUIMap();
                }

                return webpageServiceWizardUIMap;
            }
        }

        private WebpageServiceWizardUIMap webpageServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region WorkflowDesigner UI Map

        public WorkflowDesignerUIMap WorkflowDesignerUIMap
        {
            get
            {
                if (workflowDesignerUIMap == null)
                {
                    workflowDesignerUIMap = new WorkflowDesignerUIMap();
                }

                return workflowDesignerUIMap;
            }
        }

        private WorkflowDesignerUIMap workflowDesignerUIMap;

        #endregion WorkflowDesigner UI Map

        #endregion UI Maps
    }
}
