using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Utility
{
    [CodedUITest]
    public class RabbitMQPublish
    {
        [TestMethod]
		[TestCategory("Utility Tools")]
        public void RabbitMQPublishTool_OpenLargerViewUITest()
        {
            UIMap.Open_RabbitMqPublish_LargeView();
        }

        //[TestMethod]
        //[TestCategory("Utility Tools")]
        //public void ResizeAdornerMappings_Expected_AdornerMappingIsResized_UITest()
        //{
        //    UIMap.Open_RabbitMqPublish_LargeView();
        //    var heightBefore = UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.MessageComboBox.Width;
        //    UIMap.Resize_RabbitMQPublish_LargeTool();
        //    Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.MessageComboBox.Width > heightBefore);
        //}

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_RabbitMqPublish_Onto_DesignSurface();
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

        #endregion
    }
}
