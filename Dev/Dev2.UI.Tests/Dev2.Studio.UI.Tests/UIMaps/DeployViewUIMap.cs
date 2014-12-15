
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Linq;
using Dev2.Studio.UI.Tests;
using Dev2.Studio.UI.Tests.Utils;
using System;
using System.Windows.Forms;

namespace Dev2.CodedUI.Tests.UIMaps.DeployViewUIMapClasses
{
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System.Drawing;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;


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
            WpfComboBox comboBox = (WpfComboBox)sourceServerList;
            var serverItem = comboBox.Items.ToList().FirstOrDefault(c => c.FriendlyName == serverName);
            if(serverItem != null)
            {
                comboBox.SelectedIndex = comboBox.Items.IndexOf(serverItem);
            }
        }

        public void ChooseSourceServerWithKeyboard(UITestControl theTab, string serverName)
        {
            UITestControl sourceServerList = GetSourceServerList(theTab);
            Mouse.Click(sourceServerList);
            Keyboard.SendKeys("{DOWN}{ENTER}");
            Playback.Wait(2000);
        }

        public void ChooseDestinationServer(UITestControl theTab, string serverName)
        {
            UITestControl destinationServerList = GetDestinationServerList(theTab);


            //Wait for the connect control to be ready
            int counter = 0;
            while(!destinationServerList.Enabled && counter < 5)
            {
                Playback.Wait(2000);
                counter++;
            }
            if(!destinationServerList.Enabled)
            {
                throw new Exception("The connect control drop down is still disabled after 10 sec wait.");
            }

            // Click it to expand it
            Mouse.Click(destinationServerList, new Point(10, 10));
            Playback.Wait(500);

            var item = VisualTreeWalker.GetChildByAutomationIdPath(destinationServerList, "UI_DestinationServercbx_AutoID_" + serverName);

            Mouse.Click(item, new Point(5, 5));

            //Wait for the connect control to be ready
            int afterCounter = 0;
            while(!destinationServerList.Enabled && afterCounter < 5)
            {
                Playback.Wait(2000);
                afterCounter++;
            }
            if(!destinationServerList.Enabled)
            {
                throw new Exception("The connect control drop down is still disabled after 10 sec wait.");
            }
        }

        public void ChooseDestinationServerWithKeyboard(UITestControl theTab, string serverName)
        {
            UITestControl destinationServerList = GetDestinationServerList(theTab);
            Mouse.Click(destinationServerList);
            Keyboard.SendKeys("{UP}{ENTER}");
            Playback.Wait(2000);
        }

        public bool DoSourceAndDestinationCountsMatch(UITestControl theTab)
        {
            int sourceCount = GetSelectedDeployCount(theTab);
            int destinationCount = GetSelectedDeploySummaryCount(theTab);

            if(sourceCount == destinationCount)
            {
                return true;
            }
            return false;
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
            return true;
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
            Playback.Wait(1500);
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


            return VisualTreeWalker.GetChildByAutomationIdPath(deployUserControl, "SourceNavigationView", "Navigation") as WpfTree;
        }

        public UITestControl GetSourceConnectButton(UITestControl theTab)
        {
            UITestControlCollection requiredChildren = GetDeployUserControlChildren(theTab);
            foreach(UITestControl theControl in requiredChildren)
            {
                if(theControl.GetProperty("AutomationId").ToString() == "ConnectUserControl")
                {
                    foreach(UITestControl tC in theControl.GetChildren())
                    {
                        if(tC.GetProperty("AutomationId").ToString() == "UI_SourceConnectServerbtn_AutoID")
                        {
                            return tC;

                        }
                    }

                }
            }
            return null;
        }

        public UITestControl GetDestinationEditConnectionButton(UITestControl theTab)
        {
            UITestControlCollection requiredChildren = GetDeployUserControlChildren(theTab);
            foreach(UITestControl theControl in requiredChildren)
            {
                if(theControl.GetProperty("AutomationId").ToString() == "ConnectUserControl")
                {
                    foreach(UITestControl tC in theControl.GetChildren())
                    {
                        if(tC.GetProperty("AutomationId").ToString() == "UI_DestinationServerEditbtn_AutoID")
                        {
                            return tC;
                        }
                    }

                }
            }
            return null;
        }

        public UITestControl GetDestinationConnectButton(UITestControl theTab)
        {
            UITestControlCollection requiredChildren = GetDeployUserControlChildren(theTab);
            foreach(UITestControl theControl in requiredChildren)
            {
                if(theControl.GetProperty("AutomationId").ToString() == "ConnectUserControl")
                {
                    foreach(UITestControl tC in theControl.GetChildren())
                    {
                        if(tC.GetProperty("AutomationId").ToString() == "UI_DestinationConnectServerbtn_AutoID")
                        {
                            return tC;

                        }
                    }
                }
            }
            return null;
        }

        public UITestControl GetSourceContolByFriendlyName(UITestControl theTab, string controlName)
        {
            UITestControlCollection requiredChildren = GetDeployUserControlChildren(theTab);
            return (from theControl in requiredChildren
                    where theControl.GetProperty("AutomationId").ToString() == "ConnectUserControl"
                    select theControl.GetChildren().FirstOrDefault(c => c.FriendlyName == controlName && ((WpfControl)c).AutomationId.Contains("Source"))).FirstOrDefault();
        }

        public UITestControl GetDestinationContolByFriendlyName(UITestControl theTab, string controlName)
        {
            UITestControlCollection requiredChildren = GetDeployUserControlChildren(theTab);
            return (from theControl in requiredChildren
                    where theControl.GetProperty("AutomationId").ToString() == "ConnectUserControl"
                    select theControl.GetChildren().FirstOrDefault(c => c.FriendlyName == controlName && ((WpfControl)c).AutomationId.Contains("Destination"))).FirstOrDefault();
        }
    }
}
