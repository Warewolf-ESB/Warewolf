using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;


namespace Warewolf.UITests.Tabs
{
    [CodedUITest]
    public class PinUnpinPanes
    {
        [TestMethod]
        public void UnpinAndRepinNewWorkflowTab()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab);
            UIMap.Pin_Unpinned_Pane_To_Default_Position();
        }

        [TestMethod]
        public void UnpinAndRepinSettingsTab()
        {
            UIMap.Click_Settings_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab);
            UIMap.Pin_Unpinned_Pane_To_Default_Position();
        }

        [TestMethod]
        public void UnpinAndRepinServerSourceWizardTab()
        {
            UIMap.Select_NewRemoteServer_From_Explorer_Server_Dropdownlist();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab);
            UIMap.Pin_Unpinned_Pane_To_Default_Position();
        }

        [TestMethod]
        public void UnpinAndRepinDBSourceWizardTab()
        {
            UIMap.Click_New_Database_Source_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab);
            UIMap.Pin_Unpinned_Pane_To_Default_Position();
        }

        [TestMethod]
        public void UnpinAndRepinPluginSourceWizardTab()
        {
            UIMap.Click_NewPluginSource_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.PluginSourceWizardTab);
            UIMap.Pin_Unpinned_Pane_To_Default_Position();
        }

        [TestMethod]
        public void UnpinAndRepinWebSourceWizardTab()
        {
            UIMap.Click_New_Web_Source_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WebSourceWizardTab);
            UIMap.Pin_Unpinned_Pane_To_Default_Position();
        }

        [TestMethod]
        public void UnpinAndRepinDeployTab()
        {
            UIMap.Click_Deploy_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab);
            UIMap.Pin_Unpinned_Pane_To_Default_Position();
        }

        [TestMethod]
        public void UnpinAndRepinDependencyGraphTab()
        {
            UIMap.Select_Show_Dependencies_In_Explorer_Context_Menu("Hello World");
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab);
            UIMap.Pin_Unpinned_Pane_To_Default_Position();
        }

        [TestMethod]
        public void UnpinAndRepinTestsTabPage()
        {
            UIMap.Filter_Explorer("Hello World");
            UIMap.Open_Explorer_First_Item_Tests_With_Context_Menu();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage);
            UIMap.Pin_Unpinned_Pane_To_Default_Position();
        }

        [TestMethod]
        public void UnpinAndRepinSchedulerTab()
        {
            UIMap.Click_Scheduler_Ribbon_Button();
            UIMap.Unpin_Tab_With_Drag(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SchedulerTab);
            UIMap.Pin_Unpinned_Pane_To_Default_Position();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
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

        #endregion
    }
}
