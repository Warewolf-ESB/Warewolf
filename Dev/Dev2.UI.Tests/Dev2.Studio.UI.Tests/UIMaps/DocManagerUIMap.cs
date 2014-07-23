using System.Drawing;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;

// ReSharper disable CheckNamespace
namespace Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses
// ReSharper restore CheckNamespace
{
    public partial class DocManagerUIMap
    {
        UITestControl _dockManager;

        public DocManagerUIMap()
        {
            

            _dockManager = VisualTreeWalker.GetControl("UI_DocManager_AutoID");
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
            new Point();
            UIBusinessDesignStudioWindow.SetFocus();
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
