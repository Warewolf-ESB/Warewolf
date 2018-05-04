using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.DBSource.DBSourceUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Database.DatabaseToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.Tools
{
    [CodedUITest]
    public class AdvancedRecordsetTests
    {
        const string AdvancedRecordset = "Advanced Recordset";

        [TestMethod]
        [TestCategory("Database Tools")]
        public void AdvancedRecordsetTool_Small_And_LargeView()
        {
            WorkflowTabUIMap.Drag_Toolbox_AdvancedRecordset_Onto_DesignSurface();

            var advancedRecordset = DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AdvancedRecordset;

            Assert.IsTrue(advancedRecordset.Exists, "Advanced Recordset tool does not exist on design surface.");
            Assert.IsTrue(advancedRecordset.LargeView.Exists, "Advanced Recordset large does not exist on design surface on initial drop from Toolbox.");
            //Small View
            DatabaseToolsUIMap.AdvancedRecordsetTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(advancedRecordset.SmallView.Exists, "Advanced Recordset tool small view does not exist after collapsing large view with a double click.");
            //Large View
            DatabaseToolsUIMap.AdvancedRecordsetTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(advancedRecordset.LargeView.DeclareVariablesDataTable.Exists, "Declare Variables Data Table does not exist on Advanced Recordset connector tool large view.");
            Assert.IsTrue(advancedRecordset.LargeView.QueryComboBox.Exists, "Query text box does not exist on Advanced Recordset connector tool large view.");
            Assert.IsTrue(advancedRecordset.LargeView.GenerateOutputsButton.Exists, "Generate Outputs button does not exist on Advanced Recordset connector tool large view.");
        }

        [TestMethod]
        [TestCategory("Database Tools")]
        public void AdvancedRecordsetTool_Clicking_GenerateOutputs_Creates_A_Recordset_Name()
        {
            ExplorerUIMap.Filter_Explorer(AdvancedRecordset);
            ExplorerUIMap.Open_Explorer_First_Item_With_Double_Click();
            DatabaseToolsUIMap.AdvancedRecordsetTool_ChangeView_With_DoubleClick();

            var advancedRecordset = DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AdvancedRecordset;

            Assert.IsTrue(advancedRecordset.LargeView.Exists, "Advanced Recordset tool does not exist on design surface.");
            advancedRecordset.LargeView.QueryComboBox.TextEdit.Text = "select name from person";
            Mouse.Click(advancedRecordset.LargeView.GenerateOutputsButton);
            Assert.AreEqual("TableCopy", advancedRecordset.LargeView.RecordSetTextBoxEdit.Text);
        }

        [TestMethod]
        [TestCategory("Database Tools")]
        public void AdvancedRecordsetTool_Select_Name_From_Person_Creates_PersonName_Mapping()
        {
            ExplorerUIMap.Filter_Explorer(AdvancedRecordset);
            ExplorerUIMap.Open_Explorer_First_Item_With_Double_Click();
            DatabaseToolsUIMap.AdvancedRecordsetTool_ChangeView_With_DoubleClick();

            var advancedRecordset = DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AdvancedRecordset;

            Assert.IsTrue(advancedRecordset.LargeView.Exists, "Advanced Recordset tool does not exist on design surface.");
            advancedRecordset.LargeView.QueryComboBox.TextEdit.Text = "select name from person";
            Keyboard.SendKeys("{Escape}");
            Mouse.Click(advancedRecordset.LargeView.GenerateOutputsButton);
            Assert.AreEqual("[[TableCopy().name]]", advancedRecordset.LargeView.OutputsMappingDataGrTable.ItemRow.Row1Cell.Row1Combobox.TextEdit.Text);
        }

        [TestMethod]
        [TestCategory("Database Tools")]
        public void AdvancedRecordsetTool_Select_Multiple_Fields_From_Person_Creates_All_Field_Mapping()
        {
            ExplorerUIMap.Filter_Explorer(AdvancedRecordset);
            ExplorerUIMap.Open_Explorer_First_Item_With_Double_Click();
            DatabaseToolsUIMap.AdvancedRecordsetTool_ChangeView_With_DoubleClick();

            var advancedRecordset = DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AdvancedRecordset;

            Assert.IsTrue(advancedRecordset.LargeView.Exists, "Advanced Recordset tool does not exist on design surface.");
            advancedRecordset.LargeView.QueryComboBox.TextEdit.Text = "select name, age, address_id from person";
            Keyboard.SendKeys("{Escape}");
            Mouse.Click(advancedRecordset.LargeView.GenerateOutputsButton);
            Assert.AreEqual("[[TableCopy().name]]", advancedRecordset.LargeView.OutputsMappingDataGrTable.ItemRow.Row1Cell.Row1Combobox.TextEdit.Text);
            Assert.AreEqual("[[TableCopy().age]]", advancedRecordset.LargeView.OutputsMappingDataGrTable.ItemRow2.Row2Cell.Row2ComboBox.TextEdit.Text);
            Assert.AreEqual("[[TableCopy().address_id]]", advancedRecordset.LargeView.OutputsMappingDataGrTable.ItemRow3.Row3Cell.Row3ComboBox.TextEdit.Text);
        }

        [TestMethod]
        [TestCategory("Database Tools")]
        public void AdvancedRecordsetTool_Select_Name_With_An_Elias_From_Person_Creates_PersonAliasName_Mapping()
        {
            ExplorerUIMap.Filter_Explorer(AdvancedRecordset);
            ExplorerUIMap.Open_Explorer_First_Item_With_Double_Click();
            DatabaseToolsUIMap.AdvancedRecordsetTool_ChangeView_With_DoubleClick();

            var advancedRecordset = DatabaseToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AdvancedRecordset;

            Assert.IsTrue(advancedRecordset.LargeView.Exists, "Advanced Recordset tool does not exist on design surface.");
            advancedRecordset.LargeView.QueryComboBox.TextEdit.Text = "select name as firstName from person";
            Keyboard.SendKeys("{Escape}");
            Mouse.Click(advancedRecordset.LargeView.GenerateOutputsButton);
            Assert.AreEqual("[[TableCopy().firstName]]", advancedRecordset.LargeView.OutputsMappingDataGrTable.ItemRow.Row1Cell.Row1Combobox.TextEdit.Text);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
        }

        UIMap UIMap
        {
            get
            {
                if ((_UIMap == null))
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
