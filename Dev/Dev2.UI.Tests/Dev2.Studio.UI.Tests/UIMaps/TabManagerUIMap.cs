using System;
using System.Linq;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using System.Drawing;
using System.Windows.Forms;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Dev2.CodedUI.Tests.TabManagerUIMapClasses
{
    public partial class TabManagerUIMap
    {
        UIUI_TabManager_AutoIDTabList1 _tabManager;

        public TabManagerUIMap()
        {
            var theWindow = new UIBusinessDesignStudioWindow2();
            _tabManager = new UIUI_TabManager_AutoIDTabList1(theWindow);
        }

        public string GetActiveTabName()
        {
            UITestControl tab = _tabManager.Tabs[_tabManager.SelectedIndex];
            UITestControlCollection tabChildren = tab.GetChildren();
            string selectedTabName = string.Empty;
            foreach (var tabChild in tabChildren)
            {
                if (tabChild.ClassName == "Uia.TextBlock")
                {
                    selectedTabName = tabChild.FriendlyName;
                    break;
                }
            }
            //string selectedTabName = theTabManager.Tabs[theTabManager.SelectedIndex].FriendlyName;
            return selectedTabName;
        }

        public void Click(string tabName)
        {
            UITestControl control = FindTabByName(tabName);
            Mouse.Click(new Point(control.BoundingRectangle.X + 30, control.BoundingRectangle.Y + 5));
        }

        public void Click(UITestControl existingTab)
        {
            Point p = new Point(existingTab.BoundingRectangle.X + 30, existingTab.BoundingRectangle.Y + 5);
            Mouse.Click(p);
        }

        public void DragTabToTab(string firstTabName, string secondTabName)
        {
            UITestControl firstTab = FindTabByName(firstTabName);
            Point pointOnFirstTab = new Point(firstTab.BoundingRectangle.X + 10, firstTab.BoundingRectangle.Y + 10);

            UITestControl secondTab = FindTabByName(secondTabName);
            Point pointOnSecondTab = new Point(secondTab.BoundingRectangle.X + 10, secondTab.BoundingRectangle.Y + 10);

            Mouse.StartDragging(firstTab, pointOnFirstTab);
            Mouse.StopDragging(secondTab, pointOnSecondTab);

        }

        public string GetTabNameAtPosition(int position)
        {
            string tabName = _tabManager.Tabs[position].FriendlyName;
            return tabName;
        }

        public int GetTabCount()
        {
            var tabManager = GetManager();
            if (tabManager != null)
            {
                var tabs = tabManager.GetChildren();
                return tabs.Count;
            }
            return 0;
        }

        public bool CloseTab(UITestControl theTab)
        {
            foreach (var child in theTab.GetChildren().Where(child => child.GetProperty("AutomationID").ToString() == "closeBtn"))
            {
                var point = new Point();
                if (!child.TryGetClickablePoint(out point))
                {
                    ExplorerUIMap.ClosePane(theTab);
                }
                if(!child.TryGetClickablePoint(out point))
                {
                    Mouse.Move(new Point(theTab.BoundingRectangle.Left + 100, theTab.BoundingRectangle.Top + 500));
                }
                Playback.Wait(500);
                Mouse.Click(child);
                return true;
            }
            return false;
        }

        public bool CloseTab(string tabName)
        {
            const int totalTimeOut = 10;
            UITestControl control = FindTabByName(tabName);
            UITestControl close = new UITestControl(control);
            close.SearchProperties["AutomationId"] = "closeBtn";
            Playback.Wait(150);
            var point = new Point();
            var timeout = 0;
            while(!close.TryGetClickablePoint(out point) && timeout < totalTimeOut)
            {
                //try close explorer pane
                ExplorerUIMap.ClosePane(control);
                //this is for close all tabs:
                //the tab switch caused by closing the tab
                //triggers show resource in explorer tree
                //causing the explorer pane to pop out
                //blocking the tab close button for the next tab close
                timeout++;
                if(timeout>3)
                {
                    //try moving mouse over the pane (only necessary when an unsaved workflow has just been saved)
                    Mouse.Move(new Point(UIBusinessDesignStudioWindow.Left + 200, UIBusinessDesignStudioWindow.Top + 200));
                }
            }
            if (timeout < totalTimeOut)
            {
                Mouse.Click(close);
                return true;
            }
            return false;
        }

        public void MiddleClickCloseTab(string tabName)
        {
            Mouse.Click(new Point(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2));
            UITestControl control = FindTabByName(tabName);
            Point pointInTab = new Point(control.BoundingRectangle.X + 25, control.BoundingRectangle.Y + 10);
            Mouse.Move(pointInTab);
            Mouse.Click(MouseButtons.Middle);
        }

        public void CloseAllTabs()
        {
            int openTabs = GetTabCount();
            var canCloseTab = true;
            while(openTabs != 0 && canCloseTab)
            {
                var activeTab = GetActiveTab();
                canCloseTab = CloseTab_Click_No(activeTab);
                openTabs = GetTabCount();
            }
        }

        public void CloseTab_Click_Yes(string tabName)
        {
            CloseTab(tabName);

            UITestControlCollection saveDialogButtons = GetWorkflowNotSavedButtons();
            UITestControl cancelButton = saveDialogButtons[0];
            Point p = new Point(cancelButton.Left + 25, cancelButton.Top + 15);
            Mouse.MouseMoveSpeed = 1000;
            Mouse.Move(p);
            Mouse.MouseMoveSpeed = 450;
            Mouse.Click();
        }

        public bool CloseTab_Click_No(UITestControl theTab)
        {
            if (CloseTab(theTab))
            {
                try
                {
                    UITestControlCollection saveDialogButtons = GetWorkflowNotSavedButtons();
                    if(saveDialogButtons.Count > 0)
                    {
                        UITestControl cancelButton = saveDialogButtons[1];
                        Point p = new Point(cancelButton.Left + 25, cancelButton.Top + 15);
                        Mouse.MouseMoveSpeed = 1000;
                        Mouse.Move(p);
                        Mouse.MouseMoveSpeed = 450;
                        Mouse.Click();
                        return true;
                    }
                }
                catch(Exception ex)
                {
                    return true;
                    //This is empty because if the pop cant be found then the tab must just close;)
                }
            }
            return false;
        }

        public void CloseTab_Click_Cancel(string tabName)
        {
            CloseTab(tabName);

            UITestControlCollection saveDialogButtons = GetWorkflowNotSavedButtons();
            UITestControl cancelButton = saveDialogButtons[2];
            Point p = new Point(cancelButton.Left + 25, cancelButton.Top + 15);
            Mouse.MouseMoveSpeed = 1000;
            Mouse.Move(p);
            Mouse.MouseMoveSpeed = 450;
            Mouse.Click();
        }

        public UITestControl GetActiveTab()
        {
            var tab = _tabManager.Tabs[_tabManager.SelectedIndex];
            var tabChildren = tab.GetChildren();
            var selectedTabName = string.Empty;
            foreach (var tabChild in tabChildren)
            {
                if (tabChild.ClassName == "Uia.TextBlock")
                {
                    selectedTabName = tabChild.FriendlyName;
                    break;
                }
            }
            var control = _tabManager.GetTab(selectedTabName);

            return control;
        }
    }
}
