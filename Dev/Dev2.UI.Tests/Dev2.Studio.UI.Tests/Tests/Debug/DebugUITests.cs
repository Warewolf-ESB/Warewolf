using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests.UIMaps;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Tests.Debug
{
    [CodedUITest]
    public class DebugUITests : UIMapBase
    {

        #region Cleanup

        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion


        [TestMethod]
        public void CheckIfDebugProcessingBarIsShowingDurningExecutionExpectedToShowDuringExecutionOnly()
        {
            //Open the correct workflow
            ExplorerUIMap.DoubleClickWorkflow("LargeFileTesting", "TESTS");

            RibbonUIMap.ClickDebug();

            DebugUIMap.ClickExecute();

            var status = OutputUIMap.GetStatusBarStatus();
            var spinning = OutputUIMap.IsSpinnerSpinning();
            Assert.AreEqual("Executing", status, "Debug output status text does not say executing when executing");
            Assert.IsTrue(spinning, "Debug output spinner not spinning during execution");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DebugInput_whenRun10Time")]
        public void DebugInput_WhenRun10Times_ExpectInputsPersistAndXMLRemainsLinked_InputsAndXMLRemainPersisted()
        {
            // Remove the PersistSettings.dat ;)
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "");
            var settingPath = Path.Combine(appData, @"Local\Warewolf\DebugData\PersistSettings.dat");

            if(File.Exists(settingPath))
            {
                File.Delete(settingPath);
            }

            //------------Setup for test--------------------------
            //Open the correct workflow
            ExplorerUIMap.DoubleClickWorkflow("Bug9394", "BUGS");
            // prime the values ;)
            RibbonUIMap.ClickDebug();
            DebugUIMap.EnterTextIntoRow(0, "1");
            DebugUIMap.EnterTextIntoRow(1, "2");

            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution();

            //------------Assert Results-------------------------

            // Check for valid input in the input boxes ;)
            for(int i = 0; i < 9; i++)
            {
                RibbonUIMap.ClickDebug();
                DebugUIMap.WaitForDebugWindow(7000);

                Assert.AreEqual("1", DebugUIMap.GetTextFromRow(0), "After executing " + i + " times the debug input dialog did not persist");
                Assert.AreEqual("2", DebugUIMap.GetTextFromRow(1), "After executing " + i + " times the debug input dialog did not persist");


                DebugUIMap.ClickExecute();
                OutputUIMap.WaitForExecution();
            }

            //Now check the XML tab works ;)

            RibbonUIMap.ClickDebug();
            DebugUIMap.ClickXMLTab();

            // flip back and forth to check persistence ;)
            DebugUIMap.ClickInputDataTab();
            DebugUIMap.ClickXMLTab();

            SendKeys.SendWait(KeyboardCommands.TabCommand);
            Playback.Wait(200);
            SendKeys.SendWait(KeyboardCommands.SelectAllCommand);
            Playback.Wait(200);
            Clipboard.Clear();
            SendKeys.SendWait(KeyboardCommands.CopyCommand);
            var actualXML = Clipboard.GetData(DataFormats.Text);

            actualXML = actualXML.ToString().Replace(Environment.NewLine, "").Replace(" ", "");

            //close the window ;)
            DebugUIMap.CloseDebugWindow_ByCancel();

            const string expectedXML = @"<DataList><countries><CountryID>1</CountryID><Description>2</Description></countries></DataList>";

            Assert.AreEqual(expectedXML, actualXML, "Got [ " + actualXML + " ]");

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Debug_QuickDebug")]
        public void Debug_WhenUsingQuickDebugCommand_ExpectSavedInputsUsedInExecution()
        {

            // Remove the PersistSettings.dat ;)
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "");
            var settingPath = Path.Combine(appData, @"Local\Warewolf\DebugData\PersistSettings.dat");

            if(File.Exists(settingPath))
            {
                File.Delete(settingPath);
            }

            //------------Setup for test--------------------------
            //Open the correct workflow
            ExplorerUIMap.DoubleClickWorkflow("Bug9394", "BUGS");

            // prime the values ;)
            RibbonUIMap.ClickDebug();

            DebugUIMap.EnterTextIntoRow(0, "1");
            DebugUIMap.EnterTextIntoRow(1, "2");

            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution();
            RibbonUIMap.ClickDebug();
            DebugUIMap.CloseDebugWindow_ByCancel();
            //---------------Execute------------------------------                
            SendKeys.SendWait(KeyboardCommands.QuickDebug);
            //------------Assert Results-------------------------
            OutputUIMap.WaitForExecution();
            UITestControl lastStep = OutputUIMap.GetLastStep();
            string workflowStepName = OutputUIMap.GetStepName(lastStep);
            Assert.AreEqual("Bug9394", workflowStepName);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DebugOutput_whenRun10Time")]
        public void DebugOutput_WhenRun10Times_NormalExecution_CloseTagsReturned10Times()
        {
            try
            {
                //------------Setup for test--------------------------
                //Open the correct workflow
                ExplorerUIMap.DoubleClickWorkflow("TravsTestFlow", "TRAV");

                //------------Assert Results-------------------------

                // Check for valid input in the input boxes ;)
                for(int i = 0; i < 9; i++)
                {
                    RibbonUIMap.ClickDebug();

                    DebugUIMap.ClickExecute();
                    OutputUIMap.WaitForExecution(2500);
                    UITestControl lastStep = OutputUIMap.GetLastStep();
                    string workflowStepName = OutputUIMap.GetStepName(lastStep);
                    Assert.AreEqual("TravsTestFlow", workflowStepName);
            
                }
            }
            catch(Exception e)
            {
                Assert.Fail("It appears there is a debug issue. [ " + e.Message + " ]");
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DebugOutput_ContainsWorkflowOutput")]
        public void DebugOutput_WhenRunWithOutputs_ContainsWorkflowOutput()
        {
            try
            {
                //------------Setup for test--------------------------
                //Open the correct workflow
                ExplorerUIMap.DoubleClickWorkflow("TravsTestFlow", "TRAV");

                //------------Assert Results-------------------------

                // Check for valid input in the input boxes ;)
  
                    RibbonUIMap.ClickDebug();

                    DebugUIMap.ClickExecute();
                    OutputUIMap.WaitForExecution(2500);
                    UITestControl lastStep = OutputUIMap.GetLastStep();
                    string workflowStepName = OutputUIMap.GetStepName(lastStep);
                    Assert.AreEqual("TravsTestFlow", workflowStepName);
                    Assert.IsTrue(OutputUIMap.AssertDebugOutputContains(lastStep, new[] { "Outputs :", "[[a]]","=","1" }));
                    
                
            }
            catch (Exception e)
            {
                Assert.Fail("It appears there is a debug issue. [ " + e.Message + " ]");
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DebugOutput_ContainsWorkflowOutput")]
        public void DebugOutput_WhenRunWithOutputs_ContainsWorkflowInput()
        {
            try
            {
                //------------Setup for test--------------------------
                //Open the correct workflow
                ExplorerUIMap.DoubleClickWorkflow("TravsTestFlow", "TRAV");

                //------------Assert Results-------------------------

                // Check for valid input in the input boxes ;)

                RibbonUIMap.ClickDebug();

                DebugUIMap.ClickExecute();
                OutputUIMap.WaitForExecution(2500);
                UITestControl lastStep = OutputUIMap.GetStep(1);
                string workflowStepName = OutputUIMap.GetStepName(lastStep);
                Assert.AreEqual("TravsTestFlow", workflowStepName);
                Assert.IsTrue(OutputUIMap.AssertDebugOutputContains(lastStep, new[] { "Inputs :", "[[a]]", "=" }));


            }
            catch (Exception e)
            {
                Assert.Fail("It appears there is a debug issue. [ " + e.Message + " ]");
            }
        }

        [TestMethod]
        [Owner("Jai Holloway")]
        [TestCategory("DebugOutput_WhenStopped")]
        public void DebugOutput_WhenStopped_WaitsForRenderToCompleteBeforeStoppedMessage()
        {
            try
            {
                //------------Setup for test--------------------------
                //Open the correct workflow
                ExplorerUIMap.DoubleClickWorkflow("CodedUI_DebugOutputStop", "Tests");
                string status = OutputUIMap.GetStatusBarStatus();
                Assert.AreEqual("Ready", status);
                UITestControl debugButton = RibbonUIMap.ClickDebug();
                Point debugButtonPoint = new Point(debugButton.BoundingRectangle.X + 5, debugButton.BoundingRectangle.Y + 5);
                DebugUIMap.ClickExecute(1500);
                MouseCommands.ClickPoint(debugButtonPoint, 500);

                //------------Assert Results-------------------------

                status = OutputUIMap.GetStatusBarStatus();
                StringAssert.Contains(status, "Ready");
            }
            catch(Exception e)
            {
                Assert.Fail("It appears there is a debug issue. [ " + e.Message + " ]");
            }
        }
    }
}
