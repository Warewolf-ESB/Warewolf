using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.Toolbox
{
    [CodedUITest]
    public class ToolboxTests
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void ToolboxBehaviourCheck_ClearFilter_Then_PopUp_Then_UpdateHelpText_UITest()
        {
            //Clear Filter
            WorkflowTabUIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
            UIMap.Click_Clear_Toolbox_Filter_Clear_Button();
            Assert.IsTrue(string.IsNullOrEmpty(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text));
            //Update Help Text
            var initialImage = UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Help.HelpTextEditor.CaptureImage();
            UIMap.SingleClick_Toolbox();
            var assignImage = UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Help.HelpTextEditor.CaptureImage();
            Assert.AreNotEqual(initialImage, assignImage);
            //PopUp
            UIMap.DoubleClick_Toolbox();
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.OKButton.Exists);
            DialogsUIMap.Click_MessageBox_OK();
        }

        #region Additional test attributes

        [TestInitialize()]
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

        #endregion
    }
}
