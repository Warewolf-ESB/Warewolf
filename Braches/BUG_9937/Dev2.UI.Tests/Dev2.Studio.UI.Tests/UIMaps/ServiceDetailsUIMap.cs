namespace Dev2.Studio.UI.Tests.UIMaps.ServiceDetailsUIMapClasses
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
    
    
    public partial class ServiceDetailsUIMap
    {
        public bool ServiceDetailsWindowExists()
        {
            WpfWindow theWindow = ServiceDetailsWindow();
            Point p = new Point();
            bool doesExist = theWindow.TryGetClickablePoint(out p);
            return doesExist;
        }

        public void getName()
        {
            WpfWindow theWindow = ServiceDetailsWindow();

            UITestControlCollection windowChildren = theWindow.GetChildren();
            foreach (UITestControl theControl in windowChildren)
            {
                string friendlyName = theControl.FriendlyName;
                if (friendlyName == "")
                {
                    UITestControlCollection subChildren = theControl.GetChildren();
                    foreach (UITestControl subControl in subChildren)
                    {
                        string newFriendlyName = subControl.FriendlyName;
                        int j = 0;
                    }
                }
            }
        }
    }
}
