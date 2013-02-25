using System.Drawing;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Windows.Forms;

namespace Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses
{
    public partial class DocManagerUIMap
    {
        /// <summary>
        /// Clicks open one of the DocManager tabs
        /// </summary>
        /// <param name="tabName">The name of the tab (EG: Explorer, Toolbox, Variables, Output, etc)</param>
        public void ClickOpenTabPage(string tabName)
        {
            WpfTabPage theTab = FindTabPage(tabName);
            Mouse.Click(theTab, new Point(5, 5));
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
    }
}
