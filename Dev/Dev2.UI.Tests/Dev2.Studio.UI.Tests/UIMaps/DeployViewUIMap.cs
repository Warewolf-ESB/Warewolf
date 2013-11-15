using System.Windows.Forms;
using Dev2.Studio.UI.Tests;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITest.Extension;

namespace Dev2.CodedUI.Tests.UIMaps.DeployViewUIMapClasses
{
    using System.Drawing;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;


    public partial class DeployViewUIMap : UIMapBase
    {
        public void SelectServers(UITestControl theTab, string sourceServer, string destinationServer)
        {

            // Choose the source server
            ChooseSourceServer(theTab, sourceServer);

            // Choose the destination server
            ChooseDestinationServer(theTab, destinationServer);
        }

        public void ChooseSourceServer(UITestControl theTab, string serverName)
        {
            UITestControl sourceServerList = GetSourceServerList(theTab);
            WpfComboBox wpfComboList = (WpfComboBox)sourceServerList;
            
            foreach (WpfListItem theItem in wpfComboList.Items)
            {
                if (theItem.AutomationId == "U_UI_SourceServer_AutoID" + serverName )
                {
                    theItem.Select();
                    break;
                }
            }
        }

        public void ChooseDestinationServer(UITestControl theTab, string serverName)
        {
            UITestControl destinationServerList = GetDestinationServerList(theTab);
            WpfComboBox wpfComboList = (WpfComboBox)destinationServerList;
            Mouse.Click(wpfComboList, new Point(5, 5));
            foreach (WpfListItem theItem in wpfComboList.Items)
            {
                if (theItem.AutomationId == "U_UI_DestinationServercbx_AutoID_" + serverName)
                {
                    theItem.Select();
                    break;
                }
            }
        }

        public bool DoSourceAndDestinationCountsMatch(UITestControl theTab)
        {
            int sourceCount = GetSelectedDeployCount(theTab);
            int destinationCount = GetSelectedDeploySummaryCount(theTab);

            if (sourceCount == destinationCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DoesSourceServerHaveDeployItems(UITestControl theTab)
        {
            int sourceCount = GetSelectedDeployCount(theTab);
            return (sourceCount > 0);
        }

        public bool DoesDestinationServerHaveItems(UITestControl theTab)
        {
            UITestControl destinationExplorer = DestinationServerTreeviewExplorer(theTab);
            if(destinationExplorer.GetChildren().Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void ClickDeploy(UITestControl theTab)
        {
            WpfButton deployButton = GetDeployButton(theTab);
            Mouse.Move(deployButton, new Point(10, 10));
        }

        public bool IsDeployButtonEnabled(UITestControl theTab)
        {
            WpfButton deployButton = GetDeployButton(theTab);
            return deployButton.Enabled;
        }

        public void EnterTextInSourceServerFilterBox(UITestControl theTab, string text)
        {
            WpfEdit theBox = GetSourceServerFilterBox(theTab);
            Mouse.Click(new Point(theBox.BoundingRectangle.X + 15, theBox.BoundingRectangle.Y + 15));
            SendKeys.SendWait(text);
        }

        public void EnterTextInDestinationServerFilterBox(UITestControl theTab, string text)
        {
            WpfEdit theBox = GetDestinationServerFilterBox(theTab);
            Mouse.Click(new Point(theBox.BoundingRectangle.X + 15, theBox.BoundingRectangle.Y + 15));
            SendKeys.SendWait(text);
        }

        private UITestControlCollection GetWebsiteGridBlocks(UITestControl theTab)
        {
            UITestControlCollection requiredChildren = GetDeployUserControlChildren(theTab);
            UITestControlCollection gridBlocks = requiredChildren[0].GetChildren();
            return gridBlocks;
        }

        // This method is only used once, so is currently hard-coded to return a specific block
        // - StudioBugTests.cs - OpeningOldWebPageTwiceRetainsData
        public UITestControl GetWebsiteGridBlock(UITestControl theTab)
        {
            UITestControlCollection blocks = GetWebsiteGridBlocks(theTab);
            UITestControl theBlock = blocks[0]; // Weird Layout - Block 0 is 1,0 and not 0,0 as you'd think
            return theBlock;
        }

        public WpfTree GetSourceNavigationTree()
        {
            var activeTab = TabManagerUIMap.GetActiveTab();
            var deployUserControl = GetDeployUserControl(activeTab);
            var vstw = new VisualTreeWalker();

            return vstw.GetChildByAutomationIDPath(deployUserControl, "SourceNavigationView", "Navigation") as WpfTree;
        }
    }
}
