using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses
{
    public partial class RibbonUIMap
    {
        public void ClickRibbonMenu(string menuAutomationId)
        {
            // This needs some explaining :)
            // Due to the way the WpfRibbon works, unless it has been used, the default tab will not be properly initialised.
            // This will cause problems if you need to use it (Automation wise)
            // To combat this, we check if the tab we have got is actually a valid tab (Check its bounding)
            // If it's not a valid tab, we click an alternate tab - This validates our original tab.
            // We then recusrsively call the method again with the now validated tab, and it works as itended.
            // Note: This recursive call will only happen the first time the ribbon is used, as it will subsequently be initialised correctly.

            WpfTabList uIRibbonTabList = this.UIBusinessDesignStudioWindow.UIRibbonTabList;
            UITestControlCollection tabList = uIRibbonTabList.Tabs;
            UITestControl theControl = new UITestControl();
            foreach (WpfTabPage tabPage in tabList)
            {
                if(tabPage.Name == menuAutomationId)
                {
                    theControl = tabPage;
                    Point p = new Point(theControl.BoundingRectangle.X + 5, theControl.BoundingRectangle.Y + 5);
                    if(p.X < 5)
                    {
                        UITestControlCollection myCollection = theControl.GetChildren();
                        UITestControl testControl = myCollection[0].Container;
                        p = new Point(theControl.GetChildren()[0].BoundingRectangle.X + 5, theControl.GetChildren()[0].BoundingRectangle.Y + 5);
                    }
                    if(p.X > 5)
                    {
                        Mouse.Click(p);
                        return;
                    }
                }
                else
                {
                    theControl = tabPage;
                    Point p = new Point(theControl.BoundingRectangle.X + 5, theControl.BoundingRectangle.Y + 5);
                    if(p.X > 5)
                    {

                        Mouse.Click(p);
                        break;
                    }
                }
            }
            
            // Somethign has gone wrong - Retry!

            ClickRibbonMenu(menuAutomationId);

            /*
            for (int j = 0; j < uIRibbonTabList.Tabs.Count; j++)
            {
                if (uIRibbonTabList.Tabs[j].GetProperty("AutomationID").ToString() == menuAutomationId)
                {
                    if (uIRibbonTabList.Tabs[j].GetProperty("ControlType").ToString() == "TabPage" && uIRibbonTabList.Tabs[j].GetProperty("ClassName").ToString() == "Uia.RibbonTab")
                    {
                        string headerText = uIRibbonTabList.Tabs[j].GetProperty("Header").ToString();
                        if (uIRibbonTabList.Tabs[j].Left > -1)
                        {
                            theControl = uIRibbonTabList.Tabs[j];
                            break;
                        }
                    }
                }
            }
            
            Point testPoint = new Point();
            int oX = theControl.Left + 5;
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
             **/
        }

        public void ClickRibbonMenuItem(string menuName, string itemName)
        {
            // Wait awhile due to a rare bug
            System.Threading.Thread.Sleep(1500);
            ClickRibbonMenu(menuName);
            UITestControl theControl = getControl(menuName, itemName);
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
