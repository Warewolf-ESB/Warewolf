using System.Windows.Automation;
using System.Windows.Input;
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
using Dev2.Studio.UI.Tests;
using Dev2.Studio.UI.Tests.UIMaps.ActivityDropWindowUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseSourceUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DependencyGraphClasses;
using Dev2.Studio.UI.Tests.UIMaps.FeedbackUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.NewServerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.OutputUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.PluginSourceMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.ServerWizardClasses;
using Dev2.Studio.UI.Tests.UIMaps.ServiceDetailsUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.VideoTestUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using AndCondition = System.Windows.Automation.AndCondition;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using Point = System.Drawing.Point;
using PropertyCondition = System.Windows.Automation.PropertyCondition;

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
        public string ServerExeLocation;
        // To bring up the generator utility, click in a method, and press
        // Cntrl+\, Cntrl+C

        static Process studioProc;

        //[ClassInitialize]
        //public static void StartStudio(TestContext ctx)
        //{

        //    if (studioProc == null)
        //    {

        //        //get the folder that's in
        //        string thePath = Assembly.GetExecutingAssembly().Location;

        //        string dir = Path.GetDirectoryName(thePath);

        //        File.WriteAllText(@"c:\foo\ui_path.txt", dir);

        //        studioProc = new Process();

        //        studioProc.StartInfo.FileName = dir + @"\Dev2.Studio.exe";

        //        studioProc.Start();
        //        Thread.Sleep(30000); // wait 30 seconds for everything to fire up ;)
        //    }

        //}

        //[ClassCleanup]
        //public static void StopStudio()
        //{
        //    studioProc.Kill();
        //    Thread.Sleep(10000); // wait 10 seconds for everything to exit ;)
        //}


        // These run at the start of every test to make sure everything is sane
        [TestInitialize]
        public void CheckStartIsValid()
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
            theWindow.WindowTitles.Add(GetStudioWindowName());
            theWindow.Find();
            theWindow.SetFocus();


            // Disable this if you don't want the pre-test validation to occur.
            // If it's set to true it closes all open tabs, and all instances of IE before each test is run
            // Whilst debugging a Coded UI Test, you might want to keep the Workflow Designer as is
            // In this case, you should set it to false
            // ReSharper disable ReplaceWithSingleAssignment.False
            bool toCheck = true;

            // On the test box, all test initialisations should always run
            if (GetStudioWindowName().Contains("IntegrationTester"))
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
                    else
                    {
                        var ms = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ProcessId = " + findDev2Servers.ToList()[0].Id);
                        foreach (ManagementObject mo in ms.Get())
                        {
                            foreach (PropertyData prop in mo.Properties)
                            {
                                if (prop.Name == "ExecutablePath")
                                    ServerExeLocation = prop.Value.ToString();
                            }
                        }
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
                    //Make sure no tabs are open
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
                        if (theTab != "Start Page")
                        {
                            TabManagerUIMap.CloseTab(theTab);
                        }
                        else
                        {
                            if (openTabs == 1)
                            {
                                return;
                            }
                        }

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

        // This test clicks the Explorer Tab.
        [TestMethod]
        public void ClickExplorerAndDoAFewThings()
        {
            UITestControl theTab = TabManagerUIMap.FindTabByName("someWorkflow");

            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            Point p = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);

            UITestControl theItem = ToolboxUIMap.FindToolboxItemByAutomationId("Calculate");
            ToolboxUIMap.DragControlToWorkflowDesigner(theItem, p);

            WorkflowDesignerUIMap.SetStartNode(theTab, "Calculate");

            WorkflowDesignerUIMap.CalculateControl_EnterData(theTab, "Calculate", "sum(1,2)", "[[myResult]]");

            RibbonUIMap.ClickRibbonMenuItem("Home", "View in Browser");
        }

        // Comment
        [TestMethod]
        public void ThisMethodIsForTestingRandomTestFragments()
        {
          
            // RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
            //  VideoUIMapTest.doSomething();
            //  CreateCustomWorkflow("Test123");
            /*
             * 
            RibbonUIMap.ClickRibbonMenu("Help");
            RibbonUIMap.ClickRibbonMenu("Home");
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
            WorkflowWizardUIMap.CloseWizard();
            RibbonUIMap.ClickRibbonMenu("Help");
            RibbonUIMap.ClickRibbonMenu("Home");
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
            WorkflowWizardUIMap.CloseWizard();
            RibbonUIMap.ClickRibbonMenu("Help");
            RibbonUIMap.ClickRibbonMenu("Home");
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
            WorkflowWizardUIMap.CloseWizard();
             */
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

            //UITestControl commentControl = ToolboxUIMap.FindToolboxItemByAutomationId("Comment");
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

        [TestMethod]
        public void ThisIsAVideoTestMethod()
        {
            /*
            // Create a Workflow
            CreateCustomWorkflow("MyTestWorkflow");

            // Open the Toolbox tab
            DocManagerUIMap.ClickOpenTabPage("Toolbox");

            // Get the Calculate control
            UITestControl calculateControl = ToolboxUIMap.FindToolboxItemByAutomationId("Calculate");

            // Get a tab
            UITestControl theTab = TabManagerUIMap.FindTabByName("MyTestWorkflow");

            // And drag it onto the Workflow Designer
            Point pointOnWorkflowDesigner = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            ToolboxUIMap.DragControlToWorkflowDesigner(calculateControl, pointOnWorkflowDesigner);

            // Input some data into the Calculate Control
            WorkflowDesignerUIMap.CalculateControl_EnterData(theTab, "Calculate", "sum(10, 5)", "[[calcResult]]");

            // Set it as the start node
            WorkflowDesignerUIMap.SetStartNode(theTab, "Calculate");

            // And run it, by clicking the appropriate button on the Ribbon
            RibbonUIMap.ClickRibbonMenuItem("Home", "View in Browser");
            */

            // Close the browser
            ExternalUIMap.CloseAllInstancesOfIE();

            // And delete the Workflow
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "MyTestWorkflow");

        }

        #region New PBI Tests

        // PBI 8601 (Task 8855)
        [TestMethod]
        public void QuickVariableInputFromListTest()
        {
            // Create the workflow
            CreateCustomWorkflow("PBI8601");
            Thread.Sleep(2500);
            UITestControl theTab = TabManagerUIMap.FindTabByName("PBI8601");
            //UITestControl theTab = TabManagerUIMap.FindTabByName("RightClickMenuTests");

            // Add an assign control
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));

            // Click the Adorner button
            WorkflowDesignerUIMap.AssignControl_ClickQuickVariableInputButton(theTab, "Assign");

            // Enter some invalid data
            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_EnterData(theTab, "Assign", ",", "some(<).", "_suf", "varOne,varTwo,varThree");

            // Click done
            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_ClickAdd(theTab, "Assign");

            // Make sure an error has been thrown
            string previewText = WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_GetPreviewData(theTab, "Assign");
            StringAssert.Contains(previewText, "Prefix contains invalid characters");

            // Click cancel, and enter some correct data
            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_ClickCancel(theTab, "Assign");

            WorkflowDesignerUIMap.AssignControl_ClickQuickVariableInputButton(theTab, "Assign");
            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_EnterData(theTab, "Assign", ",", "pre_", "_suf", "varOne,varTwo,varThree");

            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_ClickAdd(theTab, "Assign");

            // Check the data
            string varName = WorkflowDesignerUIMap.AssignControl_GetVariableName(theTab, "Assign", 0);
            StringAssert.Contains(varName, "[[pre_varOne_suf]]");

            // All good - Clean up!
            //DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "PBI8601");
        }

        //PBI_8853
        [TestMethod]
        public void ClickNewWorkflow_Expected_WorkflowOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
            Thread.Sleep(2500);
            if (WorkflowWizardUIMap.UIWorkflowServiceDetaiWindow==null)
            {
                Assert.Fail("Error - Clicking the new Workflow button does not create the new Workflow Window");
            }
            WorkflowWizardUIMap.CloseWizard();
        }     
        
        [TestMethod]
        public void NewWorkflowShortcutKey_Expected_WorkflowOpens()
        {
//            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
            Keyboard.SendKeys("W",ModifierKeys.Control);
            //Thread.Sleep(2500);
            if (WorkflowWizardUIMap.UIWorkflowServiceDetaiWindow==null)
            {
                Assert.Fail("Error - Clicking the new Workflow button does not create the new Workflow Window");
            }
            WorkflowWizardUIMap.CloseWizard();
        }        
        
        [TestMethod]
        public void ClickNewDatabaseService_Expected_DatabaseServiceOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Database Service");
            Thread.Sleep(1500);
            UITestControl uIItemImage = DatabaseServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            if (uIItemImage == null)
            {
                Assert.Fail("Error - Clicking the new database service button does not create the new database service window");
            }
            DatabaseServiceWizardUIMap.DatabaseServiceClickCancel();
        }
        
        [TestMethod]
        public void NewDatabaseServiceShortcutKey_Expected_DatabaseServiceOpens()
        {
            //RibbonUIMap.ClickRibbonMenuItem("Home", "Database Service");
            SendKeys.SendWait("^+(D)");
            Thread.Sleep(1500);
            UITestControl uIItemImage = DatabaseServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            if (uIItemImage == null)
            {
                Assert.Fail("Error - Clicking the new database service button does not create the new database service window");
            }
            DatabaseServiceWizardUIMap.DatabaseServiceClickCancel();
        }


        [TestMethod]
        public void ClickNewPluginService_Expected_PluginServiceOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Plugin Service");
            Thread.Sleep(1500);
            UITestControl uiTestControl = PluginServiceWizardUIMap.UIPluginServiceDetailsWindow;
            if (uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the new plugin service button does not create the new plugin service window");
            }
            PluginServiceWizardUIMap.CloseWizard();
        }  
    
        [TestMethod]
        public void ClickNewPluginServiceShortcutKey_Expected_PluginServiceOpens()
        {
           // RibbonUIMap.ClickRibbonMenuItem("Home", "Plugin Service");
            SendKeys.SendWait("^+(P)");
            Thread.Sleep(1500);
            UITestControl uiTestControl = PluginServiceWizardUIMap.UIPluginServiceDetailsWindow;
            if (uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the new plugin service button does not create the new plugin service window");
            }
            PluginServiceWizardUIMap.CloseWizard();
        }      
        
        [TestMethod]
        public void ClickNewRemoteWarewolfServer_Expected_RemoteWarewolfServerOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Remote Warewolf");
            Thread.Sleep(2500);
            UITestControl uiTestControl = NewServerUIMap.UINewServerWindow;
            if (uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the remote warewolf button does not create the new server window");
            }
            NewServerUIMap.CloseWindow();
        } 
        
        [TestMethod]
        public void ClickNewDatabaseSource_Expected_DatabaseSourceOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Database");
            Thread.Sleep(1500);
            UITestControl uiTestControl = DatabaseSourceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            if (uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the Database source button does not create the new Database source window");
            }
            DatabaseSourceWizardUIMap.ClickCancel();
        } 
        
        [TestMethod]
        public void ClickNewPluginSource_Expected_PluginSourceOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Home", "Plugin");
            Thread.Sleep(2500);
            UITestControl uiTestControl = PluginSourceMap.UIPluginSourceManagmenWindow;
            if (uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the plugin source button does not create the new plugin source window");
            }
            PluginSourceMap.ClickCancel();
        }

        #endregion New PBI Tests

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

        [TestMethod]
        public void NewServer_SaveConnectionWithInvalidName_Expected_SaveWindowDoesNotAppear()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClickNewServerButton();
            NewServerUIMap.EnterServerAddress("http://www.google.co.za/dsf");
            NewServerUIMap.ClickSaveConnection();
            string result = NewServerUIMap.SaveWindow_EnterNameText_Return_NameText("test");
            if (result != "test")
            {
                NewServerUIMap.SaveWindow_ClickCancel();
                NewServerUIMap.CloseWindow();
                Assert.Fail("The save window did not appear!");
            }
            NewServerUIMap.SaveWindow_ClickCancel();
            NewServerUIMap.ClearServerAddress();
            NewServerUIMap.EnterServerAddress("invalidName");
            NewServerUIMap.ClickSaveConnection();
            result = NewServerUIMap.SaveWindow_EnterNameText_Return_NameText("test");
            if (result == "test")
            {
                Assert.Fail("The save window appeared with an invalid Server name!");
            }
            NewServerUIMap.CloseWindow();
            if (NewServerUIMap.IsNewServerWindowOpen())
            {
                Assert.Fail("The New Server window was unable to close!");
            }

        }

        #endregion Add Server Tests

        #region Dev2ServiceDetails Wizard Tests

        /*
        
        All commented out until Travis completes Bug 8477
        
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
            CreateCustomWorkflow("StandardWorkflowProperties", "CODEDUITESTCATEGORY");
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

        */

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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

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
            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "CalculateTaxReturns");
            Mouse.Click(controlOnWorkflow, new Point(5, 5));
            SendKeys.SendWait("{DELETE}");

            // Drag it on again
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Change the name
            controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "CalculateTaxReturns");
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
            //CreateCustomWorkflow("RightClickMenuTests");

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
            Thread.Sleep(500); // Give it time to close
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
            Thread.Sleep(500); // Give it time to close
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
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITWESTCATEGORY", "RightClickMenuTests");
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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag a Calculate control on
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl calculateControl = ToolboxUIMap.FindToolboxItemByAutomationId("Calculate");
            ToolboxUIMap.DragControlToWorkflowDesigner(calculateControl, workflowPoint1);

            Mouse.Click();
            SendKeys.SendWait("sum{(}");

            // Find the control
            UITestControl calculateOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Calculate");

            // Find the fxBox - This seemed resilient to filter properties for some odd reason...
            WpfEdit fxBox = new WpfEdit(calculateOnWorkflow);
            //fxBox.FilterProperties.Add("AutomationId", "UI__fxtxt_AutoID");
            //fxBox.Find();

            UITestControlCollection boxCollection = fxBox.FindMatchingControls();
            WpfEdit realfxBox = new WpfEdit();
            foreach (WpfEdit theBox in boxCollection)
            {
                string autoId = theBox.AutomationId;
                if (autoId == "UI__fxtxt_AutoID")
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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag a Multi Assign on
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl asssignControlInToolbox = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(asssignControlInToolbox, workflowPoint1);

            // Add some text
            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "Assign", 0);
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
            Mouse.Click(p);
            for (int j = 0; j < 25; j++)
            {
                Mouse.StartDragging();
                Mouse.StopDragging(-50, -50);
                Mouse.StartDragging();
                Mouse.StopDragging(p);
                Point clickPoint;
                if (!theWindow.TryGetClickablePoint(out clickPoint))
                {
                    Assert.Inconclusive("It died");
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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            // Add the data!
            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "Assign", 0);
            for (int j = 0; j < 100; j++)
            {
                // Sleeps are due to the delay when adding a lot of items
                SendKeys.SendWait("[[theVar" + j.ToString(CultureInfo.InvariantCulture) + "]]");
                Thread.Sleep(50);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(50);
                SendKeys.SendWait(j.ToString(CultureInfo.InvariantCulture));
                Thread.Sleep(50);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(50);
            }

            // And map!
            DocManagerUIMap.ClickOpenTabPage("Variables");
            //Massimo.Guerrera - 6/3/2013 - Removed because variables are now auto added to the list.
            //VariablesUIMap.UpdateDataList();

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
            // TODO: Recode this to use either the IE9 map or the IE8 map
            // Currently coded to use the IE8 map
            /*
            // Create the workflow
            //CreateCustomWorkflow("5782Point1", "CodedUITestCategory");

            // Add the variable
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            SendKeys.SendWait("testVar");

            // Click "View in Browser"
            RibbonUIMap.ClickRibbonMenuItem("Home", "View in Browser");

            // Give the slow IE time to open ;D
            System.Threading.Thread.Sleep(25000);

            // Check if the IE Body contains the data list item
            string IEText = ExternalUIMap.GetIEBodyText();
            if (!IEText.Contains("<testVar></testVar>"))
            {
                Assert.Fail("The variable was not added to the DataList :(");
            }

            // Close the browser
            ExternalUIMap.CloseAllInstancesOfIE();

            // And do cleanup
            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5782Point1");
             */
            Assert.Inconclusive("The test passes - IE needs to be remapped (IE8 map VS IE9 map)");
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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag an Assign onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theItem = ToolboxUIMap.FindControl("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theItem, workflowPoint1);

            // Fill some data
            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "Assign", 0);
            SendKeys.SendWait("[[recSet{(}{)}.Name]]");
            Thread.Sleep(100); // Wait bug if you type too fast
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(100);
            SendKeys.SendWait("myName");

            // Map it
            DocManagerUIMap.ClickOpenTabPage("Variables");
            //Massimo.Guerrera - 6/3/2013 - Removed because variables are now auto added to the list.
            //VariablesUIMap.UpdateDataList();

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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            Thread.Sleep(1000);

            // Get the data for later comparison
            string folder = GetServerEXEFolder();
            string workspaceId = GetWorkspaceID();
            string path1 = folder + @"Workspaces\" + workspaceId + @"\Services\5782Point2TWR.xml";
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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag an Assign onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theItem = ToolboxUIMap.FindControl("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theItem, workflowPoint1);

            // Fill some data
            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "Assign", 0);
            SendKeys.SendWait("[[recSet{(}{)}.Name]]");
            Thread.Sleep(100); // Wait bug if you type too fast
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(100);
            SendKeys.SendWait("myName");

            // Map it
            DocManagerUIMap.ClickOpenTabPage("Variables");
            //Massimo.Guerrera - 6/3/2013 - Removed because variables are now auto added to the list.
            //VariablesUIMap.UpdateDataList();

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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            Thread.Sleep(1000);

            // Get the data for later comparison
            string folder = GetServerEXEFolder();
            string workspaceId = GetWorkspaceID();
            string path1 = folder + @"Workspaces\" + workspaceId + @"\Services\5782Point3.xml";
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
            Thread.Sleep(1000);

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
            TabManagerUIMap.CloseTab("5782Point3");
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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            Thread.Sleep(1000);

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
        public void DebugTabUpdatesWhenXmlIsModified()
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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            Thread.Sleep(1000);

            // Get the data for later comparison
            string folder = GetServerEXEFolder();
            string workspaceId = GetWorkspaceID();
            string path1 = folder + @"Workspaces\" + workspaceId + @"\Services\5782Point5.xml";
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

            // 3. Close the tab, and click Yes
            TabManagerUIMap.CloseTab_Click_Yes("5782Point5");

            // Check that no tabs remain open
            int tabCount = TabManagerUIMap.GetTabCount();
            Assert.AreEqual(1,tabCount, "Error - Clicking No kept the tab open.");

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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            Thread.Sleep(1000);

            // Get the data for later comparison
            string folder = GetServerEXEFolder();
            string workspaceId = GetWorkspaceID();
            string path1 = folder + @"Workspaces\" + workspaceId + @"\Services\5782Point6.xml";
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

            // 3. Close the tab, and click No
            TabManagerUIMap.CloseTab_Click_No("5782Point6");

            // Check that no tabs remain open
            int tabCount = TabManagerUIMap.GetTabCount();
            Assert.AreEqual(1,tabCount, "Error - Clicking No kept the tab open.");

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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            // Let it save.....
            Thread.Sleep(2500);

            // Get the data for later comparison
            string folder = GetServerEXEFolder();
            string workspaceId = GetWorkspaceID();
            string path1 = folder + @"Workspaces\" + workspaceId + @"\";
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

            TabManagerUIMap.CloseTab_Click_No("5782Point7");

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
            theStudio.SearchProperties[WpfWindow.PropertyNames.Name] = GetStudioWindowName();
            theStudio.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            theStudio.WindowTitles.Add(GetStudioWindowName());
            theStudio.Find();
            Point closeButtonPoint = new Point(theStudio.BoundingRectangle.Left + theStudio.BoundingRectangle.Width - 10, theStudio.BoundingRectangle.Top + 10);
            Mouse.Click(closeButtonPoint);
            Thread.Sleep(2000); // Give it time to die

            // Restart the studio!
            Process.Start(fileName);

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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

            // Get a point underneath the start button for each workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag a ForEach onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl tcForEach = ToolboxUIMap.FindToolboxItemByAutomationId("ForEach");
            ToolboxUIMap.DragControlToWorkflowDesigner(tcForEach, workflowPoint1);

            // Get a sample workflow, and drag it onto the "Drop Activity Here" part of the ForEach box
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, new Point(workflowPoint1.X + 25, workflowPoint1.Y + 25));

            // Wait for the ForEach thing to do its init-y thing
            Thread.Sleep(1500);

            // And click below the tab to get us back to the normal screen
            Mouse.Move(new Point(theTab.BoundingRectangle.X + 50, theTab.BoundingRectangle.Y + 50));
            Mouse.Click();

            // Now - Onto Part 2!

            // 5792.2

            // Get the location of the ForEach box
            UITestControl forEachControl = workflowDesignerUIMap.FindControlByAutomationId(theTab, "ForEach");

            // Move the mouse to the contained CalculateTaxReturns box
            Mouse.Move(new Point(forEachControl.BoundingRectangle.X + 25, forEachControl.BoundingRectangle.Y + 75));

            // Click it
            Mouse.Click();

            // And drag it down
            Mouse.StartDragging();
            Mouse.StopDragging(new Point(workflowPoint1.X, workflowPoint1.Y + 100));

            // Now get its position
            UITestControl calcTaxReturnsControl = workflowDesignerUIMap.FindControlByAutomationId(theTab, "CalculateTaxReturns");
            try
            {
                calcTaxReturnsControl.GetClickablePoint();
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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

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
            UITestControl startNode = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
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
            /*
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
            */
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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

            // Get a point underneath the start button for the workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);


            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Click it
            UITestControl controlOnWorkflow = workflowDesignerUIMap.FindControlByAutomationId(theTab, "CalculateTaxReturns");
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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

            // Get a point underneath the start button for the workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 100);

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Click it
            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "CalculateTaxReturns");
            Mouse.Click(controlOnWorkflow, new Point(5, 5));
            WorkflowDesignerUIMap.Adorner_ClickMapping(theTab, "CalculateTaxReturns");
            controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "CalculateTaxReturns");
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
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Open the explorer, and get another Workflo
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns", workflowPoint1);

            // See if it's there
            UITestControl theWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "CalculateTaxReturns");
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
            Thread.Sleep(100); // Time to delete
            if (File.Exists(defaultFile))
            {
                Assert.Fail("The file could not be deleted!");
            }
            Process.Start(studioPath);

            // Wait for it to open
            Thread.Sleep(5000);

            // Aaaand re-close it, since the file should have now been created!
            CloseTheStudio();

            if (!File.Exists(defaultFile))
            {
                Assert.Fail("The file was not recreated!");
            }

            // Test over - Re-open the Studio D:
            Process.Start(studioPath);

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

        [TestMethod]
        public void LocalhostContextMenuWhereNewWorkflowClickedExpectNewWorkflowWindow()
        {
            //------------Setup for test--------------------------
            this.DocManagerUIMap.ClickOpenTabPage("Explorer");
            //------------Execute Test---------------------------
            this.ExplorerUIMap.Server_RightClick_NewWorkflow("localhost");
            //------------Assert Results-------------------------
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            string windowTitle = WorkflowWizardUIMap.GetWorkflowWizardName();
            Assert.AreEqual("Workflow Service Details", windowTitle);
            this.WorkflowWizardUIMap.CloseWizard();
        }       
