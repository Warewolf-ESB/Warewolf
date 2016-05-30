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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Warewolf.Studio.UISpecs
{
    [Binding]
    public class Dropdown_List_Item_Action_Steps
    {
        [When("I select \"(.*)\" from the dropdown list")]
        public void WhenISelectFromTheDropdownList(string ListItem)
        {
            var AllItems = Uimap.MainStudioWindow.GetChildren().Where(child => child.ClassName == "Uia.ComboEditorItemControl");
            Mouse.Click(AllItems.FirstOrDefault(listitem => listitem.Name == ListItem), new Point(396, 11));
        }

        [Given("\"(.*)\" exists in the dropdown list")]
        [Then("\"(.*)\" exists in the dropdown list")]
        public void AssertExistsInTheDropdownList(string ListItem)
        {
            var AllItems = Uimap.MainStudioWindow.GetChildren().Where(child => child.ClassName == "Uia.ComboEditorItemControl");
            Assert.IsTrue(AllItems.Any(listitem => listitem.Name == ListItem), ListItem + " does not appear as an option in the drop down list.");
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
