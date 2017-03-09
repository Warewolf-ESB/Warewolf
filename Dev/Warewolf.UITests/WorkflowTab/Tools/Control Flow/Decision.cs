using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.ControlFlow.ControlFlowToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Control_Flow
{
    [CodedUITest]
    public class Decision
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void DecisionTool_LargeViewResize_UITest()
        {
            //Large View
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.LargeView.Table.Exists, "Table does not exist on decision large view after dragging tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.LargeView.RequireAllDecisionsToBeTrueCheckbox.Exists, "Require All Decisions To Be True Checkbox does not exist on decision large view after dragging tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.LargeView.DisplayTextbox.Exists, "Display Textbox does not exist on decision large view after dragging tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.LargeView.TrueArmTextbox.Exists, "True Arm Textbox does not exist on decision large view after dragging tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.LargeView.FalseArmTextbox.Exists, "False Arm Textbox does not exist on decision large view after dragging tool in from the toolbox.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.DoneButton.Exists, "Done button does not exist on decision large view after dragging tool in from the toolbox.");
            //Resized Decision Tool Window
            var sizeBefore = DialogsUIMap.DecisionOrSwitchDialog.LargeView.Height;
            DialogsUIMap.Resize_Decision_LargeTool();
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.LargeView.Height > sizeBefore);
            DialogsUIMap.Click_Decision_Dialog_Cancel_Button();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void DecisionTool_MatchType_Combobox_ListItems_UITest()
        {
            ControlFlowToolsUIMap.Click_Decision_Large_View_Match_Type_Combobox();
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsContains.Exists, "Contains match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntContain.Exists, "Doesnt Contains match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntEndWith.Exists, "Doesnt End With match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntStartWith.Exists, "Doesnt Start With match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsEndsWith.Exists, "EndsWith match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsEquals.Exists, "Equals match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsGreaterThan.Exists, "Greater Than match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsGreaterThanOrEqualTo.Exists, "Greater Than Or Equal To match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsAlphanumeric.Exists, "Alphanumeric match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntEndWith.Exists, "Doesnt End With match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsDoesntStartWith.Exists, "Doesnt Start With match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsBase64.Exists, "Is Base 64 match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsBetween.Exists, "Is Between match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsBinary.Exists, "IsB inary match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsDate.Exists, "Is Date match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsEmail.Exists, "Is Email match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsHex.Exists, "Is Hex match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsNULL.Exists, "Is NULL match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsNumeric.Exists, "Is Numeric match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsBase64.Exists, "Is Base64 match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsNotNULL.Exists, "Is Not NULL match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsText.Exists, "Is Text match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsRegex.Exists, "Is Regex match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsIsXML.Exists, "Is XML match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsNotEmail.Exists, "Not Email match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsNotAlphanumeric.Exists, "Not Alphanumeric match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsNotBase64.Exists, "Not Base64 match type combobox list item does not exist.");
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.ComboboxListItemAsNotBetween.Exists, "Not Between match type combobox list item does not exist.");
            DialogsUIMap.Click_Decision_Dialog_Cancel_Button();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void CopyDecisions_With_ContextMenu_And_Paste_UITest()
        {
            DialogsUIMap.Click_Decision_Dialog_Done_Button();
            Assert.IsTrue(ControlFlowToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision.Exists, "Decision on the design surface does not exist after dragging in from the toolbox and clicking done on the dialog.");
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging in from the toolbox and clicking done on the dialog.");
            ControlFlowToolsUIMap.CopyAndPaste_Decision_Tool_On_The_Designer();
            Assert.IsFalse(UIMap.ControlExistsNow(DialogsUIMap.DecisionOrSwitchDialog), "Decision large view dialog openned after copy and paste.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Decision_Onto_DesignSurface();
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

        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_WorkflowTabUIMap == null)
                {
                    _WorkflowTabUIMap = new WorkflowTabUIMap();
                }

                return _WorkflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _WorkflowTabUIMap;

        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;

        ControlFlowToolsUIMap ControlFlowToolsUIMap
        {
            get
            {
                if (_ControlFlowToolsUIMap == null)
                {
                    _ControlFlowToolsUIMap = new ControlFlowToolsUIMap();
                }

                return _ControlFlowToolsUIMap;
            }
        }

        private ControlFlowToolsUIMap _ControlFlowToolsUIMap;

        #endregion
    }
}
