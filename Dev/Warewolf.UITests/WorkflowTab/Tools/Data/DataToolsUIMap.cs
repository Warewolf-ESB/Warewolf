using TechTalk.SpecFlow;
using Warewolf.UITests.DialogsUIMapClasses;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Drawing;
using System.Windows.Input;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.DotNetPluginSource.DotNetPluginSourceUIMapClasses;
using Warewolf.UITests.ComPluginSource.ComPluginSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Data.DataToolsUIMapClasses
{
    [Binding]
    public partial class DataToolsUIMap
    {
        [When(@"I RightClick FindIndex OnDesignSurface")]
        public void RightClick_FindIndex_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex, MouseButtons.Right, ModifierKeys.None, new Point(113, 8));
        }

        [When(@"I Open Find Index Tool Large View")]
        public void Open_FindIndexTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex, new Point(147, 11));
        }

        public void DeleteAssign_FromContextMenu()
        {
            var point = new Point();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.TryGetClickablePoint(out point));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, MouseButtons.Right, ModifierKeys.None, new Point(115, 10));
            Mouse.Click(UIMap.MainStudioWindow.DesignSurfaceContextMenu.Delete, new Point(27, 18));
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.TryGetClickablePoint(out point));
        }

        [When(@"I Click Assign tool VariableTextbox")]
        public void Click_Assign_tool_VariableTextbox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox);
        }

        [When(@"I Click Assign tool ValueTextbox")]
        public void Click_Assign_tool_ValueTextbox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox);
        }

        [When(@"I Close Data Merge LargeView")]
        public void Open_DataMergeTool_SmallView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge, new Point(257, 7));
        }

        public void Enter_Values_Into_DataMergeTool()
        {
            var row1InputVariabComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Row.InputCell.Row1InputVariabComboBox;
            var row1UsingComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Row.UsingCell.Row1UsingComboBox;
            var row2InputVariabComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Row2.InputCell.Row2InputVariabComboBox;
            var row2UsingComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Row2.UsingCell.Row2UsingComboBox;

            row1InputVariabComboBox.TextEdit.Text = "VarA";
            row1UsingComboBox.TextEdit.Text = "1";
            row2InputVariabComboBox.TextEdit.Text = "VarB";
            row2UsingComboBox.TextEdit.Text = "2";

            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Row3.MergeTypeCell.Row4MergeTypeComboBox, ModifierKeys.None);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Row3.MergeTypeCell.Row4MergeTypeComboBox.NewLineListItem, ModifierKeys.None);
        }

        public void Resize_Assign_LargeTool()
        {
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton.ItemIndicator, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(500, 562));
        }

        [When(@"I Enter SomeData Into Base Convert Large View Row1 Value Textbox")]
        public void Enter_SomeData_Into_BaseConvertTool_Row1ValueTextbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.LargeView.DataGrid.Row1.Cell.Listbox.ValueTextbox.Text = "SomeData";
        }

        [When(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeInvalidVariableName")]
        public void Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableName()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[Some$Invalid%Variable]]";
            Assert.AreEqual("[[Some$Invalid%Variable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[Some$Invalid" +
                    "%Variable]]\".");
        }

        [Given(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable")]
        [When(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable")]
        [Then(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable")]
        public void Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable()
        {
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "{Home}", ModifierKeys.Shift);
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "[[Some{Down}{Enter}Variable]]", ModifierKeys.None);
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Assign large view row1 variable textbox text does not equal \"[[SomeVariable]]\" af" +
                    "ter selecting it from the intellisense using the keyboard.");
        }

        [When(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable On Unpinned Tab")]
        public void Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_Tab()
        {
            Keyboard.SendKeys(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "[[Some{Down}{Enter}Variable]]", ModifierKeys.None);
            Assert.AreEqual("[[SomeVariable]]", UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Assign large view row1 variable textbox text does not equal \"[[SomeVariable]]\" on" +
                    " unpinned tab after selecting it from the intellisense using the keyboard.");
        }

        [When(@"I Enter Text Into Assign Small View Row1 Value Textbox As SomeVariable Using Click Intellisense Suggestion")]
        public void Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_Suggestion()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.ListItem, new Point(39, 10));
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[SomeVariable" +
                    "]]\".");
        }

        [When(@"I Enter Text Into Assign Small View Row1 Value Textbox As SomeVariable UsingIntellisense")]
        public void Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisense()
        {
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "[[{Down}{Enter}", ModifierKeys.None);
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[SomeVariable" +
                    "]]\".");
        }

        [When(@"I Open Assign Tool Large View")]
        public void Open_AssignTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
        }

        [When(@"I Open AssignObject Large Tool")]
        public void Open_AssignObject_Large_Tool()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject, new Point(159, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.DoneButton.Exists, "Done button does not exist after dragging Assign Object tool on to the workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.OpenQuickVariableInput.Exists, "OpenQuickVariableInput button does not exist after dragging Assign Object tool on to the workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row1.Exists, "Row1 does not exist after dragging Assign Object tool on to the workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.OnError.OnErrorGroup.Exists, "OnErrorGroup does not exist after dragging Assign Object tool on to the workflow surface");
        }

        [When(@"I Open Base Conversion Tool Large View")]
        public void Open_BaseConvertTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert, new Point(160, 15));
        }

        [Given(@"I Click Base Convert Large View Done Button")]
        [When(@"I Click Base Convert Large View Done Button")]
        [Then(@"I Click Base Convert Large View Done Button")]
        public void Click_BaseConvertTool_LargeView_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.DoneButton, new Point(36, 11));
        }

        [When(@"I Open Case Conversion Tool Large View")]
        public void Open_CaseConvertTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert, new Point(136, 13));
        }

        [When(@"I Open Data Merge Large View")]
        public void Open_DataMerge_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge, new Point(185, 9));
        }

        public void Click_DataMerge_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.DoneButton);
        }

        [When(@"I Open Data Split Large View")]
        public void Open_DataSplit_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit, new Point(203, 10));
        }

        [When(@"I Open Replace Tool Large View")]
        public void Open_ReplaceTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace, new Point(159, 11));
        }

        [Given(@"I Open Assign Tool QVI View")]
        [When(@"I Open Assign Tool QVI View")]
        [Then(@"I Open Assign Tool QVI View")]
        public void Open_AssignTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.OpenQuickVariableInpToggleButton.Pressed = true;
        }

        [Given(@"I Open AssignObject QVI View")]
        [When(@"I Open AssignObject QVI View")]
        [Then(@"I Open AssignObject QVI View")]
        public void Open_AssignObject_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.OpenQuickVariableInput.Pressed = true;
        }

        [Given(@"I Open Base Conversion Tool QVI View")]
        [When(@"I Open Base Conversion Tool QVI View")]
        [Then(@"I Open Base Conversion Tool QVI View")]
        public void Open_BaseConvertTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.OpenQuickVariableInpToggleButton.Pressed = true;
        }

        [Given(@"I Open Case Conversion Tool QVI View")]
        [When(@"I Open Case Conversion Tool QVI View")]
        [Then(@"I Open Case Conversion Tool QVI View")]
        public void Open_CaseConvertTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.OpenQuickVariableInpToggleButton.Pressed = true;
        }

        [Given(@"I Open Data Merge Tool QVI View")]
        [When(@"I Open Data Merge Tool QVI View")]
        [Then(@"I Open Data Merge Tool QVI View")]
        public void Open_DataMergeTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.QVIToggleButton.Pressed = true;
        }

        [Given(@"I Open Data Split Tool QVI View")]
        [When(@"I Open Data Split Tool QVI View")]
        [Then(@"I Open Data Split Tool QVI View")]
        public void Open_DataSplitTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.OpenQuickVariableInpToggleButton.Pressed = true;
        }

        [Given(@"I RightClick BaseConvert OnDesignSurface")]
        [When(@"I RightClick BaseConvert OnDesignSurface")]
        [Then(@"I RightClick BaseConvert OnDesignSurface")]
        public void RightClick_BaseConvert_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert, MouseButtons.Right, ModifierKeys.None, new Point(148, 12));
        }

        public void RightClick_AssignOnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject, MouseButtons.Right, ModifierKeys.None, new Point(156, 11));

        }

        [Given(@"I RightClick CaseConvert OnDesignSurface")]
        [When(@"I RightClick CaseConvert OnDesignSurface")]
        [Then(@"I RightClick CaseConvert OnDesignSurface")]
        public void RightClick_CaseConvert_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert, MouseButtons.Right, ModifierKeys.None, new Point(156, 10));
        }

        [Given(@"I RightClick DataMerge OnDesignSurface")]
        [When(@"I RightClick DataMerge OnDesignSurface")]
        [Then(@"I RightClick DataMerge OnDesignSurface")]
        public void RightClick_DataMerge_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge, MouseButtons.Right, ModifierKeys.None, new Point(140, 7));
        }

        [Given(@"I RightClick DataSplit OnDesignSurface")]
        [When(@"I RightClick DataSplit OnDesignSurface")]
        [Then(@"I RightClick DataSplit OnDesignSurface")]
        public void RightClick_DataSplit_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit, MouseButtons.Right, ModifierKeys.None, new Point(153, 6));
        }

        [When(@"I RightClick Replace OnDesignSurface")]
        public void RightClick_Replace_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace, MouseButtons.Right, ModifierKeys.None, new Point(100, 7));
        }

        public void Move_Assign_Message_Tool_On_The_Design_Surface()
        {
            var multiAssign1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign;
            multiAssign1.EnsureClickable(new Point(90, 7));
            Mouse.StartDragging(multiAssign1, new Point(94, 11));
            Mouse.StopDragging(multiAssign1, 70, 3);
        }

        [Then(@"I Enter ""(.*)"" in the Assign message tool")]
        [When(@"I Enter ""(.*)"" in the Assign message tool")]
        [Given(@"I Enter ""(.*)"" in the Assign message tool")]
        public void ThenIEnterInTheAssignMessageTool(string message)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = message;
        }

        public void Scroll_DownThenUp_On_DataMergeTool_SmallView()
        {
            Mouse.MoveScrollWheel(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid, -1);
        }

        public void Enter_Recordset_values()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[rec().a]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "5";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row2.VariableCell.IntellisenseCombobox.Textbox.Text = "[[rec().b]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row2.ValueCell.IntellisenseCombobox.Textbox.Text = "10";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row3.VariableCell.IntellisenseCombobox.Textbox.Text = "[[var]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row3.ValueCell.IntellisenseCombobox.Textbox.Text = "1";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row4.VariableCell.IntellisenseCombobox.Textbox.Text = "[[mr()]]";
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.Exists, "var does not exist in the variable explorer");
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field1.Exists, "rec().a does not exist in the variable explorer");
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field2.Exists, "rec().b does not exist in the variable explorer");
        }

        public void Delete_Assign_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, MouseButtons.Right, ModifierKeys.None, new Point(115, 10));
            Mouse.Click(UIMap.MainStudioWindow.DesignSurfaceContextMenu.Delete, new Point(27, 18));
            Assert.IsFalse(UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView), "Assign tool still exists on design surface after deleting with context menu.");
        }

        public void Remove_Assign_Row_1_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.RowNumberCell.Text, MouseButtons.Right, ModifierKeys.None, new Point(5, 5));
            Mouse.Click(UIMap.MainStudioWindow.DesignSurfaceContextMenu.DeleteRowMenuItem, MouseButtons.Left, ModifierKeys.None, new Point(6, 6));
            Assert.IsFalse(UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3), "Assign tool row 3 still exists after deleting row 1.");
        }

        [Given(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1")]
        [When(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1")]
        [Then(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1")]
        public void Enter_Variable_And_Value_Into_Assign(string VariableText, string ValueText)
        {
            Enter_Variable_And_Value_Into_Assign(VariableText, ValueText, 1);
        }

        public void Enter_Variable_And_Value_Into_Assign(string VariableText, string ValueText, int RowNumber)
        {
            switch (RowNumber)
            {
                case 2:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2();
                    Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3.Exists, "Assign row 3 does not exist after enter data into row 2.");
                    break;
                default:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1();
                    Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.Exists, "Assign row 2 does not exist after enter data into row 1.");
                    break;
            }
        }

        public void Enter_Person_Age_On_Assign_Object_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row2.FieldCell.FieldNameComboBox.TextEdit.Text = "[[@Person.Age]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row2.FieldValueCell.FieldValueComboBox.TextEdit.Text = "10";
        }

        public void Enter_Text_Into_Assign_QviLarge_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.QviVariableListBoxEdit.Text = "varOne,varTwo,varThree";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.QviSplitOnCharacterEdit.Text = ",";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.PrefixEdit.Text = "some(<).";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.SuffixEdit.Text = "_suf";
        }

        public void Enter_Values_Into_Data_Split_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.SmallViewContentCustom.SourceStringComboBox.TextEdit.Text = "some long string here";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.SmallViewContentCustom.SmallDataGridTable.Row1.ValueCell.ValueComboBox.TextEdit.Text = "[[res]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.SmallViewContentCustom.SmallDataGridTable.Row1.AtIndexCell.AtIndexComboBox.TextEdit.Text = "5";
        }

        public void Enter_Values_Into_Replace_Tool_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.InFiledsComboBox.TextEdit.Text = "[[rec().a]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.FindComboBox.TextEdit.Text = "u";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.ReplaceComboBox.TextEdit.Text = "o";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "res";
        }

        public void Enter_Values_Into_FindIndex_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.InFieldComboBox.TextEdit.Text = "SomeLongString";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.IndexComboBox, new Point(85, 13));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.IndexComboBox, new Point(62, 19));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit.Text = "r";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit, new Point(45, 2));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit, new Point(39, 12));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit, "{Escape}", ModifierKeys.None);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "res";
        }

        public void Enter_Person_Name_On_Assign_Object_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row1.FieldCell.FieldNameComboBox.TextEdit.Text = "[[@Person.Name]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row1.FieldValueCell.FieldValueComboBox.TextEdit.Text = "Bob";
        }

        public void Enter_Values_Into_Case_Conversion_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.SmallViewContentCustom.SmallDataGridTable.Row1.ValueCell.ValueComboBox.TextEdit.Text = "res";
        }

        [Given(@"I Assign Value To Variable With Assign Tool Small View Row 2")]
        [When(@"I Assign Value To Variable With Assign Tool Small View Row 2")]
        [Then(@"I Assign Value To Variable With Assign Tool Small View Row 2")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeOtherVariable]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.ValueCell.IntellisenseCombobox.Textbox.Text = "100";
        }

        [Given(@"I Assign Value To Variable With Assign Tool Small View Row 1")]
        [When(@"I Assign Value To Variable With Assign Tool Small View Row 1")]
        [Then(@"I Assign Value To Variable With Assign Tool Small View Row 1")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeVariable]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "50";
        }

        [Given(@"I Click Assign Tool Large View Done Button")]
        [When(@"I Click Assign Tool Large View Done Button")]
        [Then(@"I Click Assign Tool Large View Done Button")]
        public void Click_Assign_Tool_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.OpenQuickVariableInpToggleButton.Exists, "QVI toggle button does not exist in assign tool small view after clicking done button on large view.");
        }

        [Given(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName")]
        [When(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName")]
        [Then(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName")]
        public void Click_Assign_Tool_Large_View_Done_Button_With_Row1_Variable_Textbox_As_SomeInvalidVariableName()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Error1.Hyperlink.Exists, "Error popup does not exist on flowchart designer.");
            Assert.AreEqual("", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text, "Variable list scalar row 1 textbox text is not blank with invalid variable.");
        }

        [Given(@"I click AssignObject Done")]
        [When(@"I click AssignObject Done")]
        [Then(@"I click AssignObject Done")]
        public void click_AssignObject_Done()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.DoneButton, new Point(18, 10));
        }

        [Given(@"I Click Assign Tool QviLarge Preview")]
        [When(@"I Click Assign Tool QviLarge Preview")]
        [Then(@"I Click Assign Tool QviLarge Preview")]
        public void Click_Assign_Tool_QviLarge_Preview()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.PreviewCustom.PreviewGroup.PreviewButton, new Point(30, 4));
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

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        DotNetPluginSourceUIMap DotNetPluginSourceUIMap
        {
            get
            {
                if (_DotNetPluginSourceUIMap == null)
                {
                    _DotNetPluginSourceUIMap = new DotNetPluginSourceUIMap();
                }

                return _DotNetPluginSourceUIMap;
            }
        }

        private DotNetPluginSourceUIMap _DotNetPluginSourceUIMap;

        ComPluginSourceUIMap ComPluginSourceUIMap
        {
            get
            {
                if (_ComPluginSourceUIMap == null)
                {
                    _ComPluginSourceUIMap = new ComPluginSourceUIMap();
                }

                return _ComPluginSourceUIMap;
            }
        }

        private ComPluginSourceUIMap _ComPluginSourceUIMap;

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
    }
}
