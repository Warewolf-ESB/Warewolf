namespace Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Input;
    using System.CodeDom.Compiler;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System.Windows.Forms;
    
    
    public partial class DebugUIMap
    {
        public void ClickItem(int row)
        {
            WpfRow theRow = GetRow(row);
            WpfEdit requiredEdit = (WpfEdit)theRow.GetChildren()[2].GetChildren()[0];
            Mouse.Click(requiredEdit, new Point(5, 5));

        }

        public int CountRows()
        {
            WpfWindow debugWindow = GetDebugWindow();
            UITestControlCollection rowList = debugWindow.GetChildren()[1].GetChildren()[0].GetChildren()[1].GetChildren();
            return rowList.Count;
        }

        public void CloseDebugWindow_ByCancel()
        {
            WpfWindow debugWindow = GetDebugWindow();
            UITestControl theControl = new UITestControl(debugWindow);
            theControl.SearchProperties.Add("AutomationId", "UI_Cancelbtn_AutoID");
            theControl.Find();
            Mouse.Click(theControl, new Point(5, 5));
        }

        public void CloseDebugWindow()
        {
            SendKeys.SendWait("{ESCAPE}");
        }

        public bool DebugWindowExists()
        {
            WpfWindow uIDebugWindow = this.UIDebugWindow;
            Point p = new Point();
            if (uIDebugWindow.TryGetClickablePoint(out p))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ClickXMLTab()
        {
            WpfWindow uIDebugWindow = this.UIDebugWindow;
            WpfTabPage XMLTabPage = (WpfTabPage)uIDebugWindow.GetChildren()[1].GetChildren()[1];
            Mouse.Click(XMLTabPage, new Point(5, 5));
        }

        public UIBusinessDesignStudioWindow UIBusinessDesignStudioWindow
        {
            get
            {
                if((_uiBusinessDesignStudioWindow == null))
                {
                    _uiBusinessDesignStudioWindow = new UIBusinessDesignStudioWindow();
                }
                return _uiBusinessDesignStudioWindow;
            }
        }

        UIBusinessDesignStudioWindow _uiBusinessDesignStudioWindow;
    }
}
