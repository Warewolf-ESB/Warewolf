using Microsoft.VisualStudio.TestTools.UITesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;

namespace Warewolf.Studio.UISpecs
{
    [Binding]
    public sealed class Explorer_Tree_Item_ActionSteps
    {
        [When(@"I click the (.*)th item in the explorer tree")]
        public void WhenIClickTheIndexedItemInTheExplorerTree(int p0)
        {
            p0--;
            var uiTestControlCollection = Uimap.MainStudioWindow.Explorer.ExplorerTree.TreeItems.GetChildren().Where(control=>control.ControlType==ControlType.TreeItem).ToArray();
            var treeItem = uiTestControlCollection[p0];
            Mouse.Click(treeItem, new Point(15, 12));
        }

        [When(@"I double click the (.*)th item in the explorer tree")]
        public void WhenIDoubleClickTheIndexedItemInTheExplorerTree(int p0)
        {
            p0--;
            var uiTestControlCollection = Uimap.MainStudioWindow.Explorer.ExplorerTree.TreeItems.GetChildren().Where(control => control.ControlType == ControlType.TreeItem).ToArray();
            var treeItem = uiTestControlCollection[p0];
            Mouse.DoubleClick(treeItem, new Point(15, 12));
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
