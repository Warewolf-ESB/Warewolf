using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.RabbitMQSource.RabbitMQSourceUIMapClasses;
using Warewolf.UITests.Tools.ToolsUIMapClasses;

namespace Warewolf.UITests.Tools.Utility
{
    [CodedUITest]
    public class RabbitMQPublish
    {
        [TestMethod]
		[TestCategory("Utility Tools")]
        public void RabbitMQPublishTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.Exists, "RabbitMQ Publish Tool does not exist on the design surface.");
            //Small View
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.SmallViewContentCustom.SourceComboBox.Exists, "Sources Combobox does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.SmallViewContentCustom.EditSourceButton.Exists, "EditSource Button does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.SmallViewContentCustom.NewSourceButton.Exists, "NewSource Button does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.SmallViewContentCustom.QueueNameComboBox.Exists, "QueueName Combobox does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.SmallViewContentCustom.MessageComboBox.Exists, "Message Combobox does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.SmallViewContentCustom.ResultsComboBox.Exists, "Results Combobox does not exist on the design surface.");
            //Large View
            ToolsUIMap.Open_RabbitMqPublish_LargeView();
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.SourceComboBox.Exists, "Sources Combobox does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.EditSourceButton.Exists, "EditSource Button does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.NewSourceButton.Exists, "NewSource Button does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.QueueNameComboBox.Exists, "QueueName Combobox does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.DurableCheckBox.Exists, "Durable Checkbox does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.ExclusiveCheckBox.Exists, "Exclusive Checkbox does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.AutoDeleteCheckBox.Exists, "AutoDelete Checkbox does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.MessageComboBox.Exists, "Message Combobox does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.ResultsComboBox.Exists, "Results Combobox does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.OnErrorCustom.Exists, "Results Combobox does not exist on the design surface.");
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.DoneButton.Exists, "Done Button does not exist on the design surface.");
            //New Source
            ToolsUIMap.Click_NewSourceButton_From_RabbitMQPublishTool();
            Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.HostTextBoxEdit.Enabled, "Host Textbox is not enabled");
            Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.PortTextBoxEdit.Enabled, "Port Textbox is not enabled");
            Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.UserNameTextBoxEdit.Enabled, "Username Textbox is not enabled");
            Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.PasswordTextBoxEdit.Enabled, "Password Textbox is not enabled");
            Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.VirtualHostTextBoxEdit.Enabled, "Virtual Host Textbox is not enabled");
            Assert.IsFalse(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.TestConnectionButton.Enabled, "Test Connection button is enabled");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            ToolsUIMap.Drag_Toolbox_RabbitMqPublish_Onto_DesignSurface();
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

        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

        RabbitMQSourceUIMap RabbitMQSourceUIMap
        {
            get
            {
                if (_RabbitMQSourceUIMap == null)
                {
                    _RabbitMQSourceUIMap = new RabbitMQSourceUIMap();
                }

                return _RabbitMQSourceUIMap;
            }
        }

        private RabbitMQSourceUIMap _RabbitMQSourceUIMap; 

        #endregion
    }
}