//                
        [TestMethod]
        public void LocalhostContextMenuWhereNewPluginServiceClickedExpectNewWorkflowWindow()
        {
            //------------Setup for test--------------------------
            this.DocManagerUIMap.ClickOpenTabPage("Explorer");
            //------------Execute Test---------------------------
            this.ExplorerUIMap.Server_RightClick_NewPluginService("localhost");
            //------------Assert Results-------------------------
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            string windowTitle = PluginServiceWizardUIMap.GetWorkflowWizardName();
            Assert.AreEqual("Plugin Service Details", windowTitle);
            this.PluginServiceWizardUIMap.CloseWizard();
        }       
//        
//        [TestMethod]
//        public void LocalhostContextMenuWhereNewDatabaseServiceClickedExpectNewWorkflowWindow()
//        {
//            //------------Setup for test--------------------------
//            this.DocManagerUIMap.ClickOpenTabPage("Explorer");
//            //------------Execute Test---------------------------
//            this.ExplorerUIMap.Server_RightClick_NewDatabaseService("localhost");
//            //------------Assert Results-------------------------
//            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
//            var workflowWizardName = this.WebpageServiceWizardUIMap.GetWorkflowWizardName();
//            Assert.AreEqual("Webpage Service Details", workflowWizardName);
//            this.WebpageServiceWizardUIMap.CloseWizard();
//        }

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
        // Sashen.Naidoo - 28-02-2013 - Database Service wizard UI has changed.
        //[TestMethod]
        //public void AllButtonsLaunchRelevantWizards_DatabaseWizard_Expected_DatabaseWizardLaunched()
        //{
        //    RibbonUIMap.ClickRibbonMenuItem("Home", "Database Service");
        //    Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
        //    string windowTitle = DatabaseServiceWizardUIMap.GetWorkflowWizardName();
        //    Assert.AreEqual("Database Service Details", windowTitle);
        //    bool isClosed = this.DatabaseServiceWizardUIMap.CloseWizard();
        //    if (!isClosed)
        //    {
        //        Assert.Fail("Unable to close wizard window");
        //    }
        //}

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

        // Bug 7796
        // xi. Can I deploy to my server? (AKA: Can I deploy?)
        [TestMethod]
        public void CanIDeploy()
        {
            // Open the Explorer tab
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Naviate to the Workflow, and Right click it
            ExplorerUIMap.RightClickDeployProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Wait for the Deploy tab to load!
            Thread.Sleep(5000);

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
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");

            CreateCustomWorkflow("CodedUITestWorkflow");

            // Sashen - 29-11-2012 - Please swap this out to check the WorkflowDesigner for the Workflow name
            UITestControl createdTab = TabManagerUIMap.FindTabByName(workflowName);

            Assert.IsNotNull(createdTab);

            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "CodedUITestWorkflow");
        }

        /// <summary>
        /// This method will create a Custom Workflow
        /// </summary>
        /// <param name="workflowName">The name of the Worfklow you wish to create</param>
        public void CreateCustomWorkflow(string workflowName)
        {
            CreateCustomWorkflow(workflowName, "CodedUITestCategory");
        }

        public void CreateCustomWorkflow(string workflowName, string workflowCategory)
        {
            WpfWindow theWindow = new WpfWindow();
            theWindow.WindowTitles.Add(GetStudioWindowName());
            theWindow.SetFocus();
            Thread.Sleep(500);
            theWindow.SetFocus();
            Thread.Sleep(500);
            #region Workflow Input Parameters

            #endregion Workflow Input Parameters
            RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");

            // Wait due to the way browser hosting occurs, the controls have to be found by their relative
            // position to the window
            while (!WorkflowWizardUIMap.IsWindowOpen())
            {
                Thread.Sleep(1000);
            }
            Thread.Sleep(2000);
            //this.UIMap.DoesThisExist();
            WorkflowWizardUIMap.EnterWorkflowName(workflowName);
            WorkflowWizardUIMap.EnterWorkflowCategory(workflowCategory);


            WorkflowWizardUIMap.DoneButtonClick();

            // Sashen - 29-11-2012 - Please swap this out to check the WorkflowDesigner for the Workflow name
            UITestControl createdTab = TabManagerUIMap.FindTabByName(workflowName);

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
            //CreateCustomWorkflow("CanAToolBeDroppedOntoTheDesigner", "CodedUITestCategory");
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "CanAToolBeDroppedOntoTheDesigner");

            // Get the tab
            UITestControl theTab = TabManagerUIMap.FindTabByName("CanAToolBeDroppedOntoTheDesigner");

            // And click it to make sure it's focused
            //TabManagerUIMap.Click(theTab);

            // Wait a bit for user noticability
            Thread.Sleep(500);

            // Get the location of the Start button
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

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
            UITestControl commentControl = ToolboxUIMap.FindToolboxItemByAutomationId("Comment");

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
        // TODO! -- WHEN?
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
            UITestControl commentControl = ToolboxUIMap.FindToolboxItemByAutomationId("Comment");

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

        // vi - Can I drop a tool onto the designer?
        [TestMethod]
        public void DropAWorkflowOrServiceOnFromTheToolBoxAndTestTheWindowThatPopsUp()
        {
            // Create the Workflow
            CreateCustomWorkflow("WorkflowServiceDropWorkflow", "CodedUITestCategory");
            //DocManagerUIMap.ClickOpenTabPage("Explorer");
            //ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "WorkflowServiceDropWorkflow");

            // Get the tab
            UITestControl theTab = TabManagerUIMap.FindTabByName("WorkflowServiceDropWorkflow");

            // And click it to make sure it's focused
            TabManagerUIMap.Click(theTab);

            // Wait a bit for user noticability
            Thread.Sleep(500);

            // Get the location of the Start button
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

            // And click it for UI responsiveness :P
            WorkflowDesignerUIMap.ClickControl(theStartButton);

            // Get a point underneath the start button
            Point p = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Toolbox");

            // Get the comment box
            UITestControl workflowControl = ToolboxUIMap.FindToolboxItemByAutomationId("Workflow");

            // And drag it onto the point
            ToolboxUIMap.DragControlToWorkflowDesigner(workflowControl, p);

            #region Checking Ok Button enabled property

            //Single click a folder in the tree
            ActivityDropUIMap.SingleClickAFolder();

            //Get the Ok button from the window
            UITestControl buttonControl = ActivityDropUIMap.GetOkButtonOnActivityDropWindow();

            //Assert that the buttton is disabled
            Assert.IsFalse(buttonControl.Enabled);

            //Open the folder in the tree
            ActivityDropUIMap.DoubleClickAFolder();

            //Single click a resource in the tree
            ActivityDropUIMap.SingleClickAResource();

            //get the ok button from the window
            buttonControl = ActivityDropUIMap.GetOkButtonOnActivityDropWindow();

            //Assert that the button is enabled
            Assert.IsTrue(buttonControl.Enabled);

            //Single click on a folder again
            ActivityDropUIMap.SingleClickAFolder();

            //Get the ok button from the window
            buttonControl = ActivityDropUIMap.GetOkButtonOnActivityDropWindow();

            //Assert that the button is disabled
            Assert.IsFalse(buttonControl.Enabled);

            #endregion

            #region Checking the double click of a resource puts it on the design surface

            //Select a resource in the explorer view
            ActivityDropUIMap.DoubleClickAResource();

            // Check if it exists on the designer
            Assert.IsTrue(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "fileTest"));
            SendKeys.SendWait("{DELETE}");

            #endregion

            #region Checking the click of the OK button Adds the resource to the design surface

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Toolbox");

            // Get the comment box
            workflowControl = ToolboxUIMap.FindToolboxItemByAutomationId("Workflow");

            // And drag it onto the point
            ToolboxUIMap.DragControlToWorkflowDesigner(workflowControl, p);

            //Wait for the window to show up
            Thread.Sleep(2000);

            //Single click a folder in the tree
            ActivityDropUIMap.SingleClickAResource();

            //Click the Ok button on the window
            ActivityDropUIMap.ClickOkButton();

            // Check if it exists on the designer
            Assert.IsTrue(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "fileTest"));

            //Delete the resource that was dropped on
            SendKeys.SendWait("{DELETE}");

            #endregion

            #region Checking the click of the Cacnel button doesnt Adds the resource to the design surface

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Toolbox");

            // Get the comment box
            workflowControl = ToolboxUIMap.FindToolboxItemByAutomationId("Workflow");

            // And drag it onto the point
            ToolboxUIMap.DragControlToWorkflowDesigner(workflowControl, p);

            //Wait for the window to show up
            Thread.Sleep(2000);

            //Single click a folder in the tree
            ActivityDropUIMap.SingleClickAResource();

            //Click the Ok button on the window
            ActivityDropUIMap.ClickCancelButton();           

            // Check if it exists on the designer
            Assert.IsFalse(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "fileTest"));

            #endregion

            // Delete the workflow
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickDeleteProject("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "WorkflowServiceDropWorkflow");
        }     


        #endregion Tests Requiring Designer access

        #region Studio Window Tests

        // BUG 9078
        [TestMethod]
        public void StudioExit_Give_TabOpened_Expected_AllRunningProcessStop()
        {
            ProcessManager procMan = new ProcessManager("Dev2.Studio");

            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");
            if (procMan.IsProcessRunning())
            {
                // Exit the Studio
                DocManagerUIMap.CloseStudio();
                // Wait For the Studio to exit
                Thread.Sleep(3000);
                Assert.IsFalse(procMan.IsProcessRunning());
            }
            procMan.StartProcess();
            Thread.Sleep(5000);
        }

        [TestMethod]
        public void HelpTabExpectedHelpTabToBeOpened()
        {
            CreateCustomWorkflow("HelpTabWorkflow", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("HelpTabWorkflow");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

            // Get a point underneath the start button for the workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 150);

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "TestForEachOutput");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Click the help link adorner button

            WorkflowDesignerUIMap.Adorner_ClickHelp(theTab, "TestForEachOutput");

            Assert.IsTrue(TabManagerUIMap.GetActiveTabName() == "Help");

            TabManagerUIMap.CloseTab("Help");

            DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "HelpTabWorkflow");
        }

        #endregion Studio Window Tests

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
            string workspaceId = fileData.Remove(0, fileData.IndexOf("WorkspaceID=", StringComparison.Ordinal) + 13);
            workspaceId = workspaceId.Substring(0, workspaceId.IndexOf("\"", StringComparison.Ordinal));
            return workspaceId;
        }

        private string GetServerEXEFolder()
        {
            var findDev2Studios = Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName.StartsWith("Dev2.Server"));
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
            List<Process> processInstances = Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName.StartsWith(instanceName)).ToList();
            foreach (Process p in processInstances)
            {
                p.Kill();
            }
        }

        /// <summary>
        /// Deletes a service (Workflow) - Generally used at the end of a Coded UI Test
        /// </summary>
        /// <param name="server">The servername (EG: localhost)</param>
        /// <param name="serviceType">The Service Type (Eg: WORKFLOWS)</param>
        /// <param name="category">The Category(EG: CODEDUITESTCATEGORY)</param>
        /// <param name="workflowName">The Workflow Name (Eg: MyCustomWorkflow)</param>
        public void DoCleanup(string server, string serviceType, string category, string workflowName)
        {
            try
            {
                // Test complete - Delete itself
                DocManagerUIMap.ClickOpenTabPage("Explorer");
                //ExplorerUIMap.DoRefresh();

                Thread.Sleep(3500);

                // Delete the workflow
                DocManagerUIMap.ClickOpenTabPage("Explorer");
                ExplorerUIMap.RightClickDeleteProject(server, serviceType, category, workflowName);
            }
            catch (Exception e)
            {
                // Log it so the UI Test still passes...
                Trace.WriteLine(e.Message);
            }

            // Re-refresh the list to clean up
            // Thank to Jurie, this bug has been fixed! \o/ \o/ \o/
            //DocManagerUIMap.ClickOpenTabPage("Explorer");
            //ExplorerUIMap.DoRefresh();
        }

        public static string GetStudioWindowName()
        {
            return "Business Design Studio (DEV2\\" + Environment.UserName + ")";
        }

        private void CloseTheStudio()
        {
            // Close the Studio (Don't kill it)
            WpfWindow theStudio = new WpfWindow();
            theStudio.SearchProperties[WpfWindow.PropertyNames.Name] = GetStudioWindowName();
            theStudio.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            theStudio.WindowTitles.Add(TestBase.GetStudioWindowName());
            theStudio.Find();
            Point closeButtonPoint = new Point(theStudio.BoundingRectangle.Left + theStudio.BoundingRectangle.Width - 10, theStudio.BoundingRectangle.Top + 10);
            Mouse.Click(closeButtonPoint);
            Thread.Sleep(2000); // Give it time to die
        }

        #endregion

        #region Debug Tests

        [TestMethod]
        public void CheckIfDebugProcessingBarIsShowingDurningExecutionExpextedToShowDuringExecutionOnly()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            //Open the correct workflow
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("LargeFileTesting");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "TESTS", "LargeFileTesting");
            ExplorerUIMap.ClearExplorerSearchText();

            UITestControl control1 = OutputUIMap.GetStatusBar();

            UITestControlCollection preStatusBarChildren = control1.GetChildren();
            var preProgressbar = preStatusBarChildren.First(c => c.ClassName == "Uia.CircularProgressBar");
            var preLabel = preStatusBarChildren.First(c => c.ClassName == "Uia.Text");
            Assert.IsTrue(preLabel.FriendlyName == "Ready" || preLabel.FriendlyName == "Complete");
            Assert.IsTrue(preProgressbar.Height == -1);

            RibbonUIMap.ClickRibbonMenuItem("Home", "Debug");
            Thread.Sleep(1000);
            DebugUIMap.ExecuteDebug();
            Thread.Sleep(2000);
            UITestControl control = OutputUIMap.GetStatusBar();

            UITestControlCollection statusBarChildren = control.GetChildren();
            var progressbar = statusBarChildren.First(c => c.ClassName == "Uia.CircularProgressBar");
            var label = statusBarChildren.First(c => c.ClassName == "Uia.Text");
            Assert.IsTrue(label.FriendlyName == "Executing...");
            Assert.IsTrue(progressbar.Height != -1);
        }

        #endregion

        #region DataList View Tests

        [TestMethod]
        public void CheckAddMissingIsWorkingWhenManuallyAddingVariableExpectedToShowVariablesAsUnUsed()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            //Open the correct workflow
            DocManagerUIMap.ClickOpenTabPage("Explorer");            
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("CalculateTaxReturns");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");
            ExplorerUIMap.ClearExplorerSearchText();

            VariablesUIMap.ClickVariableName(0);            
            SendKeys.SendWait("codedUITestVar");
            VariablesUIMap.ClickVariableName(1);
            
            Assert.IsFalse(VariablesUIMap.CheckIfVariableIsUsed(0));
            Assert.IsTrue(VariablesUIMap.CheckIfVariableIsUsed(1));
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
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }
        private TestContext _testContextInstance;

        #region UI Maps

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

        public ToolboxUIMap ToolboxUIMap
        {
            get
            {
                if ((_toolboxUiMap == null))
                {
                    _toolboxUiMap = new ToolboxUIMap();
                }

                return _toolboxUiMap;
            }
        }

        public ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if ((_explorerUiMap == null))
                {
                    _explorerUiMap = new ExplorerUIMap();
                }

                return _explorerUiMap;
            }
        }

        public DeployViewUIMap DeployViewUIMap
        {
            get
            {
                if ((_deployViewUiMap == null))
                {
                    _deployViewUiMap = new DeployViewUIMap();
                }

                return _deployViewUiMap;
            }
        }

        private ExplorerUIMap _explorerUiMap;
        private ToolboxUIMap _toolboxUiMap;
        private DocManagerUIMap _docManagerMap;
        private DeployViewUIMap _deployViewUiMap;


        #region Connect Window UI Map

        public ServerWizard ConnectViewUIMap
        {
            get
            {
                if (_connectViewUIMap == null)
                {
                    _connectViewUIMap = new ServerWizard();
                }
                return _connectViewUIMap;
            }
        }

        private ServerWizard _connectViewUIMap;

        #endregion Connect Window UI Map

        #region Debug UI Map

        public DebugUIMap DebugUIMap
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

        #endregion 

        #region ActivityDrop Window UI Map

        public ActivityDropWindowUIMap ActivityDropUIMap
        {
            get
            {
                if (_activityDropUIMap == null)
                {
                    _activityDropUIMap = new ActivityDropWindowUIMap();
                }
                return _activityDropUIMap;
            }
        }

        private ActivityDropWindowUIMap _activityDropUIMap;

        #endregion 

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
                if (_databaseServiceWizardUIMap == null)
                {
                    _databaseServiceWizardUIMap = new DatabaseServiceWizardUIMap();
                }

                return _databaseServiceWizardUIMap;
            }
        }

        private DatabaseServiceWizardUIMap _databaseServiceWizardUIMap;

        #endregion Database Wizard UI Map
        #region Database Source Wizard UI Map

        public DatabaseSourceUIMap DatabaseSourceWizardUIMap
        {
            get
            {
                if (_databaseSourceWizardUIMap == null)
                {
                    _databaseSourceWizardUIMap = new DatabaseSourceUIMap();
                }

                return _databaseSourceWizardUIMap;
            }
        }

        private DatabaseSourceUIMap _databaseSourceWizardUIMap;

        public PluginSourceMap PluginSourceMap
        {
            get
            {
                if (_pluginSourceWizardUIMap == null)
                {
                    _pluginSourceWizardUIMap = new PluginSourceMap();
                }

                return _pluginSourceWizardUIMap;
            }
        }

        private PluginSourceMap _pluginSourceWizardUIMap;

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

        #region New Server UI Map

        public NewServerUIMap NewServerUIMap
        {
            get
            {
                if (_newServerUIMap == null)
                {
                    _newServerUIMap = new NewServerUIMap();
                }

                return _newServerUIMap;
            }
        }

        private NewServerUIMap _newServerUIMap;

        #endregion Database Wizard UI Map

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
                if (_tabManagerUIMap == null)
                {
                    _tabManagerUIMap = new TabManagerUIMap();
                }

                return _tabManagerUIMap;
            }
        }





        private TabManagerUIMap _tabManagerUIMap;

        #endregion TabManager UI Map

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

        #region Service Details UI Map

        public ServiceDetailsUIMap ServiceDetailsUIMap
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

        #endregion

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

        #region Output UI Map

        public OutputUIMap OutputUIMap
        {
            get
            {
                if (_outputUIMap == null)
                {
                    _outputUIMap = new OutputUIMap();
                }

                return _outputUIMap;
            }
        }

        private OutputUIMap _outputUIMap;

        #endregion Output UI Map

        #region VideoTest UI Map

        public VideoTestUIMap VideoTestUIMap
        {
            get
            {
                if (_videoTestUIMap == null)
                {
                    _videoTestUIMap = new VideoTestUIMap();
                }

                return _videoTestUIMap;
            }
        }

        private VideoTestUIMap _videoTestUIMap;

        #endregion VideoTest UI Map

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

        #endregion UI Maps

        private UIMap map;
    }
}
