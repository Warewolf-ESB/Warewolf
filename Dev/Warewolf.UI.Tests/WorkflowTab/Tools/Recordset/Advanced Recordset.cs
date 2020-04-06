using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using System.Drawing;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WPFUIItems;
using Warewolf.UI.Tests.WorkflowTab.Tools.Resources.ResourcesToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Database.DatabaseToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Scripting.ScriptingToolsUIMapClasses;

namespace Warewolf.UI.Tests
{
    [CodedUITest]
    public class Advanced_Recordset
    {
        const string workflow = "F6ExecuteOnFocusLostTest";
        const string SetDeclareVarToRecordset = "SetDeclareVarToRecordset";
        const string ChangeDeclareVarUpdatesOutput = "ChangeDeclareVarUpdatesOutput";
        const string CopyPasteADNameChange = "CopyPasteADNameChange";

        [TestMethod]
        [TestCategory("Recordset")]
        public void Expand_Recordset_In_Debug_Output()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            ExplorerUIMap.Filter_Explorer(workflow);
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            WorkflowTabUIMap.Drag_Explorer_Item_To_Design_Surface();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
            Assert.AreEqual("[[person().name]]", ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Flowchart1.Service.ServiceToolLargeView.UIOutputsDataGridTable.UIUI_ActivityGridRow_0Row.UIItemDev2StudioViewMoCell.UIItemComboBox.UITextEdit.Text);
            UIMap.Press_F6();
            UIMap._window.Get<TestStack.White.UIItems.TreeItems.TreeNode>(SearchCriteria.ByAutomationId("Service"))
                .Get<TestStack.White.UIItems.Button>(SearchCriteria.ByAutomationId("Expander"))
                .Click();
            while (UIMap._window.Get<TestStack.White.UIItems.Button>(SearchCriteria.ByAutomationId("HeaderSite"))
                .IsOffScreen || UIMap._window.Get<TestStack.White.UIItems.Button>(SearchCriteria.ByAutomationId("HeaderSite"))
                .ClickablePoint.Y > (UIMap._window.Bounds.Bottom-50))
            {
                UIMap._window.Get<TestStack.White.UIItems.Panel>(SearchCriteria.ByAutomationId("instScroll")).ScrollBars.Vertical.ScrollDown();
            }
            UIMap._window.Get<TestStack.White.UIItems.Button>(SearchCriteria.ByAutomationId("HeaderSite")).Click();
            Assert.IsTrue(UIMap._window.Get<TestStack.White.UIItems.TreeItems.TreeNode>(SearchCriteria.ByAutomationId("End"))
                .GetMultiple<TestStack.White.UIItems.Label>(SearchCriteria.ByAutomationId("UI_DebugOutputVariableTextBlock_AutoID")).Length == 3);
            ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Flowchart1.Service.ServiceToolLargeView.UIOutputsDataGridTable.UIUI_ActivityGridRow_0Row.UIItemDev2StudioViewMoCell.UIItemComboBox.UITextEdit.Text = "[[person(*).name]]";
            UIMap.Press_F6();
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.PersonOutputButton);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.FirstRowOfRecordset.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.ThirdRowOfRecordset.Exists);
        }

        [TestMethod]
        [TestCategory("Recordset")]
        public void Assigning_Declared_Variable_To_A_Recordset_Shows_The_Equal_Sign()
        {
            ExplorerUIMap.Filter_Explorer(SetDeclareVarToRecordset);
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Press_F6();
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SearchTextBox.Text = "Advanced Recordset";
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIIDText.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIIDText.DisplayText.Contains("="));
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIPersonidExpander.Exists);
        }

        [TestMethod]
        [TestCategory("Recordset")]
        public void Change_Declared_Value_Updates_Output()
        {
            ExplorerUIMap.Filter_Explorer(ChangeDeclareVarUpdatesOutput);
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            DatabaseToolsUIMap.Open_AdvancedRecordset_Large_View_By_Double_Click();
            Assert.AreEqual("[[filtername]]", DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AdvancedRecordset.LargeView.DeclareVariablesDataTable.UINameRow.UIItemnameColumnDisplaCell.UIThevalueofthelocalvaComboBox.UITextEdit.Text);
            UIMap.Press_F6();
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SearchTextBox.Text = "Advanced Recordset";
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UINameText.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIFilternameText.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIKimText.Exists);
            DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AdvancedRecordset.LargeView.DeclareVariablesDataTable.UINameRow.UIItemnameColumnDisplaCell.UIThevalueofthelocalvaComboBox.UITextEdit.Text = "Bob";
            UIMap.Press_F6();
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SearchTextBox.Text = "Advanced Recordset";
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UINameText.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIBobText.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UINameText.Exists);
        }

        [TestMethod]
        [TestCategory("Database Tools")]
        public void AdvancedRecordsetTool_Declare_Value_Change_Debug_Should_Update()
        {
            ExplorerUIMap.Filter_Explorer(CopyPasteADNameChange);
            ExplorerUIMap.Open_Explorer_First_Item_With_Double_Click();
            DatabaseToolsUIMap.AdvancedRecordsetTool_ChangeView_With_DoubleClick();

            var advancedRecordset = DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AdvancedRecordset;

            Assert.IsTrue(advancedRecordset.LargeView.Exists, "Advanced Recordset tool does not exist on design surface.");
            UIMap.Press_F6();

            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SearchTextBox.Text = "Advanced Recordset";

            string displayTextJohn = WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIKimText.DisplayText;
            Assert.AreEqual("John", displayTextJohn);

            DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AdvancedRecordset.LargeView.DeclareVariablesDataTable.UINameRow.UIItemnameColumnDisplaCell.UIThevalueofthelocalvaComboBox.UITextEdit.Text = "Jeff";
            UIMap.Press_F6();
            string displayTextJeff = WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIKimText.DisplayText;
            Assert.AreEqual("Jeff", displayTextJeff);

            DatabaseToolsUIMap.AdvancedRecordsetTool_Select_With_SingleClick();

            Keyboard.SendKeys("^C");
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Click_Start_Node();
            Keyboard.SendKeys("^V");

            DatabaseToolsUIMap.Right_Click_AdvancedRecordsetTool();            
            UIMap.Context_Menu_Set_As_Start_Node();
            Mouse.DoubleClick(advancedRecordset, new Point(182, 15));

            UIMap.Press_F6();
            string displayTextGeorge = WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIKimText.DisplayText;
            Assert.AreEqual("Jeff", displayTextGeorge);
        }


        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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

        #endregion
    }
}
