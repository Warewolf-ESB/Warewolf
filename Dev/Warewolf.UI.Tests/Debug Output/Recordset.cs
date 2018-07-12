﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using System.Drawing;
using Warewolf.UI.Tests.WorkflowTab.Tools.Resources.ResourcesToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Database.DatabaseToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Scripting.ScriptingToolsUIMapClasses;

namespace Warewolf.UI.Tests
{
    [CodedUITest]
    public class RecordsetTests
    {
        const string workflow = "F6ExecuteOnFocusLostTest";
        const string SetDeclareVarToRecordset = "SetDeclareVarToRecordset";
        const string ChangeDeclareVarUpdatesOutput = "ChangeDeclareVarUpdatesOutput";
        const string CopyPasteADNameChange = "CopyPasteADNameChange";

        [TestMethod]
        [TestCategory("Recordset")]
        public void F6_Execute_OnFocustLost_Thus_Persisting_Textbox_Changes()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            ExplorerUIMap.Filter_Explorer(workflow);
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            WorkflowTabUIMap.Drag_Explorer_Item_To_Design_Surface();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
            Assert.AreEqual("[[person().name]]", ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Flowchart1.Service.ServiceToolLargeView.UIOutputsDataGridTable.UIUI_ActivityGridRow_0Row.UIItemDev2StudioViewMoCell.UIItemComboBox.UITextEdit.Text);
            UIMap.Press_F6();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.PersonOutputButton.Exists);
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.PersonOutputButton);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.FirstRowOfRecordset.Exists);
            Assert.IsFalse(UIMap.ControlExistsNow(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIF6ExecuteOnFocusLostTreeItem.UIPersonnameExpander.ThirdRowOfRecordset));
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
            UIMap.Press_F6();
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SearchTextBox.Text = "Advanced Recordset";
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UINameText.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIFilternameText.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIKimText.Exists);            
            DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AdvancedRecordset.LargeView.DeclareVariablesDataTable.ActivityGridRow.ItemColumnDisplayCell1.Variable.TextEdit.Text = "Bob";
            UIMap.Press_F6();
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SearchTextBox.Text = "Advanced Recordset";
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UINameText.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIBobText.Exists);
            Assert.IsFalse(UIMap.ControlExistsNow(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIFilternameText));
            Assert.IsFalse(UIMap.ControlExistsNow(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UIKimText));
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

            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UINameText.Exists);
            Assert.AreEqual("John", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UINameText);

            advancedRecordset.LargeView.DeclareVariablesDataTable.ActivityGridRow.ItemColumnDisplayCell1.Variable.TextEdit.Text = "Jeff";
            UIMap.Press_F6();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UINameText.Exists);
            Assert.AreEqual("Jeff", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UINameText);

            Keyboard.SendKeys("^C");
            UIMap.Click_NewWorkflow_RibbonButton();
            Keyboard.SendKeys("^V");

            Mouse.Click(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTabNext);

            DatabaseToolsUIMap.AdvancedRecordsetTool_ChangeView_With_DoubleClick();

            advancedRecordset.LargeView.DeclareVariablesDataTable.ActivityGridRow.ItemColumnDisplayCell1.Variable.TextEdit.Text = "George";
            UIMap.Press_F6();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UINameText.Exists);
            Assert.AreEqual("George", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UIAdvancedRecordsetTreeItem.UINameText);
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
