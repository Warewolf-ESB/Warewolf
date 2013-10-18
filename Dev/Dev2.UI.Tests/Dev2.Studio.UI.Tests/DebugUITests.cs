using System.Windows.Forms;
using Dev2.Studio.UI.Tests.UIMaps;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class DebugUITests : UIMapBase
    {
        #region Cleanup

        [TestCleanup]
        public void MyTestCleanup()
        {
            Playback.Wait(500);
            if (StudioWindow.GetChildren()[0].ControlType == "Window")
            {
                DebugUIMap.CloseDebugWindow_ByCancel();
            }
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DebugInput_whenRun10Time")]
        public void DebugInput_WhenRun10Times_ExpectInputsPersistAndXMLRemainsLinked_InputsAndXMLRemainPersisted()
        {
            //------------Setup for test--------------------------
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            //Open the correct workflow
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Bug9394");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "Bugs", "Bug9394");

            //------------Execute Test---------------------------
            // Run debug
            RibbonUIMap.ClickRibbonMenuItem("Debug");
            PopupDialogUIMap.WaitForDialog();
            SendKeys.SendWait("{TAB}1{TAB}2");
            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution();

            //------------Assert Results-------------------------

            // Check for valid input in the input boxes ;)
            for(int i = 0; i < 10; i++)
            {
                Playback.Wait(500);

                RibbonUIMap.ClickRibbonMenuItem("Debug");
                PopupDialogUIMap.WaitForDialog();

                var getInput = DebugUIMap.GetRow(0).Cells[1].GetChildren()[0] as WpfEdit;
                Assert.AreEqual("1", getInput.Text, "After executing " + i + " times the debug input dialog did not persist");

                getInput = DebugUIMap.GetRow(1).Cells[1].GetChildren()[0] as WpfEdit;
                Assert.AreEqual("2", getInput.Text, "After executing " + i + " times the debug input dialog did not persist");

                DebugUIMap.ClickExecute();
                OutputUIMap.WaitForExecution();
            }

            // Now check the XML tab works ;)
            OutputUIMap.WaitForExecution();
            SendKeys.SendWait(KeyboardCommands.Debug);
            Playback.Wait(500);
            DebugUIMap.ClickXMLTab();
            Playback.Wait(500);

            // flip back and forth to check persistence ;)
            DebugUIMap.ClickInputDataTab();
            Playback.Wait(500);
            DebugUIMap.ClickXMLTab();
            Playback.Wait(500);

            SendKeys.SendWait(KeyboardCommands.TabCommand);
            Playback.Wait(500);
            SendKeys.SendWait(KeyboardCommands.SelectAllCommand);
            Playback.Wait(500);
            SendKeys.SendWait(KeyboardCommands.CopyCommand);
            var actualXML = Clipboard.GetData(DataFormats.Text);

            // close the window ;)
            DebugUIMap.CloseDebugWindow_ByCancel();

            var expectedXML = @"<DataList>
  <var>1</var>
  <countries>
    <CountryID>2</CountryID>
    <Description></Description>
  </countries>
</DataList>";

            Assert.AreEqual(expectedXML, actualXML);
        }
    }
}
