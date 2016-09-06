using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.DebugInputWindow
{
    [CodedUITest]
    public class SaveDebugInputs
    {
        private const string InputDataText = "Coded UI Test";

        [TestMethod]
        public void SaveDebugInputsUITest()
        {
            Uimap.Enter_Text_Into_Explorer_Filter("Hello World");
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Uimap.Open_Explorer_First_Item_With_Context_Menu();
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.MainStudioWindow.DebugInputDialog.RememberDebugInputCheckBox.Checked = true;
            Uimap.Enter_Text_Into_Debug_Input_Row1_Value_Textbox(InputDataText);
            Uimap.Click_DebugInput_Cancel_Button();
            Uimap.Click_Debug_Ribbon_Button();
            Assert.AreEqual(InputDataText, Uimap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.Cell.ComboBox.Textbox.Text, "Cancelling and re-openning the debug input dialog loses input values.");
            Uimap.Click_DebugInput_Debug_Button();
            Uimap.Click_Debug_Ribbon_Button();
            Assert.AreEqual(InputDataText, Uimap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.Cell.ComboBox.Textbox.Text, "Debugging Hello World workflow and then re-openning the debug input dialog loses input values.");
        }

        #region Additional test attributes
        
        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitForStudioStart();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + Environment.MachineName);
        }
        
        [TestCleanup()]
        public void MyTestCleanup()
        {
            Playback.PlaybackError -= Uimap.OnError;
            Uimap.TryCloseHangingDebugInputDialog();
            Uimap.TryCloseWorkflowTabs();
            Uimap.TryClearExplorerFilter();
        }

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

        UIMap Uimap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
