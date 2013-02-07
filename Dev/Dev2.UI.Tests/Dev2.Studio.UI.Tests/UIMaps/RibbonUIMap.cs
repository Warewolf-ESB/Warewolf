namespace Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses
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
    
    
    public partial class RibbonUIMap
    {
        public void ClickRibbonMenu(string menuAutomationID)
        {
            WpfTabList uIRibbonTabList = this.UIBusinessDesignStudioWindow.UIRibbonTabList;
            UITestControl theControl = new UITestControl();

            for (int j = 0; j < uIRibbonTabList.Tabs.Count; j++)
            {
                if (uIRibbonTabList.Tabs[j].GetProperty("AutomationID").ToString() == menuAutomationID)
                {
                    if (uIRibbonTabList.Tabs[j].GetProperty("ControlType").ToString() == "TabPage" && uIRibbonTabList.Tabs[j].GetProperty("ClassName").ToString() == "Uia.RibbonTab")
                    {
                        string headerText = uIRibbonTabList.Tabs[j].GetProperty("Header").ToString();
                        try
                        {
                            UITestControl topParent = theControl.TopParent;
                        }
                        catch
                        {
                            // Do nothing
                        }
                        //string parentName = topParent.Au
                        theControl = uIRibbonTabList.Tabs[j];
                        break;
                    }
                }
            }

            Point testPoint = new Point();
            int oX = theControl.Left + 5;
            int oY = theControl.Top + 5;
            UITestControl altControl = new UITestControl();
            if (!theControl.TryGetClickablePoint(out testPoint))
            {
                altControl = theControl.GetChildren()[0];
            }
            int pX = altControl.Left + 5;
            int pY = altControl.Top + 5;
            Point p = new Point();
            
            if ((oX != pX) && (pX != 4 && pY != 4))
            {
                p = new Point(altControl.Left + 5, altControl.Top + 5); 
            }
            else
            {
                p = new Point(theControl.Left + 5, theControl.Top + 5);
            }
            
            Mouse.Click(p);
        }

        public void ClickRibbonMenuItem(string menuName, string itemName)
        {
            // Wait awhile due to a rare bug
            System.Threading.Thread.Sleep(1500);
            ClickRibbonMenu(menuName);
            UITestControl theControl = getControl(menuName, itemName);
            string autoID = theControl.GetProperty("AutomationID").ToString(); // Buttons have no automation ID's...
            Point p = new Point(theControl.BoundingRectangle.X + 5, theControl.BoundingRectangle.Y + 5);
            Mouse.Click(p);
        }

        /*
        public bool RecordedFeedbackWindowExists()
        {
            
            //Recorded Feedback
        }*/
    }


}
