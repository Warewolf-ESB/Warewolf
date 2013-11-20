using System.Windows.Forms;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
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
            //Playback.Initialize();
            //Playback.PlaybackSettings.ContinueOnError = true;
            //Playback.PlaybackSettings.ShouldSearchFailFast = true;
            //Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            //Playback.PlaybackSettings.MatchExactHierarchy = true;

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

            RibbonUIMap.ClickRibbonMenuItem("Debug");
            if(DebugUIMap.WaitForDebugWindow(5000))
            {
                SendKeys.SendWait("{F5}");
                Playback.Wait(1000);
            }

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
                //------------Setup for test--------------------------
                //Open the correct workflow
                ExplorerUIMap.EnterExplorerSearchText("Bug9394");
                ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "Bug9394");

                //------------Assert Results-------------------------

                // Check for valid input in the input boxes ;)
                for (int i = 0; i < 10; i++)
                {
                    RibbonUIMap.ClickRibbonMenuItem("Debug");
                    PopupDialogUIMap.WaitForDialog();

                    var getInput = DebugUIMap.GetRow(0).Cells[1].GetChildren()[0] as WpfEdit;
                    Assert.AreEqual("1", getInput.Text,"After executing " + i + " times the debug input dialog did not persist");

                    getInput = DebugUIMap.GetRow(1).Cells[1].GetChildren()[0] as WpfEdit;
                    Assert.AreEqual("2", getInput.Text, "After executing " + i + " times the debug input dialog did not persist");

                    DebugUIMap.ClickExecute();
                    OutputUIMap.WaitForExecution();
                }

                // Now check the XML tab works ;)
                //OutputUIMap.WaitForExecution();
                //SendKeys.SendWait(KeyboardCommands.Debug);
                //Playback.Wait(200);
                //DebugUIMap.ClickXMLTab();
                //Playback.Wait(200);

                //// flip back and forth to check persistence ;)
                //DebugUIMap.ClickInputDataTab();
                //Playback.Wait(200);
                //DebugUIMap.ClickXMLTab();
                //Playback.Wait(200);

                //SendKeys.SendWait(KeyboardCommands.TabCommand);
                //Playback.Wait(200);
                //SendKeys.SendWait(KeyboardCommands.SelectAllCommand);
                //Playback.Wait(200);
                //Clipboard.Clear();
                //SendKeys.SendWait(KeyboardCommands.CopyCommand);
                //var actualXML = Clipboard.GetData(DataFormats.Text);

                // close the window ;)
                //DebugUIMap.CloseDebugWindow_ByCancel();

//                const string expectedXML = @"<DataList>
//  <countries>
//    <CountryID>1</CountryID>
//    <Description>2</Description>
//  </countries>
//</DataList>";

//                Assert.AreEqual(expectedXML, actualXML);

        }
    }
}
