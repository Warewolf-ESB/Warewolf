using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using System.Windows.Input;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class TextboxHeightChanges
    {
        [TestMethod]
        [TestCategory("Debug Input")]
        public void DebugInputTextboxHeightChangesUITest()
        {
            UIMap.Filter_Explorer("Hello World");
            UIMap.Open_Explorer_First_Item_With_Context_Menu();
            UIMap.Press_F5_To_Debug();
            var varValue = UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText;

            var heightBeforeEnterClick = varValue.Height;
            varValue.Text = "Bob";
            Keyboard.SendKeys(varValue, "{Enter}", ModifierKeys.None);
            Assert.IsTrue(varValue.Height > heightBeforeEnterClick, "Debug input dialog does not resize after adding second line.");

            Keyboard.SendKeys(varValue, "{Back}", ModifierKeys.None);
            Assert.AreEqual(heightBeforeEnterClick, varValue.Height, "Debug input dialog value textbox does not resize after deleting second line.");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
        }

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
