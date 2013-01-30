namespace Dev2.CodedUI.Tests.UIMaps.DeployViewUIMapClasses
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
    using System.Windows.Forms;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;


    public partial class DeployViewUIMap
    {
        public void SelectServers(UITestControl theTab, string sourceServer, string destinationServer)
        {

            // Choose the source server
            ChooseSourceServer(theTab, sourceServer);

            // Choose the destination server
            ChooseDestinationServer(theTab, destinationServer);
        }

        private void ChooseSourceServer(UITestControl theTab, string serverName)
        {
            UITestControl sourceServerList = GetSourceServerList(theTab);
            WpfComboBox wpfComboList = (WpfComboBox)sourceServerList;
            
            foreach (WpfListItem theItem in wpfComboList.Items)
            {
                if (theItem.AutomationId == "UI_SourceServer_" + serverName + "_AutoID")
                {
                    theItem.Select();
                    break;
                }
            }
        }

        private void ChooseDestinationServer(UITestControl theTab, string serverName)
        {
            UITestControl destinationServerList = GetDestinationServerList(theTab);
            WpfComboBox wpfComboList = (WpfComboBox)destinationServerList;
            foreach (WpfListItem theItem in wpfComboList.Items)
            {
                if (theItem.AutomationId == "UI_DestinationServer_" + serverName + "_AutoID")
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

            //UITestControl theSourceServerTree = GetSourceServerTree(theTab);


            /*
            List<WpfTreeItem> selectedNodes = new List<WpfTreeItem>();
            WpfTreeItem sourceTree =(WpfTreeItem)theSourceServerTree;
            foreach (WpfTreeItem subNodes in sourceTree.Nodes) // Workflow Services, Workder Services, Sources
            {
                WpfCheckBox theCheckBox = (WpfCheckBox)subNodes.GetChildren()[1];
                if (theCheckBox.Checked)
                {
                    selectedNodes.Add(subNodes);
                }
                foreach (WpfTreeItem category in subNodes.Nodes) // Categories
                {
                    theCheckBox = (WpfCheckBox)category.GetChildren()[1];
                    if (theCheckBox.Checked)
                    {
                        selectedNodes.Add(category);
                    }
                    foreach (WpfTreeItem item in category.Nodes) // Items
                    {
                        string itemName = item.FriendlyName;
                        theCheckBox = (WpfCheckBox)item.GetChildren()[1];
                        if (theCheckBox.Checked)
                        {
                            selectedNodes.Add(item);
                        }
                    }
                }
            }
            int selectedNodeCount = selectedNodes.Count;
            int j = 1;
             
            sourceTree.SearchProperties["Selected"] = "True";
            //foreach (var item in sourceTree.)
            {

            }
             **/
            //sourceTree.
            //foreach (
            // Get a list of all the selected
            //foreach (IExpandCollapseProvider 
            //foreach (
        }

        public bool DoesSourceServerHaveDeployItems(UITestControl theTab)
        {
            int sourceCount = GetSelectedDeployCount(theTab);
            return (sourceCount > 0);
        }

        public void ClickDeploy(UITestControl theTab)
        {
            WpfButton deployButton = GetDeployButton(theTab);
            Mouse.Move(deployButton, new Point(10, 10));
        }

        public void EnterTextInSourceServerFilterBox(UITestControl theTab, string text)
        {
            WpfEdit theBox = GetSourceServerFilterBox(theTab);
            theBox.Text = text;
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
    }
}
