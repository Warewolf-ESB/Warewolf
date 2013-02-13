using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Dev2.CodedUI.Tests.TabManagerUIMapClasses
{

    public partial class TabManagerUIMap
    {
        public string GetActiveTabName()
        {
            var theTabManager = GetTabManager();
            string selectedTabName = theTabManager.Tabs[theTabManager.SelectedIndex].FriendlyName;
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
            Mouse.Click(new Point(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2));
            UITestControl control = FindTabByName(tabName);
            UITestControl close = new UITestControl(control);
            close.SearchProperties["AutomationId"] = "closeBtn";
            Mouse.Click(close);
            // Rare closure bug if you click a DDL before
            UITestControl theTab = FindTabByName("tabName");
            if(theTab != null)
            {
                Mouse.Click(close);
            }
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
                    System.Threading.Thread.Sleep(2500);
                    isFirst = false;
                }
                CloseTab(theTab);
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
            Point p = new Point(cancelButton.Left + 25, cancelButton.Top + 25);
            Mouse.MouseMoveSpeed = 1000;
            Mouse.Move(p);
            Mouse.MouseMoveSpeed = 450;
            Mouse.Click();
        }

        public void CloseTab_Click_No(string tabName)
        {
            CloseTab(tabName);

            UITestControlCollection saveDialogButtons = GetWorkflowNotSavedButtons();
            UITestControl cancelButton = saveDialogButtons[1];
            Point p = new Point(cancelButton.Left + 25, cancelButton.Top + 25);
            Mouse.MouseMoveSpeed = 1000;
            Mouse.Move(p);
            Mouse.MouseMoveSpeed = 450;
            Mouse.Click();
        }

        public void CloseTab_Click_Cancel(string tabName)
        {
            CloseTab(tabName);

            UITestControlCollection saveDialogButtons = GetWorkflowNotSavedButtons();
            UITestControl cancelButton = saveDialogButtons[2];
            Point p = new Point(cancelButton.Left + 25, cancelButton.Top + 25);
            Mouse.MouseMoveSpeed = 1000;
            Mouse.Move(p);
            Mouse.MouseMoveSpeed = 450;
            Mouse.Click();
        }
    }
}
