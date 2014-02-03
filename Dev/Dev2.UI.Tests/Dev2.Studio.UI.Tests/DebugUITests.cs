using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class DebugUITests : UIMapBase
    {

        #region Cleanup

        [ClassInitialize]
        public static void ClassInit(TestContext tctx)
        {
            Playback.Initialize();
            Playback.PlaybackSettings.ContinueOnError = true;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.DelayBetweenActions = 1;

            // make the mouse quick ;)
            Mouse.MouseMoveSpeed = 10000;
            Mouse.MouseDragSpeed = 10000;
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
            ExplorerUIMap.EnterExplorerSearchText("LargeFileTesting");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "TESTS", "LargeFileTesting");
            Playback.Wait(2500); // extra wait cuz workflow not ready yet ;)

            RibbonUIMap.ClickRibbonMenuItem("Debug");
            DebugUIMap.WaitForDebugWindow(5000);

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
            ExplorerUIMap.EnterExplorerSearchText("Bug9394");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "Bug9394");

            // prime the values ;)
            RibbonUIMap.ClickRibbonMenuItem("Debug");
            PopupDialogUIMap.WaitForDialog();

            Playback.Wait(100);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(200);
            SendKeys.SendWait("1");
            Playback.Wait(200);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(200);
            SendKeys.SendWait("2");

            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution();

            //------------Assert Results-------------------------

            // Check for valid input in the input boxes ;)
            for(int i = 0; i < 9; i++)
            {
                RibbonUIMap.ClickRibbonMenuItem("Debug");
                PopupDialogUIMap.WaitForDialog();

                var getInput = DebugUIMap.GetRow(0).Cells[1].GetChildren()[0] as WpfEdit;
                Assert.AreEqual("1", getInput.Text, "After executing " + i + " times the debug input dialog did not persist");

                getInput = DebugUIMap.GetRow(1).Cells[1].GetChildren()[0] as WpfEdit;
                Assert.AreEqual("2", getInput.Text, "After executing " + i + " times the debug input dialog did not persist");

                DebugUIMap.ClickExecute();
                OutputUIMap.WaitForExecution();
            }

            //Now check the XML tab works ;)
            OutputUIMap.WaitForExecution();
            SendKeys.SendWait(KeyboardCommands.Debug);
            Playback.Wait(200);
            DebugUIMap.ClickXMLTab();
            Playback.Wait(200);

            // flip back and forth to check persistence ;)
            DebugUIMap.ClickInputDataTab();
            Playback.Wait(200);
            DebugUIMap.ClickXMLTab();
            Playback.Wait(200);

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
            ExplorerUIMap.EnterExplorerSearchText("Bug9394");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "Bug9394");

            // prime the values ;)
            RibbonUIMap.ClickRibbonMenuItem("Debug");
            PopupDialogUIMap.WaitForDialog();

            Playback.Wait(100);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(200);
            SendKeys.SendWait("1");
            Playback.Wait(200);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(200);
            SendKeys.SendWait("2");

            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution();
            RibbonUIMap.ClickRibbonMenuItem("Debug");
            PopupDialogUIMap.WaitForDialog();
            DebugUIMap.CloseDebugWindow_ByCancel();
            //---------------Execute------------------------------                
            SendKeys.SendWait("{F6}");
            //------------Assert Results-------------------------
            OutputUIMap.WaitForExecution();
            UITestControlCollection uiTestControlCollection = OutputUIMap.GetOutputWindow();
            Assert.IsTrue(uiTestControlCollection.Count > 1);
            UITestControl executionStep = uiTestControlCollection[uiTestControlCollection.Count - 1];
            string workflowStepName = OutputUIMap.GetStepName(executionStep);
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
                ExplorerUIMap.EnterExplorerSearchText("TravsTestFlow");
                ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "TRAV", "TravsTestFlow");
                Playback.Wait(1000);

                //------------Assert Results-------------------------

                // Check for valid input in the input boxes ;)
                for(int i = 0; i < 9; i++)
                {
                    RibbonUIMap.ClickRibbonMenuItem("Debug");
                    PopupDialogUIMap.WaitForDialog();
                    Playback.Wait(1000);

                    DebugUIMap.ClickExecute();
                    OutputUIMap.WaitForExecution();
                    UITestControlCollection uiTestControlCollection = OutputUIMap.GetOutputWindow();
                    Assert.IsTrue(uiTestControlCollection.Count > 1);
                    UITestControl lastStep = uiTestControlCollection[uiTestControlCollection.Count - 1];
                    string workflowStepName = OutputUIMap.GetStepName(lastStep);
                    Assert.AreEqual("TravsTestFlow", workflowStepName);
                }
            }
            catch(Exception e)
            {
                Assert.Fail("It appears there is a debug issue. [ " + e.Message + " ]");
            }
        }
    }
}
