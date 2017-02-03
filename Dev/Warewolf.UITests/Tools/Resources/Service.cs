using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Resources
{
    [CodedUITest]
    public class Service
    {
        [TestMethod]
		[TestCategory("Resource Tools")]
        public void ServiceTool_UIBehaviourCheck_UITest()
        {
            Assert.IsTrue(UIMap.ServicePickerDialog.Explorer.Exists, "Service picker Explorer Tree does not exist on the Design Surface");
            Assert.IsTrue(UIMap.ServicePickerDialog.OK.Exists, "Service picker OK Button does not exist on the Design Surface");
            Assert.IsTrue(UIMap.ServicePickerDialog.Cancel.Exists, "Service picker cancel button does not exist on the Design Surface");
            // OK Button does not enable after clicking folder
            Assert.IsFalse(UIMap.ServicePickerDialog.OK.Enabled, "OK Button is enabled");
            UIMap.Select_FirstItem_From_ServicePicker_Tree();
            Assert.IsFalse(UIMap.ServicePickerDialog.OK.Enabled, "OK Button is enabled");
            // Selection of Hello World enables OK Button
            UIMap.Filter_ServicePicker_Explorer("Hello World");
            UIMap.Select_FirstItem_From_ServicePicker_Tree();
            Assert.IsTrue(UIMap.ServicePickerDialog.OK.Enabled, "OK Button is not enabled");
            // Hello World workflow opens
            UIMap.Click_Service_Picker_Dialog_OK();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.Exists, "Hello World work flow does not exist after selecting OK from Service Picker");
            // Deletion successful
            UIMap.Delete_HelloWorld_With_Context_Menu();
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow), "Hello World work flow still exist after deletion.");
            UIMap.Drag_Toolbox_Service_Picker_Onto_DesignSurface();
            // Cancel Button Behaviour
            UIMap.Click_ServicePickerDialog_CancelButton();
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.ServicePickerDialog.OK), "Service picker dialog still exists after clicking cancel button.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
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
