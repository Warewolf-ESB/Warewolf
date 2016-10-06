using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.DebugInputWindow
{
    [CodedUITest]
    public class SaveDebugInputs
    {
        private const string InputDataText = "Coded UI Test";
        const string HelloWorld = "Hello World";

        [TestMethod]
        public void SaveDebugInputsUITest()
        {
            UIMap.Filter_Explorer(HelloWorld);
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            UIMap.Open_Explorer_First_Item_With_Context_Menu();
            UIMap.Click_Debug_Ribbon_Button();
            UIMap.MainStudioWindow.DebugInputDialog.RememberDebugInputCheckBox.Checked = true;
            UIMap.Enter_Text_Into_Debug_Input_Row1_Value_Textbox(InputDataText);
            UIMap.Click_DebugInput_Cancel_Button();
            UIMap.Click_Debug_Ribbon_Button();
            Assert.AreEqual(InputDataText, UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text, "Cancelling and re-openning the debug input dialog loses input values.");
            UIMap.Click_DebugInput_Debug_Button();
            UIMap.Click_Debug_Ribbon_Button();
            Assert.AreEqual(InputDataText, UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text, "Debugging Hello World workflow and then re-openning the debug input dialog loses input values.");
        }

        #region Additional test attributes
        
        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + Environment.MachineName);
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

        UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        #endregion
    }
}
