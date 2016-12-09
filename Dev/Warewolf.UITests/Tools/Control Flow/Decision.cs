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
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.LargeView.Table.Exists, "Table does not exist on decision large view after dragging tool in from the toolbox.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.LargeView.RequireAllDecisionsToBeTrueCheckbox.Exists, "Require All Decisions To Be True Checkbox does not exist on decision large view after dragging tool in from the toolbox.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.LargeView.DisplayTextbox.Exists, "Display Textbox does not exist on decision large view after dragging tool in from the toolbox.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.LargeView.TrueArmTextbox.Exists, "True Arm Textbox does not exist on decision large view after dragging tool in from the toolbox.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.LargeView.FalseArmTextbox.Exists, "False Arm Textbox does not exist on decision large view after dragging tool in from the toolbox.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.DoneButton.Exists, "Done button does not exist on decision large view after dragging tool in from the toolbox.");
            UIMap.Click_Decision_Dialog_Cancel_Button();
        }
        //[TestMethod]
        //[TestCategory("Tools")]
        //public void DecisionTool_LargeViewResize_UITest()
        //{
        //    var sizeBefore = UIMap.DecisionOrSwitchDialog.LargeView.Height;
        //    UIMap.Resize_Decision_LargeTool();
        //    Assert.IsTrue(UIMap.DecisionOrSwitchDialog.LargeView.Height > sizeBefore);
        //    UIMap.Click_Decision_Dialog_Cancel_Button();
        //    UIMap.Click_Close_Workflow_Tab_Button();
        //    UIMap.Click_MessageBox_No();
        //}

        [TestMethod]
        [TestCategory("Tools")]
        public void DecisionTool_MatchType_Combobox_ListItems_UITest()
        {
            UIMap.Click_Decision_Large_View_Match_Type_Combobox();
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsContains.Exists, "Contains match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntContain.Exists, "Doesnt Contains match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntEndWith.Exists, "Doesnt End With match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntStartWith.Exists, "Doesnt Start With match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsEndsWith.Exists, "EndsWith match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsEquals.Exists, "Equals match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsGreaterThan.Exists, "Greater Than match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsGreaterThanOrEqualTo.Exists, "Greater Than Or Equal To match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsAlphanumeric.Exists, "Alphanumeric match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntEndWith.Exists, "Doesnt End With match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntStartWith.Exists, "Doesnt Start With match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsBase64.Exists, "Is Base 64 match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsBetween.Exists, "Is Between match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsBinary.Exists, "IsB inary match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsDate.Exists, "Is Date match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsEmail.Exists, "Is Email match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsHex.Exists, "Is Hex match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsNULL.Exists, "Is NULL match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsNumeric.Exists, "Is Numeric match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsBase64.Exists, "Is Base64 match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsNotNULL.Exists, "Is Not NULL match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsText.Exists, "Is Text match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsRegex.Exists, "Is Regex match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsXML.Exists, "Is XML match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsNotEmail.Exists, "Not Email match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsNotAlphanumeric.Exists, "Not Alphanumeric match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsNotBase64.Exists, "Not Base64 match type combobox list item does not exist.");
            Assert.IsTrue(UIMap.DecisionOrSwitchDialog.ComboboxListItemAsNotBetween.Exists, "Not Between match type combobox list item does not exist.");
            UIMap.Click_Decision_Dialog_Cancel_Button();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void CopyDecisionsWithContextMenuAndPasteExpectedNoWizardsDisplayed_UITest()
        {
            UIMap.Click_Decision_Dialog_Done_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision.Exists, "Decision on the design surface does not exist after dragging in from the toolbox and clicking done on the dialog.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging in from the toolbox and clicking done on the dialog.");
            UIMap.CopyAndPaste_Decision_Tool_On_The_Designer();
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.DecisionOrSwitchDialog), "Decision large view dialog openned after copy and paste.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.InitializeABlankWorkflow();
            UIMap.Drag_Toolbox_Decision_Onto_DesignSurface();
        }

        public UIMap UIMap
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
