using Dev2.Studio.UI.Tests;
using Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;

// ReSharper disable CheckNamespace
namespace Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses
// ReSharper restore CheckNamespace
{
    public partial class RibbonUIMap : UIMapBase
    {
        #region Mappings

        private DebugUIMap _debugUIMap;

        public new DebugUIMap DebugUIMap
        {
            get
            {
                if((_debugUIMap == null))
                {
                    _debugUIMap = new DebugUIMap();
                }
                return _debugUIMap;
            }
            set { _debugUIMap = value; }
        }

        #endregion

        int loopCount;
        WpfTabList uIRibbonTabList;

        public void ClickRibbonMenu(string menuAutomationId)
        {
            // This needs some explaining :)
            // Due to the way the WpfRibbon works, unless it has been used, the default tab will not be properly initialised.
            // This will cause problems if you need to use it (Automation wise)
            // To combat this, we check if the tab we have got is actually a valid tab (Check its bounding)
            // If it's not a valid tab, we click an alternate tab - This validates our original tab.
            // We then recusrsively call the method again with the now validated tab, and it works as itended.
            // Note: This recursive call will only happen the first time the ribbon is used, as it will subsequently be initialised correctly.

            if(uIRibbonTabList == null)
            {
                uIRibbonTabList = UIBusinessDesignStudioWindow.UIRibbonTabList;
            }

            UITestControlCollection tabList = uIRibbonTabList.Tabs;
            UITestControl theControl;
            foreach(WpfTabPage tabPage in tabList)
            {
                if(tabPage.Name == menuAutomationId)
                {
                    theControl = tabPage;
                    Point p;
                    p = new Point(theControl.GetChildren()[0].BoundingRectangle.X + 20, theControl.GetChildren()[0].BoundingRectangle.Y + 10);
                    Mouse.Click(p);
                    return;
                }
                else
                {
                    theControl = tabPage;
                    Point p = new Point(theControl.BoundingRectangle.X + 5, theControl.BoundingRectangle.Y + 5);
                    if(p.X > 5)
                    {
                        Mouse.Click(p);
                        break;
                    }
                }
            }

            // Somethign has gone wrong - Retry!

            loopCount++; // This was added due to the infinite loop happening if the ribbon was totally unclickable due to a crash
            if(loopCount < 10)
            {
                ClickRibbonMenu(menuAutomationId);
            }
        }

        public void ClickNewPlugin()
        {
            ClickRibbonMenuItem("UI_RibbonHomeTabPluginServiceBtn_AutoID");
        }

        public void ClickManageSecuritySettings()
        {
            StudioWindow.SetFocus();
            ClickRibbonMenuItem("UI_RibbonHomeManageSecuritySettingsBtn_AutoID");
            StudioWindow.WaitForControlReady();
        }

        public void ClickSave()
        {
            ClickRibbonMenuItem("UI_RibbonHomeTabSaveBtn_AutoID");
            Playback.Wait(2000);
        }

        public void ClickNewWebService()
        {
            ClickRibbonMenuItem("UI_RibbonHomeTabWebServiceBtn_AutoID");
            WizardsUIMap.WaitForWizard();
        }

        public void ClickNewDbWebService()
        {
            ClickRibbonMenuItem("UI_RibbonHomeTabDBServiceBtn_AutoID");
            WizardsUIMap.WaitForWizard();
        }

        public UITestControl ClickDebug()
        {
            UITestControl clickRibbonMenuItem = ClickRibbonMenuItem("UI_RibbonDebugBtn_AutoID");
            DebugUIMap.WaitForDebugWindow(7000);
            return clickRibbonMenuItem;
        }

        public UITestControl OpenDeploy()
        {
            return ClickRibbonMenuItem("Deploy", 20000);
        }

        public UITestControl OpenScheduler()
        {
            return ClickRibbonMenuItem("Scheduler", 20000);
        }

        public UITestControl OpenManageSettings()
        {
            return ClickRibbonMenuItem("Manage Settings", 20000);
        }

        public UITestControl ClickRibbonMenuItem(string itemName, int waitAmt = 100)
        {
            var ribbonButtons = StudioWindow.GetChildren();
            var control = ribbonButtons.FirstOrDefault(c => c.FriendlyName == itemName || c.GetChildren().Any(child => child.FriendlyName.Contains(itemName)));
            if(control == null)
            {
                var message = string.Format("Resource with name : [{0}] was not found", itemName);
                throw new Exception(message);
            }

            control.WaitForControlEnabled();
            var p = new Point(control.BoundingRectangle.X + 5, control.BoundingRectangle.Y + 5);
            Mouse.Click(p);
            Playback.Wait(waitAmt);
            return control;
        }

        public UITestControl CreateNewWorkflow(int waitAmt = 0)
        {
            var uiTestControlCollection = StudioWindow.GetChildren();
            var control = uiTestControlCollection.FirstOrDefault(c => c.FriendlyName == "UI_RibbonHomeTabWorkflowBtn_AutoID");
            if(control == null)
            {
                var message = string.Format("Resource with name : [{0}] was not found", "UI_RibbonHomeTabWorkflowBtn_AutoID");
                throw new Exception(message);
            }

            var p = new Point(control.BoundingRectangle.Left + 5, control.BoundingRectangle.Top + 5);
            Mouse.Click(p);
            Playback.Wait(500);

            var tab = TabManagerUIMap.GetActiveTab();

            Playback.Wait(waitAmt);

            return tab;
        }

        public UITestControl GetControlByName(string name)
        {
            var children = UIBusinessDesignStudioWindow.GetChildren();
            var control = children.FirstOrDefault(c => c.FriendlyName == name || c.GetChildren().Any(child => child.FriendlyName.Contains(name)));

            return control;
        }

        public void DebugShortcutKeyPress()
        {
            StudioWindow.SetFocus();
            SendKeys.SendWait("{F6}");
        }

        public void SchedulerShortcutKeyPress()
        {
            StudioWindow.SetFocus();
            KeyboardCommands.SendKey("{ALT}S", 2000);
        }
    }
}
