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

namespace Warewolf.UITests.WorkflowTab.Tools.HTTPWebMethods.HTTPToolsUIMapClasses
{
    [Binding]
    public partial class HTTPToolsUIMap
    {
        [Given(@"I Enter invalid data on the DELETE Web Large View")]
        [When(@"I Enter invalid data on the DELETE Web Large View")]
        [Then(@"I Enter invalid data on the DELETE Web Large View")]
        public void Enter_Invalid_Data_DELETEWebTool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart
                .WebDelete.LargeView.Table.ItemRow1.HeaderCell.HeaderComboBox.TextEdit.Text = "stupidData";
        }

        [Given(@"I Click GET Web Tool Outputs Done Button")]
        [When(@"I Click GET Web Tool Outputs Done Button")]
        [Then(@"I Click GET Web Tool Outputs Done Button")]
        public void Click_GETWebTool_Outputs_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click POST Web Tool Outputs Done Button")]
        [When(@"I Click POST Web Tool Outputs Done Button")]
        [Then(@"I Click POST Web Tool Outputs Done Button")]
        public void Click_POSTWebTool_Outputs_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click PUT Web Tool Outputs Done Button")]
        [When(@"I Click PUT Web Tool Outputs Done Button")]
        [Then(@"I Click PUT Web Tool Outputs Done Button")]
        public void Click_PUTWebTool_Outputs_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click DELETE Web Tool Outputs Done Button")]
        [When(@"I Click DELETE Web Tool Outputs Done Button")]
        [Then(@"I Click DELETE Web Tool Outputs Done Button")]
        public void Click_DELETEWebTool_Outputs_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.DoneButton, new Point(35, 6));
        }

        [When(@"I Select Test Source From GET Web Large View Source Combobox")]
        public void Select_GETWebTool_Source_From_SourceCombobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(UIMap.MainStudioWindow.WebServerSourceComboboxListItem2, new Point(163, 17));
        }

        [When(@"I Select Test Source From POST Web Large View Source Combobox")]
        public void Select_POSTWebTool_Source_From_SourceCombobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(UIMap.MainStudioWindow.WebServerSourceComboboxListItem3, new Point(163, 17));
        }

        [When(@"I Select Test Source From PUT Web Large View Source Combobox")]
        public void Select_PUTWebTool_Source_From_SourceCombobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(UIMap.MainStudioWindow.WebServerSourceComboboxListItem4, new Point(163, 17));
        }

        [When(@"I Select Test Source From DELETE Web Large View Source Combobox")]
        public void Select_DELETEWebTool_Source_From_SourceCombobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(UIMap.MainStudioWindow.WebServerSourceComboboxListItem1, new Point(163, 17));
        }

        [Given(@"I Click GET Web Large View Generate Outputs")]
        [When(@"I Click GET Web Large View Generate Outputs")]
        [Then(@"I Click GET Web Large View Generate Outputs")]
        public void Click_GETWebTool_GenerateOutputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.GenerateOutputsButton, new Point(7, 7));
        }

        [Given(@"I Click POST Web Large View Generate Outputs")]
        [When(@"I Click POST Web Large View Generate Outputs")]
        [Then(@"I Click POST Web Large View Generate Outputs")]
        public void Click_POSTWebTool_GenerateOutputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.GenerateOutputsButton, new Point(7, 7));
        }

        [Given(@"I Click PUT Web Large View Generate Outputs")]
        [When(@"I Click PUT Web Large View Generate Outputs")]
        [Then(@"I Click PUT Web Large View Generate Outputs")]
        public void Click_PUTWebTool_GenerateOutputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.GenerateOutputsButton, new Point(7, 7));
        }

        [Given(@"I Click DELETE Web Large View Generate Outputs")]
        [When(@"I Click DELETE Web Large View Generate Outputs")]
        [Then(@"I Click DELETE Web Large View Generate Outputs")]
        public void Click_DELETEWebTool_GenerateOutputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton, new Point(7, 7));
        }

        [Given(@"I Click GET Web Large View Done Button")]
        [When(@"I Click GET Web Large View Done Button")]
        [Then(@"I Click GET Web Large View Done Button")]
        public void Click_GETWebTool_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.SmallView.Exists, "Web GET small view does not exist after clicking large view done button.");
        }

        [Given(@"I Click POST Web Large View Done Button")]
        [When(@"I Click POST Web Large View Done Button")]
        [Then(@"I Click POST Web Large View Done Button")]
        public void Click_POSTWebTool_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.SmallView.Exists, "Web POST small view does not exist after clicking large view done button.");
        }

        [Given(@"I Click PUT Web Large View Done Button")]
        [When(@"I Click PUT Web Large View Done Button")]
        [Then(@"I Click PUT Web Large View Done Button")]
        public void Click_PUTWebTool_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.SmallView.Exists, "Web PUT small view does not exist after clicking large view done button.");
        }

        [Given(@"I Click DELETE Web Large View Done Button")]
        [When(@"I Click DELETE Web Large View Done Button")]
        [Then(@"I Click DELETE Web Large View Done Button")]
        public void Click_DELETEWebTool_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.SmallView.Exists, "Web DELETE small view does not exist after clicking large view done button.");
        }

        [Given(@"I Click GET Web Large View Test Inputs Button")]
        [When(@"I Click GET Web Large View Test Inputs Button")]
        [Then(@"I Click GET Web Large View Test Inputs Button")]
        public void Click_GETWebTool_TestInputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.TestButton, new Point(21, 11));
        }

        [Given(@"I Click POST Web Large View Test Inputs Button")]
        [When(@"I Click POST Web Large View Test Inputs Button")]
        [Then(@"I Click POST Web Large View Test Inputs Button")]
        public void Click_POSTWebTool_TestInputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.TestButton, new Point(21, 11));
        }

        [Given(@"I Click PUT Web Large View Test Inputs Button")]
        [When(@"I Click PUT Web Large View Test Inputs Button")]
        [Then(@"I Click PUT Web Large View Test Inputs Button")]
        public void Click_PUTWebTool_TestInputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.TestButton, new Point(21, 11));
        }

        [Given(@"I Click DELETE Web Large View Test Inputs Button")]
        [When(@"I Click DELETE Web Large View Test Inputs Button")]
        [Then(@"I Click DELETE Web Large View Test Inputs Button")]
        public void Click_DELETEWebTool_TestInputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.TestButton, new Point(21, 11));
        }

        public void WebDeleteTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete, new Point(201, 9));
        }

        [When(@"I Collapse GetWebTool Large View to Small View With Double Click")]
        public void WebGetTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet, new Point(201, 9));
        }

        [When(@"I Collapse PostWebTool Large View to Small View With Double Click")]
        public void WebPostTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost, new Point(201, 9));
        }

        [When(@"I Collapse PutWebTool Large View to Small View With Double Click")]
        public void WebPutTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut, new Point(201, 9));
        }

        [When(@"I Open DeleteWeb Tool Large View")]
        public void Open_DeleteWebTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.Exists, "Web delete large view does not exist on the design surface");
        }

        [Given(@"I Click GET Web Large View Done Button With Invalid Large View")]
        [When(@"I Click GET Web Large View Done Button With Invalid Large View")]
        [Then(@"I Click GET Web Large View Done Button With Invalid Large View")]
        public void Click_GET_Web_Large_View_Done_Button_With_Invalid_Large_View()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.DoneButton, new Point(33, 11));
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Error1.Exists, "Error not exist after clicking large view done button on invalid large view.");
        }

        [Given(@"I Click New Source Button From HttpWebDelete Tool")]
        [When(@"I Click New Source Button From HttpWebDelete Tool")]
        [Then(@"I Click New Source Button From HttpWebDelete Tool")]
        public void Click_NewSourceButton_From_HttpWebDeleteTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.NewSourceButton);
        }

        [Given(@"I Click New Source Button From HttpWebGet Tool")]
        [When(@"I Click New Source Button From HttpWebGet Tool")]
        [Then(@"I Click New Source Button From HttpWebGet Tool")]
        public void Click_NewSourceButton_From_HttpWebGetTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.NewSourceButton);
        }

        [Given(@"I Click New Source Button From HttpWebPost Tool")]
        [When(@"I Click New Source Button From HttpWebPost Tool")]
        [Then(@"I Click New Source Button From HttpWebPost Tool")]
        public void Click_NewSourceButton_From_HttpWebPostTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.NewSourceButton);
        }

        [Given(@"I Click New Source Button From HttpWebPut Tool")]
        [When(@"I Click New Source Button From HttpWebPut Tool")]
        [Then(@"I Click New Source Button From HttpWebPut Tool")]
        public void Click_NewSourceButton_From_HttpWebPutTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.NewSourceButton);
        }

        [When(@"I Select UITestingSource From Web Server Large View Source Combobox")]
        public void Select_UITestingSource_From_Web_Server_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox.UITesting, new Point(137, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.EditSourceButton.Enabled, "Delete Web large view source combobox EDIT button is disabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton.Enabled, "Delete Web large view source combobox GenerateOutput button is disabled.");
        }

        [When(@"I Drag POSTWebTool Onto DesignSurface")]
        public void Drag_POSTWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "POST";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.POST, new Point(20, 35));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            UIMap.Click_Clear_Toolbox_Filter_Clear_Button();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.Exists, "Web Post Request large view does not exist on the design surface.");
        }

        [When(@"I Drag PUTWebTool Onto DesignSurface")]
        public void Drag_PUTWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "PUT";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.PUT, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            UIMap.Click_Clear_Toolbox_Filter_Clear_Button();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.Exists, "Put Web connector large view does not exist on the design surface after drag and drop from toolbox.");
        }

        [When(@"I Drag DELETEWebTool Onto DesignSurface")]
        public void Drag_DELETEWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "DELETE";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.DELETE, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            UIMap.Click_Clear_Toolbox_Filter_Clear_Button();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.Exists, "Delete Web Tool large view does not exist on the design surface after drag and drop from toolbox.");
        }

        [When(@"I Collapse GetWebTool Large View to Small View With Double Click")]
        public void Collapse_GETWebTool_LargeView_To_SmallView()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.Exists, "Cannot collapse Web GET tool to small view, large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet, new Point(201, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.SmallView.Exists, "Web GET small view does not exist after collapsing the large view with a double click.");
        }

        [When(@"I Collapse PostWebTool Large View to Small View With Double Click")]
        public void Collapse_POSTWebTool_LargeView_To_SmallView()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.Exists, "Cannot collapse Web POST tool to small view, large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost, new Point(201, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.SmallView.Exists, "Web POST small view does not exist after collapsing the large view with a double click.");
        }

        [When(@"I Collapse PutWebTool Large View to Small View With Double Click")]
        public void Collapse_PUTWebTool_LargeView_To_SmallView()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.Exists, "Cannot collapse Web PUT tool to small view, large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut, new Point(201, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.SmallView.Exists, "Web PUT small view does not exist after collapsing the large view with a double click.");
        }

        [When(@"I Collapse DeleteWebTool Large View to Small View With Double Click")]
        public void Collapse_DELETEWebTool_LargeView_To_SmallView()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.Exists, "Cannot collapse Web DELETE tool to small view, large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete, new Point(201, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.SmallView.Exists, "Web DELETE small view does not exist after collapsing the large view with a double click.");
        }

        public void Select_Source_From_DELETEWebTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox.WebServiceSourceFromToolListItem);
        }

        public void Click_EditSourceButton_On_DELETEWebTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.EditSourceButton);
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
