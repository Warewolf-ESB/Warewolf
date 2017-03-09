using TechTalk.SpecFlow;
using Warewolf.UITests.DialogsUIMapClasses;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Drawing;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UITesting;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.DotNetPluginSource.DotNetPluginSourceUIMapClasses;
using Warewolf.UITests.ComPluginSource.ComPluginSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.ControlFlow.ControlFlowToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Scripting.ScriptingToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Resources.ResourcesToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Recordset.RecordsetToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.LoopConstructs.LoopConstructToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.HTTPWebMethods.HTTPToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.FileFTPFTPSSFTP.FileToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Email.EmailToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Database.DatabaseToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Utility.UtilityToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.StorageDropbox.DropboxToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Sharepoint.SharepointToolsUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses
{
    [Binding]
    public partial class WorkflowTabUIMap
    {
        [Given(@"I Click PrefixContainsInvalidText Hyperlink")]
        [When(@"I Click PrefixContainsInvalidText Hyperlink")]
        [Then(@"I Click PrefixContainsInvalidText Hyperlink")]
        public void Click_PrefixContainsInvalidText_Hyperlink()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PrefixcontainsinvaliText.PrefixcontainsinvaliHyperlink, new Point(30, 4));
        }

        [Given(@"I Click ExpandAndStepIn NestedWorkflow")]
        [When(@"I Click ExpandAndStepIn NestedWorkflow")]
        [Then(@"I Click ExpandAndStepIn NestedWorkflow")]
        public void Click_ExpandAndStepIn_NestedWorkflow()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.SubWorkflow.Expanded = true;
        }

        [Given(@"I Click Debug Output Calculate Cell")]
        [When(@"I Click Debug Output Calculate Cell")]
        [Then(@"I Click Debug Output Calculate Cell")]
        public void Click_Debug_Output_Calculate_Cell()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.CalculateButton, new Point(24, 10));
            Assert.AreEqual("IsPrimarySelection=True IsSelection=True IsCurrentLocation=null IsCurrentContext=" +
                            "null IsBreakpointEnabled=null IsBreakpointBounded=null ValidationState=Valid ", UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.ItemStatus, "Calculate tool small view is not selected.");
        }

        [Given(@"I Click Debug Output GenericResource Name")]
        [When(@"I Click Debug Output GenericResource Name")]
        [Then(@"I Click Debug Output GenericResource Name")]
        public void Click_Debug_Output_GenericResource_Name()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DrawHighlight();
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.ServiceTreeItem.GenericResourceButton, new Point(24, 8));
            Assert.AreEqual("GenericResource", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Workflow1ciremoteText.DisplayText, "GenericResource remote workflow tab is not open.");
        }

        [Given(@"I Click Debug Output BaseConvert Cell")]
        [When(@"I Click Debug Output BaseConvert Cell")]
        [Then(@"I Click Debug Output BaseConvert Cell")]
        public void Click_Debug_Output_BaseConvert_Cell()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.BaseConversion1Button, new Point(33, 7));
            Assert.AreEqual("IsPrimarySelection=True IsSelection=True IsCurrentLocation=null IsCurrentContext=" +
                            "null IsBreakpointEnabled=null IsBreakpointBounded=null ValidationState=Valid ", DataToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.ItemStatus, "Base conversion small view is not selected.");
        }

        [Given(@"I Click Debug Output Assign Cell")]
        [When(@"I Click Debug Output Assign Cell")]
        [Then(@"I Click Debug Output Assign Cell")]
        public void Click_Debug_Output_Assign_Cell()
        {
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.VariableTextbox2.DisplayText, "Wrong variable name in debug output");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.Assign1Button, new Point(21, 9));
            Assert.AreEqual("IsPrimarySelection=True IsSelection=True IsCurrentLocation=null IsCurrentContext=" +
                            "null IsBreakpointEnabled=null IsBreakpointBounded=null ValidationState=Valid ", DataToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus, "Multiassign small view is not selected.");
        }

        [Given(@"I Click Assign Tool CollapseAll")]
        [When(@"I Click Assign Tool CollapseAll")]
        [Then(@"I Click Assign Tool CollapseAll")]
        public void Click_Assign_Tool_CollapseAll()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.CollapseAllToggleButton.Exists, "Expand all button does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.CollapseAllToggleButton.Pressed = true;
        }

        [Given(@"I Click Assign Tool ExpandAll")]
        [When(@"I Click Assign Tool ExpandAll")]
        [Then(@"I Click Assign Tool ExpandAll")]
        public void Click_Assign_Tool_ExpandAll()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Exists, "Expand all button does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Pressed = true;
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
        }

        public void Connect_Assign_to_Next_tool()
        {
            Mouse.Hover(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(200, 220));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 220));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, 0, 44);
        }

        public void Delete_HelloWorld_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow, MouseButtons.Right, ModifierKeys.None, new Point(115, 10));
            Mouse.Click(UIMap.MainStudioWindow.DesignSurfaceContextMenu.Delete, new Point(27, 18));
        }

        public void Click_Assign_Tool_Remove_Variable_From_Tool()
        {
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
            DataToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeOtherVariable]]";
            Keyboard.SendKeys(DataToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "{Right}{Tab}", ModifierKeys.None);
            Assert.AreEqual("[[Some$Invalid%Variable]]", DataToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[Some$Invalid%Variable]]\".");
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Exists, "Assign large view row 1 variable textbox does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Exists, "Variable filter textbox does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Text = "Other";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.ClearSearchButton.Exists, "Variable clear filter button does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.ClearSearchButton, new Point(8, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.DeleteButton.Exists, "Variable delete does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.DeleteButton, new Point(9, 8));
        }

        [Given("I Click Subworkflow Done Button")]
        [When("I Click Subworkflow Done Button")]
        [Then("I Click Subworkflow Done Button")]
        public void Click_Subworkflow_Done_Button()
        {
            Assert.IsTrue(
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView
                    .DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExternalWorkFlow
                    .DoneButton.Exists, "Done button does not exist afer dragging dice service onto design surface");
            Mouse.Click(
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView
                    .DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExternalWorkFlow
                    .DoneButton, new Point(53, 16));
        }

        [When(@"I Click Start Node")]
        public void Click_Start_Node()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode, new Point(29, 76));
        }

        [When(@"I Click Variable IsInput")]
        public void Click_Variable_IsInput()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Checked = true;
        }

        [When(@"I Click VariableList Scalar Row1 IsInputCheckbox")]
        public void Click_VariableList_Scalar_Row1_IsInputCheckbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Checked = true;
        }

        [When(@"I Click VariableList Scalar Row2 IsInputCheckbox")]
        public void Click_VariableList_Scalar_Row2_IsInputCheckbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.InputCheckbox.Checked = true;
        }

        [When(@"I Click VariableList Recordset Row1 IsInputCheckbox")]
        public void Click_VariableList_Recordset_Row1_IsInputCheckbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field1.InputCheckbox.Checked = true;
        }

        [When(@"I Click VariableList Scalar Row1 Delete Button")]
        public void Click_VariableList_Scalar_Row1_Delete_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.DeleteButton.Image, new Point(5, 8));
        }

        [When(@"I DisplayStartNodeContextMenu")]
        public void DisplayStartNodeContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode, MouseButtons.Right, ModifierKeys.None, new Point(179, 31));
        }

        [When(@"I Drag Dice Onto Dice On The DesignSurface")]
        public void Drag_Dice_Onto_Dice_On_The_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(353, 479));
            Mouse.StartDragging(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(49, 10));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(353, 479));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(353, 479));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector2.Exists, "Second connector does not exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExternalWorkFlow.DoneButton.Exists, "Done button does not exist afer dragging dice service onto design surface");
        }

        [When(@"I Drag DotNet DLL Connector Onto DesignSurface")]
        public void Drag_DotNetDLLConnector_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "DotNet DLL";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ResourceTools.DotNetDLL, new Point(16, 25));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Com DLL Connector Onto DesignSurface")]
        public void Drag_ComDLLConnector_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Com DLL";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ResourceTools.ComDLL, new Point(16, 25));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag WCF Service Connector Onto DesignSurface")]
        public void Drag_WCFServiceConnector_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "WCF";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ResourceTools.WCF, new Point(16, 25));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Explorer Localhost First Item Onto Workflow Design Surface")]
        public void Drag_Explorer_Localhost_First_Item_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "No items to drag found in the explorer tree.");
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Explorer Localhost First Items First Sub Item Onto Workflow Design Surface")]
        public void Drag_Explorer_Localhost_First_Items_First_Sub_Item_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists, "No items to drag found in the explorer tree.");
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(90, 10));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(90, 10));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Explorer Localhost Second Items First Sub Item Onto Workflow Design Surface")]
        public void Drag_Explorer_Localhost_Second_Item_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.Exists, "No items to drag found in the explorer tree.");
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, new Point(90, 10));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, new Point(90, 10));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }


        [Given(@"I Drag Explorer Remote GenericResource Onto Workflow Design Surface")]
        [When(@"I Drag Explorer Remote GenericResource Onto Workflow Design Surface")]
        [Then(@"I Drag Explorer Remote GenericResource Onto Workflow Design Surface")]
        public void Drag_Explorer_Remote_workflow1_Onto_Workflow_Design_Surface()
        {
            if (ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit.Text != "GenericResource")
                ExplorerUIMap.Click_Explorer_Refresh_Button();
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit.Text == "GenericResource", "Explorer first remote server first item is not Generic Resource.");
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(64, 5));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(64, 5));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SubWorkflow.Exists, "Workflow on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [Given(@"I Drag Explorer workflow Onto Workflow Design Surface")]
        [When(@"I Drag Explorer workflow Onto Workflow Design Surface")]
        [Then(@"I Drag Explorer workflow Onto Workflow Design Surface")]
        public void Drag_Explorer_workflow_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Explorer first remote server does not contain any items.");
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.Exists, "Workflow on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I change Hello World input variable")]
        public void Change_HelloWorld_InputVariable()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.ServiceDesignerLargeView.InputsDataGridTable.InputsGridRowOne.InputsGridRowOneCell.InputsAutoCompleteTextBox.InputsAutoCompleteTextBoxText.Text = "NewName";
            Keyboard.SendKeys("{Tab}", ModifierKeys.None);
            Assert.AreEqual("NewName", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.ServiceDesignerLargeView.InputsDataGridTable.InputsGridRowOne.InputsGridRowOneCell.InputsAutoCompleteTextBox.InputsAutoCompleteTextBoxText.Text);
        }

        [When(@"I change Hello World output variable")]
        public void Change_HelloWorld_OutputVariable()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.ServiceDesignerLargeView.OutputsDataGridTable.OutputsGridRowOne.OutputsGridRowOneCell.OutputsAutoCompleteTextBox.OutputsAutoCompleteTextBoxText.Text = "NewMessage";
            Keyboard.SendKeys("{Tab}", ModifierKeys.None);
            Assert.AreEqual("[[NewMessage]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.ServiceDesignerLargeView.OutputsDataGridTable.OutputsGridRowOne.OutputsGridRowOneCell.OutputsAutoCompleteTextBox.OutputsAutoCompleteTextBoxText.Text);
        }

        [When(@"I open ""(.*)"" in Remote Connection Integration")]
        public void WhenIOpenInRemoteConnectionIntegration(string resourceName)
        {
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.Exists, "Explorer first remote server does not contain any items.");
            Mouse.DoubleClick(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(64, 5));
        }

        [When(@"I Drag Toolbox AggregateCalculate Onto DesignSurface")]
        public void Drag_Toolbox_AggregateCalculate_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Aggregate Calculate";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.AggregateCalculate, new Point(13, 17));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox AssignObject Onto DesignSurface")]
        public void Drag_Toolbox_AssignObject_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign Object";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject, new Point(13, 17));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Base Conversion Onto DesignSurface")]
        public void Drag_Toolbox_Base_Conversion_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Base Convert";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.BaseConvert, new Point(12, 12));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Calculate Onto DesignSurface")]
        public void Drag_Toolbox_Calculate_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Calculate";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Calculate, new Point(59, -17));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Case Conversion Onto DesignSurface")]
        public void Drag_Toolbox_Case_Conversion_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Case Convert";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.CaseConvert, new Point(19, 13));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 130));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox CMD Line Onto DesignSurface")]
        public void Drag_Toolbox_CMD_Line_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "CMD Script";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 122));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.CMDScript, new Point(19, 19));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 122));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 122));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Comment Onto DesignSurface")]
        public void Drag_Toolbox_Comment_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Comment";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment, new Point(40, 15));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Copy Onto DesignSurface")]
        public void Drag_Toolbox_Copy_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Copy";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(310, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Copy, new Point(19, -3));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(310, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(310, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Count Records Onto DesignSurface")]
        public void Drag_Toolbox_Count_Records_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Count";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 125));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Count, new Point(13, 18));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 125));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Create Onto DesignSurface")]
        public void Drag_Toolbox_Create_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Create";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Create, new Point(9, 16));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 131));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Data Merge Onto DesignSurface")]
        public void Drag_Toolbox_Data_Merge_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Data Merge";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 133));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.DataMerge, new Point(54, 23));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 133));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 133));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Data Split Onto DesignSurface")]
        public void Drag_Toolbox_Data_Split_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Data Split";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.DataSplit, new Point(3, 8));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [Given(@"I Drag Toolbox Date And Time Onto DesignSurface")]
        [When(@"I Drag Toolbox Date And Time Onto DesignSurface")]
        [Then(@"I Drag Toolbox Date And Time Onto DesignSurface")]
        public void Drag_Toolbox_Date_And_Time_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Date Time";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.DateTime, new Point(20, -1));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 127));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox DateTime Difference Onto DesignSurface")]
        public void Drag_Toolbox_DateTime_Difference_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Date Time Diff";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.DateTimeDifference, new Point(48, 7));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Decision Onto DesignSurface")]
        public void Drag_Toolbox_Decision_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Decision";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision, new Point(16, 11));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
        }

        [When(@"I Drag Toolbox Delete Onto DesignSurface")]
        public void Drag_Toolbox_Delete_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 125));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Delete, new Point(13, 9));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 125));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Delete Record Onto DesignSurface")]
        public void Drag_Toolbox_Delete_Record_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Delete, new Point(43, 6));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Dropbox Delete Onto DesignSurface")]
        public void Drag_Toolbox_Dropbox_Delete_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Dropbox";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.StorageDropbox.Delete, new Point(240, 550));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 127));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Dropbox Download Onto DesignSurface")]
        public void Drag_Toolbox_Dropbox_Download_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Dropbox";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.StorageDropbox.Download, new Point(16, 6));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Dropbox FileList Onto DesignSurface")]
        public void Drag_Toolbox_Dropbox_FileList_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Dropbox";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.StorageDropbox.ListContents, new Point(124, 550));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Exchange Send Onto DesignSurface")]
        public void Drag_Toolbox_Exchange_Send_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Exchange Send";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Email.ExchangeSend, new Point(16, -39));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Find Index Onto DesignSurface")]
        public void Drag_Toolbox_Find_Index_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Find Index";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.FindIndex, new Point(9, 5));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Find Record Index Onto DesignSurface")]
        public void Drag_Toolbox_Find_Record_Index_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Find Records";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.FindRecords, new Point(8, 8));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox For Each Onto DesignSurface")]
        public void Drag_Toolbox_For_Each_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "ForEach";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.LoopTools.ForEach, new Point(40, 19));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Format Number Onto DesignSurface")]
        public void Drag_Toolbox_Format_Number_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Format Number";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.FormatNumber, new Point(18, 11));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox HTTPGETWebTool Onto DesignSurface")]
        public void Drag_HTTPGETWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "GET";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.GET, new Point(16, 25));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox HTTPPOSTWebTool Onto DesignSurface")]
        public void Drag_HTTPPOSTWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "POST";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.POST, new Point(20, 35));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox HTTPPUTWebTool Onto DesignSurface")]
        public void Drag_HTTPPUTWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "PUT";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.PUT, new Point(16, 25));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox HTTPDELETEWebTool Onto DesignSurface")]
        public void Drag_HTTPDELETEWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "DELETE";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.DELETE, new Point(16, 25));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Javascript Onto DesignSurface")]
        public void Drag_Toolbox_Javascript_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Javascript";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.JavaScript, new Point(49, 17));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox JSON Onto DesignSurface")]
        public void Drag_Toolbox_JSON_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Create JSON";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.CreateJSON, new Point(0, 10));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 127));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson.Exists, "Create JSON tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Length Onto DesignSurface")]
        public void Drag_Toolbox_Length_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Length";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 125));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Length, new Point(16, 6));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 125));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Move Onto DesignSurface")]
        public void Drag_Toolbox_Move_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Move";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Move, new Point(32, 4));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [Given(@"I Drag Toolbox MultiAssign Onto DesignSurface")]
        [When(@"I Drag Toolbox MultiAssign Onto DesignSurface")]
        [Then(@"I Drag Toolbox MultiAssign Onto DesignSurface")]
        public void Drag_Toolbox_MultiAssign_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.MultiAssign, new Point(13, 17));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [Given(@"I Make Workflow Savable")]
        [When(@"I Make Workflow Savable")]
        [Then(@"I Make Workflow Savable")]
        public void Make_Workflow_Savable_By_Dragging_Start()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode.Exists, "Start Node does not Exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode);
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
        }

        [Given(@"I Make Workflow Savable And Then Save")]
        [When(@"I Make Workflow Savable And Then Save")]
        [Then(@"I Make Workflow Savable And Then Save")]
        public void Make_Workflow_Savable_Then_Save()
        {
            var startNode = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode;
            Assert.IsTrue(startNode.Exists, "Start Node does not Exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(startNode);
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Make Workflow Savable was unsucessful.");
            Keyboard.SendKeys(startNode, "S", (ModifierKeys.Control));
        }

        [When(@"I Drag Toolbox MySql Database Onto DesignSurface")]
        public void Drag_Toolbox_MySql_Database_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "MySQL";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.MySQL, new Point(10, 15));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox ODBC Dtatbase Onto DesignSurface")]
        public void Drag_Toolbox_ODBC_Dtatbase_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "ODBC";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.ODBC, new Point(10, 15));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Oracle Database Onto DesignSurface")]
        public void Drag_Toolbox_Oracle_Database_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Oracle";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.Oracle, new Point(11, 20));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox PostgreSql Onto DesignSurface")]
        public void Drag_Toolbox_PostgreSql_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Postgre";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.Postgre, new Point(10, 15));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Python Onto DesignSurface")]
        public void Drag_Toolbox_Python_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Python";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.Python, new Point(49, 17));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox RabbitMqConsume Onto DesignSurface")]
        public void Drag_Toolbox_RabbitMqConsume_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "RabbitMq Consume";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.RabbitMQConsume, new Point(16, 11));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox RabbitMqPublish Onto DesignSurface")]
        public void Drag_Toolbox_RabbitMqPublish_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "RabbitMq Publish";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.RabbitMQPublish, new Point(16, 11));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Random Onto DesignSurface")]
        public void Drag_Toolbox_Random_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Random";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Random, new Point(9, -21));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Read File Onto DesignSurface")]
        public void Drag_Toolbox_Read_File_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read File";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 125));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.ReadFile, new Point(12, 15));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 125));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Read Folder Onto DesignSurface")]
        public void Drag_Toolbox_Read_Folder_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read Folder";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.ReadFolder, new Point(14, 3));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Rename Onto DesignSurface")]
        public void Drag_Toolbox_Rename_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Rename";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Rename, new Point(6, 11));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Replace Onto DesignSurface")]
        public void Drag_Toolbox_Replace_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Replace";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 121));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.Replace, new Point(16, 10));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 121));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 121));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Ruby Onto DesignSurface")]
        public void Drag_Toolbox_Ruby_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Ruby";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.Ruby, new Point(49, 17));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox SelectAndApply Onto DesignSurface")]
        public void Drag_Toolbox_SelectAndApply_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Select and apply";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.LoopTools.Selectandapply, new Point(40, 19));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging select and apply tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sequence Onto DesignSurface")]
        public void Drag_Toolbox_Sequence_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Sequence";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Sequence, new Point(18, -12));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Service Picker Onto DesignSurface")]
        public void Drag_Toolbox_Service_Picker_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Service";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ResourceTools.Service, new Point(50, 5));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 126));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 126));
        }

        [Given(@"I Drag Toolbox Sharepoint CopyFile Onto DesignSurface")]
        [When(@"I Drag Toolbox Sharepoint CopyFile Onto DesignSurface")]
        [Then(@"I Drag Toolbox Sharepoint CopyFile Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_CopyFile_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Copy File";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.CopyFile, new Point(10, 16));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Create Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Create_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Create List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(311, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.CreateListItems, new Point(10, 16));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }


        [When(@"I Drag Toolbox Sharepoint Delete List Item Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_DeleteListItem_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.DeleteListItems, new Point(16, 5));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Delete File Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_DeleteFile_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete File";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.DeleteFile, new Point(16, 5));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Download File Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Download_File_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Download";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.DownloadFile, new Point(124, 593));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint MoveFile Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_MoveFile_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Move";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(311, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.MoveFile, new Point(10, 16));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Read Folder Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Read_Folder_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.ReadFolder, new Point(13, 15));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Read List Item Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Read_List_Item_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.ReadListItems, new Point(13, 15));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Update Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Update List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(300, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.UpdateListItems, new Point(17, 9));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 127));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint UploadFile Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_UploadFile_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Upload";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(311, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.UploadFile, new Point(10, 16));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox SMTP Email Onto DesignSurface")]
        public void Drag_Toolbox_SMTP_Email_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "SMTP Send";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Email.SMTPSend, new Point(16, -39));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sort Record Onto DesignSurface")]
        public void Drag_Toolbox_Sort_Record_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Sort";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(300, 122));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Sort, new Point(7, 8));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 122));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 122));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox SQL Bulk Insert Onto DesignSurface")]
        public void Drag_Toolbox_SQL_Bulk_Insert_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "SQL Bulk Insert";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.SQLBulkInsert, new Point(10, 15));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }
        
        [When(@"I Drag Toolbox SQL Server Tool Onto DesignSurface")]
        public void Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "SQL Server";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.SQLServer, new Point(10, -7));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 127));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Switch Onto DesignSurface")]
        public void Drag_Toolbox_Switch_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Switch";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch, new Point(22, 30));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 126));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 126));
        }

        [When(@"I Drag Toolbox System Information Onto DesignSurface")]
        public void Drag_Toolbox_System_Information_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Sys Info";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.SysInfo, new Point(8, 12));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 129));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Unique Records Onto DesignSurface")]
        public void Drag_Toolbox_Unique_Records_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Unique";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 133));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.UniqueRecords, new Point(43, 6));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 133));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 133));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Unzip Onto DesignSurface")]
        public void Drag_Toolbox_Unzip_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Unzip";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.UnZip, new Point(15, 15));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Web Request Onto DesignSurface")]
        public void Drag_Toolbox_Web_Request_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Web Request";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.WebRequest, new Point(14, 3));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 128));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Write File Onto DesignSurface")]
        public void Drag_Toolbox_Write_File_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Write File";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 132));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.WriteFile, new Point(10, 18));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 132));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 132));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox XPath Onto DesignSurface")]
        public void Drag_Toolbox_XPath_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "XPath";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 123));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.XPath, new Point(12, -13));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 123));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 123));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Zip Onto DesignSurface")]
        public void Drag_Toolbox_Zip_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Zip";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Zip, new Point(16, 4));
            Mouse.Move(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Enter Duplicate workflow name")]
        public void Enter_Duplicate_workflow_name(string name)
        {
            DialogsUIMap.SaveDialogWindow.ServiceNameTextBox.Text = name;
        }

        [When(@"I Enter InputDebug value")]
        public void Enter_InputDebug_value()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.Exists, "InputData row does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Exists, "InputData row does not exist.");
            UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text = "100";
        }

        [When(@"I Enter Vaiablelist Items")]
        public void Enter_Vaiablelist_Items()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox, new Point(62, 3));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text = "varableA";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox, "{CapsLock}", ModifierKeys.None);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox, new Point(82, 2));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox, "{CapsLock}", ModifierKeys.None);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox.Text = "variableB";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox, new Point(84, 2));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox.Text = "VariableC";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox, "{CapsLock}", ModifierKeys.None);
        }

        [When(@"I Filter variables")]
        public void Filter_variables()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.FilterText.Exists, "Variable filter textbox does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox, new Point(89, 7));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Text = "Other";
        }

        [When(@"I Add Variables The Variable List")]
        [Given(@"I Add Variables The Variable List")]
        [Then(@"I Add Variables The Variable List")]
        public void Add_Variables(string variables)
        {
            var strings = variables.Split(',');
            if (!string.IsNullOrEmpty(strings[0]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text = strings?[0];
            if (!string.IsNullOrEmpty(strings[1]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox.Text = strings?[1];
            if (!string.IsNullOrEmpty(strings[2]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox.Text = strings?[2];
            if (!string.IsNullOrEmpty(strings[3]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem4.ScrollViewerPane.NameTextbox.Text = strings?[3];
        }

        [When(@"I Add Recordset The Recordset List")]
        [Given(@"I Add Recordset The Recordset List")]
        [Then(@"I Add Recordset The Recordset List")]
        public void Add_Recordsets(string variables)
        {
            var strings = variables.Split(',');
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text = strings?[0];
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem2.ScrollViewerPane.NameTextbox.Text = strings?[1];
        }


        [When(@"I Add Recordset Fields The Recordset List")]
        [Given(@"I Add Recordset Fields The Recordset List")]
        [Then(@"I Add Recordset Fields The Recordset List")]
        public void Add_Recordsets_Fields(string variables)
        {
            var strings = variables.Split(',');
            if (!string.IsNullOrEmpty(strings[0]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem2.Field1.ScrollViewerPane.NameTextbox.Text = strings?[0];
            if (!string.IsNullOrEmpty(strings[1]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem2.Field2.ScrollViewerPane.NameTextbox.Text = strings?[1];
            if (!string.IsNullOrEmpty(strings[0]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field1.ScrollViewerPane.NameTextbox.Text = strings?[0];
            if (!string.IsNullOrEmpty(strings[1]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field2.ScrollViewerPane.NameTextbox.Text = strings?[1];
            if (!string.IsNullOrEmpty(strings[2]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field3.ScrollViewerPane.NameTextbox.Text = strings?[2];
            if (!string.IsNullOrEmpty(strings[3]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field4.ScrollViewerPane.NameTextbox.Text = strings?[3];
        }

        [Given(@"I Sort Variable List")]
        [When(@"I Sort Variable List")]
        [Then(@"I Sort Variable List")]
        public void Click_Sort_Variable_List()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.Sort);
        }

        [When(@"I Open Assign Tool On Unpinned Tab Large View")]
        public void Open_Assign_Tool_On_Unpinned_Tab_Large_View()
        {
            Mouse.DoubleClick(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, new Point(145, 5));
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the unpinned tab design surface does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Exists, "Assign large view row 1 variable textbox does not exist after openning large view" +
                    " with a double click on an unpinned tab.");
        }

        [When(@"I Open Context Menu On Design Surface")]
        public void Open_Context_Menu_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, MouseButtons.Right, ModifierKeys.None, new Point(304, 286));
        }

        [When(@"I Open Create JSON Large View")]
        public void Open_CreateJSON_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson, new Point(124, 9));
        }

        [Given(@"I Right Click On The Folder Count")]
        [When(@"I Right Click On The Folder Count")]
        [Then(@"I Right Click On The Folder Count")]
        public void Right_Click_On_The_Folder_Count()
        {
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(211, 8));
        }

        [When(@"I Open Json Tool Large View")]
        public void Open_Json_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson, new Point(158, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson.Exists, "JSON tool large view on the design surface does not exist");
        }

        [Given(@"I Open Json Tool QVI View")]
        [When(@"I Open Json Tool QVI View")]
        [Then(@"I Open Json Tool QVI View")]
        public void Open_JSONTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson.OpenQuickVariableInpToggleButton.Pressed = true;
        }

        [Given(@"I RightClick STACKOVERFLOWTESTWORKFLOW OnDesignSurface")]
        [When(@"I RightClick STACKOVERFLOWTESTWORKFLOW OnDesignSurface")]
        [Then(@"I RightClick STACKOVERFLOWTESTWORKFLOW OnDesignSurface")]
        public void RightClick_StackOverFlowService_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.stackOverflowTestWF, MouseButtons.Right, ModifierKeys.None, new Point(181, 11));
        }

        [Given(@"I RightClick CreateJSON OnDesignSurface")]
        [When(@"I RightClick CreateJSON OnDesignSurface")]
        [Then(@"I RightClick CreateJSON OnDesignSurface")]
        public void RightClick_CreateJSON_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson, MouseButtons.Right, ModifierKeys.None, new Point(128, 9));
        }

        [When(@"I RightClick New Workflow Tab")]
        public void RightClick_New_Workflow_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab, MouseButtons.Right, ModifierKeys.None, new Point(63, 18));
        }

        [When(@"I Make Workflow Savable")]
        public void Make_Workflow_Savable()
        {
            Drag_Toolbox_Comment_Onto_DesignSurface();
        }

        [Then(@"I Click workflow tab")]
        [Given(@"I Click workflow tab")]
        [When(@"I Click workflow tab")]
        public void ThenIClickWorkflowWizardTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Exists);
        }

        [Given(@"I Save Workflow Using Shortcut")]
        [When(@"I Save Workflow Using Shortcut")]
        [Then(@"I Save Workflow Using Shortcut")]
        public void Save_Workflow_Using_Shortcut()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(74, 8));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, "S", (ModifierKeys.Control));
        }

        [Then(@"There is an error")]
        public void TheArdonerhasAnError()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Adornert_numbernText.Exists, "The tool is valid and did not throw any errors on Test Clicked.");
        }

        [Given(@"I RightClick Ardoner Hyperlink")]
        [When(@"I RightClick Ardoner Hyperlink")]
        [Then(@"I RightClick Ardoner Hyperlink")]
        public void RightClick_Adorner_Control()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Adornert_numbernText.NumbernHyperlink, MouseButtons.Right, ModifierKeys.None, new Point(88, 12));
        }

        [When(@"I Drag Start Node")]
        public void Drag_Start_Node()
        {
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode, new Point(186, 30));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode, 1400, 968);
        }

        #region UIMaps

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

        DataToolsUIMap DataToolsUIMap
        {
            get
            {
                if (_DataToolsUIMap == null)
                {
                    _DataToolsUIMap = new DataToolsUIMap();
                }

                return _DataToolsUIMap;
            }
        }

        private DataToolsUIMap _DataToolsUIMap;

        DatabaseToolsUIMap DatabaseToolsUIMap
        {
            get
            {
                if (_DatabaseToolsUIMap == null)
                {
                    _DatabaseToolsUIMap = new DatabaseToolsUIMap();
                }

                return _DatabaseToolsUIMap;
            }
        }

        private DatabaseToolsUIMap _DatabaseToolsUIMap;

        EmailToolsUIMap EmailToolsUIMap
        {
            get
            {
                if (_EmailToolsUIMap == null)
                {
                    _EmailToolsUIMap = new EmailToolsUIMap();
                }

                return _EmailToolsUIMap;
            }
        }

        private EmailToolsUIMap _EmailToolsUIMap;

        FileToolsUIMap FileToolsUIMap
        {
            get
            {
                if (_FileToolsUIMap == null)
                {
                    _FileToolsUIMap = new FileToolsUIMap();
                }

                return _FileToolsUIMap;
            }
        }

        private FileToolsUIMap _FileToolsUIMap;

        HTTPToolsUIMap HTTPToolsUIMap
        {
            get
            {
                if (_HTTPToolsUIMap == null)
                {
                    _HTTPToolsUIMap = new HTTPToolsUIMap();
                }

                return _HTTPToolsUIMap;
            }
        }

        private HTTPToolsUIMap _HTTPToolsUIMap;

        LoopConstructToolsUIMap LoopConstructToolsUIMap
        {
            get
            {
                if (_LoopConstructToolsUIMap == null)
                {
                    _LoopConstructToolsUIMap = new LoopConstructToolsUIMap();
                }

                return _LoopConstructToolsUIMap;
            }
        }

        private LoopConstructToolsUIMap _LoopConstructToolsUIMap;

        RecordsetToolsUIMap RecordsetToolsUIMap
        {
            get
            {
                if (_RecordsetToolsUIMap == null)
                {
                    _RecordsetToolsUIMap = new RecordsetToolsUIMap();
                }

                return _RecordsetToolsUIMap;
            }
        }

        private RecordsetToolsUIMap _RecordsetToolsUIMap;

        ResourcesToolsUIMap ResourcesToolsUIMap
        {
            get
            {
                if (_ResourcesToolsUIMap == null)
                {
                    _ResourcesToolsUIMap = new ResourcesToolsUIMap();
                }

                return _ResourcesToolsUIMap;
            }
        }

        private ResourcesToolsUIMap _ResourcesToolsUIMap;

        ScriptingToolsUIMap ScriptingToolsUIMap
        {
            get
            {
                if (_ScriptingToolsUIMap == null)
                {
                    _ScriptingToolsUIMap = new ScriptingToolsUIMap();
                }

                return _ScriptingToolsUIMap;
            }
        }

        private ScriptingToolsUIMap _ScriptingToolsUIMap;

        SharepointToolsUIMap SharepointToolsUIMap
        {
            get
            {
                if (_SharepointToolsUIMap == null)
                {
                    _SharepointToolsUIMap = new SharepointToolsUIMap();
                }

                return _SharepointToolsUIMap;
            }
        }

        private SharepointToolsUIMap _SharepointToolsUIMap;

        DropboxToolsUIMap DropboxToolsUIMap
        {
            get
            {
                if (_DropboxToolsUIMap == null)
                {
                    _DropboxToolsUIMap = new DropboxToolsUIMap();
                }

                return _DropboxToolsUIMap;
            }
        }

        private DropboxToolsUIMap _DropboxToolsUIMap;

        UtilityToolsUIMap UtilityToolsUIMap
        {
            get
            {
                if (_UtilityToolsUIMap == null)
                {
                    _UtilityToolsUIMap = new UtilityToolsUIMap();
                }

                return _UtilityToolsUIMap;
            }
        }

        private UtilityToolsUIMap _UtilityToolsUIMap;
    }

    #endregion
}
