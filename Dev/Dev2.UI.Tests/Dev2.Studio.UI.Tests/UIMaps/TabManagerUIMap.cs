using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests;
using Microsoft.VisualStudio.TestTools.UITesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;

// ReSharper disable once CheckNamespace
namespace Dev2.CodedUI.Tests.TabManagerUIMapClasses
{
    // ReSharper disable RedundantExtendsListEntry
    public partial class TabManagerUIMap : UIMapBase
    // ReSharper restore RedundantExtendsListEntry
    {
        // UI_DocManager_AutoID
        readonly UIUI_TabManager _tabManager;

        public TabManagerUIMap()
        {
            _tabManager = new UIUI_TabManager(StudioWindow);
        }

        public string GetActiveTabName()
        {
            var idx = _tabManager.SelectedIndex;
            if(idx == -1)
            {
                Playback.Wait(2500);
                idx = _tabManager.SelectedIndex;
            }


            UITestControl tab = _tabManager.Tabs[idx];
            UITestControlCollection tabChildren = tab.GetChildren();
            string selectedTabName = string.Empty;
            foreach(var tabChild in tabChildren)
            {
                if(tabChild.ClassName == "Uia.TextBlock")
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
            if(tabManager != null)
            {
                var tabs = tabManager.GetChildren();
                return tabs.Count;
            }
            return 0;
        }

        public bool CloseTab(UITestControl theTab)
        {
            // Trav Changes ;)
            var closeBtn = theTab.GetChildren().FirstOrDefault(child => child.GetProperty("AutomationID").ToString() == "closeBtn");

            if(closeBtn != null)
            {
                Thread.Sleep(50);
                //Playback.Wait(50);
                Mouse.Click(closeBtn);
                return true;
            }

            return false;

        }

        public bool CloseTab(string tabName)
        {
            const int TotalTimeOut = 10;
            UITestControl control = FindTabByName(tabName);
            UITestControl close = control.GetChildren().FirstOrDefault(child => child.GetProperty("AutomationID").ToString() == "closeBtn");
            Point point;
            var timeout = 0;
            while(close != null && (!close.TryGetClickablePoint(out point) && timeout < TotalTimeOut))
            {
                timeout++;
            }
            if(close != null && timeout < TotalTimeOut)
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
            try
            {
                // first try closing wizards ;)
                Playback.Wait(150);
                SendKeys.SendWait("{ESC}");

                // fetch the darn thing once ;)
                var tabManager = GetManager();

                if(tabManager != null)
                {
                    var tabs = tabManager.GetChildren();

                    foreach(var tab in tabs)
                    {
                        CloseTab_Click_No(tab);
                    }
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // just do it ;)
            }
        }

        public void CloseTab_Click_Yes(string tabName)
        {
            CloseTab(tabName);

            UITestControlCollection saveDialogButtons = GetWorkflowNotSavedButtons();
            UITestControl cancelButton = saveDialogButtons[0];
            Point p = new Point(cancelButton.Left + 25, cancelButton.Top + 15);
            Mouse.Move(p);
            Mouse.Click();
        }

        public bool CloseTab_Click_No(UITestControl theTab)
        {
            bool willHaveDialog = WillTabHaveSaveDialog(theTab);

            if(CloseTab(theTab))
            {
                // Only if we expect a save dialog should we search for it ;)
                if(willHaveDialog)
                {
                    try
                    {
                        UITestControlCollection saveDialogButtons = GetWorkflowNotSavedButtons();
                        if(saveDialogButtons.Count > 0)
                        {
                            UITestControl theBtn = saveDialogButtons[1];
                            Point p = new Point(theBtn.Left + 25, theBtn.Top + 15);
                            Mouse.MouseMoveSpeed = 10000;
                            Mouse.Move(p);
                            Mouse.Click();
                            return true;
                        }
                    }
                    catch(Exception)
                    {
                        return true;
                        //This is empty because if the pop cant be found then the tab must just close;)
                    }
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Wills the tab have save dialog.
        /// </summary>
        /// <param name="theTab">The tab.</param>
        /// <returns></returns>
        public bool WillTabHaveSaveDialog(UITestControl theTab)
        {
            Playback.Wait(200);
            var tabNameControl = theTab.GetChildren().FirstOrDefault(c => c.ClassName == "Uia.TextBlock");

            if(tabNameControl != null && tabNameControl.FriendlyName.EndsWith("*"))
            {
                return true;
            }

            return false;
        }

        public void CloseTab_Click_Cancel(string tabName)
        {
            CloseTab(tabName);

            UITestControlCollection saveDialogButtons = GetWorkflowNotSavedButtons();
            UITestControl cancelButton = saveDialogButtons[2];
            Point p = new Point(cancelButton.Left + 25, cancelButton.Top + 15);
            Mouse.Move(p);
            Mouse.Click();
        }

        public UITestControl GetActiveTab()
        {
            Playback.Wait(500);
            StudioWindow.WaitForControlEnabled();
            var tabMgr = new UIUI_TabManager(StudioWindow);
            var idx = tabMgr.SelectedIndex;

            // to do remove below once performance is sorted!
            if(idx == -1)
            {
                idx = tabMgr.SelectedIndex;
                if(idx == -1)
                {
                    return null;
                }
            }

            if(idx >= tabMgr.Tabs.Count)
            {
                Playback.Wait(10000);
            }

            return _tabManager.Tabs[_tabManager.SelectedIndex];
        }

        public UITestControl GetWorkSurface(UITestControl theTab)
        {
            return theTab.GetChildren().FirstOrDefault(t => t.GetProperty("AutomationID").ToString().Contains("WorkSurfaceContext"));
        }

        public void ClickTab(string tabName)
        {
            UITestControl findTabByName = FindTabByName(tabName);
            Mouse.Click(findTabByName);
        }
    }
}
