using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Service
    {
        [TestMethod]
		[TestCategory("Resource Tools")]
        public void ResourcePickerTests_CodedUI_DropWorkflowFromToolbox_ExpectResourcePickerToBehaveCorrectly_UITest()
        {
            Assert.IsFalse(UIMap.ServicePickerDialog.OK.Enabled);
            UIMap.Select_FirstItem_From_ServicePicker_Tree();
            Assert.IsFalse(UIMap.ServicePickerDialog.OK.Enabled);
            UIMap.Filter_ServicePicker_Explorer("Hello World");
            UIMap.Select_FirstItem_From_ServicePicker_Tree();
            Assert.IsTrue(UIMap.ServicePickerDialog.OK.Enabled);
            UIMap.Click_Service_Picker_Dialog_OK();
        }

        [TestMethod]
		[TestCategory("Resource Tools")]
        public void SelectResource_FromResourcePickerTestsAndClickCancel_UITest()
        {
            Assert.IsFalse(UIMap.ServicePickerDialog.OK.Enabled);
            UIMap.Filter_ServicePicker_Explorer("Hello World");
            UIMap.Select_FirstItem_From_ServicePicker_Tree();
            UIMap.Click_Service_Picker_Dialog_Cancel();
            Point newPoint;
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.TryGetClickablePoint(out newPoint));
        }
        [TestMethod]
		[TestCategory("Resource Tools")]
        public void SelectResource_FromResourcePickerTestsAndClickOK_UITest()
        {
            Assert.IsFalse(UIMap.ServicePickerDialog.OK.Enabled);
            UIMap.Filter_ServicePicker_Explorer("Hello World");
            UIMap.Select_FirstItem_From_ServicePicker_Tree();
            UIMap.Click_Service_Picker_Dialog_OK();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.Exists
                , "Hello World work flow does not exist after selecting OK from Service Picker");
        }
        [TestMethod]
		[TestCategory("Resource Tools")]
        public void SelectResource_FromResourcePickerAndClickOK_ThenDeleteWorkFlowAndDragServiceAgain_UITest()
        {
            Assert.IsFalse(UIMap.ServicePickerDialog.OK.Enabled);
            UIMap.Filter_ServicePicker_Explorer("Hello World");
            UIMap.Select_FirstItem_From_ServicePicker_Tree();
            UIMap.Click_Service_Picker_Dialog_OK();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.Exists
                , "Hello World work flow does not exist after selecting OK from Service Picker");
            UIMap.Delete_HelloWorld_With_Context_Menu();
            UIMap.Drag_Toolbox_Service_Picker_Onto_DesignSurface();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Service_Picker_Onto_DesignSurface();
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
