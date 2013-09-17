using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UITesting;
using System.Drawing;
using System.Windows.Forms;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Dev2.CodedUI.Tests.TabManagerUIMapClasses
{

    public partial class TabManagerUIMap
    {
        public string GetActiveTabName()
        {
            var theTabManager = GetTabManager();
            UITestControl tab = theTabManager.Tabs[theTabManager.SelectedIndex];
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
            UIUI_TabManager_AutoIDTabList1 tabManager = GetTabManager();
            string tabName = tabManager.Tabs[position].FriendlyName;
            return tabName;
        }

        public int GetTabCount()
        {
            return GetChildrenCount();
        }

        public void CloseTab(string tabName)
        {
            UITestControl control = FindTabByName(tabName);
            UITestControl close = new UITestControl(control);
            close.SearchProperties["AutomationId"] = "closeBtn";
            Playback.Wait(150);
            Mouse.Click(close);
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
            bool isFirst = true;
            int openTabs = GetTabCount();
            while (openTabs != 0)
            {
                string theTab = GetActiveTabName();
                if (isFirst)
                {
                    // Click in the middle of the screen and wait, incase a side menu is open (Which covers the tabs "X")
                    UITestControl zeTab = FindTabByName(theTab);
                    Mouse.Click(new Point(zeTab.BoundingRectangle.X + 500, zeTab.BoundingRectangle.Y + 500));
                    Playback.Wait(2500);
                    isFirst = false;
                }
                //CloseTab(theTab);
                CloseTab_Click_No(theTab);
                SendKeys.SendWait("n");

                SendKeys.SendWait("{DELETE}");     // 
                SendKeys.SendWait("{BACKSPACE}");  // Incase it was actually typed
                //

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

        public void CloseTab_Click_No(string tabName)
        {
            CloseTab(tabName);

            try
            {
                UITestControlCollection saveDialogButtons = GetWorkflowNotSavedButtons();
                if (saveDialogButtons.Count > 0)
                {
                    UITestControl cancelButton = saveDialogButtons[1];
                    Point p = new Point(cancelButton.Left + 25, cancelButton.Top + 15);
                    Mouse.MouseMoveSpeed = 1000;
                    Mouse.Move(p);
                    Mouse.MouseMoveSpeed = 450;
                    Mouse.Click();
                }
            }
            catch (Exception ex)
            {
                //This is empty because if the pop cant be found then the tab must just close;)
            }
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
            var theTabManager = GetTabManager();
            UITestControl tab = theTabManager.Tabs[theTabManager.SelectedIndex];
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
            UIBusinessDesignStudioWindow2 theWindow = new UIBusinessDesignStudioWindow2();
            UIUI_TabManager_AutoIDTabList1 tabMgr = new UIUI_TabManager_AutoIDTabList1(theWindow);
            //string firstName = uIServiceDetailsTabPage.FriendlyName;
            UITestControl control = tabMgr.GetTab(selectedTabName);

            return control;
        }
    }
}
