using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class SQL_Bulk_Insert
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void SQLBulkInsertToolUITest()
        {
            UIMap.Open_SQL_Bulk_Insert_Tool_Large_View();            
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void SqlBulkInsertTest_OpenLargeViewAndEnterAnInvalidBatchAndTimeoutSizeAndClickDone_CorrectingErrorsAndClickDoneWillReturnToSmallView_UITest()
        {
            UIMap.Open_SQL_Bulk_Insert_Tool_Large_View();
            UIMap.Click_SqlBulkInsert_Done_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Errors.Exists);
            UIMap.Select_DatabaseAndTable_From_BulkInsert_Tool();
            Point newPoint;
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem2.TryGetClickablePoint(out newPoint));
            UIMap.Click_SqlBulkInsert_Done_Button();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void SQLBulkInsertTool_OpenQVIUITest()
        {
            UIMap.Open_SQL_Bulk_Insert_Tool_Qvi_Large_View();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_SQL_Bulk_Insert_Onto_DesignSurface();
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

        #endregion
    }
}
