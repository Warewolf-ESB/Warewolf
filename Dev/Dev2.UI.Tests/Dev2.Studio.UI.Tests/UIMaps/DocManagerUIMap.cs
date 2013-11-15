using System.Drawing;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Windows.Forms;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public partial class DocManagerUIMap
    {
        UITestControl _dockManager;

        public DocManagerUIMap()
        {
            var vstw = new VisualTreeWalker();

            _dockManager = vstw.GetControl("UI_DocManager_AutoID");
        }

        /// <summary>
        /// Clicks open one of the DocManager tabs
        /// </summary>
        /// <param name="tabName">The name of the tab (EG: Explorer, Toolbox, Variables, Output, etc)</param>
        public void ClickOpenTabPage(string tabName)
        {
            WpfTabPage theTab= FindTabPage(tabName);

            Mouse.Click(theTab, new Point(10, 10));
        }

        public bool DoesTabExist(string tabName)
        {
            WpfTabPage theTab = FindTabPage(tabName);
            Point p;
            return (theTab.TryGetClickablePoint(out p));
        }

        public void CloseStudio()
        {
            Point p = new Point();
            this.UIBusinessDesignStudioWindow.SetFocus();
            SendKeys.SendWait("%{F4}");
        }

        public UITestControl GetMainPane()
        {
            foreach(var child in _dockManager.GetChildren())
            {
                if (child.ClassName == "Uia.SplitPane" && child.GetProperty("AutomationID").ToString() == string.Empty && child.Width != -1 && child.Height != -1)
                {
                    return child;
                }
            }
            return null;
        }
    }
}
