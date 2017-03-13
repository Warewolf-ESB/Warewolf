using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Resources.ResourcesToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Resources
{

    [CodedUITest]
    public class Service
    {
        [TestMethod]
		[TestCategory("Resource Tools")]
        public void ServiceTool_UIBehaviourCheck_UITest()
        {
            Assert.IsTrue(DialogsUIMap.ServicePickerDialog.Explorer.Exists, "Service picker Explorer Tree does not exist on the Design Surface");
            Assert.IsTrue(DialogsUIMap.ServicePickerDialog.OK.Exists, "Service picker OK Button does not exist on the Design Surface");
            Assert.IsTrue(DialogsUIMap.ServicePickerDialog.Cancel.Exists, "Service picker cancel button does not exist on the Design Surface");
            // OK Button does not enable after clicking folder
            Assert.IsFalse(DialogsUIMap.ServicePickerDialog.OK.Enabled, "OK Button is enabled");
            DialogsUIMap.Select_FirstItem_From_ServicePicker_Tree();
            Assert.IsFalse(DialogsUIMap.ServicePickerDialog.OK.Enabled, "OK Button is enabled");
            // Selection of Hello World enables OK Button
            DialogsUIMap.Filter_ServicePicker_Explorer("Hello World");
            DialogsUIMap.Select_FirstItem_From_ServicePicker_Tree();
            Assert.IsTrue(DialogsUIMap.ServicePickerDialog.OK.Enabled, "OK Button is not enabled");
            // Hello World workflow opens
            DialogsUIMap.Click_Service_Picker_Dialog_OK();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.Exists, "Hello World work flow does not exist after selecting OK from Service Picker");
            // Deletion successful
            WorkflowTabUIMap.Delete_HelloWorld_With_Context_Menu();
            Assert.IsFalse(UIMap.ControlExistsNow(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow), "Hello World work flow still exist after deletion.");
            WorkflowTabUIMap.Drag_Toolbox_Service_Picker_Onto_DesignSurface();
            // Cancel Button Behaviour
            DialogsUIMap.Click_ServicePickerDialog_CancelButton();
            Assert.IsFalse(UIMap.ControlExistsNow(DialogsUIMap.ServicePickerDialog.OK), "Service picker dialog still exists after clicking cancel button.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Service_Picker_Onto_DesignSurface();
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

        #endregion
    }
}
