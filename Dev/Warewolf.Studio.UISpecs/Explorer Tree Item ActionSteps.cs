using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using TechTalk.SpecFlow;

namespace Warewolf.Studio.UISpecs
{
    [Binding]
    public sealed class Explorer_Tree_Item_ActionSteps
    {
        [When(@"I click '(.*)' in the explorer tree")]
        public void WhenIClickTheItemInTheExplorerTree(string path)
        {
            UITestControl getTreeItem = GetTreeItemFromPath(path);
            Mouse.Click(getTreeItem, new Point(40, 12));
        }

        [When(@"I right click '(.*)' in the explorer tree")]
        public void WhenIRightClickTheItemInTheExplorerTree(string path)
        {
            UITestControl getTreeItem = GetTreeItemFromPath(path);
            Mouse.Click(getTreeItem, MouseButtons.Right, ModifierKeys.None, new Point(40, 12));
        }

        [When(@"I double click '(.*)' in the explorer tree")]
        public void WhenIDoubleClickTheItemInTheExplorerTree(string path)
        {
            UITestControl getTreeItem = GetTreeItemFromPath(path);
            Mouse.Click(getTreeItem, new Point(40, 12));
        }

        [When(@"I select '(.*)' from the explorer context menu")]
        public void WhenISelectFromTheContextMenu(string MenuOption)
        {
            UITestControlCollection getContextMenuItems = Uimap.MainStudioWindow.ExplorerContextMenu.GetChildren();
            var getContextMenuItem = getContextMenuItems.FirstOrDefault(item => item.Name == MenuOption);
            Mouse.Click(getContextMenuItem, new Point(48, 14));
        }

        [When(@"I expand '(.*)' in the explorer tree")]
        public void WhenIExpandTheItemInTheExplorerTree(string path)
        {
            var getTreeItem = GetTreeItemFromPath(path);
            var getExpander = getTreeItem.GetChildren().FirstOrDefault(control =>
            {
                if (control is WpfCheckBox)
                {
                    return control.FriendlyName == "ExpansionIndicator";
                }
                return false;
            });
            Mouse.Click(getExpander, new Point(18, 3));
        }

        private UITestControl GetTreeItemFromPath(string path)
        {
            var pathAsArray = path.Split('\\');
            UITestControl CurrentTreeItem = Uimap.MainStudioWindow.Explorer.ExplorerTree;
            foreach (var folder in pathAsArray)
            {
                var getNextChildren = CurrentTreeItem.GetChildren();
                var getNextTreeItemChildren = getNextChildren.Where(control => control.ControlType == ControlType.TreeItem);
                CurrentTreeItem = getNextTreeItemChildren.FirstOrDefault(treeitem =>
                {
                    var GetNameFromLabel = treeitem.GetChildren();
                    var label = (GetNameFromLabel.FirstOrDefault(control => control.ControlType == ControlType.Text) as WpfText);
                    return label.DisplayText == folder;
                });
            }
            return CurrentTreeItem;
        }

        #region Properties and Fields

        UIMap Uimap
        {
            get
            {
                if ((_uiMap == null))
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
