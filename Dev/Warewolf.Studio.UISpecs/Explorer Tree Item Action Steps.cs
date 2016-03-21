﻿using Microsoft.VisualStudio.TestTools.UITesting;
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Warewolf.Studio.UISpecs
{
    [Binding]
    public sealed class Explorer_Tree_Item_Action_Steps
    {
        [When(@"I click '(.*)' in the explorer tree")]
        public void WhenIClickTheItemInTheExplorerTree(string path)
        {
            UITestControl getTreeItem = GetTreeItemFromPath(path);
            Mouse.Click(getTreeItem, new Point(40, 12));
        }

        [When(@"I double click '(.*)' in the explorer tree")]
        public void WhenIDoubleClickTheItemInTheExplorerTree(string path)
        {
            UITestControl getTreeItem = GetTreeItemFromPath(path);
            Mouse.DoubleClick(getTreeItem, new Point(40, 12));
        }

        [When(@"I right click '(.*)' in the explorer tree")]
        public void WhenIRightClickTheItemInTheExplorerTree(string path)
        {
            UITestControl getTreeItem = GetTreeItemFromPath(path);
            Mouse.Click(getTreeItem, MouseButtons.Right, ModifierKeys.None, new Point(40, 12));
        }

        [When(@"I expand '(.*)' in the explorer tree")]
        public void WhenIExpandTheItemInTheExplorerTree(string path)
        {
            var getExpander = GetExpansionIndicator(path);
            Mouse.Click(getExpander, new Point(18, 3));
        }

        [When(@"I drag '(.*)' from the explorer tree onto the design surface")]
        public void WhenIDragTheItemFromTheExplorerTreeOntoTheDesignSurface(string path)
        {
            UITestControl getTreeItem = GetTreeItemFromPath(path);
            UITestControl getWorkflowdesigner = Uimap.MainStudioWindow.SplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            Mouse.StartDragging(getTreeItem);
            Mouse.StopDragging(getWorkflowdesigner, new Point(305, 137));
        }

        [When(@"I drag '(.*)' from the explorer tree onto '(.*)' also in the explorer tree")]
        public void WhenIDragTheItemFromTheExplorerTreeOntoTheDesignSurface(string pathFrom, string pathTo)
        {
            UITestControl getFromTreeItem = GetTreeItemFromPath(pathFrom);
            UITestControl getToTreeItem = GetTreeItemFromPath(pathTo);
            Mouse.StartDragging(getFromTreeItem);
            Mouse.StopDragging(getToTreeItem, new Point(140, 3));
        }

        [Given(@"The View permission icon for '(.*)' exists in the explorer tree")]
        [Then(@"The View permission icon for '(.*)' exists in the explorer tree")]
        public void AssertTheViewPermissionIconForTreeItemExistsInTheExplorerTree(string path)
        {
            Assert.IsTrue(GetEditButton(path).Exists, "Edit button does not exist for " + path);
        }

        [Given(@"The Execute permission icon for '(.*)' exists in the explorer tree")]
        [Then(@"The Execute permission icon for '(.*)' exists in the explorer tree")]
        public void AssertTheExecutePermissionIconForTreeItemExistsInTheExplorerTree(string path)
        {
            Assert.IsTrue(GetExecuteButton(path).Exists, "Execute button does not exist for " + path);
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

        private UITestControl GetExpansionIndicator(string treeItemPath)
        {
            var treeItem = GetTreeItemFromPath(treeItemPath);
            return treeItem.GetChildren().FirstOrDefault(control =>
            {
                if (control is WpfCheckBox)
                {
                    return control.FriendlyName == "ExpansionIndicator";
                }
                return false;
            });
        }

        private UITestControl GetEditButton(string treeItemPath)
        {
            var treeItem = GetTreeItemFromPath(treeItemPath);
            return treeItem.GetChildren().FirstOrDefault(control =>
            {
                if (control is WpfButton)
                {
                    return control.FriendlyName == "EditButton";
                }
                return false;
            });
        }

        private UITestControl GetExecuteButton(string treeItemPath)
        {
            var treeItem = GetTreeItemFromPath(treeItemPath);
            return treeItem.GetChildren().FirstOrDefault(control =>
            {
                if (control is WpfButton)
                {
                    return control.FriendlyName == "ExecuteButton";
                }
                return false;
            });
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
