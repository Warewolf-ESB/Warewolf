using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Control_Flow
{
    [CodedUITest]
    public class Decision
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void DecisionTool_LargeView_UITest()
        {
            Uimap.Open_Decision_Large_View();
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.LargeView.Exists);
            Uimap.Click_Decision_Dialog_Cancel_Button();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void DecisionTool_MatchType_Combobox_ListItems_UITest()
        {
            Uimap.Open_Decision_Large_View();
            Mouse.Click(Uimap.DecisionOrSwitchDialog.LargeView.Table.Row1.MatchTypeCell.MatchTypeCombobox, new Point(5, 5));
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsContains.Exists, "Contains match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntContain.Exists, "Doesnt Contains match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntEndWith.Exists, "Doesnt End With match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntStartWith.Exists, "Doesnt Start With match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsEndsWith.Exists, "EndsWith match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsEquals.Exists, "Equals match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsGreaterThan.Exists, "Greater Than match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsGreaterThanOrEqualTo.Exists, "Greater Than Or Equal To match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsAlphanumeric.Exists, "Alphanumeric match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntEndWith.Exists, "Doesnt End With match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntStartWith.Exists, "Doesnt Start With match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsBase64.Exists, "Is Base 64 match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsBetween.Exists, "Is Between match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsBinary.Exists, "IsB inary match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsDate.Exists, "Is Date match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsEmail.Exists, "Is Email match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsHex.Exists, "Is Hex match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsNULL.Exists, "Is NULL match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsNumeric.Exists, "Is Numeric match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsBase64.Exists, "Is Base64 match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsNotNULL.Exists, "Is Not NULL match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsText.Exists, "Is Text match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsRegex.Exists, "Is Regex match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsIsXML.Exists, "Is XML match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsNotEmail.Exists, "Not Email match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsNotAlphanumeric.Exists, "Not Alphanumeric match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsNotBase64.Exists, "Not Base64 match type combobox list item does not exist.");
            Assert.IsTrue(Uimap.DecisionOrSwitchDialog.ComboboxListItemAsNotBetween.Exists, "Not Between match type combobox list item does not exist.");
            Uimap.Click_Decision_Dialog_Cancel_Button();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void CopyDecisionsWithContextMenuAndPasteExpectedNoWizardsDisplayed_UITest()
        {
            Uimap.CopyAndPaste_Decision_Tool_On_The_Designer();
            Assert.IsFalse(Uimap.DecisionOrSwitchDialog.Exists);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
            Uimap.InitializeABlankWorkflow();
            Uimap.Drag_Toolbox_Decision_Onto_DesignSurface();
            Uimap.Click_Decision_Dialog_Done_Button();
        }
        

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
