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
                var LocalhostTreeItemChildren = CurrentTreeItem.GetChildren().Where(control => control.ControlType == ControlType.TreeItem);
                CurrentTreeItem = LocalhostTreeItemChildren.FirstOrDefault(LocalhostChild =>
                {
                    var GetNameFromLabel = LocalhostChild.GetChildren();
                    var label = (GetNameFromLabel.FirstOrDefault(control => control.ControlType == ControlType.Text) as WpfText);
                    return label.DisplayText == folder;
                });
            }
            return CurrentTreeItem;
        }

        [When("I press add")]
        public void WhenIPressAdd()
        {
            //TODO: implement act (action) logic

            ScenarioContext.Current.Pending();
        }

        [Then("the result should be (.*) on the screen")]
        public void ThenTheResultShouldBe(int result)
        {
            //TODO: implement assert (verification) logic

            ScenarioContext.Current.Pending();
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
