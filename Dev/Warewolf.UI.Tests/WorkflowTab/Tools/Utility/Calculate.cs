﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Utility.UtilityToolsUIMapClasses;

namespace Warewolf.UI.Tests.WorkflowTab.Tools.Utility
{
    [CodedUITest]
    public class Calculate
    {
        [TestMethod]
		[TestCategory("Utility Tools")]
        public void CalculateTool_Small_And_LargeView_Then_EnterSomeVariable_UITest()
        {
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.Exists, "Calculate Tool on the design surface does not exist");
            //Small View
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.SmallView.FxCombobox.Exists, "FX Combobox does not exist on design surface.");
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.SmallView.ResultCombobox.Exists, "Result Combobox does not exisst on design surface.");
            //Large View
            UtilityToolsUIMap.Open_Calculate_Tool_Large_View();
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.FxCombobox.Exists, "FX Combobox does not exist on design surface.");
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.ResultCombobox.Exists, "Result Combobox does not exist on design surface.");
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.OnErrorPane.Exists, "OnError Pane does not exist on design surface.");
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.DoneButton.Exists, "Done Button does not exist on design surface.");
            //Enter SomeVariable
            UtilityToolsUIMap.Enter_SomeVariable_Into_CalculateTool_FXCombobox();
            Assert.AreEqual("[[SomeVariable]]", UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.FxCombobox.TextEdit.Text, "Calculate large view function textbox text does not equal \"[[SomeVariable]]\"");
            UtilityToolsUIMap.Click_CalculateTool_DoneButton();
            Assert.AreEqual("[[SomeVariable]]", UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.SmallView.FxCombobox.TextEdit.Text, "Calculate large view function textbox text does not equal \"[[SomeVariable]]\"");
        }

        [TestMethod]
        [TestCategory("Utility Tools")]
        public void CalculateTool_Enter_On_DropDown_DoesNot_FreezeStudio()
        {
            //Enter SomeVariable
            UtilityToolsUIMap.Enter_SomeVariable_Into_SmallView_CalculateTool_FXCombobox();
            Assert.AreEqual("[[SomeVariable]]", UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.SmallView.FxCombobox.TextEdit.Text, "Calculate large view function textbox text does not equal \"[[SomeVariable]]\"");
            Keyboard.SendKeys("{ENTER}");
            Keyboard.SendKeys("e");
            Keyboard.SendKeys("{DOWN}");
            Keyboard.SendKeys("{ENTER}");
            Keyboard.SendKeys("{ENTER}");
            Keyboard.SendKeys("e");
            Keyboard.SendKeys("{DOWN}");
            Keyboard.SendKeys("{ENTER}");
            Keyboard.SendKeys("{ENTER}");
            UtilityToolsUIMap.Open_Calculate_Tool_Large_View();
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.DoneButton.Exists, "Done Button does not exist on design surface.");
            UtilityToolsUIMap.Click_CalculateTool_DoneButton();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Calculate_Onto_DesignSurface();
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

        UtilityToolsUIMap UtilityToolsUIMap
        {
            get
            {
                if (_UtilityToolsUIMap == null)
                {
                    _UtilityToolsUIMap = new UtilityToolsUIMap();
                }

                return _UtilityToolsUIMap;
            }
        }

        private UtilityToolsUIMap _UtilityToolsUIMap;

        #endregion
    }
}
